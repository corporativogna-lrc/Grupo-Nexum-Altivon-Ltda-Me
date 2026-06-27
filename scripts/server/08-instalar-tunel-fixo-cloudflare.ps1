param(
  [string]$Token = "",
  [string]$LocalUrl = "http://127.0.0.1:5012",
  [string]$PublicUrl = "https://api.nexumaltivon.com.br"
)

$ErrorActionPreference = "Stop"

$currentIdentity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($currentIdentity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
  throw "Abra como Administrador no servidor principal e execute novamente."
}

function Disable-CloudflareOneWarp {
  $warp = Get-Command "warp-cli.exe" -ErrorAction SilentlyContinue
  if (-not $warp) { return }

  try {
    $status = (& $warp.Source status 2>$null | Out-String)
    if ($status -notmatch "Disconnected") {
      & $warp.Source disconnect 2>$null | Out-Null
      Write-Host "Cloudflare One/WARP desconectado para nao interferir no tunnel."
    }
  } catch {
    Write-Host "Aviso: nao foi possivel validar/desconectar Cloudflare One/WARP: $($_.Exception.Message)"
  }
}

$cloudflaredCandidates = @(
  "C:\Cloudflared\cloudflared.exe",
  "C:\Program Files\cloudflared\cloudflared.exe",
  "C:\Program Files (x86)\cloudflared\cloudflared.exe"
)
$cloudflared = $cloudflaredCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $cloudflared) {
  $cloudflared = "C:\Cloudflared\cloudflared.exe"
  New-Item -ItemType Directory -Force -Path (Split-Path -Parent $cloudflared) | Out-Null
  Invoke-WebRequest -UseBasicParsing -Uri "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe" -OutFile $cloudflared -TimeoutSec 180
}

if (-not $Token) {
  $secure = Read-Host "Cole o token do tunnel fixo Cloudflare" -AsSecureString
  $ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
  try {
    $Token = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
  } finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)
  }
}

if (-not $Token -or $Token.Length -lt 20) {
  throw "Token do tunnel Cloudflare nao informado."
}

Write-Host "Validando API local em $LocalUrl..."
$localHealth = Invoke-WebRequest -UseBasicParsing -Uri "$LocalUrl/health/db" -TimeoutSec 20
if ($localHealth.StatusCode -ne 200) {
  throw "API local nao esta saudavel em $LocalUrl."
}

Disable-CloudflareOneWarp

Write-Host "Instalando servico fixo Cloudflare Tunnel..."
& $cloudflared service uninstall 2>$null
& $cloudflared service install $Token
if ($LASTEXITCODE -ne 0) {
  throw "Falha ao instalar servico fixo Cloudflare Tunnel."
}

Start-Service cloudflared -ErrorAction SilentlyContinue

Write-Host "Aguardando dominio fixo responder..."
$deadline = (Get-Date).AddMinutes(3)
$ok = $false
do {
  Start-Sleep -Seconds 5
  try {
    $publicHealth = Invoke-WebRequest -UseBasicParsing -Uri "$PublicUrl/health/db" -TimeoutSec 20
    if ($publicHealth.StatusCode -eq 200) {
      $ok = $true
      break
    }
  } catch {
    # Aguarda o conector fixo conectar no Cloudflare.
  }
} while ((Get-Date) -lt $deadline)

if (-not $ok) {
  throw "Tunnel fixo instalado, mas $PublicUrl ainda nao respondeu. Confira no Cloudflare se o hostname api.nexumaltivon.com.br aponta para http://127.0.0.1:5012."
}

Write-Host "OK - Tunnel fixo Cloudflare ativo: $PublicUrl"
