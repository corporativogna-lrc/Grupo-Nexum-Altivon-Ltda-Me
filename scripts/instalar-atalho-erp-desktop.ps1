param(
    [string]$Url = $env:NEXUM_ERP_URL,
    [string]$ShortcutName = "ERP Desktop Nexum Altivon",
    [switch]$PublicDesktop,
    [switch]$StartMenu
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$launcherScript = Join-Path $scriptDir "erp-desktop.ps1"

if (-not (Test-Path -LiteralPath $launcherScript)) {
    throw "Nao foi possivel localizar o launcher do ERP Desktop: $launcherScript"
}

$shell = New-Object -ComObject WScript.Shell
$powershellExe = Join-Path $env:SystemRoot "System32\WindowsPowerShell\v1.0\powershell.exe"
$arguments = @(
    "-NoLogo"
    "-NoProfile"
    "-ExecutionPolicy Bypass"
    "-File `"$launcherScript`""
)

if (-not [string]::IsNullOrWhiteSpace($Url)) {
    $arguments += "-Url `"$Url`""
}

$targets = New-Object System.Collections.Generic.List[string]

$userDesktop = [Environment]::GetFolderPath("Desktop")
if (-not [string]::IsNullOrWhiteSpace($userDesktop)) {
    $targets.Add($userDesktop)
}

if ($PublicDesktop) {
    $commonDesktopPath = [Environment]::GetFolderPath("CommonDesktopDirectory")
    if (-not [string]::IsNullOrWhiteSpace($commonDesktopPath)) {
        $targets.Add($commonDesktopPath)
    }
}

if ($StartMenu) {
    $startMenuPath = [Environment]::GetFolderPath("Programs")
    if (-not [string]::IsNullOrWhiteSpace($startMenuPath)) {
        $targets.Add($startMenuPath)
    }
}

if ($targets.Count -eq 0) {
    throw "Nenhum destino valido foi encontrado para criar o atalho."
}

$created = @()

foreach ($targetFolder in $targets) {
    try {
        if (-not (Test-Path -LiteralPath $targetFolder)) {
            New-Item -ItemType Directory -Path $targetFolder -Force | Out-Null
        }

        $shortcutPath = Join-Path $targetFolder "$ShortcutName.lnk"
        $shortcut = $shell.CreateShortcut($shortcutPath)
        $shortcut.TargetPath = $powershellExe
        $shortcut.Arguments = $arguments -join " "
        $shortcut.WorkingDirectory = (Split-Path -Parent $launcherScript)
        $shortcut.IconLocation = "$powershellExe,0"
        $shortcut.WindowStyle = 1
        $shortcut.Save()

        $created += $shortcutPath
    }
    catch {
        Write-Host "Pulando destino sem permissao: $targetFolder" -ForegroundColor Yellow
        Write-Host "  $($_.Exception.Message)" -ForegroundColor DarkYellow
    }
}

Write-Host "Atalho(s) criado(s) com sucesso:" -ForegroundColor Green
$created | ForEach-Object { Write-Host " - $_" }
