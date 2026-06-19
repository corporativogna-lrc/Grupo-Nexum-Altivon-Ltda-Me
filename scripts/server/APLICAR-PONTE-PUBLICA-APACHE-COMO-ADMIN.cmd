@echo off
setlocal

net session >nul 2>&1
if not "%errorlevel%"=="0" (
  echo Solicitando permissao de Administrador...
  powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
  exit /b
)

echo Validando Apache...
Y:\xampp\apache\bin\httpd.exe -t -f "Y:\xampp\apache\conf\httpd.conf"
if not "%errorlevel%"=="0" (
  pause
  exit /b 1
)

echo Reiniciando servico Apache2.4...
sc stop Apache2.4
timeout /t 4 /nobreak >nul
taskkill /IM httpd.exe /F >nul 2>&1
sc start Apache2.4
timeout /t 5 /nobreak >nul

echo Testando API pela porta 80 com host api.nexumaltivon.com...
curl.exe -i --max-time 10 -H "Host: api.nexumaltivon.com" http://127.0.0.1/health/db
pause
