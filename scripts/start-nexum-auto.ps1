param(
  [switch]$Stop
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$LogDir = Join-Path $RootDir "runtime-logs"
$RunDir = Join-Path $RootDir ".nexum-runtime"

New-Item -ItemType Directory -Force -Path $LogDir, $RunDir | Out-Null

$SupervisorPidPath = Join-Path $RunDir "supervisor.pid"
$ApiPidPath = Join-Path $RunDir "api.pid"
$FrontendPidPath = Join-Path $RunDir "frontend.pid"

if ($Stop) {
  foreach ($PidPath in @($ApiPidPath, $FrontendPidPath, $SupervisorPidPath)) {
    if (Test-Path $PidPath) {
      $ProcessId = Get-Content $PidPath -ErrorAction SilentlyContinue | Select-Object -First 1
      if ($ProcessId) {
        taskkill /PID $ProcessId /T /F 2>$null | Out-Null
      }
      Remove-Item $PidPath -Force -ErrorAction SilentlyContinue
    }
  }
  Write-Host "Nexum local services stopped."
  exit 0
}

if (Test-Path $SupervisorPidPath) {
  $ExistingPid = Get-Content $SupervisorPidPath -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($ExistingPid -and (Get-Process -Id $ExistingPid -ErrorAction SilentlyContinue)) {
    Write-Host "Nexum supervisor is already running with PID $ExistingPid."
    exit 0
  }
}

Set-Content -Path $SupervisorPidPath -Value $PID

$Watcher = {
  param(
    [string]$Name,
    [string]$Command,
    [string]$WorkingDirectory,
    [string]$LogPath,
    [string]$PidPath
  )

  while ($true) {
    Add-Content -Path $LogPath -Value ""
    Add-Content -Path $LogPath -Value "[$(Get-Date -Format s)] Starting $Name"

    $Process = Start-Process `
      -FilePath "cmd.exe" `
      -ArgumentList "/d /c $Command" `
      -WorkingDirectory $WorkingDirectory `
      -WindowStyle Hidden `
      -PassThru

    Set-Content -Path $PidPath -Value $Process.Id
    $Process.WaitForExit()

    Add-Content -Path $LogPath -Value "[$(Get-Date -Format s)] $Name exited with code $($Process.ExitCode). Restarting in 5 seconds."
    Start-Sleep -Seconds 5
  }
}

$ApiLog = Join-Path $LogDir "api.log"
$FrontendLog = Join-Path $LogDir "frontend.log"
$ApiProject = Join-Path $RootDir "NexumAltivon_Back-End\NexumAltivon.API.csproj"
$FrontendDir = Join-Path $RootDir "NexumAltivon_Front-End"

$ApiCommand = 'set ASPNETCORE_ENVIRONMENT=Development&& set ASPNETCORE_URLS=http://localhost:5010&& dotnet run --project "' + $ApiProject + '" --no-build --no-restore >> "' + $ApiLog + '" 2>&1'
$FrontendCommand = 'set BROWSER=none&& set PORT=3000&& set REACT_APP_BACKEND_URL=http://localhost:5010&& npm start >> "' + $FrontendLog + '" 2>&1'

$ApiJob = Start-Job -Name "NexumApiWatcher" -ScriptBlock $Watcher -ArgumentList "API", $ApiCommand, $RootDir, $ApiLog, $ApiPidPath
$FrontendJob = Start-Job -Name "NexumFrontendWatcher" -ScriptBlock $Watcher -ArgumentList "Frontend", $FrontendCommand, $FrontendDir, $FrontendLog, $FrontendPidPath

Write-Host "Nexum supervisor started."
Write-Host "API: http://localhost:5010"
Write-Host "Frontend: http://localhost:3000"
Write-Host "Logs: $LogDir"

Wait-Job -Job $ApiJob, $FrontendJob
