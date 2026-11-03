#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5.7241
#

[CmdletBinding()]
param(
    [string]$ProjectRoot = "D:\Nexum Altivon\NexumAltivon.com",
    [string]$TaskName = "NexumAltivonApi24h",
    [string]$ApiUrl = "http://127.0.0.1:5010",
    [string]$PublicApiUrl = "https://api.nexumaltivon.com.br",
    [string]$DatabaseServiceName = "NexumAltivonMySQL",
    [string]$TunnelServiceName = "Cloudflared",
    [switch]$LocalOnly
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
$apiAssets = Join-Path $resolvedProjectRoot "NexumAltivon_Back-End\obj\project.assets.json"
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

if (-not (Test-Path -LiteralPath $apiAssets -PathType Leaf)) {
    throw "Dependencias restauradas da API nao encontradas em $apiAssets. Execute dotnet restore no projeto oficial antes da publicacao."
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

function Ensure-IntegrationEncryptionKey {
    $content = Get-Content -LiteralPath $privateConfig -Raw
    $match = [regex]::Match($content, "(?m)^\s*\`$env:Security__IntegrationEncryptionKey\s*=\s*'(?<value>[^']+)'\s*$")
    if ($match.Success) {
        try {
            $configuredKey = [Convert]::FromBase64String($match.Groups['value'].Value)
            if ($configuredKey.Length -ne 32) {
                throw "tamanho invalido"
            }
            [Array]::Clear($configuredKey, 0, $configuredKey.Length)
            Write-Step "Chave mestra de integracoes confirmada na configuracao privada."
            return
        } catch {
            throw "Security__IntegrationEncryptionKey existe, mas nao contem 32 bytes validos em Base64."
        }
    }

    $newKey = New-Object byte[] 32
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $rng.GetBytes($newKey)
        $encodedKey = [Convert]::ToBase64String($newKey)
        Add-Content -LiteralPath $privateConfig -Value "`r`n`$env:Security__IntegrationEncryptionKey = '$encodedKey'" -Encoding UTF8
        Write-Step "Chave mestra de integracoes gerada e gravada somente na configuracao privada ignorada pelo Git."
    } finally {
        $rng.Dispose()
        [Array]::Clear($newKey, 0, $newKey.Length)
    }
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

Ensure-IntegrationEncryptionKey

$publishParent = Split-Path -Parent $publishDir
$publishStamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$stagingDir = Join-Path $publishParent "api-publish-staging-$publishStamp"
$rollbackDir = Join-Path $publishParent "api-rollback-$publishStamp"
New-Item -ItemType Directory -Path $stagingDir -Force | Out-Null

Write-Step "Publicando API oficial em diretorio limpo de staging: $stagingDir."
& dotnet publish $apiProject -c Release -o $stagingDir --self-contained false --no-restore -p:CopyToPublishDirectory=Always 2>&1 |
    ForEach-Object {
        $line = [string]$_
        Write-Output $line
        Add-Content -LiteralPath $logPath -Value $line -Encoding UTF8
    }
$publishExitCode = $LASTEXITCODE
if ($publishExitCode -ne 0) {
    throw "dotnet publish da API oficial falhou com codigo $publishExitCode. Consulte $logPath."
}

$stagedApiDll = Join-Path $stagingDir 'NexumAltivon.API.dll'
$stagedDepsJson = Join-Path $stagingDir 'NexumAltivon.API.deps.json'
$stagedEfCoreDll = Join-Path $stagingDir 'Microsoft.EntityFrameworkCore.dll'
foreach ($requiredArtifact in @($stagedApiDll, $stagedDepsJson, $stagedEfCoreDll)) {
    if (-not (Test-Path -LiteralPath $requiredArtifact -PathType Leaf)) {
        throw "Artefato obrigatorio ausente no publish staging: $requiredArtifact. A API anterior permanece preservada em $publishDir."
    }
}

$apiPackageAssets = Get-Content -LiteralPath $apiAssets -Raw | ConvertFrom-Json
$efCoreTarget = $apiPackageAssets.targets.PSObject.Properties |
    ForEach-Object { $_.Value.PSObject.Properties } |
    Where-Object { $_.Name -match '^Microsoft\.EntityFrameworkCore/[0-9]+\.' } |
    Select-Object -First 1
if (-not $efCoreTarget) {
    throw "Microsoft.EntityFrameworkCore nao consta nos assets restaurados em $apiAssets."
}

$expectedEfCoreVersion = [Version]($efCoreTarget.Name -replace '^Microsoft\.EntityFrameworkCore/', '')
$publishedEfCoreVersion = [Reflection.AssemblyName]::GetAssemblyName($stagedEfCoreDll).Version
if ($publishedEfCoreVersion.Major -ne $expectedEfCoreVersion.Major -or $publishedEfCoreVersion.Minor -ne $expectedEfCoreVersion.Minor) {
    throw "Versao EF Core do publish staging ($publishedEfCoreVersion) difere dos assets restaurados ($expectedEfCoreVersion). A API anterior permanece preservada em $publishDir."
}

# So interrompe a API depois de o novo publish estar completo e validado.
Stop-OfficialRuntime

if (Test-Path -LiteralPath $publishDir -PathType Container) {
    Move-Item -LiteralPath $publishDir -Destination $rollbackDir
}

try {
    Move-Item -LiteralPath $stagingDir -Destination $publishDir
} catch {
    if (-not (Test-Path -LiteralPath $publishDir -PathType Container) -and
        (Test-Path -LiteralPath $rollbackDir -PathType Container)) {
        Move-Item -LiteralPath $rollbackDir -Destination $publishDir
    }

    throw
}

Write-Step "Publish validado e promovido; versao EF Core $publishedEfCoreVersion. Rollback preservado em $rollbackDir."

Write-Step "Registrando e iniciando tarefa oficial $TaskName."
$installArguments = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $installTaskScript, '-ProjectRoot', $resolvedProjectRoot, '-TaskName', $TaskName, '-ApiUrl', $ApiUrl, '-DatabaseServiceName', $DatabaseServiceName, '-TunnelServiceName', $TunnelServiceName)
if ($LocalOnly) { $installArguments += '-LocalOnly' }
& powershell.exe @installArguments 2>&1 |
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
$validationArguments = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $validationScript, '-ProjectRoot', $resolvedProjectRoot, '-TaskName', $TaskName, '-ApiUrl', $ApiUrl, '-PublicApiUrl', $PublicApiUrl, '-DatabaseServiceName', $DatabaseServiceName, '-TunnelServiceName', $TunnelServiceName)
if ($LocalOnly) { $validationArguments += '-LocalOnly' }
& powershell.exe @validationArguments 2>&1 |
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
if (-not $LocalOnly) {
    $endpoints += "$($PublicApiUrl.TrimEnd('/'))/health"
}

foreach ($endpoint in $endpoints) {
    Invoke-OfficialEndpointValidation -Endpoint $endpoint
}

$listener = Get-NetTCPConnection -LocalPort 5010 -State Listen -ErrorAction Stop | Select-Object -First 1
$process = Get-CimInstance Win32_Process -Filter "ProcessId = $($listener.OwningProcess)"

$result = [pscustomobject]@{
    UpdatedAt = (Get-Date).ToString("s")
    ProjectRoot = $resolvedProjectRoot
    ApiUrl = $ApiUrl
    LocalOnly = $LocalOnly.IsPresent
    PublicValidationSkipped = $LocalOnly.IsPresent
    TaskName = $TaskName
    ListenerPid = $listener.OwningProcess
    ListenerCommand = $process.CommandLine
    PublishDir = $publishDir
    LogPath = $logPath
}

$formattedResult = $result | Format-List | Out-String
Write-Output $formattedResult
Add-Content -LiteralPath $logPath -Value $formattedResult -Encoding UTF8
