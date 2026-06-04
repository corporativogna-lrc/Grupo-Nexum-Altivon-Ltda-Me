param(
  [string]$Url = "http://localhost:5010",
  [int]$CheckSeconds = 20
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$RunDir = Join-Path $RootDir ".nexum-runtime"
$LogDir = Join-Path $RootDir "runtime-logs"
$ConfigPath = Join-Path $RunDir "api.local.env.json"
$PidPath = Join-Path $RunDir "api-guardian-api.pid"
$GuardianPidPath = Join-Path $RunDir "api-guardian.pid"
$ApiLog = Join-Path $LogDir "api-guardian-api.log"
$ApiErrLog = Join-Path $LogDir "api-guardian-api.err.log"
$GuardianLog = Join-Path $LogDir "api-guardian.log"
$ProjectPath = Join-Path $RootDir "NexumAltivon_Back-End\NexumAltivon.API.csproj"

New-Item -ItemType Directory -Force -Path $RunDir, $LogDir | Out-Null

if (Test-Path $GuardianPidPath) {
  $existingGuardianPid = Get-Content $GuardianPidPath -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($existingGuardianPid -and ($existingGuardianPid -ne $PID)) {
    $existingGuardian = Get-CimInstance Win32_Process -Filter "ProcessId = $existingGuardianPid" -ErrorAction SilentlyContinue
    $guardianCommand = if ($existingGuardian) { [string]$existingGuardian.CommandLine } else { "" }
    if ($guardianCommand -and $guardianCommand.Contains("nexum-api-guardian.ps1")) {
      exit 0
    }
  }
}

Set-Content -Path $GuardianPidPath -Value $PID

function Write-GuardianLog {
  param([string]$Message)
  Add-Content -Path $GuardianLog -Value "[$(Get-Date -Format s)] $Message"
}

function Get-LocalConfig {
  if (-not (Test-Path $ConfigPath)) {
    throw "Arquivo local de configuracao nao encontrado: $ConfigPath"
  }

  return Get-Content $ConfigPath -Raw | ConvertFrom-Json
}

function Test-ApiHealth {
  try {
    $response = Invoke-WebRequest -UseBasicParsing -Uri "$Url/health" -TimeoutSec 8
    return ($response.StatusCode -eq 200)
  } catch {
    return $false
  }
}

function Stop-ApiProcess {
  if (Test-Path $PidPath) {
    $oldPid = Get-Content $PidPath -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($oldPid) {
      $process = Get-Process -Id $oldPid -ErrorAction SilentlyContinue
      if ($process) {
        Stop-Process -Id $oldPid -Force -ErrorAction SilentlyContinue
      }
    }
    Remove-Item $PidPath -Force -ErrorAction SilentlyContinue
  }
}

function Start-ApiProcess {
  $config = Get-LocalConfig

  $env:ASPNETCORE_ENVIRONMENT = "Production"
  $env:ASPNETCORE_URLS = $Url
  $env:ConnectionStrings__DefaultConnection = $config.ConnectionStrings.DefaultConnection

  if ($config.JwtSettings.SecretKey) {
    $env:JwtSettings__SecretKey = $config.JwtSettings.SecretKey
  }

  if ($config.AdminUser.Password) {
    $env:AdminUser__Password = $config.AdminUser.Password
  }

  Write-GuardianLog "Iniciando API em $Url"
  $dotnetCommand = 'dotnet run --project "' + $ProjectPath + '" --configuration Release --no-build --no-restore'

  $process = Start-Process `
    -FilePath "cmd.exe" `
    -ArgumentList @("/d", "/c", $dotnetCommand) `
    -WorkingDirectory $RootDir `
    -WindowStyle Hidden `
    -RedirectStandardOutput $ApiLog `
    -RedirectStandardError $ApiErrLog `
    -PassThru

  Set-Content -Path $PidPath -Value $process.Id
}

Write-GuardianLog "Guardiao iniciado para $Url"

while ($true) {
  if (-not (Test-ApiHealth)) {
    Write-GuardianLog "API indisponivel. Reiniciando processo local."
    Stop-ApiProcess
    Start-ApiProcess
    Start-Sleep -Seconds 8
  }

  Start-Sleep -Seconds $CheckSeconds
}
