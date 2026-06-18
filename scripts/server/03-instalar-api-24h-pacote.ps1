param(
  [string]$PackageApiDirectory = "",
  [string]$BaseDirectory = "",
  [string]$ApiDirectory = "",
  [string]$ConfigDirectory = "",
  [string]$Url = "http://127.0.0.1:5010",
  [int]$CheckSeconds = 20
)

$ErrorActionPreference = "Stop"

$currentIdentity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($currentIdentity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
  throw "Abra o PowerShell como Administrador e execute novamente."
}

# Servidor operacional: nunca suspender, hibernar ou desligar discos na tomada.
powercfg.exe /change standby-timeout-ac 0 | Out-Null
powercfg.exe /change hibernate-timeout-ac 0 | Out-Null
powercfg.exe /change disk-timeout-ac 0 | Out-Null

$ScriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $PackageApiDirectory) {
  $PackageApiDirectory = Join-Path (Split-Path -Parent $ScriptDirectory) "api"
}
if (-not $BaseDirectory) {
  $BaseDirectory = Join-Path $env:ProgramData "NexumAltivon_API_24H"
}
$BaseDirectory = [System.IO.Path]::GetFullPath($BaseDirectory.TrimEnd('\', '/'))
if (-not $ApiDirectory) {
  $ApiDirectory = Join-Path $BaseDirectory "api"
}
if (-not $ConfigDirectory) {
  $ConfigDirectory = Join-Path $BaseDirectory "config"
}

$RunnerSource = Join-Path $ScriptDirectory "04-iniciar-api-24h.ps1"
$RunnerTarget = Join-Path $BaseDirectory "04-iniciar-api-24h.ps1"
$TunnelRunnerSource = Join-Path $ScriptDirectory "06-tunel-publico-servidor.ps1"
$TunnelRunnerTarget = Join-Path $BaseDirectory "start-public-tunnel.ps1"
$ConfigExampleSource = Join-Path $ScriptDirectory "99-api.env.example.ps1"
$ConfigTarget = Join-Path $ConfigDirectory "api.env.ps1"
$TaskName = "NexumAltivonApi24h"

if (-not (Test-Path (Join-Path $PackageApiDirectory "NexumAltivon.API.dll"))) {
  throw "Pacote publicado da API não encontrado em: $PackageApiDirectory"
}

New-Item -ItemType Directory -Force -Path $ApiDirectory, $ConfigDirectory, (Join-Path $BaseDirectory "logs"), (Join-Path $BaseDirectory "runtime") | Out-Null

$PackageApiDirectory = [System.IO.Path]::GetFullPath($PackageApiDirectory.TrimEnd('\', '/'))
$ApiDirectory = [System.IO.Path]::GetFullPath($ApiDirectory.TrimEnd('\', '/'))
if (-not [string]::Equals($PackageApiDirectory, $ApiDirectory, [StringComparison]::OrdinalIgnoreCase)) {
  Copy-Item (Join-Path $PackageApiDirectory "*") $ApiDirectory -Recurse -Force
}
Copy-Item $RunnerSource $RunnerTarget -Force
if (Test-Path $TunnelRunnerSource) {
  Copy-Item $TunnelRunnerSource $TunnelRunnerTarget -Force
}

if (-not (Test-Path $ConfigTarget)) {
  Copy-Item $ConfigExampleSource $ConfigTarget
  Write-Host "Configuração criada em: $ConfigTarget"
  Write-Host "Preencha as senhas reais antes de liberar a operação externa."
}

$configText = Get-Content $ConfigTarget -Raw
if ($configText -match 'ASPNETCORE_URLS') {
  $configText = $configText -replace '\$env:ASPNETCORE_URLS\s*=\s*"[^"]*"', '$env:ASPNETCORE_URLS = "http://0.0.0.0:5010"'
} else {
  $configText += "`r`n`$env:ASPNETCORE_URLS = `"http://0.0.0.0:5010`"`r`n"
}
Set-Content -Path $ConfigTarget -Value $configText -Encoding UTF8

try {
  New-NetFirewallRule -DisplayName "Nexum Altivon API 5010" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 5010 -ErrorAction SilentlyContinue | Out-Null
} catch {
  Write-Host "Aviso: regra de firewall da porta 5010 nao foi criada automaticamente: $($_.Exception.Message)"
}

$PowerShellPath = "$env:WINDIR\System32\WindowsPowerShell\v1.0\powershell.exe"
$Arguments = "-NoProfile -ExecutionPolicy Bypass -File `"$RunnerTarget`" -ApiDirectory `"$ApiDirectory`" -ConfigPath `"$ConfigTarget`" -BaseDirectory `"$BaseDirectory`" -Url $Url -CheckSeconds $CheckSeconds"

$Action = New-ScheduledTaskAction -Execute $PowerShellPath -Argument $Arguments -WorkingDirectory $BaseDirectory
$TriggerStartup = New-ScheduledTaskTrigger -AtStartup
$TriggerStartup.Delay = "PT60S"
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
  -Trigger $TriggerStartup `
  -Principal $Principal `
  -Settings $Settings `
  -Force | Out-Null

Start-ScheduledTask -TaskName $TaskName

Write-Host "Nexum Altivon API instalada para operar 24h a partir do pacote publicado."
Write-Host "Tarefa: $TaskName"
Write-Host "URL local: $Url"
Write-Host "Pasta da API: $ApiDirectory"
Write-Host "Configuração privada: $ConfigTarget"
