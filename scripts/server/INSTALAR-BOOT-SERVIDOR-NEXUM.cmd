@echo off
setlocal EnableExtensions
title Nexum Altivon - Boot Automatico Servidor

set "BASE=C:\NexumAltivon_API_24H"
set "GUARDIAN=%BASE%\NEXUM-GUARDIAN-CMD-5012.cmd"
set "CLOUDFLARE_START=%BASE%\START-SERVICE-TUNNEL-CLOUDFLARE.cmd"
set "TASK_API=NexumAltivon-API-Cloudflare-Guardian"
set "TASK_CF=NexumAltivon-Cloudflare-Service"

echo == Nexum Altivon - Boot Automatico Servidor ==

if not exist "%GUARDIAN%" (
  echo ERRO: guardian nao encontrado em "%GUARDIAN%".
  exit /b 1
)

if not exist "%CLOUDFLARE_START%" (
  echo ERRO: inicializador Cloudflare nao encontrado em "%CLOUDFLARE_START%".
  exit /b 1
)

schtasks.exe /Delete /TN "%TASK_API%" /F >nul 2>&1
schtasks.exe /Delete /TN "%TASK_CF%" /F >nul 2>&1

schtasks.exe /Create /TN "%TASK_CF%" /SC ONSTART /RU SYSTEM /RL HIGHEST /TR "\"%CLOUDFLARE_START%\"" /F
if errorlevel 1 exit /b 1

schtasks.exe /Create /TN "%TASK_API%" /SC ONSTART /RU SYSTEM /RL HIGHEST /TR "\"%GUARDIAN%\"" /F
if errorlevel 1 exit /b 1

sc config Cloudflared start= auto >nul 2>&1
sc start Cloudflared >nul 2>&1

schtasks.exe /Run /TN "%TASK_CF%" >nul 2>&1
schtasks.exe /Run /TN "%TASK_API%" >nul 2>&1

echo Boot automatico configurado.
echo API: %TASK_API%
echo Cloudflare: %TASK_CF%
exit /b 0
