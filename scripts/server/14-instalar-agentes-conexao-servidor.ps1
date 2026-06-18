param(
  [string]$SourceRoot = "",
  [string]$BaseDirectory = "$env:ProgramData\NexumAltivon_API_24H",
  [string]$Url = "http://127.0.0.1:5010",
  [int]$CheckSeconds = 20
)

$ErrorActionPreference = "Stop"

$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
  throw "Execute no servidor 192.168.1.72 como Administrador."
}

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $SourceRoot) {
  $SourceRoot = Split-Path -Parent (Split-Path -Parent $scriptDirectory)
}

$installer = Join-Path $scriptDirectory "01-instalar-api-24h-servidor.ps1"
if (-not (Test-Path $installer)) {
  throw "Instalador da API 24h nao encontrado: $installer"
}
$tunnelInstaller = Join-Path $scriptDirectory "07-instalar-tunel-servidor.ps1"

Write-Host "Instalando agentes de conexao no proprio servidor."
Write-Host "Base local: $BaseDirectory"
Write-Host "Banco autoridade: 192.168.1.72:3309"
Write-Host "API local supervisionada: $Url"

& $installer `
  -SourceRoot $SourceRoot `
  -BaseDirectory $BaseDirectory `
  -Url $Url `
  -CheckSeconds $CheckSeconds

$task = Get-ScheduledTask -TaskName "NexumAltivonApi24h" -ErrorAction Stop
if ($task.Principal.UserId -ne "SYSTEM") {
  throw "A tarefa NexumAltivonApi24h deve rodar como SYSTEM para iniciar com o servidor."
}

if (Test-Path $tunnelInstaller) {
  $cloudflared = Join-Path $BaseDirectory "cloudflared\cloudflared.exe"
  if (Test-Path $cloudflared) {
    & $tunnelInstaller -BaseDirectory $BaseDirectory
    $tunnelTask = Get-ScheduledTask -TaskName "NexumAltivonTunnel24h" -ErrorAction Stop
    if ($tunnelTask.Principal.UserId -ne "SYSTEM") {
      throw "A tarefa NexumAltivonTunnel24h deve rodar como SYSTEM para iniciar com o servidor."
    }
    Write-Host "Guardiao do tunel temporario instalado no boot do servidor."
  } else {
    Write-Host "cloudflared nao encontrado em $cloudflared. API 24h instalada; tunel sera instalado quando o executavel estiver nessa pasta."
  }
}

Write-Host "Agentes instalados no servidor. Esta estacao nao e dependencia de boot/conexao."
