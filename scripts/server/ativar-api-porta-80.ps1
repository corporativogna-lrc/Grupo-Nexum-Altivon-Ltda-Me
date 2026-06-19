$ErrorActionPreference = "Stop"

$projectRoot = "Y:\Nexum Altivon\NexumAltivon.com"
$baseDirectory = Join-Path $projectRoot ".nexum-runtime\api-24h"
$configPath = Join-Path $baseDirectory "config\api.env.ps1"
$logPath = Join-Path $baseDirectory "logs\ativar-api-porta-80.log"
$apiTask = "NexumAltivonApi24h"

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $logPath) | Out-Null

function Write-Step {
  param([string]$Message)
  $line = "[$(Get-Date -Format s)] $Message"
  Write-Host $line
  Add-Content -Path $logPath -Value $line
}

function Test-Http {
  param([string]$Uri)
  try {
    $response = Invoke-WebRequest -UseBasicParsing -Uri $Uri -TimeoutSec 10
    return [pscustomobject]@{ Ok = $response.StatusCode -ge 200 -and $response.StatusCode -lt 300; Content = $response.Content; Error = $null }
  } catch {
    return [pscustomobject]@{ Ok = $false; Content = ""; Error = $_.Exception.Message }
  }
}

if (-not (Test-Path $configPath)) {
  throw "Config privada da API nao encontrada: $configPath"
}

Write-Step "Desativando Apache2.4 para liberar porta 80."
sc.exe stop Apache2.4 | Out-Host
Start-Sleep -Seconds 4
Get-Process -Name httpd -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
sc.exe config Apache2.4 start= demand | Out-Host

Write-Step "Configurando API para escutar em 5010 e 80."
$configText = Get-Content $configPath -Raw
$urls = 'http://0.0.0.0:5010;http://0.0.0.0:80'
if ($configText -match 'ASPNETCORE_URLS') {
  $configText = $configText -replace '\$env:ASPNETCORE_URLS\s*=\s*"[^"]*"', "`$env:ASPNETCORE_URLS = `"$urls`""
} else {
  $configText += "`r`n`$env:ASPNETCORE_URLS = `"$urls`"`r`n"
}
Set-Content -Path $configPath -Value $configText -Encoding UTF8

Write-Step "Liberando firewall TCP 80 e 5010."
New-NetFirewallRule -DisplayName "Nexum Altivon API 80" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 80 -ErrorAction SilentlyContinue | Out-Null
New-NetFirewallRule -DisplayName "Nexum Altivon API 5010" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 5010 -ErrorAction SilentlyContinue | Out-Null

Write-Step "Reiniciando tarefa $apiTask."
Stop-ScheduledTask -TaskName $apiTask -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
$pidPath = Join-Path $baseDirectory "runtime\api.pid"
if (Test-Path $pidPath) {
  $oldPid = Get-Content $pidPath -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($oldPid) { Stop-Process -Id $oldPid -Force -ErrorAction SilentlyContinue }
  Remove-Item $pidPath -Force -ErrorAction SilentlyContinue
}
Get-CimInstance Win32_Process -ErrorAction SilentlyContinue | Where-Object {
  ($_.Name -eq "NexumAltivon.API.exe" -and $_.ExecutablePath -like "$baseDirectory*") -or
  ($_.Name -match "dotnet" -and $_.CommandLine -like "*NexumAltivon.API.dll*")
} | ForEach-Object {
  Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
}

Start-ScheduledTask -TaskName $apiTask

for ($i = 1; $i -le 30; $i++) {
  Start-Sleep -Seconds 2
  $local80 = Test-Http "http://127.0.0.1/health/db"
  if ($local80.Ok) {
    Write-Step "API saudavel na porta 80: $($local80.Content)"
    break
  }
  Write-Step "Aguardando API na porta 80. Tentativa $i/30: $($local80.Error)"
}

$network80 = Test-Http "http://192.168.1.72/health/db"
if (-not $network80.Ok) {
  throw "API nao respondeu na rede pela porta 80: $($network80.Error)"
}
Write-Step "API saudavel em http://192.168.1.72/health/db: $($network80.Content)"

$network5010 = Test-Http "http://192.168.1.72:5010/health/db"
if (-not $network5010.Ok) {
  throw "API nao respondeu na rede pela porta 5010: $($network5010.Error)"
}
Write-Step "API saudavel em http://192.168.1.72:5010/health/db: $($network5010.Content)"

Write-Step "Porta 80 pronta para Cloudflare/IP publico."
