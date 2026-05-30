using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("API/appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"API/appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var apiSettings = builder.Configuration.GetSection("ApiSettings");
var secretKey = jwtSettings["SecretKey"] ?? jwtSettings["Secret"] ?? throw new InvalidOperationException("JwtSettings:SecretKey nao configurada.");
var issuer = jwtSettings["Issuer"] ?? "NexumAltivon.API";
var audience = jwtSettings["Audience"] ?? "NexumAltivon.Admin";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

builder.Services.AddCors(options =>
{
    options.AddPolicy("NexumCorsPolicy", policy =>
    {
        var origins = apiSettings.GetSection("CorsOrigins").Get<string[]>()
            ?? new[]
            {
                "http://localhost:5000",
                "http://localhost:5001",
                "http://localhost:3000",
                "https://www.nexumaltivon.com",
                "https://admin.nexumaltivon.com"
            };

        policy.WithOrigins(origins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Token-Expired", "X-Total-Count", "X-Page-Count");
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Gerente", policy => policy.RequireRole("SuperAdmin", "Admin", "Gerente"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Nexum Altivon API",
        Version = "v1.0.0",
        Description = "API funcional inicial para site e painel administrativo Nexum Altivon."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Informe: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHealthChecks();
builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "nexum-altivon-api-keys")));

var app = builder.Build();

app.UseCors("NexumCorsPolicy");

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Nexum Altivon API v1");
        options.DocumentTitle = "Nexum Altivon API";
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapGet("/", (IHostEnvironment environment) =>
    environment.IsDevelopment() || environment.IsStaging()
        ? Results.Redirect("/swagger")
        : Results.Ok(new
        {
            status = "online",
            service = "Nexum Altivon API",
            version = "1.0.0"
        }));

app.MapPost("/api/auth/login", (
    LoginRequest request,
    IConfiguration configuration,
    IHostEnvironment environment) =>
{
    var admin = configuration.GetSection("AdminUser");
    var configuredEmail = admin["Email"] ?? "admin@nexumaltivon.com";
    var configuredPassword = admin["Password"];
    var configuredName = admin["Name"] ?? "Administrador Nexum";
    var configuredRole = admin["Role"] ?? "Gerente";

    if (string.IsNullOrWhiteSpace(configuredPassword))
    {
        if (environment.IsProduction())
        {
            return Results.Problem(
                "AdminUser:Password nao configurada para producao.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        configuredPassword = "Admin@123";
    }

    if (!string.Equals(request.Email, configuredEmail, StringComparison.OrdinalIgnoreCase)
        || request.Senha != configuredPassword)
    {
        return Results.Unauthorized();
    }

    var expiresAt = DateTime.UtcNow.AddHours(configuration.GetValue("JwtSettings:ExpirationHours", 24));
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, "1"),
        new Claim(JwtRegisteredClaimNames.Email, configuredEmail),
        new Claim(ClaimTypes.Name, configuredName),
        new Claim(ClaimTypes.Email, configuredEmail),
        new Claim(ClaimTypes.Role, configuredRole)
    };

    var token = new JwtSecurityToken(
        issuer,
        audience,
        claims,
        expires: expiresAt,
        signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

    var response = new LoginResponse(
        new JwtSecurityTokenHandler().WriteToken(token),
        string.Empty,
        expiresAt,
        new UsuarioDto(1, configuredName, configuredEmail, configuredRole));

    return Results.Ok(ApiResponse<LoginResponse>.Ok(response, "Login realizado com sucesso."));
})
.AllowAnonymous()
.WithName("Login");

app.MapGet("/api/admin/dashboard/completo", [Authorize(Policy = "Gerente")] () =>
{
    var dashboard = DashboardCompletoDto.CreateSample();
    return Results.Ok(ApiResponse<DashboardCompletoDto>.Ok(dashboard));
})
.WithName("DashboardCompleto")
;

app.MapGet("/api/admin/dashboard/kpis", [Authorize(Policy = "Gerente")] () =>
    Results.Ok(ApiResponse<DashboardKpiDto>.Ok(DashboardCompletoDto.CreateSample().Kpis)))
    .WithName("DashboardKpis")
    ;

app.MapGet("/api/lojas", () =>
    Results.Ok(ApiResponse<List<LojaDto>>.Ok(DashboardCompletoDto.Lojas)))
    .WithName("Lojas")
    ;

app.MapGet("/api/categorias", () =>
    Results.Ok(ApiResponse<List<CategoriaDto>>.Ok(StoreData.Categorias)))
    .WithName("Categorias")
    ;

app.MapGet("/api/produtos", (string? categoria_id) =>
{
    var produtos = string.IsNullOrWhiteSpace(categoria_id)
        ? StoreData.Produtos
        : StoreData.Produtos
            .Where(produto => string.Equals(produto.CategoriaId, categoria_id, StringComparison.OrdinalIgnoreCase))
            .ToList();

    return Results.Ok(ApiResponse<List<ProdutoLojaDto>>.Ok(produtos));
})
.WithName("Produtos")
;

app.MapGet("/api/produtos/destaques", () =>
    Results.Ok(ApiResponse<List<ProdutoLojaDto>>.Ok(StoreData.Produtos.Where(produto => produto.Destaque).ToList())))
    .WithName("ProdutosDestaques")
    ;

app.MapGet("/api/produtos/{id}", (string id) =>
{
    var produto = StoreData.Produtos.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));
    return produto is null ? Results.NotFound(ApiResponse<string>.Erro("Produto nao encontrado.")) : Results.Ok(ApiResponse<ProdutoLojaDto>.Ok(produto));
})
.WithName("ProdutoPorId")
;

app.MapGet("/api/cupons/{codigo}", (string codigo) =>
{
    var cupom = StoreData.Cupons.FirstOrDefault(item => string.Equals(item.Codigo, codigo, StringComparison.OrdinalIgnoreCase));
    return cupom is null ? Results.NotFound(ApiResponse<string>.Erro("Cupom invalido.")) : Results.Ok(ApiResponse<CupomDto>.Ok(cupom));
})
.WithName("CupomPorCodigo")
;

app.MapPost("/api/clientes", (ClienteRequest request) =>
{
    var cliente = new ClienteLojaDto(
        Math.Abs(HashCode.Combine(request.Email, request.Nome)),
        request.Nome,
        request.Email,
        request.Telefone);

    return Results.Ok(ApiResponse<ClienteLojaDto>.Ok(cliente, "Cliente registrado."));
})
.WithName("CriarCliente")
;

app.MapGet("/api/pedidos", () =>
    Results.Ok(ApiResponse<List<PedidoLojaDto>>.Ok(StoreData.Pedidos)))
    .WithName("Pedidos")
    ;

app.MapPost("/api/pedidos", (PedidoRequest request) =>
{
    var total = request.Itens.Sum(item =>
    {
        var produto = StoreData.Produtos.FirstOrDefault(produto => produto.Id == item.ProdutoId);
        var preco = produto?.PrecoPromocional ?? produto?.Preco ?? 0;
        return preco * item.Quantidade;
    });

    var pedido = new PedidoLojaDto(
        StoreData.Pedidos.Count + 1,
        $"NA-{DateTime.UtcNow:yyMMddHHmmss}",
        total,
        "Recebido",
        DateTime.UtcNow);

    StoreData.Pedidos.Insert(0, pedido);
    return Results.Ok(ApiResponse<PedidoLojaDto>.Ok(pedido, "Pedido criado com sucesso."));
})
.WithName("CriarPedido")
;

app.MapGet("/api/dashboard/resumo", () =>
    Results.Ok(ApiResponse<DashboardResumoDto>.Ok(StoreData.Resumo)))
    .WithName("DashboardResumo")
    ;

app.MapGet("/api/crm/leads", () =>
    Results.Ok(ApiResponse<List<LeadLojaDto>>.Ok(StoreData.Leads)))
    .WithName("Leads")
    ;

app.Run();

public sealed record LoginRequest(string Email, string Senha);

public sealed record LoginResponse(string Token, string RefreshToken, DateTime ExpiraEm, UsuarioDto Usuario);

public sealed record UsuarioDto(int Id, string Nome, string Email, string Perfil);

public sealed record ApiResponse<T>(
    bool Sucesso,
    string? Mensagem,
    T? Dados,
    List<string>? Erros = null,
    int? TotalRegistros = null,
    int? PaginaAtual = null,
    int? TotalPaginas = null)
{
    public static ApiResponse<T> Ok(T dados, string? mensagem = null, int? total = null, int? pagina = null, int? totalPaginas = null)
        => new(true, mensagem, dados, null, total, pagina, totalPaginas);

    public static ApiResponse<T> Erro(string mensagem, List<string>? erros = null)
        => new(false, mensagem, default, erros);
}

public sealed record LojaDto(
    int Id,
    string Nome,
    string Slug,
    string Segmento,
    string? Descricao,
    string CorPrimaria,
    string CorSecundaria,
    bool Ativa,
    int OrdemExibicao);

public sealed record DashboardKpiDto(
    decimal FaturamentoHoje,
    decimal FaturamentoMes,
    decimal FaturamentoAno,
    int PedidosHoje,
    int PedidosMes,
    int PedidosPendentes,
    int PedidosEnviados,
    int PedidosEntregues,
    int ClientesNovosMes,
    int ClientesAtivos,
    int TotalClientes,
    decimal TicketMedio,
    decimal TaxaConversao,
    int ProdutosAtivos,
    int ProdutosEstoqueBaixo,
    int ProdutosSemEstoque,
    int LeadsNovos,
    int LeadsConvertidos,
    int LeadsEmAtendimento);

public sealed record FaturamentoPorPeriodoDto(string Periodo, decimal Faturamento, int QuantidadePedidos);

public sealed record VendasPorLojaDto(string LojaNome, string LojaSlug, decimal Faturamento, int Pedidos, decimal TicketMedio, decimal Percentual);

public sealed record ProdutosMaisVendidosDto(int ProdutoId, string Nome, string? Imagem, string LojaNome, int QuantidadeVendida, decimal ReceitaTotal);

public sealed record ClientesRecentesDto(int Id, string Nome, string Email, string? Whatsapp, DateTime DataCadastro, int TotalPedidos, decimal TotalGasto);

public sealed record PedidosRecentesDto(int Id, string NumeroPedido, string ClienteNome, decimal Total, string Status, string StatusPagamento, string? LojaNome, DateTime DataPedido);

public sealed record LeadsRecentesDto(int Id, string Nome, string Tipo, string Status, string Prioridade, string? Email, string? Whatsapp, DateTime DataCriacao);

public sealed record CategoriaDto(string Id, string Nome, string Descricao);

public sealed record ProdutoLojaDto(
    string Id,
    string Nome,
    string Descricao,
    decimal Preco,
    decimal? PrecoPromocional,
    string ImagemUrl,
    int Estoque,
    bool Destaque,
    string Sku,
    string CategoriaId,
    decimal Avaliacao);

public sealed record CupomDto(string Codigo, decimal? DescontoPercentual, decimal? DescontoValor, decimal? ValorMinimo);

public sealed record ClienteRequest(string Nome, string Email, string? Cpf, string? Telefone);

public sealed record ClienteLojaDto(int Id, string Nome, string Email, string? Telefone);

public sealed record PedidoItemRequest(string ProdutoId, int Quantidade);

public sealed record PedidoRequest(int ClienteId, string LojaId, List<PedidoItemRequest> Itens, string? CupomCodigo, object? EnderecoEntrega);

public sealed record PedidoLojaDto(int Id, string NumeroPedido, decimal Total, string Status, DateTime CreatedAt);

public sealed record DashboardResumoDto(
    int PedidosHoje,
    int TotalClientes,
    decimal FaturamentoMes,
    int LeadsNovos,
    int ProdutosEstoqueBaixo,
    decimal Conversao,
    decimal TicketMedio);

public sealed record LeadLojaDto(int Id, string Nome, string Email, string Telefone, string Status, DateTime CreatedAt);

public static class StoreData
{
    public static readonly List<CategoriaDto> Categorias =
    [
        new("automaticos", "Automaticos", "Movimento mecanico com presenca executiva"),
        new("cronografos", "Cronografos", "Performance, precisao e leitura esportiva"),
        new("classicos", "Classicos", "Pecas discretas para rotina premium"),
        new("smart-luxo", "Smart Luxo", "Tecnologia conectada com acabamento refinado")
    ];

    public static readonly List<ProdutoLojaDto> Produtos =
    [
        new("na-atlas-chrono", "Atlas Chronograph Black", "Cronografo em aco escovado, safira antirrisco e pulseira intercambiavel para uso executivo.", 4890, 4290, "https://images.unsplash.com/photo-1523170335258-f5ed11844a49?auto=format&fit=crop&w=900&q=85", 8, true, "NA-ATL-BLK", "cronografos", 4.9m),
        new("na-orion-gold", "Orion Gold Reserve", "Relogio automatico dourado com mostrador sunray, reserva de marcha e acabamento premium.", 6990, 6490, "https://images.unsplash.com/photo-1547996160-81dfa63595aa?auto=format&fit=crop&w=900&q=85", 12, true, "NA-ORI-GLD", "automaticos", 4.8m),
        new("na-heritage-silver", "Heritage Silver 40mm", "Design classico em caixa fina, pulseira em couro italiano e resistencia a agua para o dia a dia.", 2990, null, "https://images.unsplash.com/photo-1539874754764-5a96559165b0?auto=format&fit=crop&w=900&q=85", 24, true, "NA-HER-SLV", "classicos", 4.7m),
        new("na-venture-carbon", "Venture Carbon Pro", "Caixa em carbono, pulseira esportiva premium e leitura de alta visibilidade para jornadas intensas.", 5290, 4990, "https://images.unsplash.com/photo-1434056886845-dac89ffe9b56?auto=format&fit=crop&w=900&q=85", 5, false, "NA-VEN-CBN", "cronografos", 4.6m),
        new("na-lumina-smart", "Lumina Smart Luxe", "Tela AMOLED, monitoramento completo e corpo metalico com acabamento de relojoaria.", 3890, null, "https://images.unsplash.com/photo-1508685096489-7aacd43bd3b1?auto=format&fit=crop&w=900&q=85", 18, false, "NA-LUM-SMT", "smart-luxo", 4.8m),
        new("na-minimal-rose", "Minimal Rose Mesh", "Perfil ultrafino, malha milanesa rose e mostrador minimalista para composicoes sofisticadas.", 2590, 2290, "https://images.unsplash.com/photo-1524592094714-0f0654e20314?auto=format&fit=crop&w=900&q=85", 31, false, "NA-MIN-RSE", "classicos", 4.7m)
    ];

    public static readonly List<CupomDto> Cupons =
    [
        new("NEXUM10", 10, null, 500),
        new("FRETEGRATIS", null, 89, 1000)
    ];

    public static readonly DashboardResumoDto Resumo = new(38, 1248, 286420, 64, 7, 7.8m, 3280);

    public static readonly List<PedidoLojaDto> Pedidos =
    [
        new(1029, "NA-1029", 6490, "Processando", DateTime.UtcNow.AddHours(-2)),
        new(1028, "NA-1028", 4290, "Enviado", DateTime.UtcNow.AddHours(-5)),
        new(1027, "NA-1027", 7580, "Entregue", DateTime.UtcNow.AddDays(-1))
    ];

    public static readonly List<LeadLojaDto> Leads =
    [
        new(210, "Marina Alves", "marina.alves@email.com", "(11) 98221-4400", "Qualificado", DateTime.UtcNow.AddHours(-3)),
        new(209, "Rafael Monteiro", "rafael.m@email.com", "(21) 99774-1030", "Negociacao", DateTime.UtcNow.AddHours(-8)),
        new(208, "Bianca Torres", "bianca.t@email.com", "(31) 98812-5511", "Novo", DateTime.UtcNow.AddDays(-1))
    ];
}

public sealed record DashboardCompletoDto(
    DashboardKpiDto Kpis,
    List<FaturamentoPorPeriodoDto> FaturamentoSemanal,
    List<FaturamentoPorPeriodoDto> FaturamentoMensal,
    List<VendasPorLojaDto> VendasPorLoja,
    List<ProdutosMaisVendidosDto> ProdutosMaisVendidos,
    List<ClientesRecentesDto> ClientesRecentes,
    List<PedidosRecentesDto> PedidosRecentes,
    List<LeadsRecentesDto> LeadsRecentes)
{
    public static readonly List<LojaDto> Lojas =
    [
        new(1, "Geracao Top+", "geracao-top", "Tecnologia", "Eletronicos e acessorios", "#C9A227", "#0A0A0A", true, 1),
        new(2, "Moda Mim", "moda-mim", "Moda", "Moda feminina e lifestyle", "#D81B60", "#0A0A0A", true, 2),
        new(3, "Chronos", "chronos", "Relogios", "Relogios e presentes", "#C9A227", "#1E3A5F", true, 3),
        new(4, "Grann-Tur", "grann-tur", "Viagens", "Turismo e malas", "#1E88E5", "#0A0A0A", true, 4),
        new(5, "Estruturaline", "estruturaline", "Construcao", "Materiais e solucoes estruturais", "#546E7A", "#0A0A0A", true, 5),
        new(6, "Gran-fest-festas", "gran-fest", "Festas", "Artigos para eventos", "#8E24AA", "#0A0A0A", true, 6)
    ];

    public static DashboardCompletoDto CreateSample()
    {
        var hoje = DateTime.Today;

        return new DashboardCompletoDto(
            new DashboardKpiDto(2847.50m, 45230.80m, 387450.00m, 12, 186, 23, 45, 892, 34, 1247, 3856, 243.50m, 3.2m, 1245, 18, 5, 12, 8, 15),
            [
                new("17/05", 1850.00m, 8),
                new("18/05", 2340.50m, 10),
                new("19/05", 1560.00m, 6),
                new("20/05", 3120.80m, 13),
                new("21/05", 2780.00m, 11),
                new("22/05", 1950.00m, 8),
                new("23/05", 2847.50m, 12)
            ],
            [
                new("jun/25", 32450.00m, 142),
                new("jul/25", 38920.50m, 168),
                new("ago/25", 35670.00m, 154),
                new("set/25", 42180.80m, 182),
                new("out/25", 39850.00m, 172),
                new("nov/25", 44560.00m, 195),
                new("dez/25", 52340.00m, 228),
                new("jan/26", 28950.00m, 126),
                new("fev/26", 31240.00m, 138),
                new("mar/26", 36780.00m, 162),
                new("abr/26", 41250.00m, 178),
                new("mai/26", 45230.80m, 186)
            ],
            [
                new("Geracao Top+", "geracao-top", 98560.00m, 412, 239.22m, 25.4m),
                new("Moda Mim", "moda-mim", 72340.00m, 298, 242.75m, 18.7m),
                new("Chronos", "chronos", 67890.00m, 245, 277.10m, 17.5m),
                new("Grann-Tur", "grann-tur", 54230.00m, 198, 273.89m, 14.0m),
                new("Estruturaline", "estruturaline", 45670.00m, 156, 292.76m, 11.8m),
                new("Gran-fest-festas", "gran-fest", 48760.00m, 187, 260.75m, 12.6m)
            ],
            [
                new(1, "Smartphone Galaxy S24", null, "Geracao Top+", 142, 213400.00m),
                new(2, "Relogio Chronos Elite", null, "Chronos", 98, 78400.00m),
                new(3, "Mala de Viagem Premium", null, "Grann-Tur", 87, 43500.00m),
                new(4, "Vestido Floral Verao", null, "Moda Mim", 76, 22800.00m),
                new(5, "Kit Festa Premium", null, "Gran-fest-festas", 65, 19500.00m)
            ],
            [
                new(1, "Ana Carolina Silva", "ana.silva@email.com", "(14) 99876-5432", hoje.AddDays(-1), 3, 1250.00m),
                new(2, "Bruno Oliveira", "bruno.oliveira@email.com", "(14) 99765-4321", hoje.AddDays(-1), 1, 450.00m),
                new(3, "Carla Mendes", "carla.mendes@email.com", "(14) 99654-3210", hoje.AddDays(-2), 5, 2340.00m)
            ],
            [
                new(1, "NX2605230001", "Ana Carolina Silva", 450.00m, "Pago", "Aprovado", "Geracao Top+", hoje.AddHours(-2)),
                new(2, "NX2605230002", "Bruno Oliveira", 890.00m, "EmSeparacao", "Aprovado", "Moda Mim", hoje.AddHours(-4)),
                new(3, "NX2605230003", "Carla Mendes", 1250.00m, "Pendente", "Aguardando", "Chronos", hoje.AddHours(-6))
            ],
            [
                new(1, "Fernando Lopes", "Fornecedor", "Novo", "Alta", "fernando@fornecedor.com", "(11) 98765-4321", hoje.AddHours(-3)),
                new(2, "Gabriela Rocha", "Dropshipping", "EmAtendimento", "Media", "gabriela@dropship.com", "(21) 97654-3210", hoje.AddHours(-8)),
                new(3, "Henrique Almeida", "Parceiro", "Qualificado", "Alta", "henrique@parceiro.com", "(31) 96543-2109", hoje.AddDays(-1))
            ]);
    }
}
