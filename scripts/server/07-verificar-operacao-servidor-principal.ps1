$ErrorActionPreference = "Continue"

Write-Host "== Verificacao da operacao no servidor principal =="
Write-Host ""

Write-Host "[0] Motor da API"
$apiExePath = "C:\NexumAltivon_API_24H\api\NexumAltivon.API.exe"
$apiDllPath = "C:\NexumAltivon_API_24H\api\NexumAltivon.API.dll"
$dotnetPath = @(
  "C:\Program Files\dotnet\dotnet.exe",
  "C:\Program Files (x86)\dotnet\dotnet.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $dotnetPath) {
  $dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
  if ($dotnetCommand) {
    $dotnetPath = $dotnetCommand.Source
  }
}
if ($dotnetPath) {
  Write-Host "OK - Dotnet encontrado: $dotnetPath"
} elseif (Test-Path $apiExePath) {
  Write-Host "OK - API executavel encontrada: $apiExePath"
} elseif (Test-Path $apiDllPath) {
  Write-Host "FALHOU - API publicada em DLL, mas dotnet nao encontrado"
} else {
  Write-Host "FALHOU - Nem dotnet nem API executavel foram encontrados"
}

Write-Host ""
Write-Host "[1] Tarefa API 5012"
$apiTask = Get-ScheduledTask -TaskName "NexumAltivonApi24h5012" -ErrorAction SilentlyContinue
if ($apiTask) {
  Write-Host "OK - Tarefa API encontrada: $($apiTask.State)"
} else {
  Write-Host "FALHOU - Tarefa API 5012 nao encontrada"
}

Write-Host ""
Write-Host "[2] Tarefa Ponte Publica"
$publicTask = Get-ScheduledTask -TaskName "NexumAltivonPontePublica" -ErrorAction SilentlyContinue
if ($publicTask) {
  Write-Host "OK - Tarefa Ponte Publica encontrada: $($publicTask.State)"
} else {
  Write-Host "FALHOU - Tarefa Ponte Publica nao encontrada"
}

Write-Host ""
Write-Host "[3] Porta/API local"
try {
  $health = Invoke-WebRequest -UseBasicParsing -Uri "http://127.0.0.1:5012/health/db" -TimeoutSec 15
  if ($health.StatusCode -eq 200) {
    Write-Host "OK - API local saudavel em 5012"
  } else {
    Write-Host "FALHOU - API local respondeu diferente de 200"
  }
} catch {
  Write-Host "FALHOU - API local nao respondeu em 5012: $($_.Exception.Message)"
}

Write-Host ""
Write-Host "[4] Ponte publica atual"
$runtimePath = Join-Path (Get-Location) "api-runtime.json"
if (Test-Path $runtimePath) {
  try {
    $runtime = Get-Content $runtimePath -Raw | ConvertFrom-Json
    Write-Host "URL publicada: $($runtime.apiUrl)"
    $publicHealth = Invoke-WebRequest -UseBasicParsing -Uri "$($runtime.apiUrl)/health/db" -TimeoutSec 20
    if ($publicHealth.StatusCode -eq 200) {
      Write-Host "OK - Ponte publica saudavel"
    } else {
      Write-Host "FALHOU - Ponte publica respondeu diferente de 200"
    }
  } catch {
    Write-Host "FALHOU - Ponte publica nao respondeu: $($_.Exception.Message)"
  }
} else {
  Write-Host "FALHOU - api-runtime.json nao encontrado"
}
