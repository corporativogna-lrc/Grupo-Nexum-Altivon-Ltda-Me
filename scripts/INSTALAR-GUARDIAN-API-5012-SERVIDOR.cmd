@echo off
setlocal DisableDelayedExpansion
title Nexum Altivon - Instalar Guardian API 5012 no Servidor

set "PROJECT=Y:\Nexum Altivon\NexumAltivon.com"
set "GUARDIAN=%PROJECT%\.nexum-runtime\start-api-guardian.cmd"
set "TASK=NexumAltivon-API-5012-Guardian"

echo == Nexum Altivon - Instalar Guardian API 5012 no Servidor ==

if not exist "%GUARDIAN%" (
  echo ERRO: guardian nao encontrado em "%GUARDIAN%".
  exit /b 1
)

echo Removendo tarefa antiga, se existir...
schtasks.exe /Delete /TN "%TASK%" /F >nul 2>&1

echo Criando tarefa automatica no boot e no logon...
schtasks.exe /Create /TN "%TASK%" /SC ONSTART /RL HIGHEST /TR "\"%GUARDIAN%\"" /F
if errorlevel 1 exit /b 1

schtasks.exe /Create /TN "%TASK%-Logon" /SC ONLOGON /RL HIGHEST /TR "\"%GUARDIAN%\"" /F
if errorlevel 1 exit /b 1

echo Iniciando guardian agora...
schtasks.exe /Run /TN "%TASK%" >nul 2>&1

echo Aguardando API local...
for /l %%I in (1,1,30) do (
  curl.exe -fsS http://127.0.0.1:5012/health >nul 2>&1 && goto OK_LOCAL
  timeout /t 2 /nobreak >nul
)

echo ERRO: API local nao respondeu em 5012.
exit /b 1

:OK_LOCAL
echo OK: API local ativa.
curl.exe -i http://127.0.0.1:5012/health --max-time 15

echo Validando API publica...
curl.exe -i https://api.nexumaltivon.com.br/health --max-time 25

echo Validando vitrine publica com limite...
curl.exe -i "https://api.nexumaltivon.com.br/api/produtos/destaques?limite=5" --max-time 25

echo == CONCLUIDO ==
pause
