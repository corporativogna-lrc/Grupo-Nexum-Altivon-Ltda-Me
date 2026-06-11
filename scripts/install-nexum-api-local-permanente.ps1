param(
  [string]$Url = "http://localhost:5011",
  [int]$CheckSeconds = 20
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$RunDir = Join-Path $RootDir ".nexum-runtime"
$LogDir = Join-Path $RootDir "runtime-logs"
$PublishedApiDir = Join-Path $RunDir "api-local"
$ProjectPath = Join-Path $RootDir "NexumAltivon_Back-End\NexumAltivon.API.csproj"
$ConfigPath = Join-Path $RunDir "api.local.env.json"
$StartupDir = [Environment]::GetFolderPath("Startup")
$StartupCmd = Join-Path $StartupDir "Nexum Altivon API Guardian.cmd"
$RuntimeCmd = Join-Path $RunDir "start-api-guardian.cmd"
$RunKeyPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
$RunKeyName = "NexumAltivonApiGuardian"
$GuardianScript = Join-Path $ScriptDir "nexum-api-guardian.ps1"
$TempPublishedApiDir = Join-Path $RunDir ("api-local-new-" + [guid]::NewGuid().ToString("N"))
$PidPath = Join-Path $RunDir "api-guardian-api.pid"

if (-not (Test-Path $ConfigPath)) {
  throw "Configuração local não encontrada: $ConfigPath"
}

New-Item -ItemType Directory -Force -Path $RunDir, $LogDir, $PublishedApiDir, $StartupDir, $TempPublishedApiDir | Out-Null

$BuildBase = Join-Path $env:TEMP ("nexum-api-local-" + [guid]::NewGuid().ToString("N"))
try {
  dotnet publish $ProjectPath --configuration Release --output $TempPublishedApiDir -p:UseAppHost=false -p:BaseOutputPath="$BuildBase\bin\" -p:BaseIntermediateOutputPath="$BuildBase\obj\"
  if ($LASTEXITCODE -ne 0) {
    throw "Falha ao publicar API local permanente."
  }
} finally {
  if (Test-Path $BuildBase) {
    Remove-Item $BuildBase -Recurse -Force -ErrorAction SilentlyContinue
  }
}

if (Test-Path $PidPath) {
  $oldPid = Get-Content $PidPath -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($oldPid) {
    Stop-Process -Id $oldPid -Force -ErrorAction SilentlyContinue
  }
}

$portLine = netstat -ano | Select-String -Pattern "127\.0\.0\.1:5011\s+.*LISTENING\s+(\d+)" | Select-Object -First 1
if ($portLine -and $portLine.Matches[0].Groups[1].Value) {
  Stop-Process -Id ([int]$portLine.Matches[0].Groups[1].Value) -Force -ErrorAction SilentlyContinue
}

Start-Sleep -Seconds 2

Remove-Item (Join-Path $PublishedApiDir "*") -Recurse -Force -ErrorAction SilentlyContinue
Copy-Item (Join-Path $TempPublishedApiDir "*") $PublishedApiDir -Recurse -Force
Remove-Item $TempPublishedApiDir -Recurse -Force -ErrorAction SilentlyContinue

$cmdContent = @"
@echo off
cd /d "$RootDir"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$GuardianScript" -Url $Url -CheckSeconds $CheckSeconds
"@

Set-Content -Path $RuntimeCmd -Value $cmdContent -Encoding ASCII
Set-Content -Path $StartupCmd -Value $cmdContent -Encoding ASCII
Set-ItemProperty -Path $RunKeyPath -Name $RunKeyName -Value "`"$RuntimeCmd`""

Start-Process -FilePath $RuntimeCmd -WorkingDirectory $RootDir -WindowStyle Hidden

Write-Host "API local permanente instalada."
Write-Host "Publicação local: $PublishedApiDir"
Write-Host "Inicialização Windows: $StartupCmd"
Write-Host "Registro: $RunKeyName"
