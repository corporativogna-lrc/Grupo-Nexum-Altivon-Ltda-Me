param(
  [string]$ApiDirectory = "Y:\NexumAltivon_API_24H\api",
  [string]$ConfigPath = "Y:\NexumAltivon_API_24H\config\api.env.ps1",
  [string]$BaseDirectory = "Y:\NexumAltivon_API_24H",
  [string]$Url = "http://127.0.0.1:5012",
  [int]$CheckSeconds = 20,
  [int]$StartupGraceSeconds = 75
)

$ErrorActionPreference = "Stop"

$RuntimeDirectory = Join-Path $BaseDirectory "runtime"
$LogDirectory = Join-Path $BaseDirectory "logs"
$PidPath = Join-Path $RuntimeDirectory "api.pid"
$ApiLog = Join-Path $LogDirectory "api.log"
$ApiErrorLog = Join-Path $LogDirectory "api.err.log"
$GuardianLog = Join-Path $LogDirectory "api-guardian.log"
$ApiDll = Join-Path $ApiDirectory "NexumAltivon.API.dll"
$ApiExecutable = Join-Path $ApiDirectory "NexumAltivon.API.exe"
$DotnetPathCandidates = @(
  "C:\Program Files\dotnet\dotnet.exe",
  "C:\Program Files (x86)\dotnet\dotnet.exe"
)

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
  Import-NexumConfig

  if (-not ((Test-Path $ApiExecutable) -or (Test-Path $ApiDll))) {
    throw "Publicação da API não encontrada: $ApiDirectory"
  }

  if (Test-Path $ApiExecutable) {
    $processFile = $ApiExecutable
    $processArguments = ""
  } elseif (Test-Path $ApiDll) {
    $dotnetPath = $DotnetPathCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $dotnetPath) {
      $dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
      if ($dotnetCommand) {
        $dotnetPath = $dotnetCommand.Source
      }
    }
    if (-not $dotnetPath) {
      throw "dotnet nao encontrado para iniciar a API."
    }
    $processFile = $dotnetPath
    $processArguments = "`"$ApiDll`""
  }

  Write-NexumLog "Iniciando API 24h em $Url"

  $startParams = @{
    FilePath = $processFile
    WorkingDirectory = $ApiDirectory
    WindowStyle = "Hidden"
    RedirectStandardOutput = $ApiLog
    RedirectStandardError = $ApiErrorLog
    PassThru = $true
  }

  if ($processArguments) {
    $startParams.ArgumentList = $processArguments
  }

  $process = Start-Process @startParams

  Set-Content -Path $PidPath -Value $process.Id
}

Write-NexumLog "Guardião 24h iniciado."

while ($true) {
  try {
    if (-not (Test-NexumApi)) {
      Write-NexumLog "API indisponível. Reiniciando."
      Stop-NexumApi
      Start-NexumApi
      Start-Sleep -Seconds $StartupGraceSeconds
    }
  } catch {
    Write-NexumLog "Erro no guardiao da API: $($_.Exception.Message)"
    Start-Sleep -Seconds 10
  }

  Start-Sleep -Seconds $CheckSeconds
}
