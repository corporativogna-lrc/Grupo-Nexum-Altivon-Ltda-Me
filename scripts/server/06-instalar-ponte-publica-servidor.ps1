param(
  [string]$SourceRoot = "",
  [string]$LocalUrl = "http://127.0.0.1:5012",
  [int]$CheckSeconds = 45,
  [string]$Branch = "main",
  [string]$PushUrl = "https://corporativogna-lrc@github.com/corporativogna-lrc/Grupo-Nexum-Altivon-Ltda-Me.git"
)

$ErrorActionPreference = "Stop"

$currentIdentity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($currentIdentity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
  throw "Abra o PowerShell como Administrador no servidor principal e execute novamente."
}

$ScriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $SourceRoot) {
  $SourceRoot = Split-Path -Parent (Split-Path -Parent $ScriptDirectory)
}

$GuardianScript = Join-Path $SourceRoot "scripts\nexum-public-api-guardian.ps1"
if (-not (Test-Path $GuardianScript)) {
  throw "Guardiao publico nao encontrado: $GuardianScript"
}

$CloudflaredPathCandidates = @(
  "C:\Cloudflared\cloudflared.exe",
  "C:\Program Files\cloudflared\cloudflared.exe",
  "C:\Program Files (x86)\cloudflared\cloudflared.exe"
)
$CloudflaredPath = $CloudflaredPathCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $CloudflaredPath) {
  $CloudflaredPath = "C:\Cloudflared\cloudflared.exe"
  New-Item -ItemType Directory -Force -Path (Split-Path -Parent $CloudflaredPath) | Out-Null
  Write-Host "cloudflared nao encontrado. Baixando para: $CloudflaredPath"
  Invoke-WebRequest -UseBasicParsing -Uri "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe" -OutFile $CloudflaredPath -TimeoutSec 120
}

if (-not (Test-Path $CloudflaredPath)) {
  throw "cloudflared nao encontrado no servidor principal."
}

$TaskName = "NexumAltivonPontePublica"
$PowerShellPath = "$env:WINDIR\System32\WindowsPowerShell\v1.0\powershell.exe"
$Arguments = "-NoProfile -ExecutionPolicy Bypass -File `"$GuardianScript`" -RootDir `"$SourceRoot`" -LocalUrl `"$LocalUrl`" -CheckSeconds $CheckSeconds -Branch `"$Branch`" -PushUrl `"$PushUrl`""
$GuardianRunDir = Join-Path $SourceRoot ".nexum-runtime\public-api-guardian"
$GuardianPidPath = Join-Path $GuardianRunDir "guardian.pid"
$TunnelPidPath = Join-Path $GuardianRunDir "cloudflared.pid"
$TunnelUrlPath = Join-Path $GuardianRunDir "api-url.txt"

$Action = New-ScheduledTaskAction -Execute $PowerShellPath -Argument $Arguments -WorkingDirectory $SourceRoot
$TriggerStartup = New-ScheduledTaskTrigger -AtStartup
$TriggerStartup.Delay = "PT90S"
$TriggerLogon = New-ScheduledTaskTrigger -AtLogOn
$Settings = New-ScheduledTaskSettingsSet `
  -AllowStartIfOnBatteries `
  -DontStopIfGoingOnBatteries `
  -ExecutionTimeLimit (New-TimeSpan -Days 3650) `
  -MultipleInstances IgnoreNew `
  -RestartCount 999 `
  -RestartInterval (New-TimeSpan -Minutes 1) `
  -StartWhenAvailable

Stop-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
foreach ($pidFile in @($GuardianPidPath, $TunnelPidPath)) {
  if (Test-Path $pidFile) {
    $oldPid = Get-Content -LiteralPath $pidFile -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($oldPid) {
      Stop-Process -Id ([int]$oldPid) -Force -ErrorAction SilentlyContinue
    }
  }
}
Remove-Item $GuardianPidPath, $TunnelPidPath, $TunnelUrlPath -Force -ErrorAction SilentlyContinue

Register-ScheduledTask `
  -TaskName $TaskName `
  -Action $Action `
  -Trigger @($TriggerStartup, $TriggerLogon) `
  -RunLevel Highest `
  -Settings $Settings `
  -Force | Out-Null

Start-ScheduledTask -TaskName $TaskName

$runtimePath = Join-Path $SourceRoot "api-runtime.json"
$deadline = (Get-Date).AddSeconds([Math]::Max(180, $CheckSeconds + 135))
$publicUrl = $null
do {
  Start-Sleep -Seconds 3
  try {
    $runtime = Get-Content -LiteralPath $runtimePath -Raw -ErrorAction Stop | ConvertFrom-Json
    $url = [string]$runtime.apiUrl
    if ($url -and $url -ne "https://api.trycloudflare.com") {
      $health = Invoke-WebRequest -UseBasicParsing -Uri "$url/health/db" -TimeoutSec 20 -ErrorAction Stop
      if ($health.StatusCode -eq 200) {
        Write-Host "URL publica saudavel: $url"
        $publicUrl = $url
        break
      }
    }
  } catch {
    # A tarefa pode levar alguns segundos para abrir a nova ponte.
  }
} while ((Get-Date) -lt $deadline)

if (-not $publicUrl) {
  throw "Ponte publica nao ficou saudavel. A API local esta OK, mas o Cloudflare nao entregou acesso externo valido neste momento."
}

Write-Host "Ponte publica Nexum instalada no servidor principal."
Write-Host "Tarefa: $TaskName"
Write-Host "Origem local: $LocalUrl"
Write-Host "Script: $GuardianScript"
Write-Host "Cloudflared: $CloudflaredPath"
