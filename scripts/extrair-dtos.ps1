$lines = Get-Content 'D:/Nexum Altivon/NexumAltivon.com/NexumAltivon_Back-End/API/Program.cs'
$ranges = @(22706,22720,22731,22751,22769,22777,22787,22791,22797,22811,23363,23375,23390,23410,23665,23673,23686,23858,23867,23880,23891,23898,23906,23928,23937,23948,23963,23977,23990,24002,24012,24214,24222,24305,24313,24319,24640,24659,24838,24862,24876,24945,25113)
foreach ($r in $ranges) {
    $start = $r - 1
    $end = [Math]::Min($start + 25, $lines.Count - 1)
    $block = $lines[$start..$end] -join "`n"
    Write-Host "=== Line $r ==="
    Write-Host $block
    Write-Host "---"
}
