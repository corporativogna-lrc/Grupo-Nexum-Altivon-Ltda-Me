#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5
#

[CmdletBinding()]
param(
    [string]$XamppRoot = "D:\xampp",
    [string]$ServiceName = "NexumAltivonMySQL",
    [int]$DatabasePort = 3309,
    [int]$StartupTimeoutSeconds = 60,
    [switch]$StartIfStopped
)

$ErrorActionPreference = "Stop"

function Test-PathItem {
    param(
        [string]$Label,
        [string]$Path,
        [string]$Type = "Any"
    )

    $exists = if ($Type -eq "Container") {
        Test-Path -LiteralPath $Path -PathType Container
    }
    elseif ($Type -eq "Leaf") {
        Test-Path -LiteralPath $Path -PathType Leaf
    }
    else {
        Test-Path -LiteralPath $Path
    }

    [pscustomobject]@{
        Item = $Label
        Status = if ($exists) { 1 } else { 0 }
        Resultado = if ($exists) { "OK" } else { "FALHA" }
        Detalhe = $Path
    }
}

function Test-DatabasePort {
    param([int]$Port)

    $listener = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
    [pscustomobject]@{
        Item = "Porta MySQL/MariaDB $Port"
        Status = if ($listener) { 1 } else { 0 }
        Resultado = if ($listener) { "OK" } else { "FALHA" }
        Detalhe = if ($listener) { "PID $($listener.OwningProcess -join ',')" } else { "Nenhum listener em $Port." }
    }
}

function Wait-ServiceAndPort {
    param(
        [string]$Name,
        [int]$Port,
        [int]$TimeoutSeconds
    )

    $deadline = [DateTimeOffset]::UtcNow.AddSeconds($TimeoutSeconds)
    $lastState = ""

    while ([DateTimeOffset]::UtcNow -lt $deadline) {
        $service = Get-Service -Name $Name -ErrorAction SilentlyContinue
        $listener = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue

        if ($service -and $service.Status -eq "Running" -and $listener) {
            return
        }

        $lastState = if ($service) { $service.Status.ToString() } else { "Servico ausente" }
        Start-Sleep -Seconds 2
    }

    throw "Banco nao ficou operacional na porta $Port dentro de $TimeoutSeconds segundos. Ultimo estado do servico ${Name}: $lastState."
}

$mysqlRoot = Join-Path $XamppRoot "mysql"
$mysqlBin = Join-Path $mysqlRoot "bin"
$mysqldPath = Join-Path $mysqlBin "mysqld.exe"
$mysqlAdminPath = Join-Path $mysqlBin "mysqladmin.exe"
$myIniPath = Join-Path $mysqlBin "my.ini"
$nexumDataDir = Join-Path $mysqlRoot "data\nexum_altivon"
$genesisDataDir = Join-Path $mysqlRoot "data\genesis_bd"

$results = @()
$results += Test-PathItem -Label "XAMPP root" -Path $XamppRoot -Type Container
$results += Test-PathItem -Label "MySQL root" -Path $mysqlRoot -Type Container
$results += Test-PathItem -Label "mysqld.exe" -Path $mysqldPath -Type Leaf
$results += Test-PathItem -Label "mysqladmin.exe" -Path $mysqlAdminPath -Type Leaf
$results += Test-PathItem -Label "my.ini" -Path $myIniPath -Type Leaf
$results += Test-PathItem -Label "Data dir nexum_altivon" -Path $nexumDataDir -Type Container
$results += Test-PathItem -Label "Data dir genesis_bd" -Path $genesisDataDir -Type Container

$missing = @($results | Where-Object { $_.Resultado -ne "OK" })
if ($missing.Count -gt 0) {
    $results | Format-Table -AutoSize
    throw "Estrutura XAMPP/MySQL oficial incompleta. Corrija os itens em FALHA antes de iniciar o banco."
}

$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
$results += [pscustomobject]@{
    Item = "Servico Windows $ServiceName"
    Status = if ($service) { 1 } else { 0 }
    Resultado = if ($service) { "OK" } else { "FALHA" }
    Detalhe = if ($service) { "Status $($service.Status), StartType $($service.StartType)" } else { "Servico ausente." }
}

if (-not $service) {
    $results | Format-Table -AutoSize
    throw "Servico Windows $ServiceName nao encontrado. Registre o MySQL/MariaDB do XAMPP como serviço antes da operação 24h."
}

if ($service.Status -ne "Running" -and $StartIfStopped) {
    Start-Service -Name $ServiceName
    Wait-ServiceAndPort -Name $ServiceName -Port $DatabasePort -TimeoutSeconds $StartupTimeoutSeconds
    $service = Get-Service -Name $ServiceName
}

$results += [pscustomobject]@{
    Item = "Estado do servico $ServiceName"
    Status = if ($service.Status -eq "Running") { 1 } else { 0 }
    Resultado = if ($service.Status -eq "Running") { "OK" } else { "FALHA" }
    Detalhe = "Status $($service.Status)"
}

$results += Test-DatabasePort -Port $DatabasePort
$results | Format-Table -AutoSize

$failed = @($results | Where-Object { $_.Resultado -ne "OK" })
if ($failed.Count -gt 0) {
    throw "Banco XAMPP/MySQL nao esta operacional em $DatabasePort. Falhas: $($failed.Count)."
}

Write-Host "Banco XAMPP/MySQL verificado com sucesso na porta $DatabasePort."
