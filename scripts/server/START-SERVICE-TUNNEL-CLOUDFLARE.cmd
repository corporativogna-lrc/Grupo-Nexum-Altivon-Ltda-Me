@echo off
setlocal EnableExtensions
title Nexum Altivon - Cloudflare Tunnel Service

set "LOG_DIR=C:\NexumAltivon_API_24H\logs"
if not exist "%LOG_DIR%" mkdir "%LOG_DIR%" >nul 2>&1

sc query Cloudflared | find /I "RUNNING" >nul 2>&1
if errorlevel 1 (
  sc start Cloudflared >> "%LOG_DIR%\cloudflared-startup.log" 2>>&1
)

sc query Cloudflared >> "%LOG_DIR%\cloudflared-startup.log" 2>>&1
exit /b 0
