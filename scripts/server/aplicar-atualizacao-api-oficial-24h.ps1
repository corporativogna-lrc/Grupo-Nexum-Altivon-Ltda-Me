#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5
#

[CmdletBinding()]
param(
    [string]$ProjectRoot = "D:\Nexum Altivon\NexumAltivon.com",
    [string]$TaskName = "NexumAltivonApi24h",
    [string]$ApiUrl = "http://127.0.0.1:5010",
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"

$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal]::new($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Execute este script em PowerShell elevado. A atualizacao precisa parar e iniciar a tarefa SYSTEM."
}

$resolvedProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$apiProject = Join-Path $resolvedProjectRoot "NexumAltivon_Back-End\NexumAltivon.API.csproj"
$publishDir = Join-Path $resolvedProjectRoot "runtime\api-24h\api"
$installTaskScript = Join-Path $resolvedProjectRoot "scripts\server\instalar-api-oficial-24h-task.ps1"
$privateConfig = Join-Path $resolvedProjectRoot "runtime\api-24h\api.env.ps1"
$runtimeLogDir = Join-Path $resolvedProjectRoot "runtime-logs"
$logPath = Join-Path $runtimeLogDir "api-24h-update.log"
New-Item -ItemType Directory -Path $runtimeLogDir -Force | Out-Null

if (-not (Test-Path -LiteralPath $apiProject -PathType Leaf)) {
    throw "Projeto da API nao encontrado em $apiProject."
}

if (-not (Test-Path -LiteralPath $installTaskScript -PathType Leaf)) {
    throw "Instalador da tarefa oficial nao encontrado em $installTaskScript."
}

if (-not (Test-Path -LiteralPath $privateConfig -PathType Leaf)) {
    throw "Configuracao privada oficial nao encontrada em $privateConfig."
}

$events = [System.Collections.Generic.List[string]]::new()
$events.Add("$(Get-Date -Format s) Inicio da atualizacao oficial da API.")

function Save-UpdateLog {
    $events | Set-Content -LiteralPath $logPath -Encoding UTF8
}

Save-UpdateLog

$task = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
if ($task) {
    Stop-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
    $events.Add("$(Get-Date -Format s) Tarefa $TaskName solicitada para parada.")
    Save-UpdateLog
}

$apiProcesses = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
    Where-Object { $_.CommandLine -like "*NexumAltivon.API.dll*" }

foreach ($process in $apiProcesses) {
    Stop-Process -Id $process.ProcessId -Force -ErrorAction Stop
    $events.Add("$(Get-Date -Format s) Processo API encerrado: $($process.ProcessId).")
    Save-UpdateLog
}

$wrappers = Get-CimInstance Win32_Process -Filter "Name = 'powershell.exe'" |
    Where-Object { $_.ProcessId -ne $PID -and $_.CommandLine -like "*iniciar-api-oficial-24h.ps1*" }

foreach ($process in $wrappers) {
    Stop-Process -Id $process.ProcessId -Force -ErrorAction SilentlyContinue
    $events.Add("$(Get-Date -Format s) Wrapper antigo encerrado: $($process.ProcessId).")
    Save-UpdateLog
}

for ($i = 0; $i -lt 30; $i++) {
    $busyPort = Get-NetTCPConnection -LocalPort 5010 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($null -eq $busyPort) {
        break
    }

    Start-Sleep -Seconds 1
}

$publishArgs = @("publish", $apiProject, "-c", "Release", "-o", $publishDir, "--nologo")
if ($NoBuild) {
    $publishArgs += "--no-build"
}

$publishOutput = & dotnet @publishArgs 2>&1
if ($LASTEXITCODE -ne 0) {
    $events.AddRange($publishOutput)
    Save-UpdateLog
    throw "dotnet publish falhou. Consulte $logPath."
}

$events.AddRange($publishOutput)
$events.Add("$(Get-Date -Format s) Publicacao concluida em $publishDir.")
Save-UpdateLog

try {
    & $installTaskScript -ProjectRoot $resolvedProjectRoot -TaskName $TaskName -ApiUrl $ApiUrl | Out-String | ForEach-Object { $events.Add($_) }
    Save-UpdateLog
}
catch {
    $events.Add("$(Get-Date -Format s) Falha ao reinstalar/iniciar tarefa $($TaskName): $($_.Exception.Message)")
    Save-UpdateLog
    throw
}

try {
    $health = Invoke-WebRequest -UseBasicParsing -Uri "$ApiUrl/health" -TimeoutSec 20
    if ($health.StatusCode -ne 200) {
        throw "Health-check local falhou apos atualizacao: HTTP $($health.StatusCode)."
    }
}
catch {
    $events.Add("$(Get-Date -Format s) Falha no health-check local: $($_.Exception.Message)")
    Save-UpdateLog
    throw
}

$events.Add("$(Get-Date -Format s) Health-check local OK.")
Save-UpdateLog

[pscustomobject]@{
    UpdatedAt = (Get-Date).ToString("s")
    TaskName = $TaskName
    ApiUrl = $ApiUrl
    PublishDir = $publishDir
    HealthStatus = $health.StatusCode
    LogPath = $logPath
} | Format-List
