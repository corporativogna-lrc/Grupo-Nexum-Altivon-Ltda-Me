# Triagem dos arquivos LURCO432_Patch conforme CLAUDE.md
# - Utilizados: janelas com equivalencia semantica ja implementada no projeto oficial
# - Descartados: os demais (template generico sem contrato real)
# Rodar a partir de qualquer diretorio.

$ErrorActionPreference = "Stop"

$src = "D:/Nexum Altivon/NexumAltivon.com/NexumAltivon.Desktop/LURCO432_Triagem"
$utilizados = "D:/Atualizacoes e Desenvolvimentos Uteis/Projeto Nexum Acompanhamento LURCO432/Arquivos Utilizados"
$descartados = "D:/Atualizacoes e Desenvolvimentos Uteis/Projeto Nexum Acompanhamento LURCO432/Arquivos Descartados"

if (-not (Test-Path $utilizados)) { New-Item -ItemType Directory -Force -Path $utilizados | Out-Null }
if (-not (Test-Path $descartados)) { New-Item -ItemType Directory -Force -Path $descartados | Out-Null }

# Janelas com equivalencia semantica oficial (CLAUDE.md "Auditoria LURCO432 - etapa 1")
$utilizadas = @(
  "ConciliacaoBancariaWindow_Patch",
  "CategoriasMarcasWindow_Patch",
  "CentrosCustoLucroWindow_Patch",
  "ItensServicosKitsWindow_Patch",
  "EmpresasFiliaisWindow_Patch",
  "FechamentoPeriodoWindow_Patch",
  "FiscalRoutingWindow_Patch",
  "EmissaoNfeWindow_Patch",
  "WorkflowAprovacoesWindow_Patch",
  "CorporateMasterDataWindow_Patch",
  "ContasPagarWindow_Patch",
  "ContasReceberWindow_Patch",
  "ModuleWorkspaceWindow_Patch",
  "GestaoTenantsWindow_Patch",
  "GestaoUsuariosWindow_Patch"
)

if (-not (Test-Path $src)) {
  Write-Host "LURCO432_Triagem nao existe. Nada a mover."
  exit 0
}

$all = Get-ChildItem $src -Filter "*_Patch.xaml" | Select-Object -ExpandProperty BaseName
$movU = 0
$movD = 0
$skipped = 0

foreach ($name in $all) {
  $xaml = Join-Path $src ($name + ".xaml")
  $cs   = Join-Path $src ($name + ".xaml.cs")

  if ($utilizadas -contains $name) {
    Move-Item $xaml -Destination $utilizados -Force -ErrorAction SilentlyContinue
    Move-Item $cs   -Destination $utilizados -Force -ErrorAction SilentlyContinue
    $movU++
  } else {
    Move-Item $xaml -Destination $descartados -Force -ErrorAction SilentlyContinue
    Move-Item $cs   -Destination $descartados -Force -ErrorAction SilentlyContinue
    $movD++
  }
}

Write-Host ("Utilizados movidos: " + $movU)
Write-Host ("Descartados movidos: " + $movD)
$restantes = (Get-ChildItem $src -ErrorAction SilentlyContinue | Measure-Object).Count
Write-Host ("Restantes em LURCO432_Triagem: " + $restantes)
