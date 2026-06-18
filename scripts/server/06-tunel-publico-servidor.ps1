param(
  [string]$BaseDirectory = "$env:ProgramData\NexumAltivon_API_24H",
  [string]$LocalUrl = "http://127.0.0.1:5010",
  [int]$CheckSeconds = 30
)

$ErrorActionPreference = "Stop"
$Cloudflared = Join-Path $BaseDirectory "cloudflared\cloudflared.exe"
$RuntimeDirectory = Join-Path $BaseDirectory "runtime"
$LogDirectory = Join-Path $BaseDirectory "logs"
$PidPath = Join-Path $RuntimeDirectory "cloudflared.pid"
$UrlPath = Join-Path $RuntimeDirectory "public-api-url.txt"
$OutLog = Join-Path $LogDirectory "cloudflared.out.log"
$ErrorLog = Join-Path $LogDirectory "cloudflared.err.log"
$GuardianLog = Join-Path $LogDirectory "tunnel-guardian.log"

New-Item -ItemType Directory -Force -Path $RuntimeDirectory, $LogDirectory | Out-Null

function Write-TunnelLog([string]$Message) {
  Add-Content -Path $GuardianLog -Value "[$(Get-Date -Format s)] $Message"
}

function Test-Health([string]$BaseUrl) {
  try {
    $response = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/health/db?t=$([DateTimeOffset]::UtcNow.ToUnixTimeSeconds())" -TimeoutSec 12
    return $response.StatusCode -eq 200
  } catch {
    return $false
  }
}

function Stop-Tunnel {
  if (-not (Test-Path $PidPath)) { return }
  $oldPid = Get-Content $PidPath -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($oldPid) {
    Stop-Process -Id $oldPid -Force -ErrorAction SilentlyContinue
  }
  Remove-Item $PidPath -Force -ErrorAction SilentlyContinue
}

function Start-Tunnel {
  if (-not (Test-Path $Cloudflared)) {
    throw "cloudflared nao encontrado em $Cloudflared"
  }
  if (-not (Test-Health $LocalUrl)) {
    throw "API do servidor indisponivel em $LocalUrl"
  }

  Stop-Tunnel
  Remove-Item $OutLog, $ErrorLog -Force -ErrorAction SilentlyContinue
  $process = Start-Process -FilePath $Cloudflared `
    -ArgumentList @("tunnel", "--protocol", "http2", "--url", $LocalUrl, "--no-autoupdate") `
    -WorkingDirectory (Split-Path -Parent $Cloudflared) `
    -WindowStyle Hidden `
    -RedirectStandardOutput $OutLog `
    -RedirectStandardError $ErrorLog `
    -PassThru
  Set-Content -Path $PidPath -Value $process.Id

  $deadline = (Get-Date).AddSeconds(90)
  $url = $null
  do {
    Start-Sleep -Seconds 2
    $text = Get-Content $ErrorLog -Raw -ErrorAction SilentlyContinue
    if ($text -match "https://[a-z0-9-]+\.trycloudflare\.com") {
      $url = $Matches[0]
    }
  } while (-not $url -and (Get-Date) -lt $deadline -and -not $process.HasExited)

  if (-not $url) {
    throw "Nao foi possivel obter a URL do tunel. Consulte $ErrorLog"
  }
  Set-Content -Path $UrlPath -Value $url
  Write-TunnelLog "Tunel publico conectado: $url"
  return $url
}

Write-TunnelLog "Guardiao do tunel iniciado para $LocalUrl"
while ($true) {
  try {
    $url = Get-Content $UrlPath -ErrorAction SilentlyContinue | Select-Object -First 1
    $tunnelPid = Get-Content $PidPath -ErrorAction SilentlyContinue | Select-Object -First 1
    $tunnelProcess = if ($tunnelPid) { Get-Process -Id $tunnelPid -ErrorAction SilentlyContinue } else { $null }
    if (-not (Test-Health $LocalUrl)) {
      Write-TunnelLog "API local indisponivel em $LocalUrl"
    } elseif (-not $url -or -not $tunnelProcess) {
      $url = Start-Tunnel
    }
  } catch {
    Write-TunnelLog "Falha: $($_.Exception.Message)"
  }
  Start-Sleep -Seconds $CheckSeconds
}
