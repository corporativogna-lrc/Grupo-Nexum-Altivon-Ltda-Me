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
    [string]$AppSettingsPath = "",
    [string]$PrivateApiConfigPath = "D:\Nexum Altivon\NexumAltivon.com\runtime\api-24h\api.env.ps1",
    [string]$ConnectionStringName = "DefaultConnection",
    [string]$RootUser = "root",
    [string]$RootPasswordEnvironmentVariable = "MYSQL_ROOT_PWD",
    [string[]]$ApplicationHosts = @("localhost", "127.0.0.1", "192.168.1.%"),
    [string[]]$Schemas = @("nexum_altivon", "genesis_bd")
)

$ErrorActionPreference = "Stop"

function ConvertTo-MySqlLiteral {
    param([string]$Value)
    return "'" + $Value.Replace("\", "\\").Replace("'", "''") + "'"
}

function Get-ConnectionStringValue {
    param(
        [string]$ConnectionString,
        [string[]]$Keys
    )

    foreach ($segment in $ConnectionString.Split(";", [StringSplitOptions]::RemoveEmptyEntries)) {
        $pair = $segment.Split("=", 2)
        if ($pair.Count -ne 2) {
            continue
        }

        $key = $pair[0].Trim()
        $value = $pair[1].Trim()
        foreach ($expectedKey in $Keys) {
            if ($key.Equals($expectedKey, [StringComparison]::OrdinalIgnoreCase)) {
                return $value
            }
        }
    }

    return ""
}

function Invoke-MySql {
    param(
        [string]$Sql,
        [string]$User,
        [string]$Password
    )

    $previousPassword = $env:MYSQL_PWD
    try {
        if (-not [string]::IsNullOrEmpty($Password)) {
            $env:MYSQL_PWD = $Password
        }
        elseif (Test-Path Env:\MYSQL_PWD) {
            Remove-Item Env:\MYSQL_PWD -ErrorAction SilentlyContinue
        }

        $Sql | & $mysqlPath --protocol=tcp --host=localhost --port=$DatabasePort --user=$User --batch --skip-column-names
        if ($LASTEXITCODE -ne 0) {
            throw "mysql.exe retornou codigo $LASTEXITCODE."
        }
    }
    finally {
        if ($null -ne $previousPassword) {
            $env:MYSQL_PWD = $previousPassword
        }
        else {
            Remove-Item Env:\MYSQL_PWD -ErrorAction SilentlyContinue
        }
    }
}

$scriptDir = Split-Path -Parent $PSCommandPath
$projectRoot = Split-Path -Parent (Split-Path -Parent $scriptDir)
if ([string]::IsNullOrWhiteSpace($AppSettingsPath)) {
    $AppSettingsPath = Join-Path $projectRoot "NexumAltivon_Back-End\API\appsettings.json"
}

$mysqlRoot = Join-Path $XamppRoot "mysql"
$mysqlPath = Join-Path $mysqlRoot "bin\mysql.exe"
$dataDir = Join-Path $mysqlRoot "data"

foreach ($requiredPath in @($XamppRoot, $mysqlRoot, $mysqlPath, $dataDir, $AppSettingsPath)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Caminho obrigatorio ausente: $requiredPath"
    }
}

foreach ($schema in $Schemas) {
    $schemaPath = Join-Path $dataDir $schema
    if (-not (Test-Path -LiteralPath $schemaPath -PathType Container)) {
        throw "Schema obrigatorio ausente no datadir: $schemaPath"
    }
}

$connectionString = ""
$connectionSource = ""

if (Test-Path -LiteralPath $PrivateApiConfigPath -PathType Leaf) {
    . $PrivateApiConfigPath
    $privateConnection = [Environment]::GetEnvironmentVariable("ConnectionStrings__$ConnectionStringName")
    if (-not [string]::IsNullOrWhiteSpace($privateConnection)) {
        $connectionString = $privateConnection
        $connectionSource = $PrivateApiConfigPath
    }
}

if ([string]::IsNullOrWhiteSpace($connectionString)) {
    $appSettingsContent = Get-Content -LiteralPath $AppSettingsPath -Raw
    $connectionStringMatch = [regex]::Match($appSettingsContent, '"' + [regex]::Escape($ConnectionStringName) + '"\s*:\s*"([^"]+)"')
    if (-not $connectionStringMatch.Success) {
        throw "Connection string $ConnectionStringName nao encontrada em $AppSettingsPath."
    }

    $connectionString = $connectionStringMatch.Groups[1].Value
    $connectionSource = $AppSettingsPath
}

$applicationUser = Get-ConnectionStringValue -ConnectionString $connectionString -Keys @("Uid", "User", "User Id", "Username")
$applicationPassword = Get-ConnectionStringValue -ConnectionString $connectionString -Keys @("Pwd", "Password")

if ([string]::IsNullOrWhiteSpace($applicationUser)) {
    throw "Usuario da aplicacao nao encontrado em $ConnectionStringName."
}

if ([string]::IsNullOrWhiteSpace($applicationPassword)) {
    throw "Senha da aplicacao nao encontrada em $ConnectionStringName."
}

if ($applicationPassword -match "CHANGE_ME" -or
    $applicationPassword -match "USE_ENV" -or
    $applicationPassword -eq "null") {
    throw "Senha da aplicacao em $ConnectionStringName nao e valor real na origem $connectionSource. Configure uma senha real no arquivo privado da API antes de criar o usuario MariaDB."
}

$rootPassword = ""
if (-not [string]::IsNullOrWhiteSpace($RootPasswordEnvironmentVariable) -and
    (Test-Path "Env:\$RootPasswordEnvironmentVariable")) {
    $rootPassword = (Get-Item "Env:\$RootPasswordEnvironmentVariable").Value
}

$userLiteral = ConvertTo-MySqlLiteral -Value $applicationUser
$passwordLiteral = ConvertTo-MySqlLiteral -Value $applicationPassword
$sqlLines = @()

foreach ($applicationHost in $ApplicationHosts) {
    $hostLiteral = ConvertTo-MySqlLiteral -Value $applicationHost
    $sqlLines += "CREATE USER IF NOT EXISTS $userLiteral@$hostLiteral IDENTIFIED BY $passwordLiteral;"
    $sqlLines += "ALTER USER $userLiteral@$hostLiteral IDENTIFIED BY $passwordLiteral;"

    foreach ($schema in $Schemas) {
        $sqlLines += "GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, DROP, INDEX, REFERENCES, CREATE TEMPORARY TABLES, LOCK TABLES, EXECUTE, TRIGGER, CREATE VIEW, SHOW VIEW ON ``$schema``.* TO $userLiteral@$hostLiteral;"
    }
}

$sqlLines += "FLUSH PRIVILEGES;"
$sqlLines += "SELECT User, Host FROM mysql.user WHERE User = $userLiteral ORDER BY Host;"

Invoke-MySql -Sql ($sqlLines -join "`n") -User $RootUser -Password $rootPassword

$validationSql = @"
SELECT CURRENT_USER();
SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'nexum_altivon';
SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'genesis_bd';
"@

Invoke-MySql -Sql $validationSql -User $applicationUser -Password $applicationPassword

Write-Host "Usuario $applicationUser configurado e validado em $DatabasePort para schemas: $($Schemas -join ', ')."
