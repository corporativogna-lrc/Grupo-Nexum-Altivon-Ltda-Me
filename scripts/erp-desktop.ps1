param(
    [string]$Url = $env:NEXUM_ERP_URL
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$target = Join-Path $scriptDir "erp-desktop-corrigido.ps1"

if (-not (Test-Path -LiteralPath $target)) {
    throw "Nao foi possivel localizar o script corrigido: $target"
}

& $target -Url $Url
