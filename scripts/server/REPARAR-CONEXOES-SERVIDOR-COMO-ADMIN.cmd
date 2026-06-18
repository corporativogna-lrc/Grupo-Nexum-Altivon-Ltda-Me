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

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%15-reparar-conexoes-servidor.ps1" -PackageRoot "%PACKAGE_ROOT%" -BaseDirectory "%PACKAGE_ROOT%\.nexum-runtime\api-24h" -Url http://127.0.0.1:5010 -CheckSeconds 20
pause
