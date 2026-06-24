@echo off
setlocal
cd /d "%~dp0..\.."
set "PS=%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe"

"%PS%" -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "%CD%\scripts\server\07-verificar-operacao-servidor-principal.ps1"

echo.
pause
endlocal
