param(
  [string]$PackageRoot = "",
  [string]$BaseDirectory = "$env:ProgramData\NexumAltivon_API_24H",
  [string]$Url = "http://127.0.0.1:5010",
  [int]$CheckSeconds = 20
)

$ErrorActionPreference = "Stop"

$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal]::new($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
  throw "Execute este reparo no servidor 192.168.1.72 como Administrador."
}

$scriptPath = if ($PSCommandPath) { $PSCommandPath } elseif ($MyInvocation.MyCommand.Path) { $MyInvocation.MyCommand.Path } else { $null }
$scriptDirectory = if ($scriptPath) { Split-Path -Parent $scriptPath } else { (Get-Location).Path }
if (-not $PackageRoot) {
  $PackageRoot = Split-Path -Parent (Split-Path -Parent $scriptDirectory)
}
$PackageRoot = [System.IO.Path]::GetFullPath($PackageRoot)

$packageApiDirectory = Join-Path $PackageRoot "api"
$installer = Join-Path $scriptDirectory "03-instalar-api-24h-pacote.ps1"
$tunnelInstaller = Join-Path $scriptDirectory "07-instalar-tunel-servidor.ps1"
$repairLogDirectory = Join-Path $BaseDirectory "logs"
$repairLog = Join-Path $repairLogDirectory "reparo-conexoes-servidor.log"

New-Item -ItemType Directory -Force -Path $repairLogDirectory | Out-Null

function Write-Step {
  param([string]$Message)
  $line = "[$(Get-Date -Format s)] $Message"
  Write-Host $line
  Add-Content -Path $repairLog -Value $line
}

function Assert-Path {
  param([string]$Path, [string]$Message)
  if (-not (Test-Path $Path)) {
    throw "$Message Caminho: $Path"
  }
}

function Test-Http {
  param([string]$Uri, [int]$TimeoutSec = 10)
  try {
    $response = Invoke-WebRequest -UseBasicParsing -Uri $Uri -TimeoutSec $TimeoutSec
    return [pscustomobject]@{
      Ok = $response.StatusCode -ge 200 -and $response.StatusCode -lt 300
      StatusCode = $response.StatusCode
      Content = $response.Content
      Error = $null
    }
  } catch {
    return [pscustomobject]@{
      Ok = $false
      StatusCode = 0
      Content = ""
      Error = $_.Exception.Message
    }
  }
}

Write-Step "Iniciando reparo SERVIDOR <> API <> BACKEND <> FRONTEND."
Write-Step "PackageRoot: $PackageRoot"
Write-Step "BaseDirectory: $BaseDirectory"

Assert-Path $installer "Instalador de pacote nao encontrado."

if (-not (Test-Path (Join-Path $packageApiDirectory "NexumAltivon.API.dll"))) {
  $projectPath = Join-Path $PackageRoot "NexumAltivon_Back-End\NexumAltivon.API.csproj"
  Assert-Path $projectPath "Publicacao da API nao encontrada e projeto fonte tambem nao foi localizado."

  $packageApiDirectory = Join-Path $BaseDirectory "package-api-source"
  New-Item -ItemType Directory -Force -Path $packageApiDirectory | Out-Null
  Write-Step "Publicacao da API nao encontrada no pacote. Publicando a partir do projeto fonte."

  dotnet publish $projectPath --configuration Release --output $packageApiDirectory -p:UseAppHost=false
  if ($LASTEXITCODE -ne 0 -or -not (Test-Path (Join-Path $packageApiDirectory "NexumAltivon.API.dll"))) {
    throw "Falha ao publicar a API a partir do projeto fonte. Verifique se o .NET SDK esta instalado no servidor."
  }
}

Write-Step "Instalando/atualizando API 24h no ProgramData do servidor."
& $installer -PackageApiDirectory $packageApiDirectory -BaseDirectory $BaseDirectory -Url $Url -CheckSeconds $CheckSeconds

$configPath = Join-Path $BaseDirectory "config\api.env.ps1"
Assert-Path $configPath "Configuracao privada da API nao encontrada."

$configText = Get-Content $configPath -Raw
if ($configText -match 'ASPNETCORE_URLS') {
  $configText = $configText -replace '\$env:ASPNETCORE_URLS\s*=\s*"[^"]*"', '$env:ASPNETCORE_URLS = "http://0.0.0.0:5010"'
} else {
  $configText += "`r`n`$env:ASPNETCORE_URLS = `"http://0.0.0.0:5010`"`r`n"
}
Set-Content -Path $configPath -Value $configText -Encoding UTF8
Write-Step "ASPNETCORE_URLS fixado em http://0.0.0.0:5010 para aceitar conexao local e rede."

if ($configText -match 'COLOQUE_A_SENHA_REAL_AQUI|CHANGE_ME|PREENCHER_NO_SERVIDOR') {
  Write-Step "Configuracao privada ainda esta com placeholder em $configPath"
  throw "Preencha a senha real do banco em $configPath antes de reiniciar a API. Sem isso checkout, pedidos e consultas retornam erro 500."
}

try {
  New-NetFirewallRule -DisplayName "Nexum Altivon API 5010" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 5010 -ErrorAction SilentlyContinue | Out-Null
  Write-Step "Firewall liberado para TCP 5010."
} catch {
  Write-Step "Aviso: nao foi possivel criar regra de firewall automaticamente: $($_.Exception.Message)"
}

Write-Step "Reiniciando tarefa NexumAltivonApi24h."
Stop-ScheduledTask -TaskName "NexumAltivonApi24h" -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

$orphanProcesses = Get-CimInstance Win32_Process -ErrorAction SilentlyContinue | Where-Object {
  ($_.Name -eq "NexumAltivon.API.exe" -and $_.ExecutablePath -like "$BaseDirectory*") -or
  ($_.Name -match "dotnet" -and $_.CommandLine -like "*NexumAltivon.API.dll*")
}
foreach ($process in $orphanProcesses) {
  Write-Step "Encerrando processo antigo da API: PID $($process.ProcessId)"
  Stop-Process -Id $process.ProcessId -Force -ErrorAction SilentlyContinue
}
Remove-Item (Join-Path $BaseDirectory "runtime\api.pid") -Force -ErrorAction SilentlyContinue

Start-ScheduledTask -TaskName "NexumAltivonApi24h"

$health = $null
for ($attempt = 1; $attempt -le 40; $attempt++) {
  Start-Sleep -Seconds 2
  $health = Test-Http "$Url/health" 6
  if ($health.Ok) { break }
  Write-Step "Aguardando API ficar saudavel. Tentativa $attempt/40: $($health.Error)"
}

if (-not $health.Ok) {
  Write-Step "FALHA: API nao respondeu em $Url/health. Ultimo erro: $($health.Error)"

  $apiErrorLog = Join-Path $BaseDirectory "logs\api.err.log"
  $apiOutLog = Join-Path $BaseDirectory "logs\api.log"
  $guardianLog = Join-Path $BaseDirectory "logs\api-guardian.log"
  foreach ($logPath in @($apiErrorLog, $apiOutLog, $guardianLog)) {
    if (Test-Path $logPath) {
      Write-Step "Ultimas linhas de $logPath"
      Get-Content $logPath -Tail 80 | ForEach-Object { Write-Host $_ }
    } else {
      Write-Step "Log nao encontrado: $logPath"
    }
  }

  throw "API nao abriu a porta 5010. O erro real esta nos logs exibidos acima."
}

$dbHealth = Test-Http "$Url/health/db" 10
if (-not $dbHealth.Ok) {
  Write-Step "API abriu, mas banco falhou em $Url/health/db: $($dbHealth.Error)"
  throw "API esta no ar, mas a conexao com banco falhou. Corrija ConnectionStrings__DefaultConnection em $BaseDirectory\config\api.env.ps1"
}

Write-Step "API saudavel em $Url/health/db: $($dbHealth.Content)"

$task = Get-ScheduledTask -TaskName "NexumAltivonApi24h" -ErrorAction Stop
Write-Step "Tarefa NexumAltivonApi24h: Estado=$($task.State) Usuario=$($task.Principal.UserId)"

$cloudflared = Join-Path $BaseDirectory "cloudflared\cloudflared.exe"
if ((Test-Path $cloudflared) -and (Test-Path $tunnelInstaller)) {
  Write-Step "Instalando/reiniciando guardiao do tunel temporario."
  & $tunnelInstaller -BaseDirectory $BaseDirectory
  $tunnelTask = Get-ScheduledTask -TaskName "NexumAltivonTunnel24h" -ErrorAction Stop
  Write-Step "Tarefa NexumAltivonTunnel24h: Estado=$($tunnelTask.State) Usuario=$($tunnelTask.Principal.UserId)"
} else {
  Write-Step "Tunel temporario nao instalado agora. Motivo: cloudflared.exe nao encontrado em $cloudflared."
}

Write-Step "Reparo concluido. Servidor e unica dependencia operacional; esta maquina nao participa do boot."
