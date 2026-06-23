@echo off
setlocal
cd /d "%~dp0..\.."

echo Reparando operacao Nexum no SERVIDOR PRINCIPAL...
echo.

call scripts\server\INSTALAR-OPERACAO-SERVIDOR-PRINCIPAL-COMO-ADMIN.cmd
if errorlevel 1 goto fim

echo.
call scripts\server\VERIFICAR-OPERACAO-SERVIDOR-PRINCIPAL.cmd

:fim
echo.
pause
endlocal
