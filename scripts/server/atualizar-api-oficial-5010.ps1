#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5
#

[CmdletBinding()]
param(
    [string]$ProjectRoot = "D:\Nexum Altivon\NexumAltivon.com",
    [string]$TaskName = "NexumAltivonApi24h",
    [string]$ApiUrl = "http://127.0.0.1:5010"
)

$ErrorActionPreference = "Stop"

$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal]::new($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Execute este script em PowerShell como Administrador. A API oficial 5010 roda elevada e bloqueia o publish sem permissao administrativa."
}

$resolvedProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$apiProject = Join-Path $resolvedProjectRoot "NexumAltivon_Back-End\NexumAltivon.API.csproj"
$publishDir = Join-Path $resolvedProjectRoot "runtime\api-24h\api"
$privateConfig = Join-Path $resolvedProjectRoot "runtime\api-24h\api.env.ps1"
$installTaskScript = Join-Path $resolvedProjectRoot "scripts\server\instalar-api-oficial-24h-task.ps1"
$validationScript = Join-Path $resolvedProjectRoot "scripts\server\validar-api-oficial-24h-task.ps1"
$logDir = Join-Path $resolvedProjectRoot "runtime-logs"
$logPath = Join-Path $logDir "api-oficial-5010-update.log"

New-Item -ItemType Directory -Path $logDir -Force | Out-Null

if (-not (Test-Path -LiteralPath $apiProject -PathType Leaf)) {
    throw "Projeto oficial da API nao encontrado em $apiProject."
}

if (-not (Test-Path -LiteralPath $privateConfig -PathType Leaf)) {
    throw "Configuracao privada oficial nao encontrada em $privateConfig."
}

if (-not (Test-Path -LiteralPath $installTaskScript -PathType Leaf)) {
    throw "Script de instalacao da tarefa oficial nao encontrado em $installTaskScript."
}

if (-not (Test-Path -LiteralPath $validationScript -PathType Leaf)) {
    throw "Script de validacao da tarefa oficial nao encontrado em $validationScript."
}

function Write-Step([string]$Message) {
    $line = "[$((Get-Date).ToString('s'))] $Message"
    Write-Output $line
    Add-Content -LiteralPath $logPath -Value $line -Encoding UTF8
}

function Stop-ApiPortOwner {
    param([int]$Port)

    $listeners = @(Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue)
    foreach ($listener in $listeners) {
        $pidToStop = [int]$listener.OwningProcess
        Write-Step "Parando processo dono da porta ${Port}: PID $pidToStop."
        Stop-Process -Id $pidToStop -Force -ErrorAction Stop
    }

    for ($i = 0; $i -lt 20; $i++) {
        $busy = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($null -eq $busy) {
            Write-Step "Porta $Port liberada."
            return
        }

        Start-Sleep -Seconds 1
    }

    $remaining = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($remaining) {
        throw "Porta $Port ainda ocupada pelo PID $($remaining.OwningProcess) apos tentativa administrativa de parada."
    }
}

Write-Step "Inicio da atualizacao oficial da API em $ApiUrl."

Stop-ApiPortOwner -Port 5010

Write-Step "Publicando API oficial em $publishDir."
& dotnet publish $apiProject -c Release -o $publishDir --self-contained false 2>&1 |
    Tee-Object -FilePath $logPath -Append
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish da API oficial falhou com codigo $LASTEXITCODE. Consulte $logPath."
}

Write-Step "Registrando e iniciando tarefa oficial $TaskName."
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $installTaskScript -ProjectRoot $resolvedProjectRoot -TaskName $TaskName -ApiUrl $ApiUrl 2>&1 |
    Tee-Object -FilePath $logPath -Append
if ($LASTEXITCODE -ne 0) {
    throw "Instalacao/inicializacao da tarefa oficial falhou com codigo $LASTEXITCODE. Consulte $logPath."
}

Write-Step "Validando tarefa oficial $TaskName."
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $validationScript -ProjectRoot $resolvedProjectRoot -TaskName $TaskName -ApiUrl $ApiUrl 2>&1 |
    Tee-Object -FilePath $logPath -Append
if ($LASTEXITCODE -ne 0) {
    throw "Validacao da tarefa oficial falhou com codigo $LASTEXITCODE. Consulte $logPath."
}

$endpoints = @(
    "$ApiUrl/health",
    "$ApiUrl/health/db",
    "$ApiUrl/health/db/genesis",
    "$ApiUrl/api/site/configuracoes/publico"
)

foreach ($endpoint in $endpoints) {
    Write-Step "Validando endpoint $endpoint."
    $response = Invoke-WebRequest -UseBasicParsing -Uri $endpoint -TimeoutSec 20
    if ($response.StatusCode -lt 200 -or $response.StatusCode -gt 299) {
        throw "Endpoint $endpoint retornou HTTP $($response.StatusCode)."
    }
    Write-Step "Endpoint $endpoint OK HTTP $($response.StatusCode)."
}

$listener = Get-NetTCPConnection -LocalPort 5010 -State Listen -ErrorAction Stop | Select-Object -First 1
$process = Get-CimInstance Win32_Process -Filter "ProcessId = $($listener.OwningProcess)"

$result = [pscustomobject]@{
    UpdatedAt = (Get-Date).ToString("s")
    ProjectRoot = $resolvedProjectRoot
    ApiUrl = $ApiUrl
    TaskName = $TaskName
    ListenerPid = $listener.OwningProcess
    ListenerCommand = $process.CommandLine
    PublishDir = $publishDir
    LogPath = $logPath
}

$result | Format-List | Tee-Object -FilePath $logPath -Append
