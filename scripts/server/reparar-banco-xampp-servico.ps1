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
    [int]$StartupTimeoutSeconds = 90,
    [switch]$ForceRecreateService
)

$ErrorActionPreference = "Stop"

function Assert-Administrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Execute este reparador em PowerShell/CMD como Administrador."
    }
}

function Invoke-Sc {
    param([string[]]$Arguments)

    $output = & sc.exe @Arguments 2>&1
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        throw "sc.exe $($Arguments -join ' ') falhou com codigo $exitCode. Saida: $($output -join ' ')"
    }

    return $output
}

function Wait-ServiceRemoved {
    param(
        [string]$Name,
        [int]$TimeoutSeconds
    )

    $deadline = [DateTimeOffset]::UtcNow.AddSeconds($TimeoutSeconds)
    while ([DateTimeOffset]::UtcNow -lt $deadline) {
        $service = Get-Service -Name $Name -ErrorAction SilentlyContinue
        if (-not $service) {
            return
        }

        Start-Sleep -Seconds 2
    }

    throw "Servico $Name nao foi removido dentro de $TimeoutSeconds segundos. Pode ser necessario reiniciar o Windows para concluir a remocao."
}

function Wait-DatabasePort {
    param(
        [int]$Port,
        [int]$TimeoutSeconds
    )

    $deadline = [DateTimeOffset]::UtcNow.AddSeconds($TimeoutSeconds)
    while ([DateTimeOffset]::UtcNow -lt $deadline) {
        $listener = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
        $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
        if ($listener -and $service -and $service.Status -eq "Running") {
            return
        }

        Start-Sleep -Seconds 2
    }

    throw "MySQL/MariaDB nao abriu a porta $Port dentro de $TimeoutSeconds segundos."
}

function Convert-ToMySqlPath {
    param([string]$Path)
    return $Path.TrimEnd("\").Replace("\", "/")
}

function Resolve-LocalXamppRoot {
    param([string]$Root)

    if ($Root -notmatch '^\\\\([^\\]+)\\([^\\]+)(\\.*)?$') {
        return $Root
    }

    $serverName = $Matches[1]
    $shareName = $Matches[2]
    $relativePath = $Matches[3]
    if (-not $relativePath) {
        $relativePath = ""
    }

    $localServerNames = @(
        $env:COMPUTERNAME,
        "$env:COMPUTERNAME.$env:USERDNSDOMAIN",
        "localhost",
        "127.0.0.1",
        $env:COMPUTERNAME.ToLowerInvariant()
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    if ($localServerNames -notcontains $serverName -and $serverName -ne "RODRIGOCOSTA") {
        return $Root
    }

    $share = Get-SmbShare -Name $shareName -ErrorAction SilentlyContinue
    if (-not $share -or [string]::IsNullOrWhiteSpace($share.Path)) {
        return $Root
    }

    $localRoot = $share.Path.TrimEnd("\")
    $relativeWithoutSlash = $relativePath.TrimStart("\")
    if ([string]::IsNullOrWhiteSpace($relativeWithoutSlash)) {
        return $localRoot
    }

    return (Join-Path $localRoot $relativeWithoutSlash)
}

Assert-Administrator

$serviceXamppRoot = Resolve-LocalXamppRoot -Root $XamppRoot

$mysqlRoot = Join-Path $serviceXamppRoot "mysql"
$mysqlBin = Join-Path $mysqlRoot "bin"
$mysqldPath = Join-Path $mysqlBin "mysqld.exe"
$originalIniPath = Join-Path $mysqlBin "my.ini"
$serviceIniPath = $originalIniPath
$dataDir = Join-Path $mysqlRoot "data"
$nexumDataDir = Join-Path $dataDir "nexum_altivon"
$genesisDataDir = Join-Path $dataDir "genesis_bd"

foreach ($requiredPath in @($serviceXamppRoot, $mysqlRoot, $mysqlBin, $mysqldPath, $originalIniPath, $dataDir, $nexumDataDir, $genesisDataDir)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Caminho obrigatorio ausente: $requiredPath"
    }
}

$currentService = Get-CimInstance Win32_Service -Filter "Name='$ServiceName'" -ErrorAction SilentlyContinue
$currentListener = Get-NetTCPConnection -LocalPort $DatabasePort -State Listen -ErrorAction SilentlyContinue

if ($currentService -and $currentService.State -eq "Running" -and $currentListener -and -not $ForceRecreateService) {
    Write-Host "Servico $ServiceName ja esta operacional em $DatabasePort."
    exit 0
}

if ($currentService) {
    if ($currentService.ProcessId -and $currentService.ProcessId -gt 0) {
        Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 3
    }

    Invoke-Sc -Arguments @("delete", $ServiceName) | Out-Null
    Wait-ServiceRemoved -Name $ServiceName -TimeoutSeconds 30
}

$binaryPath = "`"$mysqldPath`" --defaults-file=`"$serviceIniPath`" $ServiceName"
Invoke-Sc -Arguments @("create", $ServiceName, "binPath=", $binaryPath, "start=", "auto", "DisplayName=", "mysql") | Out-Null
Invoke-Sc -Arguments @("failure", $ServiceName, "reset=", "60", "actions=", "restart/60000/restart/60000/`"`"/60000") | Out-Null

Start-Service -Name $ServiceName
Wait-DatabasePort -Port $DatabasePort -TimeoutSeconds $StartupTimeoutSeconds

Write-Host "Servico $ServiceName reparado e operacional na porta $DatabasePort."
Write-Host "Configuracao de servico: $serviceIniPath"
