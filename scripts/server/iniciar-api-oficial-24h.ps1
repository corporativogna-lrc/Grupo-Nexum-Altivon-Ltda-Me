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
    [string]$PrivateConfigPath = "",
    [string]$ApiUrl = "http://127.0.0.1:5010"
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($PrivateConfigPath)) {
    $PrivateConfigPath = Join-Path $ProjectRoot "runtime\api-24h\api.env.ps1"
}

$resolvedProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$publishDir = Join-Path $resolvedProjectRoot "runtime\api-24h\api"
$logDir = Join-Path $resolvedProjectRoot "runtime-logs\api-24h"
$apiDll = Join-Path $publishDir "NexumAltivon.API.dll"
$publicWebRoot = Join-Path $resolvedProjectRoot "NexumAltivon_Back-End\wwwroot"
$publicUploadRoot = Join-Path $publicWebRoot "uploads"

if (-not (Test-Path -LiteralPath $PrivateConfigPath -PathType Leaf)) {
    throw "Configuracao privada da API nao encontrada em $PrivateConfigPath."
}

if (-not (Test-Path -LiteralPath $apiDll -PathType Leaf)) {
    throw "Binario da API nao encontrado em $apiDll. Execute dotnet publish antes de iniciar a tarefa."
}

if (-not (Test-Path -LiteralPath $publicUploadRoot -PathType Container)) {
    throw "Diretorio oficial de midias publicas nao encontrado em $publicUploadRoot."
}

. $PrivateConfigPath

$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ASPNETCORE_URLS = $ApiUrl
$env:Storage__PublicWebRoot = $publicWebRoot

New-Item -ItemType Directory -Path $logDir -Force | Out-Null
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$stdout = Join-Path $logDir "api-$timestamp.stdout.log"
$stderr = Join-Path $logDir "api-$timestamp.stderr.log"

Set-Location -LiteralPath $publishDir
& dotnet "NexumAltivon.API.dll" 1>> $stdout 2>> $stderr
exit $LASTEXITCODE
