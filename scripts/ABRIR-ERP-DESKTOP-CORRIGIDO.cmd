@echo off
setlocal

set SCRIPT_DIR=%~dp0

powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%erp-desktop-corrigido.ps1" -Url https://admin.nexumaltivon.com/login

if errorlevel 1 (
    echo.
    echo Erro ao executar o ERP Desktop.
    pause
)
