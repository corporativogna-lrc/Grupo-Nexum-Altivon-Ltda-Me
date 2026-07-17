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
    [string]$InstallRoot = "",
    [string]$TaskName = "NexumAltivonApi24h",
    [int]$Port = 5010,
    [int]$CloudflareOriginPort = 5010,
    [string]$XamppRoot = "D:\xampp",
    [string]$DatabaseServiceName = "NexumAltivonMySQL",
    [int]$DatabasePort = 3309,
    [string[]]$DatabaseDataDirs = @(
        "D:\xampp\mysql\data\nexum_altivon",
        "D:\xampp\mysql\data\genesis_bd"
    ),
    [int]$StartupTimeoutSeconds = 180,
    [switch]$RunAsSystem
)

$ErrorActionPreference = "Stop"

$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal]::new($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Execute este instalador em PowerShell ou CMD como Administrador."
}

if ($Port -ne 5010 -or $CloudflareOriginPort -ne 5010) {
    throw "A API e a origem Cloudflare devem usar exclusivamente a porta oficial 5010. Port=$Port CloudflareOriginPort=$CloudflareOriginPort."
}

if ($DatabasePort -ne 3309) {
    throw "O banco oficial deve usar a porta 3309. Valor recebido: $DatabasePort"
}

$resolvedProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$expectedInstallRoot = Join-Path $resolvedProjectRoot "runtime\api-24h"
if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
    $InstallRoot = $expectedInstallRoot
}

$normalizedInstallRoot = [IO.Path]::GetFullPath($InstallRoot).TrimEnd('\')
$normalizedExpectedRoot = [IO.Path]::GetFullPath($expectedInstallRoot).TrimEnd('\')
if (-not $normalizedInstallRoot.Equals($normalizedExpectedRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Instalacao externa ao projeto oficial foi recusada. Caminho permitido: $normalizedExpectedRoot"
}

if (-not (Test-Path -LiteralPath $XamppRoot -PathType Container)) {
    throw "XAMPP oficial nao encontrado em $XamppRoot."
}

foreach ($databaseDataDir in $DatabaseDataDirs) {
    if (-not (Test-Path -LiteralPath $databaseDataDir -PathType Container)) {
        throw "Diretorio oficial de dados do banco nao encontrado: $databaseDataDir"
    }
}

$updater = Join-Path $resolvedProjectRoot "scripts\server\atualizar-api-oficial-5010.ps1"
if (-not (Test-Path -LiteralPath $updater -PathType Leaf)) {
    throw "Atualizador oficial nao encontrado em $updater."
}

Write-Output "Encaminhando instalacao para o fluxo oficial em $normalizedExpectedRoot."
& $updater `
    -ProjectRoot $resolvedProjectRoot `
    -TaskName $TaskName `
    -ApiUrl "http://127.0.0.1:5010" `
    -DatabaseServiceName $DatabaseServiceName `
    -TunnelServiceName "Cloudflared"

if (-not $?) {
    throw "O atualizador oficial da API nao concluiu a instalacao."
}
