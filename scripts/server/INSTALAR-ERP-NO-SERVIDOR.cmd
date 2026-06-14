@echo off
setlocal
net session >nul 2>&1
if not "%errorlevel%"=="0" (
  powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
  exit /b
)
set "BASE=%~dp0"
if "%BASE:~-1%"=="\" set "BASE=%BASE:~0,-1%"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%BASE%\scripts\10-instalar-erp-24h.ps1" -BaseDirectory "%BASE%"
pause
