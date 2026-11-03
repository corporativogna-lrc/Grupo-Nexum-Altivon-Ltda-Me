#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5.7202
#

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$projectRoot = "D:\Nexum Altivon\NexumAltivon.com"
$apiUrl = "http://127.0.0.1:5010"
$updateScript = Join-Path $projectRoot "scripts\server\atualizar-api-oficial-5010.ps1"
$validationScript = Join-Path $projectRoot "scripts\server\validar-api-oficial-24h-task.ps1"

if (-not (Test-Path -LiteralPath $projectRoot -PathType Container)) {
    throw "Diretorio oficial do projeto nao encontrado: $projectRoot"
}

if (-not (Test-Path -LiteralPath $updateScript -PathType Leaf)) {
    throw "Atualizador oficial nao encontrado: $updateScript"
}

if (-not (Test-Path -LiteralPath $validationScript -PathType Leaf)) {
    throw "Validador oficial nao encontrado: $validationScript"
}

$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal]::new($identity)

if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Output "Solicitando elevacao administrativa do Windows para atualizar a API oficial 5010."
    $arguments = @(
        '-NoProfile',
        '-ExecutionPolicy',
        'Bypass',
        '-File',
        $PSCommandPath
    )

    try {
        $elevated = Start-Process -FilePath 'powershell.exe' -Verb RunAs -ArgumentList $arguments -Wait -PassThru
    }
    catch {
        throw "A elevacao administrativa nao foi autorizada: $($_.Exception.Message)"
    }

    if ($elevated.ExitCode -ne 0) {
        throw "A execucao elevada terminou com codigo $($elevated.ExitCode)."
    }

    Write-Output "Atualizacao elevada concluida."
    exit 0
}

Write-Output "Executando atualizacao oficial da API em $apiUrl no projeto $projectRoot."
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $updateScript -ProjectRoot $projectRoot -ApiUrl $apiUrl
if ($LASTEXITCODE -ne 0) {
    throw "O atualizador oficial terminou com codigo $LASTEXITCODE."
}

Write-Output "Executando validacao da tarefa oficial NexumAltivonApi24h."
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $validationScript -ProjectRoot $projectRoot -ApiUrl $apiUrl
if ($LASTEXITCODE -ne 0) {
    throw "O validador oficial terminou com codigo $LASTEXITCODE."
}

Write-Output "API oficial publicada e validada em $apiUrl."
