@echo off
setlocal
cd /d "%~dp0\.."
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%CD%\scripts\install-nexum-api-system-task.ps1" -Url http://localhost:5010 -CheckSeconds 20
pause
