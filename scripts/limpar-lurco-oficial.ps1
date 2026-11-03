# Remove arquivos LURCO do projeto oficial.
# Identifica os arquivos pelo uso de stubs genericos (api.ListarAsync / api.SalvarAsync / api.ExcluirAsync / api.SendAuthorizedAsync)
# que nao existem no DesktopApiClient oficial.
# Conforme CLAUDE.md: arquivos de patch nao entram como dependencia de runtime.
# O material ja esta triado em D:\Atualizacoes e Desenvolvimentos Uteis\Projeto Nexum Acompanhamento LURCO432.

$ErrorActionPreference = "Stop"
$dst = "D:/Nexum Altivon/NexumAltivon.com/NexumAltivon.Desktop"

# Lista branca: arquivos que devem PERMANECER mesmo que usem stubs semelhantes.
# Sao os formularios oficiais que ja tem contratos reais no backend.
$manter = @(
    "WorkflowAprovacoesWindow.xaml.cs",
    "TenantManagementWindow.xaml.cs",
    "AccessManagementWindow.xaml.cs"
)

$stubs = @("ListarAsync(_terminal", "SalvarAsync(_terminal", "ExcluirAsync(_terminal", "SendAuthorizedAsync<")
$removidos = @()
$mantidos = @()

$arquivos = Get-ChildItem "$dst/*.xaml.cs" -ErrorAction SilentlyContinue
foreach ($f in $arquivos) {
    $conteudo = Get-Content $f.FullName -Raw -ErrorAction SilentlyContinue
    if (-not $conteudo) { continue }

    $ehLurco = $false
    foreach ($stub in $stubs) {
        if ($conteudo.Contains($stub)) {
            # Mas so eh LURCO se o stub for usado no code-behind (nao em comentario)
            $linhas = $conteudo -split "`n"
            foreach ($linha in $linhas) {
                if ($linha -match "^\s*//") { continue }  # comentario
                if ($linha.Contains($stub)) {
                    $ehLurco = $true
                    break
                }
            }
        }
        if ($ehLurco) { break }
    }

    if ($ehLurco -and -not ($manter -contains $f.Name)) {
        Remove-Item $f.FullName -Force
        $par = $f.BaseName + ".xaml"
        if (Test-Path (Join-Path $dst $par)) {
            Remove-Item (Join-Path $dst $par) -Force
        }
        $removidos += $f.Name
    } else {
        $mantidos += $f.Name
    }
}

Write-Host "Removidos: $($removidos.Count)"
foreach ($n in $removidos) { Write-Host "  - $n" }
Write-Host "Mantidos: $($mantidos.Count)"
