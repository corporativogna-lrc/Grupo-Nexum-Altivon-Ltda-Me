<#
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7184
 #>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$OutputPath,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string[]]$Tables,

    [ValidateNotNullOrEmpty()]
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,

    [ValidateSet('DefaultConnection', 'GenesisConnection')]
    [string]$ConnectionName = 'DefaultConnection'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Resolve-ConnectorAssembly {
    param([string]$Root)

    $candidates = @(
        (Join-Path $Root 'runtime\api-24h\api\MySqlConnector.dll'),
        (Join-Path $Root 'NexumAltivon_Back-End\bin\Release\net8.0\MySqlConnector.dll'),
        (Join-Path $Root 'NexumAltivon_Back-End\bin\Debug\net8.0\MySqlConnector.dll')
    )

    $assembly = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
    if (-not $assembly) {
        throw 'MySqlConnector.dll nao encontrado. Compile ou publique a API oficial antes do backup.'
    }

    return $assembly
}

function Resolve-SystemVersion {
    param([string]$Root)

    $propsPath = Join-Path $Root 'Directory.Build.props'
    if (-not (Test-Path -LiteralPath $propsPath)) {
        throw "Arquivo de versao ausente: $propsPath"
    }

    [xml]$props = Get-Content -LiteralPath $propsPath -Raw
    $version = [string]$props.Project.PropertyGroup.Version
    if ([string]::IsNullOrWhiteSpace($version)) {
        throw "Versao nao definida em $propsPath"
    }

    return $version
}

$resolvedRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$privateConfigPath = Join-Path $resolvedRoot 'runtime\api-24h\api.env.ps1'
if (-not (Test-Path -LiteralPath $privateConfigPath)) {
    throw "Configuracao privada oficial ausente: $privateConfigPath"
}

$invalidTable = $Tables | Where-Object { $_ -notmatch '^[A-Za-z0-9_]+$' } | Select-Object -First 1
if ($invalidTable) {
    throw "Nome de tabela invalido: $invalidTable"
}

. $privateConfigPath
$connectionEnvironmentName = "ConnectionStrings__$ConnectionName"
$connectionString = [Environment]::GetEnvironmentVariable($connectionEnvironmentName, 'Process')
if ([string]::IsNullOrWhiteSpace($connectionString)) {
    throw "Conexao $connectionEnvironmentName nao configurada no ambiente privado oficial."
}

$connectorAssembly = Resolve-ConnectorAssembly -Root $resolvedRoot
Add-Type -Path $connectorAssembly
$connection = [MySqlConnector.MySqlConnectionStringBuilder]::new($connectionString)

$dumpExecutable = 'D:\xampp\mysql\bin\mysqldump.exe'
if (-not (Test-Path -LiteralPath $dumpExecutable)) {
    throw "Executavel de backup MySQL ausente: $dumpExecutable"
}

$resolvedOutput = [System.IO.Path]::GetFullPath($OutputPath)
$outputDirectory = Split-Path -Parent $resolvedOutput
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
$bodyPath = "$resolvedOutput.body"
$errorPath = "$resolvedOutput.stderr"

$arguments = @(
    '--protocol=TCP',
    '--single-transaction',
    '--skip-lock-tables',
    '--hex-blob',
    '--default-character-set=utf8mb4',
    '--skip-comments',
    '-h', [string]$connection.Server,
    '-P', [string]$connection.Port,
    '-u', [string]$connection.UserID,
    "--result-file=$bodyPath",
    [string]$connection.Database
) + $Tables

$previousPassword = $env:MYSQL_PWD
$exitCode = -1
try {
    $env:MYSQL_PWD = $connection.Password
    & $dumpExecutable @arguments 2> $errorPath
    $exitCode = $LASTEXITCODE
}
finally {
    if ($null -eq $previousPassword) {
        Remove-Item Env:\MYSQL_PWD -ErrorAction SilentlyContinue
    }
    else {
        $env:MYSQL_PWD = $previousPassword
    }
}

if ($exitCode -ne 0) {
    $errorText = if (Test-Path -LiteralPath $errorPath) { Get-Content -LiteralPath $errorPath -Raw } else { '' }
    throw "mysqldump falhou com codigo $exitCode. $errorText"
}

$version = Resolve-SystemVersion -Root $resolvedRoot
$header = @"
/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: $version
 */

"@

$utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)
$dumpBody = [System.IO.File]::ReadAllText($bodyPath)
[System.IO.File]::WriteAllText($resolvedOutput, $header + $dumpBody, $utf8WithoutBom)
Remove-Item -LiteralPath $bodyPath, $errorPath -Force -ErrorAction SilentlyContinue

$hash = Get-FileHash -LiteralPath $resolvedOutput -Algorithm SHA256
[pscustomobject]@{
    BackupPath = $resolvedOutput
    Database = $connection.Database
    TableCount = $Tables.Count
    Bytes = (Get-Item -LiteralPath $resolvedOutput).Length
    SHA256 = $hash.Hash
    CompletedAt = (Get-Date).ToString('o')
}
