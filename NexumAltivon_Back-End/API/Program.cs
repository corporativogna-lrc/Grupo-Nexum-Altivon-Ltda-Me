using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NexumAltivon.API.Data;
using NexumAltivon.API.ERP.FiscalRouting;
using NexumAltivon.API.Models;
using NexumAltivon.API.Services;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration.GetConnectionString("NexumDb");

if (!string.IsNullOrWhiteSpace(connectionString))
{
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
    builder.Services.AddDbContext<NexumDbContext>(options =>
        options.UseMySql(connectionString, serverVersion));
}

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
    options.AddPolicy("Admin", policy => policy.RequireRole("SuperAdmin", "Admin"));
    options.AddPolicy("Financeiro", policy => policy.RequireRole("SuperAdmin", "Admin", "Gerente", "Financeiro"));
    options.AddPolicy("Fiscal", policy => policy.RequireRole("SuperAdmin", "Admin", "Gerente", "Fiscal"));
    options.AddPolicy("RH", policy => policy.RequireRole("SuperAdmin", "Admin", "Gerente", "RH"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Nexum Altivon API",
        Version = "v1.1.5",
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
builder.Services.AddSingleton<IFiscalRoutingEngine, FiscalRoutingEngine>();
builder.Services.AddHttpClient("mercado-pago", client =>
{
    client.BaseAddress = new Uri("https://api.mercadopago.com/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddHttpClient("melhor-envio", client =>
{
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.UserAgent.ParseAdd("NexumAltivon/1.1.5");
});
builder.Services.AddHttpClient("mercado-livre", client =>
{
    client.BaseAddress = new Uri("https://api.mercadolibre.com/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddHttpClient("Notificacoes", client =>
{
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddScoped<INotificacaoService, NotificacaoService>();

builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "nexum-altivon-api-keys")));

var app = builder.Build();

await EnsureOperationalSchemaAsync(app.Services, app.Logger);

app.UseCors("NexumCorsPolicy");
app.UseStaticFiles();

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

app.MapGet("/health", () => Results.Text("Healthy", "text/plain"));
app.MapGet("/health/db", async (IServiceProvider services, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return Results.Ok(new { status = "sem_banco_configurado" });
    }

    try
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexumDbContext>();
        var canConnect = await db.Database.CanConnectAsync(ct);
        return canConnect
            ? Results.Ok(new { status = "Healthy" })
            : Results.Problem("Banco configurado, mas sem conexão.");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/", (IHostEnvironment environment) =>
    environment.IsDevelopment() || environment.IsStaging()
        ? Results.Redirect("/swagger")
        : Results.Ok(new
        {
            status = "online",
            service = "Nexum Altivon API",
            version = "1.1.5"
        }));

app.MapPost("/api/auth/login", async (
    LoginRequest request,
    IConfiguration configuration,
    IHostEnvironment environment,
    NexumDbContext db,
    CancellationToken ct) =>
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

    var normalizedEmail = NormalizeEmail(request.Email);
    if (string.IsNullOrWhiteSpace(normalizedEmail))
    {
        return Results.Unauthorized();
    }

    var expirationHours = configuration.GetValue("JwtSettings:ExpirationHours", 24);

    if (string.Equals(normalizedEmail, configuredEmail, StringComparison.OrdinalIgnoreCase)
        && request.Senha == configuredPassword)
    {
        var adminResponse = CreateLoginResponse(1, configuredName, configuredEmail, configuredRole, issuer, audience, signingKey, expirationHours);
        return Results.Ok(ApiResponse<LoginResponse>.Ok(adminResponse, "Login administrativo realizado com sucesso."));
    }

    var usuario = await db.Usuarios
        .AsNoTracking()
        .FirstOrDefaultAsync(item => item.Email == normalizedEmail && item.Ativo, ct);

    if (usuario is not null && BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
    {
        var perfil = usuario.Perfil.ToString();
        var usuarioResponse = CreateLoginResponse(usuario.Id, usuario.Nome, usuario.Email, perfil, issuer, audience, signingKey, expirationHours);
        return Results.Ok(ApiResponse<LoginResponse>.Ok(usuarioResponse, "Login realizado com sucesso."));
    }

    var cliente = await db.Clientes
        .FirstOrDefaultAsync(item => item.Email == normalizedEmail, ct);

    if (cliente is not null
        && !string.IsNullOrWhiteSpace(cliente.SenhaHash)
        && BCrypt.Net.BCrypt.Verify(request.Senha, cliente.SenhaHash)
        && (cliente.Status == StatusCliente.Pendente || cliente.EmailVerificadoEm is null))
    {
        return Results.Problem(
            "Confirme seu e-mail antes do primeiro acesso. Solicite um novo link se necessário.",
            statusCode: StatusCodes.Status403Forbidden);
    }

    if (cliente is not null
        && cliente.Status == StatusCliente.Ativo
        && cliente.EmailVerificadoEm is not null
        && !string.IsNullOrWhiteSpace(cliente.SenhaHash)
        && BCrypt.Net.BCrypt.Verify(request.Senha, cliente.SenhaHash))
    {
        cliente.UltimoAcesso = DateTime.UtcNow;
        cliente.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        var clienteResponse = CreateLoginResponse(cliente.Id, cliente.Nome, cliente.Email, "Cliente", issuer, audience, signingKey, expirationHours);
        return Results.Ok(ApiResponse<LoginResponse>.Ok(clienteResponse, "Login do cliente realizado com sucesso."));
    }

    return Results.Unauthorized();
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

app.MapGet("/api/lojas", async (NexumDbContext db, CancellationToken ct) =>
{
    var lojas = await db.Lojas
        .AsNoTracking()
        .OrderBy(loja => loja.OrdemExibicao)
        .ThenBy(loja => loja.Nome)
        .Select(loja => new LojaDto(
            loja.Id,
            loja.Nome,
            loja.Slug,
            loja.Segmento,
            loja.Descricao,
            loja.CorPrimaria,
            loja.CorSecundaria,
            loja.Ativa,
            loja.OrdemExibicao))
        .ToListAsync(ct);

    if (lojas.Count == 0)
    {
        lojas = DashboardCompletoDto.Lojas;
    }

    return Results.Ok(ApiResponse<List<LojaDto>>.Ok(lojas));
})
.AllowAnonymous()
.WithName("Lojas")
;

app.MapGet("/api/site/configuracoes/publico", async (NexumDbContext db, CancellationToken ct) =>
{
    var configs = await db.ConfiguracoesSistema
        .AsNoTracking()
        .ToListAsync(ct);

    var configMap = configs
        .GroupBy(item => item.Chave, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(group => group.Key, group => group.Last().Valor, StringComparer.OrdinalIgnoreCase);

    var publicConfig = BuildPublicSiteConfig(configMap);
    return Results.Ok(ApiResponse<SiteConfiguracaoPublicaDto>.Ok(publicConfig));
})
.AllowAnonymous()
.WithName("SiteConfiguracoesPublicas")
;

app.MapGet("/api/site/configuracoes", [Authorize(Policy = "Gerente")] async (NexumDbContext db, CancellationToken ct) =>
{
    var items = await db.ConfiguracoesSistema
        .AsNoTracking()
        .OrderBy(item => item.Grupo)
        .ThenBy(item => item.Chave)
        .Select(item => new SiteConfiguracaoItemDto(
            item.Id,
            item.Chave,
            item.Valor,
            item.Tipo.ToString(),
            item.Descricao,
            item.Grupo,
            item.Editavel,
            item.UpdatedAt))
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<SiteConfiguracaoItemDto>>.Ok(items));
})
.WithName("SiteConfiguracoesAdmin")
;

app.MapPut("/api/site/configuracoes", [Authorize(Policy = "Gerente")] async (SiteConfiguracaoUpdateRequest request, NexumDbContext db, CancellationToken ct) =>
{
    if (request.Itens is null || request.Itens.Count == 0)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Nenhuma configuração foi enviada."));
    }

    var requestedKeys = request.Itens
        .Where(item => !string.IsNullOrWhiteSpace(item.Chave))
        .Select(item => item.Chave.Trim())
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();

    var existing = await db.ConfiguracoesSistema
        .Where(item => requestedKeys.Contains(item.Chave))
        .ToListAsync(ct);

    foreach (var item in request.Itens)
    {
        var chave = item.Chave?.Trim();
        if (string.IsNullOrWhiteSpace(chave))
        {
            continue;
        }

        var entity = existing.FirstOrDefault(config => string.Equals(config.Chave, chave, StringComparison.OrdinalIgnoreCase));
        if (entity is null)
        {
            entity = new ConfiguracaoSistema
            {
                Chave = chave,
                CreatedAt = DateTime.UtcNow
            };
            db.ConfiguracoesSistema.Add(entity);
            existing.Add(entity);
        }

        entity.Valor = item.Valor?.Trim();
        entity.Descricao = item.Descricao?.Trim();
        entity.Grupo = item.Grupo?.Trim();
        entity.Editavel = item.Editavel ?? true;
        entity.UpdatedAt = DateTime.UtcNow;

        if (Enum.TryParse<TipoConfiguracao>(item.Tipo, true, out var tipo))
        {
            entity.Tipo = tipo;
        }
        else if (LooksLikeJson(entity.Valor))
        {
            entity.Tipo = TipoConfiguracao.JSON;
        }
        else
        {
            entity.Tipo = TipoConfiguracao.Texto;
        }
    }

    await db.SaveChangesAsync(ct);

    return Results.Ok(ApiResponse<string>.Ok("ok", "Configurações públicas do site atualizadas com sucesso."));
})
.WithName("AtualizarSiteConfiguracoes")
;

app.MapGet("/api/categorias", async (NexumDbContext db, CancellationToken ct) =>
{
    var categorias = await db.Categorias
        .AsNoTracking()
        .Where(categoria => categoria.Ativa)
        .OrderBy(categoria => categoria.Ordem)
        .ThenBy(categoria => categoria.Nome)
        .Select(categoria => new CategoriaDto(
            categoria.Slug,
            categoria.Nome,
            categoria.Descricao ?? string.Empty,
            categoria.CategoriaPai != null ? categoria.CategoriaPai.Slug : null,
            categoria.CategoriaPaiId.HasValue ? 2 : 1,
            categoria.CategoriaPai != null ? $"{categoria.CategoriaPai.Nome} / {categoria.Nome}" : categoria.Nome,
            categoria.Ordem,
            categoria.Ativa))
        .ToListAsync(ct);

    if (categorias.Count == 0)
    {
        categorias = StoreData.Categorias;
    }

    return Results.Ok(ApiResponse<List<CategoriaDto>>.Ok(categorias));
})
.AllowAnonymous()
.WithName("Categorias")
;

app.MapPost("/api/categorias", [Authorize(Policy = "Gerente")] async (
    CategoriaDto request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var slug = Slugify(request.Id);
    if (string.IsNullOrWhiteSpace(slug))
    {
        slug = Slugify(request.Nome);
    }

    if (string.IsNullOrWhiteSpace(slug))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Slug da categoria invalido."));
    }

    var exists = await db.Categorias.AnyAsync(categoria => categoria.Slug == slug, ct);
    if (exists)
    {
        return Results.Conflict(ApiResponse<string>.Erro("Categoria ja existe."));
    }

    var lojaId = await db.Lojas
        .AsNoTracking()
        .Where(loja => loja.Ativa)
        .OrderBy(loja => loja.OrdemExibicao)
        .Select(loja => loja.Id)
        .FirstOrDefaultAsync(ct);

    if (lojaId == 0)
    {
        return Results.Problem("Nenhuma loja ativa cadastrada.", statusCode: StatusCodes.Status500InternalServerError);
    }

    int? categoriaPaiId = null;
    if (!string.IsNullOrWhiteSpace(request.CategoriaPaiId))
    {
        var categoriaPaiSlug = request.CategoriaPaiId.Trim();
        categoriaPaiId = await db.Categorias
            .AsNoTracking()
            .Where(categoria => categoria.Slug == categoriaPaiSlug && categoria.Ativa)
            .Select(categoria => (int?)categoria.Id)
            .FirstOrDefaultAsync(ct);
    }

    var categoria = new Categoria
    {
        LojaId = lojaId,
        Nome = request.Nome,
        Slug = slug,
        Descricao = request.Descricao,
        CategoriaPaiId = categoriaPaiId,
        Ordem = request.Ordem ?? 0,
        Ativa = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.Categorias.Add(categoria);
    await db.SaveChangesAsync(ct);

    return Results.Created($"/api/categorias/{categoria.Slug}", ApiResponse<CategoriaDto>.Ok(new CategoriaDto(
        categoria.Slug,
        categoria.Nome,
        categoria.Descricao ?? string.Empty,
        request.CategoriaPaiId,
        categoria.CategoriaPaiId.HasValue ? 2 : 1,
        categoria.CategoriaPaiId.HasValue ? $"{request.CategoriaPaiId} / {categoria.Nome}" : categoria.Nome,
        categoria.Ordem,
        categoria.Ativa), "Categoria cadastrada."));
})
.WithName("CriarCategoria")
;

app.MapGet("/api/produtos", async (string? categoria_id, NexumDbContext db, CancellationToken ct) =>
{
    IQueryable<Produto> query = db.Produtos.AsNoTracking().Where(produto =>
        produto.Ativo
        && !string.IsNullOrWhiteSpace(produto.ImagemPrincipal)
        && (!string.IsNullOrWhiteSpace(produto.DescricaoCurta) || !string.IsNullOrWhiteSpace(produto.DescricaoLonga))
        && produto.Peso > 0
        && produto.Altura > 0
        && produto.Largura > 0
        && produto.Comprimento > 0);

    if (!string.IsNullOrWhiteSpace(categoria_id))
    {
        var categoriaId = await db.Categorias
            .AsNoTracking()
            .Where(categoria => categoria.Slug == categoria_id)
            .Select(categoria => (int?)categoria.Id)
            .FirstOrDefaultAsync(ct);

        if (categoriaId is null)
        {
            return Results.Ok(ApiResponse<List<ProdutoLojaDto>>.Ok([]));
        }

        query = query.Where(produto => produto.CategoriaId == categoriaId);
    }

    var produtos = await query
        .OrderByDescending(produto => produto.Destaque)
        .ThenByDescending(produto => produto.UpdatedAt)
        .Select(produto => new ProdutoLojaDto(
            produto.Slug,
            produto.Nome,
            produto.DescricaoCurta ?? produto.DescricaoLonga ?? string.Empty,
            produto.DescricaoCurta,
            produto.Preco,
            produto.PrecoPromocional,
            produto.ImagemPrincipal!,
            produto.EstoqueAtual,
            produto.EstoqueMinimo,
            produto.EstoqueReservado,
            produto.Destaque,
            produto.Sku,
            produto.Categoria != null ? produto.Categoria.Slug : "classicos",
            4.8m,
            produto.Custo,
            produto.Peso,
            produto.Altura,
            produto.Largura,
            produto.Comprimento,
            produto.TipoProduto.ToString(),
            produto.FornecedorId,
            produto.Marca,
            produto.Tags,
            produto.SeoTitulo,
            produto.SeoDescricao,
            produto.SeoKeywords,
            produto.ImagensGaleria))
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<ProdutoLojaDto>>.Ok(produtos));
})
.AllowAnonymous()
.WithName("Produtos")
;

app.MapGet("/api/produtos/destaques", async (NexumDbContext db, CancellationToken ct) =>
{
    var produtos = await db.Produtos
        .AsNoTracking()
        .Where(produto => produto.Ativo
            && produto.Destaque
            && !string.IsNullOrWhiteSpace(produto.ImagemPrincipal)
            && (!string.IsNullOrWhiteSpace(produto.DescricaoCurta) || !string.IsNullOrWhiteSpace(produto.DescricaoLonga))
            && produto.Peso > 0
            && produto.Altura > 0
            && produto.Largura > 0
            && produto.Comprimento > 0)
        .OrderByDescending(produto => produto.UpdatedAt)
        .Take(24)
        .Select(produto => new ProdutoLojaDto(
            produto.Slug,
            produto.Nome,
            produto.DescricaoCurta ?? produto.DescricaoLonga ?? string.Empty,
            produto.DescricaoCurta,
            produto.Preco,
            produto.PrecoPromocional,
            produto.ImagemPrincipal!,
            produto.EstoqueAtual,
            produto.EstoqueMinimo,
            produto.EstoqueReservado,
            produto.Destaque,
            produto.Sku,
            produto.Categoria != null ? produto.Categoria.Slug : "classicos",
            4.8m,
            produto.Custo,
            produto.Peso,
            produto.Altura,
            produto.Largura,
            produto.Comprimento,
            produto.TipoProduto.ToString(),
            produto.FornecedorId,
            produto.Marca,
            produto.Tags,
            produto.SeoTitulo,
            produto.SeoDescricao,
            produto.SeoKeywords,
            produto.ImagensGaleria))
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<ProdutoLojaDto>>.Ok(produtos));
})
.AllowAnonymous()
.WithName("ProdutosDestaques")
;

app.MapGet("/api/produtos/{id}", async (string id, NexumDbContext db, CancellationToken ct) =>
{
    var dto = await db.Produtos
        .AsNoTracking()
        .Where(item => item.Slug == id
            && item.Ativo
            && !string.IsNullOrWhiteSpace(item.ImagemPrincipal)
            && (!string.IsNullOrWhiteSpace(item.DescricaoCurta) || !string.IsNullOrWhiteSpace(item.DescricaoLonga))
            && item.Peso > 0
            && item.Altura > 0
            && item.Largura > 0
            && item.Comprimento > 0)
        .Select(item => new ProdutoLojaDto(
            item.Slug,
            item.Nome,
            item.DescricaoCurta ?? item.DescricaoLonga ?? string.Empty,
            item.DescricaoCurta,
            item.Preco,
            item.PrecoPromocional,
            item.ImagemPrincipal!,
            item.EstoqueAtual,
            item.EstoqueMinimo,
            item.EstoqueReservado,
            item.Destaque,
            item.Sku,
            item.Categoria != null ? item.Categoria.Slug : "classicos",
            4.8m,
            item.Custo,
            item.Peso,
            item.Altura,
            item.Largura,
            item.Comprimento,
            item.TipoProduto.ToString(),
            item.FornecedorId,
            item.Marca,
            item.Tags,
            item.SeoTitulo,
            item.SeoDescricao,
            item.SeoKeywords,
            item.ImagensGaleria))
        .FirstOrDefaultAsync(ct);

    if (dto is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Produto nao encontrado ou cadastro incompleto."));
    }

    return Results.Ok(ApiResponse<ProdutoLojaDto>.Ok(dto));
})
.AllowAnonymous()
.WithName("ProdutoPorId")
;

app.MapPost("/api/uploads/produtos/imagens", [Authorize(Policy = "Gerente")] async (
    UploadImagemRequest request,
    IWebHostEnvironment environment,
    HttpContext http,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.DataUrl))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Imagem obrigatoria."));
    }

    var separatorIndex = request.DataUrl.IndexOf(",", StringComparison.Ordinal);
    var header = separatorIndex > 0 ? request.DataUrl[..separatorIndex] : string.Empty;
    var base64 = separatorIndex > 0 ? request.DataUrl[(separatorIndex + 1)..] : request.DataUrl;
    var contentType = string.IsNullOrWhiteSpace(request.ContentType)
        ? header.Contains("image/png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg"
        : request.ContentType.Trim().ToLowerInvariant();

    var extension = contentType switch
    {
        "image/png" => ".png",
        "image/webp" => ".webp",
        "image/gif" => ".gif",
        _ => ".jpg"
    };

    if (contentType is not ("image/jpeg" or "image/jpg" or "image/png" or "image/webp" or "image/gif"))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Formato de imagem invalido."));
    }

    byte[] bytes;
    try
    {
        bytes = Convert.FromBase64String(base64);
    }
    catch
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Imagem invalida."));
    }

    if (bytes.Length == 0 || bytes.Length > 2 * 1024 * 1024)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Imagem deve ter ate 2MB."));
    }

    var root = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
    var uploadDir = Path.Combine(root, "uploads", "produtos");
    Directory.CreateDirectory(uploadDir);

    var fileName = $"produto-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}{extension}";
    var filePath = Path.Combine(uploadDir, fileName);
    await File.WriteAllBytesAsync(filePath, bytes, ct);

    var forwardedProto = http.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
    var scheme = !string.IsNullOrWhiteSpace(forwardedProto)
        ? forwardedProto
        : http.Request.Host.Host.EndsWith("trycloudflare.com", StringComparison.OrdinalIgnoreCase)
            ? "https"
            : http.Request.Scheme;
    var publicUrl = $"{scheme}://{http.Request.Host}/uploads/produtos/{fileName}";
    return Results.Ok(ApiResponse<UploadImagemDto>.Ok(new UploadImagemDto(publicUrl), "Imagem enviada."));
})
.WithName("UploadImagemProduto")
;

app.MapPost("/api/produtos", [Authorize(Policy = "Gerente")] async (
    ProdutoRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Nome)
        || string.IsNullOrWhiteSpace(request.Descricao)
        || string.IsNullOrWhiteSpace(request.ImagemUrl)
        || request.Peso is null or <= 0
        || request.Altura is null or <= 0
        || request.Largura is null or <= 0
        || request.Comprimento is null or <= 0)
    {
        return Results.BadRequest(ApiResponse<string>.Erro(
            "Nome, descricao, imagem, peso, altura, largura e comprimento sao obrigatorios."));
    }

    var slug = Slugify(request.Id) ?? Slugify(request.Nome);
    if (string.IsNullOrWhiteSpace(slug))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Id do produto invalido."));
    }

    var exists = await db.Produtos.AnyAsync(produto => produto.Slug == slug, ct);
    if (exists)
    {
        return Results.Conflict(ApiResponse<string>.Erro("Produto ja existe."));
    }

    var sku = string.IsNullOrWhiteSpace(request.Sku)
        ? $"NA-{Math.Abs(HashCode.Combine(request.Nome, request.Preco)):000000}"
        : request.Sku.Trim();

    if (await db.Produtos.AnyAsync(produto => produto.Sku == sku, ct))
    {
        sku = $"{sku}-{Random.Shared.Next(10, 99)}";
    }

    var lojaId = await db.Lojas
        .AsNoTracking()
        .Where(loja => loja.Ativa)
        .OrderBy(loja => loja.OrdemExibicao)
        .Select(loja => loja.Id)
        .FirstOrDefaultAsync(ct);

    if (lojaId == 0)
    {
        return Results.Problem("Nenhuma loja ativa cadastrada.", statusCode: StatusCodes.Status500InternalServerError);
    }

    int? categoriaId = null;
    var categoriaSlug = !string.IsNullOrWhiteSpace(request.SubcategoriaId)
        ? request.SubcategoriaId
        : request.CategoriaId;
    if (!string.IsNullOrWhiteSpace(categoriaSlug))
    {
        categoriaId = await db.Categorias
            .AsNoTracking()
            .Where(categoria => categoria.Slug == categoriaSlug)
            .Select(categoria => (int?)categoria.Id)
            .FirstOrDefaultAsync(ct);
    }

    var produto = new Produto
    {
        LojaId = lojaId,
        CategoriaId = categoriaId,
        Sku = sku,
        Nome = request.Nome,
        Slug = slug,
        DescricaoCurta = TrimOrNull(request.DescricaoCurta) ?? TrimOrNull(request.Descricao),
        DescricaoLonga = request.Descricao,
        Preco = request.Preco,
        PrecoPromocional = request.PrecoPromocional,
        Custo = request.Custo ?? 0m,
        Peso = request.Peso ?? 0m,
        Altura = request.Altura ?? 0m,
        Largura = request.Largura ?? 0m,
        Comprimento = request.Comprimento ?? 0m,
        ImagemPrincipal = request.ImagemUrl,
        ImagensGaleria = TrimOrNull(request.ImagensGaleria),
        EstoqueMinimo = request.EstoqueMinimo ?? 5,
        EstoqueAtual = request.Estoque,
        EstoqueReservado = request.EstoqueReservado ?? 0,
        TipoProduto = Enum.TryParse<NexumAltivon.API.Models.TipoProduto>(request.TipoProduto, true, out var tipoProduto) ? tipoProduto : NexumAltivon.API.Models.TipoProduto.Proprio,
        FornecedorId = request.FornecedorId,
        Marca = TrimOrNull(request.Marca),
        Tags = TrimOrNull(request.Tags),
        SeoTitulo = TrimOrNull(request.SeoTitulo),
        SeoDescricao = TrimOrNull(request.SeoDescricao),
        SeoKeywords = TrimOrNull(request.SeoKeywords),
        Destaque = request.Destaque,
        Ativo = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.Produtos.Add(produto);
    await db.SaveChangesAsync(ct);

    const string defaultImage = "https://images.unsplash.com/photo-1523170335258-f5ed11844a49?auto=format&fit=crop&w=900&q=85";
    var dto = new ProdutoLojaDto(
        produto.Slug,
        produto.Nome,
        produto.DescricaoCurta ?? produto.DescricaoLonga ?? string.Empty,
        produto.DescricaoCurta,
        produto.Preco,
        produto.PrecoPromocional,
        produto.ImagemPrincipal ?? defaultImage,
        produto.EstoqueAtual,
        produto.EstoqueMinimo,
        produto.EstoqueReservado,
        produto.Destaque,
        produto.Sku,
        categoriaSlug ?? "classicos",
        4.8m,
        produto.Custo,
        produto.Peso,
        produto.Altura,
        produto.Largura,
        produto.Comprimento,
        produto.TipoProduto.ToString(),
        produto.FornecedorId,
        produto.Marca,
        produto.Tags,
        produto.SeoTitulo,
        produto.SeoDescricao,
        produto.SeoKeywords,
        produto.ImagensGaleria);

    return Results.Created($"/api/produtos/{produto.Slug}", ApiResponse<ProdutoLojaDto>.Ok(dto, "Produto cadastrado."));
})
.WithName("CriarProduto")
;

app.MapPut("/api/produtos/{id}", [Authorize(Policy = "Gerente")] async (
    string id,
    ProdutoRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Nome)
        || string.IsNullOrWhiteSpace(request.Descricao)
        || string.IsNullOrWhiteSpace(request.ImagemUrl)
        || request.Peso is null or <= 0
        || request.Altura is null or <= 0
        || request.Largura is null or <= 0
        || request.Comprimento is null or <= 0)
    {
        return Results.BadRequest(ApiResponse<string>.Erro(
            "Nome, descricao, imagem, peso, altura, largura e comprimento sao obrigatorios."));
    }

    var produto = await db.Produtos.FirstOrDefaultAsync(item => item.Slug == id, ct);
    if (produto is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Produto nao encontrado."));
    }

    if (!string.IsNullOrWhiteSpace(request.Sku) && !string.Equals(produto.Sku, request.Sku.Trim(), StringComparison.OrdinalIgnoreCase))
    {
        var sku = request.Sku.Trim();
        var conflict = await db.Produtos.AnyAsync(item => item.Sku == sku && item.Id != produto.Id, ct);
        if (conflict)
        {
            return Results.Conflict(ApiResponse<string>.Erro("SKU ja em uso."));
        }

        produto.Sku = sku;
    }

    produto.Nome = request.Nome;
    produto.DescricaoCurta = TrimOrNull(request.DescricaoCurta) ?? TrimOrNull(request.Descricao);
    produto.DescricaoLonga = request.Descricao;
    produto.Preco = request.Preco;
    produto.PrecoPromocional = request.PrecoPromocional;
    produto.Custo = request.Custo ?? produto.Custo;
    produto.Peso = request.Peso ?? produto.Peso;
    produto.Altura = request.Altura ?? produto.Altura;
    produto.Largura = request.Largura ?? produto.Largura;
    produto.Comprimento = request.Comprimento ?? produto.Comprimento;
    produto.ImagemPrincipal = request.ImagemUrl;
    produto.ImagensGaleria = TrimOrNull(request.ImagensGaleria);
    produto.EstoqueMinimo = request.EstoqueMinimo ?? produto.EstoqueMinimo;
    produto.EstoqueAtual = request.Estoque;
    produto.EstoqueReservado = request.EstoqueReservado ?? produto.EstoqueReservado;
    produto.TipoProduto = Enum.TryParse<NexumAltivon.API.Models.TipoProduto>(request.TipoProduto, true, out var tipoProdutoAtualizado) ? tipoProdutoAtualizado : produto.TipoProduto;
    produto.FornecedorId = request.FornecedorId;
    produto.Marca = TrimOrNull(request.Marca);
    produto.Tags = TrimOrNull(request.Tags);
    produto.SeoTitulo = TrimOrNull(request.SeoTitulo);
    produto.SeoDescricao = TrimOrNull(request.SeoDescricao);
    produto.SeoKeywords = TrimOrNull(request.SeoKeywords);
    produto.Destaque = request.Destaque;
    produto.UpdatedAt = DateTime.UtcNow;

    var categoriaAtualizacaoSlug = !string.IsNullOrWhiteSpace(request.SubcategoriaId)
        ? request.SubcategoriaId
        : request.CategoriaId;
    if (!string.IsNullOrWhiteSpace(categoriaAtualizacaoSlug))
    {
        var categoriaId = await db.Categorias
            .AsNoTracking()
            .Where(categoria => categoria.Slug == categoriaAtualizacaoSlug)
            .Select(categoria => (int?)categoria.Id)
            .FirstOrDefaultAsync(ct);

        produto.CategoriaId = categoriaId;
    }

    await db.SaveChangesAsync(ct);

    const string defaultImage = "https://images.unsplash.com/photo-1523170335258-f5ed11844a49?auto=format&fit=crop&w=900&q=85";
    var dto = new ProdutoLojaDto(
        produto.Slug,
        produto.Nome,
        produto.DescricaoCurta ?? produto.DescricaoLonga ?? string.Empty,
        produto.DescricaoCurta,
        produto.Preco,
        produto.PrecoPromocional,
        produto.ImagemPrincipal ?? defaultImage,
        produto.EstoqueAtual,
        produto.EstoqueMinimo,
        produto.EstoqueReservado,
        produto.Destaque,
        produto.Sku,
        categoriaAtualizacaoSlug ?? "classicos",
        4.8m,
        produto.Custo,
        produto.Peso,
        produto.Altura,
        produto.Largura,
        produto.Comprimento,
        produto.TipoProduto.ToString(),
        produto.FornecedorId,
        produto.Marca,
        produto.Tags,
        produto.SeoTitulo,
        produto.SeoDescricao,
        produto.SeoKeywords,
        produto.ImagensGaleria);

    return Results.Ok(ApiResponse<ProdutoLojaDto>.Ok(dto, "Produto atualizado."));
})
.WithName("AtualizarProduto")
;

app.MapGet("/api/cupons/{codigo}", async (string codigo, NexumDbContext db, CancellationToken ct) =>
{
    var cupom = await db.Cupons
        .AsNoTracking()
        .FirstOrDefaultAsync(item => item.Codigo == codigo && item.Ativo, ct);

    if (cupom is null)
    {
        var fallback = StoreData.Cupons.FirstOrDefault(item => string.Equals(item.Codigo, codigo, StringComparison.OrdinalIgnoreCase));
        return fallback is null
            ? Results.NotFound(ApiResponse<string>.Erro("Cupom invalido."))
            : Results.Ok(ApiResponse<CupomDto>.Ok(fallback));
    }

    if (cupom.ValidoDe.HasValue && cupom.ValidoDe.Value > DateTime.UtcNow)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Cupom invalido."));
    }

    if (cupom.ValidoAte.HasValue && cupom.ValidoAte.Value < DateTime.UtcNow)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Cupom invalido."));
    }

    var dto = cupom.Tipo switch
    {
        TipoCupom.Percentual => new CupomDto(cupom.Codigo, cupom.Valor, null, cupom.ValorMinimoPedido),
        TipoCupom.ValorFixo => new CupomDto(cupom.Codigo, null, cupom.Valor, cupom.ValorMinimoPedido),
        TipoCupom.FreteGratis => new CupomDto(cupom.Codigo, null, cupom.Valor, cupom.ValorMinimoPedido),
        _ => new CupomDto(cupom.Codigo, cupom.Valor, null, cupom.ValorMinimoPedido)
    };

    return Results.Ok(ApiResponse<CupomDto>.Ok(dto));
})
.AllowAnonymous()
.WithName("CupomPorCodigo")
;

app.MapGet("/api/clientes", [Authorize(Policy = "Gerente")] async (NexumDbContext db, CancellationToken ct) =>
{
    var clientes = await db.Clientes
        .AsNoTracking()
        .OrderByDescending(cliente => cliente.CreatedAt)
        .Take(500)
        .Select(cliente => new ClienteLojaDto(cliente.Id, cliente.Nome, cliente.Email, cliente.Telefone, cliente.CpfCnpj))
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<ClienteLojaDto>>.Ok(clientes));
})
.WithName("Clientes")
;

app.MapGet("/api/clientes/verificar", async (string? email, string? cpf, NexumDbContext db, CancellationToken ct) =>
{
    var normalizedEmail = NormalizeEmail(email);
    var normalizedDocument = NormalizeDocument(cpf);

    if (string.IsNullOrWhiteSpace(normalizedEmail) && string.IsNullOrWhiteSpace(normalizedDocument))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Informe email ou CPF/CNPJ para verificar o cadastro."));
    }

    var cliente = await db.Clientes
        .AsNoTracking()
        .OrderByDescending(item => item.UpdatedAt)
        .FirstOrDefaultAsync(item =>
            (!string.IsNullOrWhiteSpace(normalizedEmail) && item.Email == normalizedEmail) ||
            (!string.IsNullOrWhiteSpace(normalizedDocument) &&
             ((item.CpfCnpj ?? string.Empty)
                 .Replace(".", string.Empty)
                 .Replace("-", string.Empty)
                 .Replace("/", string.Empty)
                 .Replace(" ", string.Empty)) == normalizedDocument), ct);

    var dto = cliente is null
        ? null
        : new ClienteLojaDto(cliente.Id, cliente.Nome, cliente.Email, cliente.Telefone, cliente.CpfCnpj);

    return Results.Ok(ApiResponse<CadastroClienteStatusDto>.Ok(
        new CadastroClienteStatusDto(cliente is not null, dto),
        cliente is null ? "Cadastro disponível para criação." : "Cliente já cadastrado."));
})
.AllowAnonymous()
.WithName("VerificarCadastroCliente")
;

app.MapPost("/api/clientes", async (
    ClienteRequest request,
    NexumDbContext db,
    INotificacaoService notificacaoService,
    IConfiguration configuration,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Nome))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Nome e email sao obrigatorios."));
    }

    var email = NormalizeEmail(request.Email)!;
    var cpfCnpj = NormalizeDocument(request.Cpf);
    var clienteExistente = await db.Clientes.FirstOrDefaultAsync(cliente =>
        cliente.Email == email ||
        (!string.IsNullOrWhiteSpace(cpfCnpj) &&
         ((cliente.CpfCnpj ?? string.Empty)
             .Replace(".", string.Empty)
             .Replace("-", string.Empty)
             .Replace("/", string.Empty)
             .Replace(" ", string.Empty)) == cpfCnpj), ct);

    var possuiSenha = !string.IsNullOrWhiteSpace(request.Senha);
    if (possuiSenha && request.Senha!.Trim().Length < 8)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("A senha deve ter pelo menos 8 caracteres."));
    }

    if (clienteExistente is not null)
    {
        if (possuiSenha && clienteExistente.EmailVerificadoEm is not null && !string.IsNullOrWhiteSpace(clienteExistente.SenhaHash))
        {
            return Results.Conflict(ApiResponse<string>.Erro("Este e-mail já possui acesso ativo. Use o login ou a recuperação de senha."));
        }

        string? confirmationToken = null;
        if (possuiSenha)
        {
            clienteExistente.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha.Trim(), 12);
            confirmationToken = CreateSecureToken();
            clienteExistente.TokenConfirmacaoEmail = HashToken(confirmationToken);
            clienteExistente.TokenConfirmacaoExpiraEm = DateTime.UtcNow.AddHours(24);
            clienteExistente.Status = StatusCliente.Pendente;
        }

        if (string.IsNullOrWhiteSpace(clienteExistente.Telefone) && !string.IsNullOrWhiteSpace(request.Telefone))
        {
            clienteExistente.Telefone = request.Telefone.Trim();
            clienteExistente.UpdatedAt = DateTime.UtcNow;
        }

        clienteExistente.Newsletter = request.Newsletter ?? clienteExistente.Newsletter;
        clienteExistente.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        clienteExistente.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        if (confirmationToken is not null)
        {
            await SendCustomerConfirmationEmailAsync(clienteExistente, confirmationToken, notificacaoService, configuration);
        }

        var existenteDto = new ClienteCadastroResponse(
            clienteExistente.Id,
            clienteExistente.Nome,
            clienteExistente.Email,
            confirmationToken is not null,
            clienteExistente.Status.ToString());
        return Results.Ok(ApiResponse<ClienteCadastroResponse>.Ok(
            existenteDto,
            confirmationToken is null
                ? "Cliente já cadastrado. Registro comercial reutilizado."
                : "Cadastro localizado. Enviamos um link para confirmar o e-mail e ativar o acesso."));
    }

    var cliente = new Cliente
    {
        Nome = request.Nome.Trim(),
        Email = email,
        Telefone = request.Telefone,
        Whatsapp = request.Telefone,
        CpfCnpj = cpfCnpj,
        SenhaHash = !string.IsNullOrWhiteSpace(request.Senha) ? BCrypt.Net.BCrypt.HashPassword(request.Senha.Trim(), 12) : null,
        Newsletter = request.Newsletter ?? true,
        Status = possuiSenha ? StatusCliente.Pendente : StatusCliente.Ativo,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    string? token = null;
    if (possuiSenha)
    {
        token = CreateSecureToken();
        cliente.TokenConfirmacaoEmail = HashToken(token);
        cliente.TokenConfirmacaoExpiraEm = DateTime.UtcNow.AddHours(24);
    }

    db.Clientes.Add(cliente);
    await db.SaveChangesAsync(ct);

    if (token is not null)
    {
        await SendCustomerConfirmationEmailAsync(cliente, token, notificacaoService, configuration);
    }

    var dto = new ClienteCadastroResponse(cliente.Id, cliente.Nome, cliente.Email, token is not null, cliente.Status.ToString());
    return Results.Ok(ApiResponse<ClienteCadastroResponse>.Ok(
        dto,
        token is null
            ? "Cliente comercial registrado."
            : "Cadastro realizado. Confirme o e-mail para liberar sua área do cliente."));
})
.AllowAnonymous()
.WithName("CriarCliente")
;

app.MapGet("/api/clientes/confirmar-email", async (string token, NexumDbContext db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(token))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Token de confirmação não informado."));
    }

    var tokenHash = HashToken(token);
    var cliente = await db.Clientes.FirstOrDefaultAsync(item => item.TokenConfirmacaoEmail == tokenHash, ct);
    if (cliente is null || cliente.TokenConfirmacaoExpiraEm is null || cliente.TokenConfirmacaoExpiraEm < DateTime.UtcNow)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Link inválido ou expirado. Solicite um novo link."));
    }

    cliente.EmailVerificadoEm = DateTime.UtcNow;
    cliente.TokenConfirmacaoEmail = null;
    cliente.TokenConfirmacaoExpiraEm = null;
    cliente.Status = StatusCliente.Ativo;
    cliente.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync(ct);

    return Results.Ok(ApiResponse<string>.Ok("ok", "E-mail confirmado. Sua área do cliente está liberada."));
})
.AllowAnonymous()
.WithName("ConfirmarEmailCliente")
;

app.MapPost("/api/clientes/reenviar-confirmacao", async (
    ReenviarConfirmacaoEmailRequest request,
    NexumDbContext db,
    INotificacaoService notificacaoService,
    IConfiguration configuration,
    CancellationToken ct) =>
{
    var email = NormalizeEmail(request.Email);
    var cliente = string.IsNullOrWhiteSpace(email)
        ? null
        : await db.Clientes.FirstOrDefaultAsync(item => item.Email == email, ct);

    if (cliente is not null && cliente.EmailVerificadoEm is null && !string.IsNullOrWhiteSpace(cliente.SenhaHash))
    {
        var token = CreateSecureToken();
        cliente.TokenConfirmacaoEmail = HashToken(token);
        cliente.TokenConfirmacaoExpiraEm = DateTime.UtcNow.AddHours(24);
        cliente.Status = StatusCliente.Pendente;
        cliente.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        await SendCustomerConfirmationEmailAsync(cliente, token, notificacaoService, configuration);
    }

    return Results.Ok(ApiResponse<string>.Ok(
        "ok",
        "Se houver uma conta pendente para este e-mail, um novo link será enviado."));
})
.AllowAnonymous()
.WithName("ReenviarConfirmacaoEmailCliente")
;

app.MapGet("/api/clientes/portal/me", [Authorize] async (ClaimsPrincipal principal, NexumDbContext db, CancellationToken ct) =>
{
    var email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Email);
    if (string.IsNullOrWhiteSpace(email))
    {
        return Results.Unauthorized();
    }

    var cliente = await db.Clientes
        .AsNoTracking()
        .FirstOrDefaultAsync(item => item.Email == email, ct);

    if (cliente is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Cliente nao localizado para esta sessao."));
    }

    var pedidos = await db.Pedidos
        .Include(item => item.Pagamentos)
        .AsNoTracking()
        .Where(item => item.ClienteId == cliente.Id)
        .OrderByDescending(item => item.CreatedAt)
        .Select(item => new ClientePortalPedidoDto(
            item.Id,
            item.NumeroPedido,
            item.Status.ToString(),
            item.StatusPagamento.ToString(),
            item.Total,
            item.CreatedAt,
            item.MeioPagamento ?? item.GatewayPagamento,
            item.FreteCodigoRastreio,
            item.FreteTransportadora))
        .ToListAsync(ct);

    var pedidoIds = pedidos.Select(item => item.Id).ToList();
    var documentos = pedidoIds.Count == 0
        ? []
        : await db.Fiscais
            .AsNoTracking()
            .Where(item => pedidoIds.Contains(item.PedidoId))
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => new ClientePortalDocumentoDto(
                item.Id,
                item.PedidoId,
                item.NumeroNfe,
                item.ModeloDocumento,
                item.StatusNfe.ToString(),
                item.ChaveAcesso,
                item.DanfeUrl,
                item.XmlUrl,
                item.CreatedAt))
            .ToListAsync(ct);

    var totalCompras = pedidos.Sum(item => item.Total);
    var score = cliente.Vip ? "Premium" : cliente.PontosFidelidade >= 500 ? "Gold" : cliente.PontosFidelidade >= 150 ? "Silver" : "Start";
    var portal = new ClientePortalDto(
        cliente.Id,
        cliente.Nome,
        cliente.Email,
        cliente.Telefone,
        cliente.CpfCnpj,
        cliente.PontosFidelidade,
        score,
        cliente.Vip,
        Math.Round(totalCompras / 10m, 2),
        pedidos,
        documentos,
        [
            "Canal direto com o Grupo Nexum Altivon.",
            "Pontuação de fidelidade acumulada por compras aprovadas.",
            "Espaço preparado para limites e relacionamento futuro."
        ]);

    return Results.Ok(ApiResponse<ClientePortalDto>.Ok(portal));
})
.WithName("ClientePortalMe")
;

app.MapGet("/api/fornecedores", [Authorize(Policy = "Gerente")] async (NexumDbContext db, CancellationToken ct) =>
{
    var fornecedores = await db.Fornecedores
        .AsNoTracking()
        .OrderByDescending(fornecedor => fornecedor.CreatedAt)
        .Take(500)
        .Select(fornecedor => new FornecedorDto(
            fornecedor.Id,
            string.IsNullOrWhiteSpace(fornecedor.NomeFantasia) ? fornecedor.RazaoSocial : fornecedor.NomeFantasia,
            fornecedor.Cnpj ?? string.Empty,
            fornecedor.Email ?? string.Empty,
            fornecedor.Telefone ?? string.Empty,
            fornecedor.Segmento ?? "Geral",
            fornecedor.CreatedAt))
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<FornecedorDto>>.Ok(fornecedores));
})
.WithName("Fornecedores")
;

app.MapPost("/api/fornecedores", [Authorize(Policy = "Gerente")] async (FornecedorRequest request, NexumDbContext db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Nome))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Nome do fornecedor obrigatorio."));
    }

    var documento = string.IsNullOrWhiteSpace(request.Documento) ? null : request.Documento.Trim();
    var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();
    var fornecedorExistente = await db.Fornecedores.FirstOrDefaultAsync(fornecedor =>
        (!string.IsNullOrWhiteSpace(documento) && fornecedor.Cnpj == documento) ||
        (!string.IsNullOrWhiteSpace(email) && fornecedor.Email != null && fornecedor.Email.ToLower() == email), ct);

    if (fornecedorExistente is not null)
    {
        return Results.Conflict(ApiResponse<string>.Erro("Fornecedor ja cadastrado com este documento ou e-mail."));
    }

    var fornecedor = new Fornecedor
    {
        RazaoSocial = request.Nome.Trim(),
        Cnpj = documento,
        Email = email,
        Telefone = request.Telefone,
        Segmento = request.Categoria,
        Status = StatusFornecedor.Ativo,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.Fornecedores.Add(fornecedor);
    await db.SaveChangesAsync(ct);

    var dto = new FornecedorDto(
        fornecedor.Id,
        string.IsNullOrWhiteSpace(fornecedor.NomeFantasia) ? fornecedor.RazaoSocial : fornecedor.NomeFantasia,
        fornecedor.Cnpj ?? string.Empty,
        fornecedor.Email ?? string.Empty,
        fornecedor.Telefone ?? string.Empty,
        fornecedor.Segmento ?? "Geral",
        fornecedor.CreatedAt);

    return Results.Ok(ApiResponse<FornecedorDto>.Ok(dto, "Fornecedor cadastrado."));
})
.WithName("CriarFornecedor")
;

app.MapGet("/api/pedidos", [Authorize(Policy = "Gerente")] async (NexumDbContext db, CancellationToken ct) =>
{
    var pedidos = await db.Pedidos
        .AsNoTracking()
        .Include(pedido => pedido.Pagamentos)
        .OrderByDescending(pedido => pedido.CreatedAt)
        .Take(500)
        .ToListAsync(ct);

    var dtos = pedidos.Select(pedido =>
    {
        var pagamentoAtual = pedido.Pagamentos?
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefault();

        return new PedidoLojaDto(
        pedido.Id,
        pedido.NumeroPedido,
        pedido.Total,
        FormatStatusPedido(pedido.Status),
        pedido.CreatedAt,
        FormatStatusPagamento(pedido.StatusPagamento),
        pedido.MeioPagamento,
        pedido.GatewayPagamento,
        pedido.GatewayTransacaoId,
        pedido.FreteValor,
        pedido.FreteMetodo,
        pedido.FreteTransportadora,
        pedido.FretePrazoDias,
        BuildPedidoInstruction(pedido.StatusPagamento, pedido.MeioPagamento, pedido.GatewayTransacaoId),
        pagamentoAtual?.Parcelas ?? 1,
        pagamentoAtual?.PixQrcode,
        pagamentoAtual?.BoletoUrl);
    }).ToList();

    return Results.Ok(ApiResponse<List<PedidoLojaDto>>.Ok(dtos));
})
.WithName("Pedidos")
;

app.MapPut("/api/pedidos/{id}/status", [Authorize(Policy = "Gerente")] async (
    int id,
    StatusUpdateRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
    var pedido = await db.Pedidos
        .Include(item => item.Itens)
        .Include(item => item.Pagamentos)
        .FirstOrDefaultAsync(item => item.Id == id, ct);
    if (pedido is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Pedido nao encontrado."));
    }

    if (!TryParseStatusPedido(request.NovoStatus, out var status))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Status do pedido invalido."));
    }

    var statusAnterior = pedido.Status;
    var statusAnteriorConfirmaEstoque = statusAnterior is StatusPedido.Pago or StatusPedido.EmSeparacao or StatusPedido.Enviado or StatusPedido.Entregue;
    var novoStatusConfirmaEstoque = status is StatusPedido.Pago or StatusPedido.EmSeparacao or StatusPedido.Enviado or StatusPedido.Entregue;
    var novoStatusCancelaEstoque = status is StatusPedido.Cancelado or StatusPedido.Devolvido or StatusPedido.Reembolsado;

    if (pedido.Itens is { Count: > 0 } && statusAnterior != status)
    {
        var produtoIds = pedido.Itens
            .Where(item => item.ProdutoId.HasValue)
            .Select(item => item.ProdutoId!.Value)
            .Distinct()
            .ToList();
        var produtos = await db.Produtos
            .Where(produto => produtoIds.Contains(produto.Id))
            .ToDictionaryAsync(produto => produto.Id, ct);

        foreach (var item in pedido.Itens)
        {
            if (!item.ProdutoId.HasValue || !produtos.TryGetValue(item.ProdutoId.Value, out var produto))
            {
                return Results.BadRequest(ApiResponse<string>.Erro($"Produto do pedido nao encontrado: {item.NomeProduto}."));
            }

            if (!statusAnteriorConfirmaEstoque && novoStatusConfirmaEstoque)
            {
                if (produto.EstoqueAtual < item.Quantidade)
                {
                    return Results.BadRequest(ApiResponse<string>.Erro($"Estoque insuficiente para confirmar {item.NomeProduto}."));
                }

                produto.EstoqueReservado = Math.Max(0, produto.EstoqueReservado - item.Quantidade);
                produto.EstoqueAtual -= item.Quantidade;
            }
            else if (!statusAnteriorConfirmaEstoque && novoStatusCancelaEstoque)
            {
                produto.EstoqueReservado = Math.Max(0, produto.EstoqueReservado - item.Quantidade);
            }
            else if (statusAnteriorConfirmaEstoque && novoStatusCancelaEstoque)
            {
                produto.EstoqueAtual += item.Quantidade;
            }

            produto.UpdatedAt = DateTime.UtcNow;
        }
    }

    pedido.Status = status;
    if (novoStatusConfirmaEstoque && pedido.StatusPagamento == StatusPagamento.Aguardando)
    {
        pedido.StatusPagamento = StatusPagamento.Aprovado;
        pedido.DataPagamento ??= DateTime.UtcNow;
    }
    else if (status == StatusPedido.Cancelado && pedido.StatusPagamento == StatusPagamento.Aguardando)
    {
        pedido.StatusPagamento = StatusPagamento.Cancelado;
    }

    if (pedido.Pagamentos is { Count: > 0 })
    {
        foreach (var pagamento in pedido.Pagamentos)
        {
            pagamento.Status = pedido.StatusPagamento switch
            {
                StatusPagamento.Aprovado => StatusPagamentoDetalhado.Aprovado,
                StatusPagamento.Recusado => StatusPagamentoDetalhado.Recusado,
                StatusPagamento.Estornado => StatusPagamentoDetalhado.Estornado,
                StatusPagamento.Cancelado => StatusPagamentoDetalhado.Cancelado,
                _ => pagamento.Status
            };
            pagamento.DataProcessamento ??= pedido.DataPagamento;
            pagamento.UpdatedAt = DateTime.UtcNow;
        }
    }

    pedido.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);

    var pagamentoAtual = pedido.Pagamentos?
        .OrderByDescending(item => item.CreatedAt)
        .FirstOrDefault();

    var dto = new PedidoLojaDto(
        pedido.Id,
        pedido.NumeroPedido,
        pedido.Total,
        FormatStatusPedido(pedido.Status),
        pedido.CreatedAt,
        FormatStatusPagamento(pedido.StatusPagamento),
        pedido.MeioPagamento,
        pedido.GatewayPagamento,
        pedido.GatewayTransacaoId,
        pedido.FreteValor,
        pedido.FreteMetodo,
        pedido.FreteTransportadora,
        pedido.FretePrazoDias,
        BuildPedidoInstruction(pedido.StatusPagamento, pedido.MeioPagamento, pedido.GatewayTransacaoId),
        pagamentoAtual?.Parcelas ?? 1,
        pagamentoAtual?.PixQrcode,
        pagamentoAtual?.BoletoUrl);
    return Results.Ok(ApiResponse<PedidoLojaDto>.Ok(dto, "Status do pedido atualizado."));
})
.WithName("AtualizarStatusPedido")
;

app.MapPost("/api/pedidos", async (
    PedidoRequest request,
    NexumDbContext db,
    IConfiguration configuration,
    IFiscalRoutingEngine fiscalRoutingEngine,
    IHttpClientFactory httpClientFactory,
    HttpContext http,
    CancellationToken ct) =>
{
    if (request.Itens is null || request.Itens.Count == 0)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Itens do pedido obrigatorios."));
    }

    if (request.Itens.Any(item => string.IsNullOrWhiteSpace(item.ProdutoId) || item.Quantidade <= 0))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Produto e quantidade devem ser validos."));
    }

    var itensSolicitados = request.Itens
        .GroupBy(item => item.ProdutoId.Trim(), StringComparer.OrdinalIgnoreCase)
        .Select(group => new PedidoItemRequest(group.Key, group.Sum(item => item.Quantidade)))
        .ToList();

    var cliente = await db.Clientes.FirstOrDefaultAsync(item => item.Id == request.ClienteId, ct);
    if (cliente is null)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Cliente invalido."));
    }

    int? lojaId = null;
    if (int.TryParse(request.LojaId, out var lojaIdParsed))
    {
        lojaId = lojaIdParsed;
    }

    await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
    var produtoSlugs = itensSolicitados.Select(item => item.ProdutoId).ToList();
    var produtosMap = await db.Produtos
        .Include(produto => produto.Fornecedor)
        .Where(produto => produto.Ativo && produtoSlugs.Contains(produto.Slug))
        .ToDictionaryAsync(produto => produto.Slug, ct);

    if (produtosMap.Count != produtoSlugs.Count)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Um ou mais produtos nao estao disponiveis."));
    }

    decimal subtotal = 0m;
    var itens = new List<PedidoItem>(itensSolicitados.Count);
    var abastecimentoResumo = new List<string>(itensSolicitados.Count);
    foreach (var item in itensSolicitados)
    {
        if (!produtosMap.TryGetValue(item.ProdutoId, out var produto))
        {
            return Results.BadRequest(ApiResponse<string>.Erro("Produto invalido."));
        }

        if (produto.TipoProduto == NexumAltivon.API.Models.TipoProduto.Dropshipping && produto.FornecedorId is null)
        {
            return Results.BadRequest(ApiResponse<string>.Erro(
                $"O produto {produto.Nome} está marcado como dropshipping, mas ainda não possui fornecedor vinculado."));
        }

        var estoqueDisponivel = produto.EstoqueAtual - produto.EstoqueReservado;
        if (estoqueDisponivel < item.Quantidade)
        {
            return Results.BadRequest(ApiResponse<string>.Erro(
                $"Estoque insuficiente para {produto.Nome}. Disponivel: {Math.Max(0, estoqueDisponivel)}."));
        }

        var precoUnitario = produto.PrecoPromocional ?? produto.Preco;
        var precoTotal = precoUnitario * item.Quantidade;
        subtotal += precoTotal;
        produto.EstoqueReservado += item.Quantidade;
        produto.UpdatedAt = DateTime.UtcNow;

        itens.Add(new PedidoItem
        {
            ProdutoId = produto.Id,
            NomeProduto = produto.Nome,
            SkuProduto = produto.Sku,
            ImagemProduto = produto.ImagemPrincipal,
            Quantidade = item.Quantidade,
            PrecoUnitario = precoUnitario,
            PrecoTotal = precoTotal,
            CreatedAt = DateTime.UtcNow
        });

        abastecimentoResumo.Add(BuildAbastecimentoResumo(produto, item.Quantidade));
    }

    decimal desconto = 0m;
    if (!string.IsNullOrWhiteSpace(request.CupomCodigo))
    {
        var cupom = await db.Cupons.AsNoTracking().FirstOrDefaultAsync(c => c.Codigo == request.CupomCodigo && c.Ativo, ct);
        if (cupom is not null && subtotal >= cupom.ValorMinimoPedido)
        {
            desconto = cupom.Tipo switch
            {
                TipoCupom.Percentual => subtotal * (cupom.Valor / 100m),
                TipoCupom.ValorFixo => cupom.Valor,
                TipoCupom.FreteGratis => cupom.Valor,
                _ => 0m
            };
            desconto = Math.Min(desconto, subtotal);
        }
    }

    int? enderecoEntregaId = null;
    if (request.EnderecoEntrega is not null)
    {
        var enderecoJson = System.Text.Json.JsonSerializer.Serialize(request.EnderecoEntrega);
        var enderecoRequest = System.Text.Json.JsonSerializer.Deserialize<EnderecoEntregaRequest>(
            enderecoJson,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (enderecoRequest is not null && !string.IsNullOrWhiteSpace(enderecoRequest.Cep) && !string.IsNullOrWhiteSpace(enderecoRequest.Logradouro))
        {
            var endereco = new Endereco
            {
                ClienteId = cliente.Id,
                Tipo = TipoEndereco.Entrega,
                Apelido = "Entrega",
                Cep = enderecoRequest.Cep.Trim(),
                Logradouro = enderecoRequest.Logradouro.Trim(),
                Numero = enderecoRequest.Numero?.Trim() ?? "S/N",
                Complemento = enderecoRequest.Complemento,
                Bairro = enderecoRequest.Bairro,
                Cidade = enderecoRequest.Cidade,
                Estado = enderecoRequest.Estado,
                Pais = "Brasil",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Enderecos.Add(endereco);
            await db.SaveChangesAsync(ct);
            enderecoEntregaId = endereco.Id;
        }
    }

    var pedido = new Pedido
    {
        NumeroPedido = $"NX{DateTime.UtcNow:yyMMddHHmmss}{Random.Shared.Next(10, 99)}",
        ClienteId = cliente.Id,
        EnderecoEntregaId = enderecoEntregaId,
        LojaId = lojaId,
        Status = StatusPedido.Pendente,
        StatusPagamento = StatusPagamento.Aguardando,
        MeioPagamento = request.MetodoPagamento,
        GatewayPagamento = string.IsNullOrWhiteSpace(request.GatewayPagamento) ? "ConfiguracaoPendente" : request.GatewayPagamento,
        Subtotal = subtotal,
        Desconto = desconto,
        FreteValor = request.FreteValor ?? 0m,
        FreteMetodo = request.FreteMetodo,
        FreteTransportadora = request.FreteTransportadora,
        FretePrazoDias = request.FretePrazoDias ?? 0,
        Total = Math.Max(0m, subtotal + (request.FreteValor ?? 0m) - desconto),
        CupomCodigo = request.CupomCodigo,
        Origem = OrigemPedido.Site,
        IpCliente = http.Connection.RemoteIpAddress?.ToString(),
        UserAgent = http.Request.Headers.UserAgent.ToString(),
        ObservacoesInternas = abastecimentoResumo.Count == 0
            ? null
            : $"Abastecimento automático: {string.Join(" | ", abastecimentoResumo)}",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Itens = itens
    };

    pedido.Pagamentos = new List<Pagamento>
    {
        new()
        {
            Gateway = string.IsNullOrWhiteSpace(pedido.GatewayPagamento) ? "ConfiguracaoPendente" : pedido.GatewayPagamento,
            Metodo = ParseMetodoPagamento(pedido.MeioPagamento),
            Status = StatusPagamentoDetalhado.Pendente,
            Valor = pedido.Total,
            Parcelas = Math.Clamp(request.Parcelas ?? 1, 1, 24),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }
    };

    db.Pedidos.Add(pedido);
    await db.SaveChangesAsync(ct);

    await EnsurePedidoFiscalAutomationAsync(pedido, cliente, request, db, fiscalRoutingEngine, ct);
    await db.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);

    var gatewayResult = await TryStartGatewayPaymentAsync(pedido, cliente, configuration, httpClientFactory, http, ct);
    if (gatewayResult.Started)
    {
        pedido.GatewayPagamento = gatewayResult.Gateway;
        pedido.GatewayTransacaoId = gatewayResult.TransactionId;
        pedido.StatusPagamento = StatusPagamento.Aguardando;
        var pagamento = pedido.Pagamentos?.FirstOrDefault();
        if (pagamento is not null)
        {
            pagamento.Gateway = gatewayResult.Gateway;
            pagamento.GatewayTransacaoId = gatewayResult.TransactionId;
            pagamento.PixQrcode = gatewayResult.PixQrcode;
            pagamento.BoletoUrl = gatewayResult.PaymentUrl;
            pagamento.Status = StatusPagamentoDetalhado.Pendente;
            pagamento.WebhookPayload = gatewayResult.RawPayload;
            pagamento.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
    }

    var pagamentoFinal = pedido.Pagamentos?
        .OrderByDescending(item => item.CreatedAt)
        .FirstOrDefault();

    var dto = new PedidoLojaDto(
        pedido.Id,
        pedido.NumeroPedido,
        pedido.Total,
        FormatStatusPedido(pedido.Status),
        pedido.CreatedAt,
        FormatStatusPagamento(pedido.StatusPagamento),
        pedido.MeioPagamento,
        pedido.GatewayPagamento,
        pedido.GatewayTransacaoId,
        pedido.FreteValor,
        pedido.FreteMetodo,
        pedido.FreteTransportadora,
        pedido.FretePrazoDias,
        BuildPedidoInstruction(pedido.StatusPagamento, pedido.MeioPagamento, pedido.GatewayTransacaoId),
        pagamentoFinal?.Parcelas ?? 1,
        pagamentoFinal?.PixQrcode,
        pagamentoFinal?.BoletoUrl);
    return Results.Ok(ApiResponse<PedidoLojaDto>.Ok(dto, "Pedido criado com sucesso."));
})
.AllowAnonymous()
.WithName("CriarPedido")
;

app.MapGet("/api/integracoes/status", [Authorize(Policy = "Gerente")] async (
    IConfiguration configuration,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var mercadoPagoConfigurado = IsConfiguredSecret(GetIntegrationValue(configuration, "MercadoPago:AccessToken", "Integracoes:MercadoPago:AccessToken"));
    var melhorEnvioConfigurado = IsConfiguredSecret(GetIntegrationValue(configuration, "MelhorEnvio:Token", "Integracoes:MelhorEnvio:Token"));
    var mercadoLivreConfigurado = IsConfiguredSecret(GetIntegrationValue(configuration, "MercadoLivre:AccessToken", "Integracoes:MercadoLivre:AccessToken"));
    var mercadoLivreOAuthPronto = HasAllIntegrationValues(
        configuration,
        ("MercadoLivre:AppId", "Integracoes:MercadoLivre:AppId"),
        ("MercadoLivre:ClientSecret", "Integracoes:MercadoLivre:ClientSecret"),
        ("MercadoLivre:RedirectUri", "Integracoes:MercadoLivre:RedirectUri"));
    var shopifyConfigurado = HasAllIntegrationValues(
        configuration,
        ("Shopify:StoreDomain", "Integracoes:Shopify:StoreDomain"),
        ("Shopify:AdminApiAccessToken", "Integracoes:Shopify:AdminApiAccessToken"),
        ("Shopify:ApiVersion", "Integracoes:Shopify:ApiVersion"));
    var cjDropshippingConfigurado = HasAllIntegrationValues(
        configuration,
        ("CJDropshipping:ApiEndpoint", "Integracoes:CJDropshipping:ApiEndpoint"),
        ("CJDropshipping:AccessToken", "Integracoes:CJDropshipping:AccessToken"));
    var fornecedoresAtivos = await db.Fornecedores
        .AsNoTracking()
        .CountAsync(fornecedor => fornecedor.Status == StatusFornecedor.Ativo, ct);
    var dropshippingAtivos = await db.DropshippingConfigs
        .AsNoTracking()
        .CountAsync(config => config.Ativo, ct);
    var shopifyCanalAtivo = await db.DropshippingConfigs
        .AsNoTracking()
        .AnyAsync(config => config.Ativo && config.Slug == "shopify", ct);
    var cjCanalAtivo = await db.DropshippingConfigs
        .AsNoTracking()
        .AnyAsync(config => config.Ativo && config.Slug == "cjdropshipping", ct);

    var modules = new List<IntegracaoStatusDto>
    {
        new(
            "E-commerce e API",
            "ecommerce",
            "Operacional",
            "Catalogo, clientes, pedidos, estoque e painel usam a API operacional.",
            true,
            "Producao"),
        new(
            "Dropshipping",
            "dropshipping",
            fornecedoresAtivos + dropshippingAtivos > 0 ? "Base ativa" : "Aguardando cadastros",
            fornecedoresAtivos + dropshippingAtivos > 0
                ? $"{fornecedoresAtivos} fornecedor(es) ativo(s) e {dropshippingAtivos} canal(is) dropshipping disponivel(is) para roteamento."
                : "Cadastre fornecedores/canais e vincule produtos antes de liberar o roteamento.",
            fornecedoresAtivos + dropshippingAtivos > 0,
            "Producao assistida"),
        new(
            "Shopify",
            "shopify",
            shopifyConfigurado ? "Credenciais prontas" : shopifyCanalAtivo ? "Canal publicado" : "Aguardando conexão",
            shopifyConfigurado
                ? "Loja Shopify preparada para autenticar catálogo, pedidos e estoque assim que os tokens reais forem inseridos."
                : shopifyCanalAtivo
                    ? "Canal Shopify já está publicado no sistema e aguardando domínio/tokens oficiais."
                    : "Estrutura Shopify será habilitada após cadastrar domínio da loja e token Admin API.",
            shopifyConfigurado || shopifyCanalAtivo,
            shopifyConfigurado ? "Staging pronto" : "Aguardando credenciais"),
        new(
            "CJ Dropshipping",
            "cjdropshipping",
            cjDropshippingConfigurado ? "Credenciais prontas" : cjCanalAtivo ? "Canal publicado" : "Aguardando conexão",
            cjDropshippingConfigurado
                ? "Canal CJ preparado para catálogo, sourcing e roteamento de pedidos conforme as credenciais do fornecedor."
                : cjCanalAtivo
                    ? "Canal CJ Dropshipping já está publicado no sistema e aguardando token real da operação."
                    : "Estrutura CJ Dropshipping será habilitada após cadastrar endpoint e token da conta contratada.",
            cjDropshippingConfigurado || cjCanalAtivo,
            cjDropshippingConfigurado ? "Staging pronto" : "Aguardando credenciais"),
        new(
            "Logistica e Fretes",
            "logistica",
            melhorEnvioConfigurado ? "Configurado" : "Aguardando credenciais",
            melhorEnvioConfigurado
                ? "Token logístico encontrado; falta concluir o teste de cotacao e etiqueta."
                : "Checkout registra frete, mas cotacao e etiqueta dependem do token da transportadora.",
            melhorEnvioConfigurado,
            configuration.GetValue("MelhorEnvio:Sandbox", true) ? "Sandbox" : "Producao"),
        new(
            "Gateways de pagamento",
            "gateways",
            mercadoPagoConfigurado ? "Configurado" : "Aguardando credenciais",
            mercadoPagoConfigurado
                ? "Credencial do gateway encontrada; falta validar cobranca e webhook."
                : "Pedido registra o metodo, mas a cobranca depende do token do gateway.",
            mercadoPagoConfigurado,
            mercadoPagoConfigurado ? "Configurado" : "Nao configurado"),
        new(
            "Marketplaces",
            "marketplaces",
            mercadoLivreConfigurado ? "Token ativo" : mercadoLivreOAuthPronto ? "OAuth pronto" : "Aguardando credenciais",
            mercadoLivreConfigurado
                ? "Access token encontrado; sincronizacao pode ser testada contra a API do Mercado Livre."
                : mercadoLivreOAuthPronto
                    ? "Aplicacao pronta para autorizacao do vendedor; falta concluir login OAuth e gerar access token."
                    : "Importacao de catalogo e pedidos depende das credenciais do marketplace.",
            mercadoLivreConfigurado || mercadoLivreOAuthPronto,
            "Integracao externa"),
        new(
            "Bancos e conciliacao",
            "bancaria",
            "Planejado",
            "A conciliacao bancaria sera ativada apos definir banco, convenio e credenciais seguras.",
            false,
            "Nao configurado")
    };

    return Results.Ok(ApiResponse<List<IntegracaoStatusDto>>.Ok(modules));
})
.WithName("IntegracoesStatus")
;

app.MapGet("/api/integracoes/diagnostico", [Authorize(Policy = "Gerente")] async (
    IConfiguration configuration,
    NexumDbContext db,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct) =>
{
    var slugs = new[] { "ecommerce", "dropshipping", "shopify", "cjdropshipping", "mercadopago", "melhorenvio", "mercadolivre", "bancaria" };
    var resultados = new List<IntegracaoDiagnosticoDto>();

    foreach (var slug in slugs)
    {
        resultados.Add(await BuildIntegracaoDiagnosticoAsync(slug, configuration, db, httpClientFactory, ct));
    }

    return Results.Ok(ApiResponse<List<IntegracaoDiagnosticoDto>>.Ok(resultados, "Diagnostico operacional das integracoes."));
})
.WithName("IntegracoesDiagnostico")
;

app.MapGet("/api/integracoes/credenciais-modelo", [Authorize(Policy = "Gerente")] () =>
{
    var credenciais = new List<IntegracaoCredencialDto>
    {
        new("Mercado Pago", "gateway", "MercadoPago__AccessToken", "Token privado do Mercado Pago para criar cobranças Pix/boleto/cartão.", true),
        new("Mercado Pago", "gateway", "MercadoPago__PublicKey", "Chave pública usada no checkout transparente quando o front capturar cartão.", false),
        new("Mercado Pago", "gateway", "MercadoPago__WebhookSecret", "Segredo para validar notificações/webhooks do Mercado Pago.", false),
        new("Gateway principal", "gateway", "GatewayPrincipal__Provider / GatewayPrincipal__AccessToken / GatewayPrincipal__PublicKey / GatewayPrincipal__WebhookSecret", "Estrutura reserva para o primeiro gateway adicional escolhido pela diretoria.", false),
        new("Gateway secundário", "gateway", "GatewaySecundario__Provider / GatewaySecundario__AccessToken / GatewaySecundario__PublicKey / GatewaySecundario__WebhookSecret", "Estrutura reserva para o segundo gateway adicional e contingência de cobrança.", false),
        new("Melhor Envio", "logistica", "MelhorEnvio__Token", "Token Bearer do Melhor Envio para cotação, compra de frete e etiqueta.", true),
        new("Melhor Envio", "logistica", "MelhorEnvio__Sandbox", "true para homologação; false para produção.", false),
        new("Logística principal", "logistica", "LogisticaPrincipal__Provider / LogisticaPrincipal__ApiEndpoint / LogisticaPrincipal__Token / LogisticaPrincipal__ClientId / LogisticaPrincipal__ClientSecret", "Estrutura para a principal transportadora/hub escolhida para produção.", false),
        new("Logística secundária", "logistica", "LogisticaSecundaria__Provider / LogisticaSecundaria__ApiEndpoint / LogisticaSecundaria__Token / LogisticaSecundaria__ClientId / LogisticaSecundaria__ClientSecret", "Estrutura de contingência para uma segunda transportadora ou hub logístico.", false),
        new("Dropshipping principal", "dropshipping", "DropshippingPrincipal__Provider / DropshippingPrincipal__ApiEndpoint / DropshippingPrincipal__ApiKey / DropshippingPrincipal__ApiSecret", "Canal principal de dropshipping preparado para receber as credenciais reais.", false),
        new("Dropshipping secundário", "dropshipping", "DropshippingSecundario__Provider / DropshippingSecundario__ApiEndpoint / DropshippingSecundario__ApiKey / DropshippingSecundario__ApiSecret", "Canal secundário para contingência ou operação paralela de dropshipping.", false),
        new("Shopify", "dropshipping", "Shopify__StoreDomain", "Domínio da loja Shopify que será sincronizada com catálogo, estoque e pedidos.", true),
        new("Shopify", "dropshipping", "Shopify__ApiVersion", "Versão da Admin API usada pelo conector privado do servidor.", true),
        new("Shopify", "dropshipping", "Shopify__AdminApiAccessToken", "Token privado Admin API da loja Shopify.", true),
        new("Shopify", "dropshipping", "Shopify__WebhookSecret", "Segredo para validar webhooks de pedido, produto e estoque da Shopify.", false),
        new("CJ Dropshipping", "dropshipping", "CJDropshipping__ApiEndpoint", "Endpoint base da API privada do CJ Dropshipping.", true),
        new("CJ Dropshipping", "dropshipping", "CJDropshipping__AccessToken", "Token principal para sincronizar produtos e pedidos com o CJ Dropshipping.", true),
        new("CJ Dropshipping", "dropshipping", "CJDropshipping__ApiKey", "Chave complementar da conta CJ quando o contrato exigir autenticação dupla.", false),
        new("CJ Dropshipping", "dropshipping", "CJDropshipping__WebhookSecret", "Segredo para validar notificações recebidas do CJ Dropshipping.", false),
        new("Mercado Livre", "marketplace", "MercadoLivre__AppId", "ID do aplicativo Mercado Livre.", true),
        new("Mercado Livre", "marketplace", "MercadoLivre__ClientSecret", "Segredo do aplicativo Mercado Livre.", true),
        new("Mercado Livre", "marketplace", "MercadoLivre__RedirectUri", "URL de retorno cadastrada exatamente no Mercado Livre.", true),
        new("Mercado Livre", "marketplace", "MercadoLivre__AccessToken", "Token do vendedor após autorizar o aplicativo.", false),
        new("Integrações bancárias", "bancaria", "Banco__Provider / Banco__ClientId / Banco__ClientSecret", "Credenciais do banco ou PSP escolhido para conciliação.", false)
    };

    return Results.Ok(ApiResponse<List<IntegracaoCredencialDto>>.Ok(credenciais, "Credenciais necessarias sem expor valores sensiveis."));
})
.WithName("IntegracoesCredenciaisModelo")
;

app.MapPost("/api/integracoes/testar/{slug}", [Authorize(Policy = "Gerente")] async (
    string slug,
    IConfiguration configuration,
    NexumDbContext db,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct) =>
{
    var resultado = await BuildIntegracaoDiagnosticoAsync(slug, configuration, db, httpClientFactory, ct);
    return Results.Ok(ApiResponse<IntegracaoDiagnosticoDto>.Ok(resultado, $"Teste executado para {resultado.Nome}."));
})
.WithName("TestarIntegracao")
;

app.MapPost("/api/frete/cotar", async (
    FreteCotacaoRequest request,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct) =>
{
    var cotacoes = await CotarFreteAsync(request, configuration, httpClientFactory, ct);
    return Results.Ok(ApiResponse<List<FreteCotacaoDto>>.Ok(cotacoes, cotacoes.Any(c => c.Fonte == "Melhor Envio")
        ? "Cotacao consultada no Melhor Envio."
        : "Cotacao operacional gerada pela tabela local ate configurar a transportadora."));
})
.AllowAnonymous()
.WithName("CotarFrete")
;

app.MapPost("/api/webhooks/mercadopago", async (
    HttpContext http,
    IConfiguration configuration,
    NexumDbContext db,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct) =>
{
    using var reader = new StreamReader(http.Request.Body, Encoding.UTF8);
    var payload = await reader.ReadToEndAsync(ct);
    var paymentId = http.Request.Query["id"].FirstOrDefault()
        ?? http.Request.Query["data.id"].FirstOrDefault()
        ?? TryExtractJsonPath(payload, "data", "id")
        ?? TryExtractJsonField(payload, "id");

    if (!IsConfiguredSecret(paymentId))
    {
        return Results.Ok(new { received = true, updated = false, reason = "sem_id_pagamento" });
    }

    var token = GetIntegrationValue(configuration, "MercadoPago:AccessToken", "Integracoes:MercadoPago:AccessToken");
    string? status = null;
    string? rawPaymentPayload = null;

    if (IsConfiguredSecret(token))
    {
        try
        {
            var client = httpClientFactory.CreateClient("mercado-pago");
            using var request = new HttpRequestMessage(HttpMethod.Get, $"v1/payments/{paymentId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var response = await client.SendAsync(request, ct);
            rawPaymentPayload = await response.Content.ReadAsStringAsync(ct);
            if (response.IsSuccessStatusCode)
            {
                status = TryExtractJsonField(rawPaymentPayload, "status");
            }
        }
        catch
        {
            rawPaymentPayload = payload;
        }
    }

    var pagamento = await db.Pagamentos
        .Include(item => item.Pedido)
        .FirstOrDefaultAsync(item => item.GatewayTransacaoId == paymentId, ct);

    if (pagamento is null)
    {
        return Results.Ok(new { received = true, updated = false, reason = "pagamento_nao_encontrado" });
    }

    pagamento.WebhookPayload = rawPaymentPayload ?? payload;
    pagamento.DataProcessamento = DateTime.UtcNow;
    pagamento.UpdatedAt = DateTime.UtcNow;

    if (!string.IsNullOrWhiteSpace(status))
    {
        ApplyMercadoPagoStatus(status, pagamento);
    }

    await db.SaveChangesAsync(ct);
    return Results.Ok(new { received = true, updated = true, paymentId, status });
})
.AllowAnonymous()
.WithName("WebhookMercadoPago")
;

app.MapGet("/api/dashboard/resumo", [Authorize(Policy = "Gerente")] async (NexumDbContext db, CancellationToken ct) =>
{
    var hoje = DateTime.UtcNow.Date;
    var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

    var pedidosHoje = await db.Pedidos.AsNoTracking().CountAsync(pedido => pedido.CreatedAt >= hoje, ct);
    var totalClientes = await db.Clientes.AsNoTracking().CountAsync(ct);
    var faturamentoMes = await db.Pedidos.AsNoTracking().Where(pedido => pedido.CreatedAt >= inicioMes).SumAsync(pedido => (decimal?)pedido.Total, ct) ?? 0m;
    var leadsNovos = await db.CrmLeads.AsNoTracking().CountAsync(lead => lead.Status == StatusLead.Novo, ct);
    var produtosEstoqueBaixo = await db.Produtos.AsNoTracking().CountAsync(produto => produto.Ativo && produto.EstoqueAtual <= produto.EstoqueMinimo, ct);

    var totalLeads = await db.CrmLeads.AsNoTracking().CountAsync(ct);
    var leadsConvertidos = await db.CrmLeads.AsNoTracking().CountAsync(lead => lead.Status == StatusLead.Convertido, ct);
    var conversao = totalLeads == 0 ? 0m : Math.Round((decimal)leadsConvertidos / totalLeads * 100m, 2);

    var ticketMedio = await db.Pedidos.AsNoTracking()
        .Where(pedido => pedido.CreatedAt >= inicioMes)
        .AverageAsync(pedido => (decimal?)pedido.Total, ct) ?? 0m;

    var resumo = new DashboardResumoDto(
        pedidosHoje,
        totalClientes,
        faturamentoMes,
        leadsNovos,
        produtosEstoqueBaixo,
        conversao,
        ticketMedio);

    return Results.Ok(ApiResponse<DashboardResumoDto>.Ok(resumo));
})
.WithName("DashboardResumo")
;

app.MapGet("/api/crm/leads", [Authorize(Policy = "Gerente")] async (NexumDbContext db, CancellationToken ct) =>
{
    var leads = await db.Database.SqlQueryRaw<LeadRow>(
            """
            SELECT
                id AS Id,
                nome AS Nome,
                COALESCE(email, '') AS Email,
                COALESCE(telefone, '') AS Telefone,
                COALESCE(empresa, '') AS Empresa,
                CAST(origem AS CHAR) AS Origem,
                COALESCE(anotacoes, '') AS Mensagem,
                CAST(status AS CHAR) AS Status,
                created_at AS CreatedAt
            FROM crm_leads
            ORDER BY created_at DESC
            LIMIT 500
            """)
        .ToListAsync(ct);

    var dtos = leads.Select(lead => new LeadLojaDto(
        lead.Id,
        lead.Nome,
        lead.Email,
        lead.Telefone,
        FormatStatusLeadValue(lead.Status),
        lead.CreatedAt,
        lead.Empresa,
        FormatOrigemLeadValue(lead.Origem),
        lead.Mensagem)).ToList();

    return Results.Ok(ApiResponse<List<LeadLojaDto>>.Ok(dtos));
})
.WithName("Leads")
;

app.MapPost("/api/crm/leads", async (LeadRequest request, NexumDbContext db, INotificacaoService notificacaoService, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Nome))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Nome do lead obrigatorio."));
    }

    var email = NormalizeEmail(request.Email);
    if (string.IsNullOrWhiteSpace(email))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("E-mail do lead obrigatorio."));
    }

    var telefone = NormalizePhone(request.Telefone);
    var whatsapp = NormalizePhone(request.Whatsapp) ?? telefone;
    var cnpj = NormalizeDocument(request.Cnpj);

    var existingLead = await db.CrmLeads
        .FirstOrDefaultAsync(item =>
            item.Email == email
            || (!string.IsNullOrWhiteSpace(telefone) && item.Telefone == telefone)
            || (!string.IsNullOrWhiteSpace(cnpj) && item.Cnpj == cnpj), ct);

    var lead = existingLead ?? new CrmLead
    {
        CreatedAt = DateTime.UtcNow,
        Status = StatusLead.Novo
    };

    lead.Nome = request.Nome.Trim();
    lead.Email = email;
    lead.Telefone = telefone;
    lead.Whatsapp = whatsapp;
    lead.Empresa = TrimOrNull(request.Empresa);
    lead.Cnpj = cnpj;
    lead.Segmento = TrimOrNull(request.Segmento);
    lead.Tipo = TryParseTipoLead(request.Tipo, out var tipo) ? tipo : TipoLead.ClienteVIP;
    lead.Status = TryParseStatusLead(request.Status, out var status) ? status : lead.Status;
    lead.Origem = TryParseOrigemLead(request.Origem, out var origem) ? origem : OrigemLead.Site;
    lead.Anotacoes = AppendLeadNotes(lead.Anotacoes, BuildLeadObservacao(request));
    lead.UpdatedAt = DateTime.UtcNow;

    if (existingLead is null)
    {
        db.CrmLeads.Add(lead);
    }

    await db.SaveChangesAsync(ct);

    await notificacaoService.EnviarEmailAsync(
        "corporativo.gna@gmail.com",
        existingLead is null ? $"Novo lead público: {lead.Nome}" : $"Lead público atualizado: {lead.Nome}",
        BuildLeadNotificationEmail(lead));

    var dto = new LeadLojaDto(
        lead.Id,
        lead.Nome,
        lead.Email ?? string.Empty,
        lead.Telefone ?? string.Empty,
        FormatStatusLead(lead.Status),
        lead.CreatedAt,
        lead.Empresa,
        FormatOrigemLead(lead.Origem),
        lead.Anotacoes);

    var message = existingLead is null
        ? "Lead cadastrado no CRM."
        : "Lead já existente reaproveitado e atualizado no CRM.";

    return Results.Ok(ApiResponse<LeadLojaDto>.Ok(dto, message));
})
.WithName("CriarLead")
.AllowAnonymous()
;

app.MapPut("/api/crm/leads/{id}/status", [Authorize(Policy = "Gerente")] async (int id, StatusUpdateRequest request, NexumDbContext db, CancellationToken ct) =>
{
    var lead = await db.CrmLeads.FirstOrDefaultAsync(item => item.Id == id, ct);
    if (lead is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Lead nao encontrado."));
    }

    if (!TryParseStatusLead(request.NovoStatus, out var status))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Status do lead invalido."));
    }

    lead.Status = status;
    lead.ResponsavelId = request.ResponsavelId;
    lead.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync(ct);

    var dto = new LeadLojaDto(
        lead.Id,
        lead.Nome,
        lead.Email ?? string.Empty,
        lead.Telefone ?? string.Empty,
        FormatStatusLead(lead.Status),
        lead.CreatedAt,
        lead.Empresa,
        FormatOrigemLead(lead.Origem),
        lead.Anotacoes);
    return Results.Ok(ApiResponse<LeadLojaDto>.Ok(dto, "Status do lead atualizado."));
})
.WithName("AtualizarStatusLead")
;

app.MapGet("/api/erp/empresas", [Authorize(Policy = "Gerente")] async (NexumDbContext db, CancellationToken ct) =>
{
    var empresas = await db.EmpresasGrupo
        .AsNoTracking()
        .OrderByDescending(item => item.EmitentePreferencial)
        .ThenBy(item => item.PrioridadeFiscal)
        .ThenBy(item => item.RazaoSocial)
        .Select(item => new EmpresaGrupoDto(
            item.Id,
            item.TipoCadastro,
            item.RazaoSocial,
            item.NomeFantasia,
            item.Cnpj,
            item.InscricaoEstadual,
            item.InscricaoMunicipal,
            item.MatrizFilial,
            item.CodigoEmpresa,
            item.RegimeTributario,
            item.Crt,
            item.CnaePrincipal,
            item.CnaesSecundarios,
            item.CategoriaFiscal,
            item.SubcategoriaFiscal,
            item.NcmPadrao,
            item.NaturezaOperacaoPadrao,
            item.ResponsavelLegal,
            item.ResponsavelFiscal,
            item.EmailFiscal,
            item.EmailComercial,
            item.Telefone,
            item.Whatsapp,
            item.Cep,
            item.Logradouro,
            item.Numero,
            item.Complemento,
            item.Bairro,
            item.Cidade,
            item.Estado,
            item.Pais,
            item.AmbienteNfe,
            item.SerieNfe,
            item.SerieNfce,
            item.ModeloDocumentoPdv,
            item.AmbienteNfce,
            item.ProximaNfceNumero,
            item.NfceCsc,
            item.NfceCscIdToken,
            item.PdvSerieSat,
            item.PdvImpressoraFiscal,
            item.PdvNomeCaixaPadrao,
            item.PdvContingenciaOffline,
            item.ProximaNfeNumero,
            item.CfopPadraoInterno,
            item.CfopPadraoInterestadual,
            item.AliquotaIcmsInterna,
            item.AliquotaIcmsInterestadual,
            item.AliquotaPis,
            item.AliquotaCofins,
            item.AliquotaIss,
            item.AliquotaIpi,
            item.CargaTributariaPercentual,
            item.PerfilTributacao,
            item.UsaStLegado,
            item.DestacaIcmsStSeparado,
            item.CustoOperacionalPercentual,
            item.MargemMinimaPercentual,
            item.PrioridadeFiscal,
            item.PermiteNfeEntrada,
            item.PermiteNfeSaida,
            item.PermiteDropshipping,
            item.PermiteMarketplace,
            item.EmitentePreferencial,
            item.Ativa,
            item.BeneficiosEstrategicos,
            item.ContratoResumo,
            item.Observacoes,
            item.CreatedAt,
            item.UpdatedAt))
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<EmpresaGrupoDto>>.Ok(empresas));
})
.WithName("EmpresasGrupo")
;

app.MapPost("/api/erp/empresas", [Authorize(Policy = "Gerente")] async (EmpresaGrupoRequest request, NexumDbContext db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.RazaoSocial) || string.IsNullOrWhiteSpace(request.Cnpj))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Razão social e CNPJ são obrigatórios."));
    }

    var cnpj = NormalizeDocument(request.Cnpj);
    if (string.IsNullOrWhiteSpace(cnpj))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("CNPJ inválido."));
    }

    var codigoEmpresa = TrimOrNull(request.CodigoEmpresa);
    var emailFiscal = NormalizeEmail(request.EmailFiscal);

    var duplicate = await db.EmpresasGrupo.AsNoTracking().FirstOrDefaultAsync(item =>
        item.Cnpj == cnpj
        || (!string.IsNullOrWhiteSpace(codigoEmpresa) && item.CodigoEmpresa == codigoEmpresa), ct);

    if (duplicate is not null)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Já existe empresa fiscal cadastrada com o CNPJ ou código interno informado."));
    }

    var empresa = new EmpresaGrupo
    {
        TipoCadastro = TrimOrNull(request.TipoCadastro) ?? "GrupoSocietario",
        RazaoSocial = request.RazaoSocial.Trim(),
        NomeFantasia = TrimOrNull(request.NomeFantasia),
        Cnpj = cnpj,
        InscricaoEstadual = TrimOrNull(request.InscricaoEstadual),
        InscricaoMunicipal = TrimOrNull(request.InscricaoMunicipal),
        MatrizFilial = TrimOrNull(request.MatrizFilial),
        CodigoEmpresa = codigoEmpresa,
        RegimeTributario = TrimOrNull(request.RegimeTributario),
        Crt = TrimOrNull(request.Crt),
        CnaePrincipal = TrimOrNull(request.CnaePrincipal),
        CnaesSecundarios = TrimOrNull(request.CnaesSecundarios),
        CategoriaFiscal = TrimOrNull(request.CategoriaFiscal),
        SubcategoriaFiscal = TrimOrNull(request.SubcategoriaFiscal),
        NcmPadrao = TrimOrNull(request.NcmPadrao),
        NaturezaOperacaoPadrao = TrimOrNull(request.NaturezaOperacaoPadrao),
        ResponsavelLegal = TrimOrNull(request.ResponsavelLegal),
        ResponsavelFiscal = TrimOrNull(request.ResponsavelFiscal),
        EmailFiscal = emailFiscal,
        EmailComercial = NormalizeEmail(request.EmailComercial),
        Telefone = NormalizePhone(request.Telefone),
        Whatsapp = NormalizePhone(request.Whatsapp),
        Cep = TrimOrNull(request.Cep),
        Logradouro = TrimOrNull(request.Logradouro),
        Numero = TrimOrNull(request.Numero),
        Complemento = TrimOrNull(request.Complemento),
        Bairro = TrimOrNull(request.Bairro),
        Cidade = TrimOrNull(request.Cidade),
        Estado = TrimOrNull(request.Estado)?.ToUpperInvariant(),
        Pais = TrimOrNull(request.Pais) ?? "Brasil",
        AmbienteNfe = TrimOrNull(request.AmbienteNfe),
        SerieNfe = TrimOrNull(request.SerieNfe),
        SerieNfce = TrimOrNull(request.SerieNfce),
        ModeloDocumentoPdv = TrimOrNull(request.ModeloDocumentoPdv) ?? "NFCe",
        AmbienteNfce = TrimOrNull(request.AmbienteNfce) ?? TrimOrNull(request.AmbienteNfe),
        ProximaNfceNumero = request.ProximaNfceNumero,
        NfceCsc = TrimOrNull(request.NfceCsc),
        NfceCscIdToken = TrimOrNull(request.NfceCscIdToken),
        PdvSerieSat = TrimOrNull(request.PdvSerieSat),
        PdvImpressoraFiscal = TrimOrNull(request.PdvImpressoraFiscal),
        PdvNomeCaixaPadrao = TrimOrNull(request.PdvNomeCaixaPadrao),
        PdvContingenciaOffline = request.PdvContingenciaOffline ?? false,
        ProximaNfeNumero = request.ProximaNfeNumero,
        CfopPadraoInterno = TrimOrNull(request.CfopPadraoInterno),
        CfopPadraoInterestadual = TrimOrNull(request.CfopPadraoInterestadual),
        AliquotaIcmsInterna = request.AliquotaIcmsInterna,
        AliquotaIcmsInterestadual = request.AliquotaIcmsInterestadual,
        AliquotaPis = request.AliquotaPis,
        AliquotaCofins = request.AliquotaCofins,
        AliquotaIss = request.AliquotaIss,
        AliquotaIpi = request.AliquotaIpi,
        CargaTributariaPercentual = request.CargaTributariaPercentual,
        PerfilTributacao = TrimOrNull(request.PerfilTributacao) ?? "TributacaoAtual",
        UsaStLegado = request.UsaStLegado ?? false,
        DestacaIcmsStSeparado = request.DestacaIcmsStSeparado ?? false,
        CustoOperacionalPercentual = request.CustoOperacionalPercentual,
        MargemMinimaPercentual = request.MargemMinimaPercentual,
        PrioridadeFiscal = request.PrioridadeFiscal ?? 100,
        PermiteNfeEntrada = request.PermiteNfeEntrada ?? true,
        PermiteNfeSaida = request.PermiteNfeSaida ?? true,
        PermiteDropshipping = request.PermiteDropshipping ?? false,
        PermiteMarketplace = request.PermiteMarketplace ?? false,
        EmitentePreferencial = request.EmitentePreferencial ?? false,
        Ativa = request.Ativa ?? true,
        BeneficiosEstrategicos = TrimOrNull(request.BeneficiosEstrategicos),
        ContratoResumo = TrimOrNull(request.ContratoResumo),
        Observacoes = TrimOrNull(request.Observacoes),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.EmpresasGrupo.Add(empresa);
    await db.SaveChangesAsync(ct);

    var dto = new EmpresaGrupoDto(
        empresa.Id,
        empresa.TipoCadastro,
        empresa.RazaoSocial,
        empresa.NomeFantasia,
        empresa.Cnpj,
        empresa.InscricaoEstadual,
        empresa.InscricaoMunicipal,
        empresa.MatrizFilial,
        empresa.CodigoEmpresa,
        empresa.RegimeTributario,
        empresa.Crt,
        empresa.CnaePrincipal,
        empresa.CnaesSecundarios,
        empresa.CategoriaFiscal,
        empresa.SubcategoriaFiscal,
        empresa.NcmPadrao,
        empresa.NaturezaOperacaoPadrao,
        empresa.ResponsavelLegal,
        empresa.ResponsavelFiscal,
        empresa.EmailFiscal,
        empresa.EmailComercial,
        empresa.Telefone,
        empresa.Whatsapp,
        empresa.Cep,
        empresa.Logradouro,
        empresa.Numero,
        empresa.Complemento,
        empresa.Bairro,
        empresa.Cidade,
        empresa.Estado,
        empresa.Pais,
        empresa.AmbienteNfe,
        empresa.SerieNfe,
        empresa.SerieNfce,
        empresa.ModeloDocumentoPdv,
        empresa.AmbienteNfce,
        empresa.ProximaNfceNumero,
        empresa.NfceCsc,
        empresa.NfceCscIdToken,
        empresa.PdvSerieSat,
        empresa.PdvImpressoraFiscal,
        empresa.PdvNomeCaixaPadrao,
        empresa.PdvContingenciaOffline,
        empresa.ProximaNfeNumero,
        empresa.CfopPadraoInterno,
        empresa.CfopPadraoInterestadual,
        empresa.AliquotaIcmsInterna,
        empresa.AliquotaIcmsInterestadual,
        empresa.AliquotaPis,
        empresa.AliquotaCofins,
        empresa.AliquotaIss,
        empresa.AliquotaIpi,
        empresa.CargaTributariaPercentual,
        empresa.PerfilTributacao,
        empresa.UsaStLegado,
        empresa.DestacaIcmsStSeparado,
        empresa.CustoOperacionalPercentual,
        empresa.MargemMinimaPercentual,
        empresa.PrioridadeFiscal,
        empresa.PermiteNfeEntrada,
        empresa.PermiteNfeSaida,
        empresa.PermiteDropshipping,
        empresa.PermiteMarketplace,
        empresa.EmitentePreferencial,
        empresa.Ativa,
        empresa.BeneficiosEstrategicos,
        empresa.ContratoResumo,
        empresa.Observacoes,
        empresa.CreatedAt,
        empresa.UpdatedAt);

    return Results.Ok(ApiResponse<EmpresaGrupoDto>.Ok(dto, "Empresa societária/fiscal cadastrada com sucesso."));
})
.WithName("CriarEmpresaGrupo")
;

app.MapGet("/api/fiscal/pdv/configuracoes", [Authorize(Policy = "Gerente")] async (NexumDbContext db, CancellationToken ct) =>
{
    var configuracoes = await db.EmpresasGrupo
        .AsNoTracking()
        .Where(item => item.Ativa && item.PermiteNfeSaida)
        .OrderByDescending(item => item.EmitentePreferencial)
        .ThenBy(item => item.PrioridadeFiscal)
        .Select(item => new PdvFiscalConfigDto(
            item.Id,
            item.CodigoEmpresa,
            item.RazaoSocial,
            item.Cnpj,
            item.ModeloDocumentoPdv ?? "NFCe",
            item.AmbienteNfce ?? item.AmbienteNfe,
            item.SerieNfce,
            item.ProximaNfceNumero,
            item.NfceCscIdToken,
            !string.IsNullOrWhiteSpace(item.NfceCsc),
            item.PdvSerieSat,
            item.PdvImpressoraFiscal,
            item.PdvNomeCaixaPadrao,
            item.PdvContingenciaOffline,
            item.EmitentePreferencial,
            item.Estado))
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<PdvFiscalConfigDto>>.Ok(configuracoes));
})
.WithName("PdvFiscalConfiguracoes")
;

app.MapGet("/api/fiscal/pedidos", [Authorize(Policy = "Gerente")] async (NexumDbContext db, CancellationToken ct) =>
{
    var registros = await db.Fiscais
        .AsNoTracking()
        .OrderByDescending(item => item.CreatedAt)
        .Take(100)
        .Select(item => new FiscalPedidoDto(
            item.Id,
            item.PedidoId,
            item.EmpresaGrupoId,
            item.EmpresaEmitente,
            item.CodigoEmpresaEmitente,
            item.CnpjEmitente,
            item.NumeroNfe,
            item.Serie,
            item.StatusNfe.ToString(),
            item.StatusAutomacao,
            item.ModeloDocumento,
            item.AmbienteDocumento,
            item.Cfop,
            item.NaturezaOperacao,
            item.ValorTotal,
            item.ResumoRoteamento,
            item.CreatedAt,
            item.UpdatedAt))
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<FiscalPedidoDto>>.Ok(registros));
})
.WithName("FiscalPedidos")
;

app.MapPost("/api/fiscal/simular-roteamento", [Authorize(Policy = "Gerente")] async (
    FiscalRoutingSimulationRequest request,
    NexumDbContext db,
    IFiscalRoutingEngine fiscalRoutingEngine,
    CancellationToken ct) =>
{
    var empresas = await db.EmpresasGrupo
        .AsNoTracking()
        .Where(item => item.Ativa)
        .ToListAsync(ct);

    var decision = fiscalRoutingEngine.Evaluate(
        new FiscalRoutingRequest(
            request.TipoOperacao,
            request.ValorProdutos,
            request.ValorFrete,
            request.EstadoOrigem,
            request.EstadoDestino,
            request.CategoriaFiscal,
            request.SubcategoriaFiscal,
            request.NaturezaOperacao,
            request.ExigeMarketplace,
            request.ExigeDropshipping,
            request.RequerSaidaNfe,
            request.RequerEntradaNfe),
        empresas.Select(fiscalRoutingEngine.ToSnapshot).ToList());

    var resultado = new FiscalRoutingSimulationDto(
        decision.Sucesso,
        decision.Resumo,
        decision.EmpresaSelecionada?.CodigoEmpresa,
        decision.EmpresaSelecionada?.RazaoSocial,
        decision.EmpresaSelecionada?.Cnpj,
        decision.EmpresaSelecionada?.Estado,
        decision.Ranking.Select(item => new FiscalRoutingRankingDto(
            item.Empresa.CodigoEmpresa,
            item.Empresa.RazaoSocial,
            item.Empresa.Cnpj,
            item.Empresa.RegimeTributario,
            item.Empresa.CategoriaFiscal,
            item.Empresa.SubcategoriaFiscal,
            item.CustoTributarioEstimado,
            item.CustoOperacionalEstimado,
            item.LucroEstimado,
            item.MargemEstimadaPercentual,
            item.Score,
            item.Justificativas.ToList())).ToList());

    return Results.Ok(ApiResponse<FiscalRoutingSimulationDto>.Ok(resultado));
})
.WithName("FiscalSimularRoteamento")
;

static string? Slugify(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return null;
    }

    var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
    var builder = new StringBuilder(normalized.Length);
    var lastWasDash = false;

    foreach (var c in normalized)
    {
        if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
        {
            continue;
        }

        if (char.IsLetterOrDigit(c))
        {
            builder.Append(c);
            lastWasDash = false;
            continue;
        }

        if (c is ' ' or '-' or '_' or '/' or '\\' or '.')
        {
            if (!lastWasDash && builder.Length > 0)
            {
                builder.Append('-');
                lastWasDash = true;
            }
        }
    }

    var slug = builder.ToString().Trim('-');
    return slug.Length == 0 ? null : slug;
}

static string? NormalizeEmail(string? value) =>
    string.IsNullOrWhiteSpace(value)
        ? null
        : value.Trim().ToLowerInvariant();

static string CreateSecureToken() =>
    Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

static string HashToken(string token) =>
    Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

static async Task SendCustomerConfirmationEmailAsync(
    Cliente cliente,
    string token,
    INotificacaoService notificacaoService,
    IConfiguration configuration)
{
    var siteBaseUrl = (configuration["PublicSite:BaseUrl"] ?? "https://www.nexumaltivon.com").TrimEnd('/');
    var confirmationUrl = $"{siteBaseUrl}/confirmar-email?token={Uri.EscapeDataString(token)}";
    var body = $"""
        <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;background:#111;color:#fff;padding:32px;border:1px solid #c9a227">
          <h1 style="color:#c9a227">Confirme seu e-mail</h1>
          <p>Olá, <strong>{System.Net.WebUtility.HtmlEncode(cliente.Nome)}</strong>.</p>
          <p>Confirme seu endereço para liberar a área exclusiva do cliente Nexum Altivon.</p>
          <p style="margin:28px 0"><a href="{confirmationUrl}" style="background:#c9a227;color:#000;padding:14px 22px;text-decoration:none;font-weight:bold">Confirmar meu e-mail</a></p>
          <p>Este link expira em 24 horas. Se você não fez este cadastro, ignore esta mensagem.</p>
        </div>
        """;

    await notificacaoService.EnviarEmailAsync(cliente.Email, "Confirme seu cadastro - Nexum Altivon", body);
}

static string? NormalizeDocument(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return null;
    }

    var digits = new string(value.Where(char.IsDigit).ToArray());
    return digits.Length == 0 ? null : digits;
}

static async Task EnsurePedidoFiscalAutomationAsync(
    Pedido pedido,
    Cliente cliente,
    PedidoRequest request,
    NexumDbContext db,
    IFiscalRoutingEngine fiscalRoutingEngine,
    CancellationToken ct)
{
    var pedidoFiscalExistente = await db.Fiscais.FirstOrDefaultAsync(item => item.PedidoId == pedido.Id, ct);
    if (pedidoFiscalExistente is not null)
    {
        return;
    }

    var empresas = await db.EmpresasGrupo
        .AsNoTracking()
        .Where(item => item.Ativa)
        .ToListAsync(ct);

    if (empresas.Count == 0)
    {
        db.Fiscais.Add(new Fiscal
        {
            PedidoId = pedido.Id,
            ValorTotal = pedido.Total,
            NaturezaOperacao = "Venda de mercadoria",
            ModeloDocumento = "NFe",
            AmbienteDocumento = "Pendente",
            StatusNfe = StatusNfe.Pendente,
            StatusAutomacao = "Aguardando cadastro de empresa emitente",
            ResumoRoteamento = "Nenhuma empresa fiscal ativa cadastrada para emitir a operação.",
            PayloadOperacao = JsonSerializer.Serialize(new
            {
                pedido.NumeroPedido,
                pedido.MeioPagamento,
                pedido.GatewayPagamento,
                pedido.Total
            }),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        return;
    }

    var estadoDestino = InferDestinationState(request.EnderecoEntrega, cliente);
    var empresaOrigemPadrao = empresas
        .OrderByDescending(item => item.EmitentePreferencial)
        .ThenBy(item => item.PrioridadeFiscal)
        .First();

    var routingRequest = new FiscalRoutingRequest(
        string.Equals(estadoDestino, empresaOrigemPadrao.Estado, StringComparison.OrdinalIgnoreCase)
            ? TipoOperacaoFiscal.VendaInterna
            : TipoOperacaoFiscal.VendaInterestadual,
        pedido.Subtotal,
        pedido.FreteValor,
        empresaOrigemPadrao.Estado ?? estadoDestino,
        estadoDestino,
        empresaOrigemPadrao.CategoriaFiscal,
        empresaOrigemPadrao.SubcategoriaFiscal,
        empresaOrigemPadrao.NaturezaOperacaoPadrao ?? "Venda de mercadoria",
        !string.IsNullOrWhiteSpace(pedido.Origem.ToString()) && pedido.Origem == OrigemPedido.Marketplace,
        false,
        true,
        false);

    var decision = fiscalRoutingEngine.Evaluate(routingRequest, empresas.Select(fiscalRoutingEngine.ToSnapshot).ToList());
    var selecionada = decision.EmpresaSelecionada;

    var empresaEmitente = selecionada is null
        ? empresaOrigemPadrao
        : empresas.FirstOrDefault(item => item.Id == selecionada.Id) ?? empresaOrigemPadrao;

    var mesmaUf = string.Equals(empresaEmitente.Estado, estadoDestino, StringComparison.OrdinalIgnoreCase);
    var cfop = mesmaUf
        ? empresaEmitente.CfopPadraoInterno
        : empresaEmitente.CfopPadraoInterestadual;

    var resumoPagamento = BuildFiscalPaymentSummary(pedido.MeioPagamento, pedido.GatewayPagamento);
    var perfilTributacao = empresaEmitente.PerfilTributacao ?? "TributacaoAtual";

    db.Fiscais.Add(new Fiscal
    {
        PedidoId = pedido.Id,
        EmpresaGrupoId = empresaEmitente.Id,
        EmpresaEmitente = empresaEmitente.RazaoSocial,
        CodigoEmpresaEmitente = empresaEmitente.CodigoEmpresa,
        CnpjEmitente = empresaEmitente.Cnpj,
        NumeroNfe = empresaEmitente.ProximaNfeNumero?.ToString(),
        Serie = empresaEmitente.SerieNfe,
        ValorTotal = pedido.Total,
        Cfop = cfop,
        NaturezaOperacao = empresaEmitente.NaturezaOperacaoPadrao ?? "Venda de mercadoria",
        ModeloDocumento = "NFe",
        AmbienteDocumento = empresaEmitente.AmbienteNfe ?? "Homologacao",
        StatusNfe = StatusNfe.Pendente,
        StatusAutomacao = "Pré-emissão automática preparada",
        ResumoRoteamento = $"{decision.Resumo} Perfil tributário: {perfilTributacao}. {resumoPagamento}",
        PayloadOperacao = JsonSerializer.Serialize(new
        {
            pedido.Id,
            pedido.NumeroPedido,
            pedido.Total,
            pedido.Subtotal,
            pedido.FreteValor,
            pedido.MeioPagamento,
            pedido.GatewayPagamento,
            resumoPagamento,
            perfilTributacao,
            usaStLegado = empresaEmitente.UsaStLegado,
            destacaIcmsStSeparado = empresaEmitente.DestacaIcmsStSeparado,
            cliente = new { cliente.Id, cliente.Nome, cliente.Email, cliente.CpfCnpj },
            destino = new { estadoDestino },
            ranking = decision.Ranking.Select(item => new
            {
                item.Empresa.CodigoEmpresa,
                item.Empresa.RazaoSocial,
                item.Score,
                item.CustoTributarioEstimado,
                item.CustoOperacionalEstimado,
                item.MargemEstimadaPercentual,
                item.Justificativas
            }).ToList()
        }),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    });

    if (empresaEmitente.ProximaNfeNumero.HasValue)
    {
        empresaEmitente.ProximaNfeNumero += 1;
        db.EmpresasGrupo.Update(empresaEmitente);
    }
}

static string InferDestinationState(object? enderecoEntrega, Cliente cliente)
{
    if (enderecoEntrega is not null)
    {
        try
        {
            var json = JsonSerializer.Serialize(enderecoEntrega);
            var enderecoRequest = JsonSerializer.Deserialize<EnderecoEntregaRequest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var estado = TrimOrNull(enderecoRequest?.Estado);
            if (!string.IsNullOrWhiteSpace(estado))
            {
                return estado.ToUpperInvariant();
            }
        }
        catch
        {
        }
    }

    return "SP";
}

static string BuildFiscalPaymentSummary(string? metodoPagamento, string? gatewayPagamento)
{
    var metodo = TrimOrNull(metodoPagamento)?.ToUpperInvariant() ?? "NAO INFORMADO";
    var gateway = TrimOrNull(gatewayPagamento) ?? "gateway-pendente";

    return metodo switch
    {
        "CARTAO_CREDITO" or "CARTAO DE CREDITO" or "CREDITO" => $"Pagamento em cartão de crédito via {gateway}; considerar retenções, MDR e liquidação líquida no financeiro.",
        "PIX" => $"Pagamento via PIX por {gateway}; liquidação e baixa automática por webhook.",
        "BOLETO" => $"Pagamento via boleto por {gateway}; aguarda compensação e baixa automática.",
        "DEBITO" => $"Pagamento em débito via {gateway}; tratar confirmação e liquidação bancária.",
        "DEPOSITO" => $"Pagamento por depósito identificado; exigir conciliação bancária assistida.",
        _ => $"Forma de pagamento {metodo} via {gateway}; validar regra financeira correspondente."
    };
}

static LoginResponse CreateLoginResponse(
    int id,
    string nome,
    string email,
    string perfil,
    string issuer,
    string audience,
    SymmetricSecurityKey signingKey,
    int expirationHours)
{
    var expiresAt = DateTime.UtcNow.AddHours(expirationHours);
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, email),
        new Claim(ClaimTypes.Name, nome),
        new Claim(ClaimTypes.Email, email),
        new Claim(ClaimTypes.Role, perfil),
        new Claim("perfil", perfil)
    };

    var token = new JwtSecurityToken(
        issuer,
        audience,
        claims,
        expires: expiresAt,
        signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

    return new LoginResponse(
        new JwtSecurityTokenHandler().WriteToken(token),
        string.Empty,
        expiresAt,
        new UsuarioDto(id, nome, email, perfil));
}

static string? NormalizePhone(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return null;
    }

    var digits = new string(value.Where(char.IsDigit).ToArray());
    return digits.Length == 0 ? null : digits;
}

static string? TrimOrNull(string? value) =>
    string.IsNullOrWhiteSpace(value) ? null : value.Trim();

static string FormatStatusPedido(StatusPedido status) =>
    status switch
    {
        StatusPedido.EmSeparacao => "Processando",
        _ => status.ToString()
    };

static string FormatStatusPagamento(StatusPagamento status) =>
    status switch
    {
        StatusPagamento.Aguardando => "Aguardando pagamento",
        StatusPagamento.Aprovado => "Pagamento aprovado",
        StatusPagamento.Recusado => "Pagamento recusado",
        StatusPagamento.Estornado => "Pagamento estornado",
        StatusPagamento.Cancelado => "Pagamento cancelado",
        _ => status.ToString()
    };

static MetodoPagamento ParseMetodoPagamento(string? metodo)
{
    var token = (metodo ?? string.Empty).Trim().ToLowerInvariant();
    return token switch
    {
        "pix" => MetodoPagamento.PIX,
        "cartao" or "cartão" or "cartao_credito" or "cartãocredito" or "cartaocredito" => MetodoPagamento.CartaoCredito,
        "cartao_debito" or "cartãodebito" or "cartaodebito" => MetodoPagamento.CartaoDebito,
        "boleto" => MetodoPagamento.Boleto,
        "transferencia" or "transferência" => MetodoPagamento.Transferencia,
        "wallet" or "carteira" => MetodoPagamento.Wallet,
        _ => MetodoPagamento.Outro
    };
}

static string? GetIntegrationValue(IConfiguration configuration, params string[] keys)
{
    foreach (var key in keys)
    {
        var value = configuration[key];
        if (IsConfiguredSecret(value))
        {
            return value;
        }
    }

    return null;
}

static bool IsConfiguredSecret(string? value) =>
    !string.IsNullOrWhiteSpace(value) &&
    !value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase) &&
    !value.Contains("USE_ENV", StringComparison.OrdinalIgnoreCase) &&
    !value.Equals("null", StringComparison.OrdinalIgnoreCase);

static bool HasAllIntegrationValues(IConfiguration configuration, params (string Primary, string Secondary)[] keys) =>
    keys.All(pair => IsConfiguredSecret(GetIntegrationValue(configuration, pair.Primary, pair.Secondary)));

static async Task<IntegracaoDiagnosticoDto> BuildIntegracaoDiagnosticoAsync(
    string slug,
    IConfiguration configuration,
    NexumDbContext db,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct)
{
    var normalizedSlug = NormalizeIntegrationSlug(slug);
    return normalizedSlug switch
    {
        "ecommerce" => await TestEcommerceAsync(db, ct),
        "mercadopago" or "gateways" or "gateway" => await TestMercadoPagoAsync(configuration, httpClientFactory, ct),
        "melhorenvio" or "logistica" or "frete" => await TestMelhorEnvioAsync(configuration, httpClientFactory, ct),
        "mercadolivre" or "marketplaces" or "marketplace" => await TestMercadoLivreAsync(configuration, httpClientFactory, ct),
        "dropshipping" or "dropship" => await TestDropshippingAsync(db, ct),
        "shopify" => await TestShopifyAsync(configuration, db, httpClientFactory, ct),
        "cjdropshipping" or "cjdropship" or "cj" => await TestCjDropshippingAsync(configuration, db, ct),
        "bancaria" or "bancos" or "financeiro" => TestBancaria(configuration),
        _ => new IntegracaoDiagnosticoDto(
            slug,
            normalizedSlug,
            "Desconhecida",
            false,
            false,
            "Integração não mapeada no Nexum Altivon.",
            ["Use: ecommerce, dropshipping, shopify, cjdropshipping, mercadopago, melhorenvio, mercadolivre ou bancaria."],
            DateTime.UtcNow,
            null)
    };
}

static async Task<IntegracaoDiagnosticoDto> TestEcommerceAsync(NexumDbContext db, CancellationToken ct)
{
    var canConnect = await db.Database.CanConnectAsync(ct);
    var produtos = canConnect ? await db.Produtos.AsNoTracking().CountAsync(ct) : 0;
    var pedidos = canConnect ? await db.Pedidos.AsNoTracking().CountAsync(ct) : 0;

    return new IntegracaoDiagnosticoDto(
        "E-commerce e API",
        "ecommerce",
        canConnect ? "Operacional" : "Indisponível",
        true,
        canConnect,
        canConnect
            ? $"API e banco respondendo. Produtos: {produtos}; pedidos: {pedidos}."
            : "A API não conseguiu conectar ao banco de dados real.",
        canConnect ? [] : ["Conferir serviço MariaDB/MySQL e ConnectionStrings__DefaultConnection."],
        DateTime.UtcNow,
        canConnect ? "Banco real conectado" : null);
}

static async Task<IntegracaoDiagnosticoDto> TestMercadoPagoAsync(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct)
{
    var accessToken = GetIntegrationValue(configuration, "MercadoPago:AccessToken", "Integracoes:MercadoPago:AccessToken");
    if (!IsConfiguredSecret(accessToken))
    {
        return MissingIntegration(
            "Mercado Pago",
            "mercadopago",
            "Gateway pronto no sistema, aguardando token oficial.",
            ["MercadoPago__AccessToken"]);
    }

    try
    {
        var client = httpClientFactory.CreateClient("mercado-pago");
        using var request = new HttpRequestMessage(HttpMethod.Get, "users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await client.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        return new IntegracaoDiagnosticoDto(
            "Mercado Pago",
            "mercadopago",
            response.IsSuccessStatusCode ? "Conectado" : "Credencial recusada",
            true,
            response.IsSuccessStatusCode,
            response.IsSuccessStatusCode
                ? "Mercado Pago respondeu com sucesso. Gateway apto para iniciar cobrança real."
                : $"Mercado Pago retornou {(int)response.StatusCode}. Revise token, ambiente e permissões.",
            response.IsSuccessStatusCode ? [] : ["Validar MercadoPago__AccessToken.", "Conferir se a conta tem Pix/Checkout ativo."],
            DateTime.UtcNow,
            TryExtractJsonField(body, "nickname") ?? TryExtractJsonField(body, "email"));
    }
    catch (Exception ex)
    {
        return ExternalError("Mercado Pago", "mercadopago", ex);
    }
}

static async Task<IntegracaoDiagnosticoDto> TestMelhorEnvioAsync(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct)
{
    var token = GetIntegrationValue(configuration, "MelhorEnvio:Token", "Integracoes:MelhorEnvio:Token");
    var sandbox = configuration.GetValue("MelhorEnvio:Sandbox", configuration.GetValue("Integracoes:MelhorEnvio:Sandbox", true));
    if (!IsConfiguredSecret(token))
    {
        return MissingIntegration(
            "Melhor Envio",
            "melhorenvio",
            "Logística pronta no sistema, aguardando token da transportadora/hub.",
            ["MelhorEnvio__Token"]);
    }

    try
    {
        var client = httpClientFactory.CreateClient("melhor-envio");
        client.BaseAddress = new Uri(sandbox ? "https://sandbox.melhorenvio.com.br/" : "https://www.melhorenvio.com.br/");
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v2/me/shipment/services");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await client.SendAsync(request, ct);

        return new IntegracaoDiagnosticoDto(
            "Melhor Envio",
            "melhorenvio",
            response.IsSuccessStatusCode ? "Conectado" : "Credencial recusada",
            true,
            response.IsSuccessStatusCode,
            response.IsSuccessStatusCode
                ? "Melhor Envio respondeu. Cotação/compra de frete pode ser ativada com dados completos de origem e volumes."
                : $"Melhor Envio retornou {(int)response.StatusCode}. Revise token, sandbox/produção e permissões.",
            response.IsSuccessStatusCode ? [] : ["Validar MelhorEnvio__Token.", "Confirmar se MelhorEnvio__Sandbox está no ambiente correto."],
            DateTime.UtcNow,
            sandbox ? "Sandbox" : "Produção");
    }
    catch (Exception ex)
    {
        return ExternalError("Melhor Envio", "melhorenvio", ex);
    }
}

static async Task<IntegracaoDiagnosticoDto> TestMercadoLivreAsync(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct)
{
    var accessToken = GetIntegrationValue(configuration, "MercadoLivre:AccessToken", "Integracoes:MercadoLivre:AccessToken");
    var oauthReady = HasAllIntegrationValues(
        configuration,
        ("MercadoLivre:AppId", "Integracoes:MercadoLivre:AppId"),
        ("MercadoLivre:ClientSecret", "Integracoes:MercadoLivre:ClientSecret"),
        ("MercadoLivre:RedirectUri", "Integracoes:MercadoLivre:RedirectUri"));

    if (!IsConfiguredSecret(accessToken))
    {
        return new IntegracaoDiagnosticoDto(
            "Mercado Livre",
            "mercadolivre",
            oauthReady ? "OAuth pronto" : "Aguardando credenciais",
            oauthReady,
            false,
            oauthReady
                ? "Aplicativo configurado. Falta o vendedor autorizar para gerar access token e sincronizar anúncios/pedidos."
                : "Marketplace preparado, mas ainda sem AppId, ClientSecret e RedirectUri completos.",
            oauthReady ? ["Concluir autorização OAuth do vendedor.", "Salvar MercadoLivre__AccessToken e RefreshToken."] : ["MercadoLivre__AppId", "MercadoLivre__ClientSecret", "MercadoLivre__RedirectUri"],
            DateTime.UtcNow,
            null);
    }

    try
    {
        var client = httpClientFactory.CreateClient("mercado-livre");
        using var request = new HttpRequestMessage(HttpMethod.Get, "users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await client.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        return new IntegracaoDiagnosticoDto(
            "Mercado Livre",
            "mercadolivre",
            response.IsSuccessStatusCode ? "Conectado" : "Credencial recusada",
            true,
            response.IsSuccessStatusCode,
            response.IsSuccessStatusCode
                ? "Mercado Livre respondeu com o vendedor autenticado. Pronto para sincronizar catálogo e pedidos autorizados."
                : $"Mercado Livre retornou {(int)response.StatusCode}. Refaça OAuth ou revise permissões.",
            response.IsSuccessStatusCode ? [] : ["Refazer OAuth do vendedor.", "Atualizar MercadoLivre__AccessToken/RefreshToken."],
            DateTime.UtcNow,
            TryExtractJsonField(body, "nickname") ?? TryExtractJsonField(body, "id"));
    }
    catch (Exception ex)
    {
        return ExternalError("Mercado Livre", "mercadolivre", ex);
    }
}

static async Task<IntegracaoDiagnosticoDto> TestDropshippingAsync(NexumDbContext db, CancellationToken ct)
{
    var fornecedores = await db.Fornecedores.AsNoTracking().CountAsync(fornecedor => fornecedor.Status == StatusFornecedor.Ativo, ct);
    var canais = await db.DropshippingConfigs.AsNoTracking().CountAsync(config => config.Ativo, ct);
    var operacional = fornecedores > 0 || canais > 0;

    return new IntegracaoDiagnosticoDto(
        "Dropshipping",
        "dropshipping",
        operacional ? "Base ativa" : "Aguardando cadastros",
        operacional,
        operacional,
        operacional
            ? $"{fornecedores} fornecedor(es) ativo(s) e {canais} canal(is) de dropshipping ativo(s)."
            : "Sem fornecedor/canal ativo para roteamento real de dropshipping.",
        operacional ? [] : ["Cadastrar fornecedor ativo.", "Vincular produtos ao fornecedor.", "Ativar canal de dropshipping com chave/API quando existir."],
        DateTime.UtcNow,
        "Roteamento interno");
}

static async Task<IntegracaoDiagnosticoDto> TestShopifyAsync(
    IConfiguration configuration,
    NexumDbContext db,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct)
{
    var storeDomain = GetIntegrationValue(configuration, "Shopify:StoreDomain", "Integracoes:Shopify:StoreDomain");
    var apiVersion = GetIntegrationValue(configuration, "Shopify:ApiVersion", "Integracoes:Shopify:ApiVersion");
    var accessToken = GetIntegrationValue(configuration, "Shopify:AdminApiAccessToken", "Integracoes:Shopify:AdminApiAccessToken");
    var configCompleta = IsConfiguredSecret(storeDomain) && IsConfiguredSecret(apiVersion) && IsConfiguredSecret(accessToken);
    var canalPublicado = await db.DropshippingConfigs.AsNoTracking().AnyAsync(config => config.Slug == "shopify", ct);

    if (!configCompleta)
    {
        return new IntegracaoDiagnosticoDto(
            "Shopify",
            "shopify",
            canalPublicado ? "Canal publicado" : "Aguardando conexão",
            canalPublicado,
            false,
            canalPublicado
                ? "Canal Shopify cadastrado internamente; faltam domínio da loja e token Admin API para ativação."
                : "Conector Shopify ainda não recebeu StoreDomain, ApiVersion e AdminApiAccessToken.",
            ["Shopify__StoreDomain", "Shopify__ApiVersion", "Shopify__AdminApiAccessToken"],
            DateTime.UtcNow,
            canalPublicado ? "Canal interno publicado" : null);
    }

    try
    {
        var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri($"https://{storeDomain}/admin/api/{apiVersion.Trim('/')}/");
        using var request = new HttpRequestMessage(HttpMethod.Get, "shop.json");
        request.Headers.TryAddWithoutValidation("X-Shopify-Access-Token", accessToken);
        using var response = await client.SendAsync(request, ct);

        return new IntegracaoDiagnosticoDto(
            "Shopify",
            "shopify",
            response.IsSuccessStatusCode ? "Conectado" : "Credencial recusada",
            true,
            response.IsSuccessStatusCode,
            response.IsSuccessStatusCode
                ? "Shopify respondeu com sucesso. Catálogo, pedidos e estoque podem seguir para a fase de ligação real."
                : "Shopify recebeu a requisição, mas recusou a credencial ou o domínio informado.",
            response.IsSuccessStatusCode ? [] : ["Conferir domínio da loja.", "Validar Shopify__AdminApiAccessToken."],
            DateTime.UtcNow,
            storeDomain);
    }
    catch (Exception ex)
    {
        return new IntegracaoDiagnosticoDto(
            "Shopify",
            "shopify",
            "Erro de comunicação",
            true,
            false,
            $"Falha ao consultar a Shopify: {ex.Message}",
            ["Conferir acesso externo do servidor.", "Validar domínio da loja Shopify.", "Repetir teste após inserir o token real."],
            DateTime.UtcNow,
            storeDomain);
    }
}

static async Task<IntegracaoDiagnosticoDto> TestCjDropshippingAsync(
    IConfiguration configuration,
    NexumDbContext db,
    CancellationToken ct)
{
    var endpoint = GetIntegrationValue(configuration, "CJDropshipping:ApiEndpoint", "Integracoes:CJDropshipping:ApiEndpoint");
    var accessToken = GetIntegrationValue(configuration, "CJDropshipping:AccessToken", "Integracoes:CJDropshipping:AccessToken");
    var apiKey = GetIntegrationValue(configuration, "CJDropshipping:ApiKey", "Integracoes:CJDropshipping:ApiKey");
    var configCompleta = IsConfiguredSecret(endpoint) && (IsConfiguredSecret(accessToken) || IsConfiguredSecret(apiKey));
    var canalPublicado = await db.DropshippingConfigs.AsNoTracking().AnyAsync(config => config.Slug == "cjdropshipping", ct);

    return new IntegracaoDiagnosticoDto(
        "CJ Dropshipping",
        "cjdropshipping",
        configCompleta ? "Credenciais prontas" : canalPublicado ? "Canal publicado" : "Aguardando conexão",
        configCompleta || canalPublicado,
        false,
        configCompleta
            ? "Estrutura CJ Dropshipping está pronta no servidor e aguardando apenas o vínculo operacional dos produtos."
            : canalPublicado
                ? "Canal CJ já está publicado no sistema; falta inserir AccessToken/API key reais para ativação."
                : "Conector CJ ainda não recebeu endpoint e token/credenciais reais.",
        configCompleta
            ? ["Vincular produtos do catálogo ao canal CJ.", "Executar primeira sincronização real após inserir os tokens finais."]
            : ["CJDropshipping__ApiEndpoint", "CJDropshipping__AccessToken ou CJDropshipping__ApiKey"],
        DateTime.UtcNow,
        IsConfiguredSecret(endpoint) ? endpoint : null);
}

static IntegracaoDiagnosticoDto TestBancaria(IConfiguration configuration)
{
    var provider = GetIntegrationValue(configuration, "Banco:Provider", "Integracoes:Banco:Provider");
    var clientId = GetIntegrationValue(configuration, "Banco:ClientId", "Integracoes:Banco:ClientId");
    var clientSecret = GetIntegrationValue(configuration, "Banco:ClientSecret", "Integracoes:Banco:ClientSecret");
    var configured = IsConfiguredSecret(provider) && IsConfiguredSecret(clientId) && IsConfiguredSecret(clientSecret);

    return new IntegracaoDiagnosticoDto(
        "Bancos e conciliação",
        "bancaria",
        configured ? "Credenciais cadastradas" : "Aguardando provedor",
        configured,
        configured,
        configured
            ? "Credenciais bancárias encontradas. Próximo passo: homologar extrato/cobrança com o banco definido."
            : "Ainda falta definir banco/PSP e cadastrar credenciais para conciliação automática.",
        configured ? ["Homologar extrato/cobrança no provedor definido."] : ["Banco__Provider", "Banco__ClientId", "Banco__ClientSecret"],
        DateTime.UtcNow,
        provider);
}

static IntegracaoDiagnosticoDto MissingIntegration(string nome, string slug, string detalhe, List<string> pendencias) =>
    new(nome, slug, "Aguardando credenciais", false, false, detalhe, pendencias, DateTime.UtcNow, null);

static IntegracaoDiagnosticoDto ExternalError(string nome, string slug, Exception ex) =>
    new(nome, slug, "Erro de comunicação", true, false, $"Falha ao testar provedor externo: {ex.Message}", ["Conferir internet do servidor.", "Repetir teste após validar token/provedor."], DateTime.UtcNow, null);

static string NormalizeIntegrationSlug(string slug)
{
    var normalized = (slug ?? string.Empty).Normalize(NormalizationForm.FormD).ToLowerInvariant();
    var builder = new StringBuilder(normalized.Length);
    foreach (var character in normalized)
    {
        if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(character))
        {
            builder.Append(character);
        }
    }

    return builder.ToString();
}

static string? TryExtractJsonField(string json, string propertyName)
{
    try
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty(propertyName, out var property)
            ? property.ToString()
            : null;
    }
    catch
    {
        return null;
    }
}

static string? TryExtractJsonPath(string json, params string[] path)
{
    try
    {
        using var document = JsonDocument.Parse(json);
        var current = document.RootElement;
        foreach (var segment in path)
        {
            if (!current.TryGetProperty(segment, out current))
            {
                return null;
            }
        }

        return current.ToString();
    }
    catch
    {
        return null;
    }
}

static void ApplyMercadoPagoStatus(string status, Pagamento pagamento)
{
    var normalized = status.Trim().ToLowerInvariant();
    switch (normalized)
    {
        case "approved":
        case "accredited":
            pagamento.Status = StatusPagamentoDetalhado.Aprovado;
            if (pagamento.Pedido is not null)
            {
                pagamento.Pedido.StatusPagamento = StatusPagamento.Aprovado;
                pagamento.Pedido.Status = StatusPedido.Pago;
                pagamento.Pedido.DataPagamento = DateTime.UtcNow;
                pagamento.Pedido.UpdatedAt = DateTime.UtcNow;
            }
            break;
        case "rejected":
            pagamento.Status = StatusPagamentoDetalhado.Recusado;
            if (pagamento.Pedido is not null)
            {
                pagamento.Pedido.StatusPagamento = StatusPagamento.Recusado;
                pagamento.Pedido.UpdatedAt = DateTime.UtcNow;
            }
            break;
        case "cancelled":
            pagamento.Status = StatusPagamentoDetalhado.Cancelado;
            if (pagamento.Pedido is not null)
            {
                pagamento.Pedido.StatusPagamento = StatusPagamento.Cancelado;
                pagamento.Pedido.Status = StatusPedido.Cancelado;
                pagamento.Pedido.UpdatedAt = DateTime.UtcNow;
            }
            break;
        case "refunded":
        case "charged_back":
            pagamento.Status = normalized == "charged_back" ? StatusPagamentoDetalhado.Chargeback : StatusPagamentoDetalhado.Estornado;
            if (pagamento.Pedido is not null)
            {
                pagamento.Pedido.StatusPagamento = StatusPagamento.Estornado;
                pagamento.Pedido.Status = StatusPedido.Reembolsado;
                pagamento.Pedido.UpdatedAt = DateTime.UtcNow;
            }
            break;
        case "in_process":
        case "pending":
        default:
            pagamento.Status = StatusPagamentoDetalhado.Pendente;
            break;
    }
}

static async Task<List<FreteCotacaoDto>> CotarFreteAsync(
    FreteCotacaoRequest request,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct)
{
    var token = GetIntegrationValue(configuration, "MelhorEnvio:Token", "Integracoes:MelhorEnvio:Token");
    var sandbox = configuration.GetValue("MelhorEnvio:Sandbox", configuration.GetValue("Integracoes:MelhorEnvio:Sandbox", true));

    if (IsConfiguredSecret(token) && !string.IsNullOrWhiteSpace(request.CepDestino))
    {
        try
        {
            var client = httpClientFactory.CreateClient("melhor-envio");
            client.BaseAddress = new Uri(sandbox ? "https://sandbox.melhorenvio.com.br/" : "https://www.melhorenvio.com.br/");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/v2/me/shipment/calculate");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpRequest.Content = JsonContent.Create(new
            {
                from = new { postal_code = request.CepOrigem ?? "17400000" },
                to = new { postal_code = request.CepDestino },
                products = request.Itens.Select((item, index) => new
                {
                    id = item.Sku ?? $"item-{index + 1}",
                    width = item.LarguraCm ?? 16,
                    height = item.AlturaCm ?? 8,
                    length = item.ComprimentoCm ?? 24,
                    weight = item.PesoKg ?? 0.5m,
                    insurance_value = item.ValorUnitario,
                    quantity = item.Quantidade <= 0 ? 1 : item.Quantidade
                }).ToList()
            });

            using var response = await client.SendAsync(httpRequest, ct);
            var json = await response.Content.ReadAsStringAsync(ct);
            if (response.IsSuccessStatusCode)
            {
                var cotacoes = ParseMelhorEnvioCotacoes(json);
                if (cotacoes.Count > 0)
                {
                    return cotacoes;
                }
            }
        }
        catch
        {
            // Se o provedor falhar, mantemos a venda com cotação local controlada.
        }
    }

    var valorItens = request.Itens.Sum(item => Math.Max(1, item.Quantidade) * item.ValorUnitario);
    var cepDestino = request.CepDestino ?? string.Empty;
    var interiorSp = cepDestino.StartsWith("17", StringComparison.Ordinal);
    var freteBase = valorItens >= 1000 ? 0m : interiorSp ? 29.90m : 49.90m;

    return
    [
        new("local-padrao", "Entrega padrão Nexum", "Tabela local", freteBase, interiorSp ? 3 : 7, "Tabela local"),
        new("local-expresso", "Entrega expressa assistida", "Tabela local", freteBase + 35m, interiorSp ? 1 : 4, "Tabela local")
    ];
}

static async Task<GatewayPaymentStartResult> TryStartGatewayPaymentAsync(
    Pedido pedido,
    Cliente cliente,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    HttpContext http,
    CancellationToken ct)
{
    var metodo = (pedido.MeioPagamento ?? string.Empty).Trim().ToLowerInvariant();
    if (metodo != "pix")
    {
        return GatewayPaymentStartResult.NotStarted();
    }

    var token = GetIntegrationValue(configuration, "MercadoPago:AccessToken", "Integracoes:MercadoPago:AccessToken");
    if (!IsConfiguredSecret(token))
    {
        return GatewayPaymentStartResult.NotStarted();
    }

    try
    {
        var client = httpClientFactory.CreateClient("mercado-pago");
        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/payments");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Idempotency-Key", $"nexum-{pedido.NumeroPedido}");

        var notificationUrl = BuildPublicUrl(http, "/api/webhooks/mercadopago");
        request.Content = JsonContent.Create(new
        {
            transaction_amount = pedido.Total,
            description = $"Pedido {pedido.NumeroPedido} - Nexum Altivon",
            payment_method_id = "pix",
            notification_url = notificationUrl,
            external_reference = pedido.NumeroPedido,
            payer = new
            {
                email = cliente.Email,
                first_name = cliente.Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? cliente.Nome,
                identification = new
                {
                    type = string.IsNullOrWhiteSpace(cliente.CpfCnpj) || cliente.CpfCnpj.Length > 14 ? "CNPJ" : "CPF",
                    number = new string((cliente.CpfCnpj ?? string.Empty).Where(char.IsDigit).ToArray())
                }
            }
        });

        using var response = await client.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            return GatewayPaymentStartResult.NotStarted(body);
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var transactionId = root.TryGetProperty("id", out var id) ? id.ToString() : null;
        string? qrCode = null;
        string? paymentUrl = null;

        if (root.TryGetProperty("point_of_interaction", out var pointOfInteraction)
            && pointOfInteraction.TryGetProperty("transaction_data", out var transactionData))
        {
            qrCode = transactionData.TryGetProperty("qr_code", out var qrCodeProp) ? qrCodeProp.ToString() : null;
            paymentUrl = transactionData.TryGetProperty("ticket_url", out var ticketUrlProp) ? ticketUrlProp.ToString() : null;
        }

        return GatewayPaymentStartResult.Success("MercadoPago", transactionId, qrCode, paymentUrl, body);
    }
    catch (Exception ex)
    {
        return GatewayPaymentStartResult.NotStarted(ex.Message);
    }
}

static string BuildPublicUrl(HttpContext http, string path)
{
    var forwardedProto = http.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
    var forwardedHost = http.Request.Headers["X-Forwarded-Host"].FirstOrDefault();
    var scheme = string.IsNullOrWhiteSpace(forwardedProto) ? http.Request.Scheme : forwardedProto;
    var host = string.IsNullOrWhiteSpace(forwardedHost) ? http.Request.Host.Value : forwardedHost;

    if (string.IsNullOrWhiteSpace(host) || host.Contains("localhost", StringComparison.OrdinalIgnoreCase))
    {
        host = "api.nexumaltivon.com";
        scheme = "https";
    }

    return $"{scheme}://{host}{path}";
}

static List<FreteCotacaoDto> ParseMelhorEnvioCotacoes(string json)
{
    var cotacoes = new List<FreteCotacaoDto>();
    try
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return cotacoes;
        }

        foreach (var item in document.RootElement.EnumerateArray())
        {
            var id = item.TryGetProperty("id", out var idProp) ? idProp.ToString() : Guid.NewGuid().ToString("N");
            var name = item.TryGetProperty("name", out var nameProp) ? nameProp.ToString() : "Frete Melhor Envio";
            var company = item.TryGetProperty("company", out var companyProp) && companyProp.TryGetProperty("name", out var companyName)
                ? companyName.ToString()
                : "Melhor Envio";
            var priceText = item.TryGetProperty("price", out var priceProp) ? priceProp.ToString() : "0";
            var prazo = item.TryGetProperty("delivery_time", out var prazoProp) && prazoProp.TryGetInt32(out var prazoInt) ? prazoInt : 0;

            if (decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
            {
                cotacoes.Add(new FreteCotacaoDto(id, name, company, price, prazo, "Melhor Envio"));
            }
        }
    }
    catch
    {
        return [];
    }

    return cotacoes;
}

static string BuildPedidoInstruction(StatusPagamento statusPagamento, string? metodoPagamento, string? transacaoId)
{
    if (!string.IsNullOrWhiteSpace(transacaoId))
    {
        return "Pagamento iniciado no gateway. Acompanhe a confirmação automática pelo painel.";
    }

    return statusPagamento switch
    {
        StatusPagamento.Aprovado => "Pagamento confirmado. Pedido pronto para separação e logística.",
        StatusPagamento.Cancelado => "Pagamento cancelado. Não separar mercadoria.",
        StatusPagamento.Recusado => "Pagamento recusado. Entrar em contato com o cliente antes de reenviar cobrança.",
        StatusPagamento.Estornado => "Pagamento estornado. Conferir financeiro e estoque.",
        _ when string.Equals(metodoPagamento, "pix", StringComparison.OrdinalIgnoreCase) =>
            "Pedido reservado. Configure o gateway Pix para gerar QR Code e baixa automática.",
        _ when string.Equals(metodoPagamento, "boleto", StringComparison.OrdinalIgnoreCase) =>
            "Pedido reservado. Configure o gateway de boleto para gerar linha digitável e vencimento.",
        _ =>
            "Pedido reservado. Configure o gateway para cobrança real e confirmação automática."
    };
}

static string BuildAbastecimentoResumo(Produto produto, int quantidade)
{
    var origem = produto.TipoProduto switch
    {
        NexumAltivon.API.Models.TipoProduto.Dropshipping => "Dropshipping",
        NexumAltivon.API.Models.TipoProduto.Marketplace => "Marketplace",
        NexumAltivon.API.Models.TipoProduto.Afiliado => "Afiliado",
        _ => "Estoque próprio"
    };

    var fornecedor = produto.Fornecedor is null
        ? "sem fornecedor"
        : string.IsNullOrWhiteSpace(produto.Fornecedor.NomeFantasia)
            ? produto.Fornecedor.RazaoSocial
            : produto.Fornecedor.NomeFantasia;

    var prazo = produto.Fornecedor?.PrazoEntregaDias;
    return prazo.HasValue && prazo.Value > 0
        ? $"{produto.Sku} x{quantidade} via {origem} / {fornecedor} / prazo {prazo.Value}d"
        : $"{produto.Sku} x{quantidade} via {origem} / {fornecedor}";
}

static bool TryParseStatusPedido(string? raw, out StatusPedido status)
{
    status = StatusPedido.Pendente;

    if (string.IsNullOrWhiteSpace(raw))
    {
        return false;
    }

    var token = raw.Trim().ToLowerInvariant();
    token = token.Replace(" ", "", StringComparison.Ordinal)
        .Replace("-", "", StringComparison.Ordinal)
        .Replace("_", "", StringComparison.Ordinal);

    switch (token)
    {
        case "pendente":
        case "recebido":
            status = StatusPedido.Pendente;
            return true;
        case "pago":
            status = StatusPedido.Pago;
            return true;
        case "processando":
        case "emseparacao":
        case "separacao":
            status = StatusPedido.EmSeparacao;
            return true;
        case "enviado":
            status = StatusPedido.Enviado;
            return true;
        case "entregue":
            status = StatusPedido.Entregue;
            return true;
        case "cancelado":
            status = StatusPedido.Cancelado;
            return true;
        case "devolvido":
            status = StatusPedido.Devolvido;
            return true;
        case "reembolsado":
            status = StatusPedido.Reembolsado;
            return true;
    }

    return Enum.TryParse(raw.Trim(), ignoreCase: true, out status);
}

static string FormatStatusLead(StatusLead status) =>
    status switch
    {
        StatusLead.EmAtendimento => "Contato",
        StatusLead.Convertido => "Ganho",
        _ => status.ToString()
    };

static string FormatOrigemLead(OrigemLead origem) =>
    origem switch
    {
        OrigemLead.WhatsApp => "WhatsApp",
        _ => origem.ToString()
    };

static string FormatOrigemLeadValue(string? raw)
{
    if (TryParseOrigemLead(raw, out var origem))
    {
        return FormatOrigemLead(origem);
    }

    return string.IsNullOrWhiteSpace(raw) ? "Site" : raw.Trim();
}

static string FormatStatusLeadValue(string? raw)
{
    if (TryParseStatusLead(raw, out var status))
    {
        return FormatStatusLead(status);
    }

    return string.IsNullOrWhiteSpace(raw) ? "Novo" : raw.Trim();
}

static bool TryParseStatusLead(string? raw, out StatusLead status)
{
    status = StatusLead.Novo;

    if (string.IsNullOrWhiteSpace(raw))
    {
        return false;
    }

    var token = raw.Trim().ToLowerInvariant();
    token = token.Replace(" ", "", StringComparison.Ordinal)
        .Replace("-", "", StringComparison.Ordinal)
        .Replace("_", "", StringComparison.Ordinal);

    switch (token)
    {
        case "novo":
            status = StatusLead.Novo;
            return true;
        case "contato":
        case "negociacao":
        case "ematendimento":
            status = StatusLead.EmAtendimento;
            return true;
        case "qualificado":
            status = StatusLead.Qualificado;
            return true;
        case "ganho":
        case "convertido":
            status = StatusLead.Convertido;
            return true;
        case "perdido":
            status = StatusLead.Perdido;
            return true;
        case "arquivado":
            status = StatusLead.Arquivado;
            return true;
    }

    return Enum.TryParse(raw.Trim(), ignoreCase: true, out status);
}

static bool TryParseOrigemLead(string? raw, out OrigemLead origem)
{
    origem = OrigemLead.Site;

    if (string.IsNullOrWhiteSpace(raw))
    {
        return false;
    }

    var token = raw.Trim().ToLowerInvariant();
    token = token.Replace(" ", "", StringComparison.Ordinal)
        .Replace("-", "", StringComparison.Ordinal)
        .Replace("_", "", StringComparison.Ordinal);

    switch (token)
    {
        case "site":
            origem = OrigemLead.Site;
            return true;
        case "whatsapp":
            origem = OrigemLead.WhatsApp;
            return true;
        case "email":
            origem = OrigemLead.Email;
            return true;
        case "telefone":
            origem = OrigemLead.Telefone;
            return true;
        case "marketplace":
            origem = OrigemLead.Marketplace;
            return true;
        case "indicacao":
            origem = OrigemLead.Indicacao;
            return true;
        case "campanha":
            origem = OrigemLead.Campanha;
            return true;
        case "outro":
            origem = OrigemLead.Outro;
            return true;
    }

    return Enum.TryParse(raw.Trim(), ignoreCase: true, out origem);
}

static bool TryParseTipoLead(string? raw, out TipoLead tipo)
{
    tipo = TipoLead.ClienteVIP;

    if (string.IsNullOrWhiteSpace(raw))
    {
        return false;
    }

    var token = raw.Trim().ToLowerInvariant()
        .Replace(" ", "", StringComparison.Ordinal)
        .Replace("-", "", StringComparison.Ordinal)
        .Replace("_", "", StringComparison.Ordinal);

    switch (token)
    {
        case "cliente":
        case "clientevip":
            tipo = TipoLead.ClienteVIP;
            return true;
        case "dropshipping":
            tipo = TipoLead.Dropshipping;
            return true;
        case "fornecedor":
            tipo = TipoLead.Fornecedor;
            return true;
        case "parceiro":
            tipo = TipoLead.Parceiro;
            return true;
        case "afiliado":
            tipo = TipoLead.Afiliado;
            return true;
        case "outro":
            tipo = TipoLead.Outro;
            return true;
    }

    return Enum.TryParse(raw.Trim(), ignoreCase: true, out tipo);
}

static string? BuildLeadObservacao(LeadRequest request)
{
    var notes = new List<string>();

    if (!string.IsNullOrWhiteSpace(request.Mensagem))
    {
        notes.Add($"Mensagem: {request.Mensagem.Trim()}");
    }

    if (!string.IsNullOrWhiteSpace(request.Observacao))
    {
        notes.Add($"Observação: {request.Observacao.Trim()}");
    }

    if (!string.IsNullOrWhiteSpace(request.Segmento))
    {
        notes.Add($"Segmento: {request.Segmento.Trim()}");
    }

    return notes.Count == 0 ? null : string.Join(Environment.NewLine, notes);
}

static string? AppendLeadNotes(string? current, string? incoming)
{
    var currentValue = TrimOrNull(current);
    var incomingValue = TrimOrNull(incoming);

    if (string.IsNullOrWhiteSpace(currentValue))
    {
        return incomingValue;
    }

    if (string.IsNullOrWhiteSpace(incomingValue))
    {
        return currentValue;
    }

    if (currentValue.Contains(incomingValue, StringComparison.OrdinalIgnoreCase))
    {
        return currentValue;
    }

    return $"{currentValue}{Environment.NewLine}{Environment.NewLine}{incomingValue}";
}

static string BuildLeadNotificationEmail(CrmLead lead)
{
    var origem = FormatOrigemLead(lead.Origem);
    var tipo = lead.Tipo.ToString();
    var empresa = string.IsNullOrWhiteSpace(lead.Empresa) ? "-" : lead.Empresa;
    var telefone = string.IsNullOrWhiteSpace(lead.Telefone) ? "-" : lead.Telefone;
    var whatsapp = string.IsNullOrWhiteSpace(lead.Whatsapp) ? "-" : lead.Whatsapp;
    var segmento = string.IsNullOrWhiteSpace(lead.Segmento) ? "-" : lead.Segmento;
    var observacoes = string.IsNullOrWhiteSpace(lead.Anotacoes) ? "-" : lead.Anotacoes.Replace(Environment.NewLine, "<br/>");

    return $"""
    <html>
      <body style="font-family:Arial,sans-serif;background:#f8fafc;color:#0f172a;">
        <div style="max-width:720px;margin:0 auto;background:#ffffff;border:1px solid #e2e8f0;border-radius:16px;padding:24px;">
          <h2 style="margin-top:0;color:#0f172a;">Novo contato recebido no site Nexum Altivon</h2>
          <p>Um lead público acabou de entrar pelo portal de vendas.</p>
          <table style="width:100%;border-collapse:collapse;">
            <tr><td style="padding:8px 0;font-weight:bold;">Nome</td><td>{lead.Nome}</td></tr>
            <tr><td style="padding:8px 0;font-weight:bold;">E-mail</td><td>{lead.Email}</td></tr>
            <tr><td style="padding:8px 0;font-weight:bold;">Telefone</td><td>{telefone}</td></tr>
            <tr><td style="padding:8px 0;font-weight:bold;">WhatsApp</td><td>{whatsapp}</td></tr>
            <tr><td style="padding:8px 0;font-weight:bold;">Empresa</td><td>{empresa}</td></tr>
            <tr><td style="padding:8px 0;font-weight:bold;">CNPJ</td><td>{lead.Cnpj ?? "-"}</td></tr>
            <tr><td style="padding:8px 0;font-weight:bold;">Segmento</td><td>{segmento}</td></tr>
            <tr><td style="padding:8px 0;font-weight:bold;">Origem</td><td>{origem}</td></tr>
            <tr><td style="padding:8px 0;font-weight:bold;">Tipo</td><td>{tipo}</td></tr>
            <tr><td style="padding:8px 0;font-weight:bold;">Status inicial</td><td>{FormatStatusLead(lead.Status)}</td></tr>
          </table>
          <div style="margin-top:16px;padding:16px;background:#f8fafc;border-radius:12px;">
            <strong>Observações / mensagem</strong>
            <div style="margin-top:8px;">{observacoes}</div>
          </div>
        </div>
      </body>
    </html>
    """;
}

static SiteConfiguracaoPublicaDto BuildPublicSiteConfig(IReadOnlyDictionary<string, string?> configMap)
{
    var contactEmail = GetConfigValue(configMap, "site_email_contato", "corporativo.gna@gmail.com");

    return new SiteConfiguracaoPublicaDto(
        GetConfigValue(configMap, "site_nome", "Grupo Nexum Altivon"),
        GetConfigValue(configMap, "site_url", "https://www.nexumaltivon.com"),
        contactEmail,
        GetConfigValue(configMap, "site_telefone", "(14) 99673-1879"),
        GetConfigValue(configMap, "site_telefone_secundario", "(14) 99634-8409"),
        GetConfigValue(configMap, "site_whatsapp", "5514996731879"),
        GetConfigValue(configMap, "site_whatsapp_secundario", "5514996348409"),
        GetConfigValue(configMap, "site_yara_email", contactEmail),
        GetConfigValue(configMap, "site_logo", "/assets/logo-2.jpg"),
        ParseJsonList(GetConfigValue(configMap, "home_hero_slides", string.Empty), GetDefaultHeroSlides()),
        GetConfigValue(configMap, "home_intro_titulo", "Uma Nova Era Começa"),
        GetConfigValue(configMap, "home_intro_texto_1", "A Nexum Altivon está chegando para transformar e inovar o mercado digital brasileiro."),
        GetConfigValue(configMap, "home_intro_texto_2", "Nosso compromisso é claro: entregar qualidade superior, atendimento que faz a diferença e preços acessíveis que respeitam o seu bolso."),
        GetConfigValue(configMap, "home_intro_badge", "www.nexumaltivon.com"),
        ParseJsonStringList(GetConfigValue(configMap, "home_quality_items", string.Empty), [
            "Curadoria rigorosa de fornecedores",
            "Atendimento humano e especializado",
            "Política de devolução simplificada",
            "Preços justos e acessíveis"
        ]),
        ParseJsonList(GetConfigValue(configMap, "home_partner_cards", string.Empty), GetDefaultPartnerCards()),
        GetConfigValue(configMap, "home_footer_texto", "Portal em evolução contínua para vendas, relacionamento, parceiros e operações integradas."));
}

static string GetConfigValue(IReadOnlyDictionary<string, string?> configMap, string key, string fallback) =>
    configMap.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
        ? value.Trim()
        : fallback;

static bool LooksLikeJson(string? value)
{
    var normalized = value?.Trim();
    return !string.IsNullOrWhiteSpace(normalized)
        && ((normalized.StartsWith("{") && normalized.EndsWith("}"))
            || (normalized.StartsWith("[") && normalized.EndsWith("]")));
}

static List<string> ParseJsonStringList(string? json, List<string> fallback)
{
    if (string.IsNullOrWhiteSpace(json))
    {
        return fallback;
    }

    try
    {
        var parsed = JsonSerializer.Deserialize<List<string>>(json);
        return parsed?.Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => item.Trim()).ToList() ?? fallback;
    }
    catch
    {
        return fallback;
    }
}

static List<T> ParseJsonList<T>(string? json, List<T> fallback)
{
    if (string.IsNullOrWhiteSpace(json))
    {
        return fallback;
    }

    try
    {
        var parsed = JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return parsed is { Count: > 0 } ? parsed : fallback;
    }
    catch
    {
        return fallback;
    }
}

static List<HeroSlideSiteDto> GetDefaultHeroSlides() =>
[
    new("ecommerce", "Grupo Nexum Altivon", "O Futuro do", "E-Commerce", "Seis lojas, uma operação conectada e uma proposta premium para transformar a experiência de compra online.", "https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=1920&q=88"),
    new("marcas", "6 marcas em expansão", "Uma operação,", "múltiplos mercados", "Turismo, relógios, moda, tecnologia, construção e festas com a mesma curadoria comercial do Grupo Nexum Altivon.", "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=1920&q=88"),
    new("tecnologia", "Experiência tecnológica", "Compra segura com", "atendimento humano", "Fluxos preparados para catálogo, clientes, pedidos, integrações e relacionamento com visão de crescimento contínuo.", "https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=1920&q=88")
];

static List<PartnerCardSiteDto> GetDefaultPartnerCards() =>
[
    new("Parceiros de Vendas", "Lojas físicas ou online podem ampliar seus horizontes de venda com nossa infraestrutura comercial e operação integrada.", "Quero Vender", "https://wa.me/5514996731879?text=Olá! Tenho interesse em ser parceiro de vendas do Grupo Nexum Altivon.", "Store"),
    new("Fornecedores & Distribuidores", "Distribuidores e fabricantes encontram um canal de venda em crescimento, com visão de volume, relacionamento e longo prazo.", "Quero Fornecer", "https://wa.me/5514996348409?text=Olá! Sou fornecedor/distribuidor e tenho interesse em parceria com o Grupo Nexum Altivon.", "Truck"),
    new("Dropshipping", "Integre seu catálogo às nossas lojas ou utilize nossa infraestrutura para conectar produtos, logística e novos canais.", "Quero Fazer Dropship", "https://wa.me/5514996731879?text=Olá! Tenho interesse em parceria de dropshipping com o Grupo Nexum Altivon.", "Building2")
];

static async Task EnsureOperationalSchemaAsync(IServiceProvider services, ILogger logger)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NexumDbContext>();

    if (!await db.Database.CanConnectAsync())
    {
        logger.LogWarning("Banco indisponível durante a verificação de esquema operacional.");
        return;
    }

    await db.Database.ExecuteSqlRawAsync("ALTER TABLE clientes ADD COLUMN IF NOT EXISTS email_verificado_em DATETIME NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE clientes ADD COLUMN IF NOT EXISTS token_confirmacao_email VARCHAR(255) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE clientes ADD COLUMN IF NOT EXISTS token_confirmacao_expira_em DATETIME NULL;");
    await db.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS ix_clientes_token_confirmacao_email ON clientes (token_confirmacao_email);");

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS erp_empresas_grupo (
            id INT NOT NULL AUTO_INCREMENT,
            tipo_cadastro VARCHAR(40) NOT NULL,
            razao_social VARCHAR(200) NOT NULL,
            nome_fantasia VARCHAR(200) NULL,
            cnpj VARCHAR(18) NOT NULL,
            inscricao_estadual VARCHAR(30) NULL,
            inscricao_municipal VARCHAR(30) NULL,
            matriz_filial VARCHAR(20) NULL,
            codigo_empresa VARCHAR(50) NULL,
            regime_tributario VARCHAR(60) NULL,
            crt VARCHAR(10) NULL,
            cnae_principal VARCHAR(20) NULL,
            cnaes_secundarios TEXT NULL,
            categoria_fiscal VARCHAR(100) NULL,
            subcategoria_fiscal VARCHAR(100) NULL,
            ncm_padrao VARCHAR(20) NULL,
            natureza_operacao_padrao VARCHAR(120) NULL,
            responsavel_legal VARCHAR(150) NULL,
            responsavel_fiscal VARCHAR(150) NULL,
            email_fiscal VARCHAR(150) NULL,
            email_comercial VARCHAR(150) NULL,
            telefone VARCHAR(25) NULL,
            whatsapp VARCHAR(25) NULL,
            cep VARCHAR(12) NULL,
            logradouro VARCHAR(200) NULL,
            numero VARCHAR(20) NULL,
            complemento VARCHAR(120) NULL,
            bairro VARCHAR(120) NULL,
            cidade VARCHAR(120) NULL,
            estado VARCHAR(2) NULL,
            pais VARCHAR(60) NULL,
            ambiente_nfe VARCHAR(30) NULL,
            serie_nfe VARCHAR(10) NULL,
            serie_nfce VARCHAR(10) NULL,
            modelo_documento_pdv VARCHAR(20) NULL,
            ambiente_nfce VARCHAR(30) NULL,
            proxima_nfce_numero INT NULL,
            nfce_csc VARCHAR(120) NULL,
            nfce_csc_id_token VARCHAR(20) NULL,
            pdv_serie_sat VARCHAR(20) NULL,
            pdv_impressora_fiscal VARCHAR(120) NULL,
            pdv_nome_caixa_padrao VARCHAR(80) NULL,
            pdv_contingencia_offline TINYINT(1) NOT NULL DEFAULT 0,
            proxima_nfe_numero INT NULL,
            cfop_padrao_interno VARCHAR(10) NULL,
            cfop_padrao_interestadual VARCHAR(10) NULL,
            aliquota_icms_interna DECIMAL(10,4) NULL,
            aliquota_icms_interestadual DECIMAL(10,4) NULL,
            aliquota_pis DECIMAL(10,4) NULL,
            aliquota_cofins DECIMAL(10,4) NULL,
            aliquota_iss DECIMAL(10,4) NULL,
            aliquota_ipi DECIMAL(10,4) NULL,
            carga_tributaria_percentual DECIMAL(10,4) NULL,
            custo_operacional_percentual DECIMAL(10,4) NULL,
            margem_minima_percentual DECIMAL(10,4) NULL,
            prioridade_fiscal INT NOT NULL DEFAULT 100,
            permite_nfe_entrada TINYINT(1) NOT NULL DEFAULT 1,
            permite_nfe_saida TINYINT(1) NOT NULL DEFAULT 1,
            permite_dropshipping TINYINT(1) NOT NULL DEFAULT 0,
            permite_marketplace TINYINT(1) NOT NULL DEFAULT 0,
            emitente_preferencial TINYINT(1) NOT NULL DEFAULT 0,
            ativa TINYINT(1) NOT NULL DEFAULT 1,
            beneficios_estrategicos TEXT NULL,
            contrato_resumo TEXT NULL,
            observacoes TEXT NULL,
            created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            UNIQUE KEY ux_erp_empresas_grupo_cnpj (cnpj),
            UNIQUE KEY ux_erp_empresas_grupo_codigo_empresa (codigo_empresa)
        );
        """);

    await db.Database.ExecuteSqlRawAsync("ALTER TABLE erp_empresas_grupo ADD COLUMN IF NOT EXISTS modelo_documento_pdv VARCHAR(20) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE erp_empresas_grupo ADD COLUMN IF NOT EXISTS ambiente_nfce VARCHAR(30) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE erp_empresas_grupo ADD COLUMN IF NOT EXISTS proxima_nfce_numero INT NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE erp_empresas_grupo ADD COLUMN IF NOT EXISTS nfce_csc VARCHAR(120) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE erp_empresas_grupo ADD COLUMN IF NOT EXISTS nfce_csc_id_token VARCHAR(20) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE erp_empresas_grupo ADD COLUMN IF NOT EXISTS pdv_serie_sat VARCHAR(20) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE erp_empresas_grupo ADD COLUMN IF NOT EXISTS pdv_impressora_fiscal VARCHAR(120) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE erp_empresas_grupo ADD COLUMN IF NOT EXISTS pdv_nome_caixa_padrao VARCHAR(80) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE erp_empresas_grupo ADD COLUMN IF NOT EXISTS pdv_contingencia_offline TINYINT(1) NOT NULL DEFAULT 0;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE erp_empresas_grupo ADD COLUMN IF NOT EXISTS perfil_tributacao VARCHAR(40) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE erp_empresas_grupo ADD COLUMN IF NOT EXISTS usa_st_legado TINYINT(1) NOT NULL DEFAULT 0;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE erp_empresas_grupo ADD COLUMN IF NOT EXISTS destaca_icms_st_separado TINYINT(1) NOT NULL DEFAULT 0;");

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS fiscal (
            id INT NOT NULL AUTO_INCREMENT,
            pedido_id INT NOT NULL,
            empresa_grupo_id INT NULL,
            empresa_emitente VARCHAR(200) NULL,
            codigo_empresa_emitente VARCHAR(50) NULL,
            cnpj_emitente VARCHAR(18) NULL,
            numero_nfe VARCHAR(20) NULL,
            serie VARCHAR(5) NULL,
            chave_acesso VARCHAR(44) NULL,
            xml_url VARCHAR(255) NULL,
            danfe_url VARCHAR(255) NULL,
            status_nfe VARCHAR(30) NOT NULL DEFAULT 'Pendente',
            valor_total DECIMAL(10,2) NULL,
            cfop VARCHAR(10) NULL,
            natureza_operacao VARCHAR(100) NULL,
            ambiente_documento VARCHAR(30) NULL,
            modelo_documento VARCHAR(20) NULL,
            status_automacao VARCHAR(40) NULL,
            resumo_roteamento TEXT NULL,
            payload_operacao LONGTEXT NULL,
            data_emissao DATETIME NULL,
            data_autorizacao DATETIME NULL,
            protocolo VARCHAR(50) NULL,
            motivo_cancelamento TEXT NULL,
            created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            KEY ix_fiscal_pedido_id (pedido_id),
            KEY ix_fiscal_empresa_grupo_id (empresa_grupo_id)
        );
        """);

    await db.Database.ExecuteSqlRawAsync("ALTER TABLE fiscal ADD COLUMN IF NOT EXISTS empresa_grupo_id INT NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE fiscal ADD COLUMN IF NOT EXISTS empresa_emitente VARCHAR(200) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE fiscal ADD COLUMN IF NOT EXISTS codigo_empresa_emitente VARCHAR(50) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE fiscal ADD COLUMN IF NOT EXISTS cnpj_emitente VARCHAR(18) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE fiscal ADD COLUMN IF NOT EXISTS ambiente_documento VARCHAR(30) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE fiscal ADD COLUMN IF NOT EXISTS modelo_documento VARCHAR(20) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE fiscal ADD COLUMN IF NOT EXISTS status_automacao VARCHAR(40) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE fiscal ADD COLUMN IF NOT EXISTS resumo_roteamento TEXT NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE fiscal ADD COLUMN IF NOT EXISTS payload_operacao LONGTEXT NULL;");

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS dropshipping_config (
            id INT NOT NULL AUTO_INCREMENT,
            nome VARCHAR(100) NOT NULL,
            slug VARCHAR(50) NOT NULL,
            tipo INT NOT NULL,
            api_endpoint VARCHAR(255) NULL,
            api_key VARCHAR(255) NULL,
            api_secret VARCHAR(255) NULL,
            ativo TINYINT(1) NOT NULL DEFAULT 0,
            created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            UNIQUE KEY ux_dropshipping_config_slug (slug)
        );
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        INSERT INTO dropshipping_config (nome, slug, tipo, api_endpoint, ativo)
        VALUES
            ('Shopify', 'shopify', 5, 'https://{{store}}.myshopify.com/admin/api', 0),
            ('CJ Dropshipping', 'cjdropshipping', 1, 'https://developers.cjdropshipping.com/api2.0/v1', 0)
        ON DUPLICATE KEY UPDATE
            nome = VALUES(nome),
            tipo = VALUES(tipo),
            api_endpoint = VALUES(api_endpoint),
            updated_at = CURRENT_TIMESTAMP;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS configuracoes_sistema (
            id INT NOT NULL AUTO_INCREMENT,
            chave VARCHAR(100) NOT NULL,
            valor TEXT NULL,
            tipo VARCHAR(20) NOT NULL DEFAULT 'Texto',
            descricao VARCHAR(255) NULL,
            grupo VARCHAR(50) NULL,
            editavel TINYINT(1) NOT NULL DEFAULT 1,
            created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            UNIQUE KEY ux_configuracoes_sistema_chave (chave),
            KEY ix_configuracoes_sistema_grupo (grupo)
        );
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        INSERT INTO configuracoes_sistema (chave, valor, tipo, descricao, grupo, editavel)
        VALUES
            ('site_telefone_secundario', '(14) 99634-8409', 'Texto', 'Telefone comercial secundário', 'Geral', 1),
            ('site_whatsapp_secundario', '5514996348409', 'Texto', 'WhatsApp comercial secundário', 'Geral', 1),
            ('site_yara_email', 'corporativo.gna@gmail.com', 'Texto', 'E-mail de atendimento da Yara', 'Atendimento', 1),
            ('home_intro_titulo', 'Uma Nova Era Começa', 'Texto', 'Título principal do bloco institucional da home', 'SiteHome', 1),
            ('home_intro_texto_1', 'A Nexum Altivon está chegando para transformar e inovar o mercado digital brasileiro.', 'Texto', 'Primeiro texto institucional da home', 'SiteHome', 1),
            ('home_intro_texto_2', 'Nosso compromisso é claro: entregar qualidade superior, atendimento que faz a diferença e preços acessíveis que respeitam o seu bolso.', 'Texto', 'Segundo texto institucional da home', 'SiteHome', 1),
            ('home_intro_badge', 'www.nexumaltivon.com', 'Texto', 'Texto do selo institucional da home', 'SiteHome', 1),
            ('home_footer_texto', 'Portal em evolução contínua para vendas, relacionamento, parceiros e operações integradas.', 'Texto', 'Texto do rodapé público da home', 'SiteHome', 1),
            ('home_quality_items', '["Curadoria rigorosa de fornecedores","Atendimento humano e especializado","Política de devolução simplificada","Preços justos e acessíveis"]', 'JSON', 'Itens do bloco de qualidade da home', 'SiteHome', 1),
            ('home_partner_cards', '[{"title":"Parceiros de Vendas","text":"Lojas físicas ou online podem ampliar seus horizontes de venda com nossa infraestrutura comercial e operação integrada.","cta":"Quero Vender","href":"https://wa.me/5514996731879?text=Olá! Tenho interesse em ser parceiro de vendas do Grupo Nexum Altivon.","icon":"Store"},{"title":"Fornecedores & Distribuidores","text":"Distribuidores e fabricantes encontram um canal de venda em crescimento, com visão de volume, relacionamento e longo prazo.","cta":"Quero Fornecer","href":"https://wa.me/5514996348409?text=Olá! Sou fornecedor/distribuidor e tenho interesse em parceria com o Grupo Nexum Altivon.","icon":"Truck"},{"title":"Dropshipping","text":"Integre seu catálogo às nossas lojas ou utilize nossa infraestrutura para conectar produtos, logística e novos canais.","cta":"Quero Fazer Dropship","href":"https://wa.me/5514996731879?text=Olá! Tenho interesse em parceria de dropshipping com o Grupo Nexum Altivon.","icon":"Building2"}]', 'JSON', 'Cards de parceria da home', 'SiteHome', 1),
            ('home_hero_slides', '[{"id":"ecommerce","badge":"Grupo Nexum Altivon","title":"O Futuro do","highlight":"E-Commerce","description":"Seis lojas, uma operação conectada e uma proposta premium para transformar a experiência de compra online.","image":"https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=1920&q=88"},{"id":"marcas","badge":"6 marcas em expansão","title":"Uma operação,","highlight":"múltiplos mercados","description":"Turismo, relógios, moda, tecnologia, construção e festas com a mesma curadoria comercial do Grupo Nexum Altivon.","image":"https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=1920&q=88"},{"id":"tecnologia","badge":"Experiência tecnológica","title":"Compra segura com","highlight":"atendimento humano","description":"Fluxos preparados para catálogo, clientes, pedidos, integrações e relacionamento com visão de crescimento contínuo.","image":"https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=1920&q=88"}]', 'JSON', 'Slides principais da home', 'SiteHome', 1)
        ON DUPLICATE KEY UPDATE
            valor = VALUES(valor),
            descricao = VALUES(descricao),
            grupo = VALUES(grupo),
            editavel = VALUES(editavel),
            updated_at = CURRENT_TIMESTAMP;
        """.Replace("{", "{{").Replace("}", "}}"));
}

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

public sealed record CategoriaDto(
    string Id,
    string Nome,
    string Descricao,
    string? CategoriaPaiId = null,
    int Nivel = 1,
    string? Caminho = null,
    int? Ordem = null,
    bool Ativa = true);

public sealed record ProdutoLojaDto(
    string Id,
    string Nome,
    string Descricao,
    string? DescricaoCurta,
    decimal Preco,
    decimal? PrecoPromocional,
    string ImagemUrl,
    int Estoque,
    int EstoqueMinimo,
    int EstoqueReservado,
    bool Destaque,
    string Sku,
    string CategoriaId,
    decimal Avaliacao,
    decimal Custo,
    decimal Peso,
    decimal Altura,
    decimal Largura,
    decimal Comprimento,
    string TipoProduto,
    int? FornecedorId,
    string? Marca,
    string? Tags,
    string? SeoTitulo,
    string? SeoDescricao,
    string? SeoKeywords,
    string? ImagensGaleria);

public sealed record ProdutoRequest(
    string? Id,
    string Nome,
    string? Descricao,
    string? DescricaoCurta,
    decimal Preco,
    decimal? PrecoPromocional,
    string? ImagemUrl,
    int Estoque,
    int? EstoqueMinimo,
    int? EstoqueReservado,
    bool Destaque,
    string? Sku,
    string? CategoriaId,
    string? SubcategoriaId,
    decimal? Avaliacao,
    decimal? Custo,
    decimal? Peso,
    decimal? Altura,
    decimal? Largura,
    decimal? Comprimento,
    string? TipoProduto,
    int? FornecedorId,
    string? Marca,
    string? Tags,
    string? SeoTitulo,
    string? SeoDescricao,
    string? SeoKeywords,
    string? ImagensGaleria)
{
    public ProdutoLojaDto ToProduto(string? id = null)
    {
        var produtoId = string.IsNullOrWhiteSpace(id) ? Id : id;
        if (string.IsNullOrWhiteSpace(produtoId))
        {
            produtoId = Nome.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("/", "-")
                .Replace("\\", "-");
        }

        var sku = string.IsNullOrWhiteSpace(Sku)
            ? $"NA-{Math.Abs(HashCode.Combine(Nome, Preco)):000000}"
            : Sku;

        return new ProdutoLojaDto(
            produtoId,
            Nome,
            Descricao ?? string.Empty,
            DescricaoCurta,
            Preco,
            PrecoPromocional,
            ImagemUrl ?? "https://images.unsplash.com/photo-1523170335258-f5ed11844a49?auto=format&fit=crop&w=900&q=85",
            Estoque,
            EstoqueMinimo ?? 5,
            EstoqueReservado ?? 0,
            Destaque,
            sku,
            string.IsNullOrWhiteSpace(CategoriaId) ? "classicos" : CategoriaId,
            Avaliacao ?? 4.8m,
            Custo ?? 0m,
            Peso ?? 0m,
            Altura ?? 0m,
            Largura ?? 0m,
            Comprimento ?? 0m,
            string.IsNullOrWhiteSpace(TipoProduto) ? "Proprio" : TipoProduto,
            FornecedorId,
            Marca,
            Tags,
            SeoTitulo,
            SeoDescricao,
            SeoKeywords,
            ImagensGaleria);
    }
}

public sealed record UploadImagemRequest(string? FileName, string? ContentType, string DataUrl);

public sealed record UploadImagemDto(string Url);

public sealed record CupomDto(string Codigo, decimal? DescontoPercentual, decimal? DescontoValor, decimal? ValorMinimo);

public sealed record ClienteRequest(string Nome, string Email, string? Cpf, string? Telefone, string? Senha = null, bool? Newsletter = null);

public sealed record ReenviarConfirmacaoEmailRequest(string Email);

public sealed record ClienteCadastroResponse(int Id, string Nome, string Email, bool RequerConfirmacaoEmail, string Status);

public sealed record ClienteLojaDto(int Id, string Nome, string Email, string? Telefone, string? Cpf = null);

public sealed record CadastroClienteStatusDto(bool Existe, ClienteLojaDto? Cliente);

public sealed record ClientePortalPedidoDto(
    int Id,
    string NumeroPedido,
    string Status,
    string StatusPagamento,
    decimal Total,
    DateTime DataPedido,
    string? MeioPagamento,
    string? CodigoRastreio,
    string? Transportadora);

public sealed record ClientePortalDocumentoDto(
    int Id,
    int PedidoId,
    string? NumeroDocumento,
    string? ModeloDocumento,
    string StatusDocumento,
    string? ChaveAcesso,
    string? DanfeUrl,
    string? XmlUrl,
    DateTime CreatedAt);

public sealed record ClientePortalDto(
    int Id,
    string Nome,
    string Email,
    string? Telefone,
    string? Documento,
    int PontosFidelidade,
    string ScoreRelacionamento,
    bool Vip,
    decimal LimiteFuturoEstimado,
    List<ClientePortalPedidoDto> Pedidos,
    List<ClientePortalDocumentoDto> Documentos,
    List<string> Beneficios);

public sealed record FornecedorRequest(string Nome, string? Documento, string? Email, string? Telefone, string? Categoria);

public sealed record FornecedorDto(int Id, string Nome, string Documento, string Email, string Telefone, string Categoria, DateTime CreatedAt);

public sealed record StatusUpdateRequest(
    [property: JsonPropertyName("novo_status")] string NovoStatus,
    [property: JsonPropertyName("responsavel_id")] int? ResponsavelId);

public sealed record PedidoItemRequest(string ProdutoId, int Quantidade);

public sealed record PedidoRequest(
    [property: JsonPropertyName("cliente_id")] int ClienteId,
    [property: JsonPropertyName("loja_id")] string LojaId,
    [property: JsonPropertyName("itens")] List<PedidoItemRequest> Itens,
    [property: JsonPropertyName("cupom_codigo")] string? CupomCodigo,
    [property: JsonPropertyName("endereco_entrega")] object? EnderecoEntrega,
    [property: JsonPropertyName("metodo_pagamento")] string? MetodoPagamento,
    [property: JsonPropertyName("parcelas")] int? Parcelas,
    [property: JsonPropertyName("gateway_pagamento")] string? GatewayPagamento,
    [property: JsonPropertyName("frete_valor")] decimal? FreteValor,
    [property: JsonPropertyName("frete_metodo")] string? FreteMetodo,
    [property: JsonPropertyName("frete_transportadora")] string? FreteTransportadora,
    [property: JsonPropertyName("frete_prazo_dias")] int? FretePrazoDias);

public sealed record EnderecoEntregaRequest(
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado);

public sealed record PedidoLojaDto(
    int Id,
    string NumeroPedido,
    decimal Total,
    string Status,
    DateTime CreatedAt,
    string StatusPagamento,
    string? MeioPagamento,
    string? GatewayPagamento,
    string? GatewayTransacaoId,
    decimal FreteValor,
    string? FreteMetodo,
    string? FreteTransportadora,
    int FretePrazoDias,
    string InstrucaoPagamento,
    int Parcelas,
    string? PixQrcode,
    string? PaymentUrl);

public sealed record IntegracaoStatusDto(
    string Nome,
    string Slug,
    string Status,
    string Detalhe,
    bool Configurada = false,
    string Ambiente = "Nao configurado");

public sealed record IntegracaoDiagnosticoDto(
    string Nome,
    string Slug,
    string Status,
    bool Configurada,
    bool Operacional,
    string Detalhe,
    List<string> Pendencias,
    DateTime VerificadoEm,
    string? Referencia);

public sealed record IntegracaoCredencialDto(
    string Provedor,
    string Categoria,
    string Chave,
    string Uso,
    bool Obrigatoria);

public sealed record SiteConfiguracaoItemDto(
    int Id,
    string Chave,
    string? Valor,
    string Tipo,
    string? Descricao,
    string? Grupo,
    bool Editavel,
    DateTime UpdatedAt);

public sealed record SiteConfiguracaoUpdateItemDto(
    string Chave,
    string? Valor,
    string? Tipo,
    string? Descricao,
    string? Grupo,
    bool? Editavel);

public sealed record SiteConfiguracaoUpdateRequest(List<SiteConfiguracaoUpdateItemDto> Itens);

public sealed record HeroSlideSiteDto(
    string Id,
    string Badge,
    string Title,
    string Highlight,
    string Description,
    string Image);

public sealed record PartnerCardSiteDto(
    string Title,
    string Text,
    string Cta,
    string Href,
    string Icon);

public sealed record SiteConfiguracaoPublicaDto(
    string SiteNome,
    string SiteUrl,
    string ContactEmail,
    string PrimaryPhone,
    string SecondaryPhone,
    string PrimaryWhatsapp,
    string SecondaryWhatsapp,
    string YaraEmail,
    string SiteLogo,
    List<HeroSlideSiteDto> HeroSlides,
    string IntroTitle,
    string IntroText1,
    string IntroText2,
    string IntroBadge,
    List<string> QualityItems,
    List<PartnerCardSiteDto> PartnerCards,
    string FooterText);

public sealed record FreteCotacaoRequest(
    [property: JsonPropertyName("cep_origem")]
    string? CepOrigem,
    [property: JsonPropertyName("cep_destino")]
    string? CepDestino,
    [property: JsonPropertyName("itens")]
    List<FreteCotacaoItemRequest> Itens);

public sealed record FreteCotacaoItemRequest(
    [property: JsonPropertyName("sku")]
    string? Sku,
    [property: JsonPropertyName("quantidade")]
    int Quantidade,
    [property: JsonPropertyName("valor_unitario")]
    decimal ValorUnitario,
    [property: JsonPropertyName("peso_kg")]
    decimal? PesoKg,
    [property: JsonPropertyName("altura_cm")]
    decimal? AlturaCm,
    [property: JsonPropertyName("largura_cm")]
    decimal? LarguraCm,
    [property: JsonPropertyName("comprimento_cm")]
    decimal? ComprimentoCm);

public sealed record FreteCotacaoDto(
    string Codigo,
    string Nome,
    string Transportadora,
    decimal Valor,
    int PrazoDias,
    string Fonte);

public sealed record GatewayPaymentStartResult(
    bool Started,
    string Gateway,
    string? TransactionId,
    string? PixQrcode,
    string? PaymentUrl,
    string? RawPayload)
{
    public static GatewayPaymentStartResult Success(string gateway, string? transactionId, string? pixQrcode, string? paymentUrl, string? rawPayload) =>
        new(true, gateway, transactionId, pixQrcode, paymentUrl, rawPayload);

    public static GatewayPaymentStartResult NotStarted(string? rawPayload = null) =>
        new(false, "ConfiguracaoPendente", null, null, null, rawPayload);
}

public sealed record DashboardResumoDto(
    int PedidosHoje,
    int TotalClientes,
    decimal FaturamentoMes,
    int LeadsNovos,
    int ProdutosEstoqueBaixo,
    decimal Conversao,
    decimal TicketMedio);

public sealed record LeadLojaDto(
    int Id,
    string Nome,
    string Email,
    string Telefone,
    string Status,
    DateTime CreatedAt,
    string? Empresa,
    string? Origem,
    string? Mensagem);

public sealed class LeadRow
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Empresa { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed record LeadRequest(
    string Nome,
    string Email,
    string? Telefone,
    string? Status,
    string? Origem,
    string? Observacao,
    string? Empresa,
    string? Cnpj,
    string? Segmento,
    string? Mensagem,
    string? Whatsapp,
    string? Tipo);

public sealed record EmpresaGrupoRequest(
    string? TipoCadastro,
    string RazaoSocial,
    string? NomeFantasia,
    string Cnpj,
    string? InscricaoEstadual,
    string? InscricaoMunicipal,
    string? MatrizFilial,
    string? CodigoEmpresa,
    string? RegimeTributario,
    string? Crt,
    string? CnaePrincipal,
    string? CnaesSecundarios,
    string? CategoriaFiscal,
    string? SubcategoriaFiscal,
    string? NcmPadrao,
    string? NaturezaOperacaoPadrao,
    string? ResponsavelLegal,
    string? ResponsavelFiscal,
    string? EmailFiscal,
    string? EmailComercial,
    string? Telefone,
    string? Whatsapp,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado,
    string? Pais,
    string? AmbienteNfe,
    string? SerieNfe,
    string? SerieNfce,
    string? ModeloDocumentoPdv,
    string? AmbienteNfce,
    int? ProximaNfceNumero,
    string? NfceCsc,
    string? NfceCscIdToken,
    string? PdvSerieSat,
    string? PdvImpressoraFiscal,
    string? PdvNomeCaixaPadrao,
    bool? PdvContingenciaOffline,
    int? ProximaNfeNumero,
    string? CfopPadraoInterno,
    string? CfopPadraoInterestadual,
    decimal? AliquotaIcmsInterna,
    decimal? AliquotaIcmsInterestadual,
    decimal? AliquotaPis,
    decimal? AliquotaCofins,
    decimal? AliquotaIss,
    decimal? AliquotaIpi,
    decimal? CargaTributariaPercentual,
    string? PerfilTributacao,
    bool? UsaStLegado,
    bool? DestacaIcmsStSeparado,
    decimal? CustoOperacionalPercentual,
    decimal? MargemMinimaPercentual,
    int? PrioridadeFiscal,
    bool? PermiteNfeEntrada,
    bool? PermiteNfeSaida,
    bool? PermiteDropshipping,
    bool? PermiteMarketplace,
    bool? EmitentePreferencial,
    bool? Ativa,
    string? BeneficiosEstrategicos,
    string? ContratoResumo,
    string? Observacoes);

public sealed record EmpresaGrupoDto(
    int Id,
    string TipoCadastro,
    string RazaoSocial,
    string? NomeFantasia,
    string Cnpj,
    string? InscricaoEstadual,
    string? InscricaoMunicipal,
    string? MatrizFilial,
    string? CodigoEmpresa,
    string? RegimeTributario,
    string? Crt,
    string? CnaePrincipal,
    string? CnaesSecundarios,
    string? CategoriaFiscal,
    string? SubcategoriaFiscal,
    string? NcmPadrao,
    string? NaturezaOperacaoPadrao,
    string? ResponsavelLegal,
    string? ResponsavelFiscal,
    string? EmailFiscal,
    string? EmailComercial,
    string? Telefone,
    string? Whatsapp,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado,
    string? Pais,
    string? AmbienteNfe,
    string? SerieNfe,
    string? SerieNfce,
    string? ModeloDocumentoPdv,
    string? AmbienteNfce,
    int? ProximaNfceNumero,
    string? NfceCsc,
    string? NfceCscIdToken,
    string? PdvSerieSat,
    string? PdvImpressoraFiscal,
    string? PdvNomeCaixaPadrao,
    bool PdvContingenciaOffline,
    int? ProximaNfeNumero,
    string? CfopPadraoInterno,
    string? CfopPadraoInterestadual,
    decimal? AliquotaIcmsInterna,
    decimal? AliquotaIcmsInterestadual,
    decimal? AliquotaPis,
    decimal? AliquotaCofins,
    decimal? AliquotaIss,
    decimal? AliquotaIpi,
    decimal? CargaTributariaPercentual,
    string? PerfilTributacao,
    bool UsaStLegado,
    bool DestacaIcmsStSeparado,
    decimal? CustoOperacionalPercentual,
    decimal? MargemMinimaPercentual,
    int PrioridadeFiscal,
    bool PermiteNfeEntrada,
    bool PermiteNfeSaida,
    bool PermiteDropshipping,
    bool PermiteMarketplace,
    bool EmitentePreferencial,
    bool Ativa,
    string? BeneficiosEstrategicos,
    string? ContratoResumo,
    string? Observacoes,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record PdvFiscalConfigDto(
    int Id,
    string? CodigoEmpresa,
    string RazaoSocial,
    string Cnpj,
    string ModeloDocumentoPdv,
    string? AmbienteNfce,
    string? SerieNfce,
    int? ProximaNfceNumero,
    string? NfceCscIdToken,
    bool PossuiCscConfigurado,
    string? PdvSerieSat,
    string? PdvImpressoraFiscal,
    string? PdvNomeCaixaPadrao,
    bool PdvContingenciaOffline,
    bool EmitentePreferencial,
    string? Estado);

public sealed record FiscalPedidoDto(
    int Id,
    int PedidoId,
    int? EmpresaGrupoId,
    string? EmpresaEmitente,
    string? CodigoEmpresaEmitente,
    string? CnpjEmitente,
    string? NumeroNfe,
    string? Serie,
    string StatusNfe,
    string? StatusAutomacao,
    string? ModeloDocumento,
    string? AmbienteDocumento,
    string? Cfop,
    string? NaturezaOperacao,
    decimal? ValorTotal,
    string? ResumoRoteamento,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record FiscalRoutingSimulationRequest(
    TipoOperacaoFiscal TipoOperacao,
    decimal ValorProdutos,
    decimal ValorFrete,
    string EstadoOrigem,
    string EstadoDestino,
    string? CategoriaFiscal,
    string? SubcategoriaFiscal,
    string? NaturezaOperacao,
    bool ExigeMarketplace,
    bool ExigeDropshipping,
    bool RequerSaidaNfe,
    bool RequerEntradaNfe);

public sealed record FiscalRoutingSimulationDto(
    bool Sucesso,
    string Resumo,
    string? CodigoEmpresaSelecionada,
    string? RazaoSocialSelecionada,
    string? CnpjSelecionado,
    string? EstadoSelecionado,
    List<FiscalRoutingRankingDto> Ranking);

public sealed record FiscalRoutingRankingDto(
    string CodigoEmpresa,
    string RazaoSocial,
    string Cnpj,
    string? RegimeTributario,
    string? CategoriaFiscal,
    string? SubcategoriaFiscal,
    decimal CustoTributarioEstimado,
    decimal CustoOperacionalEstimado,
    decimal LucroEstimado,
    decimal MargemEstimadaPercentual,
    decimal Score,
    List<string> Justificativas);

public static class StoreData
{
    public static readonly List<CategoriaDto> Categorias =
    [
        new("automaticos", "Automaticos", "Movimento mecanico com presenca executiva", null, 1, "Automaticos", 1, true),
        new("dress-watch", "Dress Watch", "Subcategoria para modelos executivos e sociais.", "automaticos", 2, "Automaticos / Dress Watch", 1, true),
        new("skeleton", "Skeleton", "Subcategoria para mostradores abertos e mecânica aparente.", "automaticos", 2, "Automaticos / Skeleton", 2, true),
        new("cronografos", "Cronografos", "Performance, precisao e leitura esportiva", null, 1, "Cronografos", 2, true),
        new("corrida", "Corrida", "Subcategoria para cronógrafos de perfil esportivo.", "cronografos", 2, "Cronografos / Corrida", 1, true),
        new("aventura", "Aventura", "Subcategoria para peças robustas e outdoor.", "cronografos", 2, "Cronografos / Aventura", 2, true),
        new("classicos", "Classicos", "Pecas discretas para rotina premium", null, 1, "Classicos", 3, true),
        new("social", "Social", "Subcategoria para linha formal e corporativa.", "classicos", 2, "Classicos / Social", 1, true),
        new("minimalista", "Minimalista", "Subcategoria para peças leves e design limpo.", "classicos", 2, "Classicos / Minimalista", 2, true),
        new("smart-luxo", "Smart Luxo", "Tecnologia conectada com acabamento refinado", null, 1, "Smart Luxo", 4, true),
        new("fitness-premium", "Fitness Premium", "Subcategoria para wearables esportivos premium.", "smart-luxo", 2, "Smart Luxo / Fitness Premium", 1, true),
        new("executivo-connect", "Executivo Connect", "Subcategoria para smartwatches de perfil executivo.", "smart-luxo", 2, "Smart Luxo / Executivo Connect", 2, true)
    ];

    public static readonly List<ProdutoLojaDto> Produtos =
    [
        new("na-atlas-chrono", "Atlas Chronograph Black", "Cronografo em aco escovado, safira antirrisco e pulseira intercambiavel para uso executivo.", "Cronografo executivo premium", 4890, 4290, "https://images.unsplash.com/photo-1523170335258-f5ed11844a49?auto=format&fit=crop&w=900&q=85", 8, 2, 0, true, "NA-ATL-BLK", "cronografos", 4.9m, 3150, 0.450m, 4.5m, 11m, 25m, "Proprio", null, "Nexum Altivon", "cronografo,aco,premium", "Atlas Chronograph Black", "Relógio cronógrafo premium Nexum Altivon", "relogio,cronografo,premium", null),
        new("na-orion-gold", "Orion Gold Reserve", "Relogio automatico dourado com mostrador sunray, reserva de marcha e acabamento premium.", "Automático dourado premium", 6990, 6490, "https://images.unsplash.com/photo-1547996160-81dfa63595aa?auto=format&fit=crop&w=900&q=85", 12, 2, 0, true, "NA-ORI-GLD", "automaticos", 4.8m, 4520, 0.520m, 5m, 12m, 26m, "Proprio", null, "Nexum Altivon", "automatico,dourado,reserva", "Orion Gold Reserve", "Relógio automático dourado com reserva de marcha", "automatico,dourado,relogio", null),
        new("na-heritage-silver", "Heritage Silver 40mm", "Design classico em caixa fina, pulseira em couro italiano e resistencia a agua para o dia a dia.", "Clássico prata 40mm", 2990, null, "https://images.unsplash.com/photo-1539874754764-5a96559165b0?auto=format&fit=crop&w=900&q=85", 24, 4, 0, true, "NA-HER-SLV", "classicos", 4.7m, 1890, 0.380m, 4m, 10m, 24m, "Proprio", null, "Nexum Altivon", "classico,prata,couro", "Heritage Silver 40mm", "Relógio clássico prata com pulseira em couro italiano", "classico,prata,couro", null),
        new("na-venture-carbon", "Venture Carbon Pro", "Caixa em carbono, pulseira esportiva premium e leitura de alta visibilidade para jornadas intensas.", "Carbono esportivo premium", 5290, 4990, "https://images.unsplash.com/photo-1434056886845-dac89ffe9b56?auto=format&fit=crop&w=900&q=85", 5, 2, 0, false, "NA-VEN-CBN", "cronografos", 4.6m, 3380, 0.490m, 5m, 12m, 27m, "Dropshipping", null, "Nexum Altivon", "carbono,esportivo,aventura", "Venture Carbon Pro", "Relógio em carbono com leitura esportiva premium", "carbono,esportivo,relogio", null),
        new("na-lumina-smart", "Lumina Smart Luxe", "Tela AMOLED, monitoramento completo e corpo metalico com acabamento de relojoaria.", "Smartwatch luxo AMOLED", 3890, null, "https://images.unsplash.com/photo-1508685096489-7aacd43bd3b1?auto=format&fit=crop&w=900&q=85", 18, 3, 0, false, "NA-LUM-SMT", "smart-luxo", 4.8m, 2470, 0.310m, 4m, 10m, 23m, "Marketplace", null, "Nexum Altivon", "smartwatch,amoled,luxo", "Lumina Smart Luxe", "Smartwatch premium com acabamento de relojoaria", "smartwatch,amoled,luxo", null),
        new("na-minimal-rose", "Minimal Rose Mesh", "Perfil ultrafino, malha milanesa rose e mostrador minimalista para composicoes sofisticadas.", "Rose mesh minimalista", 2590, 2290, "https://images.unsplash.com/photo-1524592094714-0f0654e20314?auto=format&fit=crop&w=900&q=85", 31, 5, 0, false, "NA-MIN-RSE", "classicos", 4.7m, 1620, 0.280m, 3.8m, 9m, 22m, "Afiliado", null, "Nexum Altivon", "rose,minimalista,mesh", "Minimal Rose Mesh", "Relógio ultrafino rose com malha milanesa", "rose,minimalista,mesh", null)
    ];

    public static readonly List<CupomDto> Cupons =
    [
        new("NEXUM10", 10, null, 500),
        new("FRETEGRATIS", null, 89, 1000)
    ];

    public static readonly List<ClienteLojaDto> Clientes =
    [
        new(1, "Ana Carolina Silva", "ana.silva@email.com", "(14) 99876-5432"),
        new(2, "Bruno Oliveira", "bruno.oliveira@email.com", "(14) 99765-4321"),
        new(3, "Carla Mendes", "carla.mendes@email.com", "(14) 99654-3210")
    ];

    public static readonly List<FornecedorDto> Fornecedores =
    [
        new(1, "Chronos Imports", "12.345.678/0001-90", "comercial@chronosimports.com", "(11) 3030-1122", "Relogios", DateTime.UtcNow.AddDays(-9)),
        new(2, "Luxury Cases Brasil", "98.765.432/0001-10", "vendas@luxurycases.com", "(21) 4040-2211", "Acessorios", DateTime.UtcNow.AddDays(-5)),
        new(3, "Embalagens Prime", "45.111.222/0001-33", "atendimento@embalagensprime.com", "(31) 3333-9191", "Operacional", DateTime.UtcNow.AddDays(-2))
    ];

    public static readonly DashboardResumoDto Resumo = new(38, 1248, 286420, 64, 7, 7.8m, 3280);

    public static readonly List<PedidoLojaDto> Pedidos =
    [
        new(1029, "NA-1029", 6490, "Processando", DateTime.UtcNow.AddHours(-2), "Aguardando pagamento", "pix", "ConfiguracaoPendente", null, 0, "Retirada / combinar entrega", "Nexum Altivon", 0, "Pedido reservado. Configure o gateway para cobrança real e confirmação automática.", 1, "QR-CODE-PIX-DEMO", null),
        new(1028, "NA-1028", 4290, "Enviado", DateTime.UtcNow.AddHours(-5), "Pagamento aprovado", "cartao", "MercadoPago", "demo-1028", 29.9m, "Entrega padrão", "Correios / Melhor Envio", 7, "Pagamento confirmado. Pedido pronto para separação e logística.", 6, null, "https://pagamento.nexumaltivon.com/demo-1028"),
        new(1027, "NA-1027", 7580, "Entregue", DateTime.UtcNow.AddDays(-1), "Pagamento aprovado", "boleto", "MercadoPago", "demo-1027", 49.9m, "Entrega expressa", "Transportadora parceira", 3, "Pagamento confirmado. Pedido pronto para separação e logística.", 1, null, "https://pagamento.nexumaltivon.com/demo-1027")
    ];

    public static readonly List<LeadLojaDto> Leads =
    [
        new(210, "Marina Alves", "marina.alves@email.com", "(11) 98221-4400", "Qualificado", DateTime.UtcNow.AddHours(-3), "Marina Atelier", "Site", "Lead de demonstração qualificado."),
        new(209, "Rafael Monteiro", "rafael.m@email.com", "(21) 99774-1030", "Negociacao", DateTime.UtcNow.AddHours(-8), "RM Distribuição", "WhatsApp", "Contato comercial em andamento."),
        new(208, "Bianca Torres", "bianca.t@email.com", "(31) 98812-5511", "Novo", DateTime.UtcNow.AddDays(-1), "BT Boutique", "Site", "Solicitou retorno comercial.")
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
