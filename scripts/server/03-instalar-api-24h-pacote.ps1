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

$ScriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $PackageApiDirectory) {
  $PackageApiDirectory = Join-Path (Split-Path -Parent $ScriptDirectory) "api"
}
if (-not $BaseDirectory) {
  $BaseDirectory = Split-Path -Parent (Split-Path -Parent $ScriptDirectory)
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
$ConfigExampleSource = Join-Path $ScriptDirectory "99-api.env.example.ps1"
$ConfigTarget = Join-Path $ConfigDirectory "api.env.ps1"
$TaskName = "NexumAltivonApi24h"

if (-not (Test-Path (Join-Path $PackageApiDirectory "NexumAltivon.API.dll"))) {
  throw "Pacote publicado da API não encontrado em: $PackageApiDirectory"
}

New-Item -ItemType Directory -Force -Path $ApiDirectory, $ConfigDirectory, (Join-Path $BaseDirectory "logs"), (Join-Path $BaseDirectory "runtime") | Out-Null

# Encerra guardioes e APIs legadas antes de substituir os binarios em uso.
Get-ScheduledTask -ErrorAction SilentlyContinue |
  Where-Object { $_.TaskName -like "NexumAltivon*Api*" } |
  ForEach-Object { Stop-ScheduledTask -TaskName $_.TaskName -ErrorAction SilentlyContinue }

Start-Sleep -Seconds 2

Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
  Where-Object {
    $_.Name -eq "NexumAltivon.API.exe" -or
    ($_.Name -match "^dotnet" -and $_.CommandLine -like "*NexumAltivon.API.dll*") -or
    ($_.Name -eq "powershell.exe" -and $_.CommandLine -like "*04-iniciar-api-24h.ps1*")
  } |
  ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }

Start-Sleep -Seconds 2

$robocopy = Join-Path $env:WINDIR "System32\robocopy.exe"
& $robocopy $PackageApiDirectory $ApiDirectory /MIR /R:5 /W:2 /NFL /NDL /NP
if ($LASTEXITCODE -gt 7) {
  throw "Falha ao atualizar a API em $ApiDirectory. Codigo Robocopy: $LASTEXITCODE"
}
Copy-Item $RunnerSource $RunnerTarget -Force

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

New-NetFirewallRule -DisplayName "Nexum Altivon API 5010" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 5010 -ErrorAction SilentlyContinue | Out-Null

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

$healthUrl = "$($Url.TrimEnd('/'))/health/db"
$healthy = $false
for ($attempt = 1; $attempt -le 30; $attempt++) {
  Start-Sleep -Seconds 2
  try {
    $response = Invoke-WebRequest -UseBasicParsing -Uri $healthUrl -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
      $healthy = $true
      break
    }
  } catch {
    Write-Host "Aguardando API: tentativa $attempt/30"
  }
}

if (-not $healthy) {
  throw "A API foi instalada, mas nao ficou saudavel em $healthUrl. Consulte $(Join-Path $BaseDirectory 'logs')."
}

Write-Host "API Nexum Altivon instalada para operar 24h a partir do pacote publicado."
Write-Host "Tarefa: $TaskName"
Write-Host "URL local: $Url"
Write-Host "Pasta da API: $ApiDirectory"
Write-Host "Configuração privada: $ConfigTarget"
Write-Host "Saude do banco: OK"
