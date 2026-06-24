@echo off
setlocal
cd /d "%~dp0..\.."
set "PS=%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe"

echo Instalando operacao Nexum no SERVIDOR PRINCIPAL...
echo.

"%PS%" -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "%CD%\scripts\server\01-instalar-api-24h-servidor.ps1" -SourceRoot "%CD%" -BaseDirectory "C:\NexumAltivon_API_24H" -Url http://127.0.0.1:5012 -CheckSeconds 20 -StartupGraceSeconds 90
if errorlevel 1 exit /b %errorlevel%

"%PS%" -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "%CD%\scripts\server\06-instalar-ponte-publica-servidor.ps1" -SourceRoot "%CD%" -LocalUrl http://127.0.0.1:5012 -CheckSeconds 45
if errorlevel 1 (
  echo.
  echo AVISO: API local instalada e saudavel, mas o Cloudflare nao entregou acesso publico agora.
  echo Para ativar o publico definitivo use: scripts\server\INSTALAR-TUNEL-FIXO-CLOUDFLARE-AGORA.cmd
)

echo.
echo Operacao instalada no servidor principal.
echo Verifique com: scripts\server\VERIFICAR-OPERACAO-SERVIDOR-PRINCIPAL.cmd
endlocal
