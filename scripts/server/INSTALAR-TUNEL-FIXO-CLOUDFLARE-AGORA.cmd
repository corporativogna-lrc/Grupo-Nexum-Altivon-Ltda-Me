@echo off
setlocal
cd /d "%~dp0..\.."
powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\server\08-instalar-tunel-fixo-cloudflare.ps1"
pause
endlocal
