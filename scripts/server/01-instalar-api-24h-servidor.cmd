@echo off
setlocal
cd /d "%~dp0\..\.."
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%CD%\scripts\server\01-instalar-api-24h-servidor.ps1" -SourceRoot "%CD%" -BaseDirectory "%CD%\.nexum-runtime\api-24h" -Url http://127.0.0.1:5012 -CheckSeconds 20
pause
