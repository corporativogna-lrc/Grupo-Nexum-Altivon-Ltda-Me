#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5
#

[CmdletBinding()]
param(
    [string]$InstallRoot = "D:\NexumAltivon_API_24H",
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
    [int]$StartupTimeoutSeconds = 90,
    [switch]$RunAsSystem
)

$ErrorActionPreference = "Stop"

function Assert-Administrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Execute este instalador em PowerShell/CMD como Administrador."
    }
}

function Assert-ConfiguredSecret {
    param(
        [string]$Name,
        [string]$Value,
        [int]$MinimumBytes = 1
    )

    if ([string]::IsNullOrWhiteSpace($Value) -or
        $Value -match "CHANGE_ME" -or
        $Value -match "USE_ENV" -or
        $Value -eq "null" -or
        [Text.Encoding]::UTF8.GetByteCount($Value) -lt $MinimumBytes) {
        throw "$Name nao esta configurado com valor real no ambiente privado do servidor."
    }
}

function Resolve-ConfiguredJwtSecret {
    $candidates = @(
        $env:JwtSettings__SecretKey,
        $env:JWT_SECRET_KEY
    )

    foreach ($candidate in $candidates) {
        if (-not [string]::IsNullOrWhiteSpace($candidate) -and
            $candidate -notmatch "CHANGE_ME" -and
            $candidate -notmatch "USE_ENV" -and
            $candidate -ne "null" -and
            [Text.Encoding]::UTF8.GetByteCount($candidate) -ge 32) {
            return $candidate
        }
    }

    throw "JwtSettings__SecretKey ou JWT_SECRET_KEY nao esta configurada com valor real de ao menos 32 bytes no ambiente privado do servidor."
}

function Wait-HttpHealthy {
    param(
        [string]$Url,
        [int]$TimeoutSeconds
    )

    $deadline = [DateTimeOffset]::UtcNow.AddSeconds($TimeoutSeconds)
    $lastError = $null

    while ([DateTimeOffset]::UtcNow -lt $deadline) {
        try {
            $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 5
            if ([int]$response.StatusCode -ge 200 -and [int]$response.StatusCode -lt 300) {
                return
            }
            $lastError = "HTTP $($response.StatusCode)"
        }
        catch {
            $lastError = $_.Exception.Message
        }

        Start-Sleep -Seconds 2
    }

    throw "A API nao respondeu saudavel em $Url dentro de $TimeoutSeconds segundos. Ultimo erro: $lastError"
}

function Clear-EncryptedAttributes {
    param([string]$Path)

    if (-not $IsWindows -and $PSVersionTable.PSEdition -eq "Core") {
        return
    }

    $rootItem = Get-Item -LiteralPath $Path -Force
    $items = @()
    if ($rootItem.PSIsContainer) {
        $items += Get-ChildItem -LiteralPath $Path -Recurse -Force -File
    }
    else {
        $items += $rootItem
    }

    foreach ($item in $items) {
        if (($item.Attributes -band [IO.FileAttributes]::Encrypted) -eq [IO.FileAttributes]::Encrypted) {
            [IO.File]::Decrypt($item.FullName)
            $item.Refresh()
            if (($item.Attributes -band [IO.FileAttributes]::Encrypted) -eq [IO.FileAttributes]::Encrypted) {
                throw "Arquivo publicado permaneceu criptografado por EFS: $($item.FullName)"
            }
        }
    }
}

function Stop-ApiRuntime {
    param(
        [string]$ScheduledTaskName,
        [int[]]$Ports,
        [int]$TimeoutSeconds = 45
    )

    $task = Get-ScheduledTask -TaskName $ScheduledTaskName -ErrorAction SilentlyContinue
    if ($task -and $task.State -eq "Running") {
        Stop-ScheduledTask -TaskName $ScheduledTaskName
    }

    $deadline = [DateTimeOffset]::UtcNow.AddSeconds($TimeoutSeconds)
    do {
        $listeners = @()
        foreach ($runtimePort in ($Ports | Sort-Object -Unique)) {
            if ($runtimePort -gt 0) {
                $listeners += @(Get-NetTCPConnection -LocalPort $runtimePort -State Listen -ErrorAction SilentlyContinue)
            }
        }

        if ($listeners.Count -eq 0) {
            return
        }

        foreach ($processId in @($listeners.OwningProcess | Sort-Object -Unique)) {
            if ($processId -and $processId -ne $PID) {
                Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
            }
        }

        Start-Sleep -Seconds 1
    } while ([DateTimeOffset]::UtcNow -lt $deadline)

    $blockedPorts = @()
    foreach ($runtimePort in ($Ports | Sort-Object -Unique)) {
        if ($runtimePort -gt 0 -and (Get-NetTCPConnection -LocalPort $runtimePort -State Listen -ErrorAction SilentlyContinue)) {
            $blockedPorts += $runtimePort
        }
    }

    if ($blockedPorts.Count -gt 0) {
        throw "Nao foi possivel liberar as portas da API antes da publicacao: $($blockedPorts -join ', ')."
    }
}

Assert-Administrator

$scriptDir = Split-Path -Parent $PSCommandPath
$projectRoot = Split-Path -Parent (Split-Path -Parent $scriptDir)
$apiProject = Join-Path $projectRoot "NexumAltivon_Back-End\NexumAltivon.API.csproj"
$configPath = Join-Path $InstallRoot "config\api.env.ps1"
$publishDir = Join-Path $InstallRoot "api"
$binDir = Join-Path $InstallRoot "bin"
$logDir = Join-Path $InstallRoot "logs"
$launcherPath = Join-Path $binDir "iniciar-api-24h.ps1"
$apiDllPath = Join-Path $publishDir "NexumAltivon.API.dll"

if (-not (Test-Path -LiteralPath $apiProject)) {
    throw "Projeto oficial da API nao encontrado em $apiProject."
}

if (-not (Test-Path -LiteralPath $configPath)) {
    throw "Arquivo privado de configuracao nao encontrado em $configPath. Crie este arquivo no servidor com variaveis reais antes da instalacao."
}

foreach ($databaseDataDir in $DatabaseDataDirs) {
    if (-not (Test-Path -LiteralPath $databaseDataDir -PathType Container)) {
        throw "Diretorio oficial de dados do banco nao encontrado: $databaseDataDir."
    }
}

$mysqlVerifier = Join-Path $scriptDir "verificar-banco-xampp.ps1"
if (-not (Test-Path -LiteralPath $mysqlVerifier)) {
    throw "Verificador oficial do banco nao encontrado em $mysqlVerifier."
}

& $mysqlVerifier -XamppRoot $XamppRoot -ServiceName $DatabaseServiceName -DatabasePort $DatabasePort

$databaseListener = Get-NetTCPConnection -LocalPort $DatabasePort -State Listen -ErrorAction SilentlyContinue
if (-not $databaseListener) {
    throw "Porta oficial do MySQL/MariaDB nao esta escutando localmente: $DatabasePort."
}

. $configPath

Assert-ConfiguredSecret -Name "ConnectionStrings__DefaultConnection" -Value $env:ConnectionStrings__DefaultConnection
Assert-ConfiguredSecret -Name "ConnectionStrings__NexumDb" -Value $env:ConnectionStrings__NexumDb
Assert-ConfiguredSecret -Name "ConnectionStrings__GenesisConnection" -Value $env:ConnectionStrings__GenesisConnection
Resolve-ConfiguredJwtSecret | Out-Null
Assert-ConfiguredSecret -Name "AdminUser__Email" -Value $env:AdminUser__Email
Assert-ConfiguredSecret -Name "AdminUser__Password" -Value $env:AdminUser__Password -MinimumBytes 12

New-Item -ItemType Directory -Path $publishDir, $binDir, $logDir -Force | Out-Null

$publishPorts = @($Port)
if ($CloudflareOriginPort -gt 0 -and $CloudflareOriginPort -ne $Port) {
    $publishPorts += $CloudflareOriginPort
}

Stop-ApiRuntime -ScheduledTaskName $TaskName -Ports $publishPorts

dotnet publish $apiProject -c Release -o $publishDir --self-contained false
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish da API retornou codigo $LASTEXITCODE."
}

if (-not (Test-Path -LiteralPath $apiDllPath)) {
    throw "Publicacao da API nao gerou $apiDllPath."
}

Clear-EncryptedAttributes -Path $publishDir
New-Item -ItemType Directory -Path (Join-Path $publishDir "wwwroot") -Force | Out-Null

$urls = @("http://127.0.0.1:$Port")
if ($CloudflareOriginPort -gt 0 -and $CloudflareOriginPort -ne $Port) {
    $urls += "http://127.0.0.1:$CloudflareOriginPort"
}
$aspNetCoreUrls = $urls -join ";"

$launcher = @"
#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5
#

`$ErrorActionPreference = "Stop"
`$configPath = "$configPath"
`$publishDir = "$publishDir"
`$logDir = "$logDir"

if (-not (Test-Path -LiteralPath `$configPath)) {
    throw "Configuracao privada da API nao encontrada em `$configPath."
}

. `$configPath

`$env:ASPNETCORE_ENVIRONMENT = "Production"
`$env:ASPNETCORE_URLS = "$aspNetCoreUrls"

New-Item -ItemType Directory -Path `$logDir -Force | Out-Null
`$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
`$stdout = Join-Path `$logDir "api-`$timestamp.stdout.log"
`$stderr = Join-Path `$logDir "api-`$timestamp.stderr.log"

Set-Location -LiteralPath `$publishDir
& dotnet "NexumAltivon.API.dll" 1>> `$stdout 2>> `$stderr
exit `$LASTEXITCODE
"@

Set-Content -LiteralPath $launcherPath -Value $launcher -Encoding UTF8

$existingTask = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
if ($existingTask -and $existingTask.State -eq "Running") {
    Stop-ScheduledTask -TaskName $TaskName
    Start-Sleep -Seconds 5
}

$action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$launcherPath`""
$triggers = @(
    (New-ScheduledTaskTrigger -AtStartup),
    (New-ScheduledTaskTrigger -AtLogOn)
)

if ($RunAsSystem) {
    $principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -RunLevel Highest
}
else {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent().Name
    $principal = New-ScheduledTaskPrincipal -UserId $currentUser -LogonType Interactive -RunLevel Highest
}

$settings = New-ScheduledTaskSettingsSet `
    -AllowStartIfOnBatteries `
    -ExecutionTimeLimit (New-TimeSpan -Days 0) `
    -MultipleInstances IgnoreNew `
    -RestartCount 3 `
    -RestartInterval (New-TimeSpan -Minutes 1)

if ($settings.PSObject.Properties.Name -contains "DisallowStartIfOnBatteries") {
    $settings.DisallowStartIfOnBatteries = $false
}

$task = New-ScheduledTask -Action $action -Trigger $triggers -Principal $principal -Settings $settings
Register-ScheduledTask -TaskName $TaskName -InputObject $task -Force | Out-Null

Start-ScheduledTask -TaskName $TaskName
Wait-HttpHealthy -Url "http://127.0.0.1:$Port/health" -TimeoutSeconds $StartupTimeoutSeconds

Write-Host "API GenesisGest.Net instalada e saudavel em http://127.0.0.1:$Port/health"
Write-Host "Tarefa agendada: $TaskName"
Write-Host "Publicacao: $publishDir"
Write-Host "Logs: $logDir"
