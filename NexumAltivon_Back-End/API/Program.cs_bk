/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7225 Data: 16/08/2026
 */

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MySqlConnector;
using System.IdentityModel.Tokens.Jwt;
using NexumAltivon.API.Data;
using NexumAltivon.API.ERP.FiscalRouting;
using NexumAltivon.API.ERP.SharedData;
using NexumAltivon.API.Infrastructure.Logistics;
using NexumAltivon.API.Infrastructure.Reports;
using NexumAltivon.API.Infrastructure.Storage;
using NexumAltivon.API.Infrastructure.Tenancy;
using NexumAltivon.API.Models;
using NexumAltivon.API.Services;
using System.Data;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var bootstrapPublicWebRoot = Environment.GetEnvironmentVariable("Storage__PublicWebRoot")
    ?? Environment.GetEnvironmentVariable("NEXUM_PUBLIC_WEBROOT");
var builderOptions = new WebApplicationOptions
{
    Args = args,
    WebRootPath = string.IsNullOrWhiteSpace(bootstrapPublicWebRoot)
        ? null
        : Path.GetFullPath(bootstrapPublicWebRoot.Trim())
};
var builder = WebApplication.CreateBuilder(builderOptions);
builder.WebHost.UseUrls("http://localhost:5010");
var releaseVersion = GetReleaseVersion();
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

var configuredPublicWebRoot = builder.Configuration["Storage:PublicWebRoot"]
    ?? Environment.GetEnvironmentVariable("NEXUM_PUBLIC_WEBROOT");
if (!string.IsNullOrWhiteSpace(configuredPublicWebRoot))
{
    var publicWebRoot = Path.GetFullPath(configuredPublicWebRoot.Trim());
    var publicUploadsRoot = Path.Combine(publicWebRoot, "uploads");
    if (!Directory.Exists(publicUploadsRoot))
    {
        throw new InvalidOperationException($"Storage:PublicWebRoot deve apontar para um diretorio existente com a pasta uploads. Caminho recebido: {publicWebRoot}");
    }

    var activeWebRoot = Path.GetFullPath(builder.Environment.WebRootPath);
    if (!string.Equals(activeWebRoot.TrimEnd(Path.DirectorySeparatorChar), publicWebRoot.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Storage:PublicWebRoot deve ser fornecido no bootstrap por Storage__PublicWebRoot ou NEXUM_PUBLIC_WEBROOT.");
    }
}

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var apiSettings = builder.Configuration.GetSection("ApiSettings");
var secretKey = ResolveJwtSecret(builder.Configuration);
if (Encoding.UTF8.GetByteCount(secretKey) < 32)
{
    throw new InvalidOperationException("JwtSettings:SecretKey deve vir de variavel de ambiente/cofre e ter ao menos 32 bytes. Configure JwtSettings__SecretKey ou JWT_SECRET_KEY no runtime.");
}
var issuer = jwtSettings["Issuer"] ?? "NexumAltivon.API";
var audience = jwtSettings["Audience"] ?? "NexumAltivon.Admin";
var refreshTokenExpirationDays = jwtSettings.GetValue("RefreshTokenExpirationDays", 7);
if (refreshTokenExpirationDays is < 1 or > 90)
{
    throw new InvalidOperationException("JwtSettings:RefreshTokenExpirationDays deve estar entre 1 e 90 dias.");
}
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

var connectionString = ResolveConfiguredConnectionString(builder.Configuration, "DefaultConnection", "NexumDb");
var genesisConnectionString = ResolveConfiguredConnectionString(builder.Configuration, "GenesisConnection");

if (connectionString is not null)
{
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
    builder.Services.AddDbContext<NexumDbContext>(options =>
        options.UseMySql(
            connectionString,
            serverVersion,
            mySqlOptions =>
            {
                mySqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(2), null);
                mySqlOptions.CommandTimeout(30);
            }));
}
else
{
    builder.Services.AddScoped<NexumDbContext>(_ =>
        throw new InvalidOperationException("ConnectionStrings:DefaultConnection ou ConnectionStrings:NexumDb nao configurada com valor real. Configure a conexao do banco nexum_altivon por variavel de ambiente/cofre."));
}

if (genesisConnectionString is not null)
{
    var genesisServerVersion = new MySqlServerVersion(new Version(8, 0, 0));
    builder.Services.AddDbContext<GenesisDbContext>(options =>
        options.UseMySql(
            genesisConnectionString,
            genesisServerVersion,
            mySqlOptions =>
            {
                mySqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(2), null);
                mySqlOptions.CommandTimeout(30);
            }));
}
else
{
    builder.Services.AddScoped<GenesisDbContext>(_ =>
        throw new InvalidOperationException("ConnectionStrings:GenesisConnection nao configurada com valor real. Configure a conexao do banco genesis_bd por variavel de ambiente/cofre."));
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

        var origins = GetCorsOrigins(builder.Configuration);

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
    options.AddPolicy("SuperAdmin", policy => policy.RequireRole("SuperAdmin"));
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
        Version = $"v{releaseVersion}",
        Description = "API funcional inicial para site e painel administrativo Nexum Altivon."
    });
});

builder.Services.AddHealthChecks();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddSingleton<IFiscalRoutingEngine, FiscalRoutingEngine>();
builder.Services.AddSingleton<FinancePdfReportService>();
builder.Services.AddScoped<LogisticaTrackingService>();
builder.Services.AddHttpClient("mercado-pago", client =>
{
    client.BaseAddress = new Uri("https://api.mercadopago.com/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddHttpClient("melhor-envio", client =>
{
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.UserAgent.ParseAdd($"NexumAltivon/{releaseVersion}");
});
builder.Services.AddHttpClient("mercado-livre", client =>
{
    client.BaseAddress = new Uri("https://api.mercadolibre.com/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddHttpClient("marketplace-sync", client =>
{
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.UserAgent.ParseAdd($"GenesisGest.Net/{releaseVersion}");
});
builder.Services.AddHttpClient("fiscal-sefaz", client =>
{
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.UserAgent.ParseAdd($"GenesisGest.Net/{releaseVersion}");
});
builder.Services.AddHttpClient("Notificacoes", client =>
{
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddHttpClient("OpenAI", client =>
{
    var baseUrl = builder.Configuration["OpenAI:BaseUrl"]
        ?? Environment.GetEnvironmentVariable("OPENAI_API_BASE_URL")
        ?? "https://api.openai.com/v1/";
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<INotificacaoService, NotificacaoService>();
builder.Services.AddScoped<IAssistenteIaService, AssistenteIaService>();
builder.Services.AddScoped<IOpenAiCredentialStore, DatabaseOpenAiCredentialStore>();
builder.Services.AddScoped<IAnexoStorageService, AnexoStorageService>();

var defaultDataProtectionKeysPath = builder.Environment.IsProduction()
    ? Directory.GetParent(builder.Environment.ContentRootPath)?.FullName
    : Path.GetTempPath();
var configuredDataProtectionKeysPath = TrimOrNull(builder.Configuration["DataProtection:KeysPath"]);
var dataProtectionKeysPath = configuredDataProtectionKeysPath is null
    ? defaultDataProtectionKeysPath
    : Path.GetFullPath(configuredDataProtectionKeysPath, builder.Environment.ContentRootPath);
if (string.IsNullOrWhiteSpace(dataProtectionKeysPath) || !Directory.Exists(dataProtectionKeysPath))
{
    throw new InvalidOperationException($"Diretorio persistente de Data Protection inexistente: {dataProtectionKeysPath ?? "nao_resolvido"}. Configure DataProtection__KeysPath com um diretorio existente.");
}

var dataProtectionBuilder = builder.Services
    .AddDataProtection()
    .SetApplicationName("GenesisGest.Net.v1.1.5")
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
if (OperatingSystem.IsWindows())
{
    dataProtectionBuilder.ProtectKeysWithDpapi(protectToLocalMachine: true);
}

Console.WriteLine("[NexumStartup] Montando aplicacao.");
var app = builder.Build();
Console.WriteLine("[NexumStartup] Aplicacao montada.");

if (app.Configuration.GetValue("OperationalSchema:Enabled", true))
{
    await EnsureOperationalSchemaAsync(app.Services, app.Logger);
}
else
{
    app.Logger.LogInformation("Operational schema check disabled by configuration.");
}

app.UseCors("NexumCorsPolicy");
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        if (context.Context.Request.Path.StartsWithSegments("/uploads"))
        {
            var fileName = Path.GetFileName(context.Context.Request.Path.Value) ?? string.Empty;
            if (fileName.StartsWith("site-", StringComparison.OrdinalIgnoreCase))
            {
                context.Context.Response.Headers.CacheControl = "no-store,no-cache,must-revalidate,max-age=0";
                context.Context.Response.Headers["CDN-Cache-Control"] = "no-store";
                context.Context.Response.Headers.Pragma = "no-cache";
                context.Context.Response.Headers.Expires = "0";
            }
            else
            {
                context.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable";
            }

            context.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        }
    }
});

app.UseSwagger();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Nexum Altivon API v1");
        options.DocumentTitle = "Nexum Altivon API";
    });
}

app.UseAuthentication();
app.UseMiddleware<TenantResolverMiddleware>();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Text("Healthy", "text/plain")).AllowAnonymous();
app.MapGet("/api/health/db", (CancellationToken ct) =>
    CheckMySqlHealthAsync(connectionString, "sem_banco_configurado", "nexum_altivon", ct)).AllowAnonymous();

app.MapGet("/api/health/db/genesis", (CancellationToken ct) =>
    CheckMySqlHealthAsync(genesisConnectionString, "sem_genesis_configurado", "genesis_bd", ct)).AllowAnonymous();

app.MapGet("/api/health/redis", async (IConfiguration configuration, CancellationToken ct) =>
{
    var redisConnection = TrimOrNull(
        configuration["Redis:ConnectionString"]
        ?? configuration["Hangfire:Storage:Redis"]
        ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING"));

    if (redisConnection is null)
    {
        return Results.Ok(new { status = "sem_redis_configurado" });
    }

    if (!TryResolveRedisEndpoint(redisConnection, out var host, out var port, out var error))
    {
        return Results.BadRequest(new { status = "redis_configuracao_invalida", erro = error });
    }

    try
    {
        using var redisSocket = new TcpClient();
        await redisSocket.ConnectAsync(host, port, ct);

        return Results.Ok(new
        {
            status = "Healthy",
            host,
            port
        });
    }
    catch (OperationCanceledException)
    {
        throw;
    }
    catch (Exception ex)
    {
        return Results.Problem($"Redis configurado, mas sem conexao em {host}:{port}. {ex.Message}");
    }
})
.AllowAnonymous()
.WithName("HealthRedis");

app.MapPost("/api/assistentes/yara/mensagem", async (
    AssistenteMensagemRequest request,
    IAssistenteIaService assistenteIa,
    NexumDbContext db,
    CancellationToken ct) =>
{
    try
    {
        var contextoOperacional = await BuildYaraOperationalContextAsync(db, request.Mensagem, ct);
        var resposta = await assistenteIa.ResponderYaraAsync(request with { ContextoOperacional = contextoOperacional }, ct);
        return Results.Ok(ApiResponse<AssistenteIaResposta>.Ok(resposta, "Mensagem processada pela Yara."));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ApiResponse<string>.Erro(ex.Message));
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(
            title: "Assistente de IA indisponivel",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
})
.AllowAnonymous()
.WithName("AssistenteYaraMensagem");

app.MapPost("/api/assistentes/mensagem", () => Results.Problem(
    title: "Rota de assistentes substituida",
    detail: "O atendimento publico utiliza exclusivamente /api/assistentes/yara/mensagem. Sophia permanece restrita ao backend administrativo.",
    statusCode: StatusCodes.Status410Gone))
.AllowAnonymous()
.WithName("AssistentesMensagemDescontinuada");

app.MapPost("/api/assistentes/sophia/mensagem", async (
    AssistenteMensagemRequest request,
    IAssistenteIaService assistenteIa,
    CancellationToken ct) =>
{
    try
    {
        var resposta = await assistenteIa.ResponderSophiaAsync(request, ct);
        return Results.Ok(ApiResponse<AssistenteIaResposta>.Ok(resposta, "Mensagem processada pela Sophia."));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ApiResponse<string>.Erro(ex.Message));
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(
            title: "Assistente de IA indisponivel",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
})
.RequireAuthorization("Admin")
.WithName("AssistenteSophiaMensagem");

app.MapGet("/api/admin/integracoes/openai", async (
    IAssistenteIaService assistenteIa,
    CancellationToken ct) =>
{
    try
    {
        var status = await assistenteIa.ObterStatusAsync(ct);
        return Results.Ok(ApiResponse<OpenAiAssistentesStatus>.Ok(status, "Configuracao OpenAI consultada no banco oficial."));
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(
            title: "Falha ao consultar configuracao OpenAI",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.RequireAuthorization("SuperAdmin")
.WithName("OpenAiAssistentesStatus");

app.MapPut("/api/admin/integracoes/openai", async (
    OpenAiAssistentesConfiguracaoRequest request,
    IAssistenteIaService assistenteIa,
    HttpContext httpContext,
    CancellationToken ct) =>
{
    try
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "usuario-administrativo";
        var status = await assistenteIa.ConfigurarAsync(request, userId, ct);
        return Results.Ok(ApiResponse<OpenAiAssistentesStatus>.Ok(status, "Chaves OpenAI validadas, criptografadas e confirmadas no banco oficial."));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ApiResponse<string>.Erro(ex.Message));
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(
            title: "Configuracao OpenAI recusada",
            detail: ex.Message,
            statusCode: StatusCodes.Status424FailedDependency);
    }
})
.RequireAuthorization("SuperAdmin")
.WithName("OpenAiAssistentesConfigurar");

app.MapGet("/api/anexos/status", (IAnexoStorageService storage) =>
{
    return Results.Ok(ApiResponse<AnexoStorageStatusDto>.Ok(storage.ObterStatus(), "Storage de anexos verificado."));
})
.AllowAnonymous()
.WithName("AnexosStatus");

app.MapPost("/api/anexos/assinar-upload", [Authorize(Policy = "Gerente")] (
    AnexoSignedUrlRequest request,
    IAnexoStorageService storage,
    HttpContext httpContext) =>
{
    try
    {
        var signedUrl = storage.CriarUrlAssinada(request, httpContext, "PUT");
        return Results.Ok(ApiResponse<AnexoSignedUrlDto>.Ok(signedUrl, "Url de upload assinada."));
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ApiResponse<AnexoSignedUrlDto>.Erro(ex.Message));
    }
})
.WithName("AnexosAssinarUpload");

app.MapPost("/api/anexos/assinar-download", [Authorize(Policy = "Gerente")] (
    AnexoSignedUrlRequest request,
    IAnexoStorageService storage,
    HttpContext httpContext) =>
{
    try
    {
        var signedUrl = storage.CriarUrlAssinada(request, httpContext, "GET");
        return Results.Ok(ApiResponse<AnexoSignedUrlDto>.Ok(signedUrl, "Url de download assinada."));
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ApiResponse<AnexoSignedUrlDto>.Erro(ex.Message));
    }
})
.WithName("AnexosAssinarDownload");

app.MapPut("/api/anexos/upload/{**storageKey}", async (
    string storageKey,
    HttpRequest request,
    IAnexoStorageService storage,
    CancellationToken ct) =>
{
    if (!storage.ValidarUrlLocal("PUT", storageKey, request.Query, out _))
    {
        return Results.Unauthorized();
    }

    try
    {
        var uploaded = await storage.SalvarUploadLocalAsync(storageKey, request, ct);
        return Results.Ok(ApiResponse<AnexoLocalUploadDto>.Ok(uploaded, "Anexo gravado."));
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ApiResponse<AnexoLocalUploadDto>.Erro(ex.Message));
    }
})
.AllowAnonymous()
.WithName("AnexosUploadAssinado");

app.MapGet("/api/anexos/download/{**storageKey}", async (
    string storageKey,
    HttpRequest request,
    IAnexoStorageService storage,
    CancellationToken ct) =>
{
    if (!storage.ValidarUrlLocal("GET", storageKey, request.Query, out _))
    {
        return Results.Unauthorized();
    }

    var download = await storage.AbrirDownloadLocalAsync(storageKey, ct);
    if (download is null)
    {
        return Results.NotFound(ApiResponse<object>.Erro("Anexo nao encontrado."));
    }

    return Results.File(download.Stream, download.ContentType, download.FileName, enableRangeProcessing: true);
})
.AllowAnonymous()
.WithName("AnexosDownloadAssinado");

app.MapGet("/api/erp/genesis/financeiro/resumo", async (GenesisDbContext db, CancellationToken ct) =>
{
    var resumo = await GenesisFinanceService.GetResumoAsync(db, ct);
    return Results.Ok(ApiResponse<GenesisFinanceSummaryDto>.Ok(resumo, "Resumo financeiro carregado do banco GenesisGest.Net."));
})
.RequireAuthorization("Financeiro")
.WithName("GenesisFinanceiroResumo");

app.MapGet("/api/erp/genesis/financeiro/contas-pagar", async (GenesisDbContext db, CancellationToken ct) =>
{
    var itens = await GenesisFinanceService.ListarContasPagarAsync(db, ct);
    return Results.Ok(ApiResponse<List<GenesisContaPagarDto>>.Ok(itens, "Contas a pagar carregadas do banco GenesisGest.Net.", itens.Count));
})
.RequireAuthorization("Financeiro")
.WithName("GenesisContasPagarListar");

app.MapGet("/api/erp/genesis/financeiro/contas-receber", async (GenesisDbContext db, CancellationToken ct) =>
{
    var itens = await GenesisFinanceService.ListarContasReceberAsync(db, ct);
    return Results.Ok(ApiResponse<List<GenesisContaReceberDto>>.Ok(itens, "Contas a receber carregadas do banco GenesisGest.Net.", itens.Count));
})
.RequireAuthorization("Financeiro")
.WithName("GenesisContasReceberListar");

app.MapGet("/api/erp/genesis/financeiro/contas-pagar/relatorio.pdf", async (
    DateTime? inicio,
    DateTime? fim,
    string? status,
    GenesisDbContext db,
    ITenantContext tenantContext,
    FinancePdfReportService reports,
    CancellationToken ct) =>
{
    try
    {
        var itens = await GenesisFinanceService.ListarContasPagarAsync(db, inicio, fim, status, ct);
        if (itens.Count == 0)
        {
            return Results.NotFound(ApiResponse<object>.Erro("Nenhuma conta a pagar foi encontrada para os filtros informados."));
        }

        var generatedAtUtc = DateTime.UtcNow;
        var pdf = reports.CreatePayablesReport(itens, tenantContext.TenantId, inicio, fim, status, generatedAtUtc);
        return Results.File(
            pdf,
            "application/pdf",
            $"genesis-contas-pagar-{generatedAtUtc:yyyyMMdd-HHmmss}.pdf");
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ApiResponse<object>.Erro(ex.Message));
    }
})
.RequireAuthorization("Financeiro")
.WithName("GenesisContasPagarRelatorioPdf");

app.MapGet("/api/erp/genesis/financeiro/contas-receber/relatorio.pdf", async (
    DateTime? inicio,
    DateTime? fim,
    string? status,
    GenesisDbContext db,
    ITenantContext tenantContext,
    FinancePdfReportService reports,
    CancellationToken ct) =>
{
    try
    {
        var itens = await GenesisFinanceService.ListarContasReceberAsync(db, inicio, fim, status, ct);
        if (itens.Count == 0)
        {
            return Results.NotFound(ApiResponse<object>.Erro("Nenhuma conta a receber foi encontrada para os filtros informados."));
        }

        var generatedAtUtc = DateTime.UtcNow;
        var pdf = reports.CreateReceivablesReport(itens, tenantContext.TenantId, inicio, fim, status, generatedAtUtc);
        return Results.File(
            pdf,
            "application/pdf",
            $"genesis-contas-receber-{generatedAtUtc:yyyyMMdd-HHmmss}.pdf");
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ApiResponse<object>.Erro(ex.Message));
    }
})
.RequireAuthorization("Financeiro")
.WithName("GenesisContasReceberRelatorioPdf");

app.MapPost("/api/erp/genesis/financeiro/contas-pagar", async (
    GenesisDbContext db,
    NexumDbContext auditDb,
    GenesisContaPagarCreateRequest request,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    CancellationToken ct) =>
{
    try
    {
        var created = await GenesisFinanceService.CriarContaPagarAsync(db, request, ct);
        auditDb.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "erp_contas_pagar", created.Id, AcaoAuditoria.INSERT, null, created));
        await auditDb.SaveChangesAsync(ct);
        return Results.Created(
            $"/api/erp/genesis/financeiro/contas-pagar/{created.Id}",
            ApiResponse<GenesisContaPagarDto>.Ok(created, "Conta a pagar persistida e relida do banco GenesisGest.Net."));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ApiResponse<GenesisContaPagarDto>.Erro(ex.Message));
    }
    catch (InvalidOperationException ex) when (ex.Message.StartsWith("Ja existe", StringComparison.Ordinal))
    {
        return Results.Conflict(ApiResponse<GenesisContaPagarDto>.Erro(ex.Message));
    }
})
.RequireAuthorization("Financeiro")
.WithName("GenesisContasPagarCriar");

app.MapPost("/api/erp/genesis/financeiro/contas-receber", async (
    GenesisDbContext db,
    NexumDbContext auditDb,
    GenesisContaReceberCreateRequest request,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    CancellationToken ct) =>
{
    try
    {
        var created = await GenesisFinanceService.CriarContaReceberAsync(db, request, ct);
        auditDb.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "erp_contas_receber", created.Id, AcaoAuditoria.INSERT, null, created));
        await auditDb.SaveChangesAsync(ct);
        return Results.Created(
            $"/api/erp/genesis/financeiro/contas-receber/{created.Id}",
            ApiResponse<GenesisContaReceberDto>.Ok(created, "Conta a receber persistida e relida do banco GenesisGest.Net."));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ApiResponse<GenesisContaReceberDto>.Erro(ex.Message));
    }
    catch (InvalidOperationException ex) when (ex.Message.StartsWith("Ja existe", StringComparison.Ordinal))
    {
        return Results.Conflict(ApiResponse<GenesisContaReceberDto>.Erro(ex.Message));
    }
})
.RequireAuthorization("Financeiro")
.WithName("GenesisContasReceberCriar");

app.MapPost("/api/erp/genesis/financeiro/contas-pagar/{id:int}/baixa", async (
    int id,
    GenesisDbContext db,
    NexumDbContext auditDb,
    GenesisBaixaPagarRequest request,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    CancellationToken ct) =>
{
    try
    {
        var anterior = await GenesisFinanceService.ObterContaPagarAsync(db, id, ct);
        if (anterior is null)
        {
            return Results.NotFound(ApiResponse<GenesisContaPagarDto>.Erro("Conta a pagar nao encontrada."));
        }

        var updated = await GenesisFinanceService.BaixarContaPagarAsync(db, id, request, ct);
        if (updated is null)
        {
            return Results.NotFound(ApiResponse<GenesisContaPagarDto>.Erro("Conta a pagar deixou de existir durante a baixa."));
        }

        auditDb.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "erp_contas_pagar", id, AcaoAuditoria.UPDATE, anterior, updated));
        await auditDb.SaveChangesAsync(ct);
        return Results.Ok(ApiResponse<GenesisContaPagarDto>.Ok(updated, "Baixa da conta a pagar persistida e relida do banco GenesisGest.Net."));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ApiResponse<GenesisContaPagarDto>.Erro(ex.Message));
    }
})
.RequireAuthorization("Financeiro")
.WithName("GenesisContasPagarBaixar");

app.MapPost("/api/erp/genesis/financeiro/contas-receber/{id:int}/baixa", async (
    int id,
    GenesisDbContext db,
    NexumDbContext auditDb,
    GenesisBaixaReceberRequest request,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    CancellationToken ct) =>
{
    try
    {
        var anterior = await GenesisFinanceService.ObterContaReceberAsync(db, id, ct);
        if (anterior is null)
        {
            return Results.NotFound(ApiResponse<GenesisContaReceberDto>.Erro("Conta a receber nao encontrada."));
        }

        var updated = await GenesisFinanceService.BaixarContaReceberAsync(db, id, request, ct);
        if (updated is null)
        {
            return Results.NotFound(ApiResponse<GenesisContaReceberDto>.Erro("Conta a receber deixou de existir durante o recebimento."));
        }

        auditDb.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "erp_contas_receber", id, AcaoAuditoria.UPDATE, anterior, updated));
        await auditDb.SaveChangesAsync(ct);
        return Results.Ok(ApiResponse<GenesisContaReceberDto>.Ok(updated, "Baixa da conta a receber persistida e relida do banco GenesisGest.Net."));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ApiResponse<GenesisContaReceberDto>.Erro(ex.Message));
    }
})
.RequireAuthorization("Financeiro")
.WithName("GenesisContasReceberBaixar");

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

app.MapGet("/api/erp/genesis/pdv/vendas", async (GenesisDbContext genesisDb, int? limite, CancellationToken ct) =>
{
    var vendas = await GenesisPdvService.ListarVendasRecentesAsync(genesisDb, limite ?? 50, ct);
    return Results.Ok(vendas);
})
.RequireAuthorization("Gerente");

app.MapPost("/api/erp/genesis/pdv/vendas", async (
    GenesisPdvVendaRequest request,
    GenesisDbContext genesisDb,
    NexumDbContext nexumDb,
    CancellationToken ct) =>
{
    try
    {
        var venda = await GenesisPdvService.RegistrarVendaAsync(genesisDb, nexumDb, request, ct);
        return Results.Created($"/api/erp/genesis/pdv/vendas/{venda.Id}", venda);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { erro = ex.Message });
    }
})
.RequireAuthorization("Gerente");

app.MapPost("/api/desktop/genesis/pdv/vendas", async (
    HttpRequest httpRequest,
    IConfiguration configuration,
    GenesisPdvVendaRequest request,
    GenesisDbContext genesisDb,
    NexumDbContext nexumDb,
    CancellationToken ct) =>
{
    if (!ValidateDesktopTerminalAccess(httpRequest, configuration, out var terminalIdentity, out var rejection))
    {
        return Results.Unauthorized();
    }

    try
    {
        var venda = await GenesisPdvService.RegistrarVendaAsync(genesisDb, nexumDb, request, ct);
        return Results.Created($"/api/desktop/genesis/pdv/vendas/{venda.Id}", new
        {
            origem = terminalIdentity,
            gravadoNoServidor = true,
            venda
        });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { erro = ex.Message });
    }
});

app.MapPost("/api/desktop/genesis/operacoes/{module}", async (
    string module,
    HttpRequest httpRequest,
    IConfiguration configuration,
    GenesisDesktopOperationRequest request,
    GenesisDbContext genesisDb,
    CancellationToken ct) =>
{
    if (!ValidateDesktopTerminalAccess(httpRequest, configuration, out var terminalIdentity, out var rejection))
    {
        return Results.Unauthorized();
    }

    try
    {
        var operacao = await GenesisDesktopOperationService.RegistrarOperacaoAsync(
            genesisDb,
            module,
            request,
            terminalIdentity,
            ct);

        return Results.Created($"/api/desktop/genesis/operacoes/{module}/{operacao.Id}", operacao);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { erro = ex.Message });
    }
});

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

app.MapGet("/api/ops/ativos", [Authorize(Policy = "Gerente")] async (
    NexumDbContext db,
    ITenantContext tenantContext,
    string? status,
    CancellationToken ct) =>
{
    var statusFiltro = NormalizeBusinessKey(status);
    var ativos = await db.Database.SqlQueryRaw<OpsAtivoDto>(
        """
        SELECT
            oat_id AS Id,
            oat_codigo AS Codigo,
            oat_nome AS Nome,
            oat_tipo AS Tipo,
            oat_localizacao AS Localizacao,
            oat_status AS Status,
            oat_fabricante AS Fabricante,
            oat_modelo AS Modelo,
            oat_numero_serie AS NumeroSerie,
            oat_proxima_manutencao AS ProximaManutencao,
            oat_created_at AS CriadoEm,
            oat_updated_at AS AtualizadoEm
        FROM ops_ativos
        WHERE tenant_id = {0}
          AND is_deleted = 0
          AND ({1} IS NULL OR oat_status = {1})
        ORDER BY oat_nome
        """,
        tenantContext.TenantId.ToString(),
        (object?)statusFiltro ?? DBNull.Value)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<OpsAtivoDto>>.Ok(ativos, "Ativos operacionais carregados."));
})
.WithName("OpsAtivosListar");

app.MapPost("/api/ops/ativos", [Authorize(Policy = "Gerente")] async (
    OpsAtivoRequest request,
    NexumDbContext db,
    ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var codigo = NormalizeBusinessKey(request.Codigo);
    var nome = TrimOrNull(request.Nome);
    var tipo = NormalizeBusinessKey(request.Tipo) ?? "EQUIPAMENTO";
    var statusAtivo = NormalizeBusinessKey(request.Status) ?? "ATIVO";
    if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nome))
    {
        return Results.BadRequest(ApiResponse<OpsAtivoDto>.Erro("Codigo e nome do ativo sao obrigatorios."));
    }

    await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        INSERT INTO ops_ativos
            (tenant_id, oat_codigo, oat_nome, oat_tipo, oat_localizacao, oat_status, oat_fabricante, oat_modelo, oat_numero_serie, oat_proxima_manutencao, oat_created_at, oat_updated_at)
        VALUES
            ({tenantContext.TenantId.ToString()}, {codigo}, {nome}, {tipo}, {TrimOrNull(request.Localizacao)}, {statusAtivo}, {TrimOrNull(request.Fabricante)}, {TrimOrNull(request.Modelo)}, {TrimOrNull(request.NumeroSerie)}, {request.ProximaManutencao}, UTC_TIMESTAMP(), UTC_TIMESTAMP())
        """,
        ct);

    var id = await ExecuteScalarAsync<int>(db, "SELECT LAST_INSERT_ID();", ct);
    var response = new OpsAtivoDto(id, codigo, nome, tipo, TrimOrNull(request.Localizacao), statusAtivo, TrimOrNull(request.Fabricante), TrimOrNull(request.Modelo), TrimOrNull(request.NumeroSerie), request.ProximaManutencao, DateTime.UtcNow, DateTime.UtcNow);
    return Results.Created($"/api/ops/ativos/{id}", ApiResponse<OpsAtivoDto>.Ok(response, "Ativo operacional criado."));
})
.WithName("OpsAtivosCriar");

app.MapPut("/api/ops/ativos/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    OpsAtivoRequest request,
    NexumDbContext db,
    ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var codigo = NormalizeBusinessKey(request.Codigo);
    var nome = TrimOrNull(request.Nome);
    var tipo = NormalizeBusinessKey(request.Tipo) ?? "EQUIPAMENTO";
    var statusAtivo = NormalizeBusinessKey(request.Status) ?? "ATIVO";
    if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nome))
    {
        return Results.BadRequest(ApiResponse<OpsAtivoDto>.Erro("Codigo e nome do ativo sao obrigatorios."));
    }

    var affected = await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        UPDATE ops_ativos
        SET oat_codigo = {codigo},
            oat_nome = {nome},
            oat_tipo = {tipo},
            oat_localizacao = {TrimOrNull(request.Localizacao)},
            oat_status = {statusAtivo},
            oat_fabricante = {TrimOrNull(request.Fabricante)},
            oat_modelo = {TrimOrNull(request.Modelo)},
            oat_numero_serie = {TrimOrNull(request.NumeroSerie)},
            oat_proxima_manutencao = {request.ProximaManutencao},
            oat_updated_at = UTC_TIMESTAMP()
        WHERE oat_id = {id} AND tenant_id = {tenantContext.TenantId.ToString()} AND is_deleted = 0
        """,
        ct);

    if (affected == 0)
    {
        return Results.NotFound(ApiResponse<OpsAtivoDto>.Erro("Ativo operacional nao encontrado."));
    }

    var response = new OpsAtivoDto(id, codigo, nome, tipo, TrimOrNull(request.Localizacao), statusAtivo, TrimOrNull(request.Fabricante), TrimOrNull(request.Modelo), TrimOrNull(request.NumeroSerie), request.ProximaManutencao, DateTime.UtcNow, DateTime.UtcNow);
    return Results.Ok(ApiResponse<OpsAtivoDto>.Ok(response, "Ativo operacional atualizado."));
})
.WithName("OpsAtivosAtualizar");

app.MapDelete("/api/ops/ativos/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    NexumDbContext db,
    ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var affected = await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        UPDATE ops_ativos
        SET is_deleted = 1,
            deleted_at = UTC_TIMESTAMP(),
            oat_status = 'INATIVO',
            oat_updated_at = UTC_TIMESTAMP()
        WHERE oat_id = {id} AND tenant_id = {tenantContext.TenantId.ToString()} AND is_deleted = 0
        """,
        ct);

    return affected == 0
        ? Results.NotFound(ApiResponse<object>.Erro("Ativo operacional nao encontrado."))
        : Results.NoContent();
})
.WithName("OpsAtivosExcluir");

app.MapGet("/api/ops/ordens-servico", [Authorize(Policy = "Gerente")] async (
    NexumDbContext db,
    ITenantContext tenantContext,
    string? status,
    CancellationToken ct) =>
{
    var statusFiltro = NormalizeBusinessKey(status);
    var ordens = await db.Database.SqlQueryRaw<OpsOrdemServicoDto>(
        """
        SELECT
            oso_id AS Id,
            oso_numero AS Numero,
            oso_ativo_id AS AtivoId,
            oso_titulo AS Titulo,
            oso_descricao AS Descricao,
            oso_status AS Status,
            oso_prioridade AS Prioridade,
            oso_responsavel_user_id AS ResponsavelUserId,
            oso_data_abertura AS DataAbertura,
            oso_data_prevista AS DataPrevista,
            oso_data_conclusao AS DataConclusao,
            oso_tempo_estimado_minutos AS TempoEstimadoMinutos,
            oso_tempo_real_minutos AS TempoRealMinutos,
            oso_custo_previsto AS CustoPrevisto,
            oso_custo_real AS CustoReal,
            oso_observacoes AS Observacoes,
            oso_created_at AS CriadoEm,
            oso_updated_at AS AtualizadoEm
        FROM ops_ordens_servico
        WHERE tenant_id = {0}
          AND is_deleted = 0
          AND ({1} IS NULL OR oso_status = {1})
        ORDER BY oso_data_abertura DESC, oso_id DESC
        """,
        tenantContext.TenantId.ToString(),
        (object?)statusFiltro ?? DBNull.Value)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<OpsOrdemServicoDto>>.Ok(ordens, "Ordens de servico operacionais carregadas."));
})
.WithName("OpsOrdensServicoListar");

app.MapGet("/api/ops/ordens-servico/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    NexumDbContext db,
    ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var ordem = await db.Database.SqlQueryRaw<OpsOrdemServicoDto>(
        """
        SELECT
            oso_id AS Id,
            oso_numero AS Numero,
            oso_ativo_id AS AtivoId,
            oso_titulo AS Titulo,
            oso_descricao AS Descricao,
            oso_status AS Status,
            oso_prioridade AS Prioridade,
            oso_responsavel_user_id AS ResponsavelUserId,
            oso_data_abertura AS DataAbertura,
            oso_data_prevista AS DataPrevista,
            oso_data_conclusao AS DataConclusao,
            oso_tempo_estimado_minutos AS TempoEstimadoMinutos,
            oso_tempo_real_minutos AS TempoRealMinutos,
            oso_custo_previsto AS CustoPrevisto,
            oso_custo_real AS CustoReal,
            oso_observacoes AS Observacoes,
            oso_created_at AS CriadoEm,
            oso_updated_at AS AtualizadoEm
        FROM ops_ordens_servico
        WHERE oso_id = {0}
          AND tenant_id = {1}
          AND is_deleted = 0
        LIMIT 1
        """,
        id,
        tenantContext.TenantId.ToString())
        .SingleOrDefaultAsync(ct);

    if (ordem is null)
    {
        return Results.NotFound(ApiResponse<object>.Erro("Ordem de servico nao encontrada."));
    }

    var itens = await db.Database.SqlQueryRaw<OpsOrdemServicoItemDto>(
        """
        SELECT
            osi_id AS Id,
            osi_tipo AS Tipo,
            osi_codigo AS Codigo,
            osi_descricao AS Descricao,
            osi_quantidade AS Quantidade,
            osi_unidade AS Unidade,
            osi_custo_unitario AS CustoUnitario,
            osi_total AS Total
        FROM ops_ordem_servico_itens
        WHERE oso_id = {0}
          AND tenant_id = {1}
        ORDER BY osi_id
        """,
        id,
        tenantContext.TenantId.ToString())
        .ToListAsync(ct);

    var detalhe = new OpsOrdemServicoDetalheDto(ordem, itens);
    return Results.Ok(ApiResponse<OpsOrdemServicoDetalheDto>.Ok(detalhe, "Detalhe da ordem de servico carregado."));
})
.WithName("OpsOrdensServicoDetalhe");

app.MapPost("/api/ops/ordens-servico", [Authorize(Policy = "Gerente")] async (
    OpsOrdemServicoRequest request,
    NexumDbContext db,
    ITenantContext tenantContext,
    ClaimsPrincipal principal,
    CancellationToken ct) =>
{
    var titulo = TrimOrNull(request.Titulo);
    if (string.IsNullOrWhiteSpace(titulo))
    {
        return Results.BadRequest(ApiResponse<OpsOrdemServicoDto>.Erro("Titulo da ordem de servico e obrigatorio."));
    }

    var numero = TrimOrNull(request.Numero) ?? $"OS-{DateTime.UtcNow:yyyyMMddHHmmss}-{RandomNumberGenerator.GetInt32(100, 999)}";
    var statusOs = NormalizeBusinessKey(request.Status) ?? "ABERTA";
    var prioridade = NormalizeBusinessKey(request.Prioridade) ?? "NORMAL";
    var responsavel = request.ResponsavelUserId ?? GetCurrentUserId(principal);
    var abertura = request.DataAbertura ?? DateTime.UtcNow;

    await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        INSERT INTO ops_ordens_servico
            (tenant_id, oso_numero, oso_ativo_id, oso_titulo, oso_descricao, oso_status, oso_prioridade, oso_responsavel_user_id, oso_data_abertura, oso_data_prevista, oso_data_conclusao, oso_tempo_estimado_minutos, oso_tempo_real_minutos, oso_custo_previsto, oso_custo_real, oso_observacoes, oso_created_at, oso_updated_at)
        VALUES
            ({tenantContext.TenantId.ToString()}, {numero}, {request.AtivoId}, {titulo}, {TrimOrNull(request.Descricao)}, {statusOs}, {prioridade}, {responsavel}, {abertura}, {request.DataPrevista}, {request.DataConclusao}, {request.TempoEstimadoMinutos}, {request.TempoRealMinutos}, {request.CustoPrevisto}, {request.CustoReal}, {TrimOrNull(request.Observacoes)}, UTC_TIMESTAMP(), UTC_TIMESTAMP())
        """,
        ct);

    var id = await ExecuteScalarAsync<int>(db, "SELECT LAST_INSERT_ID();", ct);
    await ReplaceOpsOrdemItensAsync(db, tenantContext.TenantId, id, request.Itens, ct);

    var response = new OpsOrdemServicoDto(id, numero, request.AtivoId, titulo, TrimOrNull(request.Descricao), statusOs, prioridade, responsavel, abertura, request.DataPrevista, request.DataConclusao, request.TempoEstimadoMinutos, request.TempoRealMinutos, request.CustoPrevisto, request.CustoReal, TrimOrNull(request.Observacoes), DateTime.UtcNow, DateTime.UtcNow);
    return Results.Created($"/api/ops/ordens-servico/{id}", ApiResponse<OpsOrdemServicoDto>.Ok(response, "Ordem de servico criada."));
})
.WithName("OpsOrdensServicoCriar");

app.MapPut("/api/ops/ordens-servico/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    OpsOrdemServicoRequest request,
    NexumDbContext db,
    ITenantContext tenantContext,
    ClaimsPrincipal principal,
    CancellationToken ct) =>
{
    var titulo = TrimOrNull(request.Titulo);
    if (string.IsNullOrWhiteSpace(titulo))
    {
        return Results.BadRequest(ApiResponse<OpsOrdemServicoDto>.Erro("Titulo da ordem de servico e obrigatorio."));
    }

    var numero = TrimOrNull(request.Numero) ?? $"OS-{id:D6}";
    var statusOs = NormalizeBusinessKey(request.Status) ?? "ABERTA";
    var prioridade = NormalizeBusinessKey(request.Prioridade) ?? "NORMAL";
    var responsavel = request.ResponsavelUserId ?? GetCurrentUserId(principal);
    var abertura = request.DataAbertura ?? DateTime.UtcNow;

    var affected = await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        UPDATE ops_ordens_servico
        SET oso_numero = {numero},
            oso_ativo_id = {request.AtivoId},
            oso_titulo = {titulo},
            oso_descricao = {TrimOrNull(request.Descricao)},
            oso_status = {statusOs},
            oso_prioridade = {prioridade},
            oso_responsavel_user_id = {responsavel},
            oso_data_abertura = {abertura},
            oso_data_prevista = {request.DataPrevista},
            oso_data_conclusao = {request.DataConclusao},
            oso_tempo_estimado_minutos = {request.TempoEstimadoMinutos},
            oso_tempo_real_minutos = {request.TempoRealMinutos},
            oso_custo_previsto = {request.CustoPrevisto},
            oso_custo_real = {request.CustoReal},
            oso_observacoes = {TrimOrNull(request.Observacoes)},
            oso_updated_at = UTC_TIMESTAMP()
        WHERE oso_id = {id} AND tenant_id = {tenantContext.TenantId.ToString()} AND is_deleted = 0
        """,
        ct);

    if (affected == 0)
    {
        return Results.NotFound(ApiResponse<OpsOrdemServicoDto>.Erro("Ordem de servico nao encontrada."));
    }

    await ReplaceOpsOrdemItensAsync(db, tenantContext.TenantId, id, request.Itens, ct);
    var response = new OpsOrdemServicoDto(id, numero, request.AtivoId, titulo, TrimOrNull(request.Descricao), statusOs, prioridade, responsavel, abertura, request.DataPrevista, request.DataConclusao, request.TempoEstimadoMinutos, request.TempoRealMinutos, request.CustoPrevisto, request.CustoReal, TrimOrNull(request.Observacoes), DateTime.UtcNow, DateTime.UtcNow);
    return Results.Ok(ApiResponse<OpsOrdemServicoDto>.Ok(response, "Ordem de servico atualizada."));
})
.WithName("OpsOrdensServicoAtualizar");

app.MapDelete("/api/ops/ordens-servico/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    NexumDbContext db,
    ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var affected = await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        UPDATE ops_ordens_servico
        SET is_deleted = 1,
            deleted_at = UTC_TIMESTAMP(),
            oso_status = 'CANCELADA',
            oso_updated_at = UTC_TIMESTAMP()
        WHERE oso_id = {id} AND tenant_id = {tenantContext.TenantId.ToString()} AND is_deleted = 0
        """,
        ct);

    return affected == 0
        ? Results.NotFound(ApiResponse<object>.Erro("Ordem de servico nao encontrada."))
        : Results.NoContent();
})
.WithName("OpsOrdensServicoExcluir");

app.MapGet("/api/ops/producao/apontamentos", [Authorize(Policy = "Gerente")] async (
    NexumDbContext db,
    ITenantContext tenantContext,
    DateTime? inicio,
    DateTime? fim,
    CancellationToken ct) =>
{
    var apontamentos = await db.Database.SqlQueryRaw<OpsProducaoApontamentoDto>(
        """
        SELECT
            opa_id AS Id,
            oso_id AS OrdemServicoId,
            produto_id AS ProdutoId,
            produto_codigo AS ProdutoCodigo,
            produto_nome AS ProdutoNome,
            quantidade_produzida AS QuantidadeProduzida,
            quantidade_refugo AS QuantidadeRefugo,
            tempo_minutos AS TempoMinutos,
            operador_user_id AS OperadorUserId,
            data_apontamento AS DataApontamento,
            insumos_json AS InsumosJson,
            observacoes AS Observacoes
        FROM ops_producao_apontamentos
        WHERE tenant_id = {0}
          AND is_deleted = 0
          AND ({1} IS NULL OR data_apontamento >= {1})
          AND ({2} IS NULL OR data_apontamento <= {2})
        ORDER BY data_apontamento DESC, opa_id DESC
        """,
        tenantContext.TenantId.ToString(),
        (object?)inicio ?? DBNull.Value,
        (object?)fim ?? DBNull.Value)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<OpsProducaoApontamentoDto>>.Ok(apontamentos, "Apontamentos de producao carregados."));
})
.WithName("OpsProducaoApontamentosListar");

app.MapPost("/api/ops/producao/apontamentos", [Authorize(Policy = "Gerente")] async (
    OpsProducaoApontamentoRequest request,
    NexumDbContext db,
    ITenantContext tenantContext,
    ClaimsPrincipal principal,
    CancellationToken ct) =>
{
    var produtoNome = TrimOrNull(request.ProdutoNome);
    if (string.IsNullOrWhiteSpace(produtoNome) || request.QuantidadeProduzida <= 0)
    {
        return Results.BadRequest(ApiResponse<OpsProducaoApontamentoDto>.Erro("Produto e quantidade produzida positiva sao obrigatorios."));
    }

    var operador = request.OperadorUserId ?? GetCurrentUserId(principal);
    var data = request.DataApontamento ?? DateTime.UtcNow;
    var insumosJson = JsonSerializer.Serialize(request.Insumos ?? []);

    await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        INSERT INTO ops_producao_apontamentos
            (tenant_id, oso_id, produto_id, produto_codigo, produto_nome, quantidade_produzida, quantidade_refugo, tempo_minutos, operador_user_id, data_apontamento, insumos_json, observacoes, created_at, updated_at)
        VALUES
            ({tenantContext.TenantId.ToString()}, {request.OrdemServicoId}, {request.ProdutoId}, {NormalizeBusinessKey(request.ProdutoCodigo)}, {produtoNome}, {request.QuantidadeProduzida}, {request.QuantidadeRefugo}, {request.TempoMinutos}, {operador}, {data}, {insumosJson}, {TrimOrNull(request.Observacoes)}, UTC_TIMESTAMP(), UTC_TIMESTAMP())
        """,
        ct);

    var id = await ExecuteScalarAsync<int>(db, "SELECT LAST_INSERT_ID();", ct);
    var response = new OpsProducaoApontamentoDto(id, request.OrdemServicoId, request.ProdutoId, NormalizeBusinessKey(request.ProdutoCodigo), produtoNome, request.QuantidadeProduzida, request.QuantidadeRefugo, request.TempoMinutos, operador, data, insumosJson, TrimOrNull(request.Observacoes));
    return Results.Created($"/api/ops/producao/apontamentos/{id}", ApiResponse<OpsProducaoApontamentoDto>.Ok(response, "Apontamento de producao registrado."));
})
.WithName("OpsProducaoApontamentosCriar");

app.MapGet("/api/ops/manutencao", [Authorize(Policy = "Gerente")] async (
    NexumDbContext db,
    ITenantContext tenantContext,
    string? status,
    CancellationToken ct) =>
{
    var statusFiltro = NormalizeBusinessKey(status);
    var manutencoes = await db.Database.SqlQueryRaw<OpsManutencaoDto>(
        """
        SELECT
            omt_id AS Id,
            oat_id AS AtivoId,
            omt_tipo AS Tipo,
            omt_titulo AS Titulo,
            omt_status AS Status,
            omt_data_programada AS DataProgramada,
            omt_data_inicio AS DataInicio,
            omt_data_fim AS DataFim,
            omt_responsavel_user_id AS ResponsavelUserId,
            omt_recorrencia AS Recorrencia,
            omt_custo AS Custo,
            omt_observacoes AS Observacoes,
            omt_created_at AS CriadoEm,
            omt_updated_at AS AtualizadoEm
        FROM ops_manutencoes
        WHERE tenant_id = {0}
          AND is_deleted = 0
          AND ({1} IS NULL OR omt_status = {1})
        ORDER BY omt_data_programada, omt_id
        """,
        tenantContext.TenantId.ToString(),
        (object?)statusFiltro ?? DBNull.Value)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<OpsManutencaoDto>>.Ok(manutencoes, "Manutencoes operacionais carregadas."));
})
.WithName("OpsManutencaoListar");

app.MapPost("/api/ops/manutencao", [Authorize(Policy = "Gerente")] async (
    OpsManutencaoRequest request,
    NexumDbContext db,
    ITenantContext tenantContext,
    ClaimsPrincipal principal,
    CancellationToken ct) =>
{
    var titulo = TrimOrNull(request.Titulo);
    if (string.IsNullOrWhiteSpace(titulo))
    {
        return Results.BadRequest(ApiResponse<OpsManutencaoDto>.Erro("Titulo da manutencao e obrigatorio."));
    }

    var tipo = NormalizeBusinessKey(request.Tipo) ?? "PREVENTIVA";
    var statusManutencao = NormalizeBusinessKey(request.Status) ?? "PROGRAMADA";
    var responsavel = request.ResponsavelUserId ?? GetCurrentUserId(principal);

    await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        INSERT INTO ops_manutencoes
            (tenant_id, oat_id, omt_tipo, omt_titulo, omt_status, omt_data_programada, omt_data_inicio, omt_data_fim, omt_responsavel_user_id, omt_recorrencia, omt_custo, omt_observacoes, omt_created_at, omt_updated_at)
        VALUES
            ({tenantContext.TenantId.ToString()}, {request.AtivoId}, {tipo}, {titulo}, {statusManutencao}, {request.DataProgramada}, {request.DataInicio}, {request.DataFim}, {responsavel}, {TrimOrNull(request.Recorrencia)}, {request.Custo}, {TrimOrNull(request.Observacoes)}, UTC_TIMESTAMP(), UTC_TIMESTAMP())
        """,
        ct);

    var id = await ExecuteScalarAsync<int>(db, "SELECT LAST_INSERT_ID();", ct);
    var response = new OpsManutencaoDto(id, request.AtivoId, tipo, titulo, statusManutencao, request.DataProgramada, request.DataInicio, request.DataFim, responsavel, TrimOrNull(request.Recorrencia), request.Custo, TrimOrNull(request.Observacoes), DateTime.UtcNow, DateTime.UtcNow);
    return Results.Created($"/api/ops/manutencao/{id}", ApiResponse<OpsManutencaoDto>.Ok(response, "Manutencao operacional criada."));
})
.WithName("OpsManutencaoCriar");

app.MapPut("/api/ops/manutencao/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    OpsManutencaoRequest request,
    NexumDbContext db,
    ITenantContext tenantContext,
    ClaimsPrincipal principal,
    CancellationToken ct) =>
{
    var titulo = TrimOrNull(request.Titulo);
    if (string.IsNullOrWhiteSpace(titulo))
    {
        return Results.BadRequest(ApiResponse<OpsManutencaoDto>.Erro("Titulo da manutencao e obrigatorio."));
    }

    var tipo = NormalizeBusinessKey(request.Tipo) ?? "PREVENTIVA";
    var statusManutencao = NormalizeBusinessKey(request.Status) ?? "PROGRAMADA";
    var responsavel = request.ResponsavelUserId ?? GetCurrentUserId(principal);
    var affected = await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        UPDATE ops_manutencoes
        SET oat_id = {request.AtivoId},
            omt_tipo = {tipo},
            omt_titulo = {titulo},
            omt_status = {statusManutencao},
            omt_data_programada = {request.DataProgramada},
            omt_data_inicio = {request.DataInicio},
            omt_data_fim = {request.DataFim},
            omt_responsavel_user_id = {responsavel},
            omt_recorrencia = {TrimOrNull(request.Recorrencia)},
            omt_custo = {request.Custo},
            omt_observacoes = {TrimOrNull(request.Observacoes)},
            omt_updated_at = UTC_TIMESTAMP()
        WHERE omt_id = {id} AND tenant_id = {tenantContext.TenantId.ToString()} AND is_deleted = 0
        """,
        ct);

    if (affected == 0)
    {
        return Results.NotFound(ApiResponse<OpsManutencaoDto>.Erro("Manutencao operacional nao encontrada."));
    }

    var response = new OpsManutencaoDto(id, request.AtivoId, tipo, titulo, statusManutencao, request.DataProgramada, request.DataInicio, request.DataFim, responsavel, TrimOrNull(request.Recorrencia), request.Custo, TrimOrNull(request.Observacoes), DateTime.UtcNow, DateTime.UtcNow);
    return Results.Ok(ApiResponse<OpsManutencaoDto>.Ok(response, "Manutencao operacional atualizada."));
})
.WithName("OpsManutencaoAtualizar");

app.MapDelete("/api/ops/manutencao/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    NexumDbContext db,
    ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var affected = await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        UPDATE ops_manutencoes
        SET is_deleted = 1,
            deleted_at = UTC_TIMESTAMP(),
            omt_status = 'CANCELADA',
            omt_updated_at = UTC_TIMESTAMP()
        WHERE omt_id = {id} AND tenant_id = {tenantContext.TenantId.ToString()} AND is_deleted = 0
        """,
        ct);

    return affected == 0
        ? Results.NotFound(ApiResponse<object>.Erro("Manutencao operacional nao encontrada."))
        : Results.NoContent();
})
.WithName("OpsManutencaoExcluir");

app.MapGet("/", (IHostEnvironment environment) =>
    environment.IsDevelopment() || environment.IsStaging()
        ? Results.Redirect("/swagger")
        : Results.Ok(new
        {
            status = "online",
            service = "Nexum Altivon API",
            version = releaseVersion
        }));

app.MapPost("/api/auth/login", async (
    LoginRequest request,
    NexumDbContext db,
    IDataProtectionProvider dataProtectionProvider,
    ILoggerFactory loggerFactory,
    CancellationToken ct) =>
{
    var normalizedEmail = NormalizeEmail(request.Email);
    if (string.IsNullOrWhiteSpace(normalizedEmail))
    {
        return Results.Unauthorized();
    }

    var expirationHours = builder.Configuration.GetValue("JwtSettings:ExpirationHours", 24);
    var now = DateTime.UtcNow;
    var refreshExpiresAt = now.AddDays(refreshTokenExpirationDays);

    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(item => item.Email == normalizedEmail && item.Ativo, ct);

    if (usuario is not null && BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
    {
        long? matchedTimestep = null;
        if (usuario.MfaHabilitado)
        {
            if (!TryUnprotectMfaSecret(usuario.MfaSecret, dataProtectionProvider, out var mfaSecret))
            {
                loggerFactory.CreateLogger("AuthMfa").LogError(
                    "Segredo MFA do usuario {UsuarioId} nao pode ser decifrado com o chaveiro persistente atual.",
                    usuario.Id);
                return Results.Problem(
                    "A configuracao MFA deste usuario nao pode ser lida. O administrador deve reiniciar o cadastro MFA.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            if (!TryValidateTotpCode(mfaSecret, request.MfaCode, DateTimeOffset.UtcNow, usuario.MfaUltimoPasso, out var validatedTimestep))
            {
                return Results.BadRequest(ApiResponse<LoginResponse>.Erro("MFA_REQUIRED: informe um codigo MFA valido, ainda nao utilizado, para concluir o login."));
            }

            matchedTimestep = validatedTimestep;
        }

        var refreshToken = GenerateRefreshToken();
        var refreshTokenHash = ComputeSha256Hash(refreshToken);
        if (matchedTimestep.HasValue)
        {
            var updated = await db.Usuarios
                .Where(item => item.Id == usuario.Id
                    && item.Ativo
                    && (item.MfaUltimoPasso == null || item.MfaUltimoPasso < matchedTimestep.Value))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(item => item.MfaUltimoPasso, matchedTimestep.Value)
                    .SetProperty(item => item.UltimoLogin, now)
                    .SetProperty(item => item.TokenRefresh, refreshTokenHash)
                    .SetProperty(item => item.TokenRefreshExpiraEm, refreshExpiresAt)
                    .SetProperty(item => item.UpdatedAt, now), ct);
            if (updated != 1)
            {
                return Results.BadRequest(ApiResponse<LoginResponse>.Erro("MFA_REQUIRED: o codigo MFA informado ja foi utilizado."));
            }
        }
        else
        {
            usuario.UltimoLogin = now;
            usuario.TokenRefresh = refreshTokenHash;
            usuario.TokenRefreshExpiraEm = refreshExpiresAt;
            usuario.UpdatedAt = now;
            await db.SaveChangesAsync(ct);
        }

        var perfil = usuario.Perfil.ToString();
        var usuarioResponse = CreateLoginResponse(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            perfil,
            db.CurrentTenantId,
            "usuario",
            issuer,
            audience,
            signingKey,
            expirationHours,
            refreshToken);
        return Results.Ok(ApiResponse<LoginResponse>.Ok(usuarioResponse, "Login realizado com sucesso."));
    }

    var cliente = await db.Clientes
        .FirstOrDefaultAsync(item => item.Email == normalizedEmail, ct);

    if (cliente is not null && !string.IsNullOrWhiteSpace(cliente.SenhaHash) && BCrypt.Net.BCrypt.Verify(request.Senha, cliente.SenhaHash))
    {
        if (cliente.Status != StatusCliente.Ativo)
        {
            return Results.BadRequest(ApiResponse<LoginResponse>.Erro("Seu cadastro ainda não foi confirmado. Verifique seu e-mail antes de entrar."));
        }

        var refreshToken = GenerateRefreshToken();
        cliente.UltimoAcesso = now;
        cliente.TokenRefresh = ComputeSha256Hash(refreshToken);
        cliente.TokenRefreshExpiraEm = refreshExpiresAt;
        cliente.UpdatedAt = now;
        await db.SaveChangesAsync(ct);

        var clienteResponse = CreateLoginResponse(
            cliente.Id,
            cliente.Nome,
            cliente.Email,
            "Cliente",
            db.CurrentTenantId,
            "cliente",
            issuer,
            audience,
            signingKey,
            expirationHours,
            refreshToken);
        return Results.Ok(ApiResponse<LoginResponse>.Ok(clienteResponse, "Login do cliente realizado com sucesso."));
    }

    return Results.Unauthorized();
})
.AllowAnonymous()
.WithName("Login");

app.MapPost("/api/auth/refresh", async (
    RefreshTokenRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var token = TrimOrNull(request.ResolveToken());
    var refreshToken = TrimOrNull(request.ResolveRefreshToken());
    if (token is null || refreshToken is null)
    {
        return Results.BadRequest(ApiResponse<LoginResponse>.Erro("Token e refresh token sao obrigatorios."));
    }

    var expirationHours = builder.Configuration.GetValue("JwtSettings:ExpirationHours", 24);
    var principal = ValidateExpiredJwtToken(token, issuer, audience, signingKey);
    var email = NormalizeEmail(principal?.FindFirstValue(ClaimTypes.Email) ?? principal?.FindFirstValue(JwtRegisteredClaimNames.Email));
    var subjectType = principal?.FindFirstValue("subject_type");
    var subjectIdRaw = principal?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? principal?.FindFirstValue(ClaimTypes.NameIdentifier);
    var tenantIdRaw = principal?.FindFirstValue("tenant_id");
    if (string.IsNullOrWhiteSpace(email)
        || !int.TryParse(subjectIdRaw, NumberStyles.None, CultureInfo.InvariantCulture, out var subjectId)
        || subjectId <= 0
        || !Guid.TryParse(tenantIdRaw, out var tenantId)
        || tenantId == Guid.Empty
        || subjectType is not ("usuario" or "cliente"))
    {
        return Results.Unauthorized();
    }

    var now = DateTime.UtcNow;
    var currentRefreshHash = ComputeSha256Hash(refreshToken);
    var nextRefreshToken = GenerateRefreshToken();
    var nextRefreshHash = ComputeSha256Hash(nextRefreshToken);
    var nextRefreshExpiresAt = now.AddDays(refreshTokenExpirationDays);
    LoginResponse? response = null;

    if (subjectType == "usuario")
    {
        var usuario = await db.Usuarios
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == subjectId
                && item.Email == email
                && item.Ativo
                && EF.Property<Guid>(item, "TenantId") == tenantId
                && !EF.Property<bool>(item, "IsDeleted"), ct);
        if (usuario is null)
        {
            return Results.Unauthorized();
        }

        var updated = await db.Usuarios
            .IgnoreQueryFilters()
            .Where(item => item.Id == subjectId
                && item.Email == email
                && item.Ativo
                && item.TokenRefresh == currentRefreshHash
                && item.TokenRefreshExpiraEm != null
                && item.TokenRefreshExpiraEm > now
                && EF.Property<Guid>(item, "TenantId") == tenantId
                && !EF.Property<bool>(item, "IsDeleted"))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.TokenRefresh, nextRefreshHash)
                .SetProperty(item => item.TokenRefreshExpiraEm, nextRefreshExpiresAt)
                .SetProperty(item => item.UpdatedAt, now), ct);
        if (updated != 1)
        {
            return Results.Unauthorized();
        }

        response = CreateLoginResponse(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Perfil.ToString(),
            tenantId,
            subjectType,
            issuer,
            audience,
            signingKey,
            expirationHours,
            nextRefreshToken);
    }
    else
    {
        var cliente = await db.Clientes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == subjectId
                && item.Email == email
                && item.Status == StatusCliente.Ativo
                && EF.Property<Guid>(item, "TenantId") == tenantId
                && !EF.Property<bool>(item, "IsDeleted"), ct);
        if (cliente is null)
        {
            return Results.Unauthorized();
        }

        var updated = await db.Clientes
            .IgnoreQueryFilters()
            .Where(item => item.Id == subjectId
                && item.Email == email
                && item.Status == StatusCliente.Ativo
                && item.TokenRefresh == currentRefreshHash
                && item.TokenRefreshExpiraEm != null
                && item.TokenRefreshExpiraEm > now
                && EF.Property<Guid>(item, "TenantId") == tenantId
                && !EF.Property<bool>(item, "IsDeleted"))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.TokenRefresh, nextRefreshHash)
                .SetProperty(item => item.TokenRefreshExpiraEm, nextRefreshExpiresAt)
                .SetProperty(item => item.UpdatedAt, now), ct);
        if (updated != 1)
        {
            return Results.Unauthorized();
        }

        response = CreateLoginResponse(
            cliente.Id,
            cliente.Nome,
            cliente.Email,
            "Cliente",
            tenantId,
            subjectType,
            issuer,
            audience,
            signingKey,
            expirationHours,
            nextRefreshToken);
    }

    return Results.Ok(ApiResponse<LoginResponse>.Ok(response, "Sessao renovada com sucesso."));
})
.AllowAnonymous()
.WithName("RefreshAuthToken");

app.MapPost("/api/auth/logout", [Authorize] async (
    NexumDbContext db,
    ClaimsPrincipal principal,
    CancellationToken ct) =>
{
    var userId = GetCurrentUserId(principal);
    var subjectType = principal.FindFirstValue("subject_type");
    var tenantIdRaw = principal.FindFirstValue("tenant_id");
    if (userId <= 0
        || subjectType is not ("usuario" or "cliente")
        || !Guid.TryParse(tenantIdRaw, out var tenantId)
        || tenantId == Guid.Empty)
    {
        return Results.Unauthorized();
    }

    var now = DateTime.UtcNow;
    var updated = subjectType == "usuario"
        ? await db.Usuarios
            .IgnoreQueryFilters()
            .Where(item => item.Id == userId
                && item.Ativo
                && EF.Property<Guid>(item, "TenantId") == tenantId
                && !EF.Property<bool>(item, "IsDeleted"))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.TokenRefresh, (string?)null)
                .SetProperty(item => item.TokenRefreshExpiraEm, (DateTime?)null)
                .SetProperty(item => item.UpdatedAt, now), ct)
        : await db.Clientes
            .IgnoreQueryFilters()
            .Where(item => item.Id == userId
                && item.Status == StatusCliente.Ativo
                && EF.Property<Guid>(item, "TenantId") == tenantId
                && !EF.Property<bool>(item, "IsDeleted"))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.TokenRefresh, (string?)null)
                .SetProperty(item => item.TokenRefreshExpiraEm, (DateTime?)null)
                .SetProperty(item => item.UpdatedAt, now), ct);
    if (updated != 1)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(ApiResponse<object>.Ok(new { encerrado = true }, "Sessao encerrada e refresh token revogado."));
})
.WithName("AuthLogout");

app.MapPost("/api/auth/mfa/enable", [Authorize] async (
    NexumDbContext db,
    ClaimsPrincipal principal,
    IConfiguration configuration,
    IDataProtectionProvider dataProtectionProvider,
    CancellationToken ct) =>
{
    var userId = GetCurrentUserId(principal);
    if (userId <= 0 || !string.Equals(principal.FindFirstValue("subject_type"), "usuario", StringComparison.Ordinal))
    {
        return Results.Unauthorized();
    }

    var usuario = await db.Usuarios.FirstOrDefaultAsync(item => item.Id == userId && item.Ativo, ct);
    if (usuario is null)
    {
        return Results.Unauthorized();
    }

    if (usuario.MfaHabilitado)
    {
        return Results.Conflict(ApiResponse<MfaStatusResponse>.Erro("MFA ja esta ativo para este usuario. A substituicao exige revogacao administrativa explicita."));
    }

    var rawSecret = GenerateTotpSecret();
    usuario.MfaSecret = ProtectMfaSecret(rawSecret, dataProtectionProvider);
    usuario.MfaHabilitado = false;
    usuario.MfaConfirmadoEm = null;
    usuario.MfaUltimoPasso = null;
    usuario.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync(ct);

    var issuerName = Uri.EscapeDataString(configuration["Mfa:Issuer"] ?? "GenesisGest.Net");
    var account = Uri.EscapeDataString(usuario.Email);
    var otpauth = $"otpauth://totp/{issuerName}:{account}?secret={rawSecret}&issuer={issuerName}&digits=6&period=30&algorithm=SHA1";

    return Results.Ok(ApiResponse<MfaEnableResponse>.Ok(
        new MfaEnableResponse(rawSecret, otpauth, usuario.MfaHabilitado),
        "MFA TOTP preparado. Confirme com /api/auth/mfa/verify para ativar."));
})
.WithName("AuthMfaEnable");

app.MapPost("/api/auth/mfa/verify", [Authorize] async (
    MfaVerifyRequest request,
    NexumDbContext db,
    ClaimsPrincipal principal,
    IDataProtectionProvider dataProtectionProvider,
    ILoggerFactory loggerFactory,
    CancellationToken ct) =>
{
    var userId = GetCurrentUserId(principal);
    if (userId <= 0 || !string.Equals(principal.FindFirstValue("subject_type"), "usuario", StringComparison.Ordinal))
    {
        return Results.Unauthorized();
    }

    var usuario = await db.Usuarios.FirstOrDefaultAsync(item => item.Id == userId && item.Ativo, ct);
    if (usuario is null || string.IsNullOrWhiteSpace(usuario.MfaSecret))
    {
        return Results.BadRequest(ApiResponse<MfaStatusResponse>.Erro("MFA ainda nao foi iniciado para este usuario."));
    }

    if (!TryUnprotectMfaSecret(usuario.MfaSecret, dataProtectionProvider, out var rawSecret))
    {
        loggerFactory.CreateLogger("AuthMfa").LogError(
            "Segredo MFA pendente do usuario {UsuarioId} nao pode ser decifrado com o chaveiro persistente atual.",
            usuario.Id);
        return Results.Problem(
            "A configuracao MFA pendente nao pode ser lida. Reinicie o cadastro MFA.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    if (!TryValidateTotpCode(rawSecret, request.Codigo, DateTimeOffset.UtcNow, usuario.MfaUltimoPasso, out var matchedTimestep))
    {
        return Results.BadRequest(ApiResponse<MfaStatusResponse>.Erro("Codigo MFA invalido, expirado ou ja utilizado."));
    }

    var confirmedAt = DateTime.UtcNow;
    var updated = await db.Usuarios
        .Where(item => item.Id == usuario.Id
            && item.Ativo
            && !item.MfaHabilitado
            && (item.MfaUltimoPasso == null || item.MfaUltimoPasso < matchedTimestep))
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(item => item.MfaHabilitado, true)
            .SetProperty(item => item.MfaConfirmadoEm, confirmedAt)
            .SetProperty(item => item.MfaUltimoPasso, matchedTimestep)
            .SetProperty(item => item.TokenRefresh, (string?)null)
            .SetProperty(item => item.TokenRefreshExpiraEm, (DateTime?)null)
            .SetProperty(item => item.UpdatedAt, confirmedAt), ct);
    if (updated != 1)
    {
        return Results.Conflict(ApiResponse<MfaStatusResponse>.Erro("A configuracao MFA foi alterada por outra operacao. Recarregue o estado do usuario."));
    }

    return Results.Ok(ApiResponse<MfaStatusResponse>.Ok(
        new MfaStatusResponse(true, confirmedAt),
        "MFA TOTP ativado para o usuario."));
})
.WithName("AuthMfaVerify");

app.MapGet("/api/tenants", [Authorize(Policy = "Admin")] async (NexumDbContext db, CancellationToken ct) =>
{
    var tenants = await db.Database.SqlQueryRaw<TenantDto>(
        """
        SELECT
            CAST(id AS CHAR) AS Id,
            codigo AS Codigo,
            nome AS Nome,
            documento AS Documento,
            ativo AS Ativo,
            created_at AS CriadoEm,
            updated_at AS AtualizadoEm
        FROM sys_tenants
        WHERE is_deleted = 0
        ORDER BY nome
        """)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<TenantDto>>.Ok(tenants, "Tenants corporativos carregados."));
})
.WithName("TenantsListar");

app.MapGet("/api/tenants/{id:guid}", [Authorize(Policy = "Admin")] async (Guid id, NexumDbContext db, CancellationToken ct) =>
{
    var tenant = await db.Database.SqlQueryRaw<TenantDto>(
        """
        SELECT
            CAST(id AS CHAR) AS Id,
            codigo AS Codigo,
            nome AS Nome,
            documento AS Documento,
            ativo AS Ativo,
            created_at AS CriadoEm,
            updated_at AS AtualizadoEm
        FROM sys_tenants
        WHERE id = {0} AND is_deleted = 0
        LIMIT 1
        """,
        id.ToString())
        .FirstOrDefaultAsync(ct);

    return tenant is null
        ? Results.NotFound(ApiResponse<TenantDto>.Erro("Tenant nao encontrado."))
        : Results.Ok(ApiResponse<TenantDto>.Ok(tenant, "Tenant corporativo carregado."));
})
.WithName("TenantsObter");

app.MapPost("/api/tenants", [Authorize(Policy = "Admin")] async (
    TenantUpsertRequest request,
    NexumDbContext db,
    ClaimsPrincipal principal,
    CancellationToken ct) =>
{
    var codigo = NormalizeBusinessKey(request.Codigo);
    var nome = TrimOrNull(request.Nome);
    if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nome))
    {
        return Results.BadRequest(ApiResponse<TenantDto>.Erro("Codigo e nome do tenant sao obrigatorios."));
    }

    var id = Guid.NewGuid();
    var idText = id.ToString();
    var currentUserId = GetCurrentUserGuidOrNull(principal)?.ToString();
    await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        INSERT INTO sys_tenants (id, tenant_id, codigo, nome, documento, ativo, created_by_user_id, updated_by_user_id, created_at, updated_at)
        VALUES ({idText}, {idText}, {codigo}, {nome}, {OnlyDigitsOrNull(request.Documento)}, {request.Ativo}, {currentUserId}, {currentUserId}, UTC_TIMESTAMP(), UTC_TIMESTAMP())
        """,
        ct);

    var response = new TenantDto(idText, codigo, nome, OnlyDigitsOrNull(request.Documento), request.Ativo, DateTime.UtcNow, DateTime.UtcNow);
    return Results.Created($"/api/tenants/{idText}", ApiResponse<TenantDto>.Ok(response, "Tenant criado."));
})
.WithName("TenantsCriar");

app.MapPut("/api/tenants/{id:guid}", [Authorize(Policy = "Admin")] async (
    Guid id,
    TenantUpsertRequest request,
    NexumDbContext db,
    ClaimsPrincipal principal,
    CancellationToken ct) =>
{
    var codigo = NormalizeBusinessKey(request.Codigo);
    var nome = TrimOrNull(request.Nome);
    if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nome))
    {
        return Results.BadRequest(ApiResponse<TenantDto>.Erro("Codigo e nome do tenant sao obrigatorios."));
    }

    var currentUserId = GetCurrentUserGuidOrNull(principal)?.ToString();
    var affected = await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        UPDATE sys_tenants
        SET codigo = {codigo},
            nome = {nome},
            documento = {OnlyDigitsOrNull(request.Documento)},
            ativo = {request.Ativo},
            updated_by_user_id = {currentUserId},
            updated_at = UTC_TIMESTAMP()
        WHERE id = {id.ToString()} AND is_deleted = 0
        """,
        ct);

    if (affected == 0)
    {
        return Results.NotFound(ApiResponse<TenantDto>.Erro("Tenant nao encontrado."));
    }

    var response = new TenantDto(id.ToString(), codigo, nome, OnlyDigitsOrNull(request.Documento), request.Ativo, DateTime.UtcNow, DateTime.UtcNow);
    return Results.Ok(ApiResponse<TenantDto>.Ok(response, "Tenant atualizado."));
})
.WithName("TenantsAtualizar");

app.MapDelete("/api/tenants/{id:guid}", [Authorize(Policy = "Admin")] async (Guid id, NexumDbContext db, ClaimsPrincipal principal, CancellationToken ct) =>
{
    var currentUserId = GetCurrentUserGuidOrNull(principal)?.ToString();
    var affected = await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        UPDATE sys_tenants
        SET ativo = 0,
            is_deleted = 1,
            deleted_at = UTC_TIMESTAMP(),
            updated_by_user_id = {currentUserId},
            updated_at = UTC_TIMESTAMP()
        WHERE id = {id.ToString()} AND is_deleted = 0
        """,
        ct);

    return affected == 0
        ? Results.NotFound(ApiResponse<object>.Erro("Tenant nao encontrado."))
        : Results.NoContent();
})
.WithName("TenantsExcluir");

app.MapGet("/api/workflows/definicoes", [Authorize(Policy = "Gerente")] async (
    string? entidade,
    NexumDbContext db,
    ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var filtroEntidade = NormalizeBusinessKey(entidade);
    var definicoes = await db.Database.SqlQueryRaw<WorkflowDefinicaoDto>(
        """
        SELECT
            CAST(id AS CHAR) AS Id,
            entidade AS Entidade,
            codigo AS Codigo,
            nome AS Nome,
            estados_json AS EstadosJson,
            transicoes_json AS TransicoesJson,
            ativo AS Ativo,
            created_at AS CriadoEm,
            updated_at AS AtualizadoEm
        FROM sys_workflow_definicoes
        WHERE tenant_id = {0}
          AND is_deleted = 0
          AND ({1} = '' OR entidade = {1})
        ORDER BY entidade, nome
        """,
        tenantContext.TenantId.ToString(),
        filtroEntidade)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<WorkflowDefinicaoDto>>.Ok(definicoes, "Definicoes de workflow carregadas."));
})
.WithName("WorkflowsDefinicoesListar");

app.MapGet("/api/workflows/definicoes/{id:guid}", [Authorize(Policy = "Gerente")] async (
    Guid id,
    NexumDbContext db,
    ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var definicao = await LoadWorkflowDefinitionAsync(db, tenantContext.TenantId, id, ct);
    return definicao is null
        ? Results.NotFound(ApiResponse<WorkflowDefinicaoDto>.Erro("Definicao de workflow nao encontrada."))
        : Results.Ok(ApiResponse<WorkflowDefinicaoDto>.Ok(definicao, "Definicao de workflow carregada."));
})
.WithName("WorkflowsDefinicoesObter");

app.MapPost("/api/workflows/definicoes", [Authorize(Policy = "Gerente")] async (
    WorkflowDefinicaoRequest request,
    NexumDbContext db,
    ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var definicao = NormalizeWorkflowDefinition(request, out var validationError);
    if (definicao is null)
    {
        return Results.BadRequest(ApiResponse<WorkflowDefinicaoDto>.Erro(validationError));
    }

    var duplicateCount = await db.Database.SqlQueryRaw<int>(
        """
        SELECT COUNT(*) AS Value
        FROM sys_workflow_definicoes
        WHERE tenant_id = {0} AND codigo = {1} AND is_deleted = 0
        """,
        tenantContext.TenantId.ToString(),
        definicao.Codigo)
        .SingleAsync(ct);
    if (duplicateCount > 0)
    {
        return Results.Conflict(ApiResponse<WorkflowDefinicaoDto>.Erro("Ja existe uma definicao ativa com este codigo no tenant."));
    }

    var estadosJson = JsonSerializer.Serialize(definicao.Estados);
    var transicoesJson = JsonSerializer.Serialize(definicao.Transicoes);
    var deletedDefinitionIds = await db.Database.SqlQueryRaw<string>(
        """
        SELECT CAST(id AS CHAR) AS Value
        FROM sys_workflow_definicoes
        WHERE tenant_id = {0} AND codigo = {1} AND is_deleted = 1
        LIMIT 1
        """,
        tenantContext.TenantId.ToString(),
        definicao.Codigo)
        .ToListAsync(ct);
    var deletedDefinitionId = deletedDefinitionIds.FirstOrDefault();
    var id = deletedDefinitionId ?? Guid.NewGuid().ToString();

    if (deletedDefinitionId is not null)
    {
        var restored = await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE sys_workflow_definicoes
            SET entidade = {definicao.Entidade},
                nome = {definicao.Nome},
                estados_json = {estadosJson},
                transicoes_json = {transicoesJson},
                ativo = {definicao.Ativo},
                is_deleted = 0,
                deleted_at = NULL,
                updated_at = UTC_TIMESTAMP()
            WHERE id = {deletedDefinitionId} AND tenant_id = {tenantContext.TenantId.ToString()} AND is_deleted = 1
            """,
            ct);
        if (restored != 1)
        {
            return Results.Conflict(ApiResponse<WorkflowDefinicaoDto>.Erro("A definicao foi alterada por outra operacao. Recarregue a lista."));
        }
    }
    else
    {
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"""
                INSERT INTO sys_workflow_definicoes
                    (id, tenant_id, entidade, codigo, nome, estados_json, transicoes_json, ativo, created_at, updated_at)
                VALUES
                    ({id}, {tenantContext.TenantId.ToString()}, {definicao.Entidade}, {definicao.Codigo}, {definicao.Nome}, {estadosJson}, {transicoesJson}, {definicao.Ativo}, UTC_TIMESTAMP(), UTC_TIMESTAMP())
                """,
                ct);
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Results.Conflict(ApiResponse<WorkflowDefinicaoDto>.Erro("Ja existe uma definicao ativa com este codigo no tenant."));
        }
    }

    var response = new WorkflowDefinicaoDto(id, definicao.Entidade, definicao.Codigo, definicao.Nome, estadosJson, transicoesJson, definicao.Ativo, DateTime.UtcNow, DateTime.UtcNow);
    return Results.Created($"/api/workflows/definicoes/{id}", ApiResponse<WorkflowDefinicaoDto>.Ok(response, "Definicao de workflow criada."));
})
.WithName("WorkflowsDefinicoesCriar");

app.MapPut("/api/workflows/definicoes/{id:guid}", [Authorize(Policy = "Gerente")] async (
    Guid id,
    WorkflowDefinicaoRequest request,
    NexumDbContext db,
    ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var atual = await LoadWorkflowDefinitionAsync(db, tenantContext.TenantId, id, ct);
    if (atual is null)
    {
        return Results.NotFound(ApiResponse<WorkflowDefinicaoDto>.Erro("Definicao de workflow nao encontrada."));
    }

    var definicao = NormalizeWorkflowDefinition(request, out var validationError);
    if (definicao is null)
    {
        return Results.BadRequest(ApiResponse<WorkflowDefinicaoDto>.Erro(validationError));
    }

    var duplicateCount = await db.Database.SqlQueryRaw<int>(
        """
        SELECT COUNT(*) AS Value
        FROM sys_workflow_definicoes
        WHERE tenant_id = {0} AND codigo = {1} AND id <> {2} AND is_deleted = 0
        """,
        tenantContext.TenantId.ToString(),
        definicao.Codigo,
        id.ToString())
        .SingleAsync(ct);
    if (duplicateCount > 0)
    {
        return Results.Conflict(ApiResponse<WorkflowDefinicaoDto>.Erro("Ja existe uma definicao ativa com este codigo no tenant."));
    }

    var estadosEmUso = await db.Database.SqlQueryRaw<string>(
        """
        SELECT DISTINCT estado_atual AS Value
        FROM sys_workflow_instancias
        WHERE tenant_id = {0} AND definicao_id = {1} AND is_deleted = 0
        """,
        tenantContext.TenantId.ToString(),
        id.ToString())
        .ToListAsync(ct);
    var estadosRemovidosEmUso = estadosEmUso
        .Where(estado => !definicao.Estados.Contains(estado, StringComparer.OrdinalIgnoreCase))
        .ToList();
    if (estadosRemovidosEmUso.Count > 0)
    {
        return Results.Conflict(ApiResponse<WorkflowDefinicaoDto>.Erro(
            $"A definicao nao pode remover estados utilizados por instancias ativas: {string.Join(", ", estadosRemovidosEmUso)}."));
    }
    if (!definicao.Ativo && estadosEmUso.Count > 0)
    {
        return Results.Conflict(ApiResponse<WorkflowDefinicaoDto>.Erro("A definicao possui instancias ativas e nao pode ser desativada."));
    }

    var estadosJson = JsonSerializer.Serialize(definicao.Estados);
    var transicoesJson = JsonSerializer.Serialize(definicao.Transicoes);

    var affected = await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        UPDATE sys_workflow_definicoes
        SET entidade = {definicao.Entidade},
            codigo = {definicao.Codigo},
            nome = {definicao.Nome},
            estados_json = {estadosJson},
            transicoes_json = {transicoesJson},
            ativo = {definicao.Ativo},
            updated_at = UTC_TIMESTAMP()
        WHERE id = {id.ToString()} AND tenant_id = {tenantContext.TenantId.ToString()} AND is_deleted = 0
        """,
        ct);

    if (affected == 0)
    {
        return Results.NotFound(ApiResponse<WorkflowDefinicaoDto>.Erro("Definicao de workflow nao encontrada."));
    }

    var response = new WorkflowDefinicaoDto(id.ToString(), definicao.Entidade, definicao.Codigo, definicao.Nome, estadosJson, transicoesJson, definicao.Ativo, atual.CriadoEm, DateTime.UtcNow);
    return Results.Ok(ApiResponse<WorkflowDefinicaoDto>.Ok(response, "Definicao de workflow atualizada."));
})
.WithName("WorkflowsDefinicoesAtualizar");

app.MapDelete("/api/workflows/definicoes/{id:guid}", [Authorize(Policy = "Gerente")] async (
    Guid id,
    NexumDbContext db,
    ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var activeInstanceCount = await db.Database.SqlQueryRaw<int>(
        """
        SELECT COUNT(*) AS Value
        FROM sys_workflow_instancias
        WHERE tenant_id = {0} AND definicao_id = {1} AND is_deleted = 0
        """,
        tenantContext.TenantId.ToString(),
        id.ToString())
        .SingleAsync(ct);
    if (activeInstanceCount > 0)
    {
        return Results.Conflict(ApiResponse<object>.Erro("A definicao possui instancias ativas e nao pode ser excluida."));
    }

    var affected = await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        UPDATE sys_workflow_definicoes
        SET ativo = 0,
            is_deleted = 1,
            deleted_at = UTC_TIMESTAMP(),
            updated_at = UTC_TIMESTAMP()
        WHERE id = {id.ToString()} AND tenant_id = {tenantContext.TenantId.ToString()} AND is_deleted = 0
        """,
        ct);

    return affected == 0
        ? Results.NotFound(ApiResponse<object>.Erro("Definicao de workflow nao encontrada."))
        : Results.NoContent();
})
.WithName("WorkflowsDefinicoesExcluir");

app.MapPost("/api/workflows/instancias", [Authorize(Policy = "Gerente")] async (
    WorkflowInstanciaRequest request,
    NexumDbContext db,
    ITenantContext tenantContext,
    ClaimsPrincipal principal,
    CancellationToken ct) =>
{
    var definicaoDto = await LoadWorkflowDefinitionAsync(db, tenantContext.TenantId, request.DefinicaoId, ct);
    if (definicaoDto is null || !definicaoDto.Ativo)
    {
        return Results.NotFound(ApiResponse<WorkflowInstanciaDto>.Erro("Definicao de workflow ativa nao encontrada."));
    }

    if (!TryParseWorkflowDefinition(definicaoDto, out var definicao, out var definitionError) || definicao is null)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Definicao de workflow inconsistente",
            detail: definitionError);
    }

    var entidade = NormalizeBusinessKey(request.Entidade);
    var registroChave = TrimOrNull(request.RegistroChave);
    var estadoInicial = string.IsNullOrWhiteSpace(request.EstadoInicial)
        ? definicao.Estados[0]
        : NormalizeBusinessKey(request.EstadoInicial);
    if (string.IsNullOrWhiteSpace(entidade) || string.IsNullOrWhiteSpace(registroChave))
    {
        return Results.BadRequest(ApiResponse<WorkflowInstanciaDto>.Erro("Entidade e registro sao obrigatorios."));
    }
    if (!string.Equals(entidade, definicao.Entidade, StringComparison.OrdinalIgnoreCase))
    {
        return Results.BadRequest(ApiResponse<WorkflowInstanciaDto>.Erro("A entidade da instancia nao corresponde a entidade da definicao."));
    }
    if (registroChave.Length > 120)
    {
        return Results.BadRequest(ApiResponse<WorkflowInstanciaDto>.Erro("A chave do registro deve ter no maximo 120 caracteres."));
    }
    if (!definicao.Estados.Contains(estadoInicial, StringComparer.OrdinalIgnoreCase))
    {
        return Results.BadRequest(ApiResponse<WorkflowInstanciaDto>.Erro("O estado inicial nao pertence a definicao do workflow."));
    }

    var currentUserId = GetCurrentUserId(principal);
    if (currentUserId <= 0)
    {
        return Results.Unauthorized();
    }

    var duplicateCount = await db.Database.SqlQueryRaw<int>(
        """
        SELECT COUNT(*) AS Value
        FROM sys_workflow_instancias
        WHERE tenant_id = {0}
          AND definicao_id = {1}
          AND entidade = {2}
          AND registro_chave = {3}
          AND is_deleted = 0
        """,
        tenantContext.TenantId.ToString(),
        request.DefinicaoId.ToString(),
        entidade,
        registroChave)
        .SingleAsync(ct);
    if (duplicateCount > 0)
    {
        return Results.Conflict(ApiResponse<WorkflowInstanciaDto>.Erro("Ja existe uma instancia ativa para este registro e definicao."));
    }

    var id = Guid.NewGuid().ToString();
    await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        INSERT INTO sys_workflow_instancias
            (id, tenant_id, definicao_id, entidade, registro_chave, estado_atual, solicitante_user_id, observacao, created_at, updated_at)
        VALUES
            ({id}, {tenantContext.TenantId.ToString()}, {request.DefinicaoId.ToString()}, {entidade}, {registroChave}, {estadoInicial}, {currentUserId}, {TrimOrNull(request.Observacao)}, UTC_TIMESTAMP(), UTC_TIMESTAMP())
        """,
        ct);

    var response = new WorkflowInstanciaDto(id, request.DefinicaoId.ToString(), entidade, registroChave, estadoInicial, currentUserId, DateTime.UtcNow, DateTime.UtcNow);
    return Results.Created($"/api/workflows/instancias/{id}", ApiResponse<WorkflowInstanciaDto>.Ok(response, "Instancia de workflow aberta."));
})
.WithName("WorkflowsInstanciasCriar");

app.MapGet("/api/workflows/instancias", [Authorize(Policy = "Gerente")] async (
    string? entidade,
    string? estado,
    string? registroChave,
    NexumDbContext db,
    ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var entidadeFiltro = NormalizeBusinessKey(entidade);
    var estadoFiltro = NormalizeBusinessKey(estado);
    var registroFiltro = TrimOrNull(registroChave);
    var instancias = await db.Database.SqlQueryRaw<WorkflowInstanciaDto>(
        """
        SELECT
            CAST(id AS CHAR) AS Id,
            CAST(definicao_id AS CHAR) AS DefinicaoId,
            entidade AS Entidade,
            registro_chave AS RegistroChave,
            estado_atual AS EstadoAtual,
            solicitante_user_id AS SolicitanteUserId,
            created_at AS CriadoEm,
            updated_at AS AtualizadoEm
        FROM sys_workflow_instancias
        WHERE tenant_id = {0}
          AND is_deleted = 0
          AND ({1} = '' OR entidade = {1})
          AND ({2} = '' OR estado_atual = {2})
          AND ({3} IS NULL OR registro_chave = {3})
        ORDER BY updated_at DESC, created_at DESC
        LIMIT 500
        """,
        tenantContext.TenantId.ToString(),
        entidadeFiltro,
        estadoFiltro,
        (object?)registroFiltro ?? DBNull.Value)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<WorkflowInstanciaDto>>.Ok(instancias, "Instancias de workflow carregadas.", instancias.Count));
})
.WithName("WorkflowsInstanciasListar");

app.MapGet("/api/workflows/instancias/{id:guid}", [Authorize(Policy = "Gerente")] async (
    Guid id,
    NexumDbContext db,
    ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var instancia = await LoadWorkflowInstanceAsync(db, tenantContext.TenantId, id, ct);

    return instancia is null
        ? Results.NotFound(ApiResponse<WorkflowInstanciaDto>.Erro("Instancia de workflow nao encontrada."))
        : Results.Ok(ApiResponse<WorkflowInstanciaDto>.Ok(instancia, "Instancia de workflow carregada."));
})
.WithName("WorkflowsInstanciasObter");

app.MapGet("/api/workflows/instancias/{id:guid}/transicoes", [Authorize(Policy = "Gerente")] async (
    Guid id,
    NexumDbContext db,
    ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var instancia = await LoadWorkflowInstanceAsync(db, tenantContext.TenantId, id, ct);
    if (instancia is null)
    {
        return Results.NotFound(ApiResponse<List<WorkflowTransicaoDto>>.Erro("Instancia de workflow nao encontrada."));
    }

    var transicoes = await db.Database.SqlQueryRaw<WorkflowTransicaoDto>(
        """
        SELECT
            CAST(id AS CHAR) AS Id,
            CAST(instancia_id AS CHAR) AS InstanciaId,
            estado_origem AS EstadoOrigem,
            estado_destino AS EstadoDestino,
            acao AS Acao,
            usuario_id AS UsuarioId,
            created_at AS CriadoEm
        FROM sys_workflow_transicoes
        WHERE instancia_id = {0} AND tenant_id = {1}
        ORDER BY created_at, id
        """,
        id.ToString(),
        tenantContext.TenantId.ToString())
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<WorkflowTransicaoDto>>.Ok(transicoes, "Historico de transicoes carregado.", transicoes.Count));
})
.WithName("WorkflowsInstanciasTransicoesListar");

app.MapPost("/api/workflows/instancias/{id:guid}/transicoes", [Authorize(Policy = "Gerente")] async (
    Guid id,
    WorkflowTransicaoRequest request,
    NexumDbContext db,
    ITenantContext tenantContext,
    ClaimsPrincipal principal,
    CancellationToken ct) =>
{
    var destino = NormalizeBusinessKey(request.EstadoDestino);
    var acaoSolicitada = NormalizeBusinessKey(request.Acao);
    if (string.IsNullOrWhiteSpace(destino))
    {
        return Results.BadRequest(ApiResponse<WorkflowTransicaoDto>.Erro("Estado de destino e obrigatorio."));
    }

    var executionStrategy = db.Database.CreateExecutionStrategy();
    return await executionStrategy.ExecuteAsync(async () =>
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var instancia = await LoadWorkflowInstanceAsync(db, tenantContext.TenantId, id, ct);
        if (instancia is null)
        {
            await transaction.RollbackAsync(ct);
            return (IResult)Results.NotFound(ApiResponse<WorkflowTransicaoDto>.Erro("Instancia de workflow nao encontrada."));
        }

        if (!Guid.TryParse(instancia.DefinicaoId, out var definicaoId))
        {
            await transaction.RollbackAsync(ct);
            return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Instancia de workflow inconsistente",
                detail: "O identificador da definicao vinculada nao possui formato UUID valido.");
        }

        var definicaoDto = await LoadWorkflowDefinitionAsync(db, tenantContext.TenantId, definicaoId, ct);
        if (definicaoDto is null || !definicaoDto.Ativo)
        {
            await transaction.RollbackAsync(ct);
            return Results.Conflict(ApiResponse<WorkflowTransicaoDto>.Erro("A definicao vinculada a instancia nao esta ativa."));
        }
        if (!TryParseWorkflowDefinition(definicaoDto, out var definicao, out var definitionError) || definicao is null)
        {
            await transaction.RollbackAsync(ct);
            return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Definicao de workflow inconsistente",
                detail: definitionError);
        }

        var candidatas = definicao.Transicoes
            .Where(regra => string.Equals(regra.Origem, instancia.EstadoAtual, StringComparison.OrdinalIgnoreCase)
                && string.Equals(regra.Destino, destino, StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(acaoSolicitada) || string.Equals(regra.Acao, acaoSolicitada, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        if (candidatas.Count == 0)
        {
            await transaction.RollbackAsync(ct);
            return Results.Conflict(ApiResponse<WorkflowTransicaoDto>.Erro(
                $"Nao existe transicao configurada de {instancia.EstadoAtual} para {destino}."));
        }
        if (candidatas.Count > 1)
        {
            await transaction.RollbackAsync(ct);
            return Results.BadRequest(ApiResponse<WorkflowTransicaoDto>.Erro("Informe a acao para selecionar uma transicao sem ambiguidade."));
        }

        var regraSelecionada = candidatas[0];
        if (!IsWorkflowProfileAuthorized(principal, regraSelecionada.PerfisAutorizados))
        {
            await transaction.RollbackAsync(ct);
            return Results.Forbid();
        }

        var currentUserId = GetCurrentUserId(principal);
        if (currentUserId <= 0)
        {
            await transaction.RollbackAsync(ct);
            return Results.Unauthorized();
        }

        var affected = await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE sys_workflow_instancias
            SET estado_atual = {regraSelecionada.Destino},
                updated_at = UTC_TIMESTAMP()
            WHERE id = {id.ToString()}
              AND tenant_id = {tenantContext.TenantId.ToString()}
              AND estado_atual = {instancia.EstadoAtual}
              AND is_deleted = 0
            """,
            ct);
        if (affected != 1)
        {
            await transaction.RollbackAsync(ct);
            return Results.Conflict(ApiResponse<WorkflowTransicaoDto>.Erro("A instancia foi alterada por outra operacao. Recarregue o estado atual."));
        }

        var transicaoId = Guid.NewGuid().ToString();
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO sys_workflow_transicoes
                (id, tenant_id, instancia_id, estado_origem, estado_destino, acao, usuario_id, observacao, created_at)
            VALUES
                ({transicaoId}, {tenantContext.TenantId.ToString()}, {id.ToString()}, {instancia.EstadoAtual}, {regraSelecionada.Destino}, {regraSelecionada.Acao}, {currentUserId}, {TrimOrNull(request.Observacao)}, UTC_TIMESTAMP())
            """,
            ct);

        await transaction.CommitAsync(ct);

        var response = new WorkflowTransicaoDto(transicaoId, id.ToString(), instancia.EstadoAtual, regraSelecionada.Destino, regraSelecionada.Acao, currentUserId, DateTime.UtcNow);
        return Results.Ok(ApiResponse<WorkflowTransicaoDto>.Ok(response, "Transicao de workflow registrada."));
    });
})
.WithName("WorkflowsInstanciasTransicionar");

app.MapDelete("/api/workflows/instancias/{id:guid}", [Authorize(Policy = "Gerente")] async (
    Guid id,
    NexumDbContext db,
    ITenantContext tenantContext,
    CancellationToken ct) =>
{
    var affected = await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        UPDATE sys_workflow_instancias
        SET is_deleted = 1,
            deleted_at = UTC_TIMESTAMP(),
            updated_at = UTC_TIMESTAMP()
        WHERE id = {id.ToString()} AND tenant_id = {tenantContext.TenantId.ToString()} AND is_deleted = 0
        """,
        ct);

    return affected == 0
        ? Results.NotFound(ApiResponse<object>.Erro("Instancia de workflow nao encontrada."))
        : Results.NoContent();
})
.WithName("WorkflowsInstanciasExcluir");

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

app.MapGet("/api/admin/usuarios", [Authorize(Policy = "Admin")] async (NexumDbContext db, CancellationToken ct) =>
{
    var usuarios = await db.Usuarios
        .AsNoTracking()
        .OrderBy(usuario => usuario.Nome)
        .Select(usuario => new UsuarioAcessoDto(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Perfil.ToString(),
            usuario.Ativo,
            usuario.Telefone,
            usuario.UltimoLogin,
            usuario.UpdatedAt))
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<UsuarioAcessoDto>>.Ok(
        usuarios,
        "Usuarios administrativos carregados para GenesisGest.Net e Nexum."));
})
.WithName("AdminUsuariosListar");

app.MapPost("/api/admin/usuarios", [Authorize(Policy = "Admin")] async (
    UsuarioAcessoUpsertRequest request,
    NexumDbContext db,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    CancellationToken ct) =>
{
    var email = NormalizeEmail(request.Email);
    var nome = TrimOrNull(request.Nome);
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(nome))
    {
        return Results.BadRequest(ApiResponse<UsuarioAcessoDto>.Erro("Nome e e-mail sao obrigatorios."));
    }

    if (!Enum.TryParse<PerfilUsuario>(request.Perfil, true, out var perfil))
    {
        return Results.BadRequest(ApiResponse<UsuarioAcessoDto>.Erro("Perfil invalido."));
    }

    var usuarioExistente = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email, ct);
    if (usuarioExistente is not null)
    {
        usuarioExistente.Nome = nome;
        usuarioExistente.Perfil = perfil;
        usuarioExistente.Ativo = request.Ativo;
        usuarioExistente.Telefone = TrimOrNull(request.Telefone);
        if (!string.IsNullOrWhiteSpace(request.Senha))
        {
            usuarioExistente.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);
        }
        usuarioExistente.UpdatedAt = DateTime.UtcNow;

        db.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "sys_usuarios", usuarioExistente.Id, AcaoAuditoria.UPDATE, null, usuarioExistente));
        await db.SaveChangesAsync(ct);

        var dtoExistente = new UsuarioAcessoDto(
            usuarioExistente.Id,
            usuarioExistente.Nome,
            usuarioExistente.Email,
            usuarioExistente.Perfil.ToString(),
            usuarioExistente.Ativo,
            usuarioExistente.Telefone,
            usuarioExistente.UltimoLogin,
            usuarioExistente.UpdatedAt);

        return Results.Ok(ApiResponse<UsuarioAcessoDto>.Ok(dtoExistente, "Usuario atualizado com sucesso."));
    }

    if (string.IsNullOrWhiteSpace(request.Senha))
    {
        return Results.BadRequest(ApiResponse<UsuarioAcessoDto>.Erro("Senha e obrigatoria para novos usuarios."));
    }

    var novoUsuario = new Usuario
    {
        Nome = nome,
        Email = email,
        SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
        Perfil = perfil,
        Ativo = request.Ativo,
        Telefone = TrimOrNull(request.Telefone),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.Usuarios.Add(novoUsuario);
    await db.SaveChangesAsync(ct);

    db.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "sys_usuarios", novoUsuario.Id, AcaoAuditoria.INSERT, null, novoUsuario));
    await db.SaveChangesAsync(ct);

    var dtoNovo = new UsuarioAcessoDto(
        novoUsuario.Id,
        novoUsuario.Nome,
        novoUsuario.Email,
        novoUsuario.Perfil.ToString(),
        novoUsuario.Ativo,
        novoUsuario.Telefone,
        novoUsuario.UltimoLogin,
        novoUsuario.UpdatedAt);

    return Results.Created($"/api/admin/usuarios/{novoUsuario.Id}", ApiResponse<UsuarioAcessoDto>.Ok(dtoNovo, "Usuario criado com sucesso."));
})
.WithName("AdminUsuariosCriar");

app.Run();

// Helper Methods
static string GetReleaseVersion() => "1.1.5.7225";

static string ResolveJwtSecret(IConfiguration config)
{
    return config["JwtSettings:SecretKey"]
        ?? config["JwtSettings__SecretKey"]
        ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
        ?? "S3cur3S3cr3tK3yF0rJ3wT1A2B3C4D5E6F7G8H9I0!";
}

static string? ResolveConfiguredConnectionString(IConfiguration config, params string[] names)
{
    foreach (var name in names)
    {
        var conn = config.GetConnectionString(name) ?? config[$"ConnectionStrings:{name}"];
        if (!string.IsNullOrWhiteSpace(conn)) return conn;
    }
    return null;
}

static string[] GetCorsOrigins(IConfiguration config)
{
    var origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>();
    return origins ?? new[] { "https://genesisgest.net", "https://admin.nexumaltivon.com" };
}

static async Task EnsureOperationalSchemaAsync(IServiceProvider services, ILogger logger)
{
    try
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexumDbContext>();
        await db.Database.EnsureCreatedAsync();
        logger.LogInformation("Operational schema verificado/aplicado com sucesso.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro ao garantir o schema operacional.");
    }
}

static async Task<IResult> CheckMySqlHealthAsync(string? connectionString, string fallbackStatus, string dbName, CancellationToken ct)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return Results.Ok(new { status = fallbackStatus, database = dbName });
    }

    try
    {
        using var conn = new MySqlConnection(connectionString);
        await conn.OpenAsync(ct);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1;";
        await cmd.ExecuteScalarAsync(ct);

        return Results.Ok(new { status = "Healthy", database = dbName });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Falha de conexao ao banco MySQL '{dbName}': {ex.Message}");
    }
}

static bool TryResolveRedisEndpoint(string connectionString, out string host, out int port, out string? error)
{
    host = "localhost";
    port = 6379;
    error = null;

    try
    {
        var parts = connectionString.Split(',');
        var endpoint = parts[0].Trim();
        var hostPort = endpoint.Split(':');
        host = hostPort[0];
        if (hostPort.Length > 1 && int.TryParse(hostPort[1], out var p))
        {
            port = p;
        }
        return true;
    }
    catch (Exception ex)
    {
        error = ex.Message;
        return false;
    }
}

static async Task<string> BuildYaraOperationalContextAsync(NexumDbContext db, string mensagem, CancellationToken ct)
{
    await Task.Yield();
    return "Contexto operacional de atendimento publico da assistente Yara.";
}

static LogAuditoria CreateIamAuditLog(ClaimsPrincipal principal, HttpContext context, string tabela, object id, AcaoAuditoria acao, object? anterior, object? atual)
{
    var userId = GetCurrentUserId(principal);
    return new LogAuditoria
    {
        Tabela = tabela,
        RegistroId = id.ToString() ?? "0",
        Acao = acao,
        UsuarioId = userId > 0 ? userId : null,
        Ip = context.Connection.RemoteIpAddress?.ToString(),
        DadosAnteriores = anterior != null ? JsonSerializer.Serialize(anterior) : null,
        DadosNovos = atual != null ? JsonSerializer.Serialize(atual) : null,
        CreatedAt = DateTime.UtcNow
    };
}

static bool ValidateDesktopTerminalAccess(HttpRequest request, IConfiguration config, out string terminalIdentity, out string? rejection)
{
    terminalIdentity = "Terminal-Desktop";
    rejection = null;
    if (request.Headers.TryGetValue("X-Terminal-Key", out var key) && !string.IsNullOrWhiteSpace(key))
    {
        terminalIdentity = $"Terminal-{key}";
        return true;
    }
    return true;
}

static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

static string? NormalizeBusinessKey(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

static string? NormalizeEmail(string? email) => string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

static string? OnlyDigitsOrNull(string? value)
{
    if (string.IsNullOrWhiteSpace(value)) return null;
    var digits = new string(value.Where(char.IsDigit).ToArray());
    return string.IsNullOrWhiteSpace(digits) ? null : digits;
}

static int GetCurrentUserId(ClaimsPrincipal principal)
{
    var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
    return int.TryParse(sub, out var id) ? id : 0;
}

static Guid? GetCurrentUserGuidOrNull(ClaimsPrincipal principal)
{
    var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
    return Guid.TryParse(sub, out var guid) ? guid : null;
}

static async Task<T?> ExecuteScalarAsync<T>(NexumDbContext db, string sql, CancellationToken ct)
{
    using var command = db.Database.GetDbConnection().CreateCommand();
    command.CommandText = sql;
    if (db.Database.CurrentTransaction != null)
    {
        command.Transaction = db.Database.CurrentTransaction.GetDbTransaction();
    }
    if (command.Connection?.State != ConnectionState.Open)
    {
        await db.Database.OpenConnectionAsync(ct);
    }
    var result = await command.ExecuteScalarAsync(ct);
    if (result is DBNull || result is null) return default;
    return (T)Convert.ChangeType(result, typeof(T));
}

static async Task ReplaceOpsOrdemItensAsync(NexumDbContext db, Guid tenantId, int ordemId, List<OpsOrdemServicoItemRequest>? itens, CancellationToken ct)
{
    await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM ops_ordem_servico_itens WHERE oso_id = {ordemId} AND tenant_id = {tenantId.ToString()}", ct);
    if (itens == null || itens.Count == 0) return;

    foreach (var item in itens)
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO ops_ordem_servico_itens
                (tenant_id, oso_id, osi_tipo, osi_codigo, osi_descricao, osi_quantidade, osi_unidade, osi_custo_unitario, osi_total)
            VALUES
                ({tenantId.ToString()}, {ordemId}, {NormalizeBusinessKey(item.Tipo)}, {NormalizeBusinessKey(item.Codigo)}, {TrimOrNull(item.Descricao)}, {item.Quantidade}, {NormalizeBusinessKey(item.Unidade)}, {item.CustoUnitario}, {item.Quantidade * item.CustoUnitario})
            """, ct);
    }
}

static string GenerateRefreshToken()
{
    var randomNumber = new byte[64];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomNumber);
    return Convert.ToBase64String(randomNumber);
}

static string ComputeSha256Hash(string rawData)
{
    using var sha256 = SHA256.Create();
    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
    return Convert.ToHexString(bytes);
}

static LoginResponse CreateLoginResponse(
    int id,
    string nome,
    string email,
    string perfil,
    Guid tenantId,
    string subjectType,
    string issuer,
    string audience,
    SymmetricSecurityKey signingKey,
    int expirationHours,
    string refreshToken)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var expires = DateTime.UtcNow.AddHours(expirationHours);
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, id.ToString(CultureInfo.InvariantCulture)),
        new Claim(ClaimTypes.NameIdentifier, id.ToString(CultureInfo.InvariantCulture)),
        new Claim(ClaimTypes.Name, nome),
        new Claim(ClaimTypes.Email, email),
        new Claim(ClaimTypes.Role, perfil),
        new Claim("tenant_id", tenantId.ToString()),
        new Claim("subject_type", subjectType)
    };

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = expires,
        Issuer = issuer,
        Audience = audience,
        SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return new LoginResponse(tokenHandler.WriteToken(token), refreshToken, expires, nome, email, perfil, tenantId.ToString());
}

static ClaimsPrincipal? ValidateExpiredJwtToken(string token, string issuer, string audience, SymmetricSecurityKey signingKey)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var validationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = signingKey
    };

    try
    {
        var principal = tokenHandler.ValidateToken(token, validationParameters, out var securityToken);
        if (securityToken is JwtSecurityToken jwtToken && jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
        {
            return principal;
        }
    }
    catch
    {
        // Ignore validation failures
    }
    return null;
}

static string GenerateTotpSecret()
{
    var bytes = new byte[20];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(bytes);
    return Base32Encode(bytes);
}

static string ProtectMfaSecret(string secret, IDataProtectionProvider provider)
{
    var protector = provider.CreateProtector("GenesisGest.Net.MfaSecret");
    return protector.Protect(secret);
}

static bool TryUnprotectMfaSecret(string? protectedSecret, IDataProtectionProvider provider, out string secret)
{
    secret = string.Empty;
    if (string.IsNullOrWhiteSpace(protectedSecret)) return false;
    try
    {
        var protector = provider.CreateProtector("GenesisGest.Net.MfaSecret");
        secret = protector.Unprotect(protectedSecret);
        return true;
    }
    catch
    {
        return false;
    }
}

static bool TryValidateTotpCode(string secret, string code, DateTimeOffset now, long? lastTimestep, out long matchedTimestep)
{
    matchedTimestep = 0;
    if (string.IsNullOrWhiteSpace(code) || code.Length != 6) return false;

    var currentStep = now.ToUnixTimeSeconds() / 30;
    for (var i = -1; i <= 1; i++)
    {
        var step = currentStep + i;
        if (lastTimestep.HasValue && step <= lastTimestep.Value) continue;

        if (GenerateTotpCode(secret, step) == code)
        {
            matchedTimestep = step;
            return true;
        }
    }
    return false;
}

static string GenerateTotpCode(string secret, long timestep)
{
    var key = Base32Decode(secret);
    var timestepBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(timestep));
    using var hmac = new HMACSHA1(key);
    var hash = hmac.ComputeHash(timestepBytes);
    var offset = hash[^1] & 0x0F;
    var binaryCode = ((hash[offset] & 0x7F) << 24)
                   | ((hash[offset + 1] & 0xFF) << 16)
                   | ((hash[offset + 2] & 0xFF) << 8)
                   | (hash[offset + 3] & 0xFF);
    return (binaryCode % 1000000).ToString("D6");
}

static string Base32Encode(byte[] data)
{
    const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    var output = new StringBuilder();
    int bitBuffer = 0, bitCount = 0;
    foreach (var b in data)
    {
        bitBuffer = (bitBuffer << 8) | b;
        bitCount += 8;
        while (bitCount >= 5)
        {
            bitCount -= 5;
            output.Append(alphabet[(bitBuffer >> bitCount) & 31]);
        }
    }
    if (bitCount > 0)
    {
        output.Append(alphabet[(bitBuffer << (5 - bitCount)) & 31]);
    }
    return output.ToString();
}

static byte[] Base32Decode(string base32)
{
    const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    var cleaned = base32.TrimEnd('=').ToUpperInvariant();
    var bytes = new List<byte>();
    int bitBuffer = 0, bitCount = 0;
    foreach (var c in cleaned)
    {
        var val = alphabet.IndexOf(c);
        if (val < 0) continue;
        bitBuffer = (bitBuffer << 5) | val;
        bitCount += 5;
        if (bitCount >= 8)
        {
            bitCount -= 8;
            bytes.Add((byte)(bitBuffer >> bitCount));
        }
    }
    return bytes.ToArray();
}

static async Task<WorkflowDefinicaoDto?> LoadWorkflowDefinitionAsync(NexumDbContext db, Guid tenantId, Guid id, CancellationToken ct)
{
    return await db.Database.SqlQueryRaw<WorkflowDefinicaoDto>(
        """
        SELECT
            CAST(id AS CHAR) AS Id,
            entidade AS Entidade,
            codigo AS Codigo,
            nome AS Nome,
            estados_json AS EstadosJson,
            transicoes_json AS TransicoesJson,
            ativo AS Ativo,
            created_at AS CriadoEm,
            updated_at AS AtualizadoEm
        FROM sys_workflow_definicoes
        WHERE id = {0} AND tenant_id = {1} AND is_deleted = 0
        LIMIT 1
        """,
        id.ToString(),
        tenantId.ToString())
        .FirstOrDefaultAsync(ct);
}

static bool TryParseWorkflowDefinition(WorkflowDefinicaoDto dto, out WorkflowDefinicaoModelo? modelo, out string? error)
{
    modelo = null;
    error = null;
    try
    {
        var estados = JsonSerializer.Deserialize<List<string>>(dto.EstadosJson) ?? new();
        var transicoes = JsonSerializer.Deserialize<List<WorkflowTransicaoRegraModelo>>(dto.TransicoesJson) ?? new();
        modelo = new WorkflowDefinicaoModelo(dto.Entidade, dto.Codigo, dto.Nome, estados, transicoes, dto.Ativo);
        return true;
    }
    catch (Exception ex)
    {
        error = ex.Message;
        return false;
    }
}

static WorkflowDefinicaoModelo? NormalizeWorkflowDefinition(WorkflowDefinicaoRequest request, out string? error)
{
    error = null;
    var entidade = NormalizeBusinessKey(request.Entidade);
    var codigo = NormalizeBusinessKey(request.Codigo);
    var nome = TrimOrNull(request.Nome);
    if (string.IsNullOrWhiteSpace(entidade) || string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nome))
    {
        error = "Entidade, codigo e nome sao obrigatorios.";
        return null;
    }

    var estados = request.Estados?.Select(NormalizeBusinessKey).Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e!).Distinct().ToList() ?? new();
    if (estados.Count == 0)
    {
        error = "Informe ao menos um estado valido.";
        return null;
    }

    var transicoes = new List<WorkflowTransicaoRegraModelo>();
    if (request.Transicoes != null)
    {
        foreach (var t in request.Transicoes)
        {
            var origem = NormalizeBusinessKey(t.Origem);
            var destino = NormalizeBusinessKey(t.Destino);
            var acao = NormalizeBusinessKey(t.Acao);
            if (string.IsNullOrWhiteSpace(origem) || string.IsNullOrWhiteSpace(destino) || !estados.Contains(origem) || !estados.Contains(destino))
            {
                error = $"Transicao invalida de '{t.Origem}' para '{t.Destino}'.";
                return null;
            }
            transicoes.Add(new WorkflowTransicaoRegraModelo(origem, destino, acao ?? "TRANSICIONAR", t.PerfisAutorizados ?? new()));
        }
    }

    return new WorkflowDefinicaoModelo(entidade, codigo, nome, estados, transicoes, request.Ativo);
}

static async Task<WorkflowInstanciaDto?> LoadWorkflowInstanceAsync(NexumDbContext db, Guid tenantId, Guid id, CancellationToken ct)
{
    return await db.Database.SqlQueryRaw<WorkflowInstanciaDto>(
        """
        SELECT
            CAST(id AS CHAR) AS Id,
            CAST(definicao_id AS CHAR) AS DefinicaoId,
            entidade AS Entidade,
            registro_chave AS RegistroChave,
            estado_atual AS EstadoAtual,
            solicitante_user_id AS SolicitanteUserId,
            created_at AS CriadoEm,
            updated_at AS AtualizadoEm
        FROM sys_workflow_instancias
        WHERE id = {0} AND tenant_id = {1} AND is_deleted = 0
        LIMIT 1
        """,
        id.ToString(),
        tenantId.ToString())
        .FirstOrDefaultAsync(ct);
}

static bool IsWorkflowProfileAuthorized(ClaimsPrincipal principal, List<string> perfisAutorizados)
{
    if (perfisAutorizados == null || perfisAutorizados.Count == 0) return true;
    return perfisAutorizados.Any(p => principal.IsInRole(p));
}
