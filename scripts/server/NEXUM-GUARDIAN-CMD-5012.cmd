@echo off
setlocal EnableExtensions

set "BASE=C:\NexumAltivon_API_24H"
set "API_DIR=%BASE%\api"
set "CONFIG=%BASE%\config\api.env.cmd"
set "LOG_DIR=%BASE%\logs"
set "RUNTIME_DIR=%BASE%\runtime"
set "DOTNET=C:\Program Files\dotnet\dotnet.exe"
set "HEALTH=http://127.0.0.1:5012/health"
set "PUBLISH_SOURCE=Y:\Nexum Altivon\_publish_runtime_main\NexumAltivon_Back-End\API"
set "FORCE_RESTART=%RUNTIME_DIR%\force-restart.flag"
set "APPLY_UPDATE=%RUNTIME_DIR%\apply-update.flag"
set "INSTALL_BOOT=%RUNTIME_DIR%\install-boot.flag"

if not exist "%LOG_DIR%" mkdir "%LOG_DIR%" >nul 2>&1
if not exist "%RUNTIME_DIR%" mkdir "%RUNTIME_DIR%" >nul 2>&1
if not exist "%CONFIG%" (
  echo [%date% %time%] ERRO: configuracao privada ausente: "%CONFIG%">> "%LOG_DIR%\guardian-cmd-5012.log"
  exit /b 1
)

call "%CONFIG%"

:LOOP
call :ENSURE_CLOUDFLARED

if exist "%INSTALL_BOOT%" (
  del "%INSTALL_BOOT%" >nul 2>&1
  echo [%date% %time%] Instalacao de boot automatico solicitada.>> "%LOG_DIR%\guardian-cmd-5012.log"
  call "%BASE%\INSTALAR-BOOT-SERVIDOR-NEXUM.cmd" >> "%LOG_DIR%\boot-install.log" 2>>&1
)

if exist "%APPLY_UPDATE%" (
  del "%APPLY_UPDATE%" >nul 2>&1
  echo [%date% %time%] Publicacao solicitada.>> "%LOG_DIR%\guardian-cmd-5012.log"
  call :STOP_API
  call :COPY_PUBLICATION
  call :START_API
  goto WAIT_NEXT
)

if exist "%FORCE_RESTART%" (
  del "%FORCE_RESTART%" >nul 2>&1
  echo [%date% %time%] Reinicio forcado solicitado.>> "%LOG_DIR%\guardian-cmd-5012.log"
  call :STOP_API
  call :START_API
  goto WAIT_NEXT
)

curl.exe -fsS "%HEALTH%" >nul 2>&1
if errorlevel 1 (
  echo [%date% %time%] API 5012 fora. Reiniciando.>> "%LOG_DIR%\guardian-cmd-5012.log"
  call :STOP_API
  call :START_API
)

:WAIT_NEXT
timeout /t 20 /nobreak >nul
goto LOOP

:ENSURE_CLOUDFLARED
sc query Cloudflared | find /I "RUNNING" >nul 2>&1
if errorlevel 1 (
  sc start Cloudflared >> "%LOG_DIR%\cloudflared-service.log" 2>>&1
)
exit /b 0

:STOP_API
for /f "tokens=5" %%P in ('netstat -ano ^| findstr ":5012" ^| findstr "LISTENING"') do taskkill /PID %%P /F >nul 2>&1
timeout /t 2 /nobreak >nul
exit /b 0

:COPY_PUBLICATION
if not exist "%PUBLISH_SOURCE%\NexumAltivon.API.dll" (
  echo [%date% %time%] Publicacao nao encontrada em "%PUBLISH_SOURCE%".>> "%LOG_DIR%\guardian-cmd-5012.log"
  exit /b 1
)
xcopy "%PUBLISH_SOURCE%\*" "%API_DIR%\" /E /I /Y /R >> "%LOG_DIR%\guardian-cmd-5012.log" 2>>&1
exit /b 0

:START_API
if not exist "%API_DIR%\NexumAltivon.API.dll" (
  echo [%date% %time%] ERRO: API DLL nao encontrada em "%API_DIR%".>> "%LOG_DIR%\guardian-cmd-5012.log"
  exit /b 1
)
start "" /B /D "%API_DIR%" "%DOTNET%" "%API_DIR%\NexumAltivon.API.dll" 1>>"%LOG_DIR%\api-servico.log" 2>>"%LOG_DIR%\api-servico.err.log"
timeout /t 12 /nobreak >nul
exit /b 0
