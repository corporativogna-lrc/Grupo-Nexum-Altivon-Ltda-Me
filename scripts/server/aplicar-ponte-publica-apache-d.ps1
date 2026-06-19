$ErrorActionPreference = "Stop"

$httpdConf = "D:\xampp\apache\conf\httpd.conf"
$vhostsConf = "D:\xampp\apache\conf\extra\httpd-vhosts.conf"
$httpdExe = "D:\xampp\apache\bin\httpd.exe"

if (-not (Test-Path $httpdConf)) { throw "httpd.conf nao encontrado em $httpdConf" }
if (-not (Test-Path $vhostsConf)) { throw "httpd-vhosts.conf nao encontrado em $vhostsConf" }
if (-not (Test-Path $httpdExe)) { throw "httpd.exe nao encontrado em $httpdExe" }

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
    ProxyPass / http://192.168.1.72:5010/
    ProxyPassReverse / http://192.168.1.72:5010/

    ErrorLog "logs/nexum-api-error.log"
    CustomLog "logs/nexum-api-access.log" common
</VirtualHost>
'@
}

& $httpdExe -t -f $httpdConf
if ($LASTEXITCODE -ne 0) { throw "Configuracao Apache invalida." }

sc.exe stop Apache2.4 | Out-Host
Start-Sleep -Seconds 4
taskkill.exe /IM httpd.exe /F 2>$null | Out-Null
sc.exe start Apache2.4 | Out-Host
Start-Sleep -Seconds 6

curl.exe -i --max-time 10 -H "Host: api.nexumaltivon.com" http://127.0.0.1/health/db
