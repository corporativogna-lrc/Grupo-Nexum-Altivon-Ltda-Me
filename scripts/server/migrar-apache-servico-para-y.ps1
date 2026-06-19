$ErrorActionPreference = "Stop"

$apacheRoot = "Y:\xampp\apache"
$httpdExe = Join-Path $apacheRoot "bin\httpd.exe"
$httpdConf = Join-Path $apacheRoot "conf\httpd.conf"
$vhostsConf = Join-Path $apacheRoot "conf\extra\httpd-vhosts.conf"
$logPath = "Y:\Nexum Altivon\NexumAltivon.com\.nexum-runtime\api-24h\logs\migrar-apache-y.log"

function Write-Step {
  param([string]$Message)
  $line = "[$(Get-Date -Format s)] $Message"
  Write-Host $line
  Add-Content -Path $logPath -Value $line
}

function Invoke-Sc {
  param([string[]]$Arguments, [switch]$IgnoreFailure)
  Write-Step "sc.exe $($Arguments -join ' ')"
  & sc.exe @Arguments
  $code = $LASTEXITCODE
  if ($code -ne 0 -and -not $IgnoreFailure) {
    throw "sc.exe falhou com codigo $code em: $($Arguments -join ' ')"
  }
}

if (-not (Test-Path $httpdExe)) { throw "Apache de Y: nao encontrado: $httpdExe" }
if (-not (Test-Path $httpdConf)) { throw "httpd.conf de Y: nao encontrado: $httpdConf" }
if (-not (Test-Path $vhostsConf)) { throw "httpd-vhosts.conf de Y: nao encontrado: $vhostsConf" }
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $logPath) | Out-Null
Write-Step "Iniciando migracao do Apache2.4 para Y:\xampp."

$httpdText = Get-Content $httpdConf -Raw
$httpdText = $httpdText -replace '(?m)^#LoadModule proxy_http_module modules/mod_proxy_http\.so$', 'LoadModule proxy_http_module modules/mod_proxy_http.so'
Set-Content -Path $httpdConf -Value $httpdText -Encoding ASCII

$vhostText = Get-Content $vhostsConf -Raw
if ($vhostText -notmatch 'ServerName\s+api\.nexumaltivon\.com') {
  Add-Content -Path $vhostsConf -Encoding ASCII -Value @'

<VirtualHost *:80>
    ServerName api.nexumaltivon.com
    ServerAlias back.nexumaltivon.com

    ProxyPreserveHost On
    ProxyPass / http://127.0.0.1:5010/
    ProxyPassReverse / http://127.0.0.1:5010/

    ErrorLog "logs/nexum-api-error.log"
    CustomLog "logs/nexum-api-access.log" common
</VirtualHost>

<VirtualHost *:80>
    ServerName nexumaltivon.com
    ServerAlias www.nexumaltivon.com
    DocumentRoot "Y:/xampp/htdocs"
</VirtualHost>
'@
}

& $httpdExe -t -f $httpdConf
if ($LASTEXITCODE -ne 0) { throw "Configuracao Apache em Y: invalida." }
Write-Step "Syntax OK em $httpdConf."

Invoke-Sc -Arguments @("stop", "Apache2.4") -IgnoreFailure
Start-Sleep -Seconds 4
$httpdProcesses = Get-Process -Name httpd -ErrorAction SilentlyContinue
if ($httpdProcesses) {
  Write-Step "Encerrando processos httpd existentes."
  $httpdProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
}

$serviceBinPath = '"' + $httpdExe + '" -k runservice'
Invoke-Sc -Arguments @("config", "Apache2.4", "binPath=", $serviceBinPath)
Invoke-Sc -Arguments @("config", "Apache2.4", "start=", "auto")
Invoke-Sc -Arguments @("start", "Apache2.4")
Start-Sleep -Seconds 6

Write-Host "Testando API via Apache Y: na porta 80..."
curl.exe -i --max-time 10 -H "Host: api.nexumaltivon.com" http://127.0.0.1/health/db

Write-Host "Testando API direta na porta 5010..."
curl.exe -i --max-time 10 http://127.0.0.1:5010/health/db
Write-Step "Migracao concluida."
