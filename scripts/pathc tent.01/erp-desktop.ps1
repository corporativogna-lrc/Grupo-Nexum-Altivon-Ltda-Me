param(
    [string]$Url = $env:NEXUM_ERP_URL
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($Url)) {
    $Url = "http://localhost:3000/dashboard/erp"
}

$profileDir = Join-Path $env:LOCALAPPDATA "NexumAltivon\ERPDesktop"
New-Item -ItemType Directory -Path $profileDir -Force | Out-Null

$edgeCandidates = @(
    "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe",
    "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe"
)

$chromeCandidates = @(
    "$env:ProgramFiles\Google\Chrome\Application\chrome.exe",
    "${env:ProgramFiles(x86)}\Google\Chrome\Application\chrome.exe"
)

$browser = $edgeCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $browser) {
    $browser = $chromeCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}

if ($browser) {
    Start-Process -FilePath $browser -ArgumentList @(
        "--app=$Url",
        "--new-window",
        "--user-data-dir=$profileDir",
        "--class=NexumAltivonERP"
    )
    exit 0
}

Start-Process $Url
