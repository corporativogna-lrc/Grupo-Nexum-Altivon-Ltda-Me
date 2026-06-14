param([string]$BaseDirectory = "Y:\NexumAltivon_Services\ERP")

$ErrorActionPreference = "Stop"
$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal]::new($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
  throw "Execute como Administrador."
}
$runnerSource = Join-Path $BaseDirectory "scripts\09-iniciar-erp-24h.ps1"
$runner = Join-Path $BaseDirectory "start-erp-24h.ps1"
$config = Join-Path $BaseDirectory "config\erp.env.ps1"
if (-not (Test-Path $config)) { throw "Configuracao privada ausente: $config" }
Copy-Item $runnerSource $runner -Force

$action = New-ScheduledTaskAction -Execute "$env:WINDIR\System32\WindowsPowerShell\v1.0\powershell.exe" `
  -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$runner`" -BaseDirectory `"$BaseDirectory`"" `
  -WorkingDirectory $BaseDirectory
$trigger = New-ScheduledTaskTrigger -AtStartup
$trigger.Delay = "PT90S"
$taskPrincipal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -RestartCount 999 `
  -RestartInterval (New-TimeSpan -Minutes 1) -ExecutionTimeLimit (New-TimeSpan -Days 3650) `
  -MultipleInstances IgnoreNew
Register-ScheduledTask -TaskName "NexumAltivonErp24h" -Action $action -Trigger $trigger `
  -Principal $taskPrincipal -Settings $settings -Force | Out-Null
Stop-ScheduledTask -TaskName "NexumAltivonErp24h" -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
Start-ScheduledTask -TaskName "NexumAltivonErp24h"
Write-Host "ERP instalado e iniciado na porta interna 5020."
