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
    [string]$Url = $env:NEXUM_ERP_URL,
    [ValidateSet('Perguntar', 'Chrome', 'Edge', 'Sistema')]
    [string]$BrowserChoice = $(if ($env:NEXUM_ERP_BROWSER) { $env:NEXUM_ERP_BROWSER } else { 'Chrome' })
)

try {
    $ErrorActionPreference = "Stop"

    if ([string]::IsNullOrWhiteSpace($Url)) {
        $Url = "http://127.0.0.1:3000/dashboard"
    }

    $profileDir = Join-Path $env:LOCALAPPDATA "NexumAltivon\ERPDesktop"
    if (-not (Test-Path $profileDir)) {
        New-Item -ItemType Directory -Path $profileDir -Force | Out-Null
    }

    function Get-BrowserPath {
        param([string]$Choice)

        $chromePaths = @(
            "$env:ProgramFiles\Google\Chrome\Application\chrome.exe",
            "${env:ProgramFiles(x86)}\Google\Chrome\Application\chrome.exe"
        )

        $edgePaths = @(
            "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe",
            "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe"
        )

        switch ($Choice) {
            'Chrome' {
                foreach ($path in $chromePaths) { if (Test-Path -LiteralPath $path) { return $path } }
            }
            'Edge' {
                foreach ($path in $edgePaths) { if (Test-Path -LiteralPath $path) { return $path } }
            }
            'Sistema' {
                return $null
            }
            default {
                foreach ($path in $chromePaths) { if (Test-Path -LiteralPath $path) { return $path } }
                foreach ($path in $edgePaths) { if (Test-Path -LiteralPath $path) { return $path } }
            }
        }

        return $null
    }

    $resolvedBrowserChoice = $BrowserChoice
    if ($resolvedBrowserChoice -eq 'Perguntar') {
        Write-Host ""
        Write-Host "Escolha o navegador para abrir o ERP Desktop:" -ForegroundColor Cyan
        Write-Host "  [1] Chrome"
        Write-Host "  [2] Edge"
        Write-Host "  [3] Sistema (padrao)"
        $answer = Read-Host "Opcao (Enter = Chrome)"

        switch ($answer) {
            '2' { $resolvedBrowserChoice = 'Edge' }
            '3' { $resolvedBrowserChoice = 'Sistema' }
            default { $resolvedBrowserChoice = 'Chrome' }
        }
    }

    $browser = Get-BrowserPath -Choice $resolvedBrowserChoice
    $argumentList = "--app=`"$Url`" --new-window --user-data-dir=`"$profileDir`""

    if ($browser) {
        Start-Process -FilePath $browser -ArgumentList $argumentList
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
