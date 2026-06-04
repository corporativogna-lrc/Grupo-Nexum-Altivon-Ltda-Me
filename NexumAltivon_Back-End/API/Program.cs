using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NexumAltivon.API.Data;
using NexumAltivon.API.Models;
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
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Nexum Altivon API",
        Version = "v1.1.0",
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

var healthChecks = builder.Services.AddHealthChecks();
if (!string.IsNullOrWhiteSpace(connectionString))
{
    healthChecks.AddDbContextCheck<NexumDbContext>();
}

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
            version = "1.1.0"
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
            categoria.Descricao ?? string.Empty))
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

    var categoria = new Categoria
    {
        LojaId = lojaId,
        Nome = request.Nome,
        Slug = slug,
        Descricao = request.Descricao,
        Ativa = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.Categorias.Add(categoria);
    await db.SaveChangesAsync(ct);

    return Results.Created($"/api/categorias/{categoria.Slug}", ApiResponse<CategoriaDto>.Ok(new CategoriaDto(categoria.Slug, categoria.Nome, categoria.Descricao ?? string.Empty), "Categoria cadastrada."));
})
.WithName("CriarCategoria")
;

app.MapGet("/api/produtos", async (string? categoria_id, NexumDbContext db, CancellationToken ct) =>
{
    const string defaultImage = "https://images.unsplash.com/photo-1523170335258-f5ed11844a49?auto=format&fit=crop&w=900&q=85";

    IQueryable<Produto> query = db.Produtos.AsNoTracking().Where(produto => produto.Ativo);

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
            produto.Preco,
            produto.PrecoPromocional,
            produto.ImagemPrincipal ?? defaultImage,
            produto.EstoqueAtual,
            produto.Destaque,
            produto.Sku,
            produto.Categoria != null ? produto.Categoria.Slug : "classicos",
            4.8m))
        .ToListAsync(ct);

    if (produtos.Count == 0)
    {
        produtos = string.IsNullOrWhiteSpace(categoria_id)
            ? StoreData.Produtos
            : StoreData.Produtos
                .Where(produto => string.Equals(produto.CategoriaId, categoria_id, StringComparison.OrdinalIgnoreCase))
                .ToList();
    }

    return Results.Ok(ApiResponse<List<ProdutoLojaDto>>.Ok(produtos));
})
.AllowAnonymous()
.WithName("Produtos")
;

app.MapGet("/api/produtos/destaques", async (NexumDbContext db, CancellationToken ct) =>
{
    const string defaultImage = "https://images.unsplash.com/photo-1523170335258-f5ed11844a49?auto=format&fit=crop&w=900&q=85";

    var produtos = await db.Produtos
        .AsNoTracking()
        .Where(produto => produto.Ativo && produto.Destaque)
        .OrderByDescending(produto => produto.UpdatedAt)
        .Take(24)
        .Select(produto => new ProdutoLojaDto(
            produto.Slug,
            produto.Nome,
            produto.DescricaoCurta ?? produto.DescricaoLonga ?? string.Empty,
            produto.Preco,
            produto.PrecoPromocional,
            produto.ImagemPrincipal ?? defaultImage,
            produto.EstoqueAtual,
            produto.Destaque,
            produto.Sku,
            produto.Categoria != null ? produto.Categoria.Slug : "classicos",
            4.8m))
        .ToListAsync(ct);

    if (produtos.Count == 0)
    {
        produtos = StoreData.Produtos.Where(produto => produto.Destaque).ToList();
    }

    return Results.Ok(ApiResponse<List<ProdutoLojaDto>>.Ok(produtos));
})
.AllowAnonymous()
.WithName("ProdutosDestaques")
;

app.MapGet("/api/produtos/{id}", async (string id, NexumDbContext db, CancellationToken ct) =>
{
    const string defaultImage = "https://images.unsplash.com/photo-1523170335258-f5ed11844a49?auto=format&fit=crop&w=900&q=85";

    var dto = await db.Produtos
        .AsNoTracking()
        .Where(item => item.Slug == id)
        .Select(item => new ProdutoLojaDto(
            item.Slug,
            item.Nome,
            item.DescricaoCurta ?? item.DescricaoLonga ?? string.Empty,
            item.Preco,
            item.PrecoPromocional,
            item.ImagemPrincipal ?? defaultImage,
            item.EstoqueAtual,
            item.Destaque,
            item.Sku,
            item.Categoria != null ? item.Categoria.Slug : "classicos",
            4.8m))
        .FirstOrDefaultAsync(ct);

    if (dto is null)
    {
        var fallback = StoreData.Produtos.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));
        return fallback is null
            ? Results.NotFound(ApiResponse<string>.Erro("Produto nao encontrado."))
            : Results.Ok(ApiResponse<ProdutoLojaDto>.Ok(fallback));
    }

    return Results.Ok(ApiResponse<ProdutoLojaDto>.Ok(dto));
})
.AllowAnonymous()
.WithName("ProdutoPorId")
;

app.MapPost("/api/produtos", [Authorize(Policy = "Gerente")] async (
    ProdutoRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
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
    if (!string.IsNullOrWhiteSpace(request.CategoriaId))
    {
        categoriaId = await db.Categorias
            .AsNoTracking()
            .Where(categoria => categoria.Slug == request.CategoriaId)
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
        DescricaoCurta = request.Descricao,
        DescricaoLonga = request.Descricao,
        Preco = request.Preco,
        PrecoPromocional = request.PrecoPromocional,
        ImagemPrincipal = request.ImagemUrl,
        EstoqueAtual = request.Estoque,
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
        produto.Preco,
        produto.PrecoPromocional,
        produto.ImagemPrincipal ?? defaultImage,
        produto.EstoqueAtual,
        produto.Destaque,
        produto.Sku,
        request.CategoriaId ?? "classicos",
        4.8m);

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
    produto.DescricaoCurta = request.Descricao;
    produto.DescricaoLonga = request.Descricao;
    produto.Preco = request.Preco;
    produto.PrecoPromocional = request.PrecoPromocional;
    produto.ImagemPrincipal = request.ImagemUrl;
    produto.EstoqueAtual = request.Estoque;
    produto.Destaque = request.Destaque;
    produto.UpdatedAt = DateTime.UtcNow;

    if (!string.IsNullOrWhiteSpace(request.CategoriaId))
    {
        var categoriaId = await db.Categorias
            .AsNoTracking()
            .Where(categoria => categoria.Slug == request.CategoriaId)
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
        produto.Preco,
        produto.PrecoPromocional,
        produto.ImagemPrincipal ?? defaultImage,
        produto.EstoqueAtual,
        produto.Destaque,
        produto.Sku,
        request.CategoriaId ?? "classicos",
        4.8m);

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
        .Select(cliente => new ClienteLojaDto(cliente.Id, cliente.Nome, cliente.Email, cliente.Telefone))
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<ClienteLojaDto>>.Ok(clientes));
})
.WithName("Clientes")
;

app.MapPost("/api/clientes", async (ClienteRequest request, NexumDbContext db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Nome))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Nome e email sao obrigatorios."));
    }

    var email = request.Email.Trim().ToLowerInvariant();
    var cpfCnpj = string.IsNullOrWhiteSpace(request.Cpf) ? null : request.Cpf.Trim();
    var clienteExistente = await db.Clientes.FirstOrDefaultAsync(cliente =>
        cliente.Email == email ||
        (!string.IsNullOrWhiteSpace(cpfCnpj) && cliente.CpfCnpj == cpfCnpj), ct);

    if (clienteExistente is not null)
    {
        if (string.IsNullOrWhiteSpace(clienteExistente.Telefone) && !string.IsNullOrWhiteSpace(request.Telefone))
        {
            clienteExistente.Telefone = request.Telefone.Trim();
            clienteExistente.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }

        var existenteDto = new ClienteLojaDto(clienteExistente.Id, clienteExistente.Nome, clienteExistente.Email, clienteExistente.Telefone);
        return Results.Ok(ApiResponse<ClienteLojaDto>.Ok(existenteDto, "Cliente ja cadastrado. Registro existente reutilizado."));
    }

    var cliente = new Cliente
    {
        Nome = request.Nome.Trim(),
        Email = email,
        Telefone = request.Telefone,
        CpfCnpj = cpfCnpj,
        Status = StatusCliente.Ativo,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.Clientes.Add(cliente);
    await db.SaveChangesAsync(ct);

    var dto = new ClienteLojaDto(cliente.Id, cliente.Nome, cliente.Email, cliente.Telefone);
    return Results.Ok(ApiResponse<ClienteLojaDto>.Ok(dto, "Cliente registrado."));
})
.AllowAnonymous()
.WithName("CriarCliente")
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

    var fornecedor = new Fornecedor
    {
        RazaoSocial = request.Nome.Trim(),
        Cnpj = string.IsNullOrWhiteSpace(request.Documento) ? null : request.Documento.Trim(),
        Email = request.Email,
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
        .OrderByDescending(pedido => pedido.CreatedAt)
        .Take(500)
        .Select(pedido => new
        {
            pedido.Id,
            pedido.NumeroPedido,
            pedido.Total,
            pedido.Status,
            pedido.CreatedAt
        })
        .ToListAsync(ct);

    var dtos = pedidos.Select(pedido => new PedidoLojaDto(
        pedido.Id,
        pedido.NumeroPedido,
        pedido.Total,
        FormatStatusPedido(pedido.Status),
        pedido.CreatedAt)).ToList();

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
    var pedido = await db.Pedidos.FirstOrDefaultAsync(item => item.Id == id, ct);
    if (pedido is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Pedido nao encontrado."));
    }

    if (!TryParseStatusPedido(request.NovoStatus, out var status))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Status do pedido invalido."));
    }

    pedido.Status = status;
    pedido.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync(ct);

    var dto = new PedidoLojaDto(pedido.Id, pedido.NumeroPedido, pedido.Total, FormatStatusPedido(pedido.Status), pedido.CreatedAt);
    return Results.Ok(ApiResponse<PedidoLojaDto>.Ok(dto, "Status do pedido atualizado."));
})
.WithName("AtualizarStatusPedido")
;

app.MapPost("/api/pedidos", async (PedidoRequest request, NexumDbContext db, HttpContext http, CancellationToken ct) =>
{
    if (request.Itens is null || request.Itens.Count == 0)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Itens do pedido obrigatorios."));
    }

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

    var produtosMap = await db.Produtos
        .AsNoTracking()
        .Where(produto => request.Itens.Select(item => item.ProdutoId).Contains(produto.Slug))
        .ToDictionaryAsync(produto => produto.Slug, ct);

    decimal subtotal = 0m;
    var itens = new List<PedidoItem>(request.Itens.Count);
    foreach (var item in request.Itens)
    {
        if (!produtosMap.TryGetValue(item.ProdutoId, out var produto))
        {
            return Results.BadRequest(ApiResponse<string>.Erro("Produto invalido."));
        }

        var precoUnitario = produto.PrecoPromocional ?? produto.Preco;
        var precoTotal = precoUnitario * item.Quantidade;
        subtotal += precoTotal;

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
        NumeroPedido = $"NX{DateTime.UtcNow:yyMMddHHmmss}",
        ClienteId = cliente.Id,
        EnderecoEntregaId = enderecoEntregaId,
        LojaId = lojaId,
        Status = StatusPedido.Pendente,
        StatusPagamento = StatusPagamento.Aguardando,
        Subtotal = subtotal,
        Desconto = desconto,
        Total = Math.Max(0m, subtotal - desconto),
        CupomCodigo = request.CupomCodigo,
        Origem = OrigemPedido.Site,
        IpCliente = http.Connection.RemoteIpAddress?.ToString(),
        UserAgent = http.Request.Headers.UserAgent.ToString(),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Itens = itens
    };

    db.Pedidos.Add(pedido);
    await db.SaveChangesAsync(ct);

    var dto = new PedidoLojaDto(pedido.Id, pedido.NumeroPedido, pedido.Total, FormatStatusPedido(pedido.Status), pedido.CreatedAt);
    return Results.Ok(ApiResponse<PedidoLojaDto>.Ok(dto, "Pedido criado com sucesso."));
})
.AllowAnonymous()
.WithName("CriarPedido")
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
        lead.CreatedAt)).ToList();

    return Results.Ok(ApiResponse<List<LeadLojaDto>>.Ok(dtos));
})
.WithName("Leads")
;

app.MapPost("/api/crm/leads", [Authorize(Policy = "Gerente")] async (LeadRequest request, NexumDbContext db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Nome))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Nome do lead obrigatorio."));
    }

    var lead = new CrmLead
    {
        Nome = request.Nome.Trim(),
        Email = request.Email,
        Telefone = request.Telefone,
        Status = TryParseStatusLead(request.Status, out var status) ? status : StatusLead.Novo,
        Origem = TryParseOrigemLead(request.Origem, out var origem) ? origem : OrigemLead.Site,
        Anotacoes = request.Observacao,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.CrmLeads.Add(lead);
    await db.SaveChangesAsync(ct);

    var dto = new LeadLojaDto(lead.Id, lead.Nome, lead.Email ?? string.Empty, lead.Telefone ?? string.Empty, FormatStatusLead(lead.Status), lead.CreatedAt);
    return Results.Ok(ApiResponse<LeadLojaDto>.Ok(dto, "Lead cadastrado no CRM."));
})
.WithName("CriarLead")
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

    var dto = new LeadLojaDto(lead.Id, lead.Nome, lead.Email ?? string.Empty, lead.Telefone ?? string.Empty, FormatStatusLead(lead.Status), lead.CreatedAt);
    return Results.Ok(ApiResponse<LeadLojaDto>.Ok(dto, "Status do lead atualizado."));
})
.WithName("AtualizarStatusLead")
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

static string FormatStatusPedido(StatusPedido status) =>
    status switch
    {
        StatusPedido.EmSeparacao => "Processando",
        _ => status.ToString()
    };

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

public sealed record ProdutoRequest(
    string? Id,
    string Nome,
    string? Descricao,
    decimal Preco,
    decimal? PrecoPromocional,
    string? ImagemUrl,
    int Estoque,
    bool Destaque,
    string? Sku,
    string? CategoriaId,
    decimal? Avaliacao)
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
            Preco,
            PrecoPromocional,
            ImagemUrl ?? "https://images.unsplash.com/photo-1523170335258-f5ed11844a49?auto=format&fit=crop&w=900&q=85",
            Estoque,
            Destaque,
            sku,
            string.IsNullOrWhiteSpace(CategoriaId) ? "classicos" : CategoriaId,
            Avaliacao ?? 4.8m);
    }
}

public sealed record CupomDto(string Codigo, decimal? DescontoPercentual, decimal? DescontoValor, decimal? ValorMinimo);

public sealed record ClienteRequest(string Nome, string Email, string? Cpf, string? Telefone);

public sealed record ClienteLojaDto(int Id, string Nome, string Email, string? Telefone);

public sealed record FornecedorRequest(string Nome, string? Documento, string? Email, string? Telefone, string? Categoria);

public sealed record FornecedorDto(int Id, string Nome, string Documento, string Email, string Telefone, string Categoria, DateTime CreatedAt);

public sealed record StatusUpdateRequest(
    [property: JsonPropertyName("novo_status")] string NovoStatus,
    [property: JsonPropertyName("responsavel_id")] int? ResponsavelId);

public sealed record PedidoItemRequest(string ProdutoId, int Quantidade);

public sealed record PedidoRequest(int ClienteId, string LojaId, List<PedidoItemRequest> Itens, string? CupomCodigo, object? EnderecoEntrega);

public sealed record EnderecoEntregaRequest(
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado);

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

public sealed class LeadRow
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed record LeadRequest(string Nome, string Email, string? Telefone, string? Status, string? Origem, string? Observacao);

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
