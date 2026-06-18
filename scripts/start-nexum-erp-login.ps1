param(
  [int]$Port = 3002,
  [string]$ApiUrl = "http://localhost:5010"
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$FrontendDir = Join-Path $RootDir "NexumAltivon_Front-End"
$LogDir = Join-Path $RootDir "runtime-logs"
$RunDir = Join-Path $RootDir ".nexum-runtime"
$LogPath = Join-Path $LogDir "erp-login-$Port.log"
$PidPath = Join-Path $RunDir "erp-login-$Port.pid"

New-Item -ItemType Directory -Force -Path $LogDir, $RunDir | Out-Null

if (Test-Path $PidPath) {
  $existingPid = Get-Content $PidPath -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($existingPid -and (Get-Process -Id $existingPid -ErrorAction SilentlyContinue)) {
    Write-Host "ERP/Login frontend ja esta rodando em http://localhost:$Port com PID $existingPid."
    exit 0
  }
}

$command = 'set BROWSER=none&& set PORT=' + $Port + '&& set REACT_APP_BACKEND_URL=' + $ApiUrl + '&& npm start >> "' + $LogPath + '" 2>&1'

$process = Start-Process `
  -FilePath "cmd.exe" `
  -ArgumentList "/d /c $command" `
  -WorkingDirectory $FrontendDir `
  -WindowStyle Hidden `
  -PassThru

Set-Content -Path $PidPath -Value $process.Id

Write-Host "ERP/Login frontend iniciado."
Write-Host "URL: http://localhost:$Port/login"
Write-Host "API: $ApiUrl"
Write-Host "Log: $LogPath"
