@echo off
setlocal

powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\instalar-atalho-erp-desktop.ps1" -PublicDesktop -StartMenu

if errorlevel 1 (
    echo.
    echo Erro ao instalar o atalho do ERP Desktop.
    pause
)
