param(
    [string]$Url = $env:NEXUM_ERP_URL
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$target = Join-Path (Split-Path -Parent $scriptDir) "erp-desktop.ps1"

if (-not (Test-Path -LiteralPath $target)) {
    throw "Nao foi possivel localizar o script de compatibilidade: $target"
}

& $target -Url $Url
