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
  $apiTaskInfo = Get-ScheduledTaskInfo -TaskName "NexumAltivonApi24h5012" -ErrorAction SilentlyContinue
  Write-Host "OK - Tarefa API encontrada: $($apiTask.State)"
  if ($apiTaskInfo) {
    Write-Host "Ultimo resultado da API: $($apiTaskInfo.LastTaskResult)"
  }
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
$localOk = $false
try {
  for ($attempt = 1; $attempt -le 24; $attempt++) {
    try {
      $health = Invoke-WebRequest -UseBasicParsing -Uri "http://127.0.0.1:5012/health/db" -TimeoutSec 8
      if ($health.StatusCode -eq 200) {
        $localOk = $true
        break
      }
    } catch {
      Start-Sleep -Seconds 5
    }
  }

  if ($localOk) {
    Write-Host "OK - API local saudavel em 5012"
  } else {
    Write-Host "FALHOU - API local nao respondeu em 5012 dentro do tempo limite"
  }
} catch {
  Write-Host "FALHOU - API local nao respondeu em 5012: $($_.Exception.Message)"
}

Write-Host ""
Write-Host "[3.1] Login administrativo"
if ($localOk) {
  try {
    $loginBody = @{ email = "admin@nexumaltivon.com"; senha = "1234" } | ConvertTo-Json
    $login = Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:5012/api/auth/login" -ContentType "application/json" -Body $loginBody -TimeoutSec 20
    $token = $login.dados.token
    if ($token) {
      $me = Invoke-WebRequest -UseBasicParsing -Uri "http://127.0.0.1:5012/api/auth/me" -Headers @{ Authorization = "Bearer $token" } -TimeoutSec 20
      if ($me.StatusCode -eq 200) {
        Write-Host "OK - Login admin e sessao administrativa saudaveis"
      } else {
        Write-Host "FALHOU - Sessao administrativa respondeu diferente de 200"
      }
    } else {
      Write-Host "FALHOU - Login admin nao retornou token"
    }
  } catch {
    Write-Host "FALHOU - Login admin nao validou: $($_.Exception.Message)"
  }
} else {
  Write-Host "PULADO - API local indisponivel"
}

Write-Host ""
Write-Host "[3.2] Area do cliente"
if ($localOk) {
  try {
    $clienteBody = @{ email = "cliente.portal.teste.202606230815@nexumteste.local"; senha = "ClienteTeste2026!" } | ConvertTo-Json
    $clienteLogin = Invoke-RestMethod -Method Post -Uri "http://127.0.0.1:5012/api/auth/login" -ContentType "application/json" -Body $clienteBody -TimeoutSec 20
    $clienteToken = $clienteLogin.dados.token
    if ($clienteToken) {
      $portal = Invoke-WebRequest -UseBasicParsing -Uri "http://127.0.0.1:5012/api/clientes/portal/me" -Headers @{ Authorization = "Bearer $clienteToken" } -TimeoutSec 20
      if ($portal.StatusCode -eq 200) {
        Write-Host "OK - Login cliente e portal do cliente saudaveis"
      } else {
        Write-Host "FALHOU - Portal do cliente respondeu diferente de 200"
      }
    } else {
      Write-Host "FALHOU - Login cliente nao retornou token"
    }
  } catch {
    Write-Host "FALHOU - Area do cliente nao validou: $($_.Exception.Message)"
  }
} else {
  Write-Host "PULADO - API local indisponivel"
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

Write-Host ""
Write-Host "[5] Logs locais da API"
$apiGuardianLog = "C:\NexumAltivon_API_24H\logs\api-guardian.log"
$apiErrorLog = "C:\NexumAltivon_API_24H\logs\api.err.log"
if (Test-Path $apiGuardianLog) {
  Write-Host "Ultimas linhas do guardiao:"
  Get-Content -LiteralPath $apiGuardianLog -Tail 8
} else {
  Write-Host "Sem log do guardiao em C:\NexumAltivon_API_24H"
}
if (Test-Path $apiErrorLog) {
  $errorLines = Get-Content -LiteralPath $apiErrorLog -Tail 8
  if ($errorLines) {
    Write-Host "Ultimas linhas de erro:"
    $errorLines
  }
}
