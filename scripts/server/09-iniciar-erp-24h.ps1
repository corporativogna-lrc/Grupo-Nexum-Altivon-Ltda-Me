param(
  [string]$BaseDirectory = "Y:\NexumAltivon_Services\ERP",
  [string]$Url = "http://127.0.0.1:5020",
  [int]$CheckSeconds = 20
)

$ErrorActionPreference = "Stop"
$AppDirectory = Join-Path $BaseDirectory "app"
$ConfigPath = Join-Path $BaseDirectory "config\erp.env.ps1"
$RuntimeDirectory = Join-Path $BaseDirectory "runtime"
$LogDirectory = Join-Path $BaseDirectory "logs"
$Exe = Join-Path $AppDirectory "NexumAltivon.ERP.exe"
$PidPath = Join-Path $RuntimeDirectory "erp.pid"
$OutLog = Join-Path $LogDirectory "erp.log"
$ErrorLog = Join-Path $LogDirectory "erp.err.log"
$GuardianLog = Join-Path $LogDirectory "erp-guardian.log"
New-Item -ItemType Directory -Force -Path $RuntimeDirectory, $LogDirectory | Out-Null

function Write-ErpLog([string]$Message) {
  Add-Content $GuardianLog "[$(Get-Date -Format s)] $Message"
}
function Test-Erp {
  try {
    $response = Invoke-WebRequest -UseBasicParsing "$Url/health/db" -TimeoutSec 10
    return $response.StatusCode -eq 200
  } catch { return $false }
}
function Stop-Erp {
  $oldPid = Get-Content $PidPath -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($oldPid) { Stop-Process -Id $oldPid -Force -ErrorAction SilentlyContinue }
  Remove-Item $PidPath -Force -ErrorAction SilentlyContinue
}
function Start-Erp {
  try {
    if (-not (Test-Path $ConfigPath)) { throw "Configuracao ausente: $ConfigPath" }
    if (-not (Test-Path $Exe)) { throw "Executavel ausente: $Exe" }
    . $ConfigPath
    $process = Start-Process $Exe -WorkingDirectory $AppDirectory -WindowStyle Hidden `
      -RedirectStandardOutput $OutLog -RedirectStandardError $ErrorLog -PassThru
    Set-Content $PidPath $process.Id
    Write-ErpLog "ERP iniciado em $Url com PID $($process.Id)"
  } catch {
    Write-ErpLog "Falha ao iniciar ERP: $($_.Exception.Message)"
    throw
  }
}

Write-ErpLog "Guardiao ERP iniciado."
while ($true) {
  if (-not (Test-Erp)) {
    Stop-Erp
    Start-Erp
    Start-Sleep -Seconds 12
  }
  Start-Sleep -Seconds $CheckSeconds
}
