<#
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7187
 #>

[CmdletBinding()]
param(
    [ValidateNotNullOrEmpty()]
    [string]$ProjectRoot = 'D:\Nexum Altivon\NexumAltivon.com',

    [ValidateNotNullOrEmpty()]
    [string]$ApiUrl = 'http://127.0.0.1:5010'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($PSVersionTable.PSEdition -ne 'Core' -or $PSVersionTable.PSVersion.Major -lt 7) {
    throw 'Esta validacao carrega assemblies .NET 8 da API oficial e deve ser executada com pwsh 7 ou superior.'
}

function Add-DbParameter {
    param([object]$Command, [string]$Name, [object]$Value)
    $null = $Command.Parameters.AddWithValue($Name, $(if ($null -eq $Value) { [DBNull]::Value } else { $Value }))
}

function Invoke-DbScalar {
    param([object]$Connection, [string]$Sql, [hashtable]$Parameters = @{}, [object]$Transaction = $null)
    $command = $Connection.CreateCommand()
    try {
        $command.CommandText = $Sql
        $command.CommandTimeout = 60
        if ($null -ne $Transaction) { $command.Transaction = $Transaction }
        foreach ($entry in $Parameters.GetEnumerator()) {
            Add-DbParameter -Command $command -Name $entry.Key -Value $entry.Value
        }
        return $command.ExecuteScalar()
    }
    finally {
        $command.Dispose()
    }
}

function Invoke-DbNonQuery {
    param([object]$Connection, [string]$Sql, [hashtable]$Parameters = @{}, [object]$Transaction = $null)
    $command = $Connection.CreateCommand()
    try {
        $command.CommandText = $Sql
        $command.CommandTimeout = 60
        if ($null -ne $Transaction) { $command.Transaction = $Transaction }
        foreach ($entry in $Parameters.GetEnumerator()) {
            Add-DbParameter -Command $command -Name $entry.Key -Value $entry.Value
        }
        return $command.ExecuteNonQuery()
    }
    finally {
        $command.Dispose()
    }
}

function Invoke-JsonRequest {
    param(
        [System.Net.Http.HttpClient]$Client,
        [string]$Method,
        [string]$Uri,
        [object]$Body = $null,
        [string]$BearerToken = $null
    )

    $request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::new($Method), $Uri)
    try {
        if (-not [string]::IsNullOrWhiteSpace($BearerToken)) {
            $request.Headers.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new('Bearer', $BearerToken)
        }
        $request.Headers.UserAgent.ParseAdd('GenesisGest-Clientes-Validation/1.1.5.7187')
        if ($null -ne $Body) {
            $json = $Body | ConvertTo-Json -Depth 10 -Compress
            $request.Content = [System.Net.Http.StringContent]::new($json, [Text.Encoding]::UTF8, 'application/json')
        }

        $response = $Client.SendAsync($request).GetAwaiter().GetResult()
        try {
            $bodyText = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            $parsed = $null
            if (-not [string]::IsNullOrWhiteSpace($bodyText)) {
                try { $parsed = $bodyText | ConvertFrom-Json } catch { $parsed = $null }
            }
            return [pscustomobject]@{ StatusCode = [int]$response.StatusCode; Body = $bodyText; Json = $parsed }
        }
        finally {
            $response.Dispose()
        }
    }
    finally {
        $request.Dispose()
    }
}

function Assert-StatusCode {
    param([pscustomobject]$Response, [int[]]$Expected, [string]$Operation)
    if ($Expected -notcontains $Response.StatusCode) {
        throw "$Operation retornou HTTP $($Response.StatusCode), esperado $($Expected -join '/'). Corpo: $($Response.Body)"
    }
}

function Assert-Value {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { throw $Message }
}

function New-ValidCnpj {
    param([string]$Seed)
    $numericSeed = [Convert]::ToUInt64($Seed.Substring(0, 12), 16) % 100000000
    $base = $numericSeed.ToString('D8', [CultureInfo]::InvariantCulture) + '0001'
    $digits = [Collections.Generic.List[int]]::new()
    foreach ($character in $base.ToCharArray()) { $digits.Add([int]::Parse([string]$character, [CultureInfo]::InvariantCulture)) }

    foreach ($weights in @(@(5,4,3,2,9,8,7,6,5,4,3,2), @(6,5,4,3,2,9,8,7,6,5,4,3,2))) {
        $sum = 0
        for ($index = 0; $index -lt $weights.Count; $index++) { $sum += $digits[$index] * $weights[$index] }
        $digit = 11 - ($sum % 11)
        if ($digit -ge 10) { $digit = 0 }
        $digits.Add($digit)
    }

    return -join $digits
}

$resolvedRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$privateConfigPath = Join-Path $resolvedRoot 'runtime\api-24h\api.env.ps1'
$connectorPath = Join-Path $resolvedRoot 'runtime\api-24h\api\MySqlConnector.dll'
$bcryptPath = Join-Path $resolvedRoot 'runtime\api-24h\api\BCrypt.Net-Next.dll'
$backupScript = Join-Path $resolvedRoot 'scripts\server\backup-tabelas-mysql-oficial.ps1'
foreach ($requiredPath in @($privateConfigPath, $connectorPath, $bcryptPath, $backupScript)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) { throw "Dependencia obrigatoria ausente: $requiredPath" }
}

. $privateConfigPath
Add-Type -AssemblyName System.Net.Http
Add-Type -Path $connectorPath
Add-Type -Path $bcryptPath

$connectionString = [Environment]::GetEnvironmentVariable('ConnectionStrings__DefaultConnection', 'Process')
if ([string]::IsNullOrWhiteSpace($connectionString)) {
    throw 'A conexao oficial DefaultConnection deve estar configurada no ambiente privado.'
}

$runGuid = [Guid]::NewGuid().ToString('N')
$runId = '{0}-{1}' -f (Get-Date -Format 'yyyyMMddHHmmss'), $runGuid.Substring(0, 8)
$marker = "CLIENTE-VALIDACAO-$runId"
$tenantId = '00000000-0000-0000-0000-000000000001'
$userEmail = "controle.cliente.$runId@nexumaltivon.com"
$restrictedUserEmail = "vendedor.cliente.$runId@nexumaltivon.com"
$customerEmail = "cliente.$runId@nexumaltivon.com"
$cnpj = New-ValidCnpj -Seed $runGuid
$passwordBytes = New-Object byte[] 32
[Security.Cryptography.RandomNumberGenerator]::Fill($passwordBytes)
$password = [Convert]::ToBase64String($passwordBytes)
$passwordHash = [BCrypt.Net.BCrypt]::HashPassword($password, 12)
$apiBase = $ApiUrl.TrimEnd('/')
$backupDirectory = Join-Path $resolvedRoot 'runtime\api-24h\task-backups'
$backupPath = Join-Path $backupDirectory "$marker-nexum.sql"

& $backupScript -ProjectRoot $resolvedRoot -ConnectionName DefaultConnection -OutputPath $backupPath -Tables @(
    'usuarios', 'clientes', 'enderecos', 'logs_auditoria') | Out-Null

$connection = [MySqlConnector.MySqlConnection]::new($connectionString)
$client = [System.Net.Http.HttpClient]::new()
$client.Timeout = [TimeSpan]::FromSeconds(60)
$userId = 0
$restrictedUserId = 0
$customerId = 0
$addressId = 0
$auditCount = 0
$residualRows = -1
$validationSucceeded = $false
$validationError = $null

try {
    $connection.Open()
    $null = Invoke-DbNonQuery -Connection $connection -Sql @'
INSERT INTO usuarios
    (nome, email, senha_hash, perfil, ativo, tenant_id, row_version, is_deleted, created_at, updated_at)
VALUES
    (@nome, @email, @senha, 'SuperAdmin', 1, @tenantId, UNHEX(REPLACE(UUID(), '-', '')), 0, UTC_TIMESTAMP(), UTC_TIMESTAMP())
'@ -Parameters @{ '@nome' = $marker; '@email' = $userEmail; '@senha' = $passwordHash; '@tenantId' = $tenantId }
    $userId = [int](Invoke-DbScalar -Connection $connection -Sql 'SELECT LAST_INSERT_ID()')

    $null = Invoke-DbNonQuery -Connection $connection -Sql @'
INSERT INTO usuarios
    (nome, email, senha_hash, perfil, ativo, tenant_id, row_version, is_deleted, created_at, updated_at)
VALUES
    (@nome, @email, @senha, 'Vendedor', 1, @tenantId, UNHEX(REPLACE(UUID(), '-', '')), 0, UTC_TIMESTAMP(), UTC_TIMESTAMP())
'@ -Parameters @{ '@nome' = "$marker VENDEDOR"; '@email' = $restrictedUserEmail; '@senha' = $passwordHash; '@tenantId' = $tenantId }
    $restrictedUserId = [int](Invoke-DbScalar -Connection $connection -Sql 'SELECT LAST_INSERT_ID()')

    $restrictedLogin = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/auth/login" -Body @{ email = $restrictedUserEmail; senha = $password }
    Assert-StatusCode -Response $restrictedLogin -Expected @(200) -Operation 'Login controlado sem permissao gerencial'
    $restrictedToken = [string]$restrictedLogin.Json.dados.token
    Assert-Value -Condition (-not [string]::IsNullOrWhiteSpace($restrictedToken)) -Message 'Login restrito nao retornou token JWT.'
    $restrictedRead = Invoke-JsonRequest -Client $client -Method 'GET' -Uri "$apiBase/api/clientes" -BearerToken $restrictedToken
    Assert-StatusCode -Response $restrictedRead -Expected @(403) -Operation 'Bloqueio RBAC da gestao de clientes'

    $login = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/auth/login" -Body @{ email = $userEmail; senha = $password }
    Assert-StatusCode -Response $login -Expected @(200) -Operation 'Login controlado de clientes'
    $token = [string]$login.Json.dados.token
    Assert-Value -Condition (-not [string]::IsNullOrWhiteSpace($token)) -Message 'Login nao retornou token JWT.'

    $createBody = @{
        nome = "$marker PARTICIPACOES LTDA"
        email = $customerEmail
        cpf = $cnpj
        telefone = '1130304040'
        newsletter = $true
        rgIe = 'ISENTO'
        dataNascimento = '2004-04-01T00:00:00Z'
        whatsapp = '11999994040'
        vip = $false
        pontosFidelidade = 10
        status = 'Ativo'
        tipo = 'PJ'
    }
    $created = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/clientes/admin" -BearerToken $token -Body $createBody
    Assert-StatusCode -Response $created -Expected @(201) -Operation 'Criacao administrativa do cliente'
    $customerId = [int]$created.Json.dados.id
    $initialRowVersion = [string]$created.Json.dados.rowVersion
    Assert-Value -Condition ($customerId -gt 0 -and -not [string]::IsNullOrWhiteSpace($initialRowVersion)) -Message 'Criacao do cliente nao retornou ID e RowVersion validos.'

    $readCreated = Invoke-JsonRequest -Client $client -Method 'GET' -Uri "$apiBase/api/clientes/$customerId" -BearerToken $token
    Assert-StatusCode -Response $readCreated -Expected @(200) -Operation 'Releitura do cliente criado'
    Assert-Value -Condition ([string]$readCreated.Json.dados.rowVersion -eq $initialRowVersion) -Message 'Releitura do cliente criado divergiu da versao retornada.'

    $updateBody = $createBody.Clone()
    $updateBody['nome'] = "$marker PARTICIPACOES ATUALIZADA"
    $updateBody['vip'] = $true
    $updateBody['pontosFidelidade'] = 480
    $updateBody['rowVersion'] = $initialRowVersion
    $updated = Invoke-JsonRequest -Client $client -Method 'PUT' -Uri "$apiBase/api/clientes/$customerId" -BearerToken $token -Body $updateBody
    Assert-StatusCode -Response $updated -Expected @(200) -Operation 'Atualizacao administrativa do cliente'
    $updatedRowVersion = [string]$updated.Json.dados.rowVersion
    Assert-Value -Condition (-not [string]::IsNullOrWhiteSpace($updatedRowVersion) -and $updatedRowVersion -ne $initialRowVersion) -Message 'Atualizacao do cliente nao gerou nova RowVersion.'

    $staleUpdate = Invoke-JsonRequest -Client $client -Method 'PUT' -Uri "$apiBase/api/clientes/$customerId" -BearerToken $token -Body $updateBody
    Assert-StatusCode -Response $staleUpdate -Expected @(409) -Operation 'Rejeicao de versao antiga do cliente'

    $addressCreated = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/clientes/$customerId/enderecos" -BearerToken $token -Body @{
        apelido = 'Sede administrativa'
        tipo = 'Ambos'
        cep = '01001000'
        logradouro = 'Praca da Se'
        numero = '100'
        complemento = 'Sala 18'
        bairro = 'Se'
        cidade = 'Sao Paulo'
        estado = 'SP'
        pais = 'Brasil'
        padrao = $true
    }
    Assert-StatusCode -Response $addressCreated -Expected @(201) -Operation 'Criacao do endereco do cliente'
    $addressId = [int]$addressCreated.Json.dados.id
    $addressInitialVersion = [string]$addressCreated.Json.dados.rowVersion
    Assert-Value -Condition ($addressId -gt 0 -and -not [string]::IsNullOrWhiteSpace($addressInitialVersion)) -Message 'Endereco criado sem ID ou RowVersion.'

    $addressesRead = Invoke-JsonRequest -Client $client -Method 'GET' -Uri "$apiBase/api/clientes/$customerId/enderecos" -BearerToken $token
    Assert-StatusCode -Response $addressesRead -Expected @(200) -Operation 'Releitura dos enderecos'
    $readAddress = @($addressesRead.Json.dados | Where-Object { [int]$_.id -eq $addressId })
    Assert-Value -Condition ($readAddress.Count -eq 1 -and [string]$readAddress[0].rowVersion -eq $addressInitialVersion) -Message 'Releitura do endereco criado divergiu da versao retornada.'

    $addressUpdateBody = @{
        apelido = 'Sede financeira'
        tipo = 'Cobranca'
        cep = '01001000'
        logradouro = 'Praca da Se'
        numero = '101'
        complemento = 'Sala 19'
        bairro = 'Se'
        cidade = 'Sao Paulo'
        estado = 'SP'
        pais = 'Brasil'
        padrao = $true
        rowVersion = $addressInitialVersion
    }
    $addressUpdated = Invoke-JsonRequest -Client $client -Method 'PUT' -Uri "$apiBase/api/clientes/$customerId/enderecos/$addressId" -BearerToken $token -Body $addressUpdateBody
    Assert-StatusCode -Response $addressUpdated -Expected @(200) -Operation 'Atualizacao do endereco'
    $addressUpdatedVersion = [string]$addressUpdated.Json.dados.rowVersion
    Assert-Value -Condition (-not [string]::IsNullOrWhiteSpace($addressUpdatedVersion) -and $addressUpdatedVersion -ne $addressInitialVersion) -Message 'Atualizacao do endereco nao gerou nova RowVersion.'

    $staleAddress = Invoke-JsonRequest -Client $client -Method 'PUT' -Uri "$apiBase/api/clientes/$customerId/enderecos/$addressId" -BearerToken $token -Body $addressUpdateBody
    Assert-StatusCode -Response $staleAddress -Expected @(409) -Operation 'Rejeicao de versao antiga do endereco'

    $encodedAddressVersion = [Uri]::EscapeDataString($addressUpdatedVersion)
    $addressDeleted = Invoke-JsonRequest -Client $client -Method 'DELETE' -Uri "$apiBase/api/clientes/$customerId/enderecos/$addressId`?rowVersion=$encodedAddressVersion" -BearerToken $token
    Assert-StatusCode -Response $addressDeleted -Expected @(204) -Operation 'Soft-delete do endereco'

    $addressesAfterDelete = Invoke-JsonRequest -Client $client -Method 'GET' -Uri "$apiBase/api/clientes/$customerId/enderecos" -BearerToken $token
    Assert-StatusCode -Response $addressesAfterDelete -Expected @(200) -Operation 'Releitura dos enderecos apos soft-delete'
    Assert-Value -Condition (@($addressesAfterDelete.Json.dados | Where-Object { [int]$_.id -eq $addressId }).Count -eq 0) -Message 'Endereco arquivado permaneceu na listagem operacional.'

    $customerAudit = Invoke-JsonRequest -Client $client -Method 'GET' -Uri "$apiBase/api/auditoria?tabela=clientes" -BearerToken $token
    $addressAudit = Invoke-JsonRequest -Client $client -Method 'GET' -Uri "$apiBase/api/auditoria?tabela=enderecos" -BearerToken $token
    Assert-StatusCode -Response $customerAudit -Expected @(200) -Operation 'Auditoria do cliente'
    Assert-StatusCode -Response $addressAudit -Expected @(200) -Operation 'Auditoria dos enderecos'
    $auditCount = @($customerAudit.Json.dados | Where-Object { [int]$_.registroId -eq $customerId }).Count +
        @($addressAudit.Json.dados | Where-Object { [int]$_.registroId -eq $addressId }).Count
    Assert-Value -Condition ($auditCount -ge 5) -Message "Auditoria insuficiente: $auditCount evento(s), esperado ao menos 5."

    $persistedCustomerCount = [int](Invoke-DbScalar -Connection $connection -Sql @'
SELECT COUNT(*) FROM clientes
WHERE id=@customerId AND tenant_id=@tenantId AND is_deleted=0 AND nome=@name AND vip=1 AND pontos_fidelidade=480
'@ -Parameters @{ '@customerId' = $customerId; '@tenantId' = $tenantId; '@name' = "$marker PARTICIPACOES ATUALIZADA" })
    $softDeletedAddressCount = [int](Invoke-DbScalar -Connection $connection -Sql @'
SELECT COUNT(*) FROM enderecos
WHERE id=@addressId AND cliente_id=@customerId AND tenant_id=@tenantId AND is_deleted=1 AND deleted_at IS NOT NULL
'@ -Parameters @{ '@addressId' = $addressId; '@customerId' = $customerId; '@tenantId' = $tenantId })
    Assert-Value -Condition ($persistedCustomerCount -eq 1) -Message 'MySQL nao confirmou os campos atualizados do cliente.'
    Assert-Value -Condition ($softDeletedAddressCount -eq 1) -Message 'MySQL nao confirmou o soft-delete do endereco.'

    $validationSucceeded = $true
}
catch {
    $validationError = $_.Exception.Message
}
finally {
    try {
        if ($connection.State -ne [Data.ConnectionState]::Open) { $connection.Open() }
        $cleanup = $connection.BeginTransaction()
        try {
            if ($addressId -gt 0) {
                $null = Invoke-DbNonQuery -Connection $connection -Transaction $cleanup -Sql "DELETE FROM logs_auditoria WHERE tabela='enderecos' AND registro_id=@id" -Parameters @{ '@id' = $addressId }
                $null = Invoke-DbNonQuery -Connection $connection -Transaction $cleanup -Sql 'DELETE FROM enderecos WHERE id=@id' -Parameters @{ '@id' = $addressId }
            }
            if ($customerId -gt 0) {
                $null = Invoke-DbNonQuery -Connection $connection -Transaction $cleanup -Sql "DELETE FROM logs_auditoria WHERE tabela='clientes' AND registro_id=@id" -Parameters @{ '@id' = $customerId }
                $null = Invoke-DbNonQuery -Connection $connection -Transaction $cleanup -Sql 'DELETE FROM clientes WHERE id=@id' -Parameters @{ '@id' = $customerId }
            }
            if ($userId -gt 0) {
                $null = Invoke-DbNonQuery -Connection $connection -Transaction $cleanup -Sql 'DELETE FROM usuarios WHERE id=@id' -Parameters @{ '@id' = $userId }
            }
            if ($restrictedUserId -gt 0) {
                $null = Invoke-DbNonQuery -Connection $connection -Transaction $cleanup -Sql 'DELETE FROM usuarios WHERE id=@id' -Parameters @{ '@id' = $restrictedUserId }
            }
            $cleanup.Commit()
        }
        catch {
            $cleanup.Rollback()
            throw
        }
        finally {
            $cleanup.Dispose()
        }

        $residualRows = [int](Invoke-DbScalar -Connection $connection -Sql @'
SELECT
    (SELECT COUNT(*) FROM usuarios WHERE email=@userEmail) +
    (SELECT COUNT(*) FROM usuarios WHERE email=@restrictedUserEmail) +
    (SELECT COUNT(*) FROM clientes WHERE email=@customerEmail OR cpf_cnpj=@cnpj) +
    (SELECT COUNT(*) FROM enderecos WHERE id=@addressId) +
    (SELECT COUNT(*) FROM logs_auditoria WHERE (tabela='clientes' AND registro_id=@customerId) OR (tabela='enderecos' AND registro_id=@addressId))
'@ -Parameters @{
            '@userEmail' = $userEmail
            '@restrictedUserEmail' = $restrictedUserEmail
            '@customerEmail' = $customerEmail
            '@cnpj' = $cnpj
            '@addressId' = $addressId
            '@customerId' = $customerId
        })
    }
    catch {
        $validationSucceeded = $false
        $cleanupError = "Falha na limpeza controlada: $($_.Exception.Message)"
        $validationError = if ([string]::IsNullOrWhiteSpace($validationError)) { $cleanupError } else { "$validationError | $cleanupError" }
    }
    finally {
        $password = $null
        [Array]::Clear($passwordBytes, 0, $passwordBytes.Length)
        $client.Dispose()
        $connection.Dispose()
    }
}

if ($residualRows -ne 0) {
    $validationSucceeded = $false
    $residualMessage = "A limpeza deixou $residualRows linha(s) controlada(s)."
    $validationError = if ([string]::IsNullOrWhiteSpace($validationError)) { $residualMessage } else { "$validationError | $residualMessage" }
}

$result = [pscustomobject]@{
    CheckedAt = (Get-Date).ToString('o')
    ValidationSucceeded = $validationSucceeded
    ApiUrl = $apiBase
    CustomerId = $customerId
    AddressId = $addressId
    AuditCount = $auditCount
    ResidualRows = $residualRows
    NexumBackup = $backupPath
    BackupSha256 = (Get-FileHash -LiteralPath $backupPath -Algorithm SHA256).Hash
    Error = $validationError
}
$result | Format-List

if (-not $validationSucceeded) {
    throw "Validacao ponta a ponta de clientes falhou. $validationError"
}
