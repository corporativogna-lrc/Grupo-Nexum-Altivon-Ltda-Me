param(
  [string]$Url = "http://localhost:5010",
  [int]$CheckSeconds = 20
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$RunDir = Join-Path $RootDir ".nexum-runtime"
$StartupDir = [Environment]::GetFolderPath("Startup")
$TaskName = "NexumAltivon Connectivity Guardian"
$RunKeyName = "NexumAltivonConnectivityGuardian"
$StarterScript = Join-Path $ScriptDir "start-nexum-connectivity.ps1"
$StartupCmd = Join-Path $StartupDir "Nexum Altivon Connectivity Guardian.cmd"
$LegacyStartupCmd = Join-Path $StartupDir "Nexum Altivon API Guardian.cmd"
$RuntimeCmd = Join-Path $RunDir "start-connectivity-guardian.cmd"

New-Item -ItemType Directory -Force -Path $RunDir, $StartupDir | Out-Null

$cmdContent = @"
@echo off
cd /d "$RootDir"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$StarterScript" -LocalUrl $Url -CheckSeconds $CheckSeconds
"@

Set-Content -Path $RuntimeCmd -Value $cmdContent -Encoding ASCII
Set-Content -Path $StartupCmd -Value $cmdContent -Encoding ASCII
Remove-Item -LiteralPath $LegacyStartupCmd -Force -ErrorAction SilentlyContinue

$taskAction = "`"$RuntimeCmd`""
& schtasks.exe /Create /TN $TaskName /SC ONLOGON /TR $taskAction /F | Out-Null
if ($LASTEXITCODE -ne 0) {
  Write-Host "Tarefa agendada nao criada sem permissao elevada; usando inicializacao do usuario."
}

$runKeyPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
Set-ItemProperty -Path $runKeyPath -Name $RunKeyName -Value $taskAction
Remove-ItemProperty -Path $runKeyPath -Name "NexumAltivonApiGuardian" -ErrorAction SilentlyContinue

Write-Host "Auto-start configurado para API e ponte publica Nexum Altivon."
Write-Host "Atalho: $StartupCmd"
Write-Host "Tarefa: $TaskName"
Write-Host "Registro: $RunKeyName"
