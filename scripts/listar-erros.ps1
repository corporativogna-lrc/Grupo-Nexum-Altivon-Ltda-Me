Set-Location 'D:/Nexum Altivon/NexumAltivon.com/NexumAltivon.Desktop'
$out = & dotnet build --nologo --verbosity quiet 2>&1 | Out-String
$out | Select-String -Pattern 'CS0246' | ForEach-Object {
  $line = $_.ToString()
  if ($line -match '"([A-Za-z_][A-Za-z0-9_]*)"') { $matches[1] }
} | Sort-Object -Unique | ForEach-Object { Write-Host $_ }
