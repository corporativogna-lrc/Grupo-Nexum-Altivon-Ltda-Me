@echo off
cd /d "%~dp0.."
echo ============================================
echo Nexum Altivon - Reparar Tunel Publico da API
echo ============================================
echo.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%CD%\scripts\start-nexum-connectivity.ps1" -LocalUrl http://127.0.0.1:5011 -CheckSeconds 20 -WaitForPublic
pause
