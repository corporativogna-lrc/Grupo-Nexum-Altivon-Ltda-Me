#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5
#

[CmdletBinding()]
param(
    [string]$Source = "D:\Nexum Altivon\NexumAltivon.com",
    [string]$Destination = "D:\Nexum Altivon\Backups\NexumAltivon.com-current",
    [switch]$ValidateOnly
)

$ErrorActionPreference = "Stop"

function Resolve-RequiredDirectory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        throw "$Name nao encontrado: $Path"
    }

    return [System.IO.Path]::GetFullPath((Resolve-Path -LiteralPath $Path).Path)
}

function Resolve-SafeDestination {
    param([string]$Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $allowedRoot = [System.IO.Path]::GetFullPath("D:\Nexum Altivon\Backups")
    if (-not $fullPath.StartsWith($allowedRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Destino de backup fora da raiz permitida: $fullPath. Raiz permitida: $allowedRoot"
    }

    if ($fullPath.TrimEnd('\') -eq $allowedRoot.TrimEnd('\')) {
        throw "Destino de backup nao pode ser a raiz de backups: $allowedRoot"
    }

    return $fullPath
}

$resolvedSource = Resolve-RequiredDirectory -Path $Source -Name "Projeto oficial"
$resolvedDestination = Resolve-SafeDestination -Path $Destination
$destinationParent = Split-Path -Parent $resolvedDestination

if (-not (Test-Path -LiteralPath $destinationParent -PathType Container)) {
    New-Item -ItemType Directory -Force -Path $destinationParent | Out-Null
}

$excludedDirectories = @(
    "node_modules",
    "bin",
    "obj",
    "runtime",
    "runtime-logs",
    ".nexum-runtime",
    ".vs",
    "Revisao_Exclusao_*"
)

$excludedFiles = @(
    "*.log",
    "*.tmp",
    "Thumbs.db"
)

$robocopyArgs = @(
    $resolvedSource,
    $resolvedDestination,
    "/MIR",
    "/COPY:DAT",
    "/DCOPY:DAT",
    "/R:1",
    "/W:2",
    "/MT:16",
    "/FFT",
    "/XA:SH",
    "/XD"
) + $excludedDirectories + @("/XF") + $excludedFiles

if ($ValidateOnly) {
    [pscustomobject]@{
        Source = $resolvedSource
        Destination = $resolvedDestination
        ExcludedDirectories = $excludedDirectories -join "; "
        ExcludedFiles = $excludedFiles -join "; "
        Command = "robocopy " + ($robocopyArgs -join " ")
    } | Format-List
    exit 0
}

New-Item -ItemType Directory -Force -Path $resolvedDestination | Out-Null
& robocopy @robocopyArgs
$exitCode = $LASTEXITCODE

if ($exitCode -le 7) {
    exit 0
}

throw "Robocopy falhou com codigo $exitCode ao copiar $resolvedSource para $resolvedDestination."
