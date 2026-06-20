param(
  [string]$SourceRoot = "",
  [string]$BaseDirectory = "Y:\NexumAltivon_API_24H",
  [string]$ApiDirectory = "",
  [string]$ConfigDirectory = "",
  [string]$Url = "http://127.0.0.1:5012",
  [int]$CheckSeconds = 20
)

$ErrorActionPreference = "Stop"

$currentIdentity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($currentIdentity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
  throw "Abra o PowerShell como Administrador e execute novamente."
}

$ScriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $SourceRoot) {
  $SourceRoot = Split-Path -Parent (Split-Path -Parent $ScriptDirectory)
}
$BaseDirectory = [System.IO.Path]::GetFullPath($BaseDirectory.TrimEnd('\', '/'))

$ProjectPath = Join-Path $SourceRoot "NexumAltivon_Back-End\NexumAltivon.API.csproj"
$RunnerSource = Join-Path $ScriptDirectory "04-iniciar-api-24h.ps1"
if (-not $ApiDirectory) {
  $ApiDirectory = Join-Path $BaseDirectory "api"
}
if (-not $ConfigDirectory) {
  $ConfigDirectory = Join-Path $BaseDirectory "config"
}

$RunnerTarget = Join-Path $BaseDirectory "04-iniciar-api-24h.ps1"
$ConfigExampleSource = Join-Path $ScriptDirectory "99-api.env.example.ps1"
$ConfigTarget = Join-Path $ConfigDirectory "api.env.ps1"
$TaskName = "NexumAltivonApi24h"

if (-not (Test-Path $ProjectPath)) {
  throw "Projeto da API não encontrado: $ProjectPath"
}

New-Item -ItemType Directory -Force -Path $ApiDirectory, $ConfigDirectory, (Join-Path $BaseDirectory "logs"), (Join-Path $BaseDirectory "runtime") | Out-Null

$BuildBase = Join-Path $env:TEMP ("nexum-api-publish-" + [guid]::NewGuid().ToString("N"))
try {
  dotnet publish $ProjectPath --configuration Release --output $ApiDirectory -p:UseAppHost=false -p:BaseOutputPath="$BuildBase\bin\" -p:BaseIntermediateOutputPath="$BuildBase\obj\"
  if ($LASTEXITCODE -ne 0) {
    throw "Falha ao publicar a API para: $ApiDirectory"
  }
} finally {
  if (Test-Path $BuildBase) {
    Remove-Item $BuildBase -Recurse -Force -ErrorAction SilentlyContinue
  }
}

Copy-Item $RunnerSource $RunnerTarget -Force

if (-not (Test-Path $ConfigTarget)) {
  Copy-Item $ConfigExampleSource $ConfigTarget
  Write-Host "Configuração criada em: $ConfigTarget"
  Write-Host "Preencha as senhas reais antes de liberar a operação externa."
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

Write-Host "API Nexum Altivon instalada para operar 24h."
Write-Host "Tarefa: $TaskName"
Write-Host "URL local: $Url"
Write-Host "Pasta da API: $ApiDirectory"
Write-Host "Configuração privada: $ConfigTarget"
