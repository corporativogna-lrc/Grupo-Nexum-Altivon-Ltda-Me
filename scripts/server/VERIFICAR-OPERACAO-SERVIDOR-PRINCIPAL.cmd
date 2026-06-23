@echo off
setlocal
cd /d "%~dp0..\.."

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%CD%\scripts\server\07-verificar-operacao-servidor-principal.ps1"

echo.
pause
endlocal
