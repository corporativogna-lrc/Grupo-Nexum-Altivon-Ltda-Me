@echo off
setlocal
cd /d "%~dp0..\.."
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%CD%\scripts\server\14-instalar-agentes-conexao-servidor.ps1" -SourceRoot "%CD%" -BaseDirectory "%ProgramData%\NexumAltivon_API_24H" -Url http://127.0.0.1:5010 -CheckSeconds 20
pause
