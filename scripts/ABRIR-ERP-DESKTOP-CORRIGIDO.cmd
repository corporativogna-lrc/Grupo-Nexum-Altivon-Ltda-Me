@echo off
setlocal

set SCRIPT_DIR=%~dp0

powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\erp-desktop.ps1"

if errorlevel 1 (
    echo.
    echo Erro ao executar o ERP Desktop.
    pause
)
