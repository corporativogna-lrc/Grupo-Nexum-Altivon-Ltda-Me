@echo off
cd /d "%~dp0.."
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%CD%\scripts\start-nexum-connectivity.ps1" -LocalUrl http://127.0.0.1:5010 -CheckSeconds 20 -WaitForPublic
