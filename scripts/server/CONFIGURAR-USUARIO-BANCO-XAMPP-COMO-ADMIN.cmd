REM Propriedade intelectual: Luís Rodrigo da Costa
REM Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
REM Sistema de gestão: GenesisGest.Net
REM Ano Início: 04/2024 Publicado e operacional: 05/2026
REM Versão: 1.1.5

@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%configurar-usuario-banco-xampp.ps1" %*
exit /b %ERRORLEVEL%
