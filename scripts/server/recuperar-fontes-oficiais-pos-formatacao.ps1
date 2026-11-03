#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5.7200
#

[CmdletBinding()]
param(
    [string]$ProjectRoot = "D:\Nexum Altivon\NexumAltivon.com",
    [string]$FrontendSource = "NexumAltivon_Front-End\src"
)

$ErrorActionPreference = "Stop"

function Get-FullPath {
    param([string]$Path)
    return [System.IO.Path]::GetFullPath($Path).TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
}

$root = Get-FullPath -Path $ProjectRoot
$expectedRoot = Get-FullPath -Path "D:\Nexum Altivon\NexumAltivon.com"
if ($root -ine $expectedRoot) {
    throw "A recuperação foi bloqueada: o diretório informado não é o projeto oficial D:\Nexum Altivon\NexumAltivon.com."
}

$frontendRoot = Get-FullPath -Path (Join-Path $root $FrontendSource)
if (-not (Test-Path -LiteralPath $frontendRoot)) {
    throw "Diretório de fontes do frontend não encontrado: $frontendRoot"
}

$git = Get-Command git.exe -ErrorAction Stop
$repositoryRoot = (& $git.Source -C $root.TrimEnd([System.IO.Path]::DirectorySeparatorChar) rev-parse --show-toplevel).Trim()
if ([System.IO.Path]::GetFullPath($repositoryRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar) -ine $root.TrimEnd([System.IO.Path]::DirectorySeparatorChar)) {
    throw "O diretório informado não corresponde à raiz Git oficial."
}

$changed = @(& $git.Source -C $root.TrimEnd([System.IO.Path]::DirectorySeparatorChar) diff --name-only -- $FrontendSource)
$untracked = @(& $git.Source -C $root.TrimEnd([System.IO.Path]::DirectorySeparatorChar) ls-files --others --exclude-standard -- $FrontendSource)
$tracked = @(& $git.Source -C $root.TrimEnd([System.IO.Path]::DirectorySeparatorChar) ls-files -- $FrontendSource)

$recovered = [System.Collections.Generic.List[string]]::new()
$skipped = [System.Collections.Generic.List[string]]::new()
$errors = [System.Collections.Generic.List[string]]::new()

foreach ($relativePath in $tracked) {
    if ([string]::IsNullOrWhiteSpace($relativePath)) {
        continue
    }

    if ($changed -contains $relativePath -or $untracked -contains $relativePath) {
        $skipped.Add($relativePath)
        continue
    }

    $fullPath = [System.IO.Path]::GetFullPath((Join-Path $root $relativePath))
    if (-not $fullPath.StartsWith($frontendRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Caminho fora do escopo do frontend: $relativePath"
    }

    if (-not (Test-Path -LiteralPath $fullPath)) {
        continue
    }

    $item = Get-Item -LiteralPath $fullPath
    if ($item.Attributes.ToString() -notmatch "Encrypted") {
        continue
    }

    try {
        Remove-Item -LiteralPath $fullPath -Force
        & $git.Source -C $root.TrimEnd([System.IO.Path]::DirectorySeparatorChar) checkout-index --force -- $relativePath
        if ($LASTEXITCODE -ne 0) {
            throw "git checkout-index retornou código $LASTEXITCODE"
        }
        $recovered.Add($relativePath)
    }
    catch {
        $errors.Add("$relativePath :: $($_.Exception.Message)")
    }
}

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Error $_ }
    throw "A recuperação terminou com falhas. Nenhum arquivo alterado localmente foi sobrescrito."
}

[pscustomobject]@{
    ProjectRoot = $root.TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    TrackedFiles = $tracked.Count
    RecoveredEncryptedFiles = $recovered.Count
    PreservedChangedOrUntrackedFiles = $skipped.Count
    RecoveredPaths = @($recovered)
}
