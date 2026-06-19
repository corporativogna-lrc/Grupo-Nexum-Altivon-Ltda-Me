@echo off
setlocal
set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%..\..") do set "PACKAGE_ROOT=%%~fI"

net session >nul 2>&1
if not "%errorlevel%"=="0" (
  echo Solicitando permissao de Administrador...
  powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
  exit /b
)

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%03-instalar-api-24h-pacote.ps1" -PackageApiDirectory "%PACKAGE_ROOT%\api" -BaseDirectory "%PACKAGE_ROOT%\.nexum-runtime\api-24h" -Url http://192.168.1.72:5010 -CheckSeconds 20
echo.
echo Testando checkout/API apos atualizacao...
curl.exe -i --max-time 10 http://192.168.1.72:5010/health/db
pause
