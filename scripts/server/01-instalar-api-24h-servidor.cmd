@echo off
setlocal
cd /d "%~dp0\..\.."
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%CD%\scripts\server\01-instalar-api-24h-servidor.ps1" -SourceRoot "%CD%" -BaseDirectory "Y:\NexumAltivon_API_24H" -Url http://127.0.0.1:5010 -CheckSeconds 20
pause
