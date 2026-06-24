@echo off
setlocal
cd /d "%~dp0..\.."
set "PS=%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe"
"%PS%" -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "scripts\server\08-instalar-tunel-fixo-cloudflare.ps1"
pause
endlocal
