@echo off
setlocal

set "ERP_EXE=C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon.Desktop\bin\Release\net8.0-windows\NexumAltivon.Desktop.exe"

if not exist "%ERP_EXE%" (
  echo Nao foi possivel localizar o ERP Desktop em:
  echo %ERP_EXE%
  pause
  exit /b 1
)

start "" "%ERP_EXE%"
