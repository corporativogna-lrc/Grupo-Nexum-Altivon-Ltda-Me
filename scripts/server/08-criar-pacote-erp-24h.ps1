param(
  [string]$TargetDirectory = "Y:\NexumAltivon_Services\ERP",
  [string]$SourceRoot = ""
)

$ErrorActionPreference = "Stop"
$ScriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $SourceRoot) {
  $SourceRoot = Split-Path -Parent (Split-Path -Parent $ScriptDirectory)
}
$ProjectPath = Join-Path $SourceRoot "NexumAltivon.ERP.csproj"
$AppDirectory = Join-Path $TargetDirectory "app"
$ConfigDirectory = Join-Path $TargetDirectory "config"
$ScriptTarget = Join-Path $TargetDirectory "scripts"
New-Item -ItemType Directory -Force -Path $AppDirectory, $ConfigDirectory, $ScriptTarget | Out-Null

$buildBase = Join-Path $env:TEMP ("nexum-erp-publish-" + [guid]::NewGuid().ToString("N"))
try {
  dotnet publish $ProjectPath --configuration Release --runtime win-x64 --self-contained true `
    --output $AppDirectory -p:UseAppHost=true `
    -p:BaseOutputPath="$buildBase\bin\" -p:BaseIntermediateOutputPath="$buildBase\obj\"
  if ($LASTEXITCODE -ne 0) { throw "Falha ao publicar ERP." }
} finally {
  Remove-Item $buildBase -Recurse -Force -ErrorAction SilentlyContinue
}

Copy-Item (Join-Path $ScriptDirectory "09-iniciar-erp-24h.ps1") $ScriptTarget -Force
Copy-Item (Join-Path $ScriptDirectory "10-instalar-erp-24h.ps1") $ScriptTarget -Force
Copy-Item (Join-Path $ScriptDirectory "INSTALAR-ERP-NO-SERVIDOR.cmd") $TargetDirectory -Force
Write-Host "ERP publicado em $TargetDirectory"
