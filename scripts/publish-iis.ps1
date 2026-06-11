param(
    [string]$SiteRoot = "C:\inetpub\wwwroot\nexumaltivon",
    [string]$ApiUrl = "http://localhost:5010",
    [string]$BackendUrl = "http://localhost:5010"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$apiProject = Join-Path $repoRoot "NexumAltivon_Back-End\NexumAltivon.API.csproj"
$frontRoot = Join-Path $repoRoot "NexumAltivon_Front-End"
$publishRoot = Join-Path $repoRoot "publish-iis"
$apiPublish = Join-Path $publishRoot "api"
$frontPublish = Join-Path $publishRoot "site"

Write-Host "Publishing API..."
dotnet publish $apiProject -c Release -o $apiPublish /p:UseAppHost=false

Write-Host "Building frontend..."
Push-Location $frontRoot
$env:REACT_APP_BACKEND_URL = $BackendUrl
npm ci
npm run build
Pop-Location

if (Test-Path $frontPublish) {
    Remove-Item $frontPublish -Recurse -Force
}
New-Item -ItemType Directory -Path $frontPublish | Out-Null
Copy-Item (Join-Path $frontRoot "build\*") $frontPublish -Recurse -Force

Write-Host "Copying publish output to $SiteRoot..."
New-Item -ItemType Directory -Path (Join-Path $SiteRoot "api") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $SiteRoot "site") -Force | Out-Null
Copy-Item (Join-Path $apiPublish "*") (Join-Path $SiteRoot "api") -Recurse -Force
Copy-Item (Join-Path $frontPublish "*") (Join-Path $SiteRoot "site") -Recurse -Force

Write-Host "Done. Configure IIS sites/apps to:"
Write-Host "  Site files: $SiteRoot\site"
Write-Host "  API files:  $SiteRoot\api"
Write-Host "  API URL:    $ApiUrl"
