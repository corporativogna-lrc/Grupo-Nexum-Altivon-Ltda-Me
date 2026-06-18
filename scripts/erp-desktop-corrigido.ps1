# PROJETO: GenesisGest.Net
# VERSÃO: 1.0.02.11.2520
# MÓDULO: ERP Desktop
# CAMADA: Scripts
# ARQUIVO: erp-desktop.ps1
# AUTOR: [Rodrigo Costa/InfocoSystem]
# DATA: 10/06/2026
# STATUS: Operacional Ativo.
# DESCRIÇÃO: Inicialização do ERP Desktop.

param(
    [string]$Url = $env:NEXUM_ERP_URL
)

try {
    $ErrorActionPreference = "Stop"

    if ([string]::IsNullOrWhiteSpace($Url)) {
        $Url = "https://admin.nexumaltivon.com/login"
    }

    $profileDir = Join-Path $env:LOCALAPPDATA "NexumAltivon\ERPDesktop"

    if (-not (Test-Path $profileDir)) {
        New-Item -ItemType Directory -Path $profileDir -Force | Out-Null
    }

    $browser = $null

    $candidatos = @(
        "$env:ProgramFiles\Google\Chrome\Application\chrome.exe",
        "${env:ProgramFiles(x86)}\Google\Chrome\Application\chrome.exe",
        "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe",
        "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe"
    )

    foreach ($arquivo in $candidatos) {
        if (Test-Path $arquivo) {
            $browser = $arquivo
            break
        }
    }

    if ($browser) {
        Start-Process -FilePath $browser -ArgumentList @(
            "--app=$Url",
            "--new-window",
            "--user-data-dir=$profileDir"
        )
    }
    else {
        Start-Process -FilePath $Url
    }

    exit 0
}
catch {
    Write-Host ""
    Write-Host "ERRO AO INICIAR ERP:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Yellow
    Read-Host "Pressione ENTER para finalizar"
    exit 1
}
