param(
    [string]$Url = $env:NEXUM_ERP_URL,
    [ValidateSet('Perguntar', 'Chrome', 'Edge', 'Sistema')]
    [string]$BrowserChoice = $(if ($env:NEXUM_ERP_BROWSER) { $env:NEXUM_ERP_BROWSER } else { 'Perguntar' })
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$target = Join-Path $scriptDir "erp-desktop.ps1"

if ([string]::IsNullOrWhiteSpace($Url)) {
    $Url = "http://127.0.0.1:3000/dashboard"
}

if (-not (Test-Path -LiteralPath $target)) {
    throw "Nao foi possivel localizar o launcher do ERP Desktop: $target"
}

& $target -Url $Url -BrowserChoice $BrowserChoice
