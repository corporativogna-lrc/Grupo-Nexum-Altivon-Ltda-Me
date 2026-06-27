param(
  [string]$Url = "http://127.0.0.1:5012",
  [string]$PublicUrl = "https://api.nexumaltivon.com.br"
)

$ErrorActionPreference = "Continue"

Write-Host "== Nexum Altivon - Verificação API 24h =="
Write-Host ""

Write-Host "[1/5] Tarefa automática"
Get-ScheduledTask -TaskName "NexumAltivonApi24h" -ErrorAction SilentlyContinue | Format-List TaskName, State

Write-Host "[2/5] Porta local"
netstat -ano | findstr ":5012"

Write-Host "[3/5] Saúde local"
try {
  Invoke-WebRequest -UseBasicParsing -Uri "$Url/health" -TimeoutSec 10 | Select-Object StatusCode, Content
} catch {
  Write-Host "Falhou saúde local: $($_.Exception.Message)"
}

Write-Host "[4/5] DNS público"
nslookup api.nexumaltivon.com.br 1.1.1.1

Write-Host "[5/5] Saúde pública"
try {
  Invoke-WebRequest -UseBasicParsing -Uri "$PublicUrl/health" -TimeoutSec 15 | Select-Object StatusCode, Content
} catch {
  Write-Host "Falhou saúde pública: $($_.Exception.Message)"
}
