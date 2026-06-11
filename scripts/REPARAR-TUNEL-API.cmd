@echo off
cd /d "%~dp0.."
echo ============================================
echo Nexum Altivon - Reparar Tunel Publico da API
echo ============================================
echo.
echo Verificando guardiao da ponte publica...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%CD%\scripts\nexum-public-api-guardian.ps1" -LocalUrl http://127.0.0.1:5011 -CheckSeconds 45
echo.
echo Pronto. Aguarde alguns segundos e teste:
echo https://api.nexumaltivon.com/health
pause
