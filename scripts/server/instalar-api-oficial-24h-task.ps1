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
    [string]$ApiUrl = "http://127.0.0.1:5010"
)

$ErrorActionPreference = "Stop"

$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal]::new($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Execute este script em PowerShell elevado. A tarefa 24h precisa ser registrada como SYSTEM."
}

$resolvedProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$startScript = Join-Path $resolvedProjectRoot "scripts\server\iniciar-api-oficial-24h.ps1"
$privateConfig = Join-Path $resolvedProjectRoot "runtime\api-24h\api.env.ps1"
$apiDll = Join-Path $resolvedProjectRoot "runtime\api-24h\api\NexumAltivon.API.dll"
$logPath = Join-Path $resolvedProjectRoot "runtime-logs\api-24h-task-install.log"

New-Item -ItemType Directory -Path (Split-Path -Parent $logPath) -Force | Out-Null

function Write-InstallStep {
    param([Parameter(Mandatory = $true)][string]$Message)

    $line = "[$((Get-Date).ToString('s'))] $Message"
    Write-Output $line
    Add-Content -LiteralPath $logPath -Value $line -Encoding UTF8
}

function Wait-OfficialApiStartup {
    param(
        [Parameter(Mandatory = $true)][int]$Port,
        [Parameter(Mandatory = $true)][string]$HealthUrl,
        [int]$TimeoutSeconds = 180
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    $lastError = "Aguardando primeira tentativa."

    while ((Get-Date) -lt $deadline) {
        $listener = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($listener) {
            try {
                $health = Invoke-WebRequest -UseBasicParsing -Uri $HealthUrl -TimeoutSec 20
                if ($health.StatusCode -ge 200 -and $health.StatusCode -le 299) {
                    Write-InstallStep "API oficial respondeu $HealthUrl HTTP $($health.StatusCode) na porta $Port."
                    return $listener
                }

                $lastError = "Health retornou HTTP $($health.StatusCode)."
            } catch {
                $lastError = $_.Exception.Message
            }
        } else {
            $lastError = "Porta $Port ainda sem listener."
        }

        Write-InstallStep "Aguardando API oficial em $HealthUrl. Ultimo estado: $lastError"
        Start-Sleep -Seconds 2
    }

    throw "A API oficial nao ficou pronta em $HealthUrl dentro de $TimeoutSeconds segundos. Ultimo estado: $lastError"
}

if (-not (Test-Path -LiteralPath $startScript -PathType Leaf)) {
    throw "Script oficial de inicializacao nao encontrado em $startScript."
}

if (-not (Test-Path -LiteralPath $privateConfig -PathType Leaf)) {
    throw "Configuracao privada oficial nao encontrada em $privateConfig."
}

if (-not (Test-Path -LiteralPath $apiDll -PathType Leaf)) {
    throw "Binario publicado da API nao encontrado em $apiDll."
}

$argument = "-NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File `"$startScript`" -ProjectRoot `"$resolvedProjectRoot`" -ApiUrl `"$ApiUrl`""
$action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument $argument
$startupTrigger = New-ScheduledTaskTrigger -AtStartup
$startupTrigger.Delay = "PT30S"
$logonTrigger = New-ScheduledTaskTrigger -AtLogOn
$logonTrigger.Delay = "PT30S"
$settings = New-ScheduledTaskSettingsSet `
    -MultipleInstances IgnoreNew `
    -RestartCount 3 `
    -RestartInterval (New-TimeSpan -Minutes 1) `
    -StartWhenAvailable `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries `
    -ExecutionTimeLimit (New-TimeSpan -Days 30)
$taskPrincipal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -RunLevel Highest -LogonType ServiceAccount

Register-ScheduledTask `
    -TaskName $TaskName `
    -Action $action `
    -Trigger @($startupTrigger, $logonTrigger) `
    -Settings $settings `
    -Principal $taskPrincipal `
    -Description "GenesisGest.Net Nexum Altivon API oficial 24h em http://127.0.0.1:5010." `
    -Force | Out-Null

Write-InstallStep "Tarefa $TaskName registrada como SYSTEM."

$existingApiProcesses = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
    Where-Object { $_.CommandLine -like "*NexumAltivon.API.dll*" }

foreach ($process in $existingApiProcesses) {
    Write-InstallStep "Parando processo API anterior PID $($process.ProcessId)."
    Stop-Process -Id $process.ProcessId -Force -ErrorAction Stop
}

$existingStartWrappers = Get-CimInstance Win32_Process -Filter "Name = 'powershell.exe'" |
    Where-Object { $_.ProcessId -ne $PID -and $_.CommandLine -like "*iniciar-api-oficial-24h.ps1*" }

foreach ($process in $existingStartWrappers) {
    Write-InstallStep "Parando wrapper anterior da API PID $($process.ProcessId)."
    Stop-Process -Id $process.ProcessId -Force -ErrorAction SilentlyContinue
}

for ($i = 0; $i -lt 20; $i++) {
    $busyPort = Get-NetTCPConnection -LocalPort 5010 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($null -eq $busyPort) {
        break
    }

    Start-Sleep -Seconds 1
}

Start-ScheduledTask -TaskName $TaskName
Write-InstallStep "Tarefa $TaskName iniciada. Aguardando porta 5010 e health real."

$listener = Wait-OfficialApiStartup -Port 5010 -HealthUrl "$ApiUrl/health"

$taskInfo = Get-ScheduledTaskInfo -TaskName $TaskName
$listenerProcess = Get-CimInstance Win32_Process -Filter "ProcessId = $($listener.OwningProcess)"
$listenerParent = Get-CimInstance Win32_Process -Filter "ProcessId = $($listenerProcess.ParentProcessId)"

$result = [pscustomobject]@{
    InstalledAt = (Get-Date).ToString("s")
    TaskName = $TaskName
    TaskUser = "SYSTEM"
    StartScript = $startScript
    PrivateConfig = $privateConfig
    ApiUrl = $ApiUrl
    ListenerPid = $listener.OwningProcess
    ListenerParentPid = $listenerProcess.ParentProcessId
    ListenerParent = $listenerParent.ExecutablePath
    LastRunTime = $taskInfo.LastRunTime
    LastTaskResult = $taskInfo.LastTaskResult
}

$result | Format-List | Out-String | Set-Content -LiteralPath $logPath -Encoding UTF8
$result | Format-List
