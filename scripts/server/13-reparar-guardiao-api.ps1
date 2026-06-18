$ErrorActionPreference = "Stop"

$baseDirectory = "$env:ProgramData\NexumAltivon_API_24H"
$taskName = "NexumAltivonApi24h"
$healthUrl = "http://127.0.0.1:5010/health/db"
$resultPath = Join-Path $baseDirectory "logs\reparo-guardiao.log"

$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
  throw "Execute como Administrador."
}

Stop-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue

$matchingProcesses = Get-CimInstance Win32_Process | Where-Object {
  ($_.Name -match 'powershell|pwsh' -and $_.CommandLine -like '*04-iniciar-api-24h.ps1*') -or
  ($_.Name -eq 'NexumAltivon.API.exe' -and $_.ExecutablePath -like "$baseDirectory*")
}

foreach ($process in $matchingProcesses) {
  Stop-Process -Id $process.ProcessId -Force -ErrorAction SilentlyContinue
}

Remove-Item (Join-Path $baseDirectory "runtime\api.pid") -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3
Start-ScheduledTask -TaskName $taskName

$healthy = $false
$detail = ""
for ($attempt = 0; $attempt -lt 45; $attempt++) {
  Start-Sleep -Seconds 1
  try {
    $response = Invoke-WebRequest -Uri $healthUrl -UseBasicParsing -TimeoutSec 3
    $detail = $response.Content
    if ($response.StatusCode -eq 200) {
      $healthy = $true
      break
    }
  } catch {
    $detail = $_.Exception.Message
  }
}

$result = "[$(Get-Date -Format s)] Saudavel=$healthy Detalhe=$detail"
Set-Content -Path $resultPath -Value $result
Write-Host $result

if (-not $healthy) {
  throw "A API ainda nao ficou saudavel. Consulte $resultPath e os logs da API."
}
