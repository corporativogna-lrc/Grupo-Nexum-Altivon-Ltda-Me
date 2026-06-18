@echo off
setlocal
cd /d "%~dp0..\.."

net session >nul 2>&1
if not "%errorlevel%"=="0" (
  echo Solicitando permissao de Administrador...
  powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
  exit /b
)

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%CD%\scripts\server\14-instalar-agentes-conexao-servidor.ps1" -SourceRoot "%CD%" -BaseDirectory "%ProgramData%\NexumAltivon_API_24H" -Url http://127.0.0.1:5010 -CheckSeconds 20
pause
