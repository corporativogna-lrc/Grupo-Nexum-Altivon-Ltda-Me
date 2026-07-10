#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5
#

[CmdletBinding()]
param(
    [string]$ConfigPath = "C:\ProgramData\cloudflared\config.yml",
    [string]$ServiceName = "cloudflared",
    [int]$ApiPort = 5010,
    [string[]]$RequiredHostnames = @(
        "api.nexumaltivon.com.br",
        "api.nexumaltivon.com",
        "back.nexumaltivon.com.br",
        "back.nexumaltivon.com"
    )
)

$ErrorActionPreference = "Stop"

function Assert-Administrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Execute este ativador em PowerShell/CMD como Administrador."
    }
}

function Resolve-CloudflaredPath {
    $candidates = @(
        "C:\Program Files\Cloudflared\cloudflared.exe",
        "C:\Program Files (x86)\Cloudflared\cloudflared.exe",
        "C:\cloudflared\cloudflared.exe"
    )

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return $candidate
        }
    }

    $command = Get-Command cloudflared -ErrorAction SilentlyContinue
    if ($command -and (Test-Path -LiteralPath $command.Source)) {
        return $command.Source
    }

    throw "cloudflared.exe nao encontrado. Instale o Cloudflare Tunnel no servidor e execute novamente."
}

function Test-ConfigContent {
    param([string]$Content)

    foreach ($hostname in $RequiredHostnames) {
        if ($Content -notmatch [Regex]::Escape($hostname)) {
            throw "Configuração do Cloudflare Tunnel nao contem hostname obrigatorio: $hostname."
        }
    }

    $expectedService = "http://127.0.0.1:$ApiPort"
    $alternativeService = "http://localhost:$ApiPort"
    if ($Content -notmatch [Regex]::Escape($expectedService) -and $Content -notmatch [Regex]::Escape($alternativeService)) {
        throw "Configuração do Cloudflare Tunnel nao aponta para $expectedService."
    }
}

Assert-Administrator

if (-not (Test-Path -LiteralPath $ConfigPath)) {
    throw "Configuração do Cloudflare Tunnel nao encontrada em $ConfigPath. Crie o tunnel nomeado com credenciais reais antes de ativar o serviço."
}

$cloudflared = Resolve-CloudflaredPath
$configContent = Get-Content -LiteralPath $ConfigPath -Raw
Test-ConfigContent -Content $configContent

$binaryPath = "`"$cloudflared`" tunnel --config `"$ConfigPath`" run"
$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($service) {
    & sc.exe config $ServiceName binPath= $binaryPath start= auto | Out-Null
}
else {
    New-Service -Name $ServiceName -BinaryPathName $binaryPath -StartupType Automatic -DisplayName "Cloudflare Tunnel - Nexum Altivon" | Out-Null
}

if ((Get-Service -Name $ServiceName).Status -ne "Running") {
    Start-Service -Name $ServiceName
}
Start-Sleep -Seconds 3
$service = Get-Service -Name $ServiceName
if ($service.Status -ne "Running") {
    throw "Serviço $ServiceName nao ficou em execução. Status atual: $($service.Status)."
}

Write-Host "Cloudflare Tunnel ativo como serviço Windows: $ServiceName"
Write-Host "Configuração: $ConfigPath"
Write-Host "Origem esperada da API: http://127.0.0.1:$ApiPort"
