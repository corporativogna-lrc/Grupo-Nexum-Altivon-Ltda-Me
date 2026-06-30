@echo off
setlocal EnableExtensions DisableDelayedExpansion
title Nexum Altivon - Aplicar API 5012 no Servidor

set "BASE=C:\NexumAltivon_API_24H"
set "API_DIR=%BASE%\api"
set "CONFIG=%BASE%\config\api.env.cmd"
set "LOG_DIR=%BASE%\logs"
set "PUBLISH_SOURCE=Y:\Nexum Altivon\_publish_runtime_main\NexumAltivon_Back-End\API"
set "API_EXE=%API_DIR%\NexumAltivon.API.exe"
set "API_DLL=%API_DIR%\NexumAltivon.API.dll"

if not exist "%LOG_DIR%" mkdir "%LOG_DIR%" >nul 2>&1
if not exist "%API_DIR%" mkdir "%API_DIR%" >nul 2>&1

echo == Nexum Altivon - Aplicar API 5012 no Servidor ==
echo Origem: %PUBLISH_SOURCE%
echo Destino: %API_DIR%

if not exist "%PUBLISH_SOURCE%\NexumAltivon.API.dll" (
  echo ERRO: publicacao nova nao encontrada.
  exit /b 1
)

echo Parando processo da API na porta 5012...
for /f "tokens=5" %%P in ('netstat -ano ^| findstr ":5012" ^| findstr "LISTENING"') do (
  taskkill /PID %%P /F >nul 2>&1
)
timeout /t 3 /nobreak >nul

echo Copiando publicacao nova para o runtime oficial...
xcopy "%PUBLISH_SOURCE%\*" "%API_DIR%\" /E /I /Y /R >> "%LOG_DIR%\aplicar-api-5012.log" 2>>&1
if errorlevel 1 (
  echo ERRO: falha ao copiar publicacao nova.
  exit /b 1
)

if exist "%CONFIG%" call "%CONFIG%"
set "ASPNETCORE_ENVIRONMENT=Production"
set "ASPNETCORE_URLS=http://127.0.0.1:5012;http://0.0.0.0:5012"

echo Iniciando API 5012 no servidor...
if exist "%API_EXE%" (
  start "Nexum Altivon API 5012" /MIN /D "%API_DIR%" "%API_EXE%" >> "%LOG_DIR%\api-servico.log" 2>> "%LOG_DIR%\api-servico.err.log"
) else if exist "%API_DLL%" (
  start "Nexum Altivon API 5012" /MIN /D "%API_DIR%" dotnet "%API_DLL%" >> "%LOG_DIR%\api-servico.log" 2>> "%LOG_DIR%\api-servico.err.log"
) else (
  echo ERRO: API nao encontrada no destino.
  exit /b 1
)

echo Aguardando API responder...
for /l %%I in (1,1,30) do (
  curl.exe -fsS http://127.0.0.1:5012/health >nul 2>&1 && goto OK_LOCAL
  timeout /t 2 /nobreak >nul
)

echo ERRO: API nao respondeu em 5012 apos atualizacao.
exit /b 1

:OK_LOCAL
echo OK: API local ativa em 5012.
curl.exe -i http://127.0.0.1:5012/health --max-time 15
curl.exe -i http://127.0.0.1:5012/api/pdv/cockpit --max-time 15
echo == CONCLUIDO ==
exit /b 0
