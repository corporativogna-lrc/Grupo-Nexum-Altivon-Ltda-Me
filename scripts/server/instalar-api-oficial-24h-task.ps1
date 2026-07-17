#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5
#

[CmdletBinding()]
param(
    [string]$ProjectRoot = "D:\Nexum Altivon\NexumAltivon.com",
    [string]$TaskName = "NexumAltivonApi24h",
    [string]$ApiUrl = "http://127.0.0.1:5010",
    [string]$DatabaseServiceName = "NexumAltivonMySQL",
    [string]$TunnelServiceName = "Cloudflared"
)

$ErrorActionPreference = "Stop"

$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal]::new($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Execute este script em PowerShell elevado. A tarefa 24h precisa ser registrada como SYSTEM."
}

$apiUri = [Uri]$ApiUrl
if ($apiUri.AbsoluteUri.TrimEnd('/') -ne "http://127.0.0.1:5010") {
    throw "A tarefa oficial aceita somente http://127.0.0.1:5010. Valor recebido: $ApiUrl"
}

$resolvedProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$startScript = Join-Path $resolvedProjectRoot "scripts\server\iniciar-api-oficial-24h.ps1"
$privateConfig = Join-Path $resolvedProjectRoot "runtime\api-24h\api.env.ps1"
$apiDll = Join-Path $resolvedProjectRoot "runtime\api-24h\api\NexumAltivon.API.dll"
$logPath = Join-Path $resolvedProjectRoot "runtime-logs\api-24h-task-install.log"

New-Item -ItemType Directory -Path (Split-Path -Parent $logPath) -Force | Out-Null
Set-Content -LiteralPath $logPath -Value "" -Encoding UTF8

function Write-InstallStep {
    param([Parameter(Mandatory = $true)][string]$Message)

    $line = "[$((Get-Date).ToString('s'))] $Message"
    Write-Output $line
    Add-Content -LiteralPath $logPath -Value $line -Encoding UTF8
}

function Ensure-AutomaticService {
    param([Parameter(Mandatory = $true)][string]$Name)

    $service = Get-Service -Name $Name -ErrorAction Stop
    if ($service.StartType -ne [ServiceProcess.ServiceStartMode]::Automatic) {
        Write-InstallStep "Configurando servico $Name para inicializacao automatica."
        Set-Service -Name $Name -StartupType Automatic -ErrorAction Stop
    }

    $service.Refresh()
    if ($service.Status -ne [ServiceProcess.ServiceControllerStatus]::Running) {
        Write-InstallStep "Iniciando servico obrigatorio $Name."
        Start-Service -Name $Name -ErrorAction Stop
        $service.WaitForStatus([ServiceProcess.ServiceControllerStatus]::Running, [TimeSpan]::FromSeconds(60))
    }

    $service.Refresh()
    $cimService = Get-CimInstance Win32_Service -Filter "Name = '$Name'" -ErrorAction Stop
    if ($service.Status -ne [ServiceProcess.ServiceControllerStatus]::Running -or $cimService.StartMode -ne "Auto") {
        throw "O servico obrigatorio $Name nao ficou Running/Automatic. Estado=$($service.Status) StartMode=$($cimService.StartMode)."
    }

    return [pscustomobject]@{
        Name = $Name
        Status = [string]$service.Status
        StartMode = $cimService.StartMode
        StartName = $cimService.StartName
    }
}

function Stop-ExistingOfficialRuntime {
    $existingTask = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
    if ($existingTask -and $existingTask.State -eq "Running") {
        Write-InstallStep "Parando execucao anterior da tarefa $TaskName."
        Stop-ScheduledTask -TaskName $TaskName -ErrorAction Stop
        Start-Sleep -Seconds 2
    }

    $wrappers = Get-CimInstance Win32_Process -Filter "Name = 'powershell.exe'" |
        Where-Object { $_.ProcessId -ne $PID -and $_.CommandLine -like "*iniciar-api-oficial-24h.ps1*" }
    foreach ($process in $wrappers) {
        Write-InstallStep "Encerrando supervisor oficial anterior PID $($process.ProcessId)."
        Stop-Process -Id $process.ProcessId -Force -ErrorAction SilentlyContinue
    }

    $apiProcesses = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
        Where-Object { $_.CommandLine -like "*NexumAltivon.API.dll*" }
    foreach ($process in $apiProcesses) {
        Write-InstallStep "Encerrando API oficial anterior PID $($process.ProcessId)."
        Stop-Process -Id $process.ProcessId -Force -ErrorAction SilentlyContinue
    }

    for ($attempt = 1; $attempt -le 30; $attempt++) {
        $listener = Get-NetTCPConnection -LocalPort 5010 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
        if (-not $listener) {
            return
        }

        Start-Sleep -Seconds 1
    }

    $remaining = Get-NetTCPConnection -LocalPort 5010 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
    $remainingProcess = if ($remaining) { Get-CimInstance Win32_Process -Filter "ProcessId = $($remaining.OwningProcess)" } else { $null }
    throw "A porta 5010 continua ocupada por processo nao encerrado. PID=$($remaining.OwningProcess) Executavel=$($remainingProcess.ExecutablePath)."
}

function Wait-OfficialEndpoint {
    param(
        [Parameter(Mandatory = $true)][string]$Endpoint,
        [int]$TimeoutSeconds = 180
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    $lastError = "Aguardando primeira tentativa."
    while ((Get-Date) -lt $deadline) {
        try {
            $response = Invoke-WebRequest -UseBasicParsing -Uri $Endpoint -TimeoutSec 20
            if ($response.StatusCode -ge 200 -and $response.StatusCode -le 299) {
                Write-InstallStep "Endpoint $Endpoint respondeu HTTP $($response.StatusCode)."
                return
            }

            $lastError = "HTTP $($response.StatusCode)"
        } catch {
            $lastError = $_.Exception.Message
        }

        Start-Sleep -Seconds 2
    }

    throw "O endpoint $Endpoint nao ficou pronto dentro de $TimeoutSeconds segundo(s). Ultimo erro: $lastError"
}

foreach ($requiredPath in @($startScript, $privateConfig, $apiDll)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Arquivo obrigatorio da API oficial nao encontrado em $requiredPath."
    }
}

$scheduleService = Get-Service -Name "Schedule" -ErrorAction Stop
if ($scheduleService.Status -ne [ServiceProcess.ServiceControllerStatus]::Running) {
    throw "O servico Agendador de Tarefas (Schedule) nao esta em execucao."
}

$databaseService = Ensure-AutomaticService -Name $DatabaseServiceName
$tunnelService = Ensure-AutomaticService -Name $TunnelServiceName

Stop-ExistingOfficialRuntime

$argument = "-NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File `"$startScript`" -ProjectRoot `"$resolvedProjectRoot`" -ApiUrl `"http://127.0.0.1:5010`""
$action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument $argument
$startupTrigger = New-ScheduledTaskTrigger -AtStartup
$startupTrigger.Delay = "PT15S"
$settings = New-ScheduledTaskSettingsSet `
    -MultipleInstances IgnoreNew `
    -RestartCount 999 `
    -RestartInterval (New-TimeSpan -Minutes 1) `
    -StartWhenAvailable `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries `
    -DontStopOnIdleEnd `
    -ExecutionTimeLimit ([TimeSpan]::Zero) `
    -Hidden `
    -Priority 4
$taskPrincipal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -RunLevel Highest -LogonType ServiceAccount

Register-ScheduledTask `
    -TaskName $TaskName `
    -Action $action `
    -Trigger $startupTrigger `
    -Settings $settings `
    -Principal $taskPrincipal `
    -Description "GenesisGest.Net API oficial: inicia no boot como SYSTEM, sem depender de login, em http://127.0.0.1:5010." `
    -Force | Out-Null

Write-InstallStep "Tarefa $TaskName registrada como SYSTEM com gatilho exclusivo de boot."
Start-ScheduledTask -TaskName $TaskName
Write-InstallStep "Tarefa $TaskName iniciada."

foreach ($endpoint in @(
    "$ApiUrl/health",
    "$ApiUrl/health/db",
    "$ApiUrl/health/db/genesis",
    "$ApiUrl/api/site/configuracoes/publico"
)) {
    Wait-OfficialEndpoint -Endpoint $endpoint
}

$listener = Get-NetTCPConnection -LocalPort 5010 -State Listen -ErrorAction Stop | Select-Object -First 1
$task = Get-ScheduledTask -TaskName $TaskName -ErrorAction Stop
$taskInfo = Get-ScheduledTaskInfo -TaskName $TaskName -ErrorAction Stop
$listenerProcess = Get-CimInstance Win32_Process -Filter "ProcessId = $($listener.OwningProcess)"
$listenerParent = Get-CimInstance Win32_Process -Filter "ProcessId = $($listenerProcess.ParentProcessId)"
$triggerTypes = @($task.Triggers | ForEach-Object { $_.CimClass.CimClassName })

$result = [pscustomobject]@{
    InstalledAt = (Get-Date).ToString("s")
    TaskName = $TaskName
    TaskUser = $task.Principal.UserId
    LogonType = $task.Principal.LogonType
    RunLevel = $task.Principal.RunLevel
    BootTriggerCount = @($triggerTypes | Where-Object { $_ -eq "MSFT_TaskBootTrigger" }).Count
    LogonTriggerCount = @($triggerTypes | Where-Object { $_ -eq "MSFT_TaskLogonTrigger" }).Count
    ExecutionTimeLimit = [string]$task.Settings.ExecutionTimeLimit
    RestartCount = $task.Settings.RestartCount
    StartScript = $startScript
    PrivateConfig = $privateConfig
    ApiUrl = $ApiUrl
    DatabaseService = "$($databaseService.Name):$($databaseService.Status):$($databaseService.StartMode)"
    TunnelService = "$($tunnelService.Name):$($tunnelService.Status):$($tunnelService.StartMode)"
    ListenerPid = $listener.OwningProcess
    ListenerParentPid = $listenerProcess.ParentProcessId
    ListenerParent = $listenerParent.ExecutablePath
    LastRunTime = $taskInfo.LastRunTime
    LastTaskResult = $taskInfo.LastTaskResult
}

$formattedResult = $result | Format-List | Out-String
Write-Output $formattedResult
Add-Content -LiteralPath $logPath -Value $formattedResult -Encoding UTF8
