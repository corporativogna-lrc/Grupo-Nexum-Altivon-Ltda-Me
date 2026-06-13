@echo off
setlocal

set "BASE=%~dp0"
if "%BASE:~-1%"=="\" set "BASE=%BASE:~0,-1%"

if exist "%BASE%\scripts\03-instalar-api-24h-pacote.ps1" (
  set "PACKAGE_ROOT=%BASE%"
  set "INSTALLER=%BASE%\scripts\03-instalar-api-24h-pacote.ps1"
) else (
  for %%I in ("%BASE%\..\..") do set "PACKAGE_ROOT=%%~fI"
  set "INSTALLER=%BASE%\03-instalar-api-24h-pacote.ps1"
)

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%INSTALLER%" -PackageApiDirectory "%PACKAGE_ROOT%\api" -BaseDirectory "%PACKAGE_ROOT%" -Url http://127.0.0.1:5010 -CheckSeconds 20
pause
