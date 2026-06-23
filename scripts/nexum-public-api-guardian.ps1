param(
  [string]$LocalUrl = "http://127.0.0.1:5011",
  [int]$CheckSeconds = 45,
  [string]$Branch = "main",
  [string]$PushUrl = "https://corporativogna-lrc@github.com/corporativogna-lrc/Grupo-Nexum-Altivon-Ltda-Me.git"
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$RunDir = Join-Path $RootDir ".nexum-runtime\public-api-guardian"
$LogDir = Join-Path $RootDir "runtime-logs"
$PidPath = Join-Path $RunDir "guardian.pid"
$TunnelPidPath = Join-Path $RunDir "cloudflared.pid"
$TunnelUrlPath = Join-Path $RunDir "api-url.txt"
$TunnelOutLog = Join-Path $RunDir "cloudflared.out.log"
$TunnelErrLog = Join-Path $RunDir "cloudflared.err.log"
$GuardianLog = Join-Path $LogDir "public-api-guardian.log"
$RootRuntimeConfig = Join-Path $RootDir "api-runtime.json"
$PublicRuntimeConfig = Join-Path $RootDir "NexumAltivon_Front-End\public\api-runtime.json"
$CloudflaredPathCandidates = @(
  "C:\Cloudflared\cloudflared.exe",
  "C:\Program Files\cloudflared\cloudflared.exe",
  "C:\Program Files (x86)\cloudflared\cloudflared.exe"
)
$CloudflaredPath = $CloudflaredPathCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
$GitPathCandidates = @(
  "C:\Program Files\Git\cmd\git.exe",
  "C:\Program Files\Git\bin\git.exe",
  "C:\Program Files (x86)\Git\cmd\git.exe",
  "git"
)

New-Item -ItemType Directory -Force -Path $RunDir, $LogDir, (Split-Path -Parent $PublicRuntimeConfig) | Out-Null

function Write-GuardianLog {
  param([string]$Message)
  Add-Content -Path $GuardianLog -Value "[$(Get-Date -Format s)] $Message"
}

function Set-Utf8NoBomText {
  param(
    [string]$Path,
    [string]$Value
  )

  $encoding = New-Object System.Text.UTF8Encoding($false)
  [System.IO.File]::WriteAllText($Path, $Value, $encoding)
}

function Test-HttpHealth {
  param([string]$Url)
  try {
    if (-not $Url -or $Url -eq "https://api.trycloudflare.com") {
      return $false
    }
    $response = Invoke-WebRequest -UseBasicParsing -Uri "$Url/health/db" -TimeoutSec 20
    return ($response.StatusCode -eq 200)
  } catch {
    return $false
  }
}

function Stop-ManagedTunnel {
  if (Test-Path $TunnelPidPath) {
    $oldPid = Get-Content $TunnelPidPath -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($oldPid) {
      Stop-Process -Id ([int]$oldPid) -Force -ErrorAction SilentlyContinue
    }
  }
  Remove-Item $TunnelUrlPath -Force -ErrorAction SilentlyContinue
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

  $deadline = (Get-Date).AddSeconds(55)
  $url = $null
  do {
    Start-Sleep -Seconds 2
    $text = Get-Content $TunnelErrLog -Raw -ErrorAction SilentlyContinue
    if (-not $text) {
      $text = ""
    }
    $matches = [regex]::Matches($text, "https://[a-z0-9-]+\.trycloudflare\.com")
    $validMatch = $matches | Where-Object { $_.Value -ne "https://api.trycloudflare.com" } | Select-Object -Last 1
    if ($validMatch) {
      $url = $validMatch.Value
    }
  } while (-not $url -and (Get-Date) -lt $deadline -and -not $process.HasExited)

  if (-not $url) {
    throw "Nao foi possivel obter URL publica do Cloudflare Tunnel."
  }

  $healthDeadline = (Get-Date).AddSeconds(120)
  while (-not (Test-HttpHealth $url) -and (Get-Date) -lt $healthDeadline -and -not $process.HasExited) {
    Start-Sleep -Seconds 3
  }

  if (-not (Test-HttpHealth $url)) {
    throw "URL publica obtida, mas sem saude confirmada: $url"
  }

  Set-Content -Path $TunnelUrlPath -Value $url
  Write-GuardianLog "Ponte publica ativa: $url"
  return $url
}

function Get-CurrentRuntimeUrl {
  if (-not (Test-Path $PublicRuntimeConfig)) {
    return ""
  }

  try {
    $rawConfig = Get-Content $PublicRuntimeConfig -Raw
    $config = $rawConfig.TrimStart([char]0xFEFF) | ConvertFrom-Json
    return [string]$config.apiUrl
  } catch {
    return ""
  }
}

function Publish-RuntimeUrl {
  param([string]$Url)

  if ($Url -eq "https://api.trycloudflare.com" -or -not (Test-HttpHealth $Url)) {
    Write-GuardianLog "Runtime nao publicado: URL invalida ou sem saude confirmada ($Url)."
    return
  }

  $current = Get-CurrentRuntimeUrl
  if ($current -eq $Url) {
    return
  }

  $payload = [ordered]@{
    apiUrl = $Url
    updatedAt = (Get-Date).ToUniversalTime().ToString("o")
    source = "nexum-public-api-guardian"
  } | ConvertTo-Json

  Set-Utf8NoBomText -Path $RootRuntimeConfig -Value $payload
  Set-Utf8NoBomText -Path $PublicRuntimeConfig -Value $payload

  $gitPath = $GitPathCandidates | Where-Object {
    if ($_ -eq "git") {
      return [bool](Get-Command git -ErrorAction SilentlyContinue)
    }
    Test-Path $_
  } | Select-Object -First 1

  if (-not $gitPath) {
    Write-GuardianLog "Git nao encontrado no servidor. api-runtime.json atualizado localmente, mas nao publicado no GitHub."
    return
  }

  Push-Location $RootDir
  try {
    & $gitPath add api-runtime.json NexumAltivon_Front-End/public/api-runtime.json
    & $gitPath diff --cached --quiet
    if ($LASTEXITCODE -eq 0) {
      return
    }

    & $gitPath commit -m "atualiza ponte publica da api"
    if ($LASTEXITCODE -ne 0) {
      Write-GuardianLog "Falha ao criar commit da ponte publica."
      return
    }

    $env:GCM_INTERACTIVE = "never"
    $env:GIT_TERMINAL_PROMPT = "0"
    & $gitPath push $PushUrl "HEAD:$Branch"
    if ($LASTEXITCODE -eq 0) {
      Write-GuardianLog "api-runtime.json publicado no GitHub."
    } else {
      Write-GuardianLog "Falha ao publicar api-runtime.json no GitHub."
    }
  } finally {
    Pop-Location
  }
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
Write-GuardianLog "Guardiao publico iniciado."

while ($true) {
  try {
    if (-not (Test-HttpHealth -Url $LocalUrl)) {
      Write-GuardianLog "API local indisponivel em $LocalUrl. Aguardando guardiao da API local."
      Start-Sleep -Seconds $CheckSeconds
      continue
    }

    $url = Get-Content $TunnelUrlPath -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($url -eq "https://api.trycloudflare.com") {
      $url = $null
    }
    if (-not $url -or -not (Test-HttpHealth -Url $url)) {
      $url = Start-QuickTunnel
    }

    Publish-RuntimeUrl -Url $url
  } catch {
    Write-GuardianLog "Erro no guardiao publico: $($_.Exception.Message)"
  }

  Start-Sleep -Seconds $CheckSeconds
}
