<#
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7190
 #>

[CmdletBinding()]
param(
    [ValidateNotNullOrEmpty()]
    [string]$ProjectRoot = 'D:\Nexum Altivon\NexumAltivon.com',

    [ValidateNotNullOrEmpty()]
    [string]$ApiUrl = 'http://127.0.0.1:5010',

    [ValidateNotNullOrEmpty()]
    [string]$PublicApiUrl = 'https://api.nexumaltivon.com.br'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ($PSVersionTable.PSEdition -ne 'Core' -or $PSVersionTable.PSVersion.Major -lt 7) {
    throw 'Esta validação carrega assemblies .NET 8 da API oficial e exige pwsh 7 ou superior.'
}

function Add-DbParameter {
    param([object]$Command, [string]$Name, [object]$Value)
    $null = $Command.Parameters.AddWithValue($Name, $(if ($null -eq $Value) { [DBNull]::Value } else { $Value }))
}

function Invoke-DbScalar {
    param([object]$Connection, [string]$Sql, [hashtable]$Parameters = @{})
    $command = $Connection.CreateCommand()
    try {
        $command.CommandText = $Sql
        $command.CommandTimeout = 60
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
    param([object]$Connection, [string]$Sql, [hashtable]$Parameters = @{})
    $command = $Connection.CreateCommand()
    try {
        $command.CommandText = $Sql
        $command.CommandTimeout = 60
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
        $request.Headers.UserAgent.ParseAdd('GenesisGest-Portal-Dinamico-Validation/1.1.5.7190')
        if ($null -ne $Body) {
            $json = $Body | ConvertTo-Json -Depth 16 -Compress
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

$resolvedRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path
$privateConfigPath = Join-Path $resolvedRoot 'runtime\api-24h\api.env.ps1'
$connectorPath = Join-Path $resolvedRoot 'runtime\api-24h\api\MySqlConnector.dll'
$bcryptPath = Join-Path $resolvedRoot 'runtime\api-24h\api\BCrypt.Net-Next.dll'
$backupScript = Join-Path $resolvedRoot 'scripts\server\backup-tabelas-mysql-oficial.ps1'
foreach ($requiredPath in @($privateConfigPath, $connectorPath, $bcryptPath, $backupScript)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) { throw "Dependência obrigatória ausente: $requiredPath" }
}

. $privateConfigPath
Add-Type -AssemblyName System.Net.Http
Add-Type -Path $connectorPath
Add-Type -Path $bcryptPath

$connectionString = [Environment]::GetEnvironmentVariable('ConnectionStrings__DefaultConnection', 'Process')
if ([string]::IsNullOrWhiteSpace($connectionString)) {
    throw 'A conexão oficial DefaultConnection deve estar configurada no ambiente privado.'
}

$runGuid = [Guid]::NewGuid().ToString('N')
$runId = '{0}-{1}' -f (Get-Date -Format 'yyyyMMddHHmmss'), $runGuid.Substring(0, 8)
$marker = "PORTAL-DINAMICO-$runId"
$tenantId = '00000000-0000-0000-0000-000000000001'
$userEmail = "controle.portal.$runId@nexumaltivon.com"
$restrictedUserEmail = "vendedor.portal.$runId@nexumaltivon.com"
$passwordBytes = New-Object byte[] 32
[Security.Cryptography.RandomNumberGenerator]::Fill($passwordBytes)
$password = [Convert]::ToBase64String($passwordBytes)
$passwordHash = [BCrypt.Net.BCrypt]::HashPassword($password, 12)
$apiBase = $ApiUrl.TrimEnd('/')
$publicApiBase = $PublicApiUrl.TrimEnd('/')
$backupDirectory = Join-Path $resolvedRoot 'runtime\api-24h\task-backups'
$backupPath = Join-Path $backupDirectory "$marker-nexum.sql"

& $backupScript -ProjectRoot $resolvedRoot -ConnectionName DefaultConnection -OutputPath $backupPath -Tables @(
    'usuarios', 'configuracoes_sistema', 'site_midias', 'site_perfis_publicos',
    'site_perfis_publicos_produtos', 'logs_auditoria') | Out-Null

$connection = [MySqlConnector.MySqlConnection]::new($connectionString)
$client = [System.Net.Http.HttpClient]::new()
$client.Timeout = [TimeSpan]::FromSeconds(60)
$userId = 0
$restrictedUserId = 0
$mediaId = [Guid]::Empty
$mediaRowVersion = ''
$mediaRelativePath = ''
$profileId = [Guid]::Empty
$profileRowVersion = ''
$profileSlug = "parceiro-$runId"
$originalSlides = $null
$originalInterval = $null
$originalPartnerRotation = $null
$originalColors = @{}
$slidesConfigId = 0
$intervalConfigId = 0
$partnerRotationConfigId = 0
$colorConfigIds = @{}
$productId = 0
$productSlug = ''
$configurationAuditCount = 0
$residualRows = -1
$validationSucceeded = $false
$validationError = $null

try {
    $connection.Open()
    $originalSlides = [string](Invoke-DbScalar -Connection $connection -Sql @'
SELECT valor FROM configuracoes_sistema
WHERE tenant_id = @tenantId AND chave = 'home_hero_slides' AND is_deleted = 0
LIMIT 1
'@ -Parameters @{ '@tenantId' = $tenantId })
    $originalInterval = [string](Invoke-DbScalar -Connection $connection -Sql @'
SELECT valor FROM configuracoes_sistema
WHERE tenant_id = @tenantId AND chave = 'home_hero_interval_seconds' AND is_deleted = 0
LIMIT 1
'@ -Parameters @{ '@tenantId' = $tenantId })
    $slidesConfigId = [int](Invoke-DbScalar -Connection $connection -Sql @'
SELECT id FROM configuracoes_sistema
WHERE tenant_id = @tenantId AND chave = 'home_hero_slides' AND is_deleted = 0
LIMIT 1
'@ -Parameters @{ '@tenantId' = $tenantId })
    $intervalConfigId = [int](Invoke-DbScalar -Connection $connection -Sql @'
SELECT id FROM configuracoes_sistema
WHERE tenant_id = @tenantId AND chave = 'home_hero_interval_seconds' AND is_deleted = 0
LIMIT 1
'@ -Parameters @{ '@tenantId' = $tenantId })
    $originalPartnerRotation = [string](Invoke-DbScalar -Connection $connection -Sql @'
SELECT valor FROM configuracoes_sistema
WHERE tenant_id = @tenantId AND chave = 'site_partner_rotation_seconds' AND is_deleted = 0
LIMIT 1
'@ -Parameters @{ '@tenantId' = $tenantId })
    $partnerRotationConfigId = [int](Invoke-DbScalar -Connection $connection -Sql @'
SELECT id FROM configuracoes_sistema
WHERE tenant_id = @tenantId AND chave = 'site_partner_rotation_seconds' AND is_deleted = 0
LIMIT 1
'@ -Parameters @{ '@tenantId' = $tenantId })
    foreach ($colorKey in @('site_cor_primaria', 'site_cor_secundaria', 'site_cor_fundo', 'site_cor_superficie', 'site_cor_texto', 'site_cor_texto_suave')) {
        $colorConfigIds[$colorKey] = [int](Invoke-DbScalar -Connection $connection -Sql @'
SELECT id FROM configuracoes_sistema
WHERE tenant_id = @tenantId AND chave = @key AND is_deleted = 0
LIMIT 1
'@ -Parameters @{ '@tenantId' = $tenantId; '@key' = $colorKey })
        $originalColors[$colorKey] = [string](Invoke-DbScalar -Connection $connection -Sql @'
SELECT valor FROM configuracoes_sistema
WHERE tenant_id = @tenantId AND chave = @key AND is_deleted = 0
LIMIT 1
'@ -Parameters @{ '@tenantId' = $tenantId; '@key' = $colorKey })
    }
    Assert-Value -Condition ($slidesConfigId -gt 0 -and $intervalConfigId -gt 0 -and $partnerRotationConfigId -gt 0) -Message 'As chaves oficiais de banners ou rodízio não foram encontradas no MySQL.'
    Assert-Value -Condition (@($colorConfigIds.Values | Where-Object { $_ -le 0 }).Count -eq 0) -Message 'As seis chaves oficiais de identidade visual não foram encontradas no MySQL.'

    foreach ($userDefinition in @(
        @{ Nome = $marker; Email = $userEmail; Perfil = 'SuperAdmin' },
        @{ Nome = "$marker VENDEDOR"; Email = $restrictedUserEmail; Perfil = 'Vendedor' }
    )) {
        $null = Invoke-DbNonQuery -Connection $connection -Sql @'
INSERT INTO usuarios
    (nome, email, senha_hash, perfil, ativo, tenant_id, row_version, is_deleted, created_at, updated_at)
VALUES
    (@nome, @email, @senha, @perfil, 1, @tenantId, UNHEX(REPLACE(UUID(), '-', '')), 0, UTC_TIMESTAMP(), UTC_TIMESTAMP())
'@ -Parameters @{ '@nome' = $userDefinition.Nome; '@email' = $userDefinition.Email; '@senha' = $passwordHash; '@perfil' = $userDefinition.Perfil; '@tenantId' = $tenantId }
        $createdUserId = [int](Invoke-DbScalar -Connection $connection -Sql 'SELECT LAST_INSERT_ID()')
        if ($userDefinition.Perfil -eq 'SuperAdmin') { $userId = $createdUserId } else { $restrictedUserId = $createdUserId }
    }

    $restrictedLogin = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/auth/login" -Body @{ email = $restrictedUserEmail; senha = $password }
    Assert-StatusCode -Response $restrictedLogin -Expected @(200) -Operation 'Login controlado sem permissão gerencial'
    $restrictedRead = Invoke-JsonRequest -Client $client -Method 'GET' -Uri "$apiBase/api/site/configuracoes" -BearerToken ([string]$restrictedLogin.Json.dados.token)
    Assert-StatusCode -Response $restrictedRead -Expected @(403) -Operation 'Bloqueio RBAC das configurações do portal'
    $restrictedProfilesRead = Invoke-JsonRequest -Client $client -Method 'GET' -Uri "$apiBase/api/site/perfis-publicos" -BearerToken ([string]$restrictedLogin.Json.dados.token)
    Assert-StatusCode -Response $restrictedProfilesRead -Expected @(403) -Operation 'Bloqueio RBAC dos perfis públicos'

    $login = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/auth/login" -Body @{ email = $userEmail; senha = $password }
    Assert-StatusCode -Response $login -Expected @(200) -Operation 'Login gerencial controlado'
    $token = [string]$login.Json.dados.token
    Assert-Value -Condition (-not [string]::IsNullOrWhiteSpace($token)) -Message 'O login gerencial não retornou token JWT.'

    $productsRead = Invoke-JsonRequest -Client $client -Method 'GET' -Uri "$apiBase/api/site/perfis-publicos/produtos-disponiveis" -BearerToken $token
    Assert-StatusCode -Response $productsRead -Expected @(200) -Operation 'Consulta de produtos publicáveis'
    $availableProducts = @($productsRead.Json.dados)
    Assert-Value -Condition ($availableProducts.Count -gt 0) -Message 'Não existe produto real elegível para validar o catálogo público do parceiro.'
    $productId = [int]$availableProducts[0].id
    $productSlug = [string]$availableProducts[0].slug
    Assert-Value -Condition ($productId -gt 0) -Message 'A API não retornou um identificador de produto publicável válido.'
    Assert-Value -Condition (-not [string]::IsNullOrWhiteSpace($productSlug)) -Message 'A API não retornou o slug público do produto selecionado.'

    $pngBase64 = 'iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACQSURBVHhe7dAxAQAgEIBA23xv25jqLUADGG5h5Lw7a9YAiiYNoGjSAIomDaBo0gCKJg2gaNIAiiYNoGjSAIomDaBo0gCKJg2gaNIAiiYNoGjSAIomDaBo0gCKJg2gaNIAiiYNoGjSAIomDaBo0gCKJg2gaNIAiibyAbMfV7ISaDUj1ssAAAAASUVORK5CYII='
    $mediaCreate = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/site/midias" -BearerToken $token -Body @{
        nome = $marker
        tipo = 'Banner'
        textoAlternativo = "Banner de validação operacional $runId"
        fileName = "$marker.png"
        contentType = 'image/png'
        dataUrl = "data:image/png;base64,$pngBase64"
    }
    Assert-StatusCode -Response $mediaCreate -Expected @(201) -Operation 'Persistência da mídia controlada'
    $mediaId = [Guid]$mediaCreate.Json.dados.id
    $mediaRowVersion = [string]$mediaCreate.Json.dados.rowVersion
    $mediaRelativePath = [string]$mediaCreate.Json.dados.caminhoRelativo
    Assert-Value -Condition ($mediaId -ne [Guid]::Empty -and $mediaRelativePath.StartsWith('/uploads/')) -Message 'A API não retornou ID e caminho relativo válidos para a mídia.'

    $profilePayload = @{
        tipoPerfil = 'ParceiroVenda'
        origemTipo = 'Parceiro'
        origemId = $null
        nome = "Parceiro controlado $runId"
        slug = $profileSlug
        segmento = 'Validação comercial controlada'
        atividade = 'Validação ponta a ponta do perfil comercial'
        descricao = 'Registro controlado criado exclusivamente para comprovar gravação, publicação, alteração, concorrência e limpeza do perfil comercial.'
        logoUrl = $null
        bannerUrl = $mediaRelativePath
        icone = 'Building2'
        ctaTexto = 'Ver perfil'
        ctaUrl = "/parceiros/$profileSlug"
        siteUrl = 'https://nexumaltivon.com.br'
        emailPublico = $userEmail
        telefonePublico = '+55 (31) 3333-7190'
        enderecoPublico = 'Contagem - MG'
        corPrimaria = '#2E7D32'
        corSecundaria = '#C9A227'
        corFundo = '#07110A'
        corTexto = '#F4FFF5'
        produtoIds = @($productId)
        publicado = $true
        ordemExibicao = 9900
    }
    $profileCreate = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/site/perfis-publicos" -BearerToken $token -Body $profilePayload
    Assert-StatusCode -Response $profileCreate -Expected @(201) -Operation 'Persistência do perfil comercial controlado'
    $profileId = [Guid]$profileCreate.Json.dados.id
    $profileRowVersion = [string]$profileCreate.Json.dados.rowVersion
    Assert-Value -Condition ($profileId -ne [Guid]::Empty -and -not [string]::IsNullOrWhiteSpace($profileRowVersion)) -Message 'A API não confirmou ID e RowVersion do perfil comercial.'

    $profilePayload.atividade = 'Atividade comercial atualizada e confirmada no banco'
    $profilePayload.rowVersion = $profileRowVersion
    $profileUpdate = Invoke-JsonRequest -Client $client -Method 'PUT' -Uri "$apiBase/api/site/perfis-publicos/$profileId" -BearerToken $token -Body $profilePayload
    Assert-StatusCode -Response $profileUpdate -Expected @(200) -Operation 'Atualização concorrente do perfil comercial'
    $profileRowVersion = [string]$profileUpdate.Json.dados.rowVersion
    Assert-Value -Condition ([string]$profileUpdate.Json.dados.atividade -eq $profilePayload.atividade) -Message 'A API não releu a atividade atualizada do perfil comercial.'

    $publicProfileRead = Invoke-JsonRequest -Client $client -Method 'GET' -Uri "$publicApiBase/api/site/parceiros/$profileSlug`?validation=$runId"
    Assert-StatusCode -Response $publicProfileRead -Expected @(200) -Operation 'Releitura pública do perfil comercial'
    Assert-Value -Condition ([string]$publicProfileRead.Json.dados.slug -eq $profileSlug) -Message 'A API pública não retornou o perfil comercial persistido.'
    Assert-Value -Condition ([string]$publicProfileRead.Json.dados.activity -eq $profilePayload.atividade) -Message 'A API pública não retornou a atividade comercial atualizada.'
    Assert-Value -Condition ([string]$publicProfileRead.Json.dados.image -eq $mediaRelativePath) -Message 'A API pública não preservou a mídia do perfil comercial.'
    Assert-Value -Condition ([string]$publicProfileRead.Json.dados.emailPublico -eq $userEmail) -Message 'A API pública não retornou o e-mail autorizado do perfil.'
    Assert-Value -Condition ([string]$publicProfileRead.Json.dados.corPrimaria -eq '#2E7D32') -Message 'A API pública não retornou a cor própria do perfil.'
    Assert-Value -Condition ($publicProfileRead.Json.dados.produtos.Count -eq 1) -Message 'A API pública não retornou exatamente o produto vinculado ao perfil.'
    Assert-Value -Condition ([string]$publicProfileRead.Json.dados.produtos[0].id -eq $productSlug) -Message 'O produto público divergiu do vínculo persistido.'

    $storedProfileCount = [int](Invoke-DbScalar -Connection $connection -Sql @'
SELECT COUNT(*) FROM site_perfis_publicos
WHERE id = @id AND tenant_id = @tenantId AND slug = @slug AND publicado = 1 AND is_deleted = 0
'@ -Parameters @{ '@id' = $profileId.ToString(); '@tenantId' = $tenantId; '@slug' = $profileSlug })
    Assert-Value -Condition ($storedProfileCount -eq 1) -Message 'O MySQL não contém o perfil comercial confirmado pela API.'
    $storedProductLinkCount = [int](Invoke-DbScalar -Connection $connection -Sql @'
SELECT COUNT(*) FROM site_perfis_publicos_produtos
WHERE perfil_publico_id = @profileId AND produto_id = @productId AND publicado = 1 AND is_deleted = 0
'@ -Parameters @{ '@profileId' = $profileId.ToString(); '@productId' = $productId })
    Assert-Value -Condition ($storedProductLinkCount -eq 1) -Message 'O MySQL não contém o vínculo auditável entre perfil e produto.'

    $externalReference = @(
        @{ id = "externo-$runId"; badge = 'Validação'; title = 'Referência externa'; highlight = 'Bloqueada'; description = 'Esta gravação deve ser recusada pelo contrato oficial.'; image = 'https://images.invalid/banner.png'; imageAlt = 'Referência externa'; active = $true; order = 0 }
    ) | ConvertTo-Json -Depth 8 -Compress
    $invalidExternal = Invoke-JsonRequest -Client $client -Method 'PUT' -Uri "$apiBase/api/site/configuracoes" -BearerToken $token -Body @{ itens = @(
        @{ chave = 'home_hero_slides'; valor = $externalReference; tipo = 'JSON'; descricao = 'Slides principais da home'; grupo = 'SiteHome'; editavel = $true }
    ) }
    Assert-StatusCode -Response $invalidExternal -Expected @(400) -Operation 'Rejeição de mídia externa'

    $invalidColor = Invoke-JsonRequest -Client $client -Method 'PUT' -Uri "$apiBase/api/site/configuracoes" -BearerToken $token -Body @{ itens = @(
        @{ chave = 'site_cor_primaria'; valor = 'dourado'; tipo = 'Cor'; descricao = 'Cor primaria do portal'; grupo = 'SiteAparencia'; editavel = $true }
    ) }
    Assert-StatusCode -Response $invalidColor -Expected @(400) -Operation 'Rejeição de cor global inválida'

    $testSlides = @(
        @{ id = "portal-a-$runId"; badge = 'Operação validada'; title = 'Portal dinâmico'; highlight = 'Persistência real'; description = 'Primeiro banner controlado para validar ordem, mídia e releitura pública.'; image = $mediaRelativePath; imageAlt = 'Primeiro banner de validação do portal'; active = $true; order = 9 },
        @{ id = "portal-b-$runId"; badge = 'Operação validada'; title = 'Conteúdo administrável'; highlight = 'Sem JSON manual'; description = 'Segundo banner controlado para confirmar a sequência entregue pela API pública.'; image = $mediaRelativePath; imageAlt = 'Segundo banner de validação do portal'; active = $true; order = 2 }
    ) | ConvertTo-Json -Depth 8 -Compress
    $validUpdate = Invoke-JsonRequest -Client $client -Method 'PUT' -Uri "$apiBase/api/site/configuracoes" -BearerToken $token -Body @{ itens = @(
        @{ chave = 'home_hero_interval_seconds'; valor = '4'; tipo = 'Numero'; descricao = 'Intervalo automático dos slides da home em segundos'; grupo = 'SiteHome'; editavel = $true },
        @{ chave = 'home_hero_slides'; valor = $testSlides; tipo = 'JSON'; descricao = 'Slides principais da home'; grupo = 'SiteHome'; editavel = $true },
        @{ chave = 'site_partner_rotation_seconds'; valor = '7'; tipo = 'Numero'; descricao = 'Intervalo do rodízio de parceiros'; grupo = 'SiteAparencia'; editavel = $true },
        @{ chave = 'site_cor_primaria'; valor = '#1B5E20'; tipo = 'Cor'; descricao = 'Cor primaria do portal'; grupo = 'SiteAparencia'; editavel = $true },
        @{ chave = 'site_cor_secundaria'; valor = '#F9A825'; tipo = 'Cor'; descricao = 'Cor secundaria do portal'; grupo = 'SiteAparencia'; editavel = $true },
        @{ chave = 'site_cor_fundo'; valor = '#061008'; tipo = 'Cor'; descricao = 'Cor de fundo do portal'; grupo = 'SiteAparencia'; editavel = $true },
        @{ chave = 'site_cor_superficie'; valor = '#102415'; tipo = 'Cor'; descricao = 'Cor de superficie do portal'; grupo = 'SiteAparencia'; editavel = $true },
        @{ chave = 'site_cor_texto'; valor = '#F5FFF6'; tipo = 'Cor'; descricao = 'Cor de texto do portal'; grupo = 'SiteAparencia'; editavel = $true },
        @{ chave = 'site_cor_texto_suave'; valor = '#A5D6A7'; tipo = 'Cor'; descricao = 'Cor de texto discreto do portal'; grupo = 'SiteAparencia'; editavel = $true }
    ) }
    Assert-StatusCode -Response $validUpdate -Expected @(200) -Operation 'Gravação dos banners dinâmicos'

    $publicRead = Invoke-JsonRequest -Client $client -Method 'GET' -Uri "$publicApiBase/api/site/configuracoes/publico?validation=$runId"
    Assert-StatusCode -Response $publicRead -Expected @(200) -Operation 'Releitura pública dos banners'
    Assert-Value -Condition ([int]$publicRead.Json.dados.heroIntervalSeconds -eq 4) -Message 'A API pública não releu o intervalo persistido.'
    Assert-Value -Condition ([int]$publicRead.Json.dados.partnerRotationSeconds -eq 7) -Message 'A API pública não releu o rodízio de parceiros persistido.'
    Assert-Value -Condition ([string]$publicRead.Json.dados.primaryColor -eq '#1B5E20') -Message 'A API pública não releu a cor primária persistida.'
    Assert-Value -Condition ([string]$publicRead.Json.dados.mutedTextColor -eq '#A5D6A7') -Message 'A API pública não releu a cor de texto discreto persistida.'
    Assert-Value -Condition ($publicRead.Json.dados.heroSlides.Count -eq 2) -Message 'A API pública não retornou os dois banners persistidos.'
    Assert-Value -Condition ([string]$publicRead.Json.dados.heroSlides[0].id -eq "portal-a-$runId") -Message 'A ordem pública dos banners divergiu da ordem persistida.'
    Assert-Value -Condition ([string]$publicRead.Json.dados.heroSlides[0].image -eq $mediaRelativePath) -Message 'A API pública não preservou a referência portátil da mídia.'

    $storedSlides = [string](Invoke-DbScalar -Connection $connection -Sql @'
SELECT valor FROM configuracoes_sistema
WHERE id = @id AND tenant_id = @tenantId AND is_deleted = 0
'@ -Parameters @{ '@id' = $slidesConfigId; '@tenantId' = $tenantId })
    $storedInterval = [string](Invoke-DbScalar -Connection $connection -Sql @'
SELECT valor FROM configuracoes_sistema
WHERE id = @id AND tenant_id = @tenantId AND is_deleted = 0
'@ -Parameters @{ '@id' = $intervalConfigId; '@tenantId' = $tenantId })
    Assert-Value -Condition ($storedSlides.Contains("portal-a-$runId") -and $storedSlides.Contains($mediaRelativePath)) -Message 'O MySQL não contém os banners confirmados pela API.'
    Assert-Value -Condition ($storedInterval -eq '4') -Message 'O MySQL não contém o intervalo confirmado pela API.'
    $configurationAuditCount = [int](Invoke-DbScalar -Connection $connection -Sql @'
SELECT COUNT(*) FROM logs_auditoria
WHERE usuario_id = @userId
  AND tabela = 'configuracoes_sistema'
  AND acao = 'UPDATE'
  AND dados_novos LIKE @runPattern
'@ -Parameters @{ '@userId' = $userId; '@runPattern' = "%$runId%" })
    Assert-Value -Condition ($configurationAuditCount -eq 1) -Message 'O MySQL não contém a auditoria única do lote de configurações públicas.'

    $imageRead = $client.GetAsync("$publicApiBase${mediaRelativePath}?validation=$runId").GetAwaiter().GetResult()
    try {
        Assert-Value -Condition ([int]$imageRead.StatusCode -eq 200) -Message "A mídia pública retornou HTTP $([int]$imageRead.StatusCode)."
        Assert-Value -Condition ($imageRead.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult().Length -gt 0) -Message 'A mídia pública respondeu sem conteúdo.'
    }
    finally {
        $imageRead.Dispose()
    }

    $referencedDelete = Invoke-JsonRequest -Client $client -Method 'DELETE' -Uri "$apiBase/api/site/midias/${mediaId}?rowVersion=$([Uri]::EscapeDataString($mediaRowVersion))" -BearerToken $token
    Assert-StatusCode -Response $referencedDelete -Expected @(409) -Operation 'Bloqueio de exclusão da mídia vinculada'

    $validationSucceeded = $true
}
catch {
    $validationError = $_.Exception.Message
    throw
}
finally {
    try {
        if ($connection.State -ne [Data.ConnectionState]::Open) { $connection.Open() }
        if ($slidesConfigId -gt 0 -and $null -ne $originalSlides) {
            $null = Invoke-DbNonQuery -Connection $connection -Sql 'UPDATE configuracoes_sistema SET valor = @valor, updated_at = UTC_TIMESTAMP() WHERE id = @id' -Parameters @{ '@valor' = $originalSlides; '@id' = $slidesConfigId }
        }
        if ($intervalConfigId -gt 0 -and $null -ne $originalInterval) {
            $null = Invoke-DbNonQuery -Connection $connection -Sql 'UPDATE configuracoes_sistema SET valor = @valor, updated_at = UTC_TIMESTAMP() WHERE id = @id' -Parameters @{ '@valor' = $originalInterval; '@id' = $intervalConfigId }
        }
        if ($partnerRotationConfigId -gt 0 -and $null -ne $originalPartnerRotation) {
            $null = Invoke-DbNonQuery -Connection $connection -Sql 'UPDATE configuracoes_sistema SET valor = @valor, updated_at = UTC_TIMESTAMP() WHERE id = @id' -Parameters @{ '@valor' = $originalPartnerRotation; '@id' = $partnerRotationConfigId }
        }
        foreach ($colorKey in $originalColors.Keys) {
            if ($colorConfigIds[$colorKey] -gt 0) {
                $null = Invoke-DbNonQuery -Connection $connection -Sql 'UPDATE configuracoes_sistema SET valor = @valor, updated_at = UTC_TIMESTAMP() WHERE id = @id' -Parameters @{ '@valor' = $originalColors[$colorKey]; '@id' = $colorConfigIds[$colorKey] }
            }
        }

        if ($profileId -ne [Guid]::Empty) {
            $cleanupProfileLogin = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/auth/login" -Body @{ email = $userEmail; senha = $password }
            if ($cleanupProfileLogin.StatusCode -eq 200) {
                $cleanupProfileToken = [string]$cleanupProfileLogin.Json.dados.token
                $null = Invoke-JsonRequest -Client $client -Method 'DELETE' -Uri "$apiBase/api/site/perfis-publicos/${profileId}?rowVersion=$([Uri]::EscapeDataString($profileRowVersion))" -BearerToken $cleanupProfileToken
            }
            $null = Invoke-DbNonQuery -Connection $connection -Sql 'DELETE FROM site_perfis_publicos_produtos WHERE perfil_publico_id = @id' -Parameters @{ '@id' = $profileId.ToString() }
            $null = Invoke-DbNonQuery -Connection $connection -Sql 'DELETE FROM site_perfis_publicos WHERE id = @id' -Parameters @{ '@id' = $profileId.ToString() }
        }

        if ($mediaId -ne [Guid]::Empty) {
            $cleanupLogin = Invoke-JsonRequest -Client $client -Method 'POST' -Uri "$apiBase/api/auth/login" -Body @{ email = $userEmail; senha = $password }
            if ($cleanupLogin.StatusCode -eq 200) {
                $cleanupToken = [string]$cleanupLogin.Json.dados.token
                $null = Invoke-JsonRequest -Client $client -Method 'DELETE' -Uri "$apiBase/api/site/midias/${mediaId}?rowVersion=$([Uri]::EscapeDataString($mediaRowVersion))" -BearerToken $cleanupToken
            }

            $null = Invoke-DbNonQuery -Connection $connection -Sql 'DELETE FROM site_midias WHERE id = @id' -Parameters @{ '@id' = $mediaId.ToString() }
            $publicWebRoot = [Environment]::GetEnvironmentVariable('Storage__PublicWebRoot', 'Process')
            if (-not [string]::IsNullOrWhiteSpace($publicWebRoot) -and $mediaRelativePath.StartsWith('/uploads/')) {
                $physicalPath = Join-Path $publicWebRoot.Trim() $mediaRelativePath.TrimStart('/').Replace('/', [IO.Path]::DirectorySeparatorChar)
                if (Test-Path -LiteralPath $physicalPath) { Remove-Item -LiteralPath $physicalPath -Force }
            }
        }

        if ($userId -gt 0 -or $restrictedUserId -gt 0) {
            $ids = @($userId, $restrictedUserId) | Where-Object { $_ -gt 0 }
            foreach ($id in $ids) {
                $null = Invoke-DbNonQuery -Connection $connection -Sql 'DELETE FROM logs_auditoria WHERE usuario_id = @id' -Parameters @{ '@id' = $id }
                $null = Invoke-DbNonQuery -Connection $connection -Sql 'DELETE FROM usuarios WHERE id = @id' -Parameters @{ '@id' = $id }
            }
        }

        $residualRows = [int](Invoke-DbScalar -Connection $connection -Sql @'
SELECT
    (SELECT COUNT(*) FROM usuarios WHERE email IN (@email, @restrictedEmail))
  + (SELECT COUNT(*) FROM site_midias WHERE nome = @marker)
  + (SELECT COUNT(*) FROM site_perfis_publicos WHERE slug = @profileSlug)
  + (SELECT COUNT(*) FROM site_perfis_publicos_produtos WHERE perfil_publico_id = @profileId)
  + (SELECT COUNT(*) FROM configuracoes_sistema WHERE valor LIKE @runPattern)
'@ -Parameters @{ '@email' = $userEmail; '@restrictedEmail' = $restrictedUserEmail; '@marker' = $marker; '@profileSlug' = $profileSlug; '@profileId' = $profileId.ToString(); '@runPattern' = "%$runId%" })
    }
    finally {
        $connection.Dispose()
        $client.Dispose()
    }
}

if ($residualRows -ne 0) {
    throw "A limpeza controlada terminou com $residualRows registros residuais."
}

[pscustomobject]@{
    CheckedAt = [DateTimeOffset]::Now.ToString('o')
    ValidationSucceeded = $validationSucceeded
    ValidationError = $validationError
    LocalApi = $apiBase
    PublicApi = $publicApiBase
    MediaId = $mediaId
    MediaRelativePath = $mediaRelativePath
    ProfileId = $profileId
    ProfileSlug = $profileSlug
    PublicProfileStatus = 200
    PersistedSlides = 2
    LinkedProductId = $productId
    HeroIntervalSeconds = 4
    PartnerRotationSeconds = 7
    PersistedThemeColors = 6
    ConfigurationAuditCount = $configurationAuditCount
    ExternalReferenceStatus = 400
    InvalidColorStatus = 400
    ReferencedDeleteStatus = 409
    ResidualRows = $residualRows
    BackupPath = $backupPath
    BackupSha256 = (Get-FileHash -LiteralPath $backupPath -Algorithm SHA256).Hash
}
