param(
  [string]$ApiDirectory = "Y:\NexumAltivon_API_24H\api",
  [string]$ConfigPath = "Y:\NexumAltivon_API_24H\config\api.env.ps1",
  [string]$BaseDirectory = "Y:\NexumAltivon_API_24H",
  [string]$Url = "http://127.0.0.1:5010",
  [int]$CheckSeconds = 20
)

$ErrorActionPreference = "Stop"

$RuntimeDirectory = Join-Path $BaseDirectory "runtime"
$LogDirectory = Join-Path $BaseDirectory "logs"
$PidPath = Join-Path $RuntimeDirectory "api.pid"
$ApiLog = Join-Path $LogDirectory "api.log"
$ApiErrorLog = Join-Path $LogDirectory "api.err.log"
$GuardianLog = Join-Path $LogDirectory "api-guardian.log"
$ApiDll = Join-Path $ApiDirectory "NexumAltivon.API.dll"
$ApiExe = Join-Path $ApiDirectory "NexumAltivon.API.exe"

New-Item -ItemType Directory -Force -Path $RuntimeDirectory, $LogDirectory | Out-Null

function Write-NexumLog {
  param([string]$Message)
  Add-Content -Path $GuardianLog -Value "[$(Get-Date -Format s)] $Message"
}

function Import-NexumConfig {
  if (-not (Test-Path $ConfigPath)) {
    throw "Configuração da API não encontrada: $ConfigPath"
  }

  . $ConfigPath
}

function Test-NexumApi {
  try {
    $response = Invoke-WebRequest -UseBasicParsing -Uri "$Url/health" -TimeoutSec 8
    return ($response.StatusCode -eq 200)
  } catch {
    return $false
  }
}

function Stop-NexumApi {
  if (-not (Test-Path $PidPath)) {
    return
  }

  $oldPid = Get-Content $PidPath -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($oldPid) {
    $process = Get-Process -Id $oldPid -ErrorAction SilentlyContinue
    if ($process) {
      Stop-Process -Id $oldPid -Force -ErrorAction SilentlyContinue
    }
  }

  Remove-Item $PidPath -Force -ErrorAction SilentlyContinue
}

function Start-NexumApi {
  try {
    Import-NexumConfig

    if (Test-Path $ApiExe) {
      $executable = $ApiExe
      $arguments = @()
    } elseif (Test-Path $ApiDll) {
      $executable = "dotnet"
      $arguments = @($ApiDll)
    } else {
      throw "Publicação da API não encontrada em: $ApiDirectory"
    }

    Write-NexumLog "Iniciando API 24h em $Url"

    $startParameters = @{
      FilePath = $executable
      WorkingDirectory = $ApiDirectory
      WindowStyle = "Hidden"
      RedirectStandardOutput = $ApiLog
      RedirectStandardError = $ApiErrorLog
      PassThru = $true
    }
    if ($arguments.Count -gt 0) {
      $startParameters.ArgumentList = $arguments
    }

    $process = Start-Process @startParameters

    Set-Content -Path $PidPath -Value $process.Id
  } catch {
    Write-NexumLog "Falha ao iniciar API: $($_.Exception.Message)"
    throw
  }
}

Write-NexumLog "Guardião 24h iniciado."

while ($true) {
  if (-not (Test-NexumApi)) {
    Write-NexumLog "API indisponível. Reiniciando."
    Stop-NexumApi
    Start-NexumApi
    Start-Sleep -Seconds 10
  }

  Start-Sleep -Seconds $CheckSeconds
}
