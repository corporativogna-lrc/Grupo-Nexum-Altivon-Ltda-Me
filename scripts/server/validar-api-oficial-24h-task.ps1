#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5
#

[CmdletBinding()]
param(
    [string]$TaskName = "NexumAltivonApi24h",
    [string]$ProjectRoot = "D:\Nexum Altivon\NexumAltivon.com",
    [string]$ApiUrl = "http://127.0.0.1:5010",
    [string]$PublicApiUrl = "https://api.nexumaltivon.com.br",
    [string]$DatabaseServiceName = "NexumAltivonMySQL",
    [string]$TunnelServiceName = "Cloudflared"
)

$ErrorActionPreference = "Stop"

$apiUri = [Uri]$ApiUrl
if ($apiUri.AbsoluteUri.TrimEnd('/') -ne "http://127.0.0.1:5010") {
    throw "A validacao oficial aceita somente http://127.0.0.1:5010. Valor recebido: $ApiUrl"
}

$publicUri = [Uri]$PublicApiUrl
if ($publicUri.Scheme -ne "https") {
    throw "A URL publica da API deve usar HTTPS. Valor recebido: $PublicApiUrl"
}

$resolvedProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$expectedStartScript = Join-Path $resolvedProjectRoot "scripts\server\iniciar-api-oficial-24h.ps1"
$runtimeLogDirectory = Join-Path $resolvedProjectRoot "runtime-logs"
New-Item -ItemType Directory -Path $runtimeLogDirectory -Force | Out-Null
$logPath = Join-Path $runtimeLogDirectory "api-24h-task-query.log"
$validationErrors = New-Object System.Collections.Generic.List[string]

function Test-OfficialEndpoint {
    param(
        [Parameter(Mandatory = $true)][string]$Endpoint,
        [int]$Attempts = 5,
        [int]$TimeoutSeconds = 25
    )

    $lastError = $null
    for ($attempt = 1; $attempt -le $Attempts; $attempt++) {
        try {
            $response = Invoke-WebRequest -UseBasicParsing -Uri $Endpoint -TimeoutSec $TimeoutSeconds
            if ($response.StatusCode -ge 200 -and $response.StatusCode -le 299) {
                return [pscustomobject]@{
                    Endpoint = $Endpoint
                    Succeeded = $true
                    StatusCode = $response.StatusCode
                    Attempts = $attempt
                    Error = $null
                }
            }

            $lastError = "HTTP $($response.StatusCode)"
        } catch {
            $lastError = $_.Exception.Message
        }

        if ($attempt -lt $Attempts) {
            Start-Sleep -Seconds 2
        }
    }

    return [pscustomobject]@{
        Endpoint = $Endpoint
        Succeeded = $false
        StatusCode = $null
        Attempts = $Attempts
        Error = $lastError
    }
}

function Get-ServiceEvidence {
    param([Parameter(Mandatory = $true)][string]$Name)

    try {
        $service = Get-Service -Name $Name -ErrorAction Stop
        $cimService = Get-CimInstance Win32_Service -Filter "Name = '$Name'" -ErrorAction Stop
        return [pscustomobject]@{
            Name = $Name
            Exists = $true
            Status = [string]$service.Status
            StartMode = $cimService.StartMode
            StartName = $cimService.StartName
            PathName = $cimService.PathName
        }
    } catch {
        return [pscustomobject]@{
            Name = $Name
            Exists = $false
            Status = $null
            StartMode = $null
            StartName = $null
            PathName = $null
        }
    }
}

$task = $null
$taskInfo = $null
$taskQueryError = $null
try {
    $task = Get-ScheduledTask -TaskName $TaskName -ErrorAction Stop
    $taskInfo = Get-ScheduledTaskInfo -TaskName $TaskName -ErrorAction Stop
} catch {
    $taskQueryError = $_.Exception.Message
    $validationErrors.Add("A tarefa oficial '$TaskName' nao foi encontrada ou nao pode ser lida: $taskQueryError")
}

$driveQualifier = Split-Path -Qualifier $resolvedProjectRoot
$logicalDisk = Get-CimInstance Win32_LogicalDisk | Where-Object { $_.DeviceID -eq $driveQualifier } | Select-Object -First 1
if (-not $logicalDisk -or $logicalDisk.DriveType -ne 3) {
    $validationErrors.Add("O projeto oficial precisa estar em disco local fixo. Caminho=$resolvedProjectRoot DriveType=$($logicalDisk.DriveType).")
}

$scheduleService = Get-ServiceEvidence -Name "Schedule"
$databaseService = Get-ServiceEvidence -Name $DatabaseServiceName
$tunnelService = Get-ServiceEvidence -Name $TunnelServiceName

foreach ($serviceEvidence in @($scheduleService, $databaseService, $tunnelService)) {
    if (-not $serviceEvidence.Exists) {
        $validationErrors.Add("Servico obrigatorio '$($serviceEvidence.Name)' nao esta instalado.")
        continue
    }

    if ($serviceEvidence.Status -ne "Running") {
        $validationErrors.Add("Servico obrigatorio '$($serviceEvidence.Name)' nao esta Running. Estado=$($serviceEvidence.Status).")
    }

    if ($serviceEvidence.StartMode -ne "Auto") {
        $validationErrors.Add("Servico obrigatorio '$($serviceEvidence.Name)' nao esta Automatic. StartMode=$($serviceEvidence.StartMode).")
    }
}

$bootTriggerCount = 0
$logonTriggerCount = 0
$actionExecute = $null
$actionArguments = $null
if ($task) {
    $triggerTypes = @($task.Triggers | ForEach-Object { $_.CimClass.CimClassName })
    $bootTriggerCount = @($triggerTypes | Where-Object { $_ -eq "MSFT_TaskBootTrigger" }).Count
    $logonTriggerCount = @($triggerTypes | Where-Object { $_ -eq "MSFT_TaskLogonTrigger" }).Count
    $action = @($task.Actions) | Select-Object -First 1
    $actionExecute = $action.Execute
    $actionArguments = $action.Arguments

    if ($task.State -ne "Running") {
        $validationErrors.Add("A tarefa '$TaskName' nao esta em execucao. Estado=$($task.State).")
    }

    if ($task.Principal.RunLevel -ne "Highest") {
        $validationErrors.Add("A tarefa '$TaskName' nao usa RunLevel Highest. Valor=$($task.Principal.RunLevel).")
    }

    if ($task.Principal.LogonType -ne "ServiceAccount") {
        $validationErrors.Add("A tarefa '$TaskName' nao usa LogonType ServiceAccount. Valor=$($task.Principal.LogonType).")
    }

    if ($task.Principal.UserId -notin @("SISTEMA", "SYSTEM", "NT AUTHORITY\SYSTEM", "S-1-5-18")) {
        $validationErrors.Add("A tarefa '$TaskName' nao executa como SYSTEM. Usuario=$($task.Principal.UserId).")
    }

    if ($bootTriggerCount -ne 1) {
        $validationErrors.Add("A tarefa '$TaskName' precisa possuir exatamente um gatilho de boot. Quantidade=$bootTriggerCount.")
    }

    if ($logonTriggerCount -ne 0) {
        $validationErrors.Add("A tarefa '$TaskName' nao pode depender de gatilho de logon. Quantidade=$logonTriggerCount.")
    }

    if (-not $task.Settings.StartWhenAvailable) {
        $validationErrors.Add("A tarefa '$TaskName' precisa usar StartWhenAvailable.")
    }

    if ([int]$task.Settings.RestartCount -lt 999) {
        $validationErrors.Add("A tarefa '$TaskName' precisa de RestartCount 999. Valor=$($task.Settings.RestartCount).")
    }

    if ([string]$task.Settings.RestartInterval -ne "PT1M") {
        $validationErrors.Add("A tarefa '$TaskName' precisa de RestartInterval PT1M. Valor=$($task.Settings.RestartInterval).")
    }

    if ([string]$task.Settings.ExecutionTimeLimit -notin @("PT0S", "P0D")) {
        $validationErrors.Add("A tarefa '$TaskName' possui limite finito de execucao. Valor=$($task.Settings.ExecutionTimeLimit).")
    }

    if ([string]$task.Settings.MultipleInstances -ne "IgnoreNew") {
        $validationErrors.Add("A tarefa '$TaskName' precisa bloquear instancias duplicadas. Valor=$($task.Settings.MultipleInstances).")
    }

    if ($actionExecute -notmatch "(?i)powershell(?:\.exe)?$") {
        $validationErrors.Add("A acao da tarefa '$TaskName' nao executa PowerShell. Executavel=$actionExecute.")
    }

    if ($actionArguments -notlike "*$expectedStartScript*") {
        $validationErrors.Add("A tarefa '$TaskName' nao aponta para o supervisor oficial. Argumentos=$actionArguments.")
    }

    if ($actionArguments -notlike "*-WindowStyle Hidden*" -or $actionArguments -notlike "*http://127.0.0.1:5010*") {
        $validationErrors.Add("A tarefa '$TaskName' nao esta invisivel ou nao usa a porta oficial 5010. Argumentos=$actionArguments.")
    }
}

$listener = Get-NetTCPConnection -LocalPort 5010 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
$listenerProcess = if ($listener) { Get-CimInstance Win32_Process -Filter "ProcessId = $($listener.OwningProcess)" } else { $null }
$parentProcess = if ($listenerProcess) { Get-CimInstance Win32_Process -Filter "ProcessId = $($listenerProcess.ParentProcessId)" } else { $null }

if (-not $listener) {
    $validationErrors.Add("Nao existe processo escutando a porta local 5010.")
} elseif (-not $listenerProcess -or $listenerProcess.CommandLine -notmatch "NexumAltivon\.API\.dll") {
    $validationErrors.Add("O processo da porta 5010 nao foi identificado como NexumAltivon.API.dll.")
}

$endpointResults = @(
    Test-OfficialEndpoint -Endpoint "$ApiUrl/health"
    Test-OfficialEndpoint -Endpoint "$ApiUrl/health/db"
    Test-OfficialEndpoint -Endpoint "$ApiUrl/health/db/genesis"
    Test-OfficialEndpoint -Endpoint "$ApiUrl/api/site/configuracoes/publico"
    Test-OfficialEndpoint -Endpoint "$($PublicApiUrl.TrimEnd('/'))/health"
    Test-OfficialEndpoint -Endpoint "$($PublicApiUrl.TrimEnd('/'))/api/site/configuracoes/publico"
)

foreach ($endpointResult in $endpointResults) {
    if (-not $endpointResult.Succeeded) {
        $validationErrors.Add("Endpoint $($endpointResult.Endpoint) falhou apos $($endpointResult.Attempts) tentativa(s): $($endpointResult.Error)")
    }
}

$result = [pscustomobject]@{
    CheckedAt = (Get-Date).ToString("s")
    ValidationSucceeded = ($validationErrors.Count -eq 0)
    ValidationErrors = ($validationErrors -join " | ")
    ProjectRoot = $resolvedProjectRoot
    ProjectDriveType = if ($logicalDisk) { $logicalDisk.DriveType } else { $null }
    TaskName = if ($task) { $task.TaskName } else { $TaskName }
    TaskState = if ($task) { $task.State } else { $null }
    TaskUser = if ($task) { $task.Principal.UserId } else { $null }
    LogonType = if ($task) { $task.Principal.LogonType } else { $null }
    RunLevel = if ($task) { $task.Principal.RunLevel } else { $null }
    BootTriggerCount = $bootTriggerCount
    LogonTriggerCount = $logonTriggerCount
    StartWhenAvailable = if ($task) { $task.Settings.StartWhenAvailable } else { $null }
    RestartCount = if ($task) { $task.Settings.RestartCount } else { $null }
    RestartInterval = if ($task) { [string]$task.Settings.RestartInterval } else { $null }
    ExecutionTimeLimit = if ($task) { [string]$task.Settings.ExecutionTimeLimit } else { $null }
    MultipleInstances = if ($task) { $task.Settings.MultipleInstances } else { $null }
    ActionExecute = $actionExecute
    ActionArguments = $actionArguments
    LastRunTime = if ($taskInfo) { $taskInfo.LastRunTime } else { $null }
    LastTaskResult = if ($taskInfo) { $taskInfo.LastTaskResult } else { $null }
    ScheduleService = "$($scheduleService.Status):$($scheduleService.StartMode):$($scheduleService.StartName)"
    DatabaseService = "$($databaseService.Status):$($databaseService.StartMode):$($databaseService.StartName)"
    TunnelService = "$($tunnelService.Status):$($tunnelService.StartMode):$($tunnelService.StartName)"
    ListenerPid = if ($listener) { $listener.OwningProcess } else { $null }
    ListenerCommand = if ($listenerProcess) { $listenerProcess.CommandLine } else { $null }
    ParentPid = if ($listenerProcess) { $listenerProcess.ParentProcessId } else { $null }
    ParentCommand = if ($parentProcess) { $parentProcess.CommandLine } else { $null }
    EndpointChecks = ($endpointResults | ConvertTo-Json -Compress)
}

$result | Format-List | Out-String | Set-Content -LiteralPath $logPath -Encoding UTF8
$result | Format-List

if ($validationErrors.Count -gt 0) {
    throw "Validacao da API oficial 24h falhou: $($validationErrors -join ' ') Log=$logPath"
}
