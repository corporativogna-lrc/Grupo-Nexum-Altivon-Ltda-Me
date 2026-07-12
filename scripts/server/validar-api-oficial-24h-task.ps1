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
    [string]$ApiUrl = "http://127.0.0.1:5010"
)

$ErrorActionPreference = "Stop"

$resolvedProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$runtimeLogDirectory = Join-Path $resolvedProjectRoot "runtime-logs"
New-Item -ItemType Directory -Path $runtimeLogDirectory -Force | Out-Null
$logPath = Join-Path $runtimeLogDirectory "api-24h-task-query.log"

$task = $null
$info = $null
$taskError = $null

try {
    $task = Get-ScheduledTask -TaskName $TaskName -ErrorAction Stop
    $info = Get-ScheduledTaskInfo -TaskName $TaskName -ErrorAction Stop
} catch {
    $taskError = $_.Exception.Message
}

$apiUri = [Uri]$ApiUrl
if ($apiUri.Port -le 0) {
    throw "A URL da API nao informa porta valida: $ApiUrl"
}

$listener = $null
$listenerError = $null
try {
    $listener = Get-NetTCPConnection -LocalPort $apiUri.Port -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
} catch {
    $listenerError = $_.Exception.Message
}

$process = if ($listener) { Get-CimInstance Win32_Process -Filter "ProcessId = $($listener.OwningProcess)" } else { $null }
$parent = if ($process) { Get-CimInstance Win32_Process -Filter "ProcessId = $($process.ParentProcessId)" } else { $null }

$health = $null
$healthError = $null
try {
    $health = Invoke-WebRequest -UseBasicParsing -Uri "$ApiUrl/health" -TimeoutSec 15
} catch {
    $healthError = $_.Exception.Message
}

function Test-OfficialEndpoint {
    param(
        [Parameter(Mandatory = $true)][string]$Endpoint,
        [int]$Attempts = 4,
        [int]$TimeoutSeconds = 20
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

$endpointResults = @(
    Test-OfficialEndpoint -Endpoint "$ApiUrl/health"
    Test-OfficialEndpoint -Endpoint "$ApiUrl/health/db"
    Test-OfficialEndpoint -Endpoint "$ApiUrl/health/db/genesis"
    Test-OfficialEndpoint -Endpoint "$ApiUrl/api/site/configuracoes/publico"
)

$validationErrors = New-Object System.Collections.Generic.List[string]

if (-not $task) {
    $validationErrors.Add("A tarefa '$TaskName' nao foi lida pelo usuario atual. Execute este validador em PowerShell como Administrador para confirmar a tarefa elevada.")
} else {
    if ($task.State -ne "Running") {
        $validationErrors.Add("A tarefa '$TaskName' nao esta em execucao. Estado atual: $($task.State).")
    }

    if ($task.Principal.RunLevel -ne "Highest") {
        $validationErrors.Add("A tarefa '$TaskName' nao esta configurada com RunLevel Highest. Valor atual: $($task.Principal.RunLevel).")
    }

    if ($task.Principal.UserId -notin @("SISTEMA", "SYSTEM", "NT AUTHORITY\SYSTEM")) {
        $validationErrors.Add("A tarefa '$TaskName' nao esta configurada para usuario de sistema. Usuario atual: $($task.Principal.UserId).")
    }
}

if (-not $listener) {
    $validationErrors.Add("Nao existe processo escutando a porta local $($apiUri.Port).")
}

if (-not $process) {
    $validationErrors.Add("Nao foi possivel identificar o processo dono da porta $($apiUri.Port).")
} elseif ([string]::IsNullOrWhiteSpace($process.CommandLine)) {
    $validationErrors.Add("O processo dono da porta $($apiUri.Port) foi identificado, mas a linha de comando nao foi lida pelo usuario atual. Execute em PowerShell como Administrador para confirmar NexumAltivon.API.dll.")
} elseif ($process.CommandLine -notmatch "NexumAltivon\.API\.dll") {
    $validationErrors.Add("O processo da porta $($apiUri.Port) nao aponta para NexumAltivon.API.dll. CommandLine: $($process.CommandLine)")
}

if (-not $health) {
    $validationErrors.Add("O healthcheck local falhou em $ApiUrl/health. Erro: $healthError")
} elseif ($health.StatusCode -ne 200) {
    $validationErrors.Add("O healthcheck local retornou status inesperado: $($health.StatusCode).")
}

foreach ($endpointResult in $endpointResults) {
    if (-not $endpointResult.Succeeded) {
        $validationErrors.Add("Endpoint oficial $($endpointResult.Endpoint) falhou apos $($endpointResult.Attempts) tentativa(s). Erro: $($endpointResult.Error)")
    }
}

$result = [pscustomobject]@{
    CheckedAt = (Get-Date).ToString("s")
    ValidationSucceeded = ($validationErrors.Count -eq 0)
    ValidationErrors = ($validationErrors -join " | ")
    TaskQueryError = $taskError
    ListenerQueryError = $listenerError
    HealthError = $healthError
    TaskName = if ($task) { $task.TaskName } else { $TaskName }
    TaskState = if ($task) { $task.State } else { $null }
    TaskUser = if ($task) { $task.Principal.UserId } else { $null }
    RunLevel = if ($task) { $task.Principal.RunLevel } else { $null }
    LastRunTime = if ($info) { $info.LastRunTime } else { $null }
    LastTaskResult = if ($info) { $info.LastTaskResult } else { $null }
    NextRunTime = if ($info) { $info.NextRunTime } else { $null }
    ListenerPid = if ($listener) { $listener.OwningProcess } else { $null }
    ListenerCommand = if ($process) { $process.CommandLine } else { $null }
    ParentPid = if ($process) { $process.ParentProcessId } else { $null }
    ParentCommand = if ($parent) { $parent.CommandLine } else { $null }
    HealthStatus = if ($health) { $health.StatusCode } else { $null }
    HealthBody = if ($health) { $health.Content } else { $null }
    EndpointChecks = ($endpointResults | ConvertTo-Json -Compress)
}

$result | Format-List | Out-String | Set-Content -LiteralPath $logPath -Encoding UTF8
$result | Format-List

if ($validationErrors.Count -gt 0) {
    throw "Validacao da API oficial 24h falhou: $($validationErrors -join ' ') Log: $logPath"
}
