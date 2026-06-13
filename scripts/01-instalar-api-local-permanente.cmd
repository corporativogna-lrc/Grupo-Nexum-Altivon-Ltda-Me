@echo off
setlocal
cd /d "%~dp0\.."
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%CD%\scripts\01-instalar-api-local-permanente.ps1" -Url http://localhost:5011 -CheckSeconds 20
pause
