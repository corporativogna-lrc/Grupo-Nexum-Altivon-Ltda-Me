param(
  [string]$PackageDirectory = "",
  [string]$BaseDirectory = "Y:\NexumAltivon_API_24H_20260613_V2",
  [string]$TaskName = "NexumAltivonApi24h",
  [string]$HealthUrl = "http://127.0.0.1:5010/health/db"
)

$ErrorActionPreference = "Stop"
$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
  throw "Execute esta atualizacao como Administrador."
}

if (-not $PackageDirectory) {
  $PackageDirectory = Join-Path (Split-Path -Parent $PSScriptRoot) "api"
}

$apiDirectory = Join-Path $BaseDirectory "api"
$runtimeDirectory = Join-Path $BaseDirectory "runtime"
$pidPath = Join-Path $runtimeDirectory "api.pid"
$backupDirectory = Join-Path $BaseDirectory ("backup-api-" + (Get-Date -Format "yyyyMMdd-HHmmss"))

if (-not (Test-Path (Join-Path $PackageDirectory "NexumAltivon.API.exe"))) {
  throw "Pacote da API invalido: $PackageDirectory"
}

Stop-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
if (Test-Path $pidPath) {
  $apiPid = (Get-Content $pidPath -Raw).Trim()
  if ($apiPid -match '^\d+$') {
    Stop-Process -Id ([int]$apiPid) -Force -ErrorAction SilentlyContinue
  }
}
Start-Sleep -Seconds 2

try {
  if (Test-Path $apiDirectory) {
    Move-Item -LiteralPath $apiDirectory -Destination $backupDirectory
  }
  New-Item -ItemType Directory -Force -Path $apiDirectory | Out-Null
  Copy-Item (Join-Path $PackageDirectory "*") $apiDirectory -Recurse -Force
  Start-ScheduledTask -TaskName $TaskName

  $healthy = $false
  for ($attempt = 0; $attempt -lt 40; $attempt++) {
    Start-Sleep -Seconds 1
    try {
      $response = Invoke-WebRequest -Uri $HealthUrl -UseBasicParsing -TimeoutSec 3
      if ($response.StatusCode -eq 200) {
        $healthy = $true
        break
      }
    } catch {}
  }

  if (-not $healthy) {
    throw "A API atualizada nao ficou saudavel."
  }

  Write-Host "Atualizacao aplicada e validada: $HealthUrl"
  Write-Host "Backup anterior: $backupDirectory"
} catch {
  Stop-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
  if (Test-Path $apiDirectory) {
    Remove-Item -LiteralPath $apiDirectory -Recurse -Force
  }
  if (Test-Path $backupDirectory) {
    Move-Item -LiteralPath $backupDirectory -Destination $apiDirectory
  }
  Start-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
  throw
}
