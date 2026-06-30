/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NexumAltivon.API.Data;
using NexumAltivon.API.ERP.FiscalRouting;
using NexumAltivon.API.ERP.SharedData;
using NexumAltivon.API.Models;
using NexumAltivon.API.Services;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine("[NexumStartup] Builder criado.");

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
var genesisConnectionString = builder.Configuration.GetConnectionString("GenesisConnection");

if (!string.IsNullOrWhiteSpace(connectionString))
{
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
    builder.Services.AddDbContext<NexumDbContext>(options =>
        options.UseMySql(connectionString, serverVersion));
}

if (!string.IsNullOrWhiteSpace(genesisConnectionString))
{
    var genesisServerVersion = new MySqlServerVersion(new Version(8, 0, 0));
    builder.Services.AddDbContext<GenesisDbContext>(options =>
        options.UseMySql(genesisConnectionString, genesisServerVersion));
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("NexumCorsPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging())
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            return;
        }

        var origins = apiSettings.GetSection("CorsOrigins").Get<string[]>()
            ?? new[]
            {
                "https://nexumaltivon.com.br",
                "https://www.nexumaltivon.com.br",
                "https://admin.nexumaltivon.com.br",
                "https://api.nexumaltivon.com.br"
            };

        policy
            .WithOrigins(origins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Token-Expired", "X-Total-Count", "X-Page-Count");
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
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

Console.WriteLine("[NexumStartup] Montando aplicacao.");
var app = builder.Build();
Console.WriteLine("[NexumStartup] Aplicacao montada.");

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        try
        {
            await EnsureOperationalSchemaAsync(app.Services, app.Logger);
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Operational schema check failed. API will continue running.");
        }
    });
});

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

app.MapGet("/health/db/genesis", async (IServiceProvider services, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(genesisConnectionString))
    {
        return Results.Ok(new { status = "sem_genesis_configurado" });
    }

    try
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GenesisDbContext>();
        var canConnect = await db.Database.CanConnectAsync(ct);
        return canConnect
            ? Results.Ok(new { status = "Healthy" })
            : Results.Problem("Genesis configurado, mas sem conexão.");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/api/erp/genesis/financeiro/resumo", async (GenesisDbContext db, CancellationToken ct) =>
{
    var resumo = await GenesisFinanceService.GetResumoAsync(db, ct);
    return Results.Ok(resumo);
})
.RequireAuthorization("Financeiro");

app.MapGet("/api/erp/genesis/financeiro/contas-pagar", async (GenesisDbContext db, CancellationToken ct) =>
{
    var itens = await GenesisFinanceService.ListarContasPagarAsync(db, ct);
    return Results.Ok(itens);
})
.RequireAuthorization("Financeiro");

app.MapGet("/api/erp/genesis/financeiro/contas-receber", async (GenesisDbContext db, CancellationToken ct) =>
{
    var itens = await GenesisFinanceService.ListarContasReceberAsync(db, ct);
    return Results.Ok(itens);
})
.RequireAuthorization("Financeiro");

app.MapPost("/api/erp/genesis/financeiro/contas-pagar", async (GenesisDbContext db, GenesisContaPagarCreateRequest request, CancellationToken ct) =>
{
    var created = await GenesisFinanceService.CriarContaPagarAsync(db, request, ct);
    return Results.Created($"/api/erp/genesis/financeiro/contas-pagar/{created.Id}", created);
})
.RequireAuthorization("Financeiro");

app.MapPost("/api/erp/genesis/financeiro/contas-receber", async (GenesisDbContext db, GenesisContaReceberCreateRequest request, CancellationToken ct) =>
{
    var created = await GenesisFinanceService.CriarContaReceberAsync(db, request, ct);
    return Results.Created($"/api/erp/genesis/financeiro/contas-receber/{created.Id}", created);
})
.RequireAuthorization("Financeiro");

app.MapPost("/api/erp/genesis/financeiro/contas-pagar/{id:int}/baixa", async (int id, GenesisDbContext db, GenesisBaixaPagarRequest request, CancellationToken ct) =>
{
    var updated = await GenesisFinanceService.BaixarContaPagarAsync(db, id, request, ct);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
})
.RequireAuthorization("Financeiro");

app.MapPost("/api/erp/genesis/financeiro/contas-receber/{id:int}/baixa", async (int id, GenesisDbContext db, GenesisBaixaReceberRequest request, CancellationToken ct) =>
{
    var updated = await GenesisFinanceService.BaixarContaReceberAsync(db, id, request, ct);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
})
.RequireAuthorization("Financeiro");

app.MapGet("/api/erp/genesis/financeiro/boletos", async (GenesisDbContext db, CancellationToken ct) =>
{
    var boletos = await GenesisFinanceService.ListarBoletosAsync(db, ct);
    return Results.Ok(boletos);
})
.RequireAuthorization("Financeiro");

app.MapPost("/api/erp/genesis/financeiro/boletos", async (GenesisDbContext db, GenesisBoletoCreateRequest request, CancellationToken ct) =>
{
    var created = await GenesisFinanceService.CriarBoletoAsync(db, request, ct);
    return Results.Created($"/api/erp/genesis/financeiro/boletos/{created.Id}", created);
})
.RequireAuthorization("Financeiro");

app.MapGet("/api/erp/genesis/financeiro/referencias", async (GenesisDbContext db, string? tipo, CancellationToken ct) =>
{
    var referencias = await GenesisFinanceService.ListarReferenciasAsync(db, tipo, ct);
    return Results.Ok(referencias);
})
.RequireAuthorization("Financeiro");

app.MapPost("/api/erp/genesis/financeiro/referencias", async (GenesisDbContext db, GenesisFinanceReferenciaCreateRequest request, CancellationToken ct) =>
{
    try
    {
        var created = await GenesisFinanceService.CriarReferenciaAsync(db, request, ct);
        return Results.Created($"/api/erp/genesis/financeiro/referencias/{created.Id}", created);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { erro = ex.Message });
    }
})
.RequireAuthorization("Financeiro");

app.MapGet("/api/erp/genesis/rh/resumo", async (GenesisDbContext db, CancellationToken ct) =>
{
    var resumo = await GenesisRhService.GetResumoAsync(db, ct);
    return Results.Ok(resumo);
})
.RequireAuthorization("RH");

app.MapGet("/api/erp/genesis/rh/colaboradores", async (GenesisDbContext db, CancellationToken ct) =>
{
    var colaboradores = await GenesisRhService.GetColaboradoresAsync(db, ct);
    return Results.Ok(colaboradores);
})
.RequireAuthorization("RH");

app.MapPost("/api/erp/genesis/rh/colaboradores", async (GenesisDbContext db, GenesisRhColaboradorUpsertRequest request, CancellationToken ct) =>
{
    var created = await GenesisRhService.CriarColaboradorAsync(db, request, ct);
    return Results.Created($"/api/erp/genesis/rh/colaboradores/{created.Id}", created);
})
.RequireAuthorization("RH");

app.MapPut("/api/erp/genesis/rh/colaboradores/{id:int}", async (int id, GenesisDbContext db, GenesisRhColaboradorUpsertRequest request, CancellationToken ct) =>
{
    var updated = await GenesisRhService.AtualizarColaboradorAsync(db, id, request, ct);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
})
.RequireAuthorization("RH");

app.MapPatch("/api/erp/genesis/rh/colaboradores/{id:int}/status", async (int id, GenesisDbContext db, GenesisRhStatusUpdateRequest request, CancellationToken ct) =>
{
    var updated = await GenesisRhService.AtualizarStatusAsync(db, id, request.Status, ct);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
})
.RequireAuthorization("RH");

app.MapGet("/api/erp/genesis/rh/referencias", async (GenesisDbContext db, string? tipo, CancellationToken ct) =>
{
    var referencias = await GenesisRhService.ListarReferenciasAsync(db, tipo, ct);
    return Results.Ok(referencias);
})
.RequireAuthorization("RH");

app.MapPost("/api/erp/genesis/rh/referencias", async (GenesisDbContext db, GenesisRhReferenciaCreateRequest request, CancellationToken ct) =>
{
    try
    {
        var created = await GenesisRhService.CriarReferenciaAsync(db, request, ct);
        return Results.Created($"/api/erp/genesis/rh/referencias/{created.Id}", created);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { erro = ex.Message });
    }
})
.RequireAuthorization("RH");

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
    var normalizedEmail = NormalizeEmail(request.Email);
    if (string.IsNullOrWhiteSpace(normalizedEmail))
    {
        return Results.Unauthorized();
    }

    var expirationHours = configuration.GetValue("JwtSettings:ExpirationHours", 24);

    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(item => item.Email == normalizedEmail && item.Ativo, ct);

    if (usuario is not null && BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
    {
        usuario.UltimoLogin = DateTime.UtcNow;
        usuario.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        var perfil = usuario.Perfil.ToString();
        var usuarioResponse = CreateLoginResponse(usuario.Id, usuario.Nome, usuario.Email, perfil, issuer, audience, signingKey, expirationHours);
        return Results.Ok(ApiResponse<LoginResponse>.Ok(usuarioResponse, "Login realizado com sucesso."));
    }

    if (!environment.IsProduction())
    {
        var admin = configuration.GetSection("AdminUser");
        var configuredEmail = NormalizeEmail(admin["Email"] ?? "admin@nexumaltivon.com");
        var configuredPassword = admin["Password"];
        var configuredName = admin["Name"] ?? "Administrador Nexum";
        var configuredRole = admin["Role"] ?? "Gerente";

        if (!IsConfiguredSecret(configuredPassword))
        {
            configuredPassword = "Admin@123";
        }

        if (string.Equals(normalizedEmail, configuredEmail, StringComparison.OrdinalIgnoreCase)
            && request.Senha == configuredPassword)
        {
            var adminResponse = CreateLoginResponse(1, configuredName, configuredEmail!, configuredRole, issuer, audience, signingKey, expirationHours);
            return Results.Ok(ApiResponse<LoginResponse>.Ok(adminResponse, "Login administrativo de desenvolvimento realizado com sucesso."));
        }
    }

    var cliente = await db.Clientes
        .FirstOrDefaultAsync(item => item.Email == normalizedEmail, ct);

    if (cliente is not null && !string.IsNullOrWhiteSpace(cliente.SenhaHash) && BCrypt.Net.BCrypt.Verify(request.Senha, cliente.SenhaHash))
    {
        if (cliente.Status != StatusCliente.Ativo)
        {
            return Results.BadRequest(ApiResponse<LoginResponse>.Erro("Seu cadastro ainda não foi confirmado. Verifique seu e-mail antes de entrar."));
        }

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

app.MapPost("/api/sistema/validar-token", async (
    ValidacaoTokenRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var token = TrimOrNull(request.Token);
    if (string.IsNullOrWhiteSpace(token))
    {
        return Results.Unauthorized();
    }

    var tokenHash = ComputeSha256Hash(token);
    var credencial = await db.ConfiguracoesSistema
        .AsNoTracking()
        .Where(item => item.Grupo == "Credenciais" && item.Chave.StartsWith("validacao_token_"))
        .FirstOrDefaultAsync(item => item.Valor == tokenHash, ct);

    if (credencial is null)
    {
        return Results.Unauthorized();
    }

    var response = new ValidacaoTokenResponse(credencial.Chave.ToUpperInvariant(), credencial.Descricao ?? "Token de validacao ativo");
    return Results.Ok(ApiResponse<ValidacaoTokenResponse>.Ok(response, "Token validado com sucesso."));
})
.AllowAnonymous()
.WithName("ValidarTokenSistema");

app.MapGet("/api/sistema/credenciais/status", [Authorize(Policy = "Admin")] async (NexumDbContext db, CancellationToken ct) =>
{
    var tokens = await db.ConfiguracoesSistema
        .AsNoTracking()
        .Where(item => item.Grupo == "Credenciais" && item.Chave.StartsWith("validacao_token_"))
        .OrderBy(item => item.Chave)
        .Select(item => new CredencialSistemaStatusDto(
            item.Chave.ToUpperInvariant(),
            !string.IsNullOrWhiteSpace(item.Valor),
            item.Descricao,
            item.UpdatedAt))
        .ToListAsync(ct);

    var perfisAtivos = await db.Usuarios
        .AsNoTracking()
        .Where(item => item.Ativo)
        .Select(item => item.Perfil)
        .ToListAsync(ct);

    var usuarios = perfisAtivos
        .GroupBy(perfil => perfil)
        .Select(group => new UsuarioPerfilStatusDto(group.Key.ToString(), group.Count()))
        .OrderBy(item => item.Perfil)
        .ToList();

    return Results.Ok(ApiResponse<CredenciaisSistemaStatusDto>.Ok(
        new CredenciaisSistemaStatusDto(tokens, usuarios),
        "Credenciais operacionais cadastradas sem exposicao de valores sensiveis."));
})
.WithName("CredenciaisSistemaStatus");

app.MapGet("/api/auth/me", (ClaimsPrincipal principal) =>
{
    var idRaw = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? "0";
    _ = int.TryParse(idRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id);

    var nome = principal.FindFirstValue(ClaimTypes.Name) ?? "Administrador Nexum";
    var email = principal.FindFirstValue(ClaimTypes.Email)
        ?? principal.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? string.Empty;
    var perfil = principal.FindFirstValue("perfil")
        ?? principal.FindFirstValue(ClaimTypes.Role)
        ?? "Gerente";

    return Results.Ok(ApiResponse<UsuarioDto>.Ok(new UsuarioDto(id, nome, email, perfil)));
})
.RequireAuthorization()
.WithName("AuthMe");

app.MapGet("/api/admin/dashboard/completo", [Authorize(Policy = "Gerente")] async (
    NexumDbContext db,
    CancellationToken ct) =>
{
    var dashboard = await BuildAdminDashboardAsync(db, ct);
    return Results.Ok(ApiResponse<DashboardCompletoDto>.Ok(dashboard));
})
.WithName("DashboardCompleto")
;

app.MapGet("/api/admin/dashboard/kpis", [Authorize(Policy = "Gerente")] async (
    NexumDbContext db,
    CancellationToken ct) =>
    Results.Ok(ApiResponse<DashboardKpiDto>.Ok(await BuildAdminKpisAsync(db, ct))))
    .WithName("DashboardKpis")
    ;

app.MapGet("/api/admin/genesisgest/schema-status", [Authorize(Policy = "Gerente")] async (
    NexumDbContext db,
    CancellationToken ct) =>
    Results.Ok(ApiResponse<GenesisGestSchemaStatusDto>.Ok(
        await BuildGenesisGestSchemaStatusAsync(db, ct),
        "Estrutura GenesisGest.Net verificada.")))
    .WithName("GenesisGestSchemaStatus")
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

app.MapGet("/api/produtos", async (
    string? categoria_id,
    string? categoriaIdFiltro,
    string? busca,
    int? pagina,
    int? itensPorPagina,
    int? Pagina,
    int? ItensPorPagina,
    NexumDbContext db,
    CancellationToken ct) =>
{
    IQueryable<Produto> query = FiltrarProdutosPublicaveis(db.Produtos.AsNoTracking());
    var categoriaFiltro = !string.IsNullOrWhiteSpace(categoria_id) ? categoria_id : categoriaIdFiltro;

    if (!string.IsNullOrWhiteSpace(categoriaFiltro))
    {
        var categoriaId = await db.Categorias
            .AsNoTracking()
            .Where(categoria => categoria.Slug == categoriaFiltro || categoria.Id.ToString() == categoriaFiltro)
            .Select(categoria => (int?)categoria.Id)
            .FirstOrDefaultAsync(ct);

        if (categoriaId is null)
        {
            return Results.Ok(ApiResponse<List<ProdutoLojaDto>>.Ok([]));
        }

        query = query.Where(produto => produto.CategoriaId == categoriaId);
    }

    if (!string.IsNullOrWhiteSpace(busca))
    {
        var termo = busca.Trim();
        query = query.Where(produto =>
            produto.Nome.Contains(termo) ||
            produto.Sku.Contains(termo) ||
            (produto.DescricaoCurta != null && produto.DescricaoCurta.Contains(termo)) ||
            (produto.Categoria != null && produto.Categoria.Nome.Contains(termo)));
    }

    var paginaAtual = Math.Max(1, pagina ?? Pagina ?? 1);
    var limite = Math.Clamp(itensPorPagina ?? ItensPorPagina ?? 20, 1, 60);
    var total = await query.CountAsync(ct);
    var produtos = await query
        .Include(produto => produto.Categoria)
        .OrderByDescending(produto => produto.Destaque)
        .ThenByDescending(produto => produto.UpdatedAt)
        .Skip((paginaAtual - 1) * limite)
        .Take(limite)
        .ToListAsync(ct);

    var totalPaginas = (int)Math.Ceiling(total / (double)limite);
    return Results.Ok(ApiResponse<List<ProdutoLojaDto>>.Ok(
        produtos.Select(MapearProdutoLojaDto).ToList(),
        total: total,
        pagina: paginaAtual,
        totalPaginas: totalPaginas));
})
.AllowAnonymous()
.WithName("Produtos")
;

app.MapGet("/api/produtos/destaques", async (int? limite, NexumDbContext db, CancellationToken ct) =>
{
    var totalDestaques = Math.Clamp(limite ?? 5, 1, 12);
    var produtos = await FiltrarProdutosPublicaveis(db.Produtos.AsNoTracking())
        .Include(produto => produto.Categoria)
        .Where(produto => produto.Destaque)
        .OrderByDescending(produto => produto.UpdatedAt)
        .Take(totalDestaques)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<ProdutoLojaDto>>.Ok(produtos.Select(MapearProdutoLojaDto).ToList()));
})
.AllowAnonymous()
.WithName("ProdutosDestaques")
;

app.MapGet("/api/produtos/{id}", async (string id, NexumDbContext db, CancellationToken ct) =>
{
    var produto = await FiltrarProdutosPublicaveis(db.Produtos.AsNoTracking())
        .Include(item => item.Categoria)
        .Where(item => item.Slug == id)
        .FirstOrDefaultAsync(ct);

    if (produto is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Produto nao encontrado ou cadastro incompleto."));
    }

    var dto = MapearProdutoLojaDto(produto);
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

    var empresaAquisicao = await ObterEmpresaAquisicaoAsync(db, request.EmpresaAquisicaoCodigo, ct);
    var sku = await GerarSkuProdutoAsync(db, request, empresaAquisicao, null, ct);

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
        CodigoBarras = GerarCodigoBarrasProdutoPorSku(sku),
        QrCode = GerarQrCodeProdutoCadastro(sku, request.Nome, request.TipoProduto, request.FornecedorId),
        IdentificacaoEstoque = GerarIdentificacaoEstoqueCadastro(sku, request.Nome, request.TipoProduto, request.FornecedorId),
        Destaque = request.Destaque,
        Ativo = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.Produtos.Add(produto);
    await db.SaveChangesAsync(ct);

    var dto = new ProdutoLojaDto(
        produto.Slug,
        produto.Nome,
        produto.DescricaoCurta ?? produto.DescricaoLonga ?? string.Empty,
        produto.DescricaoCurta,
        produto.Preco,
        produto.PrecoPromocional,
        produto.ImagemPrincipal ?? string.Empty,
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
        produto.ImagensGaleria,
        produto.CodigoBarras,
        produto.QrCode,
        produto.IdentificacaoEstoque);

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

    var empresaAquisicao = await ObterEmpresaAquisicaoAsync(db, request.EmpresaAquisicaoCodigo, ct);
    produto.Sku = await GerarSkuProdutoAsync(db, request, empresaAquisicao, produto.Id, ct);

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
    produto.CodigoBarras = GerarCodigoBarrasProdutoPorSku(produto.Sku);
    produto.QrCode = GerarQrCodeProdutoCadastro(produto.Sku, request.Nome, request.TipoProduto, request.FornecedorId);
    produto.IdentificacaoEstoque = GerarIdentificacaoEstoqueCadastro(produto.Sku, request.Nome, request.TipoProduto, request.FornecedorId);
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

    var dto = new ProdutoLojaDto(
        produto.Slug,
        produto.Nome,
        produto.DescricaoCurta ?? produto.DescricaoLonga ?? string.Empty,
        produto.DescricaoCurta,
        produto.Preco,
        produto.PrecoPromocional,
        produto.ImagemPrincipal ?? string.Empty,
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
        produto.ImagensGaleria,
        produto.CodigoBarras,
        produto.QrCode,
        produto.IdentificacaoEstoque);

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
        return Results.NotFound(ApiResponse<string>.Erro("Cupom invalido."));
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
    var clientesDb = await db.Clientes
        .AsNoTracking()
        .OrderByDescending(cliente => cliente.CreatedAt)
        .Take(500)
        .ToListAsync(ct);
    var clientes = clientesDb.Select(ToClienteLojaDto).ToList();

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

    if (!string.IsNullOrWhiteSpace(normalizedDocument) && !IsValidCpfCnpj(normalizedDocument))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("CPF/CNPJ inválido."));
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

    var dto = cliente is null ? null : ToClienteLojaDto(cliente);

    return Results.Ok(ApiResponse<CadastroClienteStatusDto>.Ok(
        new CadastroClienteStatusDto(cliente is not null, dto),
        cliente is null ? "Cadastro disponível para criação." : "Cliente já cadastrado."));
})
.AllowAnonymous()
.WithName("VerificarCadastroCliente")
;

app.MapPost("/api/clientes", async (ClienteRequest request, IConfiguration configuration, INotificacaoService notificacaoService, NexumDbContext db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Nome))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Nome e email sao obrigatorios."));
    }

    var email = NormalizeEmail(request.Email)!;
    var cpfCnpj = NormalizeDocument(request.Cpf ?? request.CpfCnpj);
    if (!string.IsNullOrWhiteSpace(cpfCnpj) && !IsValidCpfCnpj(cpfCnpj))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("CPF/CNPJ inválido."));
    }
    var clienteExistente = await db.Clientes.FirstOrDefaultAsync(cliente =>
        cliente.Email == email ||
        (!string.IsNullOrWhiteSpace(cpfCnpj) &&
         ((cliente.CpfCnpj ?? string.Empty)
             .Replace(".", string.Empty)
             .Replace("-", string.Empty)
             .Replace("/", string.Empty)
             .Replace(" ", string.Empty)) == cpfCnpj), ct);

    if (clienteExistente is not null)
    {
        if (string.IsNullOrWhiteSpace(clienteExistente.SenhaHash) && !string.IsNullOrWhiteSpace(request.Senha))
        {
            clienteExistente.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha.Trim(), 12);
        }

        if (string.IsNullOrWhiteSpace(clienteExistente.Telefone) && !string.IsNullOrWhiteSpace(request.Telefone))
        {
            clienteExistente.Telefone = request.Telefone.Trim();
            clienteExistente.UpdatedAt = DateTime.UtcNow;
        }

        if (string.IsNullOrWhiteSpace(clienteExistente.Whatsapp) && !string.IsNullOrWhiteSpace(request.Whatsapp))
        {
            clienteExistente.Whatsapp = request.Whatsapp.Trim();
        }

        if (string.IsNullOrWhiteSpace(clienteExistente.RgIe) && !string.IsNullOrWhiteSpace(request.RgIe))
        {
            clienteExistente.RgIe = request.RgIe.Trim();
        }

        clienteExistente.DataNascimento ??= request.DataNascimento;
        clienteExistente.Avatar = string.IsNullOrWhiteSpace(request.Avatar) ? clienteExistente.Avatar : request.Avatar.Trim();
        clienteExistente.Newsletter = request.Newsletter ?? clienteExistente.Newsletter;
        clienteExistente.UpdatedAt = DateTime.UtcNow;

        if (clienteExistente.Status != StatusCliente.Ativo)
        {
            clienteExistente.TokenConfirmacaoEmail ??= Guid.NewGuid().ToString("N");
            clienteExistente.Status = StatusCliente.Pendente;
            var baseUrlExistente = configuration["PublicSite:BaseUrl"]?.TrimEnd('/') ?? "https://nexumaltivon.com.br";
            var linkExistente = $"{baseUrlExistente}/confirmar-cadastro.html?token={Uri.EscapeDataString(clienteExistente.TokenConfirmacaoEmail)}";
            await notificacaoService.EnviarConfirmacaoCadastroAsync(clienteExistente, linkExistente);
        }

        await db.SaveChangesAsync(ct);

        var existenteDto = ToClienteLojaDto(clienteExistente);
        return Results.Ok(ApiResponse<ClienteLojaDto>.Ok(existenteDto, clienteExistente.Status == StatusCliente.Ativo
            ? "Cliente ja cadastrado. Registro existente reutilizado."
            : "Cliente já cadastrado. Reenviamos o link de confirmação para liberar o acesso."));
    }

    var cliente = new Cliente
    {
        Nome = request.Nome.Trim(),
        Email = email,
        Telefone = request.Telefone,
        Whatsapp = string.IsNullOrWhiteSpace(request.Whatsapp) ? request.Telefone : request.Whatsapp.Trim(),
        CpfCnpj = cpfCnpj,
        RgIe = string.IsNullOrWhiteSpace(request.RgIe) ? null : request.RgIe.Trim(),
        DataNascimento = request.DataNascimento,
        Avatar = string.IsNullOrWhiteSpace(request.Avatar) ? null : request.Avatar.Trim(),
        Tipo = Enum.TryParse<TipoCliente>(request.Tipo, true, out var tipoCliente) ? tipoCliente : TipoCliente.PF,
        SenhaHash = !string.IsNullOrWhiteSpace(request.Senha) ? BCrypt.Net.BCrypt.HashPassword(request.Senha.Trim(), 12) : null,
        Newsletter = request.Newsletter ?? true,
        Status = StatusCliente.Pendente,
        TokenConfirmacaoEmail = Guid.NewGuid().ToString("N"),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.Clientes.Add(cliente);
    await db.SaveChangesAsync(ct);

    var baseUrl = configuration["PublicSite:BaseUrl"]?.TrimEnd('/') ?? "https://nexumaltivon.com.br";
    var linkConfirmacao = $"{baseUrl}/confirmar-cadastro.html?token={Uri.EscapeDataString(cliente.TokenConfirmacaoEmail ?? string.Empty)}";
    await notificacaoService.EnviarConfirmacaoCadastroAsync(cliente, linkConfirmacao);

    var dto = ToClienteLojaDto(cliente);
    return Results.Ok(ApiResponse<ClienteLojaDto>.Ok(dto, "Cliente registrado. Enviamos um link de confirmação por e-mail."));
})
.AllowAnonymous()
.WithName("CriarCliente")
;

app.MapPut("/api/clientes/{id:int}", [Authorize(Policy = "Gerente")] async (int id, ClienteRequest request, NexumDbContext db, CancellationToken ct) =>
{
    var cliente = await db.Clientes.FirstOrDefaultAsync(item => item.Id == id, ct);
    if (cliente is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Cliente nao encontrado."));
    }

    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Nome))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Nome e email sao obrigatorios."));
    }

    var email = NormalizeEmail(request.Email)!;
    var cpfCnpj = NormalizeDocument(request.Cpf ?? request.CpfCnpj);
    if (!string.IsNullOrWhiteSpace(cpfCnpj) && !IsValidCpfCnpj(cpfCnpj))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("CPF/CNPJ inválido."));
    }

    var duplicado = await db.Clientes.FirstOrDefaultAsync(item =>
        item.Id != cliente.Id &&
        (item.Email == email ||
         (!string.IsNullOrWhiteSpace(cpfCnpj) &&
          ((item.CpfCnpj ?? string.Empty)
              .Replace(".", string.Empty)
              .Replace("-", string.Empty)
              .Replace("/", string.Empty)
              .Replace(" ", string.Empty)) == cpfCnpj)), ct);

    if (duplicado is not null)
    {
        return Results.Conflict(ApiResponse<string>.Erro("Cliente ja cadastrado com este email ou CPF/CNPJ."));
    }

    cliente.Nome = request.Nome.Trim();
    cliente.Email = email;
    cliente.CpfCnpj = cpfCnpj;
    cliente.RgIe = string.IsNullOrWhiteSpace(request.RgIe) ? null : request.RgIe.Trim();
    cliente.DataNascimento = request.DataNascimento;
    cliente.Telefone = string.IsNullOrWhiteSpace(request.Telefone) ? null : request.Telefone.Trim();
    cliente.Whatsapp = string.IsNullOrWhiteSpace(request.Whatsapp) ? cliente.Telefone : request.Whatsapp.Trim();
    cliente.Avatar = string.IsNullOrWhiteSpace(request.Avatar) ? null : request.Avatar.Trim();
    cliente.Newsletter = request.Newsletter ?? cliente.Newsletter;
    cliente.Vip = request.Vip ?? cliente.Vip;
    cliente.PontosFidelidade = request.PontosFidelidade ?? cliente.PontosFidelidade;
    cliente.Tipo = Enum.TryParse<TipoCliente>(request.Tipo, true, out var tipo) ? tipo : cliente.Tipo;
    cliente.Status = Enum.TryParse<StatusCliente>(request.Status, true, out var status) ? status : cliente.Status;
    cliente.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync(ct);

    return Results.Ok(ApiResponse<ClienteLojaDto>.Ok(ToClienteLojaDto(cliente), "Cliente atualizado."));
})
.WithName("AtualizarCliente")
;

app.MapGet("/api/clientes/confirmar", async (string token, NexumDbContext db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(token))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Token de confirmação inválido."));
    }

    var cliente = await db.Clientes.FirstOrDefaultAsync(item => item.TokenConfirmacaoEmail == token, ct);
    if (cliente is null)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Token de confirmação não encontrado ou expirado."));
    }

    cliente.Status = StatusCliente.Ativo;
    cliente.ConfirmadoEm = DateTime.UtcNow;
    cliente.TokenConfirmacaoEmail = null;
    cliente.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync(ct);

    return Results.Ok(ApiResponse<ClienteLojaDto>.Ok(
        new ClienteLojaDto(cliente.Id, cliente.Nome, cliente.Email, cliente.Telefone, cliente.CpfCnpj),
        "Cadastro confirmado com sucesso. Agora você já pode entrar na área do cliente."));
})
.AllowAnonymous()
.WithName("ConfirmarCadastroCliente")
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

    var enderecos = await db.Enderecos
        .AsNoTracking()
        .Where(item => item.ClienteId == cliente.Id)
        .OrderByDescending(item => item.Padrao)
        .ThenByDescending(item => item.CreatedAt)
        .Select(item => new ClientePortalEnderecoDto(
            item.Id,
            item.Apelido,
            item.Tipo.ToString(),
            item.Cep,
            item.Logradouro,
            item.Numero,
            item.Complemento,
            item.Bairro,
            item.Cidade,
            item.Estado,
            item.Pais,
            item.Padrao))
        .ToListAsync(ct);

    var totalCompras = pedidos.Sum(item => item.Total);
    var score = cliente.Vip ? "Premium" : cliente.PontosFidelidade >= 500 ? "Gold" : cliente.PontosFidelidade >= 150 ? "Silver" : "Start";
    var portal = new ClientePortalDto(
        cliente.Id,
        cliente.Nome,
        cliente.Email,
        cliente.Telefone,
        cliente.CpfCnpj,
        cliente.Status.ToString(),
        cliente.ConfirmadoEm,
        cliente.PontosFidelidade,
        score,
        cliente.Vip,
        Math.Round(totalCompras / 10m, 2),
        pedidos,
        documentos,
        enderecos,
        [
            "Canal direto com o Grupo Nexum Altivon.",
            "Pontuação de fidelidade acumulada por compras aprovadas.",
            "Espaço preparado para limites e relacionamento futuro."
        ]);

    return Results.Ok(ApiResponse<ClientePortalDto>.Ok(portal));
})
.WithName("ClientePortalMe")
;

app.MapPost("/api/clientes/portal/enderecos", [Authorize] async (ClientePortalEnderecoRequest request, ClaimsPrincipal principal, NexumDbContext db, CancellationToken ct) =>
{
    var cliente = await GetClientePortalAsync(principal, db, ct);
    if (cliente is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Cliente nao localizado para esta sessao."));
    }

    var validation = ValidateEnderecoRequest(request);
    if (validation is not null)
    {
        return Results.BadRequest(ApiResponse<string>.Erro(validation));
    }

    var hasEndereco = await db.Enderecos.AnyAsync(item => item.ClienteId == cliente.Id, ct);
    var definirComoPadrao = request.Padrao || !hasEndereco;
    if (definirComoPadrao)
    {
        await db.Enderecos
            .Where(item => item.ClienteId == cliente.Id && item.Padrao)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.Padrao, false)
                .SetProperty(item => item.UpdatedAt, DateTime.UtcNow), ct);
    }

    var endereco = new Endereco
    {
        ClienteId = cliente.Id,
        Tipo = ParseTipoEndereco(request.Tipo),
        Apelido = string.IsNullOrWhiteSpace(request.Apelido) ? (definirComoPadrao ? "Principal" : "Auxiliar") : request.Apelido.Trim(),
        Cep = NormalizeDocument(request.Cep) ?? string.Empty,
        Logradouro = request.Logradouro!.Trim(),
        Numero = request.Numero!.Trim(),
        Complemento = TrimOrNull(request.Complemento),
        Bairro = TrimOrNull(request.Bairro),
        Cidade = TrimOrNull(request.Cidade),
        Estado = TrimOrNull(request.Estado)?.ToUpperInvariant(),
        Pais = string.IsNullOrWhiteSpace(request.Pais) ? "Brasil" : request.Pais.Trim(),
        Padrao = definirComoPadrao,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.Enderecos.Add(endereco);
    await db.SaveChangesAsync(ct);

    return Results.Ok(ApiResponse<ClientePortalEnderecoDto>.Ok(ToClientePortalEnderecoDto(endereco), "Endereco cadastrado."));
})
.WithName("ClientePortalCriarEndereco")
;

app.MapPut("/api/clientes/portal/enderecos/{id:int}", [Authorize] async (int id, ClientePortalEnderecoRequest request, ClaimsPrincipal principal, NexumDbContext db, CancellationToken ct) =>
{
    var cliente = await GetClientePortalAsync(principal, db, ct);
    if (cliente is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Cliente nao localizado para esta sessao."));
    }

    var endereco = await db.Enderecos.FirstOrDefaultAsync(item => item.Id == id && item.ClienteId == cliente.Id, ct);
    if (endereco is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Endereco nao localizado."));
    }

    var validation = ValidateEnderecoRequest(request);
    if (validation is not null)
    {
        return Results.BadRequest(ApiResponse<string>.Erro(validation));
    }

    if (request.Padrao && !endereco.Padrao)
    {
        await db.Enderecos
            .Where(item => item.ClienteId == cliente.Id && item.Id != endereco.Id && item.Padrao)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.Padrao, false)
                .SetProperty(item => item.UpdatedAt, DateTime.UtcNow), ct);
    }

    endereco.Tipo = ParseTipoEndereco(request.Tipo);
    endereco.Apelido = string.IsNullOrWhiteSpace(request.Apelido) ? endereco.Apelido : request.Apelido.Trim();
    endereco.Cep = NormalizeDocument(request.Cep) ?? string.Empty;
    endereco.Logradouro = request.Logradouro!.Trim();
    endereco.Numero = request.Numero!.Trim();
    endereco.Complemento = TrimOrNull(request.Complemento);
    endereco.Bairro = TrimOrNull(request.Bairro);
    endereco.Cidade = TrimOrNull(request.Cidade);
    endereco.Estado = TrimOrNull(request.Estado)?.ToUpperInvariant();
    endereco.Pais = string.IsNullOrWhiteSpace(request.Pais) ? "Brasil" : request.Pais.Trim();
    endereco.Padrao = request.Padrao || endereco.Padrao;
    endereco.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync(ct);

    return Results.Ok(ApiResponse<ClientePortalEnderecoDto>.Ok(ToClientePortalEnderecoDto(endereco), "Endereco atualizado."));
})
.WithName("ClientePortalAtualizarEndereco")
;

app.MapPut("/api/clientes/portal/enderecos/{id:int}/principal", [Authorize] async (int id, ClaimsPrincipal principal, NexumDbContext db, CancellationToken ct) =>
{
    var cliente = await GetClientePortalAsync(principal, db, ct);
    if (cliente is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Cliente nao localizado para esta sessao."));
    }

    var endereco = await db.Enderecos.FirstOrDefaultAsync(item => item.Id == id && item.ClienteId == cliente.Id, ct);
    if (endereco is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Endereco nao localizado."));
    }

    await db.Enderecos
        .Where(item => item.ClienteId == cliente.Id && item.Id != endereco.Id && item.Padrao)
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(item => item.Padrao, false)
            .SetProperty(item => item.UpdatedAt, DateTime.UtcNow), ct);

    endereco.Padrao = true;
    endereco.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync(ct);

    return Results.Ok(ApiResponse<ClientePortalEnderecoDto>.Ok(ToClientePortalEnderecoDto(endereco), "Endereco principal definido."));
})
.WithName("ClientePortalDefinirEnderecoPrincipal")
;

app.MapDelete("/api/clientes/portal/enderecos/{id:int}", [Authorize] async (int id, ClaimsPrincipal principal, NexumDbContext db, CancellationToken ct) =>
{
    var cliente = await GetClientePortalAsync(principal, db, ct);
    if (cliente is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Cliente nao localizado para esta sessao."));
    }

    var endereco = await db.Enderecos.FirstOrDefaultAsync(item => item.Id == id && item.ClienteId == cliente.Id, ct);
    if (endereco is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Endereco nao localizado."));
    }

    var eraPadrao = endereco.Padrao;
    db.Enderecos.Remove(endereco);
    await db.SaveChangesAsync(ct);

    if (eraPadrao)
    {
        var novoPadrao = await db.Enderecos
            .Where(item => item.ClienteId == cliente.Id)
            .OrderByDescending(item => item.UpdatedAt)
            .FirstOrDefaultAsync(ct);
        if (novoPadrao is not null)
        {
            novoPadrao.Padrao = true;
            novoPadrao.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }

    return Results.Ok(ApiResponse<string>.Ok("Endereco removido."));
})
.WithName("ClientePortalRemoverEndereco")
;

app.MapGet("/api/fornecedores", [Authorize(Policy = "Gerente")] async (NexumDbContext db, CancellationToken ct) =>
{
    var fornecedoresDb = await db.Fornecedores
        .AsNoTracking()
        .OrderByDescending(fornecedor => fornecedor.CreatedAt)
        .Take(500)
        .ToListAsync(ct);
    var fornecedores = fornecedoresDb.Select(ToFornecedorDto).ToList();

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
    if (!string.IsNullOrWhiteSpace(documento) && !IsValidCpfCnpj(documento))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("CNPJ/CPF do fornecedor inválido."));
    }
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
        NomeFantasia = string.IsNullOrWhiteSpace(request.NomeFantasia) ? request.Nome.Trim() : request.NomeFantasia.Trim(),
        Cnpj = documento,
        Ie = string.IsNullOrWhiteSpace(request.Ie) ? null : request.Ie.Trim(),
        Email = email,
        Telefone = string.IsNullOrWhiteSpace(request.Telefone) ? null : request.Telefone.Trim(),
        Whatsapp = string.IsNullOrWhiteSpace(request.Whatsapp) ? request.Telefone : request.Whatsapp.Trim(),
        Endereco = string.IsNullOrWhiteSpace(request.Endereco) ? null : request.Endereco.Trim(),
        Cidade = string.IsNullOrWhiteSpace(request.Cidade) ? null : request.Cidade.Trim(),
        Estado = string.IsNullOrWhiteSpace(request.Estado) ? null : request.Estado.Trim(),
        Cep = string.IsNullOrWhiteSpace(request.Cep) ? null : request.Cep.Trim(),
        Segmento = request.Categoria,
        LojaVinculadaId = request.LojaVinculadaId,
        ComissaoPercentual = request.ComissaoPercentual ?? 0.00m,
        PrazoEntregaDias = request.PrazoEntregaDias ?? 7,
        Status = Enum.TryParse<StatusFornecedor>(request.Status, true, out var statusFornecedor) ? statusFornecedor : StatusFornecedor.Ativo,
        Observacoes = string.IsNullOrWhiteSpace(request.Observacoes) ? null : request.Observacoes.Trim(),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.Fornecedores.Add(fornecedor);
    await db.SaveChangesAsync(ct);

    return Results.Ok(ApiResponse<FornecedorDto>.Ok(ToFornecedorDto(fornecedor), "Fornecedor cadastrado."));
})
.WithName("CriarFornecedor")
;

app.MapPut("/api/fornecedores/{id:int}", [Authorize(Policy = "Gerente")] async (int id, FornecedorRequest request, NexumDbContext db, CancellationToken ct) =>
{
    var fornecedor = await db.Fornecedores.FirstOrDefaultAsync(item => item.Id == id, ct);
    if (fornecedor is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Fornecedor nao encontrado."));
    }

    if (string.IsNullOrWhiteSpace(request.Nome))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Nome do fornecedor obrigatorio."));
    }

    var documento = string.IsNullOrWhiteSpace(request.Documento) ? null : request.Documento.Trim();
    if (!string.IsNullOrWhiteSpace(documento) && !IsValidCpfCnpj(documento))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("CNPJ/CPF do fornecedor inválido."));
    }
    var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();
    var fornecedorExistente = await db.Fornecedores.FirstOrDefaultAsync(item =>
        item.Id != fornecedor.Id &&
        ((!string.IsNullOrWhiteSpace(documento) && item.Cnpj == documento) ||
         (!string.IsNullOrWhiteSpace(email) && item.Email != null && item.Email.ToLower() == email)), ct);

    if (fornecedorExistente is not null)
    {
        return Results.Conflict(ApiResponse<string>.Erro("Fornecedor ja cadastrado com este documento ou e-mail."));
    }

    fornecedor.RazaoSocial = request.Nome.Trim();
    fornecedor.NomeFantasia = string.IsNullOrWhiteSpace(request.NomeFantasia) ? request.Nome.Trim() : request.NomeFantasia.Trim();
    fornecedor.Cnpj = documento;
    fornecedor.Ie = string.IsNullOrWhiteSpace(request.Ie) ? null : request.Ie.Trim();
    fornecedor.Email = email;
    fornecedor.Telefone = string.IsNullOrWhiteSpace(request.Telefone) ? null : request.Telefone.Trim();
    fornecedor.Whatsapp = string.IsNullOrWhiteSpace(request.Whatsapp) ? fornecedor.Telefone : request.Whatsapp.Trim();
    fornecedor.Endereco = string.IsNullOrWhiteSpace(request.Endereco) ? null : request.Endereco.Trim();
    fornecedor.Cidade = string.IsNullOrWhiteSpace(request.Cidade) ? null : request.Cidade.Trim();
    fornecedor.Estado = string.IsNullOrWhiteSpace(request.Estado) ? null : request.Estado.Trim();
    fornecedor.Cep = string.IsNullOrWhiteSpace(request.Cep) ? null : request.Cep.Trim();
    fornecedor.Segmento = request.Categoria;
    fornecedor.LojaVinculadaId = request.LojaVinculadaId;
    fornecedor.ComissaoPercentual = request.ComissaoPercentual ?? fornecedor.ComissaoPercentual;
    fornecedor.PrazoEntregaDias = request.PrazoEntregaDias ?? fornecedor.PrazoEntregaDias;
    fornecedor.Status = Enum.TryParse<StatusFornecedor>(request.Status, true, out var status) ? status : fornecedor.Status;
    fornecedor.Observacoes = string.IsNullOrWhiteSpace(request.Observacoes) ? null : request.Observacoes.Trim();
    fornecedor.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync(ct);

    return Results.Ok(ApiResponse<FornecedorDto>.Ok(ToFornecedorDto(fornecedor), "Fornecedor atualizado."));
})
.WithName("AtualizarFornecedor")
;

app.MapGet("/api/compras/painel", [Authorize(Policy = "Gerente")] async (NexumDbContext db, CancellationToken ct) =>
{
    var painel = await BuildComprasPainelAsync(db, ct);
    return Results.Ok(ApiResponse<ComprasPainelDto>.Ok(painel, "Fluxo de compras e entradas carregado."));
})
.WithName("ComprasPainel")
;

app.MapPost("/api/compras/solicitacoes", [Authorize(Policy = "Gerente")] async (CompraSolicitacaoRequest request, NexumDbContext db, CancellationToken ct) =>
{
    if (request.Quantidade <= 0)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Quantidade solicitada deve ser maior que zero."));
    }

    Produto? produto = null;
    if (request.ProdutoId.HasValue)
    {
        produto = await db.Produtos.FirstOrDefaultAsync(item => item.Id == request.ProdutoId.Value, ct);
        if (produto is null)
        {
            return Results.NotFound(ApiResponse<string>.Erro("Produto nao encontrado."));
        }
    }

    var produtoNome = produto?.Nome ?? TrimOrNull(request.ProdutoNome);
    if (string.IsNullOrWhiteSpace(produtoNome))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Informe o produto ou a descricao do item solicitado."));
    }

    var now = DateTime.UtcNow;
    var origem = NormalizeCompraOrigem(request.Origem);
    var finalidade = TrimOrNull(request.Finalidade) ?? "Reposicao/operacao";
    var prioridade = TrimOrNull(request.Prioridade) ?? "Normal";
    var observacoes = TrimOrNull(request.Observacoes);

    await db.Database.ExecuteSqlInterpolatedAsync($"""
        INSERT INTO compras_solicitacoes
            (produto_id, produto_nome, quantidade_solicitada, finalidade, origem, status, prioridade, observacoes, created_at, updated_at)
        VALUES
            ({produto?.Id}, {produtoNome}, {request.Quantidade}, {finalidade}, {origem}, {"Aberta"}, {prioridade}, {observacoes}, {now}, {now});
        """, ct);

    var solicitacaoId = await ExecuteScalarAsync<int>(db, "SELECT LAST_INSERT_ID();", ct);
    var painel = await BuildComprasPainelAsync(db, ct);
    return Results.Created($"/api/compras/solicitacoes/{solicitacaoId}", ApiResponse<ComprasPainelDto>.Ok(painel, "Solicitacao de compra registrada para cotacao."));
})
.WithName("RegistrarCompraSolicitacao")
;

app.MapPost("/api/compras/cotacoes", [Authorize(Policy = "Gerente")] async (CompraCotacaoRequest request, NexumDbContext db, CancellationToken ct) =>
{
    if (request.FornecedorId <= 0)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Fornecedor obrigatorio para cotacao."));
    }

    if (request.Quantidade <= 0 || request.CustoUnitario <= 0)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Quantidade e custo unitario devem ser maiores que zero."));
    }

    var fornecedor = await db.Fornecedores.AsNoTracking().FirstOrDefaultAsync(item => item.Id == request.FornecedorId, ct);
    if (fornecedor is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Fornecedor nao encontrado."));
    }

    Produto? produto = null;
    if (request.ProdutoId.HasValue)
    {
        produto = await db.Produtos.FirstOrDefaultAsync(item => item.Id == request.ProdutoId.Value, ct);
        if (produto is null)
        {
            return Results.NotFound(ApiResponse<string>.Erro("Produto nao encontrado."));
        }
    }

    var produtoNome = produto?.Nome ?? TrimOrNull(request.ProdutoNome);
    if (string.IsNullOrWhiteSpace(produtoNome))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Informe o produto ou a descricao do item cotado."));
    }

    var now = DateTime.UtcNow;
    var total = request.Quantidade * request.CustoUnitario;
    var origem = NormalizeCompraOrigem(request.Origem);
    var finalidade = TrimOrNull(request.Finalidade) ?? "Reposicao/operacao";
    int? produtoIdCotacao = produto?.Id;
    var prioridade = TrimOrNull(request.Prioridade) ?? "Normal";
    var observacoesCotacao = TrimOrNull(request.Observacoes);
    var prazoEntrega = request.PrazoEntregaDias ?? fornecedor.PrazoEntregaDias;

    await db.Database.ExecuteSqlInterpolatedAsync($"""
        INSERT INTO compras_solicitacoes
            (produto_id, produto_nome, quantidade_solicitada, finalidade, origem, status, prioridade, observacoes, created_at, updated_at)
        VALUES
            ({produtoIdCotacao}, {produtoNome}, {request.Quantidade}, {finalidade}, {origem}, {"Cotado"}, {prioridade}, {observacoesCotacao}, {now}, {now});
        """, ct);

    var solicitacaoId = await ExecuteScalarAsync<int>(db, "SELECT LAST_INSERT_ID();", ct);

    await db.Database.ExecuteSqlInterpolatedAsync($"""
        INSERT INTO compras_cotacoes
            (solicitacao_id, fornecedor_id, produto_id, produto_nome, quantidade, custo_unitario, valor_total, prazo_entrega_dias, origem, status, observacoes, created_at, updated_at)
        VALUES
            ({solicitacaoId}, {request.FornecedorId}, {produtoIdCotacao}, {produtoNome}, {request.Quantidade}, {request.CustoUnitario}, {total}, {prazoEntrega}, {origem}, {"Selecionada"}, {observacoesCotacao}, {now}, {now});
        """, ct);

    var painel = await BuildComprasPainelAsync(db, ct);
    return Results.Created($"/api/compras/cotacoes/{solicitacaoId}", ApiResponse<ComprasPainelDto>.Ok(painel, "Cotacao registrada e disponivel para pedido de compra."));
})
.WithName("RegistrarCompraCotacao")
;

app.MapPost("/api/compras/pedidos", [Authorize(Policy = "Gerente")] async (CompraPedidoRequest request, NexumDbContext db, IServiceProvider services, CancellationToken ct) =>
{
    if (request.FornecedorId <= 0)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Fornecedor obrigatorio para pedido de compra."));
    }

    if (request.Itens is null || request.Itens.Count == 0)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Inclua ao menos um item no pedido de compra."));
    }

    if (request.Itens.Any(item => item.Quantidade <= 0 || item.CustoUnitario <= 0))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Todos os itens devem ter quantidade e custo validos."));
    }

    var fornecedor = await db.Fornecedores.AsNoTracking().FirstOrDefaultAsync(item => item.Id == request.FornecedorId, ct);
    if (fornecedor is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Fornecedor nao encontrado."));
    }

    var produtoIds = request.Itens
        .Where(item => item.ProdutoId.HasValue)
        .Select(item => item.ProdutoId!.Value)
        .Distinct()
        .ToList();
    var produtos = await db.Produtos
        .Where(item => produtoIds.Contains(item.Id))
        .ToDictionaryAsync(item => item.Id, ct);

    if (produtoIds.Count != produtos.Count)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Um ou mais produtos informados nao existem."));
    }

    var now = DateTime.UtcNow;
    var numeroPedido = $"COMP-{now:yyyyMMddHHmmss}";
    var origem = NormalizeCompraOrigem(request.Origem);
    var finalidade = TrimOrNull(request.Finalidade) ?? "Reposicao/operacao";
    var vencimento = request.DataVencimento ?? now.Date.AddDays(7);
    var total = request.Itens.Sum(item => item.Quantidade * item.CustoUnitario);

    await using var transaction = await db.Database.BeginTransactionAsync(ct);

    await db.Database.ExecuteSqlInterpolatedAsync($"""
        INSERT INTO compras_pedidos
            (numero, fornecedor_id, solicitacao_id, origem, finalidade, status, status_fiscal, valor_total, data_prevista_entrega, observacoes, created_at, updated_at)
        VALUES
            ({numeroPedido}, {request.FornecedorId}, {request.SolicitacaoId}, {origem}, {finalidade}, {"Aberto"}, {"Pendente"}, {total}, {request.DataPrevistaEntrega}, {TrimOrNull(request.Observacoes)}, {now}, {now});
        """, ct);

    var compraPedidoId = await ExecuteScalarAsync<int>(db, "SELECT LAST_INSERT_ID();", ct);

    foreach (var item in request.Itens)
    {
        var produto = item.ProdutoId.HasValue ? produtos[item.ProdutoId.Value] : null;
        var produtoNome = produto?.Nome ?? TrimOrNull(item.ProdutoNome);
        int? itemProdutoId = produto?.Id;
        var itemSku = produto?.Sku ?? TrimOrNull(item.Sku);
        if (string.IsNullOrWhiteSpace(produtoNome))
        {
            await transaction.RollbackAsync(ct);
            return Results.BadRequest(ApiResponse<string>.Erro("Todo item sem produto vinculado precisa de descricao."));
        }

        var itemTotal = item.Quantidade * item.CustoUnitario;
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO compras_pedido_itens
                (compra_pedido_id, produto_id, produto_nome, sku, quantidade, quantidade_recebida, custo_unitario, valor_total, origem, finalidade, created_at, updated_at)
            VALUES
                ({compraPedidoId}, {itemProdutoId}, {produtoNome}, {itemSku}, {item.Quantidade}, 0, {item.CustoUnitario}, {itemTotal}, {origem}, {finalidade}, {now}, {now});
            """, ct);
    }

    db.Financeiros.Add(new Financeiro
    {
        Tipo = TipoLancamento.Despesa,
        Categoria = "Compras de mercadorias",
        Descricao = $"Pedido de compra {numeroPedido} - {fornecedor.RazaoSocial}",
        Valor = total,
        DataVencimento = vencimento,
        Status = StatusLancamento.Pendente,
        MeioPagamento = TrimOrNull(request.MeioPagamento) ?? "A definir",
        Observacoes = $"CompraId={compraPedidoId}; origem={origem}; finalidade={finalidade}",
        CreatedAt = now,
        UpdatedAt = now
    });

    await db.SaveChangesAsync(ct);
    await TryCreateGenesisContaPagarCompraAsync(services, numeroPedido, fornecedor.Id, fornecedor.RazaoSocial, total, now, vencimento, request.MeioPagamento, ct);
    await transaction.CommitAsync(ct);

    var painel = await BuildComprasPainelAsync(db, ct);
    return Results.Created($"/api/compras/pedidos/{compraPedidoId}", ApiResponse<ComprasPainelDto>.Ok(painel, "Pedido de compra gerado com conta a pagar."));
})
.WithName("CriarCompraPedido")
;

app.MapPost("/api/compras/pedidos/{id:int}/entradas", [Authorize(Policy = "Gerente")] async (int id, CompraEntradaRequest request, NexumDbContext db, CancellationToken ct) =>
{
    var pedido = await db.Database.SqlQueryRaw<CompraPedidoLookupRow>(
        "SELECT id AS Id, numero AS Numero, status AS Status, origem AS Origem, fornecedor_id AS FornecedorId FROM compras_pedidos WHERE id = {0} LIMIT 1",
        id)
        .FirstOrDefaultAsync(ct);

    if (pedido is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Pedido de compra nao encontrado."));
    }

    var itens = await db.Database.SqlQueryRaw<CompraPedidoItemLookupRow>(
        "SELECT id AS Id, produto_id AS ProdutoId, produto_nome AS ProdutoNome, quantidade AS Quantidade, quantidade_recebida AS QuantidadeRecebida, custo_unitario AS CustoUnitario FROM compras_pedido_itens WHERE compra_pedido_id = {0}",
        id)
        .ToListAsync(ct);

    if (itens.Count == 0)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Pedido de compra sem itens para entrada."));
    }

    var quantidades = request.Itens?.ToDictionary(item => item.ItemId, item => item.QuantidadeRecebida) ?? [];
    var now = DateTime.UtcNow;
    var documento = TrimOrNull(request.NumeroDocumento);
    var chaveNfe = TrimOrNull(request.ChaveNfeEntrada);
    var totalEntrada = 0m;

    await using var transaction = await db.Database.BeginTransactionAsync(ct);

    await db.Database.ExecuteSqlInterpolatedAsync($"""
        INSERT INTO compras_entradas
            (compra_pedido_id, fornecedor_id, numero_documento, chave_nfe_entrada, tipo_entrada, status_fiscal, valor_total, recebido_por, observacoes, created_at, updated_at)
        VALUES
            ({id}, {pedido.FornecedorId}, {documento}, {chaveNfe}, {NormalizeCompraOrigem(request.TipoEntrada ?? pedido.Origem)}, {(!string.IsNullOrWhiteSpace(chaveNfe) ? "NFeInformada" : "FiscalPendente")}, 0, {TrimOrNull(request.RecebidoPor) ?? "Operacao"}, {TrimOrNull(request.Observacoes)}, {now}, {now});
        """, ct);

    var entradaId = await ExecuteScalarAsync<int>(db, "SELECT LAST_INSERT_ID();", ct);
    var todosRecebidos = true;

    foreach (var item in itens)
    {
        var pendente = Math.Max(0, item.Quantidade - item.QuantidadeRecebida);
        var recebidaAgora = quantidades.TryGetValue(item.Id, out var quantidadeInformada)
            ? quantidadeInformada
            : pendente;
        recebidaAgora = Math.Min(Math.Max(0, recebidaAgora), pendente);

        if (recebidaAgora <= 0)
        {
            if (pendente > 0)
            {
                todosRecebidos = false;
            }
            continue;
        }

        var valorItem = recebidaAgora * item.CustoUnitario;
        totalEntrada += valorItem;
        var novaQuantidadeRecebida = item.QuantidadeRecebida + recebidaAgora;
        if (novaQuantidadeRecebida < item.Quantidade)
        {
            todosRecebidos = false;
        }

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO compras_entrada_itens
                (compra_entrada_id, compra_pedido_item_id, produto_id, produto_nome, quantidade_recebida, custo_unitario, valor_total, created_at, updated_at)
            VALUES
                ({entradaId}, {item.Id}, {item.ProdutoId}, {item.ProdutoNome}, {recebidaAgora}, {item.CustoUnitario}, {valorItem}, {now}, {now});
            """, ct);

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE compras_pedido_itens
            SET quantidade_recebida = {novaQuantidadeRecebida}, updated_at = {now}
            WHERE id = {item.Id};
            """, ct);

        if (item.ProdutoId.HasValue)
        {
            var produto = await db.Produtos.FirstOrDefaultAsync(produto => produto.Id == item.ProdutoId.Value, ct);
            if (produto is not null)
            {
                produto.EstoqueAtual += recebidaAgora;
                produto.Custo = item.CustoUnitario;
                produto.CodigoBarras ??= GerarCodigoBarrasProduto(produto);
                produto.QrCode = GerarQrCodeProduto(produto, pedido, entradaId, documento ?? chaveNfe);
                produto.IdentificacaoEstoque = GerarIdentificacaoEstoqueProduto(produto, pedido, entradaId, recebidaAgora, item.CustoUnitario, documento ?? chaveNfe);
                produto.UpdatedAt = now;

                await db.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO estoque_movimentos
                        (produto_id, compra_entrada_id, tipo, quantidade, saldo_resultante, custo_unitario, origem, documento, observacoes, created_at)
                    VALUES
                        ({produto.Id}, {entradaId}, {"EntradaCompra"}, {recebidaAgora}, {produto.EstoqueAtual}, {item.CustoUnitario}, {pedido.Origem}, {documento ?? chaveNfe}, {TrimOrNull(request.Observacoes)}, {now});
                    """, ct);
            }
        }
    }

    if (totalEntrada <= 0)
    {
        await transaction.RollbackAsync(ct);
        return Results.BadRequest(ApiResponse<string>.Erro("Nenhuma quantidade pendente foi recebida."));
    }

    await db.Database.ExecuteSqlInterpolatedAsync($"""
        UPDATE compras_entradas
        SET valor_total = {totalEntrada}, updated_at = {now}
        WHERE id = {entradaId};
        """, ct);

    await db.Database.ExecuteSqlInterpolatedAsync($"""
        UPDATE compras_pedidos
        SET status = {(todosRecebidos ? "Recebido" : "RecebidoParcial")},
            status_fiscal = {(!string.IsNullOrWhiteSpace(chaveNfe) ? "NFeEntradaInformada" : "FiscalPendente")},
            updated_at = {now}
        WHERE id = {id};
        """, ct);

    await db.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);

    var painel = await BuildComprasPainelAsync(db, ct);
    return Results.Ok(ApiResponse<ComprasPainelDto>.Ok(painel, "Entrada registrada, estoque atualizado e fiscal sinalizado."));
})
.WithName("RegistrarCompraEntrada")
;

app.MapPatch("/api/compras/solicitacoes/{id:int}/status", [Authorize(Policy = "Gerente")] async (int id, CompraStatusRequest request, NexumDbContext db, CancellationToken ct) =>
{
    var novoStatus = NormalizeCompraSolicitacaoStatus(request.Status ?? request.NovoStatus);
    if (novoStatus is null)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Status de solicitacao invalido."));
    }

    var atual = await db.Database.SqlQueryRaw<string>(
        "SELECT status AS Value FROM compras_solicitacoes WHERE id = {0} LIMIT 1",
        id)
        .FirstOrDefaultAsync(ct);

    if (string.IsNullOrWhiteSpace(atual))
    {
        return Results.NotFound(ApiResponse<string>.Erro("Solicitacao de compra nao encontrada."));
    }

    var now = DateTime.UtcNow;
    var observacao = TrimOrNull(request.Observacoes);
    if (observacao is null)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE compras_solicitacoes
            SET status = {novoStatus}, updated_at = {now}
            WHERE id = {id};
            """, ct);
    }
    else
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE compras_solicitacoes
            SET status = {novoStatus},
                observacoes = CONCAT_WS('\n', NULLIF(observacoes, ''), {observacao}),
                updated_at = {now}
            WHERE id = {id};
            """, ct);
    }

    var painel = await BuildComprasPainelAsync(db, ct);
    return Results.Ok(ApiResponse<ComprasPainelDto>.Ok(painel, "Status da solicitacao atualizado."));
})
.WithName("AtualizarCompraSolicitacaoStatus")
;

app.MapPatch("/api/compras/pedidos/{id:int}/status", [Authorize(Policy = "Gerente")] async (int id, CompraStatusRequest request, NexumDbContext db, CancellationToken ct) =>
{
    var novoStatus = NormalizeCompraPedidoStatus(request.Status ?? request.NovoStatus);
    if (novoStatus is null)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Status de pedido de compra invalido."));
    }

    var atual = await db.Database.SqlQueryRaw<string>(
        "SELECT status AS Value FROM compras_pedidos WHERE id = {0} LIMIT 1",
        id)
        .FirstOrDefaultAsync(ct);

    if (string.IsNullOrWhiteSpace(atual))
    {
        return Results.NotFound(ApiResponse<string>.Erro("Pedido de compra nao encontrado."));
    }

    if (atual.Equals("Recebido", StringComparison.OrdinalIgnoreCase) && novoStatus == "Cancelado")
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Pedido ja recebido nao pode ser cancelado."));
    }

    var now = DateTime.UtcNow;
    var observacao = TrimOrNull(request.Observacoes);
    if (observacao is null)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE compras_pedidos
            SET status = {novoStatus}, updated_at = {now}
            WHERE id = {id};
            """, ct);
    }
    else
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE compras_pedidos
            SET status = {novoStatus},
                observacoes = CONCAT_WS('\n', NULLIF(observacoes, ''), {observacao}),
                updated_at = {now}
            WHERE id = {id};
            """, ct);
    }

    var painel = await BuildComprasPainelAsync(db, ct);
    return Results.Ok(ApiResponse<ComprasPainelDto>.Ok(painel, "Status do pedido de compra atualizado."));
})
.WithName("AtualizarCompraPedidoStatus")
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
        pedido.FreteCodigoRastreio,
        BuildPedidoInstruction(pedido.StatusPagamento, pedido.MeioPagamento, pedido.GatewayTransacaoId),
        pagamentoAtual?.Parcelas ?? 1,
        pagamentoAtual?.PixQrcode,
        pagamentoAtual?.BoletoUrl);
    }).ToList();

    return Results.Ok(ApiResponse<List<PedidoLojaDto>>.Ok(dtos));
})
.WithName("Pedidos")
;

app.MapGet("/api/financeiro/lancamentos", [Authorize(Policy = "Financeiro")] async (
    int? pedidoId,
    string? tipo,
    string? status,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var query = db.Financeiros
        .AsNoTracking()
        .Include(item => item.Pedido)
        .AsQueryable();

    if (pedidoId is > 0)
    {
        query = query.Where(item => item.PedidoId == pedidoId.Value);
    }

    if (!string.IsNullOrWhiteSpace(tipo) && Enum.TryParse<TipoLancamento>(tipo, true, out var tipoLancamento))
    {
        query = query.Where(item => item.Tipo == tipoLancamento);
    }

    if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<StatusLancamento>(status, true, out var statusLancamento))
    {
        query = query.Where(item => item.Status == statusLancamento);
    }

    var lancamentos = await query
        .OrderByDescending(item => item.CreatedAt)
        .Take(500)
        .Select(item => new FinanceiroLancamentoDto(
            item.Id,
            item.PedidoId,
            item.Pedido != null ? item.Pedido.NumeroPedido : null,
            item.Tipo.ToString(),
            item.Status.ToString(),
            item.Categoria,
            item.Descricao,
            item.Valor,
            item.DataVencimento,
            item.DataPagamento,
            item.MeioPagamento,
            item.ContaBancaria,
            item.Observacoes,
            item.CreatedAt))
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<FinanceiroLancamentoDto>>.Ok(lancamentos));
})
.WithName("FinanceiroLancamentos")
;

app.MapPost("/api/financeiro/lancamentos", [Authorize(Policy = "Financeiro")] async (
    FinanceiroLancamentoRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (!Enum.TryParse<TipoLancamento>(request.Tipo, true, out var tipo))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Tipo de lancamento financeiro invalido."));
    }

    if (request.Valor <= 0)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Informe um valor financeiro maior que zero."));
    }

    var status = StatusLancamento.Pendente;
    if (!string.IsNullOrWhiteSpace(request.Status) &&
        !Enum.TryParse<StatusLancamento>(request.Status, true, out status))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Status financeiro invalido."));
    }

    var pedidoExiste = request.PedidoId is null || await db.Pedidos.AnyAsync(item => item.Id == request.PedidoId.Value, ct);
    if (!pedidoExiste)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Pedido vinculado ao financeiro nao foi encontrado."));
    }

    var now = DateTime.UtcNow;
    var lancamento = new Financeiro
    {
        PedidoId = request.PedidoId,
        Tipo = tipo,
        Status = status,
        Categoria = Truncate(request.Categoria, 100),
        Descricao = Truncate(request.Descricao, 255),
        Valor = request.Valor,
        DataVencimento = request.DataVencimento,
        DataPagamento = status == StatusLancamento.Pago ? request.DataPagamento ?? now : request.DataPagamento,
        MeioPagamento = Truncate(request.MeioPagamento, 50),
        ContaBancaria = Truncate(request.ContaBancaria, 100),
        ComprovanteUrl = Truncate(request.ComprovanteUrl, 255),
        Observacoes = request.Observacoes,
        CreatedAt = now,
        UpdatedAt = now
    };

    db.Financeiros.Add(lancamento);
    await db.SaveChangesAsync(ct);

    var dto = new FinanceiroLancamentoDto(
        lancamento.Id,
        lancamento.PedidoId,
        null,
        lancamento.Tipo.ToString(),
        lancamento.Status.ToString(),
        lancamento.Categoria,
        lancamento.Descricao,
        lancamento.Valor,
        lancamento.DataVencimento,
        lancamento.DataPagamento,
        lancamento.MeioPagamento,
        lancamento.ContaBancaria,
        lancamento.Observacoes,
        lancamento.CreatedAt);

    return Results.Created($"/api/financeiro/lancamentos/{lancamento.Id}", ApiResponse<FinanceiroLancamentoDto>.Ok(dto, "Lancamento financeiro registrado."));
})
.WithName("FinanceiroCriarLancamento")
;

app.MapPatch("/api/financeiro/lancamentos/{id:int}/status", [Authorize(Policy = "Financeiro")] async (
    int id,
    FinanceiroLancamentoStatusRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var lancamento = await db.Financeiros.FirstOrDefaultAsync(item => item.Id == id, ct);
    if (lancamento is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Lancamento financeiro nao encontrado."));
    }

    if (!Enum.TryParse<StatusLancamento>(request.Status, true, out var status))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Status financeiro invalido."));
    }

    lancamento.Status = status;
    lancamento.UpdatedAt = DateTime.UtcNow;

    if (status == StatusLancamento.Pago)
    {
        lancamento.DataPagamento = request.DataPagamento ?? DateTime.UtcNow;
        lancamento.MeioPagamento = Truncate(request.MeioPagamento, 50) ?? lancamento.MeioPagamento;
        lancamento.ContaBancaria = Truncate(request.ContaBancaria, 100) ?? lancamento.ContaBancaria;
        lancamento.ComprovanteUrl = Truncate(request.ComprovanteUrl, 255) ?? lancamento.ComprovanteUrl;
    }

    if (!string.IsNullOrWhiteSpace(request.Observacoes))
    {
        lancamento.Observacoes = string.IsNullOrWhiteSpace(lancamento.Observacoes)
            ? request.Observacoes
            : $"{lancamento.Observacoes}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm} - {request.Observacoes}";
    }

    await db.SaveChangesAsync(ct);

    return Results.Ok(ApiResponse<string>.Ok("Status financeiro atualizado."));
})
.WithName("FinanceiroAtualizarStatusLancamento")
;

app.MapGet("/api/pedidos/acompanhar", async (
    string numero,
    string? email,
    string? documento,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var numeroPedido = (numero ?? string.Empty).Trim();
    var emailCliente = NormalizeEmail(email);
    var documentoCliente = NormalizeDocument(documento);

    if (string.IsNullOrWhiteSpace(numeroPedido)
        || (string.IsNullOrWhiteSpace(emailCliente) && string.IsNullOrWhiteSpace(documentoCliente)))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Informe o numero do pedido e o e-mail ou documento do cliente."));
    }

    var pedido = await db.Pedidos
        .Include(item => item.Cliente)
        .Include(item => item.Pagamentos)
        .AsNoTracking()
        .FirstOrDefaultAsync(item => item.NumeroPedido == numeroPedido, ct);

    if (pedido?.Cliente is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Pedido nao encontrado para os dados informados."));
    }

    var emailConfere = !string.IsNullOrWhiteSpace(emailCliente)
        && string.Equals(pedido.Cliente.Email, emailCliente, StringComparison.OrdinalIgnoreCase);
    var documentoPedido = NormalizeDocument(pedido.Cliente.CpfCnpj);
    var documentoConfere = !string.IsNullOrWhiteSpace(documentoCliente)
        && !string.IsNullOrWhiteSpace(documentoPedido)
        && documentoPedido == documentoCliente;

    if (!emailConfere && !documentoConfere)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Pedido nao encontrado para os dados informados."));
    }

    var pagamentoAtual = pedido.Pagamentos?
        .OrderByDescending(item => item.CreatedAt)
        .FirstOrDefault();

    var dto = new PedidoAcompanhamentoDto(
        pedido.Id,
        pedido.NumeroPedido,
        pedido.Cliente.Nome,
        pedido.Total,
        FormatStatusPedido(pedido.Status),
        FormatStatusPagamento(pedido.StatusPagamento),
        pedido.MeioPagamento,
        pedido.GatewayPagamento,
        pedido.FreteMetodo,
        pedido.FreteTransportadora,
        pedido.FretePrazoDias,
        pedido.FreteCodigoRastreio,
        BuildPedidoInstruction(pedido.StatusPagamento, pedido.MeioPagamento, pedido.GatewayTransacaoId),
        pagamentoAtual?.BoletoUrl,
        pedido.CreatedAt,
        pedido.UpdatedAt);

    return Results.Ok(ApiResponse<PedidoAcompanhamentoDto>.Ok(dto));
})
.AllowAnonymous()
.WithName("AcompanharPedido")
;

app.MapPut("/api/pedidos/{id}/status", [Authorize(Policy = "Gerente")] async (
    int id,
    StatusUpdateRequest request,
    NexumDbContext db,
    INotificacaoService notificacaoService,
    CancellationToken ct) =>
{
    await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
    var pedido = await db.Pedidos
        .Include(item => item.Itens)
        .Include(item => item.Pagamentos)
        .Include(item => item.Cliente)
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
    var statusPagamentoAnterior = pedido.StatusPagamento;
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

    if (pedido.Cliente is not null && statusAnterior != pedido.Status)
    {
        if (statusPagamentoAnterior != pedido.StatusPagamento && pedido.StatusPagamento == StatusPagamento.Aprovado)
        {
            await notificacaoService.EnviarConfirmacaoPagamentoAsync(pedido.Cliente, pedido);
        }

        if (status is StatusPedido.EmSeparacao or StatusPedido.Enviado or StatusPedido.Entregue or StatusPedido.Cancelado or StatusPedido.Devolvido or StatusPedido.Reembolsado)
        {
            var mensagem = BuildMensagemAtualizacaoPedido(pedido);
            await notificacaoService.EnviarStatusPedidoAsync(pedido.Cliente, pedido, mensagem);
        }
    }

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
        pedido.FreteCodigoRastreio,
        BuildPedidoInstruction(pedido.StatusPagamento, pedido.MeioPagamento, pedido.GatewayTransacaoId),
        pagamentoAtual?.Parcelas ?? 1,
        pagamentoAtual?.PixQrcode,
        pagamentoAtual?.BoletoUrl);
    return Results.Ok(ApiResponse<PedidoLojaDto>.Ok(dto, "Status do pedido atualizado."));
})
.WithName("AtualizarStatusPedido")
;

app.MapPost("/api/pedidos/{id}/fluxo-operacional", [Authorize(Policy = "Gerente")] async (
    int id,
    NexumDbContext db,
    IFiscalRoutingEngine fiscalRoutingEngine,
    INotificacaoService notificacaoService,
    CancellationToken ct) =>
{
    await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
    var pedido = await db.Pedidos
        .Include(item => item.Itens)
        .Include(item => item.Pagamentos)
        .Include(item => item.Cliente)
        .Include(item => item.EnderecoEntrega)
        .FirstOrDefaultAsync(item => item.Id == id, ct);

    if (pedido is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Pedido nao encontrado."));
    }

    if (!TryGetNextOperationalStatus(pedido.Status, out var proximoStatus))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Pedido ja esta em etapa final ou nao permite avanco automatico."));
    }

    var statusAnterior = pedido.Status;
    var statusPagamentoAnterior = pedido.StatusPagamento;
    var erroEstoque = await ApplyPedidoStatusTransitionAsync(pedido, proximoStatus, db, ct);
    if (!string.IsNullOrWhiteSpace(erroEstoque))
    {
        return Results.BadRequest(ApiResponse<string>.Erro(erroEstoque));
    }

    await SyncFinanceiroPedidoOperacionalAsync(pedido, db, ct);

    if (pedido.Cliente is not null)
    {
        await EnsurePedidoFiscalAutomationAsync(
            pedido,
            pedido.Cliente,
            BuildPedidoRequestFromPedido(pedido),
            db,
            fiscalRoutingEngine,
            ct);
    }

    var fiscal = await db.Fiscais.FirstOrDefaultAsync(item => item.PedidoId == pedido.Id, ct);
    if (fiscal is not null)
    {
        fiscal.StatusAutomacao = proximoStatus switch
        {
            StatusPedido.Pago => "Pagamento confirmado; pré-emissão fiscal pronta para conferência.",
            StatusPedido.EmSeparacao => "Pedido em separação; NF-e pendente de emissão/autorização.",
            StatusPedido.Enviado => "Pedido enviado; conferir emissão/autorização fiscal antes da baixa final.",
            StatusPedido.Entregue => "Entrega concluída; fiscal permanece disponível para auditoria.",
            _ => fiscal.StatusAutomacao
        };
        fiscal.UpdatedAt = DateTime.UtcNow;
    }

    pedido.ObservacoesInternas = AppendOperationalObservation(
        pedido.ObservacoesInternas,
        $"Fluxo operacional avançou de {statusAnterior} para {proximoStatus} em {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC.");

    await db.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);

    if (pedido.Cliente is not null)
    {
        if (statusPagamentoAnterior != pedido.StatusPagamento && pedido.StatusPagamento == StatusPagamento.Aprovado)
        {
            await notificacaoService.EnviarConfirmacaoPagamentoAsync(pedido.Cliente, pedido);
        }

        if (proximoStatus is StatusPedido.EmSeparacao or StatusPedido.Enviado or StatusPedido.Entregue)
        {
            await notificacaoService.EnviarStatusPedidoAsync(pedido.Cliente, pedido, BuildMensagemAtualizacaoPedido(pedido));
        }
    }

    return Results.Ok(ApiResponse<PedidoLojaDto>.Ok(BuildPedidoLojaDto(pedido), $"Fluxo operacional avançado para {FormatStatusPedido(pedido.Status)}."));
})
.WithName("AvancarFluxoOperacionalPedido")
;

app.MapPut("/api/pedidos/{id}/logistica", [Authorize(Policy = "Gerente")] async (
    int id,
    PedidoLogisticaRequest request,
    NexumDbContext db,
    INotificacaoService notificacaoService,
    CancellationToken ct) =>
{
    var pedido = await db.Pedidos
        .Include(item => item.Pagamentos)
        .Include(item => item.Cliente)
        .FirstOrDefaultAsync(item => item.Id == id, ct);

    if (pedido is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Pedido nao encontrado."));
    }

    pedido.FreteMetodo = TrimOrNull(request.FreteMetodo) ?? pedido.FreteMetodo;
    pedido.FreteTransportadora = TrimOrNull(request.FreteTransportadora) ?? pedido.FreteTransportadora;
    pedido.FreteCodigoRastreio = TrimOrNull(request.FreteCodigoRastreio);
    pedido.FretePrazoDias = request.FretePrazoDias is > 0 ? Math.Min(request.FretePrazoDias.Value, 120) : pedido.FretePrazoDias;
    pedido.UpdatedAt = DateTime.UtcNow;

    if (pedido.Status == StatusPedido.Enviado && pedido.Cliente is not null)
    {
        await notificacaoService.EnviarStatusPedidoAsync(pedido.Cliente, pedido, BuildMensagemAtualizacaoPedido(pedido));
    }

    await db.SaveChangesAsync(ct);

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
        pedido.FreteCodigoRastreio,
        BuildPedidoInstruction(pedido.StatusPagamento, pedido.MeioPagamento, pedido.GatewayTransacaoId),
        pagamentoAtual?.Parcelas ?? 1,
        pagamentoAtual?.PixQrcode,
        pagamentoAtual?.BoletoUrl);

    return Results.Ok(ApiResponse<PedidoLojaDto>.Ok(dto, "Logistica do pedido atualizada."));
})
.WithName("AtualizarLogisticaPedido")
;

app.MapPost("/api/pedidos", async (
    PedidoRequest request,
    NexumDbContext db,
    IConfiguration configuration,
    IFiscalRoutingEngine fiscalRoutingEngine,
    INotificacaoService notificacaoService,
    IHttpClientFactory httpClientFactory,
    IServiceProvider services,
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

    var cliente = await LoadClienteCheckoutAsync(db, request.ClienteId, ct);
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
    var produtosMap = await FiltrarProdutosPublicaveis(db.Produtos)
        .Include(produto => produto.Fornecedor)
        .Where(produto => produtoSlugs.Contains(produto.Slug))
        .ToDictionaryAsync(produto => produto.Slug, ct);

    if (produtosMap.Count != produtoSlugs.Count)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Um ou mais produtos nao estao liberados para venda."));
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
    EnderecoEntregaRequest? enderecoRequest = null;
    if (request.EnderecoEntrega is not null)
    {
        var enderecoJson = System.Text.Json.JsonSerializer.Serialize(request.EnderecoEntrega);
        enderecoRequest = System.Text.Json.JsonSerializer.Deserialize<EnderecoEntregaRequest>(
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

    if (enderecoRequest is null || string.IsNullOrWhiteSpace(enderecoRequest.Cep))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("CEP de entrega obrigatorio para calcular o frete."));
    }

    var freteRequest = new FreteCotacaoRequest(
        configuration["Frete:CepOrigem"] ?? configuration["MelhorEnvio:CepOrigem"] ?? "17400000",
        enderecoRequest.Cep,
        itensSolicitados.Select(item =>
        {
            var produto = produtosMap[item.ProdutoId];
            return new FreteCotacaoItemRequest(
                produto.Sku,
                item.Quantidade,
                produto.PrecoPromocional ?? produto.Preco,
                produto.Peso,
                produto.Altura,
                produto.Largura,
                produto.Comprimento);
        }).ToList());
    var freteCotacoes = await CotarFreteAsync(freteRequest, configuration, httpClientFactory, ct);
    var freteSelecionado = string.IsNullOrWhiteSpace(request.FreteMetodo)
        ? freteCotacoes.OrderBy(item => item.Valor).ThenBy(item => item.PrazoDias).FirstOrDefault()
        : freteCotacoes.FirstOrDefault(item => string.Equals(item.Codigo, request.FreteMetodo, StringComparison.OrdinalIgnoreCase));

    if (freteSelecionado is null)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Opcao de frete invalida ou indisponivel. Refaca a cotacao."));
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
        FreteValor = freteSelecionado.Valor,
        FreteMetodo = freteSelecionado.Codigo,
        FreteTransportadora = freteSelecionado.Transportadora,
        FretePrazoDias = freteSelecionado.PrazoDias,
        Total = Math.Max(0m, subtotal + freteSelecionado.Valor - desconto),
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
    db.Financeiros.Add(BuildFinanceiroReceitaPedido(pedido));
    await db.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);

    try
    {
        await EnsurePedidoFiscalAutomationAsync(pedido, cliente, request, db, fiscalRoutingEngine, ct);
        await db.SaveChangesAsync(ct);
    }
    catch
    {
        // A automacao fiscal nao pode derrubar a venda: o pedido ja foi gravado e commitado.
    }

    await TryCreateGenesisContaReceberPedidoAsync(services, pedido, cliente, ct);
    await notificacaoService.EnviarConfirmacaoPedidoAsync(cliente, pedido);
    var metodoPagamento = (request.MetodoPagamento ?? string.Empty).Trim().ToLowerInvariant();
    var gatewayResult = metodoPagamento switch
    {
        "cartao" or "cartão" or "cartao_credito" or "cartãocredito" or "cartaocredito" =>
            await TryStartMercadoPagoPaymentAsync(pedido, cliente, request.DadosCartao, request.Parcelas ?? 1, configuration, httpClientFactory, http, ct),
        "boleto" =>
            await TryStartMercadoPagoPaymentAsync(pedido, cliente, null, request.Parcelas ?? 1, configuration, httpClientFactory, http, ct),
        _ =>
            await TryStartGatewayPaymentAsync(pedido, cliente, configuration, httpClientFactory, http, ct)
    };
    if (gatewayResult.Started)
    {
        pedido.GatewayPagamento = gatewayResult.Gateway;
        pedido.GatewayTransacaoId = gatewayResult.TransactionId;
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
        pedido.FreteCodigoRastreio,
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

app.MapPost("/api/logistica/roteamento", [Authorize(Policy = "Gerente")] async (
    LogisticaRoteamentoRequest request,
    IConfiguration configuration,
    NexumDbContext db,
    IHttpClientFactory httpClientFactory,
    IFiscalRoutingEngine fiscalRoutingEngine,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.CepDestino))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("CEP de destino obrigatorio para selecionar coleta logistica."));
    }

    if (request.Itens.Count == 0)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Informe ao menos um item para calcular coleta, cubagem e frete."));
    }

    var cotacoes = await CotarFreteAsync(
        new FreteCotacaoRequest(request.CepOrigem, request.CepDestino, request.Itens),
        configuration,
        httpClientFactory,
        ct);

    var freteSelecionado = cotacoes
        .OrderBy(item => item.Valor)
        .ThenBy(item => item.PrazoDias)
        .FirstOrDefault();

    if (freteSelecionado is null)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Nenhuma opcao de coleta ou frete disponivel para o destino informado."));
    }

    var empresas = await db.EmpresasGrupo
        .AsNoTracking()
        .Where(item => item.Ativa)
        .ToListAsync(ct);

    var tipoOperacao = request.TipoOperacao ?? TipoOperacaoFiscal.VendaInterna;
    var decision = fiscalRoutingEngine.Evaluate(
        new FiscalRoutingRequest(
            tipoOperacao,
            request.ValorProdutos,
            freteSelecionado.Valor,
            string.IsNullOrWhiteSpace(request.EstadoOrigem) ? "SP" : request.EstadoOrigem,
            string.IsNullOrWhiteSpace(request.EstadoDestino) ? "SP" : request.EstadoDestino,
            request.CategoriaFiscal,
            request.SubcategoriaFiscal,
            request.NaturezaOperacao,
            request.ExigeMarketplace,
            request.ExigeDropshipping,
            true,
            false),
        empresas.Select(fiscalRoutingEngine.ToSnapshot).ToList());

    var candidatoFiscal = decision.EmpresaSelecionada is null
        ? null
        : decision.Ranking.FirstOrDefault(item => item.Empresa.Id == decision.EmpresaSelecionada.Id);
    var custoFiscal = candidatoFiscal?.CustoTotalEstimado ?? 0m;
    var custoOperacionalTotal = decimal.Round(custoFiscal + freteSelecionado.Valor, 2);
    var receitaTotal = request.ValorProdutos + freteSelecionado.Valor;
    var lucroEstimado = decimal.Round(receitaTotal - custoOperacionalTotal, 2);
    var margemEstimada = receitaTotal <= 0 ? 0m : decimal.Round(lucroEstimado / receitaTotal * 100m, 2);

    var pendencias = new List<string>();
    if (!decision.Sucesso)
    {
        pendencias.Add(decision.Resumo);
    }

    if (!cotacoes.Any(item => string.Equals(item.Fonte, "Melhor Envio", StringComparison.OrdinalIgnoreCase)))
    {
        pendencias.Add("Cotacao usando tabela local ate credencial oficial da transportadora ficar operacional.");
    }

    var resumo = $"Coleta sugerida: {freteSelecionado.Transportadora} / {freteSelecionado.Nome} por R$ {freteSelecionado.Valor:F2} em {freteSelecionado.PrazoDias} dia(s). Emitente: {decision.EmpresaSelecionada?.CodigoEmpresa ?? "pendente"}. Custo total estimado: R$ {custoOperacionalTotal:F2}. Margem prevista: {margemEstimada:F2}%.";
    var resposta = new LogisticaRoteamentoResponseDto(
        freteSelecionado,
        cotacoes,
        decision.EmpresaSelecionada?.CodigoEmpresa,
        decision.EmpresaSelecionada?.RazaoSocial,
        decision.EmpresaSelecionada?.Cnpj,
        decision.Resumo,
        custoOperacionalTotal,
        lucroEstimado,
        margemEstimada,
        resumo,
        pendencias.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
        DateTime.UtcNow);

    return Results.Ok(ApiResponse<LogisticaRoteamentoResponseDto>.Ok(resposta, "Roteamento logistico calculado com menor custo e melhor margem."));
})
.WithName("LogisticaRoteamento")
;

app.MapPost("/api/webhooks/mercadopago", async (
    HttpContext http,
    IConfiguration configuration,
    NexumDbContext db,
    INotificacaoService notificacaoService,
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
        .ThenInclude(pedido => pedido!.Cliente)
        .FirstOrDefaultAsync(item => item.GatewayTransacaoId == paymentId, ct);

    if (pagamento is null)
    {
        return Results.Ok(new { received = true, updated = false, reason = "pagamento_nao_encontrado" });
    }

    var statusPagamentoAnterior = pagamento.Pedido?.StatusPagamento;
    pagamento.WebhookPayload = rawPaymentPayload ?? payload;
    pagamento.DataProcessamento = DateTime.UtcNow;
    pagamento.UpdatedAt = DateTime.UtcNow;

    if (!string.IsNullOrWhiteSpace(status))
    {
        ApplyMercadoPagoStatus(status, pagamento);
    }

    await db.SaveChangesAsync(ct);

    if (pagamento.Pedido?.Cliente is not null && statusPagamentoAnterior != pagamento.Pedido.StatusPagamento && pagamento.Pedido.StatusPagamento == StatusPagamento.Aprovado)
    {
        await notificacaoService.EnviarConfirmacaoPagamentoAsync(pagamento.Pedido.Cliente, pagamento.Pedido);
    }

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

app.MapGet("/api/gestao-corporativa/painel", [Authorize(Policy = "Gerente")] async (NexumDbContext db, CancellationToken ct) =>
{
    var agora = DateTime.UtcNow;
    var comprasPainel = await BuildComprasPainelAsync(db, ct);

    var totalEmpresas = await db.EmpresasGrupo.AsNoTracking().CountAsync(ct);
    var empresasAtivas = await db.EmpresasGrupo.AsNoTracking().CountAsync(empresa => empresa.Ativa, ct);
    var emitentesSaida = await db.EmpresasGrupo.AsNoTracking().CountAsync(empresa => empresa.Ativa && empresa.PermiteNfeSaida, ct);
    var emitentesEntrada = await db.EmpresasGrupo.AsNoTracking().CountAsync(empresa => empresa.Ativa && empresa.PermiteNfeEntrada, ct);
    var empresasSemCnpj = await db.EmpresasGrupo.AsNoTracking().CountAsync(empresa => empresa.Cnpj == "" || empresa.Cnpj == null, ct);
    var empresasSemSerie = await db.EmpresasGrupo.AsNoTracking().CountAsync(empresa => empresa.Ativa && empresa.PermiteNfeSaida && (empresa.SerieNfe == null || empresa.SerieNfe == ""), ct);

    var produtosAtivos = await db.Produtos.AsNoTracking().CountAsync(produto => produto.Ativo, ct);
    var produtosSemIdentificacao = await db.Produtos.AsNoTracking()
        .CountAsync(produto => produto.Ativo && (produto.CodigoBarras == null || produto.CodigoBarras == "" || produto.QrCode == null || produto.QrCode == "" || produto.IdentificacaoEstoque == null || produto.IdentificacaoEstoque == ""), ct);
    var produtosSemFornecedor = await db.Produtos.AsNoTracking()
        .CountAsync(produto => produto.Ativo && produto.FornecedorId == null, ct);
    var produtosEstoqueBaixo = await db.Produtos.AsNoTracking()
        .CountAsync(produto => produto.Ativo && produto.EstoqueAtual <= produto.EstoqueMinimo, ct);
    var produtosMargem = await db.Produtos.AsNoTracking()
        .Where(produto => produto.Ativo && produto.Preco > 0)
        .Select(produto => new { produto.Preco, produto.Custo })
        .ToListAsync(ct);
    var margemMedia = produtosMargem.Count == 0
        ? 0m
        : Math.Round(produtosMargem.Average(produto => produto.Preco <= 0 ? 0m : ((produto.Preco - produto.Custo) / produto.Preco) * 100m), 2);

    var clientesTotal = await db.Clientes.AsNoTracking().CountAsync(ct);
    var clientesSemDocumento = await db.Clientes.AsNoTracking()
        .CountAsync(cliente => cliente.CpfCnpj == null || cliente.CpfCnpj == "", ct);
    var fornecedoresTotal = await db.Fornecedores.AsNoTracking().CountAsync(ct);
    var fornecedoresSemDocumento = await db.Fornecedores.AsNoTracking()
        .CountAsync(fornecedor => fornecedor.Cnpj == null || fornecedor.Cnpj == "", ct);
    var fornecedoresSemLoja = await db.Fornecedores.AsNoTracking()
        .CountAsync(fornecedor => fornecedor.Status == StatusFornecedor.Ativo && fornecedor.LojaVinculadaId == null, ct);

    var pedidosAbertos = await db.Pedidos.AsNoTracking()
        .CountAsync(pedido => pedido.Status != StatusPedido.Entregue && pedido.Status != StatusPedido.Cancelado && pedido.Status != StatusPedido.Devolvido && pedido.Status != StatusPedido.Reembolsado, ct);
    var pedidosPagosSemFiscal = await db.Pedidos.AsNoTracking()
        .CountAsync(pedido => pedido.StatusPagamento == StatusPagamento.Aprovado && !db.Fiscais.Any(fiscal => fiscal.PedidoId == pedido.Id), ct);
    var pedidosLogisticaPendente = await db.Pedidos.AsNoTracking()
        .CountAsync(pedido => pedido.Status == StatusPedido.Enviado && (pedido.FreteCodigoRastreio == null || pedido.FreteCodigoRastreio == ""), ct);
    var financeiroPendente = await db.Financeiros.AsNoTracking()
        .CountAsync(lancamento => lancamento.Status == StatusLancamento.Pendente || lancamento.Status == StatusLancamento.Atrasado, ct);
    var fiscalPendente = await db.Fiscais.AsNoTracking()
        .CountAsync(fiscal => fiscal.StatusNfe == StatusNfe.Pendente || fiscal.StatusNfe == StatusNfe.Emitida, ct);

    var comprasPendentes = comprasPainel.Solicitacoes.Count(solicitacao => !string.Equals(solicitacao.Status, "aprovada", StringComparison.OrdinalIgnoreCase) && !string.Equals(solicitacao.Status, "cancelada", StringComparison.OrdinalIgnoreCase))
        + comprasPainel.Pedidos.Count(pedido => !string.Equals(pedido.Status, "recebido", StringComparison.OrdinalIgnoreCase) && !string.Equals(pedido.Status, "cancelado", StringComparison.OrdinalIgnoreCase));

    var indicadores = new List<GestaoCorporativaIndicadorDto>
    {
        new("empresas", "Empresas operacionais", $"{empresasAtivas}/{totalEmpresas}", $"{emitentesSaida} emitentes de saída e {emitentesEntrada} emitentes de entrada", BuildHealthStatus(empresasSemCnpj + empresasSemSerie), "erp-empresas"),
        new("cadastros", "Cadastros documentados", $"{clientesTotal + fornecedoresTotal}", $"{clientesSemDocumento + fornecedoresSemDocumento} sem CPF/CNPJ ou CNPJ", BuildHealthStatus(clientesSemDocumento + fornecedoresSemDocumento), "cadastros"),
        new("produtos", "Produtos publicáveis", $"{produtosAtivos}", $"{produtosSemIdentificacao + produtosSemFornecedor} exigem amarração física ou fornecedor", BuildHealthStatus(produtosSemIdentificacao + produtosSemFornecedor), "cadastro-produtos"),
        new("compras", "Aquisições em curso", $"{comprasPendentes}", $"{comprasPainel.Entradas.Count} entradas registradas para alimentar estoque", BuildHealthStatus(comprasPendentes), "erp-compras"),
        new("vendas", "Pedidos em ciclo aberto", $"{pedidosAbertos}", $"{pedidosPagosSemFiscal} pagos aguardando documento fiscal", BuildHealthStatus(pedidosPagosSemFiscal), "pedidos"),
        new("financeiro", "Pendências financeiras", $"{financeiroPendente}", "Receitas, despesas, taxas ou conciliações ainda abertas", BuildHealthStatus(financeiroPendente), "erp-financeiro"),
        new("fiscal", "Documentos fiscais", $"{fiscalPendente}", "NF-e/NFC-e pendentes ou emitidas sem autorização final", BuildHealthStatus(fiscalPendente), "erp-fiscal"),
        new("logistica", "Rastreio e coleta", $"{pedidosLogisticaPendente}", "Pedidos enviados que ainda precisam de rastreio/coleta", BuildHealthStatus(pedidosLogisticaPendente), "erp-logistica"),
        new("margem", "Margem média de itens", $"{margemMedia:N2}%", "Baseada no preço e custo dos produtos ativos", margemMedia > 0 ? "ok" : "atencao", "cadastro-produtos")
    };

    var alertas = new List<GestaoCorporativaAlertaDto>();
    AddCorporateAlert(alertas, produtosSemIdentificacao, "Cadastros", "Produtos sem identificação física", $"{produtosSemIdentificacao} itens ativos ainda precisam de código de barras, QR Code ou identificação de estoque.", "alta", "cadastro-produtos");
    AddCorporateAlert(alertas, produtosSemFornecedor, "Compras", "Produtos sem fornecedor", $"{produtosSemFornecedor} itens ativos não estão vinculados a origem de aquisição.", "alta", "erp-compras");
    AddCorporateAlert(alertas, produtosEstoqueBaixo, "Estoque", "Produtos abaixo do mínimo", $"{produtosEstoqueBaixo} itens exigem cotação, compra, entrada ou reposição.", "media", "erp-compras");
    AddCorporateAlert(alertas, clientesSemDocumento, "Clientes", "Clientes sem CPF/CNPJ", $"{clientesSemDocumento} clientes precisam de documento válido para faturamento.", "alta", "cadastro-clientes");
    AddCorporateAlert(alertas, fornecedoresSemDocumento, "Fornecedores", "Fornecedores sem CNPJ", $"{fornecedoresSemDocumento} fornecedores precisam de documento para compras formais.", "alta", "cadastro-fornecedores");
    AddCorporateAlert(alertas, fornecedoresSemLoja, "Fornecedores", "Fornecedores sem loja vinculada", $"{fornecedoresSemLoja} fornecedores ativos precisam de vínculo operacional.", "media", "cadastro-fornecedores");
    AddCorporateAlert(alertas, pedidosPagosSemFiscal, "Fiscal", "Vendas pagas sem documento fiscal", $"{pedidosPagosSemFiscal} pedidos pagos ainda precisam de emissão fiscal.", "alta", "erp-fiscal");
    AddCorporateAlert(alertas, financeiroPendente, "Financeiro", "Financeiro pendente", $"{financeiroPendente} lançamentos precisam de baixa, conciliação ou cancelamento.", "media", "erp-financeiro");
    AddCorporateAlert(alertas, pedidosLogisticaPendente, "Logística", "Envios sem rastreio", $"{pedidosLogisticaPendente} pedidos enviados ainda precisam de rastreio/coleta.", "media", "erp-logistica");
    AddCorporateAlert(alertas, empresasSemSerie + empresasSemCnpj, "Empresas", "Configuração fiscal empresarial incompleta", $"{empresasSemSerie + empresasSemCnpj} pendências em CNPJ ou série fiscal das empresas.", "alta", "erp-empresas");

    if (alertas.Count == 0)
    {
        alertas.Add(new GestaoCorporativaAlertaDto("Geral", "Ciclo corporativo sem pendências críticas", "Os cadastros principais, amarrações fiscais, financeiras e logísticas não retornaram lacunas críticas neste momento.", "ok", "overview"));
    }

    var vinculos = new List<GestaoCorporativaVinculoDto>
    {
        new("Empresa", "Fiscal", emitentesSaida + emitentesEntrada, empresasSemSerie + empresasSemCnpj, BuildHealthStatus(empresasSemSerie + empresasSemCnpj)),
        new("Produto", "Fornecedor/Compras", produtosAtivos, produtosSemFornecedor + comprasPendentes, BuildHealthStatus(produtosSemFornecedor + comprasPendentes)),
        new("Pedido", "Financeiro", pedidosAbertos, financeiroPendente, BuildHealthStatus(financeiroPendente)),
        new("Pedido", "Fiscal", pedidosAbertos, pedidosPagosSemFiscal + fiscalPendente, BuildHealthStatus(pedidosPagosSemFiscal + fiscalPendente)),
        new("Pedido", "Logística", pedidosAbertos, pedidosLogisticaPendente, BuildHealthStatus(pedidosLogisticaPendente)),
        new("Cliente", "Documento/Faturamento", clientesTotal, clientesSemDocumento, BuildHealthStatus(clientesSemDocumento))
    };

    var painel = new GestaoCorporativaPainelDto(indicadores, alertas, vinculos, margemMedia, agora);
    return Results.Ok(ApiResponse<GestaoCorporativaPainelDto>.Ok(painel));
})
.WithName("GestaoCorporativaPainel")
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

app.MapPut("/api/fiscal/pedidos/{id}/status", [Authorize(Policy = "Gerente")] async (
    int id,
    StatusUpdateRequest request,
    NexumDbContext db,
    INotificacaoService notificacaoService,
    CancellationToken ct) =>
{
    var fiscal = await db.Fiscais
        .Include(item => item.Pedido)
        .ThenInclude(pedido => pedido!.Cliente)
        .FirstOrDefaultAsync(item => item.Id == id, ct);

    if (fiscal is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Registro fiscal nao encontrado."));
    }

    if (!TryParseStatusNfe(request.NovoStatus, out var novoStatus))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Status fiscal invalido."));
    }

    var statusAnterior = fiscal.StatusNfe;
    fiscal.StatusNfe = novoStatus;
    fiscal.UpdatedAt = DateTime.UtcNow;

    if (novoStatus == StatusNfe.Emitida)
    {
        fiscal.DataEmissao ??= DateTime.UtcNow;
        fiscal.StatusAutomacao = "NFe emitida.";
    }
    else if (novoStatus == StatusNfe.Autorizada)
    {
        fiscal.DataEmissao ??= DateTime.UtcNow;
        fiscal.DataAutorizacao ??= DateTime.UtcNow;
        fiscal.StatusAutomacao = "NFe autorizada.";
    }
    else if (novoStatus is StatusNfe.Cancelada or StatusNfe.Denegada or StatusNfe.Inutilizada)
    {
        fiscal.StatusAutomacao = $"NFe {novoStatus.ToString().ToLowerInvariant()}.";
    }

    await db.SaveChangesAsync(ct);

    if (fiscal.Pedido?.Cliente is not null &&
        statusAnterior != novoStatus &&
        novoStatus is StatusNfe.Emitida or StatusNfe.Autorizada &&
        fiscal.EmailClienteNotificadoEm is null)
    {
        await notificacaoService.EnviarNotaFiscalEmitidaAsync(fiscal.Pedido.Cliente, fiscal.Pedido, fiscal);
        fiscal.EmailClienteNotificadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    return Results.Ok(ApiResponse<string>.Ok("Status fiscal atualizado com sucesso."));
})
.WithName("AtualizarStatusFiscal")
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
            item.CustoTotalEstimado,
            item.LucroEstimado,
            item.MargemEstimadaPercentual,
            item.Score,
            item.Justificativas.ToList())).ToList());

    return Results.Ok(ApiResponse<FiscalRoutingSimulationDto>.Ok(resultado));
})
.WithName("FiscalSimularRoteamento")
;

app.MapPost("/api/fiscal/preparar-emissao-manual", [Authorize(Policy = "Gerente")] async (
    FiscalManualEmissaoRequest request,
    IConfiguration configuration,
    NexumDbContext db,
    IFiscalRoutingEngine fiscalRoutingEngine,
    CancellationToken ct) =>
{
    var certificado = TestCertificadoNFe(configuration);
    var empresas = await db.EmpresasGrupo
        .AsNoTracking()
        .Where(item => item.Ativa)
        .ToListAsync(ct);

    var routingRequest = new FiscalRoutingRequest(
        request.TipoOperacao,
        request.Subtotal,
        request.Frete,
        request.EstadoOrigem,
        request.EstadoDestino,
        request.CategoriaFiscal,
        request.SubcategoriaFiscal,
        request.NaturezaOperacao,
        request.ExigeMarketplace,
        request.ExigeDropshipping,
        request.RequerSaidaNfe,
        request.RequerEntradaNfe);

    var decision = fiscalRoutingEngine.Evaluate(routingRequest, empresas.Select(fiscalRoutingEngine.ToSnapshot).ToList());
    var pendencias = new List<string>();

    if (!certificado.Operacional)
    {
        pendencias.AddRange(certificado.Pendencias);
    }

    if (decision.EmpresaSelecionada is null)
    {
        pendencias.Add("Nenhuma empresa ativa atende os critérios fiscais informados.");
    }

    if (request.Subtotal <= 0)
    {
        pendencias.Add("Subtotal precisa ser maior que zero.");
    }

    if (request.MargemMinima > 0 && request.ImpostosEstimados > 0)
    {
        var receitaLiquidaEstim = request.Subtotal + request.Frete - request.ImpostosEstimados;
        var margemPercentual = request.Subtotal <= 0 ? 0 : receitaLiquidaEstim / request.Subtotal * 100m;
        if (margemPercentual < request.MargemMinima)
        {
            pendencias.Add("Margem estimada abaixo do mínimo informado.");
        }
    }

    var resumo = new FiscalManualEmissaoResponseDto(
        certificado.Operacional,
        certificado.Status,
        certificado.Referencia,
        decision.Resumo,
        decision.EmpresaSelecionada?.CodigoEmpresa,
        decision.EmpresaSelecionada?.RazaoSocial,
        decision.EmpresaSelecionada?.Cnpj,
        decision.EmpresaSelecionada?.Estado,
        pendencias.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
        DateTime.UtcNow);

    return Results.Ok(ApiResponse<FiscalManualEmissaoResponseDto>.Ok(resumo));
})
.WithName("FiscalPrepararEmissaoManual")
;

app.MapPost("/api/fiscal/rascunho-manual", [Authorize(Policy = "Gerente")] async (
    FiscalManualEmissaoRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var key = "fiscal.manual.rascunho";
    var payload = JsonSerializer.Serialize(request, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    var entity = await db.ConfiguracoesSistema.FirstOrDefaultAsync(item => item.Chave == key, ct);

    if (entity is null)
    {
        entity = new ConfiguracaoSistema
        {
            Chave = key,
            Tipo = TipoConfiguracao.JSON,
            Grupo = "Fiscal",
            Descricao = "Rascunho manual de emissão de NF-e",
            Editavel = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.ConfiguracoesSistema.Add(entity);
    }

    entity.Valor = payload;
    entity.Tipo = TipoConfiguracao.JSON;
    entity.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync(ct);

    var resposta = new
    {
        Salvo = true,
        Chave = key,
        SalvoEm = entity.UpdatedAt
    };

    return Results.Ok(ApiResponse<object>.Ok(resposta, "Rascunho fiscal salvo com sucesso."));
})
.WithName("FiscalSalvarRascunhoManual")
;

app.MapGet("/api/fiscal/rascunho-manual", [Authorize(Policy = "Gerente")] async (
    NexumDbContext db,
    CancellationToken ct) =>
{
    var key = "fiscal.manual.rascunho";
    var entity = await db.ConfiguracoesSistema.AsNoTracking().FirstOrDefaultAsync(item => item.Chave == key, ct);

    if (entity is null || string.IsNullOrWhiteSpace(entity.Valor))
    {
        return Results.Ok(ApiResponse<object>.Ok(new { Existe = false }, "Nenhum rascunho encontrado."));
    }

    return Results.Ok(ApiResponse<object>.Ok(new
    {
        Existe = true,
        entity.Valor,
        entity.UpdatedAt
    }, "Rascunho manual localizado."));
})
.WithName("FiscalObterRascunhoManual")
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

static async Task<EmpresaGrupo?> ObterEmpresaAquisicaoAsync(NexumDbContext db, string? empresaReferencia, CancellationToken ct)
{
    var referencia = TrimOrNull(empresaReferencia);
    if (!string.IsNullOrWhiteSpace(referencia) && int.TryParse(referencia, out var empresaId))
    {
        var empresaPorId = await db.EmpresasGrupo.AsNoTracking().FirstOrDefaultAsync(item => item.Id == empresaId && item.Ativa, ct);
        if (empresaPorId is not null)
        {
            return empresaPorId;
        }
    }

    if (!string.IsNullOrWhiteSpace(referencia))
    {
        var codigo = referencia.Trim();
        var empresaPorCodigo = await db.EmpresasGrupo
            .AsNoTracking()
            .FirstOrDefaultAsync(item =>
                item.Ativa &&
                (item.CodigoEmpresa == codigo || item.NomeFantasia == codigo || item.RazaoSocial == codigo), ct);
        if (empresaPorCodigo is not null)
        {
            return empresaPorCodigo;
        }
    }

    return await db.EmpresasGrupo
        .AsNoTracking()
        .Where(item => item.Ativa)
        .OrderByDescending(item => item.EmitentePreferencial)
        .ThenBy(item => item.PrioridadeFiscal)
        .ThenBy(item => item.Id)
        .FirstOrDefaultAsync(ct);
}

static async Task<string> GerarSkuProdutoAsync(NexumDbContext db, ProdutoRequest request, EmpresaGrupo? empresa, int? produtoIdIgnorado, CancellationToken ct)
{
    var empresaCodigo = NormalizarCodigoAlfa(
        empresa?.CodigoEmpresa
        ?? empresa?.NomeFantasia
        ?? empresa?.RazaoSocial
        ?? request.EmpresaAquisicaoCodigo
        ?? "NEXUM",
        "NEXUM");
    var aquisicaoCodigo = ObterCodigoAquisicao(request.TipoProduto, request.FornecedorId);
    var prefixo = $"{empresaCodigo}-{aquisicaoCodigo}";
    var sequencialInformado = ExtrairSequencialSku(request.Sku);
    var sequencial = sequencialInformado ?? await ObterProximoSequencialSkuAsync(db, prefixo, produtoIdIgnorado, ct);
    var sku = $"{prefixo}-{sequencial:000000}";

    while (await db.Produtos.AnyAsync(item => item.Sku == sku && (!produtoIdIgnorado.HasValue || item.Id != produtoIdIgnorado.Value), ct))
    {
        sequencial++;
        sku = $"{prefixo}-{sequencial:000000}";
    }

    return sku;
}

static async Task<int> ObterProximoSequencialSkuAsync(NexumDbContext db, string prefixo, int? produtoIdIgnorado, CancellationToken ct)
{
    var prefixoCompleto = $"{prefixo}-";
    var skus = await db.Produtos
        .AsNoTracking()
        .Where(item => item.Sku.StartsWith(prefixoCompleto) && (!produtoIdIgnorado.HasValue || item.Id != produtoIdIgnorado.Value))
        .Select(item => item.Sku)
        .ToListAsync(ct);

    var maior = 0;
    foreach (var sku in skus)
    {
        var sequencial = ExtrairSequencialSku(sku);
        if (sequencial.HasValue && sequencial.Value > maior)
        {
            maior = sequencial.Value;
        }
    }

    return maior + 1;
}

static int? ExtrairSequencialSku(string? sku)
{
    if (string.IsNullOrWhiteSpace(sku))
    {
        return null;
    }

    var digits = new string(sku.Where(char.IsDigit).ToArray());
    if (digits.Length == 0)
    {
        return null;
    }

    var sequencialTexto = digits.Length > 6 ? digits[^6..] : digits;
    return int.TryParse(sequencialTexto, out var sequencial) && sequencial > 0 ? sequencial : null;
}

static string ObterCodigoAquisicao(string? tipoProduto, int? fornecedorId)
{
    var valor = $"{tipoProduto ?? string.Empty} {(fornecedorId.HasValue ? "fornecedor" : string.Empty)}".ToLowerInvariant();
    if (valor.Contains("drop"))
    {
        return "DROP";
    }

    if (valor.Contains("fornecedor") || valor.Contains("dist"))
    {
        return "DIST";
    }

    return "ECOM";
}

static string NormalizarCodigoAlfa(string? value, string fallback)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return fallback;
    }

    var normalized = value.Trim().Normalize(NormalizationForm.FormD);
    var builder = new StringBuilder(normalized.Length);
    foreach (var c in normalized)
    {
        if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
        {
            continue;
        }

        if (char.IsLetterOrDigit(c))
        {
            builder.Append(char.ToUpperInvariant(c));
        }
    }

    var codigo = builder.ToString();
    if (string.IsNullOrWhiteSpace(codigo))
    {
        return fallback;
    }

    return codigo.Length > 10 ? codigo[..10] : codigo;
}

static string GerarCodigoBarrasProduto(Produto produto) => GerarCodigoBarrasProdutoPorSku(produto.Sku);

static string GerarCodigoBarrasProdutoPorSku(string sku)
{
    var digits = new string((sku ?? string.Empty).Where(char.IsDigit).ToArray());
    var seed = digits.Length > 0
        ? digits
        : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sku ?? string.Empty))).Where(char.IsDigit).DefaultIfEmpty('0').Aggregate(new StringBuilder(), (builder, value) => builder.Append(value)).ToString();
    var baseCode = $"789{seed.PadLeft(9, '0')[^9..]}";
    var sum = 0;
    for (var index = 0; index < baseCode.Length; index++)
    {
        var digit = baseCode[index] - '0';
        sum += index % 2 == 0 ? digit : digit * 3;
    }

    var checkDigit = (10 - (sum % 10)) % 10;
    return $"{baseCode}{checkDigit}";
}

static string GerarQrCodeProdutoCadastro(string sku, string nome, string? tipoProduto, int? fornecedorId)
{
    var payload = new
    {
        tipo = "PRODUTO",
        sku,
        nome,
        aquisicao = ObterCodigoAquisicao(tipoProduto, fornecedorId),
        fornecedorId,
        criadoEm = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
    };

    return JsonSerializer.Serialize(payload);
}

static string GerarIdentificacaoEstoqueCadastro(string sku, string nome, string? tipoProduto, int? fornecedorId) =>
    $"SKU={sku};ITEM={nome};AQUISICAO={ObterCodigoAquisicao(tipoProduto, fornecedorId)};FORNECEDOR={fornecedorId?.ToString(CultureInfo.InvariantCulture) ?? "NAO_VINCULADO"}";

static string GerarQrCodeProduto(Produto produto, CompraPedidoLookupRow pedido, int entradaId, string? documento)
{
    var payload = new
    {
        tipo = "ESTOQUE_ENTRADA",
        sku = produto.Sku,
        produtoId = produto.Id,
        produto = produto.Nome,
        pedidoId = pedido.Id,
        pedido = pedido.Numero,
        entradaId,
        origem = pedido.Origem,
        fornecedorId = pedido.FornecedorId,
        documento,
        estoque = produto.EstoqueAtual,
        atualizadoEm = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
    };

    return JsonSerializer.Serialize(payload);
}

static string GerarIdentificacaoEstoqueProduto(Produto produto, CompraPedidoLookupRow pedido, int entradaId, int quantidadeRecebida, decimal custoUnitario, string? documento) =>
    $"SKU={produto.Sku};ITEM={produto.Nome};ORIGEM={pedido.Origem};PEDIDO={pedido.Numero};ENTRADA={entradaId};QTD={quantidadeRecebida};CUSTO={custoUnitario.ToString("0.00", CultureInfo.InvariantCulture)};FORNECEDOR={pedido.FornecedorId};DOC={documento ?? "PENDENTE"}";

static string? NormalizeEmail(string? value) =>
    string.IsNullOrWhiteSpace(value)
        ? null
        : value.Trim().ToLowerInvariant();

static string? Truncate(string? value, int maxLength)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return null;
    }

    var trimmed = value.Trim();
    return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
}

static string ComputeSha256Hash(string value)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
    return Convert.ToHexString(bytes).ToLowerInvariant();
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

static bool IsValidCpfCnpj(string? value)
{
    var digits = NormalizeDocument(value);
    if (string.IsNullOrWhiteSpace(digits))
    {
        return false;
    }

    return digits.Length switch
    {
        11 => IsValidCpf(digits),
        14 => IsValidCnpj(digits),
        _ => false
    };
}

static bool IsValidCpf(string cpf)
{
    if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
    {
        return false;
    }

    int SumDigit(int length)
    {
        var sum = 0;
        for (var i = 0; i < length; i++)
        {
            sum += (cpf[i] - '0') * (length + 1 - i);
        }
        return sum;
    }

    var first = SumDigit(9);
    var firstDigit = first % 11 < 2 ? 0 : 11 - (first % 11);
    if (firstDigit != cpf[9] - '0')
    {
        return false;
    }

    var second = SumDigit(10);
    var secondDigit = second % 11 < 2 ? 0 : 11 - (second % 11);
    return secondDigit == cpf[10] - '0';
}

static bool IsValidCnpj(string cnpj)
{
    if (cnpj.Length != 14 || cnpj.Distinct().Count() == 1)
    {
        return false;
    }

    int CalculateDigit(int length, int[] weights)
    {
        var sum = 0;
        for (var i = 0; i < length; i++)
        {
            sum += (cnpj[i] - '0') * weights[i];
        }

        var mod = sum % 11;
        return mod < 2 ? 0 : 11 - mod;
    }

    var firstWeights = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
    var secondWeights = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

    var firstDigit = CalculateDigit(12, firstWeights);
    if (firstDigit != cnpj[12] - '0')
    {
        return false;
    }

    var secondDigit = CalculateDigit(13, secondWeights);
    return secondDigit == cnpj[13] - '0';
}

static async Task<Cliente?> LoadClienteCheckoutAsync(
    NexumDbContext db,
    int clienteId,
    CancellationToken ct)
{
    var connection = db.Database.GetDbConnection();
    var shouldClose = connection.State != ConnectionState.Open;
    if (shouldClose)
    {
        await connection.OpenAsync(ct);
    }

    try
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, nome, email, cpf_cnpj, telefone, whatsapp, newsletter, vip, pontos_fidelidade, status, tipo
            FROM clientes
            WHERE id = @id
            LIMIT 1
            """;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@id";
        parameter.Value = clienteId;
        command.Parameters.Add(parameter);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        var statusRaw = ReadNullableString(reader, "status");
        var tipoRaw = ReadNullableString(reader, "tipo");

        return new Cliente
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Nome = ReadNullableString(reader, "nome") ?? string.Empty,
            Email = ReadNullableString(reader, "email") ?? string.Empty,
            CpfCnpj = ReadNullableString(reader, "cpf_cnpj"),
            Telefone = ReadNullableString(reader, "telefone"),
            Whatsapp = ReadNullableString(reader, "whatsapp"),
            Newsletter = ReadNullableBoolean(reader, "newsletter") ?? true,
            Vip = ReadNullableBoolean(reader, "vip") ?? false,
            PontosFidelidade = ReadNullableInt32(reader, "pontos_fidelidade") ?? 0,
            Status = Enum.TryParse<StatusCliente>(statusRaw, true, out var status) ? status : StatusCliente.Pendente,
            Tipo = Enum.TryParse<TipoCliente>(tipoRaw, true, out var tipo) ? tipo : TipoCliente.PF
        };
    }
    finally
    {
        if (shouldClose && connection.State == ConnectionState.Open)
        {
            await connection.CloseAsync();
        }
    }
}

static string? ReadNullableString(IDataRecord reader, string columnName)
{
    var ordinal = reader.GetOrdinal(columnName);
    return reader.IsDBNull(ordinal) ? null : Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
}

static bool? ReadNullableBoolean(IDataRecord reader, string columnName)
{
    var ordinal = reader.GetOrdinal(columnName);
    if (reader.IsDBNull(ordinal))
    {
        return null;
    }

    var value = reader.GetValue(ordinal);
    return value switch
    {
        bool boolValue => boolValue,
        sbyte signedByte => signedByte != 0,
        byte unsignedByte => unsignedByte != 0,
        short shortValue => shortValue != 0,
        ushort ushortValue => ushortValue != 0,
        int intValue => intValue != 0,
        long longValue => longValue != 0,
        string textValue when bool.TryParse(textValue, out var parsedBool) => parsedBool,
        string textValue when int.TryParse(textValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedInt) => parsedInt != 0,
        _ => Convert.ToBoolean(value, CultureInfo.InvariantCulture)
    };
}

static int? ReadNullableInt32(IDataRecord reader, string columnName)
{
    var ordinal = reader.GetOrdinal(columnName);
    if (reader.IsDBNull(ordinal))
    {
        return null;
    }

    var value = reader.GetValue(ordinal);
    return value switch
    {
        int intValue => intValue,
        long longValue => checked((int)longValue),
        short shortValue => shortValue,
        byte byteValue => byteValue,
        sbyte signedByte => signedByte,
        string textValue when int.TryParse(textValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedInt) => parsedInt,
        _ => Convert.ToInt32(value, CultureInfo.InvariantCulture)
    };
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
                pedido.Id,
                pedido.NumeroPedido,
                pedido.Subtotal,
                pedido.Desconto,
                pedido.FreteValor,
                pedido.FreteMetodo,
                pedido.FreteTransportadora,
                pedido.FretePrazoDias,
                pedido.MeioPagamento,
                pedido.GatewayPagamento,
                pedido.Total,
                cliente = new
                {
                    cliente.Id,
                    cliente.Nome,
                    cliente.Email,
                    cliente.Telefone,
                    cliente.CpfCnpj
                },
                enderecoEntrega = request.EnderecoEntrega,
                itens = pedido.Itens?.Select(item => new
                {
                    item.ProdutoId,
                    item.SkuProduto,
                    item.NomeProduto,
                    item.Quantidade,
                    item.PrecoUnitario,
                    item.PrecoTotal
                }).ToList()
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
            pedido.FreteMetodo,
            pedido.FreteTransportadora,
            pedido.FretePrazoDias,
            pedido.Desconto,
            pedido.MeioPagamento,
            pedido.GatewayPagamento,
            resumoPagamento,
            perfilTributacao,
            usaStLegado = empresaEmitente.UsaStLegado,
            destacaIcmsStSeparado = empresaEmitente.DestacaIcmsStSeparado,
            cliente = new
            {
                cliente.Id,
                cliente.Nome,
                cliente.Email,
                cliente.Telefone,
                cliente.CpfCnpj
            },
            enderecoEntrega = request.EnderecoEntrega,
            destino = new { estadoDestino },
            itens = pedido.Itens?.Select(item => new
            {
                item.ProdutoId,
                item.SkuProduto,
                item.NomeProduto,
                item.Quantidade,
                item.PrecoUnitario,
                item.PrecoTotal
            }).ToList(),
            ranking = decision.Ranking.Select(item => new
            {
                item.Empresa.CodigoEmpresa,
                item.Empresa.RazaoSocial,
                item.Score,
                item.CustoTributarioEstimado,
                item.CustoOperacionalEstimado,
                item.CustoTotalEstimado,
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

static string BuildHealthStatus(int pending) =>
    pending <= 0 ? "ok" : pending <= 3 ? "atencao" : "critico";

static void AddCorporateAlert(List<GestaoCorporativaAlertaDto> alertas, int quantidade, string modulo, string titulo, string detalhe, string severidade, string acao)
{
    if (quantidade <= 0)
    {
        return;
    }

    alertas.Add(new GestaoCorporativaAlertaDto(modulo, titulo, detalhe, severidade, acao));
}

static string NormalizeCompraOrigem(string? value)
{
    var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
    return normalized switch
    {
        "dropshipping" or "drop" => "Dropshipping",
        "parceria" or "parceiro" or "marketplace" => "Parceria",
        "encomenda" or "pedido_cliente" or "cliente" => "Encomenda",
        "estoque" or "estoque_fisico" or "compra_direta" or "direta" => "EstoqueFisico",
        _ => "EstoqueFisico"
    };
}

static string? NormalizeCompraSolicitacaoStatus(string? value)
{
    var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
    return normalized switch
    {
        "aberta" or "aberto" => "Aberta",
        "cotado" or "cotada" or "cotacao" => "Cotado",
        "aprovado" or "aprovada" => "Aprovada",
        "atendido" or "atendida" => "Atendida",
        "cancelado" or "cancelada" => "Cancelada",
        "fechado" or "fechada" => "Fechada",
        _ => null
    };
}

static string? NormalizeCompraPedidoStatus(string? value)
{
    var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
    return normalized switch
    {
        "aberto" or "aberta" => "Aberto",
        "aprovado" or "aprovada" => "Aprovado",
        "recebidoparcial" or "recebido_parcial" or "recebido parcial" or "parcial" => "RecebidoParcial",
        "recebido" or "recebida" => "Recebido",
        "cancelado" or "cancelada" => "Cancelado",
        "fechado" or "fechada" => "Fechado",
        _ => null
    };
}

static async Task TryCreateGenesisContaPagarCompraAsync(
    IServiceProvider services,
    string numeroPedido,
    int fornecedorId,
    string fornecedorNome,
    decimal valor,
    DateTime emissao,
    DateTime vencimento,
    string? formaPagamento,
    CancellationToken ct)
{
    try
    {
        var genesisDb = services.GetService<GenesisDbContext>();
        if (genesisDb is null || !await genesisDb.Database.CanConnectAsync(ct))
        {
            return;
        }

        var existe = await genesisDb.ContasPagar.AnyAsync(item => item.NumeroDocumento == numeroPedido, ct);
        if (existe)
        {
            return;
        }

        genesisDb.ContasPagar.Add(new GenesisContaPagar
        {
            NumeroDocumento = numeroPedido,
            FornecedorId = fornecedorId,
            Descricao = $"Compra de mercadorias - {fornecedorNome}",
            ValorOriginal = valor,
            ValorPago = 0,
            ValorMulta = 0,
            ValorJuros = 0,
            ValorDesconto = 0,
            DataEmissao = emissao,
            DataVencimento = vencimento,
            Status = "ABERTO",
            FormaPagamento = TrimOrNull(formaPagamento) ?? "A DEFINIR",
            NumeroBoleto = null
        });

        await genesisDb.SaveChangesAsync(ct);
    }
    catch
    {
        // A compra no banco principal continua sendo a fonte oficial caso o Genesis esteja indisponivel.
    }
}

static Financeiro BuildFinanceiroReceitaPedido(Pedido pedido)
{
    var now = DateTime.UtcNow;
    return new Financeiro
    {
        Pedido = pedido,
        Tipo = TipoLancamento.Receita,
        Categoria = "Vendas online",
        Descricao = $"Conta a receber do pedido {pedido.NumeroPedido}",
        Valor = pedido.Total,
        DataVencimento = now.AddDays(1),
        Status = StatusLancamento.Pendente,
        MeioPagamento = TrimOrNull(pedido.MeioPagamento) ?? "A definir",
        ContaBancaria = "A definir",
        Observacoes = $"Gerado automaticamente no checkout; origem={pedido.Origem}; gateway={pedido.GatewayPagamento}",
        CreatedAt = now,
        UpdatedAt = now
    };
}

static bool TryGetNextOperationalStatus(StatusPedido statusAtual, out StatusPedido proximoStatus)
{
    proximoStatus = statusAtual switch
    {
        StatusPedido.Pendente => StatusPedido.Pago,
        StatusPedido.Pago => StatusPedido.EmSeparacao,
        StatusPedido.EmSeparacao => StatusPedido.Enviado,
        StatusPedido.Enviado => StatusPedido.Entregue,
        _ => statusAtual
    };

    return proximoStatus != statusAtual;
}

static async Task<string?> ApplyPedidoStatusTransitionAsync(Pedido pedido, StatusPedido novoStatus, NexumDbContext db, CancellationToken ct)
{
    var statusAnterior = pedido.Status;
    var statusAnteriorConfirmaEstoque = statusAnterior is StatusPedido.Pago or StatusPedido.EmSeparacao or StatusPedido.Enviado or StatusPedido.Entregue;
    var novoStatusConfirmaEstoque = novoStatus is StatusPedido.Pago or StatusPedido.EmSeparacao or StatusPedido.Enviado or StatusPedido.Entregue;
    var novoStatusCancelaEstoque = novoStatus is StatusPedido.Cancelado or StatusPedido.Devolvido or StatusPedido.Reembolsado;

    if (pedido.Itens is { Count: > 0 } && statusAnterior != novoStatus)
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
                return $"Produto do pedido nao encontrado: {item.NomeProduto}.";
            }

            if (!statusAnteriorConfirmaEstoque && novoStatusConfirmaEstoque)
            {
                if (produto.EstoqueAtual < item.Quantidade)
                {
                    return $"Estoque insuficiente para confirmar {item.NomeProduto}.";
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

            item.StatusItem = novoStatus switch
            {
                StatusPedido.EmSeparacao => StatusItemPedido.Separado,
                StatusPedido.Enviado => StatusItemPedido.Enviado,
                StatusPedido.Entregue => StatusItemPedido.Entregue,
                StatusPedido.Cancelado or StatusPedido.Devolvido or StatusPedido.Reembolsado => StatusItemPedido.Cancelado,
                _ => item.StatusItem
            };
            produto.UpdatedAt = DateTime.UtcNow;
        }
    }

    pedido.Status = novoStatus;
    pedido.UpdatedAt = DateTime.UtcNow;

    if (novoStatusConfirmaEstoque && pedido.StatusPagamento == StatusPagamento.Aguardando)
    {
        pedido.StatusPagamento = StatusPagamento.Aprovado;
        pedido.DataPagamento ??= DateTime.UtcNow;
    }

    if (novoStatus == StatusPedido.Enviado)
    {
        pedido.DataEnvio ??= DateTime.UtcNow;
        pedido.FreteTransportadora = TrimOrNull(pedido.FreteTransportadora) ?? "Operação Nexum";
        pedido.FreteMetodo = TrimOrNull(pedido.FreteMetodo) ?? "roteamento-interno";
        pedido.FreteCodigoRastreio = TrimOrNull(pedido.FreteCodigoRastreio) ?? $"NX-{pedido.NumeroPedido}";
        pedido.FretePrazoDias = pedido.FretePrazoDias <= 0 ? 3 : pedido.FretePrazoDias;
    }

    if (novoStatus == StatusPedido.Entregue)
    {
        pedido.DataEntrega ??= DateTime.UtcNow;
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

    return null;
}

static async Task SyncFinanceiroPedidoOperacionalAsync(Pedido pedido, NexumDbContext db, CancellationToken ct)
{
    var financeiro = await db.Financeiros
        .FirstOrDefaultAsync(item => item.PedidoId == pedido.Id && item.Tipo == TipoLancamento.Receita, ct);

    if (financeiro is null)
    {
        financeiro = BuildFinanceiroReceitaPedido(pedido);
        db.Financeiros.Add(financeiro);
    }

    if (pedido.StatusPagamento == StatusPagamento.Aprovado)
    {
        financeiro.Status = StatusLancamento.Pago;
        financeiro.DataPagamento ??= pedido.DataPagamento ?? DateTime.UtcNow;
        financeiro.ContaBancaria = string.IsNullOrWhiteSpace(financeiro.ContaBancaria) || financeiro.ContaBancaria == "A definir"
            ? "Conta operacional a conciliar"
            : financeiro.ContaBancaria;
    }

    if (pedido.Status is StatusPedido.Cancelado or StatusPedido.Reembolsado)
    {
        financeiro.Status = pedido.Status == StatusPedido.Reembolsado ? StatusLancamento.Estornado : StatusLancamento.Cancelado;
    }

    financeiro.Valor = pedido.Total;
    financeiro.MeioPagamento = TrimOrNull(pedido.MeioPagamento) ?? financeiro.MeioPagamento ?? "A definir";
    financeiro.Observacoes = AppendOperationalObservation(
        financeiro.Observacoes,
        $"Sincronizado pelo fluxo operacional do pedido {pedido.NumeroPedido} em {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC.");
    financeiro.UpdatedAt = DateTime.UtcNow;
}

static PedidoRequest BuildPedidoRequestFromPedido(Pedido pedido)
{
    var endereco = pedido.EnderecoEntrega is null
        ? null
        : new EnderecoEntregaRequest(
            pedido.EnderecoEntrega.Cep,
            pedido.EnderecoEntrega.Logradouro,
            pedido.EnderecoEntrega.Numero,
            pedido.EnderecoEntrega.Complemento,
            pedido.EnderecoEntrega.Bairro,
            pedido.EnderecoEntrega.Cidade,
            pedido.EnderecoEntrega.Estado);

    var itens = pedido.Itens?
        .Select(item => new PedidoItemRequest(item.ProdutoId?.ToString(CultureInfo.InvariantCulture) ?? item.SkuProduto ?? item.NomeProduto, item.Quantidade))
        .ToList() ?? [];

    return new PedidoRequest(
        pedido.ClienteId,
        pedido.LojaId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
        itens,
        pedido.CupomCodigo,
        endereco,
        pedido.MeioPagamento,
        pedido.Parcelas,
        pedido.GatewayPagamento,
        null,
        pedido.FreteValor,
        pedido.FreteMetodo,
        pedido.FreteTransportadora,
        pedido.FretePrazoDias);
}

static PedidoLojaDto BuildPedidoLojaDto(Pedido pedido)
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
        pedido.FreteCodigoRastreio,
        BuildPedidoInstruction(pedido.StatusPagamento, pedido.MeioPagamento, pedido.GatewayTransacaoId),
        pagamentoAtual?.Parcelas ?? pedido.Parcelas,
        pagamentoAtual?.PixQrcode,
        pagamentoAtual?.BoletoUrl);
}

static string AppendOperationalObservation(string? current, string note)
{
    var normalized = TrimOrNull(current);
    return string.IsNullOrWhiteSpace(normalized)
        ? note
        : $"{normalized}{Environment.NewLine}{note}";
}

static async Task TryCreateGenesisContaReceberPedidoAsync(
    IServiceProvider services,
    Pedido pedido,
    Cliente cliente,
    CancellationToken ct)
{
    try
    {
        var genesisDb = services.GetService<GenesisDbContext>();
        if (genesisDb is null || !await genesisDb.Database.CanConnectAsync(ct))
        {
            return;
        }

        var existe = await genesisDb.ContasReceber
            .AnyAsync(item => item.NumeroPedidoReferencia == pedido.NumeroPedido || item.NumeroDocumento == pedido.NumeroPedido, ct);
        if (existe)
        {
            return;
        }

        genesisDb.ContasReceber.Add(new GenesisContaReceber
        {
            NumeroDocumento = pedido.NumeroPedido,
            ClienteId = null,
            Descricao = $"Venda online - {cliente.Nome}",
            ValorOriginal = pedido.Total,
            ValorRecebido = 0,
            ValorMulta = 0,
            ValorJuros = 0,
            ValorDesconto = pedido.Desconto,
            DataEmissao = pedido.CreatedAt,
            DataVencimento = pedido.CreatedAt.AddDays(1),
            Status = "PENDENTE",
            FormaRecebimento = TrimOrNull(pedido.MeioPagamento) ?? "A DEFINIR",
            NumeroPedidoReferencia = pedido.NumeroPedido
        });

        await genesisDb.SaveChangesAsync(ct);
    }
    catch
    {
        // O pedido e o financeiro local permanecem como fonte oficial caso o banco Genesis esteja indisponivel.
    }
}

static async Task<Cliente?> GetClientePortalAsync(ClaimsPrincipal principal, NexumDbContext db, CancellationToken ct)
{
    var email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Email);
    if (string.IsNullOrWhiteSpace(email))
    {
        return null;
    }

    return await db.Clientes.FirstOrDefaultAsync(item => item.Email == email, ct);
}

static string? ValidateEnderecoRequest(ClientePortalEnderecoRequest request)
{
    var cep = NormalizeDocument(request.Cep);
    if (string.IsNullOrWhiteSpace(request.Logradouro) ||
        string.IsNullOrWhiteSpace(request.Numero) ||
        string.IsNullOrWhiteSpace(request.Bairro) ||
        string.IsNullOrWhiteSpace(request.Cidade) ||
        string.IsNullOrWhiteSpace(request.Estado))
    {
        return "Endereco incompleto.";
    }

    if (cep is null || cep.Length != 8)
    {
        return "CEP invalido.";
    }

    var estado = request.Estado.Trim();
    return estado.Length == 2 ? null : "Estado deve ter 2 letras.";
}

static TipoEndereco ParseTipoEndereco(string? value) =>
    Enum.TryParse<TipoEndereco>(value, true, out var tipo) ? tipo : TipoEndereco.Entrega;

static ClienteLojaDto ToClienteLojaDto(Cliente cliente) => new(
    cliente.Id,
    cliente.Nome,
    cliente.Email,
    cliente.Telefone,
    cliente.CpfCnpj,
    cliente.Tipo.ToString(),
    cliente.RgIe,
    cliente.DataNascimento,
    cliente.Whatsapp,
    cliente.Avatar,
    cliente.Newsletter,
    cliente.Vip,
    cliente.PontosFidelidade,
    cliente.Status.ToString(),
    cliente.UltimoAcesso,
    cliente.ConfirmadoEm,
    cliente.CreatedAt,
    cliente.UpdatedAt);

static FornecedorDto ToFornecedorDto(Fornecedor fornecedor) => new(
    fornecedor.Id,
    string.IsNullOrWhiteSpace(fornecedor.NomeFantasia) ? fornecedor.RazaoSocial : fornecedor.NomeFantasia,
    fornecedor.Cnpj ?? string.Empty,
    fornecedor.Email ?? string.Empty,
    fornecedor.Telefone ?? string.Empty,
    fornecedor.Segmento ?? "Geral",
    fornecedor.CreatedAt,
    fornecedor.RazaoSocial,
    fornecedor.NomeFantasia,
    fornecedor.Ie,
    fornecedor.Whatsapp,
    fornecedor.Endereco,
    fornecedor.Cidade,
    fornecedor.Estado,
    fornecedor.Cep,
    fornecedor.LojaVinculadaId,
    fornecedor.ComissaoPercentual,
    fornecedor.PrazoEntregaDias,
    fornecedor.Status.ToString(),
    fornecedor.Observacoes,
    fornecedor.UpdatedAt);

static ClientePortalEnderecoDto ToClientePortalEnderecoDto(Endereco endereco) => new(
    endereco.Id,
    endereco.Apelido,
    endereco.Tipo.ToString(),
    endereco.Cep,
    endereco.Logradouro,
    endereco.Numero,
    endereco.Complemento,
    endereco.Bairro,
    endereco.Cidade,
    endereco.Estado,
    endereco.Pais,
    endereco.Padrao);

static IQueryable<Produto> FiltrarProdutosPublicaveis(IQueryable<Produto> query) =>
    query.Where(produto =>
        produto.Ativo &&
        produto.LojaId > 0 &&
        produto.CategoriaId.HasValue &&
        !string.IsNullOrEmpty(produto.Nome) &&
        !string.IsNullOrEmpty(produto.Sku) &&
        !string.IsNullOrEmpty(produto.Slug) &&
        (!string.IsNullOrEmpty(produto.DescricaoCurta) || !string.IsNullOrEmpty(produto.DescricaoLonga)) &&
        !string.IsNullOrEmpty(produto.ImagemPrincipal) &&
        produto.Preco > 0 &&
        produto.Peso > 0 &&
        produto.Altura > 0 &&
        produto.Largura > 0 &&
        produto.Comprimento > 0);

static ProdutoLojaDto MapearProdutoLojaDto(Produto produto) => new(
    produto.Slug,
    produto.Nome,
    produto.DescricaoCurta ?? produto.DescricaoLonga ?? string.Empty,
    produto.DescricaoCurta,
    produto.Preco,
    produto.PrecoPromocional,
    produto.ImagemPrincipal ?? string.Empty,
    produto.EstoqueAtual,
    produto.EstoqueMinimo,
    produto.EstoqueReservado,
    produto.Destaque,
    produto.Sku,
    produto.Categoria?.Slug ?? "classicos",
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
    produto.ImagensGaleria,
    produto.CodigoBarras,
    produto.QrCode,
    produto.IdentificacaoEstoque);

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
        "certificadonfe" or "certificadonf" or "nfe" or "certificado" => TestCertificadoNFe(configuration),
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
            ["Use: ecommerce, dropshipping, shopify, cjdropshipping, mercadopago, melhorenvio, mercadolivre, certificado ou bancaria."],
            DateTime.UtcNow,
            null)
    };
}

static IntegracaoDiagnosticoDto TestCertificadoNFe(IConfiguration configuration)
{
    var tipo = GetIntegrationValue(configuration, "CertificadoNFe:Tipo", "Integracoes:CertificadoNFe:Tipo");
    var arquivoPfx = GetIntegrationValue(configuration, "CertificadoNFe:ArquivoPfx", "Integracoes:CertificadoNFe:ArquivoPfx");
    var senha = GetIntegrationValue(configuration, "CertificadoNFe:Senha", "Integracoes:CertificadoNFe:Senha");
    var thumbprint = GetIntegrationValue(configuration, "CertificadoNFe:Thumbprint", "Integracoes:CertificadoNFe:Thumbprint");
    var cnpj = GetIntegrationValue(configuration, "CertificadoNFe:Cnpj", "Integracoes:CertificadoNFe:Cnpj");
    var validoAte = GetIntegrationValue(configuration, "CertificadoNFe:ValidoAte", "Integracoes:CertificadoNFe:ValidoAte");
    var modelo = string.IsNullOrWhiteSpace(tipo) ? "Nao configurado" : tipo.ToUpperInvariant();

    if (string.Equals(tipo, "A1", StringComparison.OrdinalIgnoreCase))
    {
        var arquivoExiste = !string.IsNullOrWhiteSpace(arquivoPfx) && File.Exists(arquivoPfx);
        var configurada = arquivoExiste && IsConfiguredSecret(senha);
        var operacional = configurada && !string.IsNullOrWhiteSpace(cnpj);

        return new IntegracaoDiagnosticoDto(
            "Certificado NF-e",
            "certificado",
            operacional ? "Pronto" : "Aguardando arquivo",
            configurada,
            operacional,
            operacional
                ? "Certificado A1 localizado e apto para uso no emissor."
                : "Certificado A1 ainda depende do arquivo PFX e da senha no servidor.",
            operacional
                ? []
                : ["CertificadoNFe__ArquivoPfx", "CertificadoNFe__Senha", "CertificadoNFe__Cnpj"],
            DateTime.UtcNow,
            $"Tipo: A1 | CNPJ: {cnpj ?? "nao informado"} | Validade: {validoAte ?? "nao informada"}");
    }

    if (string.Equals(tipo, "A3", StringComparison.OrdinalIgnoreCase))
    {
        var configurada = IsConfiguredSecret(thumbprint);
        var operacional = configurada && !string.IsNullOrWhiteSpace(cnpj);

        return new IntegracaoDiagnosticoDto(
            "Certificado NF-e",
            "certificado",
            operacional ? "Pronto" : "Aguardando token/identificador",
            configurada,
            operacional,
            operacional
                ? "Certificado A3 identificado e apto para emissão."
                : "Certificado A3 precisa do thumbprint/serial válido no servidor.",
            operacional
                ? []
                : ["CertificadoNFe__Thumbprint", "CertificadoNFe__Cnpj"],
            DateTime.UtcNow,
            $"Tipo: A3 | CNPJ: {cnpj ?? "nao informado"} | Validade: {validoAte ?? "nao informada"}");
    }

    return new IntegracaoDiagnosticoDto(
        "Certificado NF-e",
        "certificado",
        "Aguardando configuração",
        false,
        false,
        "Informe se o certificado será A1 ou A3. O sistema está pronto para validar o emissor assim que a empresa cadastrar os dados.",
        ["CertificadoNFe__Tipo", "CertificadoNFe__Cnpj"],
        DateTime.UtcNow,
        modelo);
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
        client.BaseAddress = new Uri($"https://{storeDomain}/admin/api/{apiVersion!.Trim('/')}/");
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

static async Task<GatewayPaymentStartResult> TryStartMercadoPagoPaymentAsync(
    Pedido pedido,
    Cliente cliente,
    object? dadosCartao,
    int parcelas,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    HttpContext http,
    CancellationToken ct)
{
    var token = GetIntegrationValue(configuration, "MercadoPago:AccessToken", "Integracoes:MercadoPago:AccessToken");
    if (!IsConfiguredSecret(token))
    {
        return GatewayPaymentStartResult.NotStarted();
    }

    try
    {
        var client = httpClientFactory.CreateClient("mercado-pago");
        var metodo = (pedido.MeioPagamento ?? string.Empty).Trim().ToLowerInvariant();

        if (metodo == "boleto")
        {
            using var boletoRequest = new HttpRequestMessage(HttpMethod.Post, "v1/payments");
            boletoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            boletoRequest.Headers.Add("X-Idempotency-Key", $"nexum-{pedido.NumeroPedido}-boleto");
            boletoRequest.Content = JsonContent.Create(new
            {
                transaction_amount = pedido.Total,
                description = $"Pedido {pedido.NumeroPedido} - Nexum Altivon",
                payment_method_id = "bolbradesco",
                notification_url = BuildPublicUrl(http, "/api/webhooks/mercadopago"),
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

            using var boletoResponse = await client.SendAsync(boletoRequest, ct);
            var boletoBody = await boletoResponse.Content.ReadAsStringAsync(ct);
            if (!boletoResponse.IsSuccessStatusCode)
            {
                return GatewayPaymentStartResult.NotStarted(boletoBody);
            }

            using var boletoDocument = JsonDocument.Parse(boletoBody);
            var boletoRoot = boletoDocument.RootElement;
            var boletoId = boletoRoot.TryGetProperty("id", out var boletoIdProp) ? boletoIdProp.ToString() : null;
            string? boletoPaymentUrl = null;
            string? linhaDigitavel = null;
            if (boletoRoot.TryGetProperty("transaction_details", out var boletoDetails))
            {
                boletoPaymentUrl = boletoDetails.TryGetProperty("external_resource_url", out var boletoUrlProp) ? boletoUrlProp.ToString() : null;
                linhaDigitavel = boletoDetails.TryGetProperty("payment_method_reference_id", out var linhaProp) ? linhaProp.ToString() : null;
            }

            return GatewayPaymentStartResult.Success("MercadoPago", boletoId, linhaDigitavel, boletoPaymentUrl, boletoBody);
        }

        if (dadosCartao is null)
        {
            return GatewayPaymentStartResult.NotStarted("dados_cartao_ausentes");
        }

        var cartaoJson = JsonSerializer.Serialize(dadosCartao);
        var cartao = JsonSerializer.Deserialize<DadosCartaoPedidoRequest>(cartaoJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (cartao is null || string.IsNullOrWhiteSpace(cartao.Numero) || string.IsNullOrWhiteSpace(cartao.NomeTitular) || string.IsNullOrWhiteSpace(cartao.Validade))
        {
            return GatewayPaymentStartResult.NotStarted("dados_cartao_invalidos");
        }

        var partes = cartao.Validade.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (partes.Length != 2 || !int.TryParse(partes[0], out var mesValidade) || !int.TryParse(partes[1], out var anoCurto))
        {
            return GatewayPaymentStartResult.NotStarted("validade_cartao_invalida");
        }

        var tokenRequest = new
        {
            card_number = cartao.Numero.Replace(" ", string.Empty),
            expiration_month = mesValidade,
            expiration_year = int.Parse($"20{anoCurto:D2}"),
            security_code = cartao.Cvv,
            cardholder = new
            {
                name = cartao.NomeTitular,
                identification = new
                {
                    type = string.IsNullOrWhiteSpace(cartao.CpfTitular) || cartao.CpfTitular.Length > 14 ? "CNPJ" : "CPF",
                    number = new string((cartao.CpfTitular ?? string.Empty).Where(char.IsDigit).ToArray())
                }
            }
        };

        using var tokenRequestMessage = new HttpRequestMessage(HttpMethod.Post, "v1/card_tokens");
        tokenRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        tokenRequestMessage.Content = JsonContent.Create(tokenRequest);

        using var tokenResponse = await client.SendAsync(tokenRequestMessage, ct);
        var tokenBody = await tokenResponse.Content.ReadAsStringAsync(ct);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            return GatewayPaymentStartResult.NotStarted(tokenBody);
        }

        using var tokenDocument = JsonDocument.Parse(tokenBody);
        var cardToken = tokenDocument.RootElement.TryGetProperty("id", out var tokenIdProp) ? tokenIdProp.ToString() : null;
        if (string.IsNullOrWhiteSpace(cardToken))
        {
            return GatewayPaymentStartResult.NotStarted(tokenBody);
        }

        using var paymentRequest = new HttpRequestMessage(HttpMethod.Post, "v1/payments");
        paymentRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        paymentRequest.Headers.Add("X-Idempotency-Key", $"nexum-{pedido.NumeroPedido}-cartao");
        paymentRequest.Content = JsonContent.Create(new
        {
            transaction_amount = pedido.Total,
            token = cardToken,
            description = $"Pedido {pedido.NumeroPedido} - Nexum Altivon",
            installments = Math.Clamp(parcelas, 1, 24),
            payment_method_id = "master",
            payer = new
            {
                email = cliente.Email,
                identification = new
                {
                    type = string.IsNullOrWhiteSpace(cliente.CpfCnpj) || cliente.CpfCnpj.Length > 14 ? "CNPJ" : "CPF",
                    number = new string((cliente.CpfCnpj ?? string.Empty).Where(char.IsDigit).ToArray())
                }
            },
            external_reference = pedido.NumeroPedido,
            notification_url = BuildPublicUrl(http, "/api/webhooks/mercadopago")
        });

        using var paymentResponse = await client.SendAsync(paymentRequest, ct);
        var paymentBody = await paymentResponse.Content.ReadAsStringAsync(ct);
        if (!paymentResponse.IsSuccessStatusCode)
        {
            return GatewayPaymentStartResult.NotStarted(paymentBody);
        }

        using var paymentDocument = JsonDocument.Parse(paymentBody);
        var paymentRoot = paymentDocument.RootElement;
        var paymentId = paymentRoot.TryGetProperty("id", out var paymentIdProp) ? paymentIdProp.ToString() : null;
        string? paymentUrl = null;
        if (paymentRoot.TryGetProperty("transaction_details", out var transactionDetails))
        {
            paymentUrl = transactionDetails.TryGetProperty("external_resource_url", out var urlProp) ? urlProp.ToString() : null;
        }

        return GatewayPaymentStartResult.Success("MercadoPago", paymentId, null, paymentUrl, paymentBody);
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
        host = "api.nexumaltivon.com.br";
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

static bool TryParseStatusNfe(string? raw, out StatusNfe status)
{
    status = StatusNfe.Pendente;

    if (string.IsNullOrWhiteSpace(raw))
    {
        return false;
    }

    return Enum.TryParse(raw.Trim(), ignoreCase: true, out status);
}

static string BuildMensagemAtualizacaoPedido(Pedido pedido)
{
    return pedido.Status switch
    {
        StatusPedido.Pago => $"Pagamento confirmado para o pedido {pedido.NumeroPedido}. Estamos preparando o envio.",
        StatusPedido.EmSeparacao => $"Seu pedido {pedido.NumeroPedido} entrou em separacao. Em breve seguira para expedicao.",
        StatusPedido.Enviado => string.IsNullOrWhiteSpace(pedido.FreteCodigoRastreio)
            ? $"Seu pedido {pedido.NumeroPedido} foi enviado e esta em transporte."
            : $"Seu pedido {pedido.NumeroPedido} foi enviado. Codigo de rastreio: {pedido.FreteCodigoRastreio}.",
        StatusPedido.Entregue => $"Seu pedido {pedido.NumeroPedido} foi entregue. Obrigado por comprar conosco.",
        StatusPedido.Cancelado => $"O pedido {pedido.NumeroPedido} foi cancelado. Se precisar, fale com nosso atendimento.",
        StatusPedido.Devolvido => $"O pedido {pedido.NumeroPedido} foi devolvido e voltou para analise interna.",
        StatusPedido.Reembolsado => $"O pedido {pedido.NumeroPedido} foi reembolsado com sucesso.",
        _ => $"Seu pedido {pedido.NumeroPedido} foi atualizado para o status {pedido.Status}."
    };
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
        GetConfigValue(configMap, "site_url", "https://nexumaltivon.com.br"),
        contactEmail,
        GetConfigValue(configMap, "site_telefone", "(14) 99673-1879"),
        GetConfigValue(configMap, "site_telefone_secundario", "(14) 99634-8409"),
        GetConfigValue(configMap, "site_whatsapp", "5514996731879"),
        GetConfigValue(configMap, "site_whatsapp_secundario", "5514996348409"),
        GetConfigValue(configMap, "site_yara_email", contactEmail),
        GetConfigValue(configMap, "site_logo", "/imagens/homepage/Logo-2.png"),
        GetConfigValue(configMap, "site_subtitulo", "Participações societárias"),
        GetConfigValue(configMap, "site_institucional_url", "/institucional"),
        GetConfigValue(configMap, "site_politica_privacidade_url", "/politica-privacidade"),
        GetConfigValue(configMap, "site_politica_reembolso_url", "/politica-reembolso"),
        ParseJsonList(GetConfigValue(configMap, "home_hero_slides", string.Empty), GetDefaultHeroSlides()),
        ParseJsonList(GetConfigValue(configMap, "home_lojas_cards", string.Empty), GetDefaultStoreCards()),
        GetConfigValue(configMap, "home_intro_titulo", "Uma Nova Era Começa"),
        GetConfigValue(configMap, "home_intro_texto_1", "O Grupo Nexum Altivon está chegando para transformar e inovar o mercado digital brasileiro."),
        GetConfigValue(configMap, "home_intro_texto_2", "Nosso compromisso é claro: entregar qualidade superior, atendimento que faz a diferença e preços acessíveis que respeitam o seu bolso."),
        GetConfigValue(configMap, "home_intro_badge", "nexumaltivon.com.br"),
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
    new("ecommerce", "Grupo Nexum Altivon", "O Futuro do", "E-Commerce", "Seis lojas, uma operação conectada e uma proposta premium para transformar a experiência de compra online.", "/imagens/homepage/banner-ecommerce.svg"),
    new("marcas", "6 marcas em expansão", "Uma operação,", "múltiplos mercados", "Turismo, relógios, moda, tecnologia, construção e festas com a mesma curadoria comercial do Grupo Nexum Altivon.", "/imagens/homepage/banner-marcas.svg"),
    new("tecnologia", "Experiência tecnológica", "Compra segura com", "atendimento humano", "Fluxos preparados para catálogo, clientes, pedidos, integrações e relacionamento com visão de crescimento contínuo.", "/imagens/homepage/banner-atendimento.svg")
];

static List<StoreCardSiteDto> GetDefaultStoreCards() =>
[
    new("Gran Tur", "gran-tur", "Viagens & Turismo", "Mochilas, malas, acessórios de viagem e produtos para explorar o mundo com estilo e conforto.", "/imagens/homepage/loja-gran-tur.svg", "Plane"),
    new("Chronos", "chronos", "Relógios & Acessórios", "Relógios e acessórios para quem valoriza precisão, presença e elegância.", "/imagens/homepage/loja-chronos.svg", "Watch"),
    new("Moda Mim", "moda-mim", "Moda & Vestuário", "Roupas, calçados e acessórios para uma experiência de compra prática e atual.", "/imagens/homepage/loja-moda-mim.svg", "Shirt"),
    new("Geração Top+", "geracao-top", "Tecnologia & Gadgets", "Smartphones, eletrônicos, acessórios e tecnologia para rotina, trabalho e lazer.", "/imagens/homepage/loja-geracao-top.svg", "Smartphone"),
    new("Estruturaline", "estruturaline", "Construção & Estruturas", "Materiais, ferramentas e soluções para quem constrói com seriedade.", "/imagens/homepage/loja-estruturaline.svg", "Hammer"),
    new("Gran Festas", "gran-festas", "Festas & Eventos", "Decoração, utensílios e produtos para encontros, comemorações e eventos.", "/imagens/homepage/loja-gran-festas.svg", "Gift")
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
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    if (!await db.Database.CanConnectAsync())
    {
        logger.LogWarning("Banco indisponível durante a verificação de esquema operacional.");
        return;
    }

    await EnsureUsuariosSchemaAsync(db);
    await EnsureSystemAdminUsersAsync(db, configuration, logger);
    await EnsureGenesisGestOriginalSchemaAsync(db, logger);

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
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE produtos ADD COLUMN IF NOT EXISTS codigo_barras VARCHAR(64) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE produtos ADD COLUMN IF NOT EXISTS qr_code VARCHAR(500) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE produtos ADD COLUMN IF NOT EXISTS identificacao_estoque VARCHAR(500) NULL;");

    var produtosSemIdentificacao = await db.Produtos
        .Where(produto => string.IsNullOrEmpty(produto.CodigoBarras) || string.IsNullOrEmpty(produto.QrCode) || string.IsNullOrEmpty(produto.IdentificacaoEstoque))
        .Take(500)
        .ToListAsync();

    foreach (var produto in produtosSemIdentificacao)
    {
        produto.CodigoBarras = string.IsNullOrWhiteSpace(produto.CodigoBarras)
            ? GerarCodigoBarrasProduto(produto)
            : produto.CodigoBarras;
        produto.QrCode = string.IsNullOrWhiteSpace(produto.QrCode)
            ? GerarQrCodeProdutoCadastro(produto.Sku, produto.Nome, produto.TipoProduto.ToString(), produto.FornecedorId)
            : produto.QrCode;
        produto.IdentificacaoEstoque = string.IsNullOrWhiteSpace(produto.IdentificacaoEstoque)
            ? GerarIdentificacaoEstoqueCadastro(produto.Sku, produto.Nome, produto.TipoProduto.ToString(), produto.FornecedorId)
            : produto.IdentificacaoEstoque;
        produto.UpdatedAt = DateTime.UtcNow;
    }

    if (produtosSemIdentificacao.Count > 0)
    {
        await db.SaveChangesAsync();
    }

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
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE fiscal ADD COLUMN IF NOT EXISTS email_cliente_notificado_em DATETIME NULL;");

    await db.Database.ExecuteSqlRawAsync("ALTER TABLE clientes ADD COLUMN IF NOT EXISTS token_confirmacao_email VARCHAR(255) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE clientes ADD COLUMN IF NOT EXISTS confirmado_em DATETIME NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE clientes ADD COLUMN IF NOT EXISTS status INT NOT NULL DEFAULT 3;");

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
            ('site_nome', 'Grupo Nexum Altivon', 'Texto', 'Nome público principal exibido no site', 'Geral', 1),
            ('site_subtitulo', 'Participações societárias', 'Texto', 'Subtítulo discreto da marca no cabeçalho e rodapé', 'Geral', 1),
            ('site_logo', '/imagens/homepage/Logo-2.png', 'Imagem', 'Logo público carregado pela home, cabeçalho e rodapé', 'Geral', 1),
            ('site_institucional_url', '/institucional', 'Texto', 'Link da página institucional', 'SiteHome', 1),
            ('site_politica_privacidade_url', '/politica-privacidade', 'Texto', 'Link da política de privacidade', 'SiteHome', 1),
            ('site_politica_reembolso_url', '/politica-reembolso', 'Texto', 'Link da política de reembolso', 'SiteHome', 1),
            ('site_telefone_secundario', '(14) 99634-8409', 'Texto', 'Telefone comercial secundário', 'Geral', 1),
            ('site_whatsapp_secundario', '5514996348409', 'Texto', 'WhatsApp comercial secundário', 'Geral', 1),
            ('site_yara_email', 'corporativo.gna@gmail.com', 'Texto', 'E-mail de atendimento da Yara', 'Atendimento', 1),
            ('home_intro_titulo', 'Uma Nova Era Começa', 'Texto', 'Título principal do bloco institucional da home', 'SiteHome', 1),
            ('home_intro_texto_1', 'O Grupo Nexum Altivon está chegando para transformar e inovar o mercado digital brasileiro.', 'Texto', 'Primeiro texto institucional da home', 'SiteHome', 1),
            ('home_intro_texto_2', 'Nosso compromisso é claro: entregar qualidade superior, atendimento que faz a diferença e preços acessíveis que respeitam o seu bolso.', 'Texto', 'Segundo texto institucional da home', 'SiteHome', 1),
            ('home_intro_badge', 'nexumaltivon.com.br', 'Texto', 'Texto do selo institucional da home', 'SiteHome', 1),
            ('home_footer_texto', 'Portal em evolução contínua para vendas, relacionamento, parceiros e operações integradas.', 'Texto', 'Texto do rodapé público da home', 'SiteHome', 1),
            ('home_quality_items', '["Curadoria rigorosa de fornecedores","Atendimento humano e especializado","Política de devolução simplificada","Preços justos e acessíveis"]', 'JSON', 'Itens do bloco de qualidade da home', 'SiteHome', 1),
            ('home_lojas_cards', '[{{"nome":"Gran Tur","slug":"gran-tur","segmento":"Viagens & Turismo","descricao":"Mochilas, malas, acessórios de viagem e produtos para explorar o mundo com estilo e conforto.","imagem":"/imagens/homepage/loja-gran-tur.svg","icon":"Plane"}},{{"nome":"Chronos","slug":"chronos","segmento":"Relógios & Acessórios","descricao":"Relógios e acessórios para quem valoriza precisão, presença e elegância.","imagem":"/imagens/homepage/loja-chronos.svg","icon":"Watch"}},{{"nome":"Moda Mim","slug":"moda-mim","segmento":"Moda & Vestuário","descricao":"Roupas, calçados e acessórios para uma experiência de compra prática e atual.","imagem":"/imagens/homepage/loja-moda-mim.svg","icon":"Shirt"}},{{"nome":"Geração Top+","slug":"geracao-top","segmento":"Tecnologia & Gadgets","descricao":"Smartphones, eletrônicos, acessórios e tecnologia para rotina, trabalho e lazer.","imagem":"/imagens/homepage/loja-geracao-top.svg","icon":"Smartphone"}},{{"nome":"Estruturaline","slug":"estruturaline","segmento":"Construção & Estruturas","descricao":"Materiais, ferramentas e soluções para quem constrói com seriedade.","imagem":"/imagens/homepage/loja-estruturaline.svg","icon":"Hammer"}},{{"nome":"Gran Festas","slug":"gran-festas","segmento":"Festas & Eventos","descricao":"Decoração, utensílios e produtos para encontros, comemorações e eventos.","imagem":"/imagens/homepage/loja-gran-festas.svg","icon":"Gift"}}]', 'JSON', 'Cards e imagens das lojas na Home e página Lojas', 'SiteHome', 1),
            ('home_partner_cards', '[{{"title":"Parceiros de Vendas","text":"Lojas físicas ou online podem ampliar seus horizontes de venda com nossa infraestrutura comercial e operação integrada.","cta":"Quero Vender","href":"https://wa.me/5514996731879?text=Olá! Tenho interesse em ser parceiro de vendas do Grupo Nexum Altivon.","icon":"Store"}},{{"title":"Fornecedores & Distribuidores","text":"Distribuidores e fabricantes encontram um canal de venda em crescimento, com visão de volume, relacionamento e longo prazo.","cta":"Quero Fornecer","href":"https://wa.me/5514996348409?text=Olá! Sou fornecedor/distribuidor e tenho interesse em parceria com o Grupo Nexum Altivon.","icon":"Truck"}},{{"title":"Dropshipping","text":"Integre seu catálogo às nossas lojas ou utilize nossa infraestrutura para conectar produtos, logística e novos canais.","cta":"Quero Fazer Dropship","href":"https://wa.me/5514996731879?text=Olá! Tenho interesse em parceria de dropshipping com o Grupo Nexum Altivon.","icon":"Building2"}}]', 'JSON', 'Cards de parceria da home', 'SiteHome', 1),
            ('home_hero_slides', '[{{"id":"ecommerce","badge":"Grupo Nexum Altivon","title":"O Futuro do","highlight":"E-Commerce","description":"Seis lojas, uma operação conectada e uma proposta premium para transformar a experiência de compra online.","image":"/imagens/homepage/banner-ecommerce.svg"}},{{"id":"marcas","badge":"6 marcas em expansão","title":"Uma operação,","highlight":"múltiplos mercados","description":"Turismo, relógios, moda, tecnologia, construção e festas com a mesma curadoria comercial do Grupo Nexum Altivon.","image":"/imagens/homepage/banner-marcas.svg"}},{{"id":"tecnologia","badge":"Experiência tecnológica","title":"Compra segura com","highlight":"atendimento humano","description":"Fluxos preparados para catálogo, clientes, pedidos, integrações e relacionamento com visão de crescimento contínuo.","image":"/imagens/homepage/banner-atendimento.svg"}}]', 'JSON', 'Slides principais da home', 'SiteHome', 1)
        ON DUPLICATE KEY UPDATE
            valor = VALUES(valor),
            descricao = VALUES(descricao),
            grupo = VALUES(grupo),
            editavel = VALUES(editavel),
            updated_at = CURRENT_TIMESTAMP;
        """.Replace("{", "{{").Replace("}", "}}"));

    await EnsureComprasSchemaAsync(db);
    await EnsureValidationTokensAsync(db);
    await EnsurePedidosReceberAsync(db);
    await EnsureGenesisSharedSchemaAsync(scope.ServiceProvider, logger);
}

static async Task EnsureGenesisSharedSchemaAsync(IServiceProvider services, ILogger logger)
{
    var genesisDb = services.GetService<GenesisDbContext>();
    if (genesisDb is null)
    {
        return;
    }

    try
    {
        if (!await genesisDb.Database.CanConnectAsync())
        {
            logger.LogWarning("Banco GenesisGest.Net indisponivel durante a verificacao de schema compartilhado.");
            return;
        }

        await genesisDb.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS erp_contas_receber (
                id INT NOT NULL AUTO_INCREMENT,
                numero_documento VARCHAR(40) NULL,
                cliente_id INT NULL,
                descricao VARCHAR(200) NULL,
                valor_original DECIMAL(18,2) NOT NULL DEFAULT 0,
                valor_recebido DECIMAL(18,2) NOT NULL DEFAULT 0,
                valor_multa DECIMAL(18,2) NOT NULL DEFAULT 0,
                valor_juros DECIMAL(18,2) NOT NULL DEFAULT 0,
                valor_desconto DECIMAL(18,2) NOT NULL DEFAULT 0,
                data_emissao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                status VARCHAR(50) NULL,
                data_vencimento DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                data_recebimento DATETIME NULL,
                forma_recebimento VARCHAR(50) NULL,
                numero_pedido_referencia VARCHAR(100) NULL,
                PRIMARY KEY (id)
            );
            """);

        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_receber ADD COLUMN IF NOT EXISTS numero_documento VARCHAR(40) NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_receber ADD COLUMN IF NOT EXISTS cliente_id INT NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_receber ADD COLUMN IF NOT EXISTS descricao VARCHAR(200) NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_receber ADD COLUMN IF NOT EXISTS valor_original DECIMAL(18,2) NOT NULL DEFAULT 0;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_receber ADD COLUMN IF NOT EXISTS valor_recebido DECIMAL(18,2) NOT NULL DEFAULT 0;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_receber ADD COLUMN IF NOT EXISTS valor_multa DECIMAL(18,2) NOT NULL DEFAULT 0;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_receber ADD COLUMN IF NOT EXISTS valor_juros DECIMAL(18,2) NOT NULL DEFAULT 0;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_receber ADD COLUMN IF NOT EXISTS valor_desconto DECIMAL(18,2) NOT NULL DEFAULT 0;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_receber ADD COLUMN IF NOT EXISTS data_emissao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_receber ADD COLUMN IF NOT EXISTS status VARCHAR(50) NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_receber ADD COLUMN IF NOT EXISTS data_vencimento DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_receber ADD COLUMN IF NOT EXISTS data_recebimento DATETIME NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_receber ADD COLUMN IF NOT EXISTS forma_recebimento VARCHAR(50) NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_receber ADD COLUMN IF NOT EXISTS numero_pedido_referencia VARCHAR(100) NULL;");
        await DropForeignKeyIfExistsAsync(genesisDb, "erp_contas_receber", "FK_erp_contas_receber_Clientes_ClienteId");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_receber ADD COLUMN IF NOT EXISTS ClienteId INT NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("UPDATE erp_contas_receber SET ClienteId = NULL WHERE ClienteId IS NOT NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_receber MODIFY COLUMN ClienteId INT NULL;");

        await genesisDb.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS erp_contas_pagar (
                id INT NOT NULL AUTO_INCREMENT,
                numero_documento VARCHAR(40) NULL,
                fornecedor_id INT NULL,
                descricao VARCHAR(200) NULL,
                valor_original DECIMAL(18,2) NOT NULL DEFAULT 0,
                valor_pago DECIMAL(18,2) NOT NULL DEFAULT 0,
                valor_multa DECIMAL(18,2) NOT NULL DEFAULT 0,
                valor_juros DECIMAL(18,2) NOT NULL DEFAULT 0,
                valor_desconto DECIMAL(18,2) NOT NULL DEFAULT 0,
                data_emissao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                status VARCHAR(50) NULL,
                data_vencimento DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                data_pagamento DATETIME NULL,
                forma_pagamento VARCHAR(50) NULL,
                numero_boleto VARCHAR(100) NULL,
                PRIMARY KEY (id)
            );
            """);

        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_pagar ADD COLUMN IF NOT EXISTS numero_documento VARCHAR(40) NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_pagar ADD COLUMN IF NOT EXISTS fornecedor_id INT NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_pagar ADD COLUMN IF NOT EXISTS descricao VARCHAR(200) NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_pagar ADD COLUMN IF NOT EXISTS valor_original DECIMAL(18,2) NOT NULL DEFAULT 0;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_pagar ADD COLUMN IF NOT EXISTS valor_pago DECIMAL(18,2) NOT NULL DEFAULT 0;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_pagar ADD COLUMN IF NOT EXISTS valor_multa DECIMAL(18,2) NOT NULL DEFAULT 0;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_pagar ADD COLUMN IF NOT EXISTS valor_juros DECIMAL(18,2) NOT NULL DEFAULT 0;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_pagar ADD COLUMN IF NOT EXISTS valor_desconto DECIMAL(18,2) NOT NULL DEFAULT 0;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_pagar ADD COLUMN IF NOT EXISTS data_emissao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_pagar ADD COLUMN IF NOT EXISTS status VARCHAR(50) NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_pagar ADD COLUMN IF NOT EXISTS data_vencimento DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_pagar ADD COLUMN IF NOT EXISTS data_pagamento DATETIME NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_pagar ADD COLUMN IF NOT EXISTS forma_pagamento VARCHAR(50) NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_pagar ADD COLUMN IF NOT EXISTS numero_boleto VARCHAR(100) NULL;");
        await DropForeignKeyIfExistsAsync(genesisDb, "erp_contas_pagar", "FK_erp_contas_pagar_erp_centros_custo_CentroCustoId");
        await DropForeignKeyIfExistsAsync(genesisDb, "erp_contas_pagar", "FK_erp_contas_pagar_erp_fornecedores_FornecedorId");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_pagar ADD COLUMN IF NOT EXISTS CentroCustoId INT NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_pagar ADD COLUMN IF NOT EXISTS FornecedorId INT NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("UPDATE erp_contas_pagar SET CentroCustoId = NULL WHERE CentroCustoId IS NOT NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("UPDATE erp_contas_pagar SET FornecedorId = NULL WHERE FornecedorId IS NOT NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_pagar MODIFY COLUMN CentroCustoId INT NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_contas_pagar MODIFY COLUMN FornecedorId INT NULL;");

        await SyncGenesisReceivablesFromNexumAsync(services, genesisDb);
        await SyncGenesisPayablesFromNexumAsync(services, genesisDb);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Falha ao ajustar schema financeiro compartilhado GenesisGest.Net.");
    }
}

static async Task SyncGenesisReceivablesFromNexumAsync(IServiceProvider services, GenesisDbContext genesisDb)
{
    var nexumDb = services.GetService<NexumDbContext>();
    if (nexumDb is null)
    {
        return;
    }

    var pedidos = await nexumDb.Pedidos
        .AsNoTracking()
        .Include(item => item.Cliente)
        .Where(item => item.Total > 0)
        .OrderByDescending(item => item.CreatedAt)
        .Take(200)
        .ToListAsync();

    foreach (var pedido in pedidos)
    {
        var existe = await genesisDb.ContasReceber
            .AnyAsync(item => item.NumeroPedidoReferencia == pedido.NumeroPedido || item.NumeroDocumento == pedido.NumeroPedido);
        if (existe)
        {
            continue;
        }

        genesisDb.ContasReceber.Add(new GenesisContaReceber
        {
            NumeroDocumento = pedido.NumeroPedido,
            ClienteId = null,
            Descricao = $"Venda online - {pedido.Cliente?.Nome ?? "Cliente Nexum"}",
            ValorOriginal = pedido.Total,
            ValorRecebido = 0,
            ValorMulta = 0,
            ValorJuros = 0,
            ValorDesconto = pedido.Desconto,
            DataEmissao = pedido.CreatedAt,
            DataVencimento = pedido.CreatedAt.AddDays(1),
            Status = "PENDENTE",
            FormaRecebimento = TrimOrNull(pedido.MeioPagamento) ?? "A DEFINIR",
            NumeroPedidoReferencia = pedido.NumeroPedido
        });
    }

    await genesisDb.SaveChangesAsync();
}

static async Task SyncGenesisPayablesFromNexumAsync(IServiceProvider services, GenesisDbContext genesisDb)
{
    var nexumDb = services.GetService<NexumDbContext>();
    if (nexumDb is null)
    {
        return;
    }

    var compras = await nexumDb.Database.SqlQueryRaw<CompraPedidoResumoRow>(
        """
        SELECT
            p.id AS Id,
            p.numero AS Numero,
            p.fornecedor_id AS FornecedorId,
            COALESCE(NULLIF(f.nome_fantasia, ''), f.razao_social, 'Fornecedor Nexum') AS FornecedorNome,
            p.origem AS Origem,
            p.finalidade AS Finalidade,
            p.status AS Status,
            p.status_fiscal AS StatusFiscal,
            p.valor_total AS ValorTotal,
            p.data_prevista_entrega AS DataPrevistaEntrega,
            p.created_at AS CreatedAt
        FROM compras_pedidos p
        LEFT JOIN fornecedores f ON f.id = p.fornecedor_id
        WHERE p.valor_total > 0
        ORDER BY p.created_at DESC
        LIMIT 200
        """)
        .ToListAsync();

    foreach (var compra in compras)
    {
        var existe = await genesisDb.ContasPagar.AnyAsync(item => item.NumeroDocumento == compra.Numero);
        if (existe)
        {
            continue;
        }

        genesisDb.ContasPagar.Add(new GenesisContaPagar
        {
            NumeroDocumento = compra.Numero,
            FornecedorId = compra.FornecedorId,
            Descricao = $"Compra de mercadorias - {compra.FornecedorNome ?? "Fornecedor Nexum"}",
            ValorOriginal = compra.ValorTotal,
            ValorPago = 0,
            ValorMulta = 0,
            ValorJuros = 0,
            ValorDesconto = 0,
            DataEmissao = compra.CreatedAt,
            DataVencimento = compra.DataPrevistaEntrega ?? compra.CreatedAt.AddDays(7),
            Status = compra.Status == "Recebido" ? "ABERTO" : "PENDENTE",
            FormaPagamento = "A DEFINIR",
            NumeroBoleto = null
        });
    }

    await genesisDb.SaveChangesAsync();
}

static async Task DropForeignKeyIfExistsAsync(DbContext db, string tableName, string foreignKeyName)
{
    var connection = db.Database.GetDbConnection();
    var shouldClose = connection.State != ConnectionState.Open;
    if (shouldClose)
    {
        await connection.OpenAsync();
    }

    try
    {
        await using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.TABLE_CONSTRAINTS
            WHERE CONSTRAINT_SCHEMA = DATABASE()
              AND TABLE_NAME = @tableName
              AND CONSTRAINT_NAME = @foreignKeyName
              AND CONSTRAINT_TYPE = 'FOREIGN KEY';
            """;

        var tableParam = checkCommand.CreateParameter();
        tableParam.ParameterName = "@tableName";
        tableParam.Value = tableName;
        checkCommand.Parameters.Add(tableParam);

        var keyParam = checkCommand.CreateParameter();
        keyParam.ParameterName = "@foreignKeyName";
        keyParam.Value = foreignKeyName;
        checkCommand.Parameters.Add(keyParam);

        var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
        if (count == 0)
        {
            return;
        }

        await using var dropCommand = connection.CreateCommand();
        dropCommand.CommandText = $"ALTER TABLE `{tableName}` DROP FOREIGN KEY `{foreignKeyName}`;";
        await dropCommand.ExecuteNonQueryAsync();
    }
    finally
    {
        if (shouldClose)
        {
            await connection.CloseAsync();
        }
    }
}

static async Task EnsurePedidosReceberAsync(NexumDbContext db)
{
    await db.Database.ExecuteSqlRawAsync(
        """
        INSERT INTO financeiro
            (pedido_id, tipo, categoria, descricao, valor, data_vencimento, status, meio_pagamento, observacoes, created_at, updated_at)
        SELECT
            p.id,
            'Receita',
            'Vendas online',
            CONCAT('Conta a receber do pedido ', p.numero_pedido),
            p.total,
            DATE_ADD(COALESCE(p.created_at, UTC_TIMESTAMP()), INTERVAL 1 DAY),
            'Pendente',
            COALESCE(p.meio_pagamento, 'A definir'),
            CONCAT('Gerado automaticamente por reconciliação operacional; pedido=', p.numero_pedido),
            UTC_TIMESTAMP(),
            UTC_TIMESTAMP()
        FROM pedidos p
        WHERE p.total > 0
          AND NOT EXISTS (
              SELECT 1
              FROM financeiro f
              WHERE f.pedido_id = p.id
                AND f.tipo = 'Receita'
          );
        """);
}

static async Task EnsureComprasSchemaAsync(NexumDbContext db)
{
    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS compras_solicitacoes (
            id INT NOT NULL AUTO_INCREMENT,
            produto_id INT NULL,
            produto_nome VARCHAR(200) NOT NULL,
            quantidade_solicitada INT NOT NULL,
            finalidade VARCHAR(120) NOT NULL,
            origem VARCHAR(40) NOT NULL,
            status VARCHAR(40) NOT NULL DEFAULT 'Aberta',
            prioridade VARCHAR(30) NOT NULL DEFAULT 'Normal',
            observacoes TEXT NULL,
            created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            KEY ix_compras_solicitacoes_produto_id (produto_id),
            KEY ix_compras_solicitacoes_status (status),
            KEY ix_compras_solicitacoes_origem (origem)
        );
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS compras_cotacoes (
            id INT NOT NULL AUTO_INCREMENT,
            solicitacao_id INT NULL,
            fornecedor_id INT NOT NULL,
            produto_id INT NULL,
            produto_nome VARCHAR(200) NOT NULL,
            quantidade INT NOT NULL,
            custo_unitario DECIMAL(10,2) NOT NULL,
            valor_total DECIMAL(10,2) NOT NULL,
            prazo_entrega_dias INT NULL,
            origem VARCHAR(40) NOT NULL,
            status VARCHAR(40) NOT NULL DEFAULT 'Recebida',
            observacoes TEXT NULL,
            created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            KEY ix_compras_cotacoes_solicitacao_id (solicitacao_id),
            KEY ix_compras_cotacoes_fornecedor_id (fornecedor_id),
            KEY ix_compras_cotacoes_produto_id (produto_id),
            KEY ix_compras_cotacoes_status (status)
        );
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS compras_pedidos (
            id INT NOT NULL AUTO_INCREMENT,
            numero VARCHAR(40) NOT NULL,
            fornecedor_id INT NOT NULL,
            solicitacao_id INT NULL,
            origem VARCHAR(40) NOT NULL,
            finalidade VARCHAR(120) NOT NULL,
            status VARCHAR(40) NOT NULL DEFAULT 'Aberto',
            status_fiscal VARCHAR(40) NOT NULL DEFAULT 'Pendente',
            valor_total DECIMAL(10,2) NOT NULL DEFAULT 0,
            data_prevista_entrega DATETIME NULL,
            observacoes TEXT NULL,
            created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            UNIQUE KEY ux_compras_pedidos_numero (numero),
            KEY ix_compras_pedidos_fornecedor_id (fornecedor_id),
            KEY ix_compras_pedidos_status (status),
            KEY ix_compras_pedidos_origem (origem)
        );
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS compras_pedido_itens (
            id INT NOT NULL AUTO_INCREMENT,
            compra_pedido_id INT NOT NULL,
            produto_id INT NULL,
            produto_nome VARCHAR(200) NOT NULL,
            sku VARCHAR(80) NULL,
            quantidade INT NOT NULL,
            quantidade_recebida INT NOT NULL DEFAULT 0,
            custo_unitario DECIMAL(10,2) NOT NULL,
            valor_total DECIMAL(10,2) NOT NULL,
            origem VARCHAR(40) NOT NULL,
            finalidade VARCHAR(120) NOT NULL,
            created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            KEY ix_compras_pedido_itens_pedido_id (compra_pedido_id),
            KEY ix_compras_pedido_itens_produto_id (produto_id)
        );
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS compras_entradas (
            id INT NOT NULL AUTO_INCREMENT,
            compra_pedido_id INT NOT NULL,
            fornecedor_id INT NOT NULL,
            numero_documento VARCHAR(80) NULL,
            chave_nfe_entrada VARCHAR(60) NULL,
            tipo_entrada VARCHAR(40) NOT NULL,
            status_fiscal VARCHAR(40) NOT NULL DEFAULT 'FiscalPendente',
            valor_total DECIMAL(10,2) NOT NULL DEFAULT 0,
            recebido_por VARCHAR(120) NULL,
            observacoes TEXT NULL,
            created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            KEY ix_compras_entradas_pedido_id (compra_pedido_id),
            KEY ix_compras_entradas_fornecedor_id (fornecedor_id),
            KEY ix_compras_entradas_status_fiscal (status_fiscal)
        );
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS compras_entrada_itens (
            id INT NOT NULL AUTO_INCREMENT,
            compra_entrada_id INT NOT NULL,
            compra_pedido_item_id INT NOT NULL,
            produto_id INT NULL,
            produto_nome VARCHAR(200) NOT NULL,
            quantidade_recebida INT NOT NULL,
            custo_unitario DECIMAL(10,2) NOT NULL,
            valor_total DECIMAL(10,2) NOT NULL,
            created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            KEY ix_compras_entrada_itens_entrada_id (compra_entrada_id),
            KEY ix_compras_entrada_itens_produto_id (produto_id)
        );
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS estoque_movimentos (
            id INT NOT NULL AUTO_INCREMENT,
            produto_id INT NOT NULL,
            compra_entrada_id INT NULL,
            pedido_id INT NULL,
            tipo VARCHAR(40) NOT NULL,
            quantidade INT NOT NULL,
            saldo_resultante INT NOT NULL,
            custo_unitario DECIMAL(10,2) NULL,
            origem VARCHAR(40) NULL,
            documento VARCHAR(120) NULL,
            observacoes TEXT NULL,
            created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            KEY ix_estoque_movimentos_produto_id (produto_id),
            KEY ix_estoque_movimentos_compra_entrada_id (compra_entrada_id),
            KEY ix_estoque_movimentos_tipo (tipo)
        );
        """);
}

static async Task EnsureGenesisGestOriginalSchemaAsync(NexumDbContext db, ILogger logger)
{
    var candidatePaths = new[]
    {
        Path.Combine(AppContext.BaseDirectory, "Database", "2026-06-29-genesisgest-original-schema.sql"),
        Path.Combine(AppContext.BaseDirectory, "API", "Database", "2026-06-29-genesisgest-original-schema.sql"),
        Path.Combine(Directory.GetCurrentDirectory(), "API", "Database", "2026-06-29-genesisgest-original-schema.sql"),
        Path.Combine(Directory.GetCurrentDirectory(), "NexumAltivon_Back-End", "API", "Database", "2026-06-29-genesisgest-original-schema.sql")
    };

    var schemaPath = candidatePaths.FirstOrDefault(File.Exists);
    if (schemaPath is null)
    {
        logger.LogWarning("Schema GenesisGest.Net original nao encontrado para carga incremental.");
        return;
    }

    var sql = StripSqlComments(await File.ReadAllTextAsync(schemaPath, Encoding.UTF8));
    var applied = 0;

    foreach (var statement in SplitSqlStatements(sql))
    {
        if (string.IsNullOrWhiteSpace(statement))
        {
            continue;
        }

        await ExecuteSqlStatementDirectAsync(db, statement);
        applied++;
    }

    logger.LogInformation("Schema GenesisGest.Net original sincronizado com {StatementCount} comandos seguros.", applied);
}

static async Task ExecuteSqlStatementDirectAsync(NexumDbContext db, string statement)
{
    var connection = db.Database.GetDbConnection();
    if (connection.State != ConnectionState.Open)
    {
        await db.Database.OpenConnectionAsync();
    }

    await using var command = connection.CreateCommand();
    command.CommandText = statement;
    command.CommandTimeout = 180;
    await command.ExecuteNonQueryAsync();
}

static async Task<T> ExecuteScalarAsync<T>(DbContext db, string statement, CancellationToken ct, params (string Name, object? Value)[] parameters)
{
    var connection = db.Database.GetDbConnection();
    var shouldClose = connection.State != ConnectionState.Open;
    if (shouldClose)
    {
        await connection.OpenAsync(ct);
    }

    try
    {
        await using var command = connection.CreateCommand();
        command.CommandText = statement;
        command.CommandTimeout = 60;
        if (db.Database.CurrentTransaction is not null)
        {
            command.Transaction = db.Database.CurrentTransaction.GetDbTransaction();
        }

        foreach (var (name, value) in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        var result = await command.ExecuteScalarAsync(ct);
        if (result is null || result == DBNull.Value)
        {
            return default!;
        }

        return (T)Convert.ChangeType(result, typeof(T), CultureInfo.InvariantCulture);
    }
    finally
    {
        if (shouldClose)
        {
            await connection.CloseAsync();
        }
    }
}

static string StripSqlComments(string sql)
{
    var result = new StringBuilder(sql.Length);
    var inSingleQuote = false;
    var inDoubleQuote = false;
    var inBacktick = false;
    var inBlockComment = false;
    var inLineComment = false;

    for (var i = 0; i < sql.Length; i++)
    {
        var current = sql[i];
        var next = i + 1 < sql.Length ? sql[i + 1] : '\0';

        if (inBlockComment)
        {
            if (current == '*' && next == '/')
            {
                inBlockComment = false;
                i++;
            }
            continue;
        }

        if (inLineComment)
        {
            if (current is '\r' or '\n')
            {
                inLineComment = false;
                result.Append(current);
            }
            continue;
        }

        if (!inSingleQuote && !inDoubleQuote && !inBacktick && current == '/' && next == '*')
        {
            inBlockComment = true;
            i++;
            continue;
        }

        if (!inSingleQuote && !inDoubleQuote && !inBacktick && current == '-' && next == '-')
        {
            inLineComment = true;
            i++;
            continue;
        }

        if (current == '\'' && !inDoubleQuote && !inBacktick)
        {
            inSingleQuote = !inSingleQuote;
        }
        else if (current == '"' && !inSingleQuote && !inBacktick)
        {
            inDoubleQuote = !inDoubleQuote;
        }
        else if (current == '`' && !inSingleQuote && !inDoubleQuote)
        {
            inBacktick = !inBacktick;
        }

        result.Append(current);
    }

    return result.ToString();
}

static IEnumerable<string> SplitSqlStatements(string sql)
{
    var builder = new StringBuilder();
    var inSingleQuote = false;
    var inDoubleQuote = false;
    var inBacktick = false;

    foreach (var current in sql)
    {
        if (current == '\'' && !inDoubleQuote && !inBacktick)
        {
            inSingleQuote = !inSingleQuote;
        }
        else if (current == '"' && !inSingleQuote && !inBacktick)
        {
            inDoubleQuote = !inDoubleQuote;
        }
        else if (current == '`' && !inSingleQuote && !inDoubleQuote)
        {
            inBacktick = !inBacktick;
        }

        if (current == ';' && !inSingleQuote && !inDoubleQuote && !inBacktick)
        {
            var statement = builder.ToString().Trim();
            builder.Clear();

            if (!string.IsNullOrWhiteSpace(statement))
            {
                yield return statement;
            }

            continue;
        }

        builder.Append(current);
    }

    var trailingStatement = builder.ToString().Trim();
    if (!string.IsNullOrWhiteSpace(trailingStatement))
    {
        yield return trailingStatement;
    }
}

static async Task<GenesisGestSchemaStatusDto> BuildGenesisGestSchemaStatusAsync(NexumDbContext db, CancellationToken ct)
{
    var modules = new[]
    {
        ("adm", "Administrativo"),
        ("bi", "Cockpit e BI"),
        ("cfg", "Configuracoes"),
        ("cmp", "Compras e suprimentos"),
        ("cnt", "Contabilidade"),
        ("est", "Estoque e WMS"),
        ("fin", "Financeiro"),
        ("fis", "Fiscal"),
        ("jur", "Juridico"),
        ("log", "Logistica"),
        ("pcp", "Producao"),
        ("rh", "Recursos humanos"),
        ("vnd", "Vendas e CRM"),
        ("vw", "Visoes integradas")
    };

    var status = new List<GenesisGestModuloStatusDto>();

    foreach (var (prefix, name) in modules)
    {
        var count = await ExecuteScalarAsync<int>(
            db,
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND LEFT(table_name, @prefixLength) = @prefix",
            ct,
            ("@prefixLength", prefix.Length + 1),
            ("@prefix", $"{prefix}_"));

        status.Add(new GenesisGestModuloStatusDto(prefix, name, count, count > 0));
    }

    var total = status.Sum(module => module.EstruturasDisponiveis);
    var bridges = await ExecuteScalarAsync<int>(
        db,
        "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name LIKE 'vw_nexum_genesis_%'",
        ct);

    return new GenesisGestSchemaStatusDto(
        GenesisOriginalEstruturasEsperadas: 125,
        EstruturasGenesisDisponiveis: total,
        PontesNexumGenesisDisponiveis: bridges,
        Sincronizado: total >= 125 && bridges >= 3,
        Modulos: status);
}

static async Task EnsureUsuariosSchemaAsync(NexumDbContext db)
{
    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS usuarios (
            id INT NOT NULL AUTO_INCREMENT,
            nome VARCHAR(150) NOT NULL,
            email VARCHAR(150) NOT NULL,
            senha_hash VARCHAR(255) NOT NULL,
            perfil VARCHAR(30) NOT NULL DEFAULT 'Vendedor',
            avatar VARCHAR(255) NULL,
            telefone VARCHAR(20) NULL,
            ativo TINYINT(1) NOT NULL DEFAULT 1,
            ultimo_login DATETIME NULL,
            token_refresh VARCHAR(255) NULL,
            created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            UNIQUE KEY ux_usuarios_email (email)
        );
        """);

    await db.Database.ExecuteSqlRawAsync("ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS senha_hash VARCHAR(255) NOT NULL DEFAULT '';");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS perfil VARCHAR(30) NOT NULL DEFAULT 'Vendedor';");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS ativo TINYINT(1) NOT NULL DEFAULT 1;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS ultimo_login DATETIME NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS token_refresh VARCHAR(255) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;");
}

static async Task EnsureSystemAdminUsersAsync(NexumDbContext db, IConfiguration configuration, ILogger logger)
{
    var admin = configuration.GetSection("AdminUser");
    var email = NormalizeEmail(admin["Email"] ?? "admin@nexumaltivon.com");
    var password = admin["Password"];
    var name = TrimOrNull(admin["Name"]) ?? "Administrador Nexum";
    var roleRaw = TrimOrNull(admin["Role"]) ?? "Gerente";

    if (string.IsNullOrWhiteSpace(email) || !IsConfiguredSecret(password))
    {
        logger.LogWarning("AdminUser nao foi gravado no banco porque email ou senha de ambiente nao estao configurados.");
        return;
    }

    var role = Enum.TryParse<PerfilUsuario>(roleRaw, true, out var parsedRole)
        ? parsedRole
        : PerfilUsuario.Gerente;

    if (string.Equals(email, "admin@nexumaltivon.com", StringComparison.OrdinalIgnoreCase)
        && role is not PerfilUsuario.Admin
        && role is not PerfilUsuario.SuperAdmin)
    {
        role = PerfilUsuario.Admin;
    }

    var usuario = await db.Usuarios.FirstOrDefaultAsync(item => item.Email == email);
    if (usuario is null)
    {
        db.Usuarios.Add(new Usuario
        {
            Nome = name,
            Email = email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(password, 12),
            Perfil = role,
            Ativo = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }
    else
    {
        usuario.Nome = name;
        usuario.Perfil = role;
        usuario.Ativo = true;
        usuario.UpdatedAt = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(usuario.SenhaHash) || !BCrypt.Net.BCrypt.Verify(password, usuario.SenhaHash))
        {
            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(password, 12);
        }
    }

    await db.SaveChangesAsync();
}

static async Task EnsureValidationTokensAsync(NexumDbContext db)
{
    var tokens = new (string Key, string Hash, string Description)[]
    {
        ("validacao_token_01", "6135f83d946d18267f24118fee932fe807554f55da6cee693b4a8db131662297", "TOKEN01 - Gabriel/Rafael/Miguel/Castiel"),
        ("validacao_token_02", "512cddda6828e66ba75f47b0f64f3433a2267b61faaa5685270154adba3ecc8a", "TOKEN02 - Yara/Rodrigo/Vinicius/Sophia"),
        ("validacao_token_03", "d72000837011904c56211687ac8ac1e72004de9c29338ddbf379f9e63f91facd", "TOKEN03 - Nexum/Chronnus/Estruturaline/Altivon")
    };

    foreach (var token in tokens)
    {
        var config = await db.ConfiguracoesSistema.FirstOrDefaultAsync(item => item.Chave == token.Key);
        if (config is null)
        {
            db.ConfiguracoesSistema.Add(new ConfiguracaoSistema
            {
                Chave = token.Key,
                Valor = token.Hash,
                Tipo = TipoConfiguracao.Senha,
                Descricao = token.Description,
                Grupo = "Credenciais",
                Editavel = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            config.Valor = token.Hash;
            config.Tipo = TipoConfiguracao.Senha;
            config.Descricao = token.Description;
            config.Grupo = "Credenciais";
            config.Editavel = false;
            config.UpdatedAt = DateTime.UtcNow;
        }
    }

    await db.SaveChangesAsync();
}

static IQueryable<Produto> ProdutosPublicaveisDashboard(NexumDbContext db) =>
    db.Produtos.AsNoTracking().Where(produto =>
        produto.Ativo &&
        produto.LojaId > 0 &&
        produto.CategoriaId.HasValue &&
        !string.IsNullOrEmpty(produto.Nome) &&
        !string.IsNullOrEmpty(produto.Sku) &&
        !string.IsNullOrEmpty(produto.Slug) &&
        (!string.IsNullOrEmpty(produto.DescricaoCurta) || !string.IsNullOrEmpty(produto.DescricaoLonga)) &&
        !string.IsNullOrEmpty(produto.ImagemPrincipal) &&
        produto.Preco > 0 &&
        produto.Peso > 0 &&
        produto.Altura > 0 &&
        produto.Largura > 0 &&
        produto.Comprimento > 0);

static async Task<ComprasPainelDto> BuildComprasPainelAsync(NexumDbContext db, CancellationToken ct)
{
    var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

    var solicitacoesAbertas = await ExecuteScalarAsync<int>(
        db,
        "SELECT COUNT(*) FROM compras_solicitacoes WHERE status IN ('Aberta', 'Cotado')",
        ct);

    var pedidosAbertos = await ExecuteScalarAsync<int>(
        db,
        "SELECT COUNT(*) FROM compras_pedidos WHERE status IN ('Aberto', 'RecebidoParcial')",
        ct);

    var entradasMes = await ExecuteScalarAsync<int>(
        db,
        "SELECT COUNT(*) FROM compras_entradas WHERE created_at >= @inicioMes",
        ct,
        ("@inicioMes", inicioMes));

    var valorComprasAbertas = await ExecuteScalarAsync<decimal>(
        db,
        "SELECT COALESCE(SUM(valor_total), 0) FROM compras_pedidos WHERE status IN ('Aberto', 'RecebidoParcial')",
        ct);

    var contasAPagarCompras = await db.Financeiros.AsNoTracking()
        .CountAsync(item => item.Tipo == TipoLancamento.Despesa &&
            item.Status == StatusLancamento.Pendente &&
            item.Categoria == "Compras de mercadorias", ct);

    var fornecedoresAtivos = await db.Fornecedores.AsNoTracking()
        .CountAsync(item => item.Status == StatusFornecedor.Ativo, ct);

    var produtosReposicao = await db.Produtos.AsNoTracking()
        .Where(produto => produto.Ativo &&
            (produto.EstoqueAtual <= produto.EstoqueMinimo || produto.TipoProduto == TipoProduto.Dropshipping || produto.TipoProduto == TipoProduto.Marketplace))
        .OrderBy(produto => produto.EstoqueAtual - produto.EstoqueMinimo)
        .ThenBy(produto => produto.Nome)
        .Take(60)
        .Select(produto => new CompraProdutoReposicaoDto(
            produto.Id,
            produto.Nome,
            produto.Sku,
            produto.EstoqueAtual,
            produto.EstoqueMinimo,
            produto.TipoProduto.ToString(),
            produto.FornecedorId,
            produto.Custo))
        .ToListAsync(ct);

    var fornecedores = await db.Fornecedores.AsNoTracking()
        .Where(fornecedor => fornecedor.Status == StatusFornecedor.Ativo)
        .OrderBy(fornecedor => fornecedor.RazaoSocial)
        .Take(100)
        .Select(fornecedor => new CompraFornecedorResumoDto(
            fornecedor.Id,
            string.IsNullOrWhiteSpace(fornecedor.NomeFantasia) ? fornecedor.RazaoSocial : fornecedor.NomeFantasia,
            fornecedor.Cnpj,
            fornecedor.Segmento,
            fornecedor.PrazoEntregaDias,
            7))
        .ToListAsync(ct);

    var solicitacoes = await db.Database.SqlQueryRaw<CompraSolicitacaoRow>(
        """
        SELECT
            s.id AS Id,
            s.produto_id AS ProdutoId,
            s.produto_nome AS ProdutoNome,
            s.quantidade_solicitada AS Quantidade,
            s.finalidade AS Finalidade,
            s.origem AS Origem,
            s.status AS Status,
            s.prioridade AS Prioridade,
            s.created_at AS CreatedAt
        FROM compras_solicitacoes s
        ORDER BY s.created_at DESC
        LIMIT 50
        """)
        .ToListAsync(ct);

    var pedidos = await db.Database.SqlQueryRaw<CompraPedidoResumoRow>(
        """
        SELECT
            p.id AS Id,
            p.numero AS Numero,
            p.fornecedor_id AS FornecedorId,
            COALESCE(NULLIF(f.nome_fantasia, ''), f.razao_social) AS FornecedorNome,
            p.origem AS Origem,
            p.finalidade AS Finalidade,
            p.status AS Status,
            p.status_fiscal AS StatusFiscal,
            p.valor_total AS ValorTotal,
            p.data_prevista_entrega AS DataPrevistaEntrega,
            p.created_at AS CreatedAt
        FROM compras_pedidos p
        LEFT JOIN fornecedores f ON f.id = p.fornecedor_id
        ORDER BY p.created_at DESC
        LIMIT 50
        """)
        .ToListAsync(ct);

    var pedidoItens = await db.Database.SqlQueryRaw<CompraPedidoItemResumoRow>(
        """
        SELECT
            i.compra_pedido_id AS CompraPedidoId,
            i.id AS Id,
            i.produto_id AS ProdutoId,
            i.produto_nome AS ProdutoNome,
            i.sku AS Sku,
            i.quantidade AS Quantidade,
            i.quantidade_recebida AS QuantidadeRecebida,
            i.custo_unitario AS CustoUnitario,
            i.valor_total AS ValorTotal,
            i.origem AS Origem,
            i.finalidade AS Finalidade
        FROM compras_pedido_itens i
        INNER JOIN compras_pedidos p ON p.id = i.compra_pedido_id
        ORDER BY p.created_at DESC, i.id ASC
        LIMIT 250
        """)
        .ToListAsync(ct);

    var itensPorPedido = pedidoItens
        .GroupBy(item => item.CompraPedidoId)
        .ToDictionary(
            grupo => grupo.Key,
            grupo => grupo.Select(item => new CompraPedidoItemResumoDto(
                item.Id,
                item.ProdutoId,
                item.ProdutoNome,
                item.Sku,
                item.Quantidade,
                item.QuantidadeRecebida,
                Math.Max(0, item.Quantidade - item.QuantidadeRecebida),
                item.CustoUnitario,
                item.ValorTotal,
                item.Origem,
                item.Finalidade)).ToList());

    var entradas = await db.Database.SqlQueryRaw<CompraEntradaResumoRow>(
        """
        SELECT
            e.id AS Id,
            e.compra_pedido_id AS CompraPedidoId,
            p.numero AS PedidoNumero,
            COALESCE(NULLIF(f.nome_fantasia, ''), f.razao_social) AS FornecedorNome,
            e.numero_documento AS NumeroDocumento,
            e.chave_nfe_entrada AS ChaveNfeEntrada,
            e.tipo_entrada AS TipoEntrada,
            e.status_fiscal AS StatusFiscal,
            e.valor_total AS ValorTotal,
            e.created_at AS CreatedAt
        FROM compras_entradas e
        LEFT JOIN compras_pedidos p ON p.id = e.compra_pedido_id
        LEFT JOIN fornecedores f ON f.id = e.fornecedor_id
        ORDER BY e.created_at DESC
        LIMIT 50
        """)
        .ToListAsync(ct);

    var alertas = new List<string>();
    if (produtosReposicao.Count > 0)
    {
        alertas.Add($"{produtosReposicao.Count} produto(s) exigem reposicao, vinculo de fornecedor ou controle de dropshipping.");
    }
    if (fornecedoresAtivos == 0)
    {
        alertas.Add("Nenhum fornecedor ativo cadastrado para compras.");
    }
    if (contasAPagarCompras > 0)
    {
        alertas.Add($"{contasAPagarCompras} conta(s) a pagar geradas por compras aguardam baixa.");
    }

    return new ComprasPainelDto(
        new ComprasKpiDto(
            solicitacoesAbertas,
            pedidosAbertos,
            entradasMes,
            valorComprasAbertas,
            contasAPagarCompras,
            fornecedoresAtivos,
            produtosReposicao.Count),
        alertas,
        solicitacoes.Select(item => new CompraSolicitacaoDto(
            item.Id,
            item.ProdutoId,
            item.ProdutoNome,
            item.Quantidade,
            item.Finalidade,
            item.Origem,
            item.Status,
            item.Prioridade,
            item.CreatedAt)).ToList(),
        pedidos.Select(item => new CompraPedidoResumoDto(
            item.Id,
            item.Numero,
            item.FornecedorId,
            item.FornecedorNome ?? "Fornecedor nao identificado",
            item.Origem,
            item.Finalidade,
            item.Status,
            item.StatusFiscal,
            item.ValorTotal,
            item.DataPrevistaEntrega,
            item.CreatedAt,
            itensPorPedido.TryGetValue(item.Id, out var itens) ? itens : [])).ToList(),
        entradas.Select(item => new CompraEntradaResumoDto(
            item.Id,
            item.CompraPedidoId,
            item.PedidoNumero ?? string.Empty,
            item.FornecedorNome ?? "Fornecedor nao identificado",
            item.NumeroDocumento,
            item.ChaveNfeEntrada,
            item.TipoEntrada,
            item.StatusFiscal,
            item.ValorTotal,
            item.CreatedAt)).ToList(),
        produtosReposicao,
        fornecedores);
}

static async Task<DashboardKpiDto> BuildAdminKpisAsync(NexumDbContext db, CancellationToken ct)
{
    var hoje = DateTime.UtcNow.Date;
    var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
    var inicioAno = new DateTime(hoje.Year, 1, 1);

    var pedidosValidos = db.Pedidos.AsNoTracking()
        .Where(pedido => pedido.Status != StatusPedido.Cancelado && pedido.Status != StatusPedido.Reembolsado);

    var faturamentoHoje = await pedidosValidos
        .Where(pedido => pedido.CreatedAt >= hoje)
        .SumAsync(pedido => (decimal?)pedido.Total, ct) ?? 0m;

    var faturamentoMes = await pedidosValidos
        .Where(pedido => pedido.CreatedAt >= inicioMes)
        .SumAsync(pedido => (decimal?)pedido.Total, ct) ?? 0m;

    var faturamentoAno = await pedidosValidos
        .Where(pedido => pedido.CreatedAt >= inicioAno)
        .SumAsync(pedido => (decimal?)pedido.Total, ct) ?? 0m;

    var totalPedidosValidos = await pedidosValidos.CountAsync(ct);
    var faturamentoTotal = await pedidosValidos.SumAsync(pedido => (decimal?)pedido.Total, ct) ?? 0m;
    var ticketMedio = totalPedidosValidos > 0 ? Math.Round(faturamentoTotal / totalPedidosValidos, 2) : 0m;

    return new DashboardKpiDto(
        faturamentoHoje,
        faturamentoMes,
        faturamentoAno,
        await pedidosValidos.CountAsync(pedido => pedido.CreatedAt >= hoje, ct),
        await pedidosValidos.CountAsync(pedido => pedido.CreatedAt >= inicioMes, ct),
        await db.Pedidos.AsNoTracking().CountAsync(pedido => pedido.Status == StatusPedido.Pendente, ct),
        await db.Pedidos.AsNoTracking().CountAsync(pedido => pedido.Status == StatusPedido.Enviado, ct),
        await db.Pedidos.AsNoTracking().CountAsync(pedido => pedido.Status == StatusPedido.Entregue, ct),
        await db.Clientes.AsNoTracking().CountAsync(cliente => cliente.CreatedAt >= inicioMes, ct),
        await db.Clientes.AsNoTracking().CountAsync(cliente => cliente.Status == StatusCliente.Ativo, ct),
        await db.Clientes.AsNoTracking().CountAsync(ct),
        ticketMedio,
        0m,
        await ProdutosPublicaveisDashboard(db).CountAsync(ct),
        await ProdutosPublicaveisDashboard(db).CountAsync(produto => produto.EstoqueAtual > 0 && produto.EstoqueAtual <= produto.EstoqueMinimo, ct),
        await ProdutosPublicaveisDashboard(db).CountAsync(produto => produto.EstoqueAtual == 0, ct),
        await db.CrmLeads.AsNoTracking().CountAsync(lead => lead.Status == StatusLead.Novo, ct),
        await db.CrmLeads.AsNoTracking().CountAsync(lead => lead.Status == StatusLead.Convertido, ct),
        await db.CrmLeads.AsNoTracking().CountAsync(lead => lead.Status == StatusLead.EmAtendimento, ct));
}

static async Task<DashboardCompletoDto> BuildAdminDashboardAsync(NexumDbContext db, CancellationToken ct)
{
    var hoje = DateTime.UtcNow.Date;
    var pedidosValidos = db.Pedidos.AsNoTracking()
        .Where(pedido => pedido.Status != StatusPedido.Cancelado && pedido.Status != StatusPedido.Reembolsado);

    var faturamentoSemanal = new List<FaturamentoPorPeriodoDto>();
    for (var i = 6; i >= 0; i--)
    {
        var dia = hoje.AddDays(-i);
        var fimDia = dia.AddDays(1);
        var pedidosDia = pedidosValidos.Where(pedido => pedido.CreatedAt >= dia && pedido.CreatedAt < fimDia);
        faturamentoSemanal.Add(new FaturamentoPorPeriodoDto(
            dia.ToString("dd/MM", CultureInfo.InvariantCulture),
            await pedidosDia.SumAsync(pedido => (decimal?)pedido.Total, ct) ?? 0m,
            await pedidosDia.CountAsync(ct)));
    }

    var faturamentoMensal = new List<FaturamentoPorPeriodoDto>();
    for (var i = 11; i >= 0; i--)
    {
        var mes = hoje.AddMonths(-i);
        var inicioMes = new DateTime(mes.Year, mes.Month, 1);
        var fimMes = inicioMes.AddMonths(1);
        var pedidosMes = pedidosValidos.Where(pedido => pedido.CreatedAt >= inicioMes && pedido.CreatedAt < fimMes);
        faturamentoMensal.Add(new FaturamentoPorPeriodoDto(
            mes.ToString("MMM/yy", CultureInfo.InvariantCulture),
            await pedidosMes.SumAsync(pedido => (decimal?)pedido.Total, ct) ?? 0m,
            await pedidosMes.CountAsync(ct)));
    }

    var dataInicioLojas = hoje.AddMonths(-1);
    var pedidosPorLoja = await pedidosValidos
        .Include(pedido => pedido.Loja)
        .Where(pedido => pedido.CreatedAt >= dataInicioLojas)
        .ToListAsync(ct);
    var totalLojas = pedidosPorLoja.Sum(pedido => pedido.Total);
    var vendasPorLoja = pedidosPorLoja
        .GroupBy(pedido => new { pedido.LojaId, Nome = pedido.Loja?.Nome ?? "Sem Loja", Slug = pedido.Loja?.Slug ?? "" })
        .Select(grupo =>
        {
            var faturamento = grupo.Sum(pedido => pedido.Total);
            var pedidos = grupo.Count();
            return new VendasPorLojaDto(
                grupo.Key.Nome,
                grupo.Key.Slug,
                faturamento,
                pedidos,
                pedidos > 0 ? Math.Round(faturamento / pedidos, 2) : 0m,
                totalLojas > 0 ? Math.Round(faturamento / totalLojas * 100m, 2) : 0m);
        })
        .OrderByDescending(item => item.Faturamento)
        .ToList();

    var itensVendidos = await db.PedidoItens.AsNoTracking()
        .Include(item => item.Pedido)
        .Include(item => item.Produto)
        .ThenInclude(produto => produto!.Loja)
        .Where(item => item.Pedido != null &&
            item.Pedido.Status != StatusPedido.Cancelado &&
            item.Pedido.Status != StatusPedido.Reembolsado)
        .ToListAsync(ct);
    var produtosMaisVendidos = itensVendidos
        .GroupBy(item => new
        {
            ProdutoId = item.ProdutoId ?? 0,
            item.NomeProduto,
            item.ImagemProduto,
            LojaNome = item.Produto?.Loja?.Nome ?? "Sem Loja"
        })
        .Select(grupo => new ProdutosMaisVendidosDto(
            grupo.Key.ProdutoId,
            grupo.Key.NomeProduto,
            grupo.Key.ImagemProduto,
            grupo.Key.LojaNome,
            grupo.Sum(item => item.Quantidade),
            grupo.Sum(item => item.PrecoTotal)))
        .OrderByDescending(item => item.QuantidadeVendida)
        .Take(10)
        .ToList();

    var clientesBase = await db.Clientes.AsNoTracking()
        .OrderByDescending(cliente => cliente.CreatedAt)
        .Take(10)
        .ToListAsync(ct);
    var clienteIds = clientesBase.Select(cliente => cliente.Id).ToList();
    var pedidosClientes = await pedidosValidos
        .Where(pedido => clienteIds.Contains(pedido.ClienteId))
        .ToListAsync(ct);
    var clientesRecentes = clientesBase
        .Select(cliente =>
        {
            var pedidosCliente = pedidosClientes.Where(pedido => pedido.ClienteId == cliente.Id).ToList();
            return new ClientesRecentesDto(
                cliente.Id,
                cliente.Nome,
                cliente.Email,
                cliente.Whatsapp,
                cliente.CreatedAt,
                pedidosCliente.Count,
                pedidosCliente.Sum(pedido => pedido.Total));
        })
        .ToList();

    var pedidosRecentes = await db.Pedidos.AsNoTracking()
        .Include(pedido => pedido.Cliente)
        .Include(pedido => pedido.Loja)
        .OrderByDescending(pedido => pedido.CreatedAt)
        .Take(10)
        .Select(pedido => new PedidosRecentesDto(
            pedido.Id,
            pedido.NumeroPedido,
            pedido.Cliente != null ? pedido.Cliente.Nome : "Cliente nao identificado",
            pedido.Total,
            pedido.Status.ToString(),
            pedido.StatusPagamento.ToString(),
            pedido.Loja != null ? pedido.Loja.Nome : null,
            pedido.CreatedAt))
        .ToListAsync(ct);

    var leadsRecentes = await db.CrmLeads.AsNoTracking()
        .OrderByDescending(lead => lead.CreatedAt)
        .Take(10)
        .Select(lead => new LeadsRecentesDto(
            lead.Id,
            lead.Nome,
            lead.Tipo.ToString(),
            lead.Status.ToString(),
            lead.Prioridade.ToString(),
            lead.Email,
            lead.Whatsapp,
            lead.CreatedAt))
        .ToListAsync(ct);

    return new DashboardCompletoDto(
        await BuildAdminKpisAsync(db, ct),
        faturamentoSemanal,
        faturamentoMensal,
        vendasPorLoja,
        produtosMaisVendidos,
        clientesRecentes,
        pedidosRecentes,
        leadsRecentes);
}

Console.WriteLine("[NexumStartup] Iniciando servidor HTTP.");
app.Run();

public sealed record LoginRequest(string Email, string Senha);

public sealed record LoginResponse(string Token, string RefreshToken, DateTime ExpiraEm, UsuarioDto Usuario);

public sealed record UsuarioDto(int Id, string Nome, string Email, string Perfil);

public sealed record ValidacaoTokenRequest(string Token);

public sealed record ValidacaoTokenResponse(string Codigo, string Descricao);

public sealed record CredencialSistemaStatusDto(string Codigo, bool Configurada, string? Descricao, DateTime UpdatedAt);

public sealed record UsuarioPerfilStatusDto(string Perfil, int UsuariosAtivos);

public sealed record CredenciaisSistemaStatusDto(
    List<CredencialSistemaStatusDto> TokensValidacao,
    List<UsuarioPerfilStatusDto> UsuariosPorPerfil);

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

public sealed record FinanceiroLancamentoDto(
    int Id,
    int? PedidoId,
    string? NumeroPedido,
    string Tipo,
    string Status,
    string? Categoria,
    string? Descricao,
    decimal Valor,
    DateTime? DataVencimento,
    DateTime? DataPagamento,
    string? MeioPagamento,
    string? ContaBancaria,
    string? Observacoes,
    DateTime CreatedAt);

public sealed record FinanceiroLancamentoRequest(
    string Tipo,
    string? Status,
    string? Categoria,
    string? Descricao,
    decimal Valor,
    DateTime? DataVencimento,
    DateTime? DataPagamento,
    string? MeioPagamento,
    string? ContaBancaria,
    string? ComprovanteUrl,
    string? Observacoes,
    int? PedidoId);

public sealed record FinanceiroLancamentoStatusRequest(
    string Status,
    DateTime? DataPagamento,
    string? MeioPagamento,
    string? ContaBancaria,
    string? ComprovanteUrl,
    string? Observacoes);

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
    string? ImagensGaleria,
    string? CodigoBarras = null,
    string? QrCode = null,
    string? IdentificacaoEstoque = null);

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
    string? ImagensGaleria,
    string? EmpresaAquisicaoCodigo = null)
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
            ImagemUrl ?? string.Empty,
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

public sealed record ClienteRequest(
    string Nome,
    string Email,
    string? Cpf,
    string? Telefone,
    string? Senha = null,
    bool? Newsletter = null,
    string? CpfCnpj = null,
    string? RgIe = null,
    DateTime? DataNascimento = null,
    string? Whatsapp = null,
    string? Avatar = null,
    bool? Vip = null,
    int? PontosFidelidade = null,
    string? Status = null,
    string? Tipo = null);

public sealed record ClienteLojaDto(
    int Id,
    string Nome,
    string Email,
    string? Telefone,
    string? Cpf = null,
    string? Tipo = null,
    string? RgIe = null,
    DateTime? DataNascimento = null,
    string? Whatsapp = null,
    string? Avatar = null,
    bool Newsletter = true,
    bool Vip = false,
    int PontosFidelidade = 0,
    string? Status = null,
    DateTime? UltimoAcesso = null,
    DateTime? ConfirmadoEm = null,
    DateTime? CreatedAt = null,
    DateTime? UpdatedAt = null);

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
    string StatusCadastro,
    DateTime? ConfirmadoEm,
    int PontosFidelidade,
    string ScoreRelacionamento,
    bool Vip,
    decimal LimiteFuturoEstimado,
    List<ClientePortalPedidoDto> Pedidos,
    List<ClientePortalDocumentoDto> Documentos,
    List<ClientePortalEnderecoDto> Enderecos,
    List<string> Beneficios);

public sealed record ClientePortalEnderecoDto(
    int Id,
    string Apelido,
    string Tipo,
    string Cep,
    string Logradouro,
    string Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado,
    string Pais,
    bool Padrao);

public sealed record ClientePortalEnderecoRequest(
    string? Apelido,
    string? Tipo,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado,
    string? Pais,
    bool Padrao);

public sealed record FornecedorRequest(
    string Nome,
    string? Documento,
    string? Email,
    string? Telefone,
    string? Categoria,
    string? NomeFantasia = null,
    string? Ie = null,
    string? Whatsapp = null,
    string? Endereco = null,
    string? Cidade = null,
    string? Estado = null,
    string? Cep = null,
    int? LojaVinculadaId = null,
    decimal? ComissaoPercentual = null,
    int? PrazoEntregaDias = null,
    string? Status = null,
    string? Observacoes = null);

public sealed record FornecedorDto(
    int Id,
    string Nome,
    string Documento,
    string Email,
    string Telefone,
    string Categoria,
    DateTime CreatedAt,
    string? RazaoSocial = null,
    string? NomeFantasia = null,
    string? Ie = null,
    string? Whatsapp = null,
    string? Endereco = null,
    string? Cidade = null,
    string? Estado = null,
    string? Cep = null,
    int? LojaVinculadaId = null,
    decimal ComissaoPercentual = 0m,
    int PrazoEntregaDias = 7,
    string? Status = null,
    string? Observacoes = null,
    DateTime? UpdatedAt = null);

public sealed record CompraSolicitacaoRequest(
    int? ProdutoId,
    string? ProdutoNome,
    int Quantidade,
    string? Origem,
    string? Finalidade,
    string? Prioridade,
    string? Observacoes);

public sealed record CompraCotacaoRequest(
    int FornecedorId,
    int? ProdutoId,
    string? ProdutoNome,
    int Quantidade,
    decimal CustoUnitario,
    string? Origem,
    string? Finalidade,
    string? Prioridade,
    int? PrazoEntregaDias,
    string? Observacoes);

public sealed record CompraPedidoRequest(
    int FornecedorId,
    int? SolicitacaoId,
    string? Origem,
    string? Finalidade,
    DateTime? DataPrevistaEntrega,
    DateTime? DataVencimento,
    string? MeioPagamento,
    string? Observacoes,
    List<CompraPedidoItemRequest> Itens);

public sealed record CompraPedidoItemRequest(
    int? ProdutoId,
    string? ProdutoNome,
    string? Sku,
    int Quantidade,
    decimal CustoUnitario);

public sealed record CompraEntradaRequest(
    string? NumeroDocumento,
    string? ChaveNfeEntrada,
    string? TipoEntrada,
    string? RecebidoPor,
    string? Observacoes,
    List<CompraEntradaItemRequest>? Itens);

public sealed record CompraEntradaItemRequest(int ItemId, int QuantidadeRecebida);

public sealed class CompraStatusRequest
{
    public string? Status { get; set; }

    [JsonPropertyName("novo_status")]
    public string? NovoStatus { get; set; }

    public string? Observacoes { get; set; }
}

public sealed record ComprasPainelDto(
    ComprasKpiDto Kpis,
    List<string> Alertas,
    List<CompraSolicitacaoDto> Solicitacoes,
    List<CompraPedidoResumoDto> Pedidos,
    List<CompraEntradaResumoDto> Entradas,
    List<CompraProdutoReposicaoDto> ProdutosReposicao,
    List<CompraFornecedorResumoDto> Fornecedores);

public sealed record ComprasKpiDto(
    int SolicitacoesAbertas,
    int PedidosAbertos,
    int EntradasMes,
    decimal ValorComprasAbertas,
    int ContasAPagarCompras,
    int FornecedoresAtivos,
    int ProdutosReposicao);

public sealed record CompraSolicitacaoDto(
    int Id,
    int? ProdutoId,
    string ProdutoNome,
    int Quantidade,
    string Finalidade,
    string Origem,
    string Status,
    string Prioridade,
    DateTime CreatedAt);

public sealed record CompraPedidoResumoDto(
    int Id,
    string Numero,
    int FornecedorId,
    string FornecedorNome,
    string Origem,
    string Finalidade,
    string Status,
    string StatusFiscal,
    decimal ValorTotal,
    DateTime? DataPrevistaEntrega,
    DateTime CreatedAt,
    List<CompraPedidoItemResumoDto> Itens);

public sealed record CompraPedidoItemResumoDto(
    int Id,
    int? ProdutoId,
    string ProdutoNome,
    string? Sku,
    int Quantidade,
    int QuantidadeRecebida,
    int QuantidadePendente,
    decimal CustoUnitario,
    decimal ValorTotal,
    string Origem,
    string Finalidade);

public sealed record CompraEntradaResumoDto(
    int Id,
    int CompraPedidoId,
    string PedidoNumero,
    string FornecedorNome,
    string? NumeroDocumento,
    string? ChaveNfeEntrada,
    string TipoEntrada,
    string StatusFiscal,
    decimal ValorTotal,
    DateTime CreatedAt);

public sealed record CompraProdutoReposicaoDto(
    int ProdutoId,
    string ProdutoNome,
    string Sku,
    int EstoqueAtual,
    int EstoqueMinimo,
    string TipoProduto,
    int? FornecedorId,
    decimal CustoAtual);

public sealed record CompraFornecedorResumoDto(
    int Id,
    string Nome,
    string? Documento,
    string? Segmento,
    int PrazoEntregaDias,
    int PrazoPagamentoDias);

public sealed class CompraSolicitacaoRow
{
    public int Id { get; set; }
    public int? ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public string Finalidade { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Prioridade { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class CompraPedidoResumoRow
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int FornecedorId { get; set; }
    public string? FornecedorNome { get; set; }
    public string Origem { get; set; } = string.Empty;
    public string Finalidade { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusFiscal { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public DateTime? DataPrevistaEntrega { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class CompraPedidoItemResumoRow
{
    public int CompraPedidoId { get; set; }
    public int Id { get; set; }
    public int? ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public int Quantidade { get; set; }
    public int QuantidadeRecebida { get; set; }
    public decimal CustoUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public string Origem { get; set; } = string.Empty;
    public string Finalidade { get; set; } = string.Empty;
}

public sealed class CompraEntradaResumoRow
{
    public int Id { get; set; }
    public int CompraPedidoId { get; set; }
    public string? PedidoNumero { get; set; }
    public string? FornecedorNome { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? ChaveNfeEntrada { get; set; }
    public string TipoEntrada { get; set; } = string.Empty;
    public string StatusFiscal { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class CompraPedidoLookupRow
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public int FornecedorId { get; set; }
}

public sealed class CompraPedidoItemLookupRow
{
    public int Id { get; set; }
    public int? ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public int QuantidadeRecebida { get; set; }
    public decimal CustoUnitario { get; set; }
}

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
    [property: JsonPropertyName("dados_cartao")] object? DadosCartao,
    [property: JsonPropertyName("frete_valor")] decimal? FreteValor,
    [property: JsonPropertyName("frete_metodo")] string? FreteMetodo,
    [property: JsonPropertyName("frete_transportadora")] string? FreteTransportadora,
    [property: JsonPropertyName("frete_prazo_dias")] int? FretePrazoDias);

public sealed record DadosCartaoPedidoRequest(
    [property: JsonPropertyName("numero")] string Numero,
    [property: JsonPropertyName("nomeTitular")] string NomeTitular,
    [property: JsonPropertyName("validade")] string Validade,
    [property: JsonPropertyName("cvv")] string Cvv,
    [property: JsonPropertyName("cpfTitular")] string? CpfTitular);

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
    string? FreteCodigoRastreio,
    string InstrucaoPagamento,
    int Parcelas,
    string? PixQrcode,
    string? PaymentUrl);

public sealed record PedidoLogisticaRequest(
    [property: JsonPropertyName("frete_metodo")] string? FreteMetodo,
    [property: JsonPropertyName("frete_transportadora")] string? FreteTransportadora,
    [property: JsonPropertyName("frete_prazo_dias")] int? FretePrazoDias,
    [property: JsonPropertyName("frete_codigo_rastreio")] string? FreteCodigoRastreio);

public sealed record PedidoAcompanhamentoDto(
    int Id,
    string NumeroPedido,
    string ClienteNome,
    decimal Total,
    string Status,
    string StatusPagamento,
    string? MeioPagamento,
    string? GatewayPagamento,
    string? FreteMetodo,
    string? FreteTransportadora,
    int FretePrazoDias,
    string? FreteCodigoRastreio,
    string InstrucaoPagamento,
    string? PaymentUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt);

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

public sealed record StoreCardSiteDto(
    string Nome,
    string Slug,
    string Segmento,
    string Descricao,
    string Imagem,
    string Icon);

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
    string SiteSubtitulo,
    string InstitutionalUrl,
    string PrivacyUrl,
    string RefundUrl,
    List<HeroSlideSiteDto> HeroSlides,
    List<StoreCardSiteDto> StoreCards,
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

public sealed record LogisticaRoteamentoRequest(
    [property: JsonPropertyName("cep_origem")]
    string? CepOrigem,
    [property: JsonPropertyName("cep_destino")]
    string? CepDestino,
    [property: JsonPropertyName("itens")]
    List<FreteCotacaoItemRequest> Itens,
    [property: JsonPropertyName("valor_produtos")]
    decimal ValorProdutos,
    [property: JsonPropertyName("estado_origem")]
    string EstadoOrigem,
    [property: JsonPropertyName("estado_destino")]
    string EstadoDestino,
    [property: JsonPropertyName("categoria_fiscal")]
    string? CategoriaFiscal,
    [property: JsonPropertyName("subcategoria_fiscal")]
    string? SubcategoriaFiscal,
    [property: JsonPropertyName("natureza_operacao")]
    string? NaturezaOperacao,
    [property: JsonPropertyName("tipo_operacao")]
    TipoOperacaoFiscal? TipoOperacao,
    [property: JsonPropertyName("exige_marketplace")]
    bool ExigeMarketplace,
    [property: JsonPropertyName("exige_dropshipping")]
    bool ExigeDropshipping);

public sealed record LogisticaRoteamentoResponseDto(
    FreteCotacaoDto FreteSelecionado,
    List<FreteCotacaoDto> OpcoesFrete,
    string? CodigoEmpresaEmitente,
    string? EmpresaEmitente,
    string? CnpjEmitente,
    string RoteamentoFiscalResumo,
    decimal CustoTotalEstimado,
    decimal LucroEstimado,
    decimal MargemEstimadaPercentual,
    string ResumoLogistico,
    List<string> Pendencias,
    DateTime CalculadoEm);

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

public sealed record GestaoCorporativaPainelDto(
    List<GestaoCorporativaIndicadorDto> Indicadores,
    List<GestaoCorporativaAlertaDto> Alertas,
    List<GestaoCorporativaVinculoDto> Vinculos,
    decimal MargemEstimadaPercentual,
    DateTime AtualizadoEm);

public sealed record GestaoCorporativaIndicadorDto(
    string Chave,
    string Titulo,
    string Valor,
    string Detalhe,
    string Status,
    string Modulo);

public sealed record GestaoCorporativaAlertaDto(
    string Modulo,
    string Titulo,
    string Detalhe,
    string Severidade,
    string Acao);

public sealed record GestaoCorporativaVinculoDto(
    string Origem,
    string Destino,
    int Total,
    int Pendentes,
    string Status);

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
    decimal CustoTotalEstimado,
    decimal LucroEstimado,
    decimal MargemEstimadaPercentual,
    decimal Score,
    List<string> Justificativas);

public sealed record FiscalManualEmissaoRequest(
    string EmpresaEmissora,
    string CnpjEmissor,
    string ClienteDestinatario,
    string DocumentoDestinatario,
    string NaturezaOperacao,
    string Cfop,
    decimal Subtotal,
    decimal Frete,
    decimal ImpostosEstimados,
    decimal MargemMinima,
    string? Observacoes,
    TipoOperacaoFiscal TipoOperacao,
    string EstadoOrigem,
    string EstadoDestino,
    string? CategoriaFiscal,
    string? SubcategoriaFiscal,
    bool ExigeMarketplace,
    bool ExigeDropshipping,
    bool RequerSaidaNfe,
    bool RequerEntradaNfe);

public sealed record FiscalManualEmissaoResponseDto(
    bool CertificadoOperacional,
    string CertificadoStatus,
    string? CertificadoReferencia,
    string RoteamentoResumo,
    string? CodigoEmpresaSelecionada,
    string? RazaoSocialSelecionada,
    string? CnpjSelecionado,
    string? EstadoSelecionado,
    List<string> Pendencias,
    DateTime GeradoEm);

public sealed record DashboardCompletoDto(
    DashboardKpiDto Kpis,
    List<FaturamentoPorPeriodoDto> FaturamentoSemanal,
    List<FaturamentoPorPeriodoDto> FaturamentoMensal,
    List<VendasPorLojaDto> VendasPorLoja,
    List<ProdutosMaisVendidosDto> ProdutosMaisVendidos,
    List<ClientesRecentesDto> ClientesRecentes,
    List<PedidosRecentesDto> PedidosRecentes,
    List<LeadsRecentesDto> LeadsRecentes);

public sealed record GenesisGestSchemaStatusDto(
    int GenesisOriginalEstruturasEsperadas,
    int EstruturasGenesisDisponiveis,
    int PontesNexumGenesisDisponiveis,
    bool Sincronizado,
    List<GenesisGestModuloStatusDto> Modulos);

public sealed record GenesisGestModuloStatusDto(
    string Prefixo,
    string Nome,
    int EstruturasDisponiveis,
    bool Disponivel);
