#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5.7226 Data: 16/08/2026
#

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Wpf', 'Api', 'Solucao')]
    [string]$Modo
)

$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$configuration = 'Release'

$installedSdks = @(& dotnet --list-sdks 2>$null)
if ($LASTEXITCODE -ne 0 -or $installedSdks.Count -eq 0) {
    throw 'SDK .NET ausente. O build oficial exige um SDK instalado; runtime sozinho nao compila a solucao.'
}

$target = switch ($Modo) {
    'Wpf' {
        @{
            Command = 'build'
            Project = Join-Path $projectRoot 'NexumAltivon.Desktop\NexumAltivon.Desktop.csproj'
            Output = 'bin\Release\validacao-wpf'
            Intermediate = 'obj\Release\validacao-wpf\'
            Parallel = 'false'
        }
    }
    'Api' {
        @{
            Command = 'build'
            Project = Join-Path $projectRoot 'NexumAltivon_Back-End\NexumAltivon.API.csproj'
            Output = 'bin\Release\validacao-api'
            Intermediate = 'obj\Release\validacao-api\'
            Parallel = 'false'
        }
    }
    'Solucao' {
        @{
            Command = 'build'
            Project = Join-Path $projectRoot 'NexumAltivon.ERP.sln'
            Output = 'bin\Release\validacao-solucao'
            Intermediate = 'obj\Release\validacao-solucao\'
            Parallel = 'true'
        }
    }
}

if (-not (Test-Path -LiteralPath $target.Project)) {
    throw "Alvo de compilação não encontrado: $($target.Project)"
}

Push-Location $projectRoot
try {
    $arguments = @(
        $target.Command,
        $target.Project,
        '-c',
        $configuration,
        '--no-restore',
        '-p:UseSharedCompilation=true',
        "-p:BuildInParallel=$($target.Parallel)",
        "-p:OutputPath=$($target.Output)",
        "-p:IntermediateOutputPath=$($target.Intermediate)"
    )

    Write-Host "Compilação oficial: $Modo"
    Write-Host "Projeto: $($target.Project)"
    Write-Host "Saída isolada: $($target.Output)"
    & dotnet @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "A compilação oficial falhou com código $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}
