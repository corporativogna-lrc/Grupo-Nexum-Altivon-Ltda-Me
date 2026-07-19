#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5.7186
#

[CmdletBinding()]
param(
    [string]$ProjectRoot = "D:\Nexum Altivon\NexumAltivon.com",
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"
$officialProjectRoot = "D:\Nexum Altivon\NexumAltivon.com"
$resolvedProjectRoot = (Resolve-Path -LiteralPath $ProjectRoot).Path.TrimEnd("\")

if (-not [StringComparer]::OrdinalIgnoreCase.Equals($resolvedProjectRoot, $officialProjectRoot)) {
    throw "A auditoria somente pode ser executada no projeto oficial $officialProjectRoot. Recebido: $resolvedProjectRoot."
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $resolvedProjectRoot "docs\MATRIZ-PARIDADE-BANCO-WPF-2026-07-18.md"
}

$resolvedOutputDirectory = (Resolve-Path -LiteralPath (Split-Path -Parent $OutputPath)).Path
$fullOutputPath = [IO.Path]::GetFullPath((Join-Path $resolvedOutputDirectory (Split-Path -Leaf $OutputPath)))
if (-not $fullOutputPath.StartsWith($resolvedProjectRoot + "\", [StringComparison]::OrdinalIgnoreCase)) {
    throw "O relatorio deve permanecer dentro do projeto oficial. Recebido: $fullOutputPath."
}

$privateConfigurationPath = Join-Path $resolvedProjectRoot "runtime\api-24h\api.env.ps1"
$mysqlExecutable = "D:\xampp\mysql\bin\mysql.exe"
if (-not (Test-Path -LiteralPath $privateConfigurationPath -PathType Leaf)) {
    throw "Configuracao privada oficial ausente em $privateConfigurationPath."
}
if (-not (Test-Path -LiteralPath $mysqlExecutable -PathType Leaf)) {
    throw "Cliente MySQL oficial ausente em $mysqlExecutable."
}

. $privateConfigurationPath

function ConvertFrom-ConnectionString {
    param([Parameter(Mandatory)][string]$ConnectionString)

    $values = @{}
    foreach ($part in ($ConnectionString -split ";")) {
        $separatorIndex = $part.IndexOf("=")
        if ($separatorIndex -le 0) {
            continue
        }

        $key = $part.Substring(0, $separatorIndex).Trim().ToLowerInvariant()
        $values[$key] = $part.Substring($separatorIndex + 1).Trim()
    }

    $server = if ($values.ContainsKey("server")) { $values["server"] } else { $values["host"] }
    $port = if ($values.ContainsKey("port")) { $values["port"] } else { "3306" }
    $user = if ($values.ContainsKey("user id")) { $values["user id"] } elseif ($values.ContainsKey("uid")) { $values["uid"] } else { $values["user"] }
    $password = if ($values.ContainsKey("password")) { $values["password"] } else { $values["pwd"] }

    if ([string]::IsNullOrWhiteSpace($server) -or
        [string]::IsNullOrWhiteSpace($port) -or
        [string]::IsNullOrWhiteSpace($user) -or
        [string]::IsNullOrWhiteSpace($password)) {
        throw "A connection string oficial nao possui servidor, porta, usuario e senha completos."
    }

    return [pscustomobject]@{
        Server = $server
        Port = $port
        User = $user
        Password = $password
    }
}

function Get-DatabaseObjects {
    param(
        [Parameter(Mandatory)][string]$Schema,
        [Parameter(Mandatory)][string]$ConnectionString
    )

    if ($Schema -notin @("nexum_altivon", "genesis_bd")) {
        throw "Schema nao autorizado para esta auditoria: $Schema."
    }

    $connection = ConvertFrom-ConnectionString -ConnectionString $ConnectionString
    $query = @"
SELECT TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = '$Schema'
ORDER BY TABLE_NAME;
"@

    $previousPassword = $env:MYSQL_PWD
    $env:MYSQL_PWD = $connection.Password
    try {
        $result = & $mysqlExecutable `
            --protocol=tcp `
            "--host=$($connection.Server)" `
            "--port=$($connection.Port)" `
            "--user=$($connection.User)" `
            --batch `
            --skip-column-names `
            "--execute=$query" 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "Falha ao consultar information_schema para $Schema. Codigo: $LASTEXITCODE. Saida: $($result -join ' ')"
        }
    }
    finally {
        if ($null -eq $previousPassword) {
            Remove-Item Env:MYSQL_PWD -ErrorAction SilentlyContinue
        }
        else {
            $env:MYSQL_PWD = $previousPassword
        }
    }

    return @($result | ForEach-Object {
        $columns = $_ -split "`t", 3
        if ($columns.Count -ne 3) {
            throw "Linha inesperada retornada por information_schema no schema $Schema."
        }

        [pscustomobject]@{
            Schema = $columns[0]
            Name = $columns[1]
            Type = $columns[2]
        }
    })
}

function New-SourceIndex {
    param(
        [Parameter(Mandatory)][string]$Root,
        [Parameter(Mandatory)][string[]]$Extensions
    )

    $extensionArguments = @()
    foreach ($extension in $Extensions) {
        $extensionArguments += @("-g", "*.$extension")
    }

    $paths = @(& rg --files $Root @extensionArguments)
    if ($LASTEXITCODE -notin @(0, 1)) {
        throw "Falha ao inventariar fontes em $Root com rg. Codigo: $LASTEXITCODE."
    }

    return @($paths | ForEach-Object {
        $absolutePath = [IO.Path]::GetFullPath($_)
        if (-not $absolutePath.StartsWith($resolvedProjectRoot + "\", [StringComparison]::OrdinalIgnoreCase)) {
            throw "O arquivo indexado esta fora do projeto oficial: $absolutePath."
        }

        [pscustomobject]@{
            Path = $absolutePath.Substring($resolvedProjectRoot.Length + 1).Replace("\", "/")
            Content = (Get-Content -LiteralPath $absolutePath -Raw).ToLowerInvariant()
        }
    })
}

function Get-PreliminaryClassification {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Type
    )

    if ($Type -eq "VIEW") {
        return "View"
    }

    $normalized = $Name.ToLowerInvariant()
    if ($normalized -match "(__efmigrationshistory|hangfire|migration|schema_version|sys_config|configuracoes_sistema)" -or
        $normalized -match "^(aggregatedcounter|counter|hash|job|jobparameter|jobqueue|list|lock|server|set|state)$") {
        return "Tecnica"
    }
    if ($normalized -match "(auditoria|audit|logs?|historico|history|trilha|rastreamento|outbox)") {
        return "Auditoria"
    }
    if ($normalized -match "(integrac|webhook|marketplace|dropship|sincron|sync|gateway|token|api_|certificado|sefaz)") {
        return "Integracao"
    }
    if ($normalized -match "(_itens?$|_parcelas?$|_contatos?$|_enderecos?$|_anexos?$|_detalhes?$|_eventos?$|_movimentos?$|_documentos?$)") {
        return "Filha de agregado"
    }
    if ($normalized -match "(pedido|venda|compra|pagamento|receber|pagar|lancamento|movimentacao|estoque|inventario|transferencia|cotacao|solicitacao|entrada|saida|folha|ponto|admiss|demiss|ordem|produc|manutenc|ticket|oportunidade|campanha|conciliacao|fechamento|faturamento|nfe|nfce|cte|mdfe|sped)") {
        return "Transacional"
    }

    return "Administrativa"
}

function Get-LiteralReferences {
    param(
        [Parameter(Mandatory)][string]$ObjectName,
        [Parameter(Mandatory)][object[]]$SourceIndex
    )

    $needle = $ObjectName.ToLowerInvariant()
    return @($SourceIndex |
        Where-Object { $_.Content.IndexOf($needle, [StringComparison]::Ordinal) -ge 0 } |
        Select-Object -ExpandProperty Path |
        Sort-Object -Unique)
}

function ConvertTo-MarkdownCell {
    param([string[]]$Values)

    if ($null -eq $Values -or $Values.Count -eq 0) {
        return "-"
    }

    return (($Values | ForEach-Object { "``$($_.Replace('|', '\|'))``" }) -join "<br>")
}

$defaultConnection = $env:ConnectionStrings__DefaultConnection
$genesisConnection = $env:ConnectionStrings__GenesisConnection
if ([string]::IsNullOrWhiteSpace($defaultConnection) -or [string]::IsNullOrWhiteSpace($genesisConnection)) {
    throw "As connection strings oficiais dos schemas nexum_altivon e genesis_bd sao obrigatorias."
}

$databaseObjects = @(
    Get-DatabaseObjects -Schema "nexum_altivon" -ConnectionString $defaultConnection
    Get-DatabaseObjects -Schema "genesis_bd" -ConnectionString $genesisConnection
) | Sort-Object Schema, Name

if ($databaseObjects.Count -eq 0) {
    throw "information_schema nao retornou objetos para os schemas oficiais."
}

$apiSourceIndex = New-SourceIndex -Root (Join-Path $resolvedProjectRoot "NexumAltivon_Back-End") -Extensions @("cs")
$desktopSourceIndex = New-SourceIndex -Root (Join-Path $resolvedProjectRoot "NexumAltivon.Desktop") -Extensions @("cs", "xaml")
$desktopWindows = @($desktopSourceIndex |
    Where-Object { $_.Path.EndsWith("Window.xaml", [StringComparison]::OrdinalIgnoreCase) } |
    Select-Object -ExpandProperty Path |
    Sort-Object -Unique)
$genericOperationWindows = @($desktopSourceIndex |
    Where-Object {
        $_.Path.EndsWith("Window.xaml.cs", [StringComparison]::OrdinalIgnoreCase) -and
        $_.Content.IndexOf("submitoperationasync(", [StringComparison]::Ordinal) -ge 0
    } |
    Select-Object -ExpandProperty Path |
    Sort-Object -Unique)
$localOutboxWindows = @($desktopSourceIndex |
    Where-Object {
        $_.Path.EndsWith("Window.xaml.cs", [StringComparison]::OrdinalIgnoreCase) -and
        ($_.Content.IndexOf("saveoperationasync(", [StringComparison]::Ordinal) -ge 0 -or
         $_.Content.IndexOf("savesaleasync(", [StringComparison]::Ordinal) -ge 0)
    } |
    Select-Object -ExpandProperty Path |
    Sort-Object -Unique)
$directHttpClientWindows = @($desktopSourceIndex |
    Where-Object {
        $_.Path.EndsWith("Window.xaml.cs", [StringComparison]::OrdinalIgnoreCase) -and
        $_.Content.IndexOf("new httpclient", [StringComparison]::Ordinal) -ge 0
    } |
    Select-Object -ExpandProperty Path |
    Sort-Object -Unique)
$genericOperationService = $apiSourceIndex |
    Where-Object { $_.Path.EndsWith("GenesisDesktopOperationService.cs", [StringComparison]::OrdinalIgnoreCase) } |
    Select-Object -First 1
if ($genericOperationWindows.Count -gt 0 -and
    ($null -eq $genericOperationService -or
     $genericOperationService.Content.IndexOf("insert ignore into sys_operacoes_desktop", [StringComparison]::Ordinal) -lt 0)) {
    throw "Nao foi possivel comprovar o destino persistente do endpoint generico do Desktop."
}

$matrix = @($databaseObjects | ForEach-Object {
    $apiReferences = Get-LiteralReferences -ObjectName $_.Name -SourceIndex $apiSourceIndex
    $desktopReferences = Get-LiteralReferences -ObjectName $_.Name -SourceIndex $desktopSourceIndex
    [pscustomobject]@{
        Schema = $_.Schema
        Name = $_.Name
        Type = if ($_.Type -eq "BASE TABLE") { "Tabela" } else { "View" }
        Classification = Get-PreliminaryClassification -Name $_.Name -Type $_.Type
        ApiReferences = $apiReferences
        DesktopReferences = $desktopReferences
        ApiLiteral = $apiReferences.Count -gt 0
        DesktopLiteral = $desktopReferences.Count -gt 0
    }
})

$summaryBySchema = $matrix | Group-Object Schema | Sort-Object Name
$summaryByClassification = $matrix | Group-Object Classification | Sort-Object Name
$generatedAt = [DateTimeOffset]::Now.ToString("yyyy-MM-ddTHH:mm:sszzz")
$lines = [Collections.Generic.List[string]]::new()
$lines.Add("<!--")
$lines.Add(("Propriedade intelectual: Lu{0}s Rodrigo da Costa" -f [char]0x00ED))
$lines.Add("Com apoio: IA Chatgpt/Codex que atende por nome: Sophia")
$lines.Add(("Sistema de gest{0}o: GenesisGest.Net" -f [char]0x00E3))
$lines.Add(("Ano In{0}cio: 04/2024 Publicado e operacional: 05/2026" -f [char]0x00ED))
$lines.Add(("Vers{0}o: 1.1.5.7186" -f [char]0x00E3))
$lines.Add("-->")
$lines.Add("")
$lines.Add("# Matriz de Paridade Banco-WPF")
$lines.Add("")
$lines.Add("Gerado em: $generatedAt")
$lines.Add("")
$lines.Add("Fonte de banco: ``information_schema.TABLES`` dos schemas oficiais em ``127.0.0.1:3309``. Fonte de codigo: arquivos ``.cs`` da API e arquivos ``.cs``/``.xaml`` do Desktop no checkout oficial. Nenhuma credencial e gravada neste relatorio.")
$lines.Add("")
$lines.Add("A classificacao desta primeira etapa e preliminar e baseada em regras de nome declaradas no auditor. Referencia literal significa somente que o nome fisico da tabela foi localizado no codigo; nao comprova CRUD, formulario, permissao, auditoria ou homologacao. Ausencia de referencia literal tambem nao prova ausencia funcional, pois EF Core e DTOs podem usar nomes de entidade. A confirmacao clinica pertence ao WPFDB-02.")
$lines.Add("")
$lines.Add("## Resumo Factual")
$lines.Add("")
$lines.Add("| Medida | Quantidade |")
$lines.Add("|---|---:|")
$lines.Add("| Objetos inventariados | $($matrix.Count) |")
foreach ($group in $summaryBySchema) {
    $lines.Add("| Schema ``$($group.Name)`` | $($group.Count) |")
}
$lines.Add("| Tabelas base | $(($matrix | Where-Object Type -eq 'Tabela').Count) |")
$lines.Add("| Views | $(($matrix | Where-Object Type -eq 'View').Count) |")
$lines.Add("| Com referencia literal na API | $(($matrix | Where-Object ApiLiteral).Count) |")
$lines.Add("| Com referencia literal no Desktop | $(($matrix | Where-Object DesktopLiteral).Count) |")
$lines.Add("| Janelas WPF | $($desktopWindows.Count) |")
$lines.Add("")
$lines.Add("## Classificacao Preliminar")
$lines.Add("")
$lines.Add("| Classe | Quantidade |")
$lines.Add("|---|---:|")
foreach ($group in $summaryByClassification) {
    $lines.Add("| $($group.Name) | $($group.Count) |")
}
$lines.Add("")
$lines.Add("## Achados Impeditivos do Desktop")
$lines.Add("")
$lines.Add("As janelas abaixo chamam ``SubmitOperationAsync``. O servico associado comprova gravacao em ``genesis_bd.sys_operacoes_desktop``; esse registro generico de payload nao comprova persistencia nas tabelas de dominio declaradas pela tela e nao pode sustentar mensagem de conclusao funcional:")
$lines.Add("")
foreach ($path in $genericOperationWindows) {
    $lines.Add("- ``$path``")
}
$lines.Add("")
$lines.Add("As janelas abaixo gravam contingencia por ``SaveOperationAsync`` ou ``SaveSaleAsync``. A homologacao WPFDB-08 exige inventariar e provar reprocessamento, idempotencia, confirmacao no servidor e remocao segura do arquivo antes de considerar o fluxo offline concluido:")
$lines.Add("")
foreach ($path in $localOutboxWindows) {
    $lines.Add("- ``$path``")
}
$lines.Add("")
$lines.Add("As janelas abaixo instanciam ``HttpClient`` diretamente e devem ser reconciliadas com ``DesktopApiClient`` para centralizar autenticacao, endpoints, cancelamento e tratamento de erro:")
$lines.Add("")
foreach ($path in $directHttpClientWindows) {
    $lines.Add("- ``$path``")
}
$lines.Add("")
$lines.Add("## Inventario Integral")
$lines.Add("")
$lines.Add("| Schema | Objeto | Tipo | Classe preliminar | Referencias literais API | Referencias literais Desktop |")
$lines.Add("|---|---|---|---|---|---|")
foreach ($item in $matrix) {
    $apiCell = ConvertTo-MarkdownCell -Values $item.ApiReferences
    $desktopCell = ConvertTo-MarkdownCell -Values $item.DesktopReferences
    $lines.Add("| ``$($item.Schema)`` | ``$($item.Name)`` | $($item.Type) | $($item.Classification) | $apiCell | $desktopCell |")
}
$lines.Add("")
$lines.Add("## Proximo Gate")
$lines.Add("")
$lines.Add("WPFDB-02 deve revisar cada linha, confirmar o dominio real, identificar entidade/DTO/endpoint/permissao/formulario e registrar operacoes permitidas. Nenhuma linha desta matriz pode ser promovida a concluida somente por coincidencia de nome.")

$temporaryOutputPath = "$fullOutputPath.tmp-$PID"
try {
    Set-Content -LiteralPath $temporaryOutputPath -Value $lines -Encoding utf8
    Move-Item -LiteralPath $temporaryOutputPath -Destination $fullOutputPath -Force
}
finally {
    if (Test-Path -LiteralPath $temporaryOutputPath) {
        Remove-Item -LiteralPath $temporaryOutputPath -Force
    }
}

[pscustomobject]@{
    GeneratedAt = $generatedAt
    ProjectRoot = $resolvedProjectRoot
    OutputPath = $fullOutputPath
    Objects = $matrix.Count
    BaseTables = ($matrix | Where-Object Type -eq "Tabela").Count
    Views = ($matrix | Where-Object Type -eq "View").Count
    ApiLiteralReferences = ($matrix | Where-Object ApiLiteral).Count
    DesktopLiteralReferences = ($matrix | Where-Object DesktopLiteral).Count
    DesktopWindows = $desktopWindows.Count
    GenericOperationWindows = $genericOperationWindows.Count
    LocalOutboxWindows = $localOutboxWindows.Count
    DirectHttpClientWindows = $directHttpClientWindows.Count
}
