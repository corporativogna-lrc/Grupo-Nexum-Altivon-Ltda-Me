param(
  [string]$TargetDirectory = "\\192.168.1.72\Servidor_NexumAltivon\NexumAltivon_API_24H_Y_FIX",
  [string]$SourceRoot = ""
)

$ErrorActionPreference = "Stop"

$ScriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $SourceRoot) {
  $SourceRoot = Split-Path -Parent (Split-Path -Parent $ScriptDirectory)
}

$ProjectPath = Join-Path $SourceRoot "NexumAltivon_Back-End\NexumAltivon.API.csproj"
$ApiPackageDirectory = Join-Path $TargetDirectory "api"
$ScriptPackageDirectory = Join-Path $TargetDirectory "scripts"

if (-not (Test-Path $ProjectPath)) {
  throw "Projeto da API não encontrado: $ProjectPath"
}

if (Test-Path $TargetDirectory) {
  Remove-Item $TargetDirectory -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $ApiPackageDirectory, $ScriptPackageDirectory | Out-Null

$BuildBase = Join-Path $env:TEMP ("nexum-api-publish-" + [guid]::NewGuid().ToString("N"))
try {
  dotnet publish $ProjectPath --configuration Release --runtime win-x64 --self-contained true --output $ApiPackageDirectory -p:UseAppHost=true -p:BaseOutputPath="$BuildBase\bin\" -p:BaseIntermediateOutputPath="$BuildBase\obj\"
  if ($LASTEXITCODE -ne 0) {
    throw "Falha ao publicar a API para o pacote: $ApiPackageDirectory"
  }
} finally {
  if (Test-Path $BuildBase) {
    Remove-Item $BuildBase -Recurse -Force -ErrorAction SilentlyContinue
  }
}

Copy-Item (Join-Path $ScriptDirectory "99-api.env.example.ps1") $ScriptPackageDirectory -Force
Copy-Item (Join-Path $ScriptDirectory "03-instalar-api-24h-pacote.ps1") $ScriptPackageDirectory -Force
Copy-Item (Join-Path $ScriptDirectory "04-iniciar-api-24h.ps1") $ScriptPackageDirectory -Force
Copy-Item (Join-Path $ScriptDirectory "05-verificar-api-24h.ps1") $ScriptPackageDirectory -Force
Copy-Item (Join-Path $ScriptDirectory "06-tunel-publico-servidor.ps1") $ScriptPackageDirectory -Force
Copy-Item (Join-Path $ScriptDirectory "07-instalar-tunel-servidor.ps1") $ScriptPackageDirectory -Force
Copy-Item (Join-Path $ScriptDirectory "11-aplicar-atualizacao-api.ps1") $ScriptPackageDirectory -Force
Copy-Item (Join-Path $ScriptDirectory "13-reparar-guardiao-api.ps1") $ScriptPackageDirectory -Force
Copy-Item (Join-Path $ScriptDirectory "14-instalar-agentes-conexao-servidor.ps1") $ScriptPackageDirectory -Force
Copy-Item (Join-Path $ScriptDirectory "15-reparar-conexoes-servidor.ps1") $ScriptPackageDirectory -Force
Copy-Item (Join-Path $ScriptDirectory "REPARAR-CONEXOES-SERVIDOR-COMO-ADMIN.cmd") $ScriptPackageDirectory -Force
Copy-Item (Join-Path $ScriptDirectory "03-instalar-api-24h-pacote.cmd") $TargetDirectory -Force
Copy-Item (Join-Path $ScriptDirectory "APLICAR-ATUALIZACAO-API.cmd") $TargetDirectory -Force
Copy-Item (Join-Path $ScriptDirectory "INSTALAR-TUNEL-NO-SERVIDOR.cmd") $TargetDirectory -Force
Copy-Item (Join-Path $ScriptDirectory "14-INSTALAR-AGENTES-CONEXAO-SERVIDOR-COMO-ADMIN.cmd") $TargetDirectory -Force
Copy-Item (Join-Path $ScriptDirectory "REPARAR-CONEXOES-SERVIDOR-COMO-ADMIN.cmd") $TargetDirectory -Force
Copy-Item (Join-Path $SourceRoot "API_24H_SERVIDOR.md") $TargetDirectory -Force

Write-Host "Pacote da API 24h criado em:"
Write-Host $TargetDirectory
Write-Host ""
Write-Host "No servidor, execute como Administrador:"
Write-Host (Join-Path $TargetDirectory "03-instalar-api-24h-pacote.cmd")
