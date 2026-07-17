#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5
#

[CmdletBinding()]
param(
    [string]$XamppRoot = "D:\xampp",
    [int]$DatabasePort = 3309,
    [string]$PrivateApiConfigPath = "D:\Nexum Altivon\NexumAltivon.com\runtime\api-24h\api.env.ps1",
    [string]$RootUser = "root",
    [string]$RootPasswordEnvironmentVariable = "MYSQL_ROOT_PWD"
)

$ErrorActionPreference = "Stop"

function ConvertTo-MySqlLiteral {
    param([Parameter(Mandatory)][string]$Value)

    return "'" + $Value.Replace("\", "\\").Replace("'", "''") + "'"
}

function Get-ConnectionStringValue {
    param(
        [Parameter(Mandatory)][string]$ConnectionString,
        [Parameter(Mandatory)][string[]]$Keys
    )

    foreach ($segment in $ConnectionString.Split(";", [StringSplitOptions]::RemoveEmptyEntries)) {
        $pair = $segment.Split("=", 2)
        if ($pair.Count -ne 2) {
            continue
        }

        foreach ($key in $Keys) {
            if ($pair[0].Trim().Equals($key, [StringComparison]::OrdinalIgnoreCase)) {
                return $pair[1].Trim()
            }
        }
    }

    return ""
}

function Invoke-MySql {
    param(
        [Parameter(Mandatory)][string]$Sql,
        [Parameter(Mandatory)][string]$User,
        [string]$Password = ""
    )

    $previousPassword = $env:MYSQL_PWD
    try {
        if ([string]::IsNullOrEmpty($Password)) {
            Remove-Item Env:\MYSQL_PWD -ErrorAction SilentlyContinue
        }
        else {
            $env:MYSQL_PWD = $Password
        }

        $output = $Sql | & $script:MySqlPath `
            --protocol=tcp `
            --host=127.0.0.1 `
            --port=$DatabasePort `
            --user=$User `
            --batch `
            --skip-column-names

        if ($LASTEXITCODE -ne 0) {
            throw "mysql.exe retornou codigo $LASTEXITCODE para o usuario informado."
        }

        return @($output)
    }
    finally {
        if ($null -eq $previousPassword) {
            Remove-Item Env:\MYSQL_PWD -ErrorAction SilentlyContinue
        }
        else {
            $env:MYSQL_PWD = $previousPassword
        }
    }
}

if (-not (Test-Path -LiteralPath $PrivateApiConfigPath -PathType Leaf)) {
    throw "Configuracao privada obrigatoria ausente: $PrivateApiConfigPath"
}

$script:MySqlPath = Join-Path $XamppRoot "mysql\bin\mysql.exe"
if (-not (Test-Path -LiteralPath $script:MySqlPath -PathType Leaf)) {
    throw "Cliente MySQL obrigatorio ausente: $script:MySqlPath"
}

$originalContent = [IO.File]::ReadAllText($PrivateApiConfigPath)
$connectionMatches = [regex]::Matches(
    $originalContent,
    "ConnectionStrings__(?:DefaultConnection|GenesisConnection)\s*=\s*'([^']+)'"
)

if ($connectionMatches.Count -ne 2) {
    throw "As duas connection strings privadas oficiais nao foram encontradas."
}

$connections = @($connectionMatches | ForEach-Object { $_.Groups[1].Value })
$users = @($connections | ForEach-Object {
    Get-ConnectionStringValue -ConnectionString $_ -Keys @("Uid", "User", "User Id", "Username")
} | Select-Object -Unique)
$passwords = @($connections | ForEach-Object {
    Get-ConnectionStringValue -ConnectionString $_ -Keys @("Pwd", "Password")
} | Select-Object -Unique)

if ($users.Count -ne 1 -or [string]::IsNullOrWhiteSpace($users[0])) {
    throw "As connection strings privadas devem usar um unico usuario operacional."
}

if ($passwords.Count -ne 1 -or [string]::IsNullOrWhiteSpace($passwords[0])) {
    throw "As connection strings privadas devem usar uma unica credencial operacional."
}

$applicationUser = $users[0]
$oldPassword = $passwords[0]
$rootPassword = ""
if (-not [string]::IsNullOrWhiteSpace($RootPasswordEnvironmentVariable)) {
    $rootPassword = [Environment]::GetEnvironmentVariable($RootPasswordEnvironmentVariable)
}

$rootHostsSql = "SELECT Host FROM mysql.user WHERE User = " +
    (ConvertTo-MySqlLiteral -Value $applicationUser) +
    " ORDER BY Host;"
$applicationHosts = @(Invoke-MySql -Sql $rootHostsSql -User $RootUser -Password $rootPassword)
if ($applicationHosts.Count -eq 0) {
    throw "Nenhuma conta MySQL foi encontrada para o usuario operacional."
}

$randomBytes = New-Object byte[] 32
$randomNumberGenerator = [Security.Cryptography.RandomNumberGenerator]::Create()
try {
    $randomNumberGenerator.GetBytes($randomBytes)
}
finally {
    $randomNumberGenerator.Dispose()
}
$newPassword = [BitConverter]::ToString($randomBytes).Replace("-", "")

$passwordOccurrences = [regex]::Matches($originalContent, "(?<=Pwd=)[^;'\r\n]+")
if ($passwordOccurrences.Count -ne 2) {
    throw "A configuracao privada nao possui exatamente duas credenciais para rotacao."
}

$updatedContent = [regex]::Replace($originalContent, "(?<=Pwd=)[^;'\r\n]+", $newPassword)
$temporaryPath = "$PrivateApiConfigPath.rotate.tmp"
[IO.File]::WriteAllText($temporaryPath, $updatedContent, (New-Object Text.UTF8Encoding($false)))

$userLiteral = ConvertTo-MySqlLiteral -Value $applicationUser
$newPasswordLiteral = ConvertTo-MySqlLiteral -Value $newPassword
$oldPasswordLiteral = ConvertTo-MySqlLiteral -Value $oldPassword
$rotateSql = ($applicationHosts | ForEach-Object {
    "ALTER USER $userLiteral@$(ConvertTo-MySqlLiteral -Value $_) IDENTIFIED BY $newPasswordLiteral;"
}) -join "`n"
$rollbackSql = ($applicationHosts | ForEach-Object {
    "ALTER USER $userLiteral@$(ConvertTo-MySqlLiteral -Value $_) IDENTIFIED BY $oldPasswordLiteral;"
}) -join "`n"

$databaseRotated = $false
$configurationUpdated = $false
try {
    Invoke-MySql -Sql $rotateSql -User $RootUser -Password $rootPassword | Out-Null
    $databaseRotated = $true

    Move-Item -LiteralPath $temporaryPath -Destination $PrivateApiConfigPath -Force
    $configurationUpdated = $true

    $validationSql = @"
SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'nexum_altivon';
SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'genesis_bd';
"@
    $tableCounts = @(Invoke-MySql -Sql $validationSql -User $applicationUser -Password $newPassword)
    if ($tableCounts.Count -ne 2 -or [long]$tableCounts[0] -le 0 -or [long]$tableCounts[1] -le 0) {
        throw "A nova credencial nao confirmou acesso aos dois schemas oficiais."
    }

    [pscustomobject]@{
        Rotated = $true
        User = $applicationUser
        AccountsUpdated = $applicationHosts.Count
        SchemasValidated = 2
        PrivateConfiguration = $PrivateApiConfigPath
    }
}
catch {
    if ($configurationUpdated) {
        [IO.File]::WriteAllText($PrivateApiConfigPath, $originalContent, (New-Object Text.UTF8Encoding($false)))
    }

    if ($databaseRotated) {
        Invoke-MySql -Sql $rollbackSql -User $RootUser -Password $rootPassword | Out-Null
    }

    throw
}
finally {
    Remove-Item -LiteralPath $temporaryPath -Force -ErrorAction SilentlyContinue
    [Array]::Clear($randomBytes, 0, $randomBytes.Length)
    $newPassword = $null
    $oldPassword = $null
}
