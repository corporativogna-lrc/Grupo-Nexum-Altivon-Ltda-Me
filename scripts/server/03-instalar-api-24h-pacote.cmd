@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%\..\..") do set "PACKAGE_ROOT=%%~fI"
set "INSTALLER=%SCRIPT_DIR%03-instalar-api-24h-pacote.ps1"
set "RUNTIME=%PACKAGE_ROOT%\.nexum-runtime\api-24h"

net session >nul 2>&1
if not "%errorlevel%"=="0" (
  powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
  exit /b
)

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%INSTALLER%" -PackageApiDirectory "%PACKAGE_ROOT%\api" -BaseDirectory "%RUNTIME%" -Url http://127.0.0.1:5012 -CheckSeconds 20
pause
