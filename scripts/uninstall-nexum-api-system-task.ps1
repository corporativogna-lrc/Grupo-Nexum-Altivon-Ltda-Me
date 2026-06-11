$ErrorActionPreference = "Stop"
$TaskName = "NexumAltivonApiGuardianSystem"

$currentIdentity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($currentIdentity)
$isAdmin = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
  throw "Abra o PowerShell ou Prompt de Comando como Administrador e execute este desinstalador novamente."
}

Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false -ErrorAction SilentlyContinue
Write-Host "Tarefa removida: $TaskName"
