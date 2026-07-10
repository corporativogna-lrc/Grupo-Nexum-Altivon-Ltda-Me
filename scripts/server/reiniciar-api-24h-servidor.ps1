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
    [int]$Port = 5010,
    [int]$StartupTimeoutSeconds = 90
)

$ErrorActionPreference = "Stop"

function Assert-Administrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Execute este script como Administrador para reiniciar a API e liberar a porta $Port."
    }
}

function Stop-ApiPort {
    param([int]$TargetPort)

    $listeners = Get-NetTCPConnection -LocalPort $TargetPort -State Listen -ErrorAction SilentlyContinue
    foreach ($listener in $listeners) {
        try {
            Stop-Process -Id $listener.OwningProcess -Force -ErrorAction Stop
        }
        catch {
            throw "Nao foi possivel encerrar o processo $($listener.OwningProcess) que ocupa a porta ${TargetPort}: $($_.Exception.Message)"
        }
    }
}

function Wait-HttpHealthy {
    param(
        [string]$Url,
        [int]$TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        try {
            $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 5
            if ([int]$response.StatusCode -ge 200 -and [int]$response.StatusCode -lt 300) {
                return
            }
        }
        catch {
            Start-Sleep -Seconds 2
        }
    } while ((Get-Date) -lt $deadline)

    throw "API nao ficou saudavel em $Url dentro de $TimeoutSeconds segundos."
}

Assert-Administrator

$task = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
if (-not $task) {
    throw "Tarefa agendada $TaskName nao encontrada. Execute instalar-api-24h-servidor.ps1 antes de reiniciar a API."
}

Stop-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3
Stop-ApiPort -TargetPort $Port
Start-Sleep -Seconds 2
Start-ScheduledTask -TaskName $TaskName

Wait-HttpHealthy -Url "http://127.0.0.1:$Port/health" -TimeoutSeconds $StartupTimeoutSeconds

$task = Get-ScheduledTask -TaskName $TaskName
$listener = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $listener) {
    throw "API respondeu health, mas a porta $Port nao apareceu como Listen."
}

if ($task.State -ne "Running") {
    throw "API respondeu health, mas a tarefa $TaskName ficou em estado $($task.State). Isso indicaria processo orfao e precisa ser corrigido."
}

[pscustomobject]@{
    TaskName = $TaskName
    TaskState = $task.State
    Port = $Port
    ListeningPid = $listener.OwningProcess
    Health = "OK"
}
