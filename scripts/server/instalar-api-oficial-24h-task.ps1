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

$existingApiProcesses = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
    Where-Object { $_.CommandLine -like "*NexumAltivon.API.dll*" }

foreach ($process in $existingApiProcesses) {
    Stop-Process -Id $process.ProcessId -Force -ErrorAction Stop
}

$existingStartWrappers = Get-CimInstance Win32_Process -Filter "Name = 'powershell.exe'" |
    Where-Object { $_.ProcessId -ne $PID -and $_.CommandLine -like "*iniciar-api-oficial-24h.ps1*" }

foreach ($process in $existingStartWrappers) {
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
Start-Sleep -Seconds 15

$listener = Get-NetTCPConnection -LocalPort 5010 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
if ($null -eq $listener) {
    $info = Get-ScheduledTaskInfo -TaskName $TaskName
    throw "Tarefa $TaskName registrada, mas a API nao abriu a porta 5010. LastTaskResult=$($info.LastTaskResult)."
}

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
