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
    [string]$PrivateConfigPath = "",
    [string]$ApiUrl = "http://127.0.0.1:5010",
    [ValidateRange(5, 300)][int]$RestartDelaySeconds = 10,
    [ValidateRange(60, 3600)][int]$StableRunSeconds = 300,
    [ValidateRange(30, 900)][int]$MaxRestartDelaySeconds = 300,
    [ValidateRange(5, 180)][int]$LogRetentionCount = 30
)

$ErrorActionPreference = "Stop"
$mutexName = "Global\GenesisGest_NexumAltivonApi24h"
$mutex = $null
$ownsMutex = $false

function Write-SupervisorLog {
    param([Parameter(Mandatory = $true)][string]$Message)

    $line = "[$((Get-Date).ToString('s'))] $Message"
    Write-Output $line
    Add-Content -LiteralPath $script:supervisorLog -Value $line -Encoding UTF8
}

function Assert-RequiredEnvironment {
    param([Parameter(Mandatory = $true)][string[]]$Names)

    foreach ($name in $Names) {
        $value = [Environment]::GetEnvironmentVariable($name, "Process")
        if ([string]::IsNullOrWhiteSpace($value)) {
            throw "Variavel obrigatoria '$name' ausente na configuracao privada $PrivateConfigPath."
        }
    }
}

function Remove-ExpiredApiLogs {
    param(
        [Parameter(Mandatory = $true)][string]$Directory,
        [Parameter(Mandatory = $true)][int]$RetentionCount
    )

    foreach ($pattern in @("api-*.stdout.log", "api-*.stderr.log")) {
        Get-ChildItem -LiteralPath $Directory -Filter $pattern -File -ErrorAction SilentlyContinue |
            Sort-Object LastWriteTimeUtc -Descending |
            Select-Object -Skip $RetentionCount |
            Remove-Item -Force -ErrorAction SilentlyContinue
    }
}

try {
    $apiUri = [Uri]$ApiUrl
    if ($apiUri.AbsoluteUri.TrimEnd('/') -ne "http://127.0.0.1:5010") {
        throw "A API oficial deve escutar exclusivamente em http://127.0.0.1:5010. Valor recebido: $ApiUrl"
    }

    if ([string]::IsNullOrWhiteSpace($PrivateConfigPath)) {
        $PrivateConfigPath = Join-Path $ProjectRoot "runtime\api-24h\api.env.ps1"
    }

    $resolvedProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
    $publishDir = Join-Path $resolvedProjectRoot "runtime\api-24h\api"
    $logDir = Join-Path $resolvedProjectRoot "runtime-logs\api-24h"
    $apiDll = Join-Path $publishDir "NexumAltivon.API.dll"
    $publicWebRoot = Join-Path $resolvedProjectRoot "NexumAltivon_Back-End\wwwroot"
    $publicUploadRoot = Join-Path $publicWebRoot "uploads"
    $script:supervisorLog = Join-Path $logDir "supervisor.log"

    New-Item -ItemType Directory -Path $logDir -Force | Out-Null

    if (-not (Test-Path -LiteralPath $PrivateConfigPath -PathType Leaf)) {
        throw "Configuracao privada da API nao encontrada em $PrivateConfigPath."
    }

    if (-not (Test-Path -LiteralPath $apiDll -PathType Leaf)) {
        throw "Binario da API nao encontrado em $apiDll. Execute o atualizador oficial antes de iniciar a tarefa."
    }

    if (-not (Test-Path -LiteralPath $publicUploadRoot -PathType Container)) {
        throw "Diretorio oficial de midias publicas nao encontrado em $publicUploadRoot."
    }

    $dotnet = Get-Command dotnet.exe -ErrorAction Stop

    . $PrivateConfigPath

    Assert-RequiredEnvironment -Names @(
        "ConnectionStrings__DefaultConnection",
        "ConnectionStrings__GenesisConnection"
    )

    $jwtSecret = [Environment]::GetEnvironmentVariable("JwtSettings__SecretKey", "Process")
    if ([string]::IsNullOrWhiteSpace($jwtSecret)) {
        $jwtSecret = [Environment]::GetEnvironmentVariable("JWT_SECRET_KEY", "Process")
    }

    if ([string]::IsNullOrWhiteSpace($jwtSecret) -or [Text.Encoding]::UTF8.GetByteCount($jwtSecret) -lt 32) {
        throw "O segredo JWT da configuracao privada deve possuir pelo menos 32 bytes."
    }

    $env:ASPNETCORE_ENVIRONMENT = "Production"
    $env:ASPNETCORE_URLS = "http://127.0.0.1:5010"
    $env:Storage__PublicWebRoot = $publicWebRoot

    $mutex = [Threading.Mutex]::new($false, $mutexName)
    try {
        $ownsMutex = $mutex.WaitOne(0, $false)
    } catch [Threading.AbandonedMutexException] {
        $ownsMutex = $true
    }

    if (-not $ownsMutex) {
        Write-SupervisorLog "Outro supervisor oficial ja esta ativo. Esta execucao sera encerrada sem abrir uma segunda API."
        exit 0
    }

    Write-SupervisorLog "Supervisor oficial iniciado por $([Security.Principal.WindowsIdentity]::GetCurrent().Name) em $env:ASPNETCORE_URLS."
    Set-Location -LiteralPath $publishDir

    $currentDelaySeconds = $RestartDelaySeconds
    while ($true) {
        Remove-ExpiredApiLogs -Directory $logDir -RetentionCount $LogRetentionCount

        $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
        $stdout = Join-Path $logDir "api-$timestamp.stdout.log"
        $stderr = Join-Path $logDir "api-$timestamp.stderr.log"
        $startedAt = Get-Date

        Write-SupervisorLog "Iniciando NexumAltivon.API.dll. stdout=$stdout stderr=$stderr"
        & $dotnet.Source "NexumAltivon.API.dll" 1>> $stdout 2>> $stderr
        $exitCode = $LASTEXITCODE
        $runSeconds = [Math]::Max(0, [int]((Get-Date) - $startedAt).TotalSeconds)

        Write-SupervisorLog "API encerrou com codigo $exitCode apos $runSeconds segundo(s). Reinicio automatico programado."

        if ($runSeconds -ge $StableRunSeconds) {
            $currentDelaySeconds = $RestartDelaySeconds
        } else {
            $currentDelaySeconds = [Math]::Min($MaxRestartDelaySeconds, [Math]::Max($RestartDelaySeconds, $currentDelaySeconds * 2))
        }

        Write-SupervisorLog "Aguardando $currentDelaySeconds segundo(s) antes da proxima inicializacao."
        Start-Sleep -Seconds $currentDelaySeconds
    }
} catch {
    if (-not [string]::IsNullOrWhiteSpace($script:supervisorLog)) {
        try {
            Write-SupervisorLog "Falha fatal do supervisor: $($_.Exception.Message)"
        } catch {
            Write-Error $_.Exception.Message
        }
    }

    throw
} finally {
    if ($ownsMutex -and $mutex) {
        try {
            $mutex.ReleaseMutex()
        } catch {
            Write-Error $_.Exception.Message
        }
    }

    if ($mutex) {
        $mutex.Dispose()
    }
}
