@echo off
setlocal EnableExtensions
chcp 65001 >nul

set "DEST=C:\Users\Rodrigo Costa\Desktop\Fluxo\Resultado"
set "SRC=%~dp0"

if not exist "%DEST%" mkdir "%DEST%"
if errorlevel 1 (
  echo ERRO: nao foi possivel criar "%DEST%".
  exit /b 1
)

copy /Y "%SRC%FLUXO-MESTRE-CLINICO-GENESISGEST-2026-07-19.md" "%DEST%\FLUXO-MESTRE-CLINICO-GENESISGEST-2026-07-19.md" >nul
if errorlevel 1 (
  echo ERRO: falha ao copiar o arquivo principal.
  exit /b 2
)

copy /Y "%SRC%MANIFESTO-FLUXO-MESTRE-GENESISGEST.txt" "%DEST%\MANIFESTO-FLUXO-MESTRE-GENESISGEST.txt" >nul
copy /Y "%SRC%SHA256SUMS-FLUXO-MESTRE.txt" "%DEST%\SHA256SUMS-FLUXO-MESTRE.txt" >nul

if not exist "%DEST%\Fontes" mkdir "%DEST%\Fontes"
copy /Y "%SRC%Fontes\CHECKLIST-CLINICO-GENESISGEST-2026-07-10.md" "%DEST%\Fontes\CHECKLIST-CLINICO-GENESISGEST-2026-07-10.md" >nul
copy /Y "%SRC%Fontes\ARVORE-REAL-E-META-GENESISGEST-2026-07-18.md" "%DEST%\Fontes\ARVORE-REAL-E-META-GENESISGEST-2026-07-18.md" >nul
copy /Y "%SRC%Fontes\MATRIZ-PARIDADE-BANCO-WPF-2026-07-18.md" "%DEST%\Fontes\MATRIZ-PARIDADE-BANCO-WPF-2026-07-18.md" >nul

set "ACTUAL="
for /f "tokens=* delims=" %%H in ('certutil -hashfile "%DEST%\FLUXO-MESTRE-CLINICO-GENESISGEST-2026-07-19.md" SHA256 ^| findstr /R /I "^[0-9A-F][0-9A-F]*$"') do set "ACTUAL=%%H"
set "ACTUAL=%ACTUAL: =%"
if not defined ACTUAL (
  echo ERRO: nao foi possivel calcular o hash SHA-256.
  exit /b 3
)
set "EXPECTED=7ad4495f6d81a1b982110e6bbdfd1cc415e46f1c96964fb9f47fa36120fb916b"
if /I not "%ACTUAL%"=="%EXPECTED%" (
  echo ERRO: o hash do arquivo instalado nao corresponde ao manifesto.
  echo Esperado: %EXPECTED%
  echo Obtido:   %ACTUAL%
  exit /b 3
)

> "%DEST%\CONFIRMACAO-INSTALACAO.txt" echo Fluxo Mestre instalado e validado em %DATE% %TIME%.
>> "%DEST%\CONFIRMACAO-INSTALACAO.txt" echo SHA-256: %ACTUAL%

echo.
echo Fluxo Mestre instalado com sucesso em:
echo %DEST%
echo.
start "" explorer "%DEST%"
exit /b 0
