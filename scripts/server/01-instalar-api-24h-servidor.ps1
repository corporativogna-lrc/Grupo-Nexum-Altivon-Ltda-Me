param(
  [string]$SourceRoot = "",
  [string]$BaseDirectory = "C:\NexumAltivon_API_24H",
  [string]$ApiDirectory = "",
  [string]$ConfigDirectory = "",
  [string]$Url = "http://127.0.0.1:5012",
  [int]$CheckSeconds = 20
)

$ErrorActionPreference = "Stop"

$currentIdentity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($currentIdentity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
  throw "Abra o PowerShell como Administrador e execute novamente."
}

$ScriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $SourceRoot) {
  $SourceRoot = Split-Path -Parent (Split-Path -Parent $ScriptDirectory)
}
$BaseDirectory = [System.IO.Path]::GetFullPath($BaseDirectory.TrimEnd('\', '/'))

$ProjectPath = Join-Path $SourceRoot "NexumAltivon_Back-End\NexumAltivon.API.csproj"
$RunnerSource = Join-Path $ScriptDirectory "04-iniciar-api-24h.ps1"
if (-not $ApiDirectory) {
  $ApiDirectory = Join-Path $BaseDirectory "api"
}
if (-not $ConfigDirectory) {
  $ConfigDirectory = Join-Path $BaseDirectory "config"
}

$RunnerTarget = Join-Path $BaseDirectory "04-iniciar-api-24h.ps1"
$ConfigExampleSource = Join-Path $ScriptDirectory "99-api.env.example.ps1"
$ConfigTarget = Join-Path $ConfigDirectory "api.env.ps1"
$SourceConfig = Join-Path $SourceRoot ".nexum-runtime\api-24h\config\api.env.ps1"
$TaskName = "NexumAltivonApi24h5012"
$DotnetPathCandidates = @(
  "C:\Program Files\dotnet\dotnet.exe",
  "C:\Program Files (x86)\dotnet\dotnet.exe"
)

if (-not (Test-Path $ProjectPath)) {
  throw "Projeto da API não encontrado: $ProjectPath"
}

foreach ($oldTaskName in @("NexumAltivonApi24h", "NexumAltivonApiGuardian", $TaskName)) {
  Stop-ScheduledTask -TaskName $oldTaskName -ErrorAction SilentlyContinue
  Unregister-ScheduledTask -TaskName $oldTaskName -Confirm:$false -ErrorAction SilentlyContinue
}

$oldPidPath = Join-Path $BaseDirectory "runtime\api.pid"
if (Test-Path $oldPidPath) {
  $oldPid = Get-Content -LiteralPath $oldPidPath -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($oldPid) {
    Stop-Process -Id $oldPid -Force -ErrorAction SilentlyContinue
  }
}

Get-Process -Name "NexumAltivon.API" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3

New-Item -ItemType Directory -Force -Path $ApiDirectory, $ConfigDirectory, (Join-Path $BaseDirectory "logs"), (Join-Path $BaseDirectory "runtime") | Out-Null

$DotnetPath = $DotnetPathCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $DotnetPath) {
  $dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
  if ($dotnetCommand) {
    $DotnetPath = $dotnetCommand.Source
  }
}

$ApiExecutable = Join-Path $ApiDirectory "NexumAltivon.API.exe"
$ApiDll = Join-Path $ApiDirectory "NexumAltivon.API.dll"
$SourcePublishedApiDirectory = Join-Path $SourceRoot ".nexum-runtime\api-24h\api"
$SourcePublishedApiExecutable = Join-Path $SourcePublishedApiDirectory "NexumAltivon.API.exe"
$SourcePublishedApiDll = Join-Path $SourcePublishedApiDirectory "NexumAltivon.API.dll"
if ($DotnetPath) {
  $BuildBase = Join-Path $env:TEMP ("nexum-api-publish-" + [guid]::NewGuid().ToString("N"))
  try {
    & $DotnetPath publish $ProjectPath --configuration Release --output $ApiDirectory -p:UseAppHost=false -p:BaseOutputPath="$BuildBase\bin\" -p:BaseIntermediateOutputPath="$BuildBase\obj\"
    if ($LASTEXITCODE -ne 0) {
      throw "Falha ao publicar a API para: $ApiDirectory"
    }
  } finally {
    if (Test-Path $BuildBase) {
      Remove-Item $BuildBase -Recurse -Force -ErrorAction SilentlyContinue
    }
  }
} elseif ((Test-Path $SourcePublishedApiExecutable) -or (Test-Path $SourcePublishedApiDll)) {
  Write-Host "Dotnet nao encontrado no servidor. Copiando API ja publicada para pasta local: $ApiDirectory"
  Get-ChildItem -LiteralPath $SourcePublishedApiDirectory -Force | Copy-Item -Destination $ApiDirectory -Recurse -Force
} elseif ((Test-Path $ApiExecutable) -or (Test-Path $ApiDll)) {
  Write-Host "Dotnet nao encontrado no servidor. Usando API local ja publicada em: $ApiDirectory"
} else {
  throw "dotnet nao encontrado e API publicada nao encontrada. Publique a API em $SourcePublishedApiDirectory ou instale .NET 8 SDK no servidor."
}

if (-not ((Test-Path $ApiExecutable) -or (Test-Path $ApiDll))) {
  throw "Publicacao da API nao encontrada em: $ApiDirectory"
}

Copy-Item $RunnerSource $RunnerTarget -Force

if (Test-Path $SourceConfig) {
  Copy-Item $SourceConfig $ConfigTarget -Force
  Write-Host "Configuracao real sincronizada para a pasta local do servidor: $ConfigTarget"
} else {
  if (-not (Test-Path $ConfigTarget)) {
    Copy-Item $ConfigExampleSource $ConfigTarget
    Write-Host "Configuração criada em: $ConfigTarget"
    Write-Host "Preencha as senhas reais antes de liberar a operação externa."
  }
}

if (Test-Path $ConfigTarget) {
  $configText = Get-Content -LiteralPath $ConfigTarget -Raw
  $desiredUrlLine = '$env:ASPNETCORE_URLS = "http://0.0.0.0:5012"'
  if ($configText -match '(?m)^\s*\$env:ASPNETCORE_URLS\s*=') {
    $configText = $configText -replace '(?m)^\s*\$env:ASPNETCORE_URLS\s*=.*$', $desiredUrlLine
  } else {
    $configText = $configText.TrimEnd() + [Environment]::NewLine + $desiredUrlLine + [Environment]::NewLine
  }
  Set-Content -LiteralPath $ConfigTarget -Value $configText -Encoding UTF8
  Write-Host "Configuração preservada e ajustada para operar em 5012: $ConfigTarget"
}

$PowerShellPath = "$env:WINDIR\System32\WindowsPowerShell\v1.0\powershell.exe"
$Arguments = "-NoProfile -ExecutionPolicy Bypass -File `"$RunnerTarget`" -ApiDirectory `"$ApiDirectory`" -ConfigPath `"$ConfigTarget`" -BaseDirectory `"$BaseDirectory`" -Url $Url -CheckSeconds $CheckSeconds"

$Action = New-ScheduledTaskAction -Execute $PowerShellPath -Argument $Arguments -WorkingDirectory $BaseDirectory
$TriggerStartup = New-ScheduledTaskTrigger -AtStartup
$TriggerStartup.Delay = "PT60S"
$Principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -RunLevel Highest
$Settings = New-ScheduledTaskSettingsSet `
  -AllowStartIfOnBatteries `
  -DontStopIfGoingOnBatteries `
  -ExecutionTimeLimit (New-TimeSpan -Days 3650) `
  -MultipleInstances IgnoreNew `
  -RestartCount 999 `
  -RestartInterval (New-TimeSpan -Minutes 1) `
  -StartWhenAvailable

Register-ScheduledTask `
  -TaskName $TaskName `
  -Action $Action `
  -Trigger $TriggerStartup `
  -Principal $Principal `
  -Settings $Settings `
  -Force | Out-Null

Start-ScheduledTask -TaskName $TaskName
Start-Sleep -Seconds 8

$healthy = $false
for ($attempt = 1; $attempt -le 6; $attempt++) {
  try {
    $response = Invoke-WebRequest -UseBasicParsing -Uri "$Url/health" -TimeoutSec 8
    if ($response.StatusCode -eq 200) {
      $healthy = $true
      break
    }
  } catch {
    Start-Sleep -Seconds 5
  }
}

if (-not $healthy) {
  $guardianLog = Join-Path $BaseDirectory "logs\api-guardian.log"
  $errorLog = Join-Path $BaseDirectory "logs\api.err.log"
  Write-Host "API instalada, mas ainda nao respondeu em $Url."
  if (Test-Path $guardianLog) {
    Write-Host "Ultimas linhas do guardiao:"
    Get-Content -LiteralPath $guardianLog -Tail 12
  }
  if (Test-Path $errorLog) {
    $errorLines = Get-Content -LiteralPath $errorLog -Tail 12
    if ($errorLines) {
      Write-Host "Ultimas linhas de erro:"
      $errorLines
    }
  }
  throw "API 5012 nao iniciou corretamente. Veja o erro acima."
}

Write-Host "API Nexum Altivon instalada para operar 24h."
Write-Host "Tarefa: $TaskName"
Write-Host "URL local: $Url"
Write-Host "Pasta da API: $ApiDirectory"
Write-Host "Configuração privada: $ConfigTarget"
Write-Host "Dotnet: $DotnetPath"
