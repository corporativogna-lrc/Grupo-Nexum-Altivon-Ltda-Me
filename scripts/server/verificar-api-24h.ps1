#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5
#

[CmdletBinding()]
param(
    [string]$TaskName = "NexumAltivonApi24h",
    [int]$Port = 5010,
    [int]$CloudflareOriginPort = 5010,
    [string]$XamppRoot = "D:\xampp",
    [string]$DatabaseServiceName = "NexumAltivonMySQL",
    [int]$DatabasePort = 3309,
    [string[]]$DatabaseDataDirs = @(
        "D:\xampp\mysql\data\nexum_altivon",
        "D:\xampp\mysql\data\genesis_bd"
    ),
    [string[]]$PublicHealthUrls = @(
        "https://api.nexumaltivon.com.br/health",
        "https://api.nexumaltivon.com/health"
    ),
    [int]$TimeoutSec = 15
)

$ErrorActionPreference = "Stop"

function Test-HttpEndpoint {
    param([string]$Url)

    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec $TimeoutSec
        return [pscustomobject]@{
            Item = $Url
            Status = [int]$response.StatusCode
            Resultado = if ([int]$response.StatusCode -ge 200 -and [int]$response.StatusCode -lt 300) { "OK" } else { "FALHA" }
            Detalhe = ""
        }
    }
    catch {
        $statusCode = 0
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
            $statusCode = [int]$_.Exception.Response.StatusCode
        }

        return [pscustomobject]@{
            Item = $Url
            Status = $statusCode
            Resultado = "FALHA"
            Detalhe = $_.Exception.Message
        }
    }
}

$results = @()

$scriptDir = Split-Path -Parent $PSCommandPath
$mysqlVerifier = Join-Path $scriptDir "verificar-banco-xampp.ps1"
if (Test-Path -LiteralPath $mysqlVerifier) {
    try {
        & $mysqlVerifier -XamppRoot $XamppRoot -ServiceName $DatabaseServiceName -DatabasePort $DatabasePort | Out-Host
    }
    catch {
        $results += [pscustomobject]@{
            Item = "Verificacao banco XAMPP"
            Status = 0
            Resultado = "FALHA"
            Detalhe = $_.Exception.Message
        }
    }
}

foreach ($databaseDataDir in $DatabaseDataDirs) {
    $exists = Test-Path -LiteralPath $databaseDataDir -PathType Container
    $results += [pscustomobject]@{
        Item = "Diretorio banco $databaseDataDir"
        Status = if ($exists) { 1 } else { 0 }
        Resultado = if ($exists) { "OK" } else { "FALHA" }
        Detalhe = if ($exists) { "Diretorio encontrado." } else { "Diretorio oficial ausente." }
    }
}

$databaseListener = Get-NetTCPConnection -LocalPort $DatabasePort -State Listen -ErrorAction SilentlyContinue
$results += [pscustomobject]@{
    Item = "Porta MySQL/MariaDB $DatabasePort"
    Status = if ($databaseListener) { 1 } else { 0 }
    Resultado = if ($databaseListener) { "OK" } else { "FALHA" }
    Detalhe = if ($databaseListener) { "PID $($databaseListener.OwningProcess -join ',')" } else { "Nenhum listener em $DatabasePort." }
}

$task = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
$results += [pscustomobject]@{
    Item = "Tarefa agendada $TaskName"
    Status = if ($task) { 1 } else { 0 }
    Resultado = if ($task) { "OK" } else { "FALHA" }
    Detalhe = if ($task) { $task.State.ToString() } else { "Tarefa inexistente." }
}

$listener = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
$results += [pscustomobject]@{
    Item = "Porta local $Port"
    Status = if ($listener) { 1 } else { 0 }
    Resultado = if ($listener) { "OK" } else { "FALHA" }
    Detalhe = if ($listener) { "PID $($listener.OwningProcess -join ',')" } else { "Nenhum listener em $Port." }
}

$results += Test-HttpEndpoint -Url "http://127.0.0.1:$Port/health"

if ($CloudflareOriginPort -gt 0 -and $CloudflareOriginPort -ne $Port) {
    $cloudflareOriginListener = Get-NetTCPConnection -LocalPort $CloudflareOriginPort -State Listen -ErrorAction SilentlyContinue
    $results += [pscustomobject]@{
        Item = "Porta local Cloudflare $CloudflareOriginPort"
        Status = if ($cloudflareOriginListener) { 1 } else { 0 }
        Resultado = if ($cloudflareOriginListener) { "OK" } else { "FALHA" }
        Detalhe = if ($cloudflareOriginListener) { "PID $($cloudflareOriginListener.OwningProcess -join ',')" } else { "Nenhum listener em $CloudflareOriginPort." }
    }

    $results += Test-HttpEndpoint -Url "http://127.0.0.1:$CloudflareOriginPort/health"
}

foreach ($publicUrl in $PublicHealthUrls) {
    $results += Test-HttpEndpoint -Url $publicUrl
}

$results | Format-Table -AutoSize

$failed = @($results | Where-Object { $_.Resultado -ne "OK" })
if ($failed.Count -gt 0) {
    throw "Verificacao da API 24h falhou em $($failed.Count) item(ns)."
}

Write-Host "API 24h verificada com sucesso."
