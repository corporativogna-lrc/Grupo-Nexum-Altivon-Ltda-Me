param([string]$BaseDirectory = "$env:ProgramData\NexumAltivon_API_24H")

$ErrorActionPreference = "Stop"
$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal]::new($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
  throw "Execute como Administrador."
}

$runner = Join-Path $BaseDirectory "start-public-tunnel.ps1"
if (-not (Test-Path $runner)) { throw "Script do tunel nao encontrado: $runner" }
$action = New-ScheduledTaskAction -Execute "$env:WINDIR\System32\WindowsPowerShell\v1.0\powershell.exe" `
  -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$runner`" -BaseDirectory `"$BaseDirectory`"" `
  -WorkingDirectory $BaseDirectory
$trigger = New-ScheduledTaskTrigger -AtStartup
$trigger.Delay = "PT75S"
$taskPrincipal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -RestartCount 999 -RestartInterval (New-TimeSpan -Minutes 1) -ExecutionTimeLimit (New-TimeSpan -Days 3650) -MultipleInstances IgnoreNew
Register-ScheduledTask -TaskName "NexumAltivonTunnel24h" -Action $action -Trigger $trigger -Principal $taskPrincipal -Settings $settings -Force | Out-Null
Stop-ScheduledTask -TaskName "NexumAltivonTunnel24h" -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
Start-ScheduledTask -TaskName "NexumAltivonTunnel24h"
Write-Host "Tunel publico instalado e iniciado."
