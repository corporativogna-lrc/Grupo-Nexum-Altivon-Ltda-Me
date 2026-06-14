$ErrorActionPreference = "Stop"

$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
  throw "Execute como Administrador."
}

Set-TimeZone -Id "E. South America Standard Time"
Set-Service -Name W32Time -StartupType Automatic
Start-Service -Name W32Time -ErrorAction SilentlyContinue

w32tm.exe /config /manualpeerlist:"time.windows.com,0x8 time.cloudflare.com,0x8" /syncfromflags:manual /update | Out-Null
Restart-Service -Name W32Time
w32tm.exe /resync /force | Out-Null

Write-Host "Fuso horario: $((Get-TimeZone).DisplayName)"
Write-Host "Data e hora: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')"
Write-Host "Sincronizacao automatica ativada."
