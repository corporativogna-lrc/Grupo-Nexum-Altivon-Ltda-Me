<#
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7185
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
    param(
        [object]$Command,
        [string]$Name,
        [object]$Value
    )

    $parameterValue = if ($null -eq $Value) { [DBNull]::Value } else { $Value }
    $null = $Command.Parameters.AddWithValue($Name, $parameterValue)
}

function Invoke-DbScalar {
    param(
        [object]$Connection,
        [string]$Sql,
        [hashtable]$Parameters = @{},
        [object]$Transaction = $null
    )

    $command = $Connection.CreateCommand()
    try {
        $command.CommandText = $Sql
        $command.CommandTimeout = 60
        if ($null -ne $Transaction) {
            $command.Transaction = $Transaction
        }

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
    param(
        [object]$Connection,
        [string]$Sql,
        [hashtable]$Parameters = @{},
        [object]$Transaction = $null
    )

    $command = $Connection.CreateCommand()
    try {
        $command.CommandText = $Sql
        $command.CommandTimeout = 60
        if ($null -ne $Transaction) {
            $command.Transaction = $Transaction
        }

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

    $request = [System.Net.Http.HttpRequestMessage]::new(
        [System.Net.Http.HttpMethod]::new($Method),
        $Uri)
    try {
        if (-not [string]::IsNullOrWhiteSpace($BearerToken)) {
            $request.Headers.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new('Bearer', $BearerToken)
        }

        $request.Headers.UserAgent.ParseAdd('GenesisGest-Compras-Validation/1.1.5.7185')
        if ($null -ne $Body) {
            $json = $Body | ConvertTo-Json -Depth 12 -Compress
            $request.Content = [System.Net.Http.StringContent]::new($json, [Text.Encoding]::UTF8, 'application/json')
        }

        $response = $Client.SendAsync($request).GetAwaiter().GetResult()
        try {
            $responseBody = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            $parsed = $null
            if (-not [string]::IsNullOrWhiteSpace($responseBody)) {
                try {
                    $parsed = $responseBody | ConvertFrom-Json
                }
                catch {
                    $parsed = $null
                }
            }

            return [pscustomobject]@{
                StatusCode = [int]$response.StatusCode
                Body = $responseBody
                Json = $parsed
            }
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
    param(
        [pscustomobject]$Response,
        [int[]]$Expected,
        [string]$Operation
    )

    if ($Expected -notcontains $Response.StatusCode) {
        $safeBody = if ([string]::IsNullOrWhiteSpace($Response.Body)) { '<corpo vazio>' } else { $Response.Body }
        throw "$Operation retornou HTTP $($Response.StatusCode), esperado $($Expected -join '/'). Corpo: $safeBody"
    }
}

function Assert-Value {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

$resolvedRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$privateConfigPath = Join-Path $resolvedRoot 'runtime\api-24h\api.env.ps1'
$connectorPath = Join-Path $resolvedRoot 'runtime\api-24h\api\MySqlConnector.dll'
$bcryptPath = Join-Path $resolvedRoot 'runtime\api-24h\api\BCrypt.Net-Next.dll'
$backupScript = Join-Path $resolvedRoot 'scripts\server\backup-tabelas-mysql-oficial.ps1'

foreach ($requiredPath in @($privateConfigPath, $connectorPath, $bcryptPath, $backupScript)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Dependencia obrigatoria ausente: $requiredPath"
    }
}

. $privateConfigPath
Add-Type -AssemblyName System.Net.Http
Add-Type -Path $connectorPath
Add-Type -Path $bcryptPath

$defaultConnectionString = [Environment]::GetEnvironmentVariable('ConnectionStrings__DefaultConnection', 'Process')
$genesisConnectionString = [Environment]::GetEnvironmentVariable('ConnectionStrings__GenesisConnection', 'Process')
if ([string]::IsNullOrWhiteSpace($defaultConnectionString) -or [string]::IsNullOrWhiteSpace($genesisConnectionString)) {
    throw 'As conexoes oficiais DefaultConnection e GenesisConnection devem estar configuradas no ambiente privado.'
}

$runId = '{0}-{1}' -f (Get-Date -Format 'yyyyMMddHHmmss'), ([Guid]::NewGuid().ToString('N').Substring(0, 8))
$marker = "COMPRA-VALIDACAO-$runId"
$tenantId = '00000000-0000-0000-0000-000000000001'
$email = "controle.compras.$runId@nexumaltivon.com"
$passwordBytes = New-Object byte[] 32
$randomGenerator = [Security.Cryptography.RandomNumberGenerator]::Create()
try {
    $randomGenerator.GetBytes($passwordBytes)
}
finally {
    $randomGenerator.Dispose()
}
$password = [Convert]::ToBase64String($passwordBytes)
$passwordHash = [BCrypt.Net.BCrypt]::HashPassword($password, 12)
$apiBase = $ApiUrl.TrimEnd('/')

$backupDirectory = Join-Path $resolvedRoot 'runtime\api-24h\task-backups'
$nexumBackup = Join-Path $backupDirectory "$marker-nexum.sql"
$genesisBackup = Join-Path $backupDirectory "$marker-genesis.sql"

& $backupScript `
    -ProjectRoot $resolvedRoot `
    -ConnectionName DefaultConnection `
    -OutputPath $nexumBackup `
    -Tables @(
        'usuarios', 'fornecedores', 'produtos', 'financeiro', 'logs_auditoria', 'notificacoes',
        'compras_solicitacoes', 'compras_cotacoes', 'compras_pedidos', 'compras_pedido_itens',
        'compras_entradas', 'compras_entrada_itens', 'estoque_movimentos') | Out-Null

& $backupScript `
    -ProjectRoot $resolvedRoot `
    -ConnectionName GenesisConnection `
    -OutputPath $genesisBackup `
    -Tables @('erp_contas_pagar') | Out-Null

$nexumConnection = [MySqlConnector.MySqlConnection]::new($defaultConnectionString)
$genesisConnection = [MySqlConnector.MySqlConnection]::new($genesisConnectionString)
$client = [System.Net.Http.HttpClient]::new()
$client.Timeout = [TimeSpan]::FromSeconds(60)

$userId = 0
$supplierId = 0
$productId = 0
$requestId = 0
$quoteId = 0
$orderId = 0
$orderItemId = 0
$orderNumber = $null
$entryIds = [Collections.Generic.List[int]]::new()
$validationSucceeded = $false
$validationError = $null
$auditCount = 0
$residualRows = -1

try {
    $nexumConnection.Open()
    $genesisConnection.Open()

    $fixtureTransaction = $nexumConnection.BeginTransaction()
    try {
        $storeId = [int](Invoke-DbScalar -Connection $nexumConnection -Transaction $fixtureTransaction -Sql @'
SELECT id FROM lojas
WHERE tenant_id = @tenantId AND is_deleted = 0 AND ativa = 1
ORDER BY ordem_exibicao, id
LIMIT 1
'@ -Parameters @{ '@tenantId' = $tenantId })
        Assert-Value -Condition ($storeId -gt 0) -Message 'Nenhuma loja ativa do tenant operacional foi encontrada.'

        $null = Invoke-DbNonQuery -Connection $nexumConnection -Transaction $fixtureTransaction -Sql @'
INSERT INTO usuarios
    (nome, email, senha_hash, perfil, ativo, tenant_id, row_version, is_deleted, created_at, updated_at)
VALUES
    (@nome, @email, @senha, 'SuperAdmin', 1, @tenantId, UNHEX(REPLACE(UUID(), '-', '')), 0, UTC_TIMESTAMP(), UTC_TIMESTAMP())
'@ -Parameters @{
            '@nome' = $marker
            '@email' = $email
            '@senha' = $passwordHash
            '@tenantId' = $tenantId
        }
        $userId = [int](Invoke-DbScalar -Connection $nexumConnection -Transaction $fixtureTransaction -Sql 'SELECT LAST_INSERT_ID()')

        $null = Invoke-DbNonQuery -Connection $nexumConnection -Transaction $fixtureTransaction -Sql @'
INSERT INTO fornecedores
    (razao_social, nome_fantasia, email, segmento, prazo_entrega_dias, status,
     observacoes, tenant_id, row_version, is_deleted, created_at, updated_at)
VALUES
    (@marker, @marker, @email, 'Operacao interna', 3, 'Ativo', @marker,
     @tenantId, UNHEX(REPLACE(UUID(), '-', '')), 0, UTC_TIMESTAMP(), UTC_TIMESTAMP())
'@ -Parameters @{
            '@marker' = $marker
            '@email' = $email
            '@tenantId' = $tenantId
        }
        $supplierId = [int](Invoke-DbScalar -Connection $nexumConnection -Transaction $fixtureTransaction -Sql 'SELECT LAST_INSERT_ID()')

        $null = Invoke-DbNonQuery -Connection $nexumConnection -Transaction $fixtureTransaction -Sql @'
INSERT INTO produtos
    (loja_id, sku, nome, slug, preco, custo, estoque_minimo, estoque_atual, estoque_reservado,
     tipo_produto, fornecedor_id, ativo, tenant_id, row_version, is_deleted, created_at, updated_at)
VALUES
    (@lojaId, @sku, @nome, @slug, 50.00, 25.00, 0, 0, 0,
     'Proprio', @fornecedorId, 1, @tenantId, UNHEX(REPLACE(UUID(), '-', '')), 0, UTC_TIMESTAMP(), UTC_TIMESTAMP())
'@ -Parameters @{
            '@lojaId' = $storeId
            '@sku' = $marker
            '@nome' = $marker
            '@slug' = $marker.ToLowerInvariant()
            '@fornecedorId' = $supplierId
            '@tenantId' = $tenantId
        }
        $productId = [int](Invoke-DbScalar -Connection $nexumConnection -Transaction $fixtureTransaction -Sql 'SELECT LAST_INSERT_ID()')
        $fixtureTransaction.Commit()
    }
    catch {
        $fixtureTransaction.Rollback()
        throw
    }
    finally {
        $fixtureTransaction.Dispose()
    }

    $login = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/auth/login" -Body @{
        email = $email
        senha = $password
    }
    Assert-StatusCode -Response $login -Expected @(200) -Operation 'Login controlado de Compras'
    $token = [string]$login.Json.dados.token
    Assert-Value -Condition (-not [string]::IsNullOrWhiteSpace($token)) -Message 'Login nao retornou token JWT.'

    $panelBefore = Invoke-JsonRequest -Client $client -Method 'GET' -Uri "$apiBase/api/compras/painel" -BearerToken $token
    Assert-StatusCode -Response $panelBefore -Expected @(200) -Operation 'Leitura inicial do painel de Compras'

    $requestResponse = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/compras/solicitacoes" -BearerToken $token -Body @{
        produtoId = $productId
        produtoNome = $marker
        quantidade = 4
        origem = 'EstoqueFisico'
        finalidade = 'Validacao operacional controlada'
        prioridade = 'Normal'
        observacoes = $marker
    }
    Assert-StatusCode -Response $requestResponse -Expected @(201) -Operation 'Criacao da solicitacao de compra'
    $createdRequest = @($requestResponse.Json.dados.solicitacoes | Where-Object { $_.produtoId -eq $productId }) | Select-Object -First 1
    Assert-Value -Condition ($null -ne $createdRequest) -Message 'Solicitacao criada nao apareceu na releitura retornada pela API.'
    $requestId = [int]$createdRequest.id

    $quoteResponse = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/compras/cotacoes" -BearerToken $token -Body @{
        fornecedorId = $supplierId
        solicitacaoId = $requestId
        produtoId = $productId
        produtoNome = $marker
        quantidade = 4
        custoUnitario = 25.00
        origem = 'EstoqueFisico'
        finalidade = 'Validacao operacional controlada'
        prioridade = 'Normal'
        prazoEntregaDias = 3
        observacoes = $marker
    }
    Assert-StatusCode -Response $quoteResponse -Expected @(201) -Operation 'Registro da cotacao'
    $createdQuote = @($quoteResponse.Json.dados.cotacoes | Where-Object { $_.solicitacaoId -eq $requestId -and $_.fornecedorId -eq $supplierId }) | Select-Object -First 1
    Assert-Value -Condition ($null -ne $createdQuote) -Message 'Cotacao criada nao apareceu na releitura retornada pela API.'
    $quoteId = [int]$createdQuote.id

    foreach ($transition in @('EmAprovacao', 'Aprovada')) {
        $statusResponse = Invoke-JsonRequest -Client $client -Method 'PATCH' -Uri "$apiBase/api/compras/solicitacoes/$requestId/status" -BearerToken $token -Body @{
            status = $transition
            observacoes = $marker
        }
        Assert-StatusCode -Response $statusResponse -Expected @(200) -Operation "Transicao da solicitacao para $transition"
        $confirmedRequest = @($statusResponse.Json.dados.solicitacoes | Where-Object { $_.id -eq $requestId }) | Select-Object -First 1
        Assert-Value -Condition ($null -ne $confirmedRequest) -Message 'Solicitacao nao encontrada apos transicao.'
    }

    $orderResponse = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/compras/pedidos" -BearerToken $token -Body @{
        fornecedorId = $supplierId
        solicitacaoId = $requestId
        origem = 'EstoqueFisico'
        finalidade = 'Validacao operacional controlada'
        dataPrevistaEntrega = [DateTime]::UtcNow.AddDays(3).ToString('o')
        dataVencimento = [DateTime]::UtcNow.AddDays(7).ToString('o')
        meioPagamento = 'Transferencia bancaria'
        observacoes = $marker
        itens = @(@{
            produtoId = $productId
            produtoNome = $marker
            sku = $marker
            quantidade = 4
            custoUnitario = 25.00
        })
    }

    $orderIdValue = Invoke-DbScalar -Connection $nexumConnection -Sql @'
SELECT id FROM compras_pedidos
WHERE solicitacao_id = @requestId AND fornecedor_id = @supplierId AND tenant_id = @tenantId AND is_deleted = 0
ORDER BY id DESC LIMIT 1
'@ -Parameters @{
        '@requestId' = $requestId
        '@supplierId' = $supplierId
        '@tenantId' = $tenantId
    }
    if ($null -ne $orderIdValue -and $orderIdValue -ne [DBNull]::Value) {
        $orderId = [int]$orderIdValue
        $orderNumber = [string](Invoke-DbScalar -Connection $nexumConnection -Sql 'SELECT numero FROM compras_pedidos WHERE id=@id' -Parameters @{ '@id' = $orderId })
    }
    Assert-StatusCode -Response $orderResponse -Expected @(201) -Operation 'Geracao do pedido com conta a pagar Genesis'
    Assert-Value -Condition ($orderId -gt 0) -Message 'Pedido criado nao foi confirmado no MySQL.'

    $orderItemId = [int](Invoke-DbScalar -Connection $nexumConnection -Sql @'
SELECT id FROM compras_pedido_itens
WHERE compra_pedido_id=@orderId AND produto_id=@productId AND tenant_id=@tenantId AND is_deleted=0
LIMIT 1
'@ -Parameters @{ '@orderId' = $orderId; '@productId' = $productId; '@tenantId' = $tenantId })
    Assert-Value -Condition ($orderItemId -gt 0) -Message 'Item do pedido nao foi confirmado no MySQL.'

    $approveOrder = Invoke-JsonRequest -Client $client -Method 'PATCH' -Uri "$apiBase/api/compras/pedidos/$orderId/status" -BearerToken $token -Body @{
        status = 'Aprovado'
        observacoes = $marker
    }
    Assert-StatusCode -Response $approveOrder -Expected @(200) -Operation 'Aprovacao do pedido de compra'

    $partialDocument = "$marker-PARCIAL"
    $partialEntry = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/compras/pedidos/$orderId/entradas" -BearerToken $token -Body @{
        numeroDocumento = $partialDocument
        tipoEntrada = 'EstoqueFisico'
        recebidoPor = $marker
        observacoes = $marker
        itens = @(@{ itemId = $orderItemId; quantidadeRecebida = 2 })
    }
    Assert-StatusCode -Response $partialEntry -Expected @(200) -Operation 'Entrada parcial de mercadoria'
    $partialEntryRow = @($partialEntry.Json.dados.entradas | Where-Object { $_.numeroDocumento -eq $partialDocument }) | Select-Object -First 1
    Assert-Value -Condition ($null -ne $partialEntryRow) -Message 'Entrada parcial nao apareceu na releitura retornada pela API.'
    $entryIds.Add([int]$partialEntryRow.id)

    $duplicateEntry = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/compras/pedidos/$orderId/entradas" -BearerToken $token -Body @{
        numeroDocumento = $partialDocument
        tipoEntrada = 'EstoqueFisico'
        recebidoPor = $marker
        observacoes = $marker
        itens = @(@{ itemId = $orderItemId; quantidadeRecebida = 1 })
    }
    Assert-StatusCode -Response $duplicateEntry -Expected @(409) -Operation 'Rejeicao de documento de entrada duplicado'

    $excessEntry = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/compras/pedidos/$orderId/entradas" -BearerToken $token -Body @{
        numeroDocumento = "$marker-EXCESSO"
        tipoEntrada = 'EstoqueFisico'
        recebidoPor = $marker
        observacoes = $marker
        itens = @(@{ itemId = $orderItemId; quantidadeRecebida = 3 })
    }
    Assert-StatusCode -Response $excessEntry -Expected @(400) -Operation 'Rejeicao de quantidade acima do saldo pendente'

    $finalDocument = "$marker-FINAL"
    $finalEntry = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/compras/pedidos/$orderId/entradas" -BearerToken $token -Body @{
        numeroDocumento = $finalDocument
        tipoEntrada = 'EstoqueFisico'
        recebidoPor = $marker
        observacoes = $marker
        itens = @(@{ itemId = $orderItemId; quantidadeRecebida = 2 })
    }
    Assert-StatusCode -Response $finalEntry -Expected @(200) -Operation 'Entrada final de mercadoria'
    $finalEntryRow = @($finalEntry.Json.dados.entradas | Where-Object { $_.numeroDocumento -eq $finalDocument }) | Select-Object -First 1
    Assert-Value -Condition ($null -ne $finalEntryRow) -Message 'Entrada final nao apareceu na releitura retornada pela API.'
    $entryIds.Add([int]$finalEntryRow.id)

    $closeOrder = Invoke-JsonRequest -Client $client -Method 'PATCH' -Uri "$apiBase/api/compras/pedidos/$orderId/status" -BearerToken $token -Body @{
        status = 'Fechado'
        observacoes = $marker
    }
    Assert-StatusCode -Response $closeOrder -Expected @(200) -Operation 'Fechamento do pedido recebido'

    $requestCount = [int](Invoke-DbScalar -Connection $nexumConnection -Sql 'SELECT COUNT(*) FROM compras_solicitacoes WHERE id=@id AND status=''Atendida''' -Parameters @{ '@id' = $requestId })
    $quoteCount = [int](Invoke-DbScalar -Connection $nexumConnection -Sql 'SELECT COUNT(*) FROM compras_cotacoes WHERE id=@id AND solicitacao_id=@requestId' -Parameters @{ '@id' = $quoteId; '@requestId' = $requestId })
    $orderCount = [int](Invoke-DbScalar -Connection $nexumConnection -Sql 'SELECT COUNT(*) FROM compras_pedidos WHERE id=@id AND status=''Fechado''' -Parameters @{ '@id' = $orderId })
    $entryCount = [int](Invoke-DbScalar -Connection $nexumConnection -Sql 'SELECT COUNT(*) FROM compras_entradas WHERE compra_pedido_id=@id' -Parameters @{ '@id' = $orderId })
    $movementCount = [int](Invoke-DbScalar -Connection $nexumConnection -Sql 'SELECT COUNT(*) FROM estoque_movimentos WHERE produto_id=@id AND compra_entrada_id IS NOT NULL' -Parameters @{ '@id' = $productId })
    $stock = [int](Invoke-DbScalar -Connection $nexumConnection -Sql 'SELECT estoque_atual FROM produtos WHERE id=@id' -Parameters @{ '@id' = $productId })
    $financeCount = [int](Invoke-DbScalar -Connection $nexumConnection -Sql 'SELECT COUNT(*) FROM financeiro WHERE observacoes LIKE @reference' -Parameters @{ '@reference' = "%CompraId=$orderId;%" })
    $auditCount = [int](Invoke-DbScalar -Connection $nexumConnection -Sql 'SELECT COUNT(*) FROM logs_auditoria WHERE usuario_id=@userId' -Parameters @{ '@userId' = $userId })
    $genesisCount = [int](Invoke-DbScalar -Connection $genesisConnection -Sql 'SELECT COUNT(*) FROM erp_contas_pagar WHERE numero_documento=@numero' -Parameters @{ '@numero' = $orderNumber })

    Assert-Value -Condition ($requestCount -eq 1) -Message 'Solicitacao nao terminou como Atendida.'
    Assert-Value -Condition ($quoteCount -eq 1) -Message 'Cotacao nao foi vinculada a solicitacao.'
    Assert-Value -Condition ($orderCount -eq 1) -Message 'Pedido nao terminou como Fechado.'
    Assert-Value -Condition ($entryCount -eq 2) -Message 'Quantidade de entradas persistidas diverge do fluxo executado.'
    Assert-Value -Condition ($movementCount -eq 2) -Message 'Movimentos de estoque persistidos divergem das entradas.'
    Assert-Value -Condition ($stock -eq 4) -Message 'Saldo de estoque final diverge da quantidade recebida.'
    Assert-Value -Condition ($financeCount -eq 1) -Message 'Lancamento financeiro Nexum nao foi confirmado.'
    Assert-Value -Condition ($genesisCount -eq 1) -Message 'Conta a pagar Genesis nao foi confirmada.'
    Assert-Value -Condition ($auditCount -ge 13) -Message 'Trilha de auditoria de Compras esta incompleta.'

    $validationSucceeded = $true
}
catch {
    $validationError = $_.Exception.Message
}
finally {
    try {
        if ($genesisConnection.State -eq [Data.ConnectionState]::Open -and -not [string]::IsNullOrWhiteSpace($orderNumber)) {
            $null = Invoke-DbNonQuery -Connection $genesisConnection -Sql 'DELETE FROM erp_contas_pagar WHERE numero_documento=@numero' -Parameters @{ '@numero' = $orderNumber }
        }

        if ($nexumConnection.State -eq [Data.ConnectionState]::Open) {
            $cleanup = $nexumConnection.BeginTransaction()
            try {
                if ($userId -gt 0) {
                    $null = Invoke-DbNonQuery -Connection $nexumConnection -Transaction $cleanup -Sql 'DELETE FROM logs_auditoria WHERE usuario_id=@userId' -Parameters @{ '@userId' = $userId }
                }
                if (-not [string]::IsNullOrWhiteSpace($orderNumber)) {
                    $likeOrder = "%$orderNumber%"
                    $null = Invoke-DbNonQuery -Connection $nexumConnection -Transaction $cleanup -Sql 'DELETE FROM logs_auditoria WHERE dados_novos LIKE @orderNumber' -Parameters @{ '@orderNumber' = $likeOrder }
                    $null = Invoke-DbNonQuery -Connection $nexumConnection -Transaction $cleanup -Sql 'DELETE FROM notificacoes WHERE mensagem LIKE @orderNumber' -Parameters @{ '@orderNumber' = $likeOrder }
                    $null = Invoke-DbNonQuery -Connection $nexumConnection -Transaction $cleanup -Sql 'DELETE FROM financeiro WHERE descricao LIKE @orderNumber OR observacoes LIKE @orderId' -Parameters @{ '@orderNumber' = $likeOrder; '@orderId' = "%CompraId=$orderId;%" }
                }
                if ($orderId -gt 0) {
                    $null = Invoke-DbNonQuery -Connection $nexumConnection -Transaction $cleanup -Sql 'DELETE FROM estoque_movimentos WHERE compra_entrada_id IN (SELECT id FROM compras_entradas WHERE compra_pedido_id=@orderId)' -Parameters @{ '@orderId' = $orderId }
                    $null = Invoke-DbNonQuery -Connection $nexumConnection -Transaction $cleanup -Sql 'DELETE FROM compras_entrada_itens WHERE compra_entrada_id IN (SELECT id FROM compras_entradas WHERE compra_pedido_id=@orderId)' -Parameters @{ '@orderId' = $orderId }
                    $null = Invoke-DbNonQuery -Connection $nexumConnection -Transaction $cleanup -Sql 'DELETE FROM compras_entradas WHERE compra_pedido_id=@orderId' -Parameters @{ '@orderId' = $orderId }
                    $null = Invoke-DbNonQuery -Connection $nexumConnection -Transaction $cleanup -Sql 'DELETE FROM compras_pedido_itens WHERE compra_pedido_id=@orderId' -Parameters @{ '@orderId' = $orderId }
                    $null = Invoke-DbNonQuery -Connection $nexumConnection -Transaction $cleanup -Sql 'DELETE FROM compras_pedidos WHERE id=@orderId' -Parameters @{ '@orderId' = $orderId }
                }
                if ($requestId -gt 0) {
                    $null = Invoke-DbNonQuery -Connection $nexumConnection -Transaction $cleanup -Sql 'DELETE FROM compras_cotacoes WHERE solicitacao_id=@requestId' -Parameters @{ '@requestId' = $requestId }
                    $null = Invoke-DbNonQuery -Connection $nexumConnection -Transaction $cleanup -Sql 'DELETE FROM compras_solicitacoes WHERE id=@requestId' -Parameters @{ '@requestId' = $requestId }
                }
                if ($productId -gt 0) {
                    $null = Invoke-DbNonQuery -Connection $nexumConnection -Transaction $cleanup -Sql 'DELETE FROM produtos WHERE id=@productId' -Parameters @{ '@productId' = $productId }
                }
                if ($supplierId -gt 0) {
                    $null = Invoke-DbNonQuery -Connection $nexumConnection -Transaction $cleanup -Sql 'DELETE FROM fornecedores WHERE id=@supplierId' -Parameters @{ '@supplierId' = $supplierId }
                }
                if ($userId -gt 0) {
                    $null = Invoke-DbNonQuery -Connection $nexumConnection -Transaction $cleanup -Sql 'DELETE FROM usuarios WHERE id=@userId' -Parameters @{ '@userId' = $userId }
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

            $residualRows = [int](Invoke-DbScalar -Connection $nexumConnection -Sql @'
SELECT
    (SELECT COUNT(*) FROM usuarios WHERE email=@email) +
    (SELECT COUNT(*) FROM fornecedores WHERE razao_social=@marker) +
    (SELECT COUNT(*) FROM produtos WHERE sku=@marker) +
    (SELECT COUNT(*) FROM compras_solicitacoes WHERE produto_nome=@marker) +
    (SELECT COUNT(*) FROM compras_cotacoes WHERE produto_nome=@marker) +
    (SELECT COUNT(*) FROM compras_pedido_itens WHERE produto_nome=@marker) +
    (SELECT COUNT(*) FROM notificacoes WHERE mensagem LIKE @markerLike)
'@ -Parameters @{ '@email' = $email; '@marker' = $marker; '@markerLike' = "%$marker%" })
        }
    }
    catch {
        if ($null -eq $validationError) {
            $validationError = "Falha na limpeza controlada: $($_.Exception.Message)"
        }
        else {
            $validationError = "$validationError | Falha na limpeza controlada: $($_.Exception.Message)"
        }
        $validationSucceeded = $false
    }
    finally {
        $password = $null
        [Array]::Clear($passwordBytes, 0, $passwordBytes.Length)
        $client.Dispose()
        $nexumConnection.Dispose()
        $genesisConnection.Dispose()
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
    RequestId = $requestId
    QuoteId = $quoteId
    OrderId = $orderId
    EntryCount = $entryIds.Count
    AuditCount = $auditCount
    ResidualRows = $residualRows
    NexumBackup = $nexumBackup
    GenesisBackup = $genesisBackup
    Error = $validationError
}
$result | Format-List

if (-not $validationSucceeded) {
    throw "Validacao ponta a ponta de Compras falhou. $validationError"
}
