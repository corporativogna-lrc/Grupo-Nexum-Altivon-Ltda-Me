<#
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7186
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
        $request.Headers.UserAgent.ParseAdd('GenesisGest-Fornecedores-Validation/1.1.5.7186')
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
$marker = "FORNECEDOR-VALIDACAO-$runId"
$tenantId = '00000000-0000-0000-0000-000000000001'
$email = "controle.fornecedor.$runId@nexumaltivon.com"
$supplierEmail = "compras.$runId@nexumaltivon.com"
$contactEmail = "contato.$runId@nexumaltivon.com"
$cnpj = New-ValidCnpj -Seed $runGuid
$passwordBytes = New-Object byte[] 32
[Security.Cryptography.RandomNumberGenerator]::Fill($passwordBytes)
$password = [Convert]::ToBase64String($passwordBytes)
$passwordHash = [BCrypt.Net.BCrypt]::HashPassword($password, 12)
$apiBase = $ApiUrl.TrimEnd('/')
$backupDirectory = Join-Path $resolvedRoot 'runtime\api-24h\task-backups'
$backupPath = Join-Path $backupDirectory "$marker-nexum.sql"

& $backupScript -ProjectRoot $resolvedRoot -ConnectionName DefaultConnection -OutputPath $backupPath -Tables @(
    'usuarios', 'lojas', 'fornecedores', 'md_fornecedor_contatos', 'logs_auditoria') | Out-Null

$connection = [MySqlConnector.MySqlConnection]::new($connectionString)
$client = [System.Net.Http.HttpClient]::new()
$client.Timeout = [TimeSpan]::FromSeconds(60)
$userId = 0
$supplierId = 0
$contactId = 0
$auditCount = 0
$residualRows = -1
$validationSucceeded = $false
$validationError = $null

try {
    $connection.Open()
    $storeId = [int](Invoke-DbScalar -Connection $connection -Sql @'
SELECT id FROM lojas
WHERE tenant_id = @tenantId AND is_deleted = 0 AND ativa = 1
ORDER BY ordem_exibicao, id
LIMIT 1
'@ -Parameters @{ '@tenantId' = $tenantId })
    Assert-Value -Condition ($storeId -gt 0) -Message 'Nenhuma loja ativa do tenant operacional foi encontrada.'

    $null = Invoke-DbNonQuery -Connection $connection -Sql @'
INSERT INTO usuarios
    (nome, email, senha_hash, perfil, ativo, tenant_id, row_version, is_deleted, created_at, updated_at)
VALUES
    (@nome, @email, @senha, 'SuperAdmin', 1, @tenantId, UNHEX(REPLACE(UUID(), '-', '')), 0, UTC_TIMESTAMP(), UTC_TIMESTAMP())
'@ -Parameters @{ '@nome' = $marker; '@email' = $email; '@senha' = $passwordHash; '@tenantId' = $tenantId }
    $userId = [int](Invoke-DbScalar -Connection $connection -Sql 'SELECT LAST_INSERT_ID()')

    $login = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/auth/login" -Body @{ email = $email; senha = $password }
    Assert-StatusCode -Response $login -Expected @(200) -Operation 'Login controlado de fornecedores'
    $token = [string]$login.Json.dados.token
    Assert-Value -Condition (-not [string]::IsNullOrWhiteSpace($token)) -Message 'Login nao retornou token JWT.'

    $createBody = @{
        nome = "$marker LTDA"
        documento = $cnpj
        email = $supplierEmail
        telefone = '1130304040'
        categoria = 'Insumos operacionais'
        nomeFantasia = $marker
        ie = 'ISENTO'
        whatsapp = '11999990000'
        endereco = 'Avenida Corporativa, 100'
        cidade = 'Sao Paulo'
        estado = 'SP'
        cep = '01001000'
        lojaVinculadaId = $storeId
        comissaoPercentual = 2.50
        prazoEntregaDias = 5
        status = 'Ativo'
        observacoes = $marker
    }
    $created = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/fornecedores" -BearerToken $token -Body $createBody
    Assert-StatusCode -Response $created -Expected @(201) -Operation 'Criacao do fornecedor'
    $supplierId = [int]$created.Json.dados.id
    $initialRowVersion = [string]$created.Json.dados.rowVersion
    Assert-Value -Condition ($supplierId -gt 0 -and -not [string]::IsNullOrWhiteSpace($initialRowVersion)) -Message 'Criacao nao retornou ID e RowVersion validos.'

    $readCreated = Invoke-JsonRequest -Client $client -Method 'GET' -Uri "$apiBase/api/fornecedores/$supplierId" -BearerToken $token
    Assert-StatusCode -Response $readCreated -Expected @(200) -Operation 'Releitura do fornecedor criado'
    Assert-Value -Condition ([string]$readCreated.Json.dados.rowVersion -eq $initialRowVersion) -Message 'Releitura do fornecedor criado divergiu da versao retornada.'

    $updateBody = $createBody.Clone()
    $updateBody['nomeFantasia'] = "$marker ATUALIZADO"
    $updateBody['prazoEntregaDias'] = 9
    $updateBody['rowVersion'] = $initialRowVersion
    $updated = Invoke-JsonRequest -Client $client -Method 'PUT' -Uri "$apiBase/api/fornecedores/$supplierId" -BearerToken $token -Body $updateBody
    Assert-StatusCode -Response $updated -Expected @(200) -Operation 'Atualizacao do fornecedor'
    $updatedRowVersion = [string]$updated.Json.dados.rowVersion
    Assert-Value -Condition (-not [string]::IsNullOrWhiteSpace($updatedRowVersion) -and $updatedRowVersion -ne $initialRowVersion) -Message 'Atualizacao nao gerou uma nova RowVersion.'

    $staleUpdate = Invoke-JsonRequest -Client $client -Method 'PUT' -Uri "$apiBase/api/fornecedores/$supplierId" -BearerToken $token -Body $updateBody
    Assert-StatusCode -Response $staleUpdate -Expected @(409) -Operation 'Rejeicao de versao antiga do fornecedor'

    $contactCreated = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/fornecedores/$supplierId/contatos" -BearerToken $token -Body @{
        nome = "$marker COMERCIAL"
        cargo = 'Compras'
        email = $contactEmail
        telefone = '1130305050'
        celular = '11999995050'
        principal = $true
        ativo = $true
    }
    Assert-StatusCode -Response $contactCreated -Expected @(201) -Operation 'Criacao do contato do fornecedor'
    $contactId = [int]$contactCreated.Json.dados.id
    $contactInitialVersion = [string]$contactCreated.Json.dados.rowVersion
    Assert-Value -Condition ($contactId -gt 0 -and -not [string]::IsNullOrWhiteSpace($contactInitialVersion)) -Message 'Contato criado sem ID ou RowVersion.'

    $contactUpdated = Invoke-JsonRequest -Client $client -Method 'PUT' -Uri "$apiBase/api/fornecedores/$supplierId/contatos/$contactId" -BearerToken $token -Body @{
        nome = "$marker FINANCEIRO"
        cargo = 'Financeiro'
        email = $contactEmail
        telefone = '1130306060'
        celular = '11999996060'
        principal = $true
        ativo = $true
        rowVersion = $contactInitialVersion
    }
    Assert-StatusCode -Response $contactUpdated -Expected @(200) -Operation 'Atualizacao do contato do fornecedor'
    $contactUpdatedVersion = [string]$contactUpdated.Json.dados.rowVersion
    Assert-Value -Condition ($contactUpdatedVersion -ne $contactInitialVersion) -Message 'Atualizacao do contato nao gerou nova RowVersion.'

    $staleContact = Invoke-JsonRequest -Client $client -Method 'PUT' -Uri "$apiBase/api/fornecedores/$supplierId/contatos/$contactId" -BearerToken $token -Body @{
        nome = "$marker VERSAO ANTIGA"
        principal = $true
        ativo = $true
        rowVersion = $contactInitialVersion
    }
    Assert-StatusCode -Response $staleContact -Expected @(409) -Operation 'Rejeicao de versao antiga do contato'

    $encodedContactVersion = [Uri]::EscapeDataString($contactUpdatedVersion)
    $contactDeleted = Invoke-JsonRequest -Client $client -Method 'DELETE' -Uri "$apiBase/api/fornecedores/$supplierId/contatos/$contactId`?rowVersion=$encodedContactVersion" -BearerToken $token
    Assert-StatusCode -Response $contactDeleted -Expected @(200) -Operation 'Soft-delete do contato'

    $contactsAfterDelete = Invoke-JsonRequest -Client $client -Method 'GET' -Uri "$apiBase/api/fornecedores/$supplierId/contatos" -BearerToken $token
    Assert-StatusCode -Response $contactsAfterDelete -Expected @(200) -Operation 'Releitura dos contatos apos soft-delete'
    Assert-Value -Condition (@($contactsAfterDelete.Json.dados | Where-Object { [int]$_.id -eq $contactId }).Count -eq 0) -Message 'Contato desativado permaneceu na listagem operacional.'

    $supplierAudit = Invoke-JsonRequest -Client $client -Method 'GET' -Uri "$apiBase/api/auditoria?tabela=fornecedores" -BearerToken $token
    $contactAudit = Invoke-JsonRequest -Client $client -Method 'GET' -Uri "$apiBase/api/auditoria?tabela=md_fornecedor_contatos" -BearerToken $token
    Assert-StatusCode -Response $supplierAudit -Expected @(200) -Operation 'Auditoria do fornecedor'
    Assert-StatusCode -Response $contactAudit -Expected @(200) -Operation 'Auditoria dos contatos'
    $auditCount = @($supplierAudit.Json.dados | Where-Object { [int]$_.registroId -eq $supplierId }).Count +
        @($contactAudit.Json.dados | Where-Object { [int]$_.registroId -eq $contactId }).Count
    Assert-Value -Condition ($auditCount -ge 5) -Message "Auditoria insuficiente: $auditCount evento(s), esperado ao menos 5."

    $persistedSupplierCount = [int](Invoke-DbScalar -Connection $connection -Sql @'
SELECT COUNT(*) FROM fornecedores
WHERE id=@supplierId AND tenant_id=@tenantId AND is_deleted=0 AND nome_fantasia=@name AND prazo_entrega_dias=9
'@ -Parameters @{ '@supplierId' = $supplierId; '@tenantId' = $tenantId; '@name' = "$marker ATUALIZADO" })
    $softDeletedContactCount = [int](Invoke-DbScalar -Connection $connection -Sql @'
SELECT COUNT(*) FROM md_fornecedor_contatos
WHERE fco_id=@contactId AND fco_fornecedor_id=@supplierId AND fco_tenant_id=@tenantId AND fco_is_deleted=1 AND fco_ativo=0
'@ -Parameters @{ '@contactId' = $contactId; '@supplierId' = $supplierId; '@tenantId' = $tenantId })
    Assert-Value -Condition ($persistedSupplierCount -eq 1) -Message 'MySQL nao confirmou os campos atualizados do fornecedor.'
    Assert-Value -Condition ($softDeletedContactCount -eq 1) -Message 'MySQL nao confirmou o soft-delete do contato.'

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
            if ($contactId -gt 0) {
                $null = Invoke-DbNonQuery -Connection $connection -Transaction $cleanup -Sql "DELETE FROM logs_auditoria WHERE tabela='md_fornecedor_contatos' AND registro_id=@id" -Parameters @{ '@id' = $contactId }
                $null = Invoke-DbNonQuery -Connection $connection -Transaction $cleanup -Sql 'DELETE FROM md_fornecedor_contatos WHERE fco_id=@id' -Parameters @{ '@id' = $contactId }
            }
            if ($supplierId -gt 0) {
                $null = Invoke-DbNonQuery -Connection $connection -Transaction $cleanup -Sql "DELETE FROM logs_auditoria WHERE tabela='fornecedores' AND registro_id=@id" -Parameters @{ '@id' = $supplierId }
                $null = Invoke-DbNonQuery -Connection $connection -Transaction $cleanup -Sql 'DELETE FROM fornecedores WHERE id=@id' -Parameters @{ '@id' = $supplierId }
            }
            if ($userId -gt 0) {
                $null = Invoke-DbNonQuery -Connection $connection -Transaction $cleanup -Sql 'DELETE FROM usuarios WHERE id=@id' -Parameters @{ '@id' = $userId }
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
    (SELECT COUNT(*) FROM fornecedores WHERE cnpj=@cnpj OR observacoes=@marker) +
    (SELECT COUNT(*) FROM md_fornecedor_contatos WHERE fco_email=@contactEmail) +
    (SELECT COUNT(*) FROM logs_auditoria WHERE (tabela='fornecedores' AND registro_id=@supplierId) OR (tabela='md_fornecedor_contatos' AND registro_id=@contactId))
'@ -Parameters @{
            '@userEmail' = $email
            '@cnpj' = $cnpj
            '@marker' = $marker
            '@contactEmail' = $contactEmail
            '@supplierId' = $supplierId
            '@contactId' = $contactId
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
    SupplierId = $supplierId
    ContactId = $contactId
    AuditCount = $auditCount
    ResidualRows = $residualRows
    NexumBackup = $backupPath
    BackupSha256 = (Get-FileHash -LiteralPath $backupPath -Algorithm SHA256).Hash
    Error = $validationError
}
$result | Format-List

if (-not $validationSucceeded) {
    throw "Validacao ponta a ponta de fornecedores falhou. $validationError"
}
