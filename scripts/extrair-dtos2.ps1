$linesProg = Get-Content 'D:/Nexum Altivon/NexumAltivon.com/NexumAltivon_Back-End/API/Program.cs'
$linesIa    = Get-Content 'D:/Nexum Altivon/NexumAltivon.com/NexumAltivon_Back-End/API/Services/AssistenteIaService.cs'

# Ranges
$programRanges = @{
    23410 = 30   # CrmCampanhaDto (continuacao)
    23540 = 5    # LeadsRecentesDto
    24319 = 18   # SiteMidiaDto (continuacao)
    24414 = 30   # SitePerfilPublicoRequest
    24441 = 30   # SitePerfilPublicoAdminDto
    24471 = 12   # SitePerfilOrigemDto
    24478 = 12   # SitePerfilProdutoDisponivelDto
    24838 = 12   # LeadLojaDto
    24862 = 18   # LeadRequest
    24945 = 30   # EmpresaGrupoDto (continuacao)
    23686 = 18   # CupomAdminDto
    23673 = 18   # CupomAdminRequest
    24214 = 18   # DropshippingCanalRequest
    24222 = 18   # DropshippingCanalDto
}
foreach ($r in $programRanges.Keys) {
    $start = $r - 1
    $end = [Math]::Min($start + $programRanges[$r], $linesProg.Count - 1)
    $block = $linesProg[$start..$end] -join "`n"
    Write-Host "=== Program.cs Line $r ==="
    Write-Host $block
    Write-Host "---"
}

# AssistenteIaService.cs
$iaRanges = @(380,386,388,394,400,402,409)
foreach ($r in $iaRanges) {
    $start = $r - 1
    $end = [Math]::Min($start + 18, $linesIa.Count - 1)
    $block = $linesIa[$start..$end] -join "`n"
    Write-Host "=== AssistenteIaService.cs Line $r ==="
    Write-Host $block
    Write-Host "---"
}
