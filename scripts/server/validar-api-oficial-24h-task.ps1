#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5
#

[CmdletBinding()]
param(
    [string]$TaskName = "NexumAltivonApi24h",
    [string]$ProjectRoot = "D:\Nexum Altivon\NexumAltivon.com"
)

$ErrorActionPreference = "Stop"

$resolvedProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$logPath = Join-Path $resolvedProjectRoot "runtime-logs\api-24h-task-query.log"

$task = Get-ScheduledTask -TaskName $TaskName
$info = Get-ScheduledTaskInfo -TaskName $TaskName
$listener = Get-NetTCPConnection -LocalPort 5010 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
$process = if ($listener) { Get-CimInstance Win32_Process -Filter "ProcessId = $($listener.OwningProcess)" } else { $null }
$parent = if ($process) { Get-CimInstance Win32_Process -Filter "ProcessId = $($process.ParentProcessId)" } else { $null }
$health = Invoke-WebRequest -UseBasicParsing -Uri "http://127.0.0.1:5010/health" -TimeoutSec 15

$result = [pscustomobject]@{
    CheckedAt = (Get-Date).ToString("s")
    TaskName = $task.TaskName
    TaskState = $task.State
    TaskUser = $task.Principal.UserId
    RunLevel = $task.Principal.RunLevel
    LastRunTime = $info.LastRunTime
    LastTaskResult = $info.LastTaskResult
    NextRunTime = $info.NextRunTime
    ListenerPid = if ($listener) { $listener.OwningProcess } else { $null }
    ListenerCommand = if ($process) { $process.CommandLine } else { $null }
    ParentPid = if ($process) { $process.ParentProcessId } else { $null }
    ParentCommand = if ($parent) { $parent.CommandLine } else { $null }
    HealthStatus = $health.StatusCode
    HealthBody = $health.Content
}

$result | Format-List | Out-String | Set-Content -LiteralPath $logPath -Encoding UTF8
$result | Format-List
