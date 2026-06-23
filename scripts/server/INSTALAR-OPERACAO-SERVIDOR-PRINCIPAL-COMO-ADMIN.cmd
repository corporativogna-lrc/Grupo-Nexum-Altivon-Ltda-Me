@echo off
setlocal
cd /d "%~dp0..\.."

echo Instalando operacao Nexum no SERVIDOR PRINCIPAL...
echo.

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%CD%\scripts\server\01-instalar-api-24h-servidor.ps1" -SourceRoot "%CD%" -BaseDirectory "C:\NexumAltivon_API_24H" -Url http://127.0.0.1:5012 -CheckSeconds 20
if errorlevel 1 exit /b %errorlevel%

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%CD%\scripts\server\06-instalar-ponte-publica-servidor.ps1" -SourceRoot "%CD%" -LocalUrl http://127.0.0.1:5012 -CheckSeconds 45
if errorlevel 1 exit /b %errorlevel%

echo.
echo Operacao instalada no servidor principal.
echo Verifique com: scripts\server\VERIFICAR-OPERACAO-SERVIDOR-PRINCIPAL.cmd
endlocal
