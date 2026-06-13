param(
  [string]$LocalUrl = "http://127.0.0.1:5011",
  [int]$CheckSeconds = 20,
  [switch]$WaitForPublic
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$RunDir = Join-Path $RootDir ".nexum-runtime"
$ApiGuardian = Join-Path $ScriptDir "nexum-api-guardian.ps1"
$PublicGuardian = Join-Path $ScriptDir "nexum-public-api-guardian.ps1"
$PublicUrlPath = Join-Path $RunDir "public-api-guardian\api-url.txt"

New-Item -ItemType Directory -Force -Path $RunDir | Out-Null

function Test-Health {
  param([string]$BaseUrl)
  try {
    $response = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/health?t=$([DateTimeOffset]::UtcNow.ToUnixTimeSeconds())" -TimeoutSec 10
    return ($response.StatusCode -eq 200)
  } catch {
    return $false
  }
}

function Test-GuardianProcess {
  param([string]$ScriptName)
  $escapedRoot = [regex]::Escape($RootDir)
  return [bool](Get-CimInstance Win32_Process -Filter "Name = 'powershell.exe'" -ErrorAction SilentlyContinue |
    Where-Object { $_.CommandLine -match [regex]::Escape($ScriptName) -and $_.CommandLine -match $escapedRoot } |
    Select-Object -First 1)
}

if (-not (Test-GuardianProcess -ScriptName "nexum-api-guardian.ps1")) {
  Start-Process powershell.exe `
    -ArgumentList @("-NoProfile", "-ExecutionPolicy", "Bypass", "-File", "`"$ApiGuardian`"", "-Url", $LocalUrl, "-CheckSeconds", $CheckSeconds) `
    -WorkingDirectory $RootDir `
    -WindowStyle Hidden
}

$localDeadline = (Get-Date).AddSeconds(90)
while (-not (Test-Health -BaseUrl $LocalUrl) -and (Get-Date) -lt $localDeadline) {
  Start-Sleep -Seconds 3
}

if (-not (Test-Health -BaseUrl $LocalUrl)) {
  throw "API local nao iniciou em $LocalUrl. Consulte runtime-logs\api-guardian-api.err.log."
}

if (-not (Test-GuardianProcess -ScriptName "nexum-public-api-guardian.ps1")) {
  Start-Process powershell.exe `
    -ArgumentList @("-NoProfile", "-ExecutionPolicy", "Bypass", "-File", "`"$PublicGuardian`"", "-LocalUrl", $LocalUrl, "-CheckSeconds", 30) `
    -WorkingDirectory $RootDir `
    -WindowStyle Hidden
}

Write-Host "API local saudavel: $LocalUrl"

if ($WaitForPublic) {
  $publicDeadline = (Get-Date).AddSeconds(120)
  $publicUrl = ""
  do {
    Start-Sleep -Seconds 3
    $publicUrl = Get-Content $PublicUrlPath -ErrorAction SilentlyContinue | Select-Object -First 1
  } while ((-not $publicUrl -or -not (Test-Health -BaseUrl $publicUrl)) -and (Get-Date) -lt $publicDeadline)

  if (-not $publicUrl -or -not (Test-Health -BaseUrl $publicUrl)) {
    throw "Ponte publica nao ficou saudavel. Consulte runtime-logs\public-api-guardian.log."
  }

  Write-Host "API publica saudavel: $publicUrl"
}
