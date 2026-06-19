@echo off
setlocal

net session >nul 2>&1
if not "%errorlevel%"=="0" (
  echo Solicitando permissao de Administrador...
  powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
  exit /b
)

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0migrar-apache-servico-para-y.ps1"
pause
