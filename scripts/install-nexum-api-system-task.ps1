param(
  [string]$Url = "http://localhost:5010",
  [int]$CheckSeconds = 20,
  [int]$DelaySeconds = 60
)

$ErrorActionPreference = "Stop"

$currentIdentity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($currentIdentity)
$isAdmin = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
  throw "Abra o PowerShell ou Prompt de Comando como Administrador e execute este instalador novamente."
}

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$TaskName = "NexumAltivonApiGuardianSystem"
$GuardianScript = Join-Path $ScriptDir "nexum-api-guardian.ps1"
$PowerShellPath = "$env:WINDIR\System32\WindowsPowerShell\v1.0\powershell.exe"
$Arguments = "-NoProfile -ExecutionPolicy Bypass -File `"$GuardianScript`" -Url $Url -CheckSeconds $CheckSeconds"

$Action = New-ScheduledTaskAction -Execute $PowerShellPath -Argument $Arguments -WorkingDirectory $RootDir
$Trigger = New-ScheduledTaskTrigger -AtStartup
$Trigger.Delay = "PT$($DelaySeconds)S"
$Principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -RunLevel Highest
$Settings = New-ScheduledTaskSettingsSet `
  -AllowStartIfOnBatteries `
  -DontStopIfGoingOnBatteries `
  -ExecutionTimeLimit (New-TimeSpan -Days 3650) `
  -MultipleInstances IgnoreNew `
  -RestartCount 999 `
  -RestartInterval (New-TimeSpan -Minutes 1) `
  -StartWhenAvailable

Register-ScheduledTask `
  -TaskName $TaskName `
  -Action $Action `
  -Trigger $Trigger `
  -Principal $Principal `
  -Settings $Settings `
  -Force | Out-Null

Start-ScheduledTask -TaskName $TaskName

Write-Host "Tarefa definitiva instalada e iniciada: $TaskName"
Write-Host "Ela inicia a API automaticamente com o Windows, sem depender do Codex ou do login."
