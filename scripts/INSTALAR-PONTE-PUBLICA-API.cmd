@echo off
cd /d "%~dp0.."
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%CD%\scripts\nexum-public-api-guardian.ps1" -LocalUrl http://127.0.0.1:5011 -CheckSeconds 45
