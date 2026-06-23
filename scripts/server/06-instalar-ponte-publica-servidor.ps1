param(
  [string]$SourceRoot = "",
  [string]$LocalUrl = "http://127.0.0.1:5012",
  [int]$CheckSeconds = 45,
  [string]$Branch = "main",
  [string]$PushUrl = "https://corporativogna-lrc@github.com/corporativogna-lrc/Grupo-Nexum-Altivon-Ltda-Me.git"
)

$ErrorActionPreference = "Stop"

$currentIdentity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($currentIdentity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
  throw "Abra o PowerShell como Administrador no servidor principal e execute novamente."
}

$ScriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $SourceRoot) {
  $SourceRoot = Split-Path -Parent (Split-Path -Parent $ScriptDirectory)
}

$GuardianScript = Join-Path $SourceRoot "scripts\nexum-public-api-guardian.ps1"
if (-not (Test-Path $GuardianScript)) {
  throw "Guardiao publico nao encontrado: $GuardianScript"
}

$CloudflaredPath = "C:\Program Files (x86)\cloudflared\cloudflared.exe"
if (-not (Test-Path $CloudflaredPath)) {
  throw "cloudflared nao encontrado no servidor principal: $CloudflaredPath"
}

$TaskName = "NexumAltivonPontePublica"
$PowerShellPath = "$env:WINDIR\System32\WindowsPowerShell\v1.0\powershell.exe"
$Arguments = "-NoProfile -ExecutionPolicy Bypass -File `"$GuardianScript`" -LocalUrl $LocalUrl -CheckSeconds $CheckSeconds -Branch $Branch -PushUrl `"$PushUrl`""

$Action = New-ScheduledTaskAction -Execute $PowerShellPath -Argument $Arguments -WorkingDirectory $SourceRoot
$TriggerStartup = New-ScheduledTaskTrigger -AtStartup
$TriggerStartup.Delay = "PT90S"
$TriggerLogon = New-ScheduledTaskTrigger -AtLogOn
$Settings = New-ScheduledTaskSettingsSet `
  -AllowStartIfOnBatteries `
  -DontStopIfGoingOnBatteries `
  -ExecutionTimeLimit (New-TimeSpan -Days 3650) `
  -MultipleInstances IgnoreNew `
  -RestartCount 999 `
  -RestartInterval (New-TimeSpan -Minutes 1) `
  -StartWhenAvailable

Stop-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue

Register-ScheduledTask `
  -TaskName $TaskName `
  -Action $Action `
  -Trigger @($TriggerStartup, $TriggerLogon) `
  -RunLevel Highest `
  -Settings $Settings `
  -Force | Out-Null

Start-ScheduledTask -TaskName $TaskName

Write-Host "Ponte publica Nexum instalada no servidor principal."
Write-Host "Tarefa: $TaskName"
Write-Host "Origem local: $LocalUrl"
Write-Host "Script: $GuardianScript"
