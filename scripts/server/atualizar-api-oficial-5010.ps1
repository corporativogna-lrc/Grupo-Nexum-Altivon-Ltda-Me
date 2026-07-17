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
    [string]$ApiUrl = "http://127.0.0.1:5010",
    [string]$DatabaseServiceName = "NexumAltivonMySQL",
    [string]$TunnelServiceName = "Cloudflared"
)

$ErrorActionPreference = "Stop"

$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal]::new($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Execute este script em PowerShell como Administrador. A API oficial 5010 roda elevada e bloqueia o publish sem permissao administrativa."
}

$apiUri = [Uri]$ApiUrl
if ($apiUri.AbsoluteUri.TrimEnd('/') -ne "http://127.0.0.1:5010") {
    throw "O atualizador oficial aceita somente http://127.0.0.1:5010. Valor recebido: $ApiUrl"
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
Set-Content -LiteralPath $logPath -Value "" -Encoding UTF8

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

function Stop-OfficialRuntime {
    $task = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
    if ($task -and $task.State -eq "Running") {
        Write-Step "Parando tarefa oficial $TaskName antes da publicacao."
        Stop-ScheduledTask -TaskName $TaskName -ErrorAction Stop
        Start-Sleep -Seconds 2
    }

    $wrappers = Get-CimInstance Win32_Process -Filter "Name = 'powershell.exe'" |
        Where-Object { $_.ProcessId -ne $PID -and $_.CommandLine -like "*iniciar-api-oficial-24h.ps1*" }
    foreach ($process in $wrappers) {
        Write-Step "Encerrando supervisor oficial PID $($process.ProcessId)."
        Stop-Process -Id $process.ProcessId -Force -ErrorAction SilentlyContinue
    }

    $apiProcesses = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
        Where-Object { $_.CommandLine -like "*NexumAltivon.API.dll*" }
    foreach ($process in $apiProcesses) {
        Write-Step "Encerrando API oficial PID $($process.ProcessId)."
        Stop-Process -Id $process.ProcessId -Force -ErrorAction SilentlyContinue
    }

    for ($attempt = 1; $attempt -le 30; $attempt++) {
        $busy = Get-NetTCPConnection -LocalPort 5010 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
        if (-not $busy) {
            Write-Step "Porta 5010 liberada pela parada controlada da API oficial."
            return
        }

        Start-Sleep -Seconds 1
    }

    $remaining = Get-NetTCPConnection -LocalPort 5010 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
    $remainingProcess = if ($remaining) { Get-CimInstance Win32_Process -Filter "ProcessId = $($remaining.OwningProcess)" } else { $null }
    throw "A porta 5010 esta ocupada por processo nao identificado como a API oficial. PID=$($remaining.OwningProcess) Executavel=$($remainingProcess.ExecutablePath). O atualizador nao encerra processos desconhecidos."
}

function Invoke-OfficialEndpointValidation {
    param(
        [Parameter(Mandatory = $true)][string]$Endpoint,
        [int]$Attempts = 5,
        [int]$TimeoutSeconds = 25
    )

    $lastError = $null
    for ($attempt = 1; $attempt -le $Attempts; $attempt++) {
        Write-Step "Validando endpoint $Endpoint tentativa $attempt de $Attempts."
        try {
            $response = Invoke-WebRequest -UseBasicParsing -Uri $Endpoint -TimeoutSec $TimeoutSeconds
            if ($response.StatusCode -ge 200 -and $response.StatusCode -le 299) {
                Write-Step "Endpoint $Endpoint OK HTTP $($response.StatusCode)."
                return
            }

            $lastError = "HTTP $($response.StatusCode)"
        } catch {
            $lastError = $_.Exception.Message
        }

        if ($attempt -lt $Attempts) {
            Start-Sleep -Seconds 2
        }
    }

    throw "Endpoint $Endpoint falhou apos $Attempts tentativa(s). Ultimo erro: $lastError"
}

Write-Step "Inicio da atualizacao oficial da API em $ApiUrl."

Stop-OfficialRuntime

Write-Step "Publicando API oficial em $publishDir."
& dotnet publish $apiProject -c Release -o $publishDir --self-contained false 2>&1 |
    ForEach-Object {
        $line = [string]$_
        Write-Output $line
        Add-Content -LiteralPath $logPath -Value $line -Encoding UTF8
    }
$publishExitCode = $LASTEXITCODE
if ($publishExitCode -ne 0) {
    throw "dotnet publish da API oficial falhou com codigo $publishExitCode. Consulte $logPath."
}

Write-Step "Registrando e iniciando tarefa oficial $TaskName."
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $installTaskScript -ProjectRoot $resolvedProjectRoot -TaskName $TaskName -ApiUrl $ApiUrl -DatabaseServiceName $DatabaseServiceName -TunnelServiceName $TunnelServiceName 2>&1 |
    ForEach-Object {
        $line = [string]$_
        Write-Output $line
        Add-Content -LiteralPath $logPath -Value $line -Encoding UTF8
    }
$installExitCode = $LASTEXITCODE
if ($installExitCode -ne 0) {
    throw "Instalacao/inicializacao da tarefa oficial falhou com codigo $installExitCode. Consulte $logPath."
}

Write-Step "Validando tarefa oficial $TaskName."
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $validationScript -ProjectRoot $resolvedProjectRoot -TaskName $TaskName -ApiUrl $ApiUrl -DatabaseServiceName $DatabaseServiceName -TunnelServiceName $TunnelServiceName 2>&1 |
    ForEach-Object {
        $line = [string]$_
        Write-Output $line
        Add-Content -LiteralPath $logPath -Value $line -Encoding UTF8
    }
$validationExitCode = $LASTEXITCODE
if ($validationExitCode -ne 0) {
    throw "Validacao da tarefa oficial falhou com codigo $validationExitCode. Consulte $logPath."
}

$endpoints = @(
    "$ApiUrl/health",
    "$ApiUrl/health/db",
    "$ApiUrl/health/db/genesis",
    "$ApiUrl/api/site/configuracoes/publico"
)

foreach ($endpoint in $endpoints) {
    Invoke-OfficialEndpointValidation -Endpoint $endpoint
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

$formattedResult = $result | Format-List | Out-String
Write-Output $formattedResult
Add-Content -LiteralPath $logPath -Value $formattedResult -Encoding UTF8
