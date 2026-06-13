param(
  [string]$LocalUrl = "http://127.0.0.1:5011",
  [int]$CheckSeconds = 45,
  [string]$Branch = "main",
  [string]$PushUrl = "https://github.com/corporativogna-lrc/Grupo-Nexum-Altivon-Ltda-Me.git"
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$RunDir = Join-Path $RootDir ".nexum-runtime\public-api-guardian"
$LogDir = Join-Path $RootDir "runtime-logs"
$PidPath = Join-Path $RunDir "guardian.pid"
$TunnelPidPath = Join-Path $RunDir "cloudflared.pid"
$TunnelUrlPath = Join-Path $RunDir "api-url.txt"
$LastPublishedUrlPath = Join-Path $RunDir "last-published-url.txt"
$PublisherDir = Join-Path $RunDir "publisher"
$TunnelOutLog = Join-Path $RunDir "cloudflared.out.log"
$TunnelErrLog = Join-Path $RunDir "cloudflared.err.log"
$GuardianLog = Join-Path $LogDir "public-api-guardian.log"
$CloudflaredPath = "C:\Program Files (x86)\cloudflared\cloudflared.exe"

New-Item -ItemType Directory -Force -Path $RunDir, $LogDir | Out-Null

function Write-GuardianLog {
  param([string]$Message)
  Add-Content -Path $GuardianLog -Value "[$(Get-Date -Format s)] $Message"
}

function Set-Utf8NoBomText {
  param([string]$Path, [string]$Value)
  $encoding = New-Object System.Text.UTF8Encoding($false)
  [System.IO.File]::WriteAllText($Path, $Value, $encoding)
}

function Test-HttpHealth {
  param([string]$Url)
  try {
    $response = Invoke-WebRequest -UseBasicParsing -Uri "$Url/health?t=$([DateTimeOffset]::UtcNow.ToUnixTimeSeconds())" -TimeoutSec 12
    return ($response.StatusCode -eq 200)
  } catch {
    return $false
  }
}

function Stop-ManagedTunnel {
  if (-not (Test-Path $TunnelPidPath)) { return }
  $oldPid = Get-Content $TunnelPidPath -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($oldPid) {
    $process = Get-CimInstance Win32_Process -Filter "ProcessId = $oldPid" -ErrorAction SilentlyContinue
    if ($process -and ([string]$process.Name -eq "cloudflared.exe")) {
      Stop-Process -Id ([int]$oldPid) -Force -ErrorAction SilentlyContinue
    }
  }
  Remove-Item $TunnelPidPath -Force -ErrorAction SilentlyContinue
}

function Start-QuickTunnel {
  if (-not (Test-Path $CloudflaredPath)) {
    throw "cloudflared nao encontrado em $CloudflaredPath"
  }

  Stop-ManagedTunnel
  Remove-Item $TunnelOutLog, $TunnelErrLog -Force -ErrorAction SilentlyContinue
  Write-GuardianLog "Abrindo nova ponte publica para $LocalUrl"

  $process = Start-Process `
    -FilePath $CloudflaredPath `
    -ArgumentList @("tunnel", "--protocol", "http2", "--url", $LocalUrl, "--no-autoupdate") `
    -RedirectStandardOutput $TunnelOutLog `
    -RedirectStandardError $TunnelErrLog `
    -WindowStyle Hidden `
    -PassThru

  Set-Content -Path $TunnelPidPath -Value $process.Id
  $deadline = (Get-Date).AddSeconds(60)
  $url = $null
  do {
    Start-Sleep -Seconds 2
    $text = Get-Content $TunnelErrLog -Raw -ErrorAction SilentlyContinue
    if ($text -match "https://[a-z0-9-]+\.trycloudflare\.com") {
      $url = $Matches[0]
    }
  } while (-not $url -and (Get-Date) -lt $deadline -and -not $process.HasExited)

  if (-not $url) {
    throw "Nao foi possivel obter URL publica do Cloudflare Tunnel."
  }

  Set-Content -Path $TunnelUrlPath -Value $url
  Write-GuardianLog "Ponte publica ativa: $url"
  return $url
}

function Initialize-Publisher {
  if (-not (Test-Path (Join-Path $PublisherDir ".git"))) {
    if (Test-Path $PublisherDir) {
      Remove-Item $PublisherDir -Recurse -Force
    }
    git clone --quiet --branch $Branch --single-branch $PushUrl $PublisherDir
    if ($LASTEXITCODE -ne 0) { throw "Falha ao criar clone isolado para publicar a URL." }
  }

  git -C $PublisherDir fetch --quiet origin $Branch
  if ($LASTEXITCODE -ne 0) { throw "Falha ao atualizar clone isolado." }
  git -C $PublisherDir checkout --quiet -B runtime-url-publisher "origin/$Branch"
  if ($LASTEXITCODE -ne 0) { throw "Falha ao preparar branch isolada." }
}

function Publish-RuntimeUrl {
  param([string]$Url)

  $lastPublished = Get-Content $LastPublishedUrlPath -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($lastPublished -eq $Url) { return }

  Initialize-Publisher
  $payload = [ordered]@{
    apiUrl = $Url
    updatedAt = (Get-Date).ToUniversalTime().ToString("o")
    source = "nexum-public-api-guardian"
  } | ConvertTo-Json

  $rootRuntime = Join-Path $PublisherDir "api-runtime.json"
  $publicRuntime = Join-Path $PublisherDir "NexumAltivon_Front-End\public\api-runtime.json"
  New-Item -ItemType Directory -Force -Path (Split-Path -Parent $publicRuntime) | Out-Null
  Set-Utf8NoBomText -Path $rootRuntime -Value $payload
  Set-Utf8NoBomText -Path $publicRuntime -Value $payload

  git -C $PublisherDir add api-runtime.json NexumAltivon_Front-End/public/api-runtime.json
  git -C $PublisherDir diff --cached --quiet
  if ($LASTEXITCODE -eq 0) {
    Set-Content -Path $LastPublishedUrlPath -Value $Url
    return
  }

  git -C $PublisherDir commit --quiet -m "atualiza ponte publica da api"
  if ($LASTEXITCODE -ne 0) { throw "Falha ao criar commit isolado da URL." }

  $env:GCM_INTERACTIVE = "never"
  $env:GIT_TERMINAL_PROMPT = "0"
  git -C $PublisherDir push --quiet origin "HEAD:$Branch"
  if ($LASTEXITCODE -ne 0) { throw "Falha ao publicar api-runtime.json no GitHub." }

  Set-Content -Path $LastPublishedUrlPath -Value $Url
  Write-GuardianLog "api-runtime.json publicado no GitHub: $Url"
}

if (Test-Path $PidPath) {
  $existingPid = Get-Content $PidPath -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($existingPid -and ($existingPid -ne $PID)) {
    $existing = Get-CimInstance Win32_Process -Filter "ProcessId = $existingPid" -ErrorAction SilentlyContinue
    if ($existing -and ([string]$existing.CommandLine).Contains("nexum-public-api-guardian.ps1")) {
      exit 0
    }
  }
}

Set-Content -Path $PidPath -Value $PID
Write-GuardianLog "Guardiao publico iniciado para $LocalUrl."

while ($true) {
  try {
    if (-not (Test-HttpHealth -Url $LocalUrl)) {
      Write-GuardianLog "API local indisponivel em $LocalUrl."
      Start-Sleep -Seconds $CheckSeconds
      continue
    }

    $url = Get-Content $TunnelUrlPath -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $url -or -not (Test-HttpHealth -Url $url)) {
      $url = Start-QuickTunnel
    }

    Publish-RuntimeUrl -Url $url
  } catch {
    Write-GuardianLog "Erro no guardiao publico: $($_.Exception.Message)"
  }

  Start-Sleep -Seconds $CheckSeconds
}
