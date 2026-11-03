/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
<<<<<<< HEAD
 * Versão: 1.1.5.7260 Data: 30/10/2026
=======
 * Versão: 1.1.5.7225 Data: 16/08/2026
>>>>>>> 0b859b83b843d38e6b609297964ac23e3fe66577
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
        return Results.Created($"/api/erp/genesis/pdv/vendas/{venda.Id}", ApiResponse<GenesisPdvVendaDto>.Ok(venda, "Venda PDV registrada."));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ApiResponse<string>.Erro(ex.Message));
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
        return Results.BadRequest(ApiResponse<string>.Erro(ex.Message));
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

<<<<<<< HEAD
app.MapPut("/api/admin/usuarios/{id:int}", [Authorize(Policy = "Admin")] async (
    int id,
    UsuarioAcessoUpsertRequest request,
    NexumDbContext db,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    CancellationToken ct) =>
{
    var usuario = await db.Usuarios.FirstOrDefaultAsync(item => item.Id == id, ct);
    if (usuario is null)
    {
        return Results.NotFound(ApiResponse<UsuarioAcessoDto>.Erro("Usuario nao encontrado."));
    }

    var currentUserId = GetCurrentUserId(principal);
    var currentRole = principal.FindFirstValue("perfil") ?? principal.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    var nome = TrimOrNull(request.Nome);
    var email = NormalizeEmail(request.Email);
    var senha = TrimOrNull(request.Senha);
    var perfilRaw = TrimOrNull(request.Perfil) ?? usuario.Perfil.ToString();

    if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email))
    {
        return Results.BadRequest(ApiResponse<UsuarioAcessoDto>.Erro("Nome e e-mail sao obrigatorios."));
    }

    if (await db.Usuarios.AnyAsync(item => item.Id != id && item.Email == email, ct))
    {
        return Results.Conflict(ApiResponse<UsuarioAcessoDto>.Erro("Outro usuario ja usa este e-mail."));
    }

    if (!Enum.TryParse<PerfilUsuario>(perfilRaw, true, out var perfil))
    {
        return Results.BadRequest(ApiResponse<UsuarioAcessoDto>.Erro("Perfil informado nao existe."));
    }

    if (usuario.Perfil == PerfilUsuario.SuperAdmin && !string.Equals(currentRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
    {
        return Results.Forbid();
    }

    if (perfil == PerfilUsuario.SuperAdmin && !string.Equals(currentRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
    {
        return Results.Forbid();
    }

    if (currentUserId == usuario.Id && !request.Ativo)
    {
        return Results.BadRequest(ApiResponse<UsuarioAcessoDto>.Erro("O usuario logado nao pode desativar o proprio acesso."));
    }

    var anterior = ToUsuarioAcessoDto(usuario);
    usuario.Nome = nome;
    usuario.Email = email;
    usuario.Perfil = perfil;
    usuario.Telefone = TrimOrNull(request.Telefone);
    usuario.Ativo = request.Ativo;
    usuario.UpdatedAt = DateTime.UtcNow;

    if (!string.IsNullOrWhiteSpace(senha))
    {
        if (senha.Length < 8)
        {
            return Results.BadRequest(ApiResponse<UsuarioAcessoDto>.Erro("A nova senha deve ter pelo menos 8 caracteres."));
        }

        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha, 12);
        usuario.TokenRefresh = null;
        usuario.TokenRefreshExpiraEm = null;
    }

    var atualizado = ToUsuarioAcessoDto(usuario);
    return await ExecuteWithDatabaseStrategyAsync(db, async () =>
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        db.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "usuarios", usuario.Id, AcaoAuditoria.UPDATE, anterior, atualizado));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return Results.Ok(ApiResponse<UsuarioAcessoDto>.Ok(atualizado, "Usuario administrativo atualizado."));
    });
})
.WithName("AdminUsuariosAtualizar");

app.MapGet("/api/admin/usuarios/perfis", [Authorize(Policy = "Admin")] (ClaimsPrincipal principal) =>
{
    var currentRole = principal.FindFirstValue("perfil") ?? principal.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    var isSuperAdmin = string.Equals(currentRole, "SuperAdmin", StringComparison.OrdinalIgnoreCase);
    var perfis = Enum.GetNames<PerfilUsuario>()
        .Where(perfil => isSuperAdmin || perfil != PerfilUsuario.SuperAdmin.ToString())
        .ToList();

    return Results.Ok(ApiResponse<List<string>>.Ok(perfis, "Perfis administrativos disponiveis."));
})
.WithName("AdminUsuariosPerfis");

app.MapGet("/api/perfis", [Authorize(Policy = "Admin")] async (NexumDbContext db, CancellationToken ct) =>
{
    var perfis = await db.Database.SqlQueryRaw<PerfilAcessoDto>(
        """
        SELECT
            prf_id AS Id,
            prf_nome AS Nome,
            prf_descricao AS Descricao,
            prf_alcada_maxima AS AlcadaMaxima,
            prf_nivel_hierarquico AS NivelHierarquico,
            prf_ativo AS Ativo,
            prf_data_cadastro AS CriadoEm
        FROM adm_perfis
        ORDER BY prf_nivel_hierarquico, prf_nome
        """)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<PerfilAcessoDto>>.Ok(perfis, "Perfis corporativos carregados."));
})
.WithName("GrcPerfisListar");

app.MapPost("/api/perfis", [Authorize(Policy = "Admin")] async (
    PerfilAcessoUpsertRequest request,
    NexumDbContext db,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    CancellationToken ct) =>
{
    var nome = TrimOrNull(request.Nome);
    if (string.IsNullOrWhiteSpace(nome))
    {
        return Results.BadRequest(ApiResponse<PerfilAcessoDto>.Erro("Nome do perfil e obrigatorio."));
    }

    var conflitos = DetectSoDConflicts(nome, Enumerable.Empty<string>());
    if (conflitos.Count > 0)
    {
        return Results.BadRequest(ApiResponse<PerfilAcessoDto>.Erro("Perfil conflita com regras SoD.", conflitos));
    }

    return await ExecuteWithDatabaseStrategyAsync(db, async () =>
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO adm_perfis (prf_nome, prf_descricao, prf_alcada_maxima, prf_nivel_hierarquico, prf_ativo, prf_data_cadastro)
            VALUES ({nome}, {TrimOrNull(request.Descricao)}, {request.AlcadaMaxima}, {request.NivelHierarquico}, {request.Ativo}, UTC_TIMESTAMP())
            """,
            ct);

        var id = await db.Database.SqlQueryRaw<int>("SELECT LAST_INSERT_ID() AS Value").SingleAsync(ct);
        var perfil = await LoadPerfilAsync(db, id, ct);
        db.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "adm_perfis", id, AcaoAuditoria.INSERT, null, perfil));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Results.Created($"/api/perfis/{id}", ApiResponse<PerfilAcessoDto>.Ok(perfil!, "Perfil corporativo criado."));
    });
})
.WithName("GrcPerfisCriar");

app.MapPut("/api/perfis/{id:int}", [Authorize(Policy = "Admin")] async (
    int id,
    PerfilAcessoUpsertRequest request,
    NexumDbContext db,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    CancellationToken ct) =>
{
    var atual = await LoadPerfilAsync(db, id, ct);
    if (atual is null)
    {
        return Results.NotFound(ApiResponse<PerfilAcessoDto>.Erro("Perfil nao encontrado."));
    }

    var nome = TrimOrNull(request.Nome);
    if (string.IsNullOrWhiteSpace(nome))
    {
        return Results.BadRequest(ApiResponse<PerfilAcessoDto>.Erro("Nome do perfil e obrigatorio."));
    }

    return await ExecuteWithDatabaseStrategyAsync(db, async () =>
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE adm_perfis
            SET prf_nome = {nome},
                prf_descricao = {TrimOrNull(request.Descricao)},
                prf_alcada_maxima = {request.AlcadaMaxima},
                prf_nivel_hierarquico = {request.NivelHierarquico},
                prf_ativo = {request.Ativo}
            WHERE prf_id = {id}
            """,
            ct);

        var atualizado = await LoadPerfilAsync(db, id, ct);
        db.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "adm_perfis", id, AcaoAuditoria.UPDATE, atual, atualizado));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Results.Ok(ApiResponse<PerfilAcessoDto>.Ok(atualizado!, "Perfil corporativo atualizado."));
    });
})
.WithName("GrcPerfisAtualizar");

app.MapDelete("/api/perfis/{id:int}", [Authorize(Policy = "Admin")] async (
    int id,
    NexumDbContext db,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    CancellationToken ct) =>
{
    var atual = await LoadPerfilAsync(db, id, ct);
    if (atual is null)
    {
        return Results.NotFound(ApiResponse<PerfilAcessoDto>.Erro("Perfil nao encontrado."));
    }

    var desativado = atual with { Ativo = false };
    return await ExecuteWithDatabaseStrategyAsync(db, async () =>
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE adm_perfis SET prf_ativo = 0 WHERE prf_id = {id}", ct);
        db.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "adm_perfis", id, AcaoAuditoria.DELETE, atual, desativado));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Results.Ok(ApiResponse<PerfilAcessoDto>.Ok(desativado, "Perfil desativado com soft delete operacional."));
    });
})
.WithName("GrcPerfisExcluir");

app.MapGet("/api/permissoes", [Authorize(Policy = "Admin")] async (NexumDbContext db, CancellationToken ct) =>
{
    var permissoes = await db.Database.SqlQueryRaw<PermissaoAcessoDto>(
        """
        SELECT
            prm_id AS Id,
            prm_modulo AS Modulo,
            prm_funcionalidade AS Funcionalidade,
            prm_chave AS Chave,
            prm_descricao AS Descricao,
            prm_ativo AS Ativo
        FROM adm_permissoes
        ORDER BY prm_modulo, prm_funcionalidade, prm_chave
        """)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<PermissaoAcessoDto>>.Ok(permissoes, "Catalogo de permissoes carregado."));
})
.WithName("GrcPermissoesListar");

app.MapPost("/api/permissoes", [Authorize(Policy = "Admin")] async (
    PermissaoAcessoUpsertRequest request,
    NexumDbContext db,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    CancellationToken ct) =>
{
    var modulo = NormalizeBusinessKey(request.Modulo);
    var funcionalidade = TrimOrNull(request.Funcionalidade);
    var chave = NormalizePermissionKey(request.Chave);
    if (string.IsNullOrWhiteSpace(modulo) || string.IsNullOrWhiteSpace(funcionalidade) || string.IsNullOrWhiteSpace(chave))
    {
        return Results.BadRequest(ApiResponse<PermissaoAcessoDto>.Erro("Modulo, funcionalidade e chave sao obrigatorios."));
    }

    return await ExecuteWithDatabaseStrategyAsync(db, async () =>
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO adm_permissoes (prm_modulo, prm_funcionalidade, prm_chave, prm_descricao, prm_ativo)
            VALUES ({modulo}, {funcionalidade}, {chave}, {TrimOrNull(request.Descricao)}, {request.Ativo})
            """,
            ct);

        var id = await db.Database.SqlQueryRaw<int>("SELECT LAST_INSERT_ID() AS Value").SingleAsync(ct);
        var permissao = await LoadPermissaoAsync(db, id, ct);
        db.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "adm_permissoes", id, AcaoAuditoria.INSERT, null, permissao));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Results.Created($"/api/permissoes/{id}", ApiResponse<PermissaoAcessoDto>.Ok(permissao!, "Permissao criada."));
    });
})
.WithName("GrcPermissoesCriar");

app.MapPut("/api/permissoes/{id:int}", [Authorize(Policy = "Admin")] async (
    int id,
    PermissaoAcessoUpsertRequest request,
    NexumDbContext db,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    CancellationToken ct) =>
{
    var atual = await LoadPermissaoAsync(db, id, ct);
    if (atual is null)
    {
        return Results.NotFound(ApiResponse<PermissaoAcessoDto>.Erro("Permissao nao encontrada."));
    }

    var modulo = NormalizeBusinessKey(request.Modulo);
    var funcionalidade = TrimOrNull(request.Funcionalidade);
    var chave = NormalizePermissionKey(request.Chave);
    if (string.IsNullOrWhiteSpace(modulo) || string.IsNullOrWhiteSpace(funcionalidade) || string.IsNullOrWhiteSpace(chave))
    {
        return Results.BadRequest(ApiResponse<PermissaoAcessoDto>.Erro("Modulo, funcionalidade e chave sao obrigatorios."));
    }

    return await ExecuteWithDatabaseStrategyAsync(db, async () =>
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE adm_permissoes
            SET prm_modulo = {modulo},
                prm_funcionalidade = {funcionalidade},
                prm_chave = {chave},
                prm_descricao = {TrimOrNull(request.Descricao)},
                prm_ativo = {request.Ativo}
            WHERE prm_id = {id}
            """,
            ct);

        var atualizada = await LoadPermissaoAsync(db, id, ct);
        db.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "adm_permissoes", id, AcaoAuditoria.UPDATE, atual, atualizada));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Results.Ok(ApiResponse<PermissaoAcessoDto>.Ok(atualizada!, "Permissao atualizada."));
    });
})
.WithName("GrcPermissoesAtualizar");

app.MapDelete("/api/permissoes/{id:int}", [Authorize(Policy = "Admin")] async (
    int id,
    NexumDbContext db,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    CancellationToken ct) =>
{
    var atual = await LoadPermissaoAsync(db, id, ct);
    if (atual is null)
    {
        return Results.NotFound(ApiResponse<PermissaoAcessoDto>.Erro("Permissao nao encontrada."));
    }

    var desativada = atual with { Ativo = false };
    return await ExecuteWithDatabaseStrategyAsync(db, async () =>
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE adm_permissoes SET prm_ativo = 0 WHERE prm_id = {id}", ct);
        db.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "adm_permissoes", id, AcaoAuditoria.DELETE, atual, desativada));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Results.Ok(ApiResponse<PermissaoAcessoDto>.Ok(desativada, "Permissao desativada."));
    });
})
.WithName("GrcPermissoesExcluir");

app.MapGet("/api/perfis/{id:int}/permissoes", [Authorize(Policy = "Admin")] async (int id, NexumDbContext db, CancellationToken ct) =>
{
    if (await LoadPerfilAsync(db, id, ct) is null)
    {
        return Results.NotFound(ApiResponse<List<PerfilPermissaoDto>>.Erro("Perfil nao encontrado."));
    }

    var permissoes = await LoadPerfilPermissoesAsync(db, id, ct);
    return Results.Ok(ApiResponse<List<PerfilPermissaoDto>>.Ok(permissoes, "Matriz RBAC do perfil carregada."));
})
.WithName("GrcPerfilPermissoesListar");

app.MapPost("/api/perfis/{id:int}/permissoes", [Authorize(Policy = "Admin")] async (
    int id,
    PerfilPermissaoUpsertRequest request,
    NexumDbContext db,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    CancellationToken ct) =>
{
    if (await LoadPerfilAsync(db, id, ct) is null)
    {
        return Results.NotFound(ApiResponse<PerfilPermissaoDto>.Erro("Perfil nao encontrado."));
    }

    if (await LoadPermissaoAsync(db, request.PermissaoId, ct) is null)
    {
        return Results.NotFound(ApiResponse<PerfilPermissaoDto>.Erro("Permissao nao encontrada."));
    }

    var anterior = (await LoadPerfilPermissoesAsync(db, id, ct))
        .FirstOrDefault(item => item.PermissaoId == request.PermissaoId);
    return await ExecuteWithDatabaseStrategyAsync(db, async () =>
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO adm_perfil_permissoes (ppr_perfil_id, ppr_permissao_id, ppr_leitura, ppr_escrita, ppr_exclusao, ppr_impressao)
            VALUES ({id}, {request.PermissaoId}, {request.Leitura}, {request.Escrita}, {request.Exclusao}, {request.Impressao})
            ON DUPLICATE KEY UPDATE
                ppr_leitura = VALUES(ppr_leitura),
                ppr_escrita = VALUES(ppr_escrita),
                ppr_exclusao = VALUES(ppr_exclusao),
                ppr_impressao = VALUES(ppr_impressao)
            """,
            ct);

        var permissao = (await LoadPerfilPermissoesAsync(db, id, ct)).First(item => item.PermissaoId == request.PermissaoId);
        db.LogsAuditoria.Add(CreateIamAuditLog(
            principal,
            httpContext,
            "adm_perfil_permissoes",
            permissao.Id,
            anterior is null ? AcaoAuditoria.INSERT : AcaoAuditoria.UPDATE,
            anterior,
            permissao));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Results.Ok(ApiResponse<PerfilPermissaoDto>.Ok(permissao, "Permissao vinculada ao perfil."));
    });
})
.WithName("GrcPerfilPermissoesVincular");

app.MapDelete("/api/perfis/{id:int}/permissoes/{permissaoId:int}", [Authorize(Policy = "Admin")] async (
    int id,
    int permissaoId,
    NexumDbContext db,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    CancellationToken ct) =>
{
    var anterior = (await LoadPerfilPermissoesAsync(db, id, ct))
        .FirstOrDefault(item => item.PermissaoId == permissaoId);
    if (anterior is null)
    {
        return Results.NotFound(ApiResponse<object>.Erro("Vinculo de permissao nao encontrado."));
    }

    return await ExecuteWithDatabaseStrategyAsync(db, async () =>
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM adm_perfil_permissoes WHERE ppr_perfil_id = {id} AND ppr_permissao_id = {permissaoId}",
            ct);

        db.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "adm_perfil_permissoes", anterior.Id, AcaoAuditoria.DELETE, anterior, null));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Results.Ok(ApiResponse<object>.Ok(new { PerfilId = id, PermissaoId = permissaoId }, "Vinculo de permissao removido do perfil."));
    });
})
.WithName("GrcPerfilPermissoesRemover");

app.MapGet("/api/auditoria", [Authorize(Policy = "Admin")] async (
    string? modulo,
    string? tabela,
    int? usuario,
    DateTime? dataInicio,
    DateTime? dataFim,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var query = db.LogsAuditoria.AsNoTracking().AsQueryable();
    if (!string.IsNullOrWhiteSpace(modulo))
    {
        query = query.Where(log => log.Endpoint != null && log.Endpoint.Contains(modulo));
    }

    if (!string.IsNullOrWhiteSpace(tabela))
    {
        query = query.Where(log => log.Tabela == tabela);
    }

    if (usuario.HasValue)
    {
        query = query.Where(log => log.UsuarioId == usuario.Value);
    }

    if (dataInicio.HasValue)
    {
        query = query.Where(log => log.CreatedAt >= dataInicio.Value);
    }

    if (dataFim.HasValue)
    {
        query = query.Where(log => log.CreatedAt <= dataFim.Value);
    }

    var auditoria = await query
        .OrderByDescending(log => log.CreatedAt)
        .Take(500)
        .Select(log => new AuditoriaOperacionalDto(
            log.Id,
            log.Tabela,
            log.RegistroId,
            log.Acao.ToString(),
            log.UsuarioId,
            log.UsuarioTipo.ToString(),
            log.IpAddress,
            log.Endpoint,
            log.CreatedAt))
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<AuditoriaOperacionalDto>>.Ok(auditoria, "Trilha de auditoria carregada.", auditoria.Count));
})
.WithName("GrcAuditoriaListar");

app.MapGet("/api/auditoria/{id:long}", [Authorize(Policy = "Admin")] async (long id, NexumDbContext db, CancellationToken ct) =>
{
    var log = await db.LogsAuditoria
        .AsNoTracking()
        .Where(item => item.Id == id)
        .Select(item => new AuditoriaDetalheDto(
            item.Id,
            item.Tabela,
            item.RegistroId,
            item.Acao.ToString(),
            item.UsuarioId,
            item.UsuarioTipo.ToString(),
            item.IpAddress,
            item.UserAgent,
            item.Endpoint,
            item.DadosAnteriores,
            item.DadosNovos,
            item.CreatedAt))
        .FirstOrDefaultAsync(ct);

    return log is null
        ? Results.NotFound(ApiResponse<AuditoriaDetalheDto>.Erro("Registro de auditoria nao encontrado."))
        : Results.Ok(ApiResponse<AuditoriaDetalheDto>.Ok(log, "Registro de auditoria carregado."));
})
.WithName("GrcAuditoriaDetalhar");

app.MapGet("/api/sod/regras", [Authorize(Policy = "Admin")] () =>
{
    var regras = BuildSoDRules();
    return Results.Ok(ApiResponse<List<SoDRegraDto>>.Ok(regras, "Regras de segregacao de funcoes carregadas."));
})
.WithName("GrcSoDRegras");

app.MapPost("/api/sod/validar", [Authorize(Policy = "Admin")] (SoDValidacaoRequest request) =>
{
    var conflitos = DetectSoDConflicts(request.Perfil, request.Permissoes);
    var result = new SoDValidacaoResponse(conflitos.Count == 0, conflitos);
    return Results.Ok(ApiResponse<SoDValidacaoResponse>.Ok(result, conflitos.Count == 0 ? "Sem conflito SoD." : "Conflitos SoD identificados."));
})
.WithName("GrcSoDValidar");

app.MapGet("/api/pessoas", [Authorize(Policy = "Gerente")] async (
    string? termo,
    string? tipo,
    bool? cliente,
    bool? fornecedor,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var pessoas = await db.Database.SqlQueryRaw<PessoaMasterDataDto>(
        """
        SELECT
            pes_id AS Id,
            pes_tipo AS Tipo,
            pes_nome_razao AS NomeRazao,
            pes_nome_fantasia AS NomeFantasia,
            pes_cpf_cnpj AS CpfCnpj,
            pes_rg_ie AS RgIe,
            pes_cliente AS Cliente,
            pes_fornecedor AS Fornecedor,
            pes_colaborador AS Colaborador,
            pes_transportadora AS Transportadora,
            pes_email AS Email,
            pes_telefone AS Telefone,
            pes_celular AS Celular,
            pes_cidade AS Cidade,
            pes_uf AS Uf,
            pes_ativo AS Ativo,
            pes_data_cadastro AS CriadoEm,
            pes_data_atualizacao AS AtualizadoEm
        FROM adm_pessoas_empresas
        WHERE pes_ativo = 1
          AND ({0} IS NULL OR pes_nome_razao LIKE CONCAT('%', {0}, '%') OR pes_nome_fantasia LIKE CONCAT('%', {0}, '%') OR pes_cpf_cnpj LIKE CONCAT('%', {0}, '%'))
          AND ({1} IS NULL OR pes_tipo = {1})
          AND ({2} IS NULL OR pes_cliente = {2})
          AND ({3} IS NULL OR pes_fornecedor = {3})
        ORDER BY pes_nome_razao
        LIMIT 500
        """,
        (object?)TrimOrNull(termo) ?? DBNull.Value,
        (object?)NormalizePessoaTipo(tipo) ?? DBNull.Value,
        (object?)cliente ?? DBNull.Value,
        (object?)fornecedor ?? DBNull.Value)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<PessoaMasterDataDto>>.Ok(pessoas, "Pessoas master data carregadas.", pessoas.Count));
})
.WithName("MasterDataPessoasListar");

app.MapGet("/api/pessoas/{id:int}", [Authorize(Policy = "Gerente")] async (int id, NexumDbContext db, CancellationToken ct) =>
{
    var pessoa = await LoadPessoaAsync(db, id, ct);
    return pessoa is null
        ? Results.NotFound(ApiResponse<PessoaMasterDataDto>.Erro("Pessoa nao encontrada."))
        : Results.Ok(ApiResponse<PessoaMasterDataDto>.Ok(pessoa, "Pessoa carregada."));
})
.WithName("MasterDataPessoasDetalhar");

app.MapPost("/api/pessoas", [Authorize(Policy = "Gerente")] async (
    PessoaMasterDataRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var nome = TrimOrNull(request.NomeRazao);
    var tipo = NormalizePessoaTipo(request.Tipo);
    if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(tipo))
    {
        return Results.BadRequest(ApiResponse<PessoaMasterDataDto>.Erro("Tipo e nome/razao social sao obrigatorios."));
    }

    await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        INSERT INTO adm_pessoas_empresas
            (pes_tipo, pes_nome_razao, pes_nome_fantasia, pes_cpf_cnpj, pes_rg_ie, pes_cliente, pes_fornecedor, pes_colaborador,
             pes_transportadora, pes_endereco, pes_numero, pes_complemento, pes_bairro, pes_cidade, pes_uf, pes_cep, pes_telefone,
             pes_celular, pes_email, pes_site, pes_observacoes, pes_ativo, pes_data_cadastro, pes_data_atualizacao)
        VALUES
            ({tipo}, {nome}, {TrimOrNull(request.NomeFantasia)}, {OnlyDigitsOrNull(request.CpfCnpj)}, {TrimOrNull(request.RgIe)},
             {request.Cliente}, {request.Fornecedor}, {request.Colaborador}, {request.Transportadora}, {TrimOrNull(request.Endereco)},
             {TrimOrNull(request.Numero)}, {TrimOrNull(request.Complemento)}, {TrimOrNull(request.Bairro)}, {TrimOrNull(request.Cidade)},
             {NormalizeUf(request.Uf)}, {OnlyDigitsOrNull(request.Cep)}, {TrimOrNull(request.Telefone)}, {TrimOrNull(request.Celular)},
             {NormalizeEmail(request.Email)}, {TrimOrNull(request.Site)}, {TrimOrNull(request.Observacoes)}, {request.Ativo},
             UTC_TIMESTAMP(), UTC_TIMESTAMP())
        """,
        ct);

    var id = await db.Database.SqlQueryRaw<int>("SELECT LAST_INSERT_ID() AS Value").SingleAsync(ct);
    return Results.Created($"/api/pessoas/{id}", ApiResponse<PessoaMasterDataDto>.Ok((await LoadPessoaAsync(db, id, ct))!, "Pessoa master data criada."));
})
.WithName("MasterDataPessoasCriar");

app.MapPut("/api/pessoas/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    PessoaMasterDataRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (await LoadPessoaAsync(db, id, ct) is null)
    {
        return Results.NotFound(ApiResponse<PessoaMasterDataDto>.Erro("Pessoa nao encontrada."));
    }

    var nome = TrimOrNull(request.NomeRazao);
    var tipo = NormalizePessoaTipo(request.Tipo);
    if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(tipo))
    {
        return Results.BadRequest(ApiResponse<PessoaMasterDataDto>.Erro("Tipo e nome/razao social sao obrigatorios."));
    }

    await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        UPDATE adm_pessoas_empresas
        SET pes_tipo = {tipo},
            pes_nome_razao = {nome},
            pes_nome_fantasia = {TrimOrNull(request.NomeFantasia)},
            pes_cpf_cnpj = {OnlyDigitsOrNull(request.CpfCnpj)},
            pes_rg_ie = {TrimOrNull(request.RgIe)},
            pes_cliente = {request.Cliente},
            pes_fornecedor = {request.Fornecedor},
            pes_colaborador = {request.Colaborador},
            pes_transportadora = {request.Transportadora},
            pes_endereco = {TrimOrNull(request.Endereco)},
            pes_numero = {TrimOrNull(request.Numero)},
            pes_complemento = {TrimOrNull(request.Complemento)},
            pes_bairro = {TrimOrNull(request.Bairro)},
            pes_cidade = {TrimOrNull(request.Cidade)},
            pes_uf = {NormalizeUf(request.Uf)},
            pes_cep = {OnlyDigitsOrNull(request.Cep)},
            pes_telefone = {TrimOrNull(request.Telefone)},
            pes_celular = {TrimOrNull(request.Celular)},
            pes_email = {NormalizeEmail(request.Email)},
            pes_site = {TrimOrNull(request.Site)},
            pes_observacoes = {TrimOrNull(request.Observacoes)},
            pes_ativo = {request.Ativo},
            pes_data_atualizacao = UTC_TIMESTAMP()
        WHERE pes_id = {id}
        """,
        ct);

    return Results.Ok(ApiResponse<PessoaMasterDataDto>.Ok((await LoadPessoaAsync(db, id, ct))!, "Pessoa master data atualizada."));
})
.WithName("MasterDataPessoasAtualizar");

app.MapDelete("/api/pessoas/{id:int}", [Authorize(Policy = "Gerente")] async (int id, NexumDbContext db, CancellationToken ct) =>
{
    var pessoa = await LoadPessoaAsync(db, id, ct);
    if (pessoa is null)
    {
        return Results.NotFound(ApiResponse<PessoaMasterDataDto>.Erro("Pessoa nao encontrada."));
    }

    await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE adm_pessoas_empresas SET pes_ativo = 0, pes_data_atualizacao = UTC_TIMESTAMP() WHERE pes_id = {id}", ct);
    return Results.Ok(ApiResponse<PessoaMasterDataDto>.Ok(pessoa with { Ativo = false }, "Pessoa desativada com soft delete operacional."));
})
.WithName("MasterDataPessoasExcluir");

app.MapGet("/api/centros-custo", [Authorize(Policy = "Gerente")] async (NexumDbContext db, CancellationToken ct) =>
{
    var centros = await db.Database.SqlQueryRaw<CentroCustoDto>(
        """
        SELECT
            ccu_id AS Id,
            ccu_codigo AS Codigo,
            ccu_nome AS Nome,
            ccu_descricao AS Descricao,
            ccu_tipo AS Tipo,
            ccu_pai_id AS PaiId,
            ccu_responsavel_usr_id AS ResponsavelUsuarioId,
            ccu_status AS Status,
            ccu_data_cadastro AS CriadoEm,
            ccu_data_alteracao AS AtualizadoEm
        FROM fin_centros_custo
        WHERE ccu_data_exclusao IS NULL
        ORDER BY ccu_codigo, ccu_nome
        """)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<CentroCustoDto>>.Ok(centros, "Centros de custo carregados.", centros.Count));
})
.WithName("MasterDataCentrosCustoListar");

app.MapPost("/api/centros-custo", [Authorize(Policy = "Gerente")] async (
    CentroCustoRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var codigo = NormalizeBusinessCode(request.Codigo);
    var nome = TrimOrNull(request.Nome);
    var tipo = NormalizeCentroCustoTipo(request.Tipo);
    if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nome))
    {
        return Results.BadRequest(ApiResponse<CentroCustoDto>.Erro("Codigo e nome do centro de custo sao obrigatorios."));
    }

    await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        INSERT INTO fin_centros_custo
            (ccu_codigo, ccu_nome, ccu_descricao, ccu_observacoes, ccu_tipo, ccu_pai_id, ccu_responsavel_usr_id, ccu_status,
             ccu_data_cadastro, ccu_data_alteracao, ccu_data_inclusao)
        VALUES
            ({codigo}, {nome}, {TrimOrNull(request.Descricao)}, {TrimOrNull(request.Observacoes)}, {tipo}, {request.PaiId},
             {request.ResponsavelUsuarioId}, {NormalizeStatusChar(request.Status)}, UTC_TIMESTAMP(), UTC_TIMESTAMP(), UTC_TIMESTAMP())
        """,
        ct);

    var id = await db.Database.SqlQueryRaw<int>("SELECT LAST_INSERT_ID() AS Value").SingleAsync(ct);
    return Results.Created($"/api/centros-custo/{id}", ApiResponse<CentroCustoDto>.Ok((await LoadCentroCustoAsync(db, id, ct))!, "Centro de custo criado."));
})
.WithName("MasterDataCentrosCustoCriar");

app.MapPut("/api/centros-custo/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    CentroCustoRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (await LoadCentroCustoAsync(db, id, ct) is null)
    {
        return Results.NotFound(ApiResponse<CentroCustoDto>.Erro("Centro de custo nao encontrado."));
    }

    await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        UPDATE fin_centros_custo
        SET ccu_codigo = {NormalizeBusinessCode(request.Codigo)},
            ccu_nome = {TrimOrNull(request.Nome)},
            ccu_descricao = {TrimOrNull(request.Descricao)},
            ccu_observacoes = {TrimOrNull(request.Observacoes)},
            ccu_tipo = {NormalizeCentroCustoTipo(request.Tipo)},
            ccu_pai_id = {request.PaiId},
            ccu_responsavel_usr_id = {request.ResponsavelUsuarioId},
            ccu_status = {NormalizeStatusChar(request.Status)},
            ccu_data_alteracao = UTC_TIMESTAMP()
        WHERE ccu_id = {id}
        """,
        ct);

    return Results.Ok(ApiResponse<CentroCustoDto>.Ok((await LoadCentroCustoAsync(db, id, ct))!, "Centro de custo atualizado."));
})
.WithName("MasterDataCentrosCustoAtualizar");

app.MapDelete("/api/centros-custo/{id:int}", [Authorize(Policy = "Gerente")] async (int id, NexumDbContext db, CancellationToken ct) =>
{
    var centro = await LoadCentroCustoAsync(db, id, ct);
    if (centro is null)
    {
        return Results.NotFound(ApiResponse<CentroCustoDto>.Erro("Centro de custo nao encontrado."));
    }

    await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE fin_centros_custo SET ccu_status = 'I', ccu_data_exclusao = UTC_TIMESTAMP() WHERE ccu_id = {id}", ct);
    return Results.Ok(ApiResponse<CentroCustoDto>.Ok(centro with { Status = "I" }, "Centro de custo desativado."));
})
.WithName("MasterDataCentrosCustoExcluir");

app.MapGet("/api/itens-servico", [Authorize(Policy = "Gerente")] async (
    string? termo,
    string? tipo,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var itens = await db.Database.SqlQueryRaw<ItemServicoDto>(
        """
        SELECT
            itm_id AS Id,
            itm_emp_id AS EmpresaId,
            itm_codigo AS Codigo,
            itm_tipo AS Tipo,
            itm_descricao AS Descricao,
            itm_descricao_detalhada AS DescricaoDetalhada,
            itm_unidade AS Unidade,
            itm_ncm AS Ncm,
            itm_cest AS Cest,
            itm_controla_estoque AS ControlaEstoque,
            itm_controla_lote AS ControlaLote,
            itm_controla_serie AS ControlaSerie,
            itm_ativo AS Ativo,
            itm_data_cadastro AS CriadoEm
        FROM vnd_itens
        WHERE itm_ativo = 1
          AND ({0} IS NULL OR itm_codigo LIKE CONCAT('%', {0}, '%') OR itm_descricao LIKE CONCAT('%', {0}, '%'))
          AND ({1} IS NULL OR itm_tipo = {1})
        ORDER BY itm_descricao
        LIMIT 500
        """,
        (object?)TrimOrNull(termo) ?? DBNull.Value,
        (object?)NormalizeItemTipo(tipo) ?? DBNull.Value)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<ItemServicoDto>>.Ok(itens, "Itens e servicos carregados.", itens.Count));
})
.WithName("MasterDataItensServicoListar");

app.MapGet("/api/erp/estoque-fisico", [Authorize(Policy = "Gerente")] async (
    string? termo,
    NexumDbContext nexumDb,
    GenesisDbContext genesisDb,
    CancellationToken ct) =>
{
    var pesquisa = TrimOrNull(termo);
    if (pesquisa?.Length > 100)
    {
        return Results.BadRequest(ApiResponse<List<GenesisSaldoEstoqueDto>>.Erro(
            "A pesquisa aceita no máximo 100 caracteres."));
    }

    var tenantId = nexumDb.CurrentTenantId.ToString();
    var tenant = await nexumDb.Database.SqlQueryRaw<TenantDocumentoLookup>(
        "SELECT documento AS Documento FROM sys_tenants WHERE id = {0} AND tenant_id = {0} AND ativo = 1 AND is_deleted = 0 LIMIT 1",
        tenantId)
        .FirstOrDefaultAsync(ct);
    var cnpj = OnlyDigits(tenant?.Documento) ?? string.Empty;
    if (cnpj.Length != 14 || !IsValidCnpj(cnpj))
    {
        return Results.Conflict(ApiResponse<List<GenesisSaldoEstoqueDto>>.Erro(
            "Tenant sem CNPJ empresarial válido. Cadastre o CNPJ em Administração > Tenants para vincular o estoque Genesis."));
    }

    var empresas = await genesisDb.Database.SqlQueryRaw<GenesisEmpresaLookup>(
        "SELECT emp_id AS Id FROM adm_empresas WHERE emp_ativo = 1 AND REPLACE(REPLACE(REPLACE(REPLACE(emp_cnpj, '.', ''), '/', ''), '-', ''), ' ', '') = {0} ORDER BY emp_id LIMIT 2",
        cnpj)
        .ToListAsync(ct);
    if (empresas.Count == 0)
    {
        return Results.Conflict(ApiResponse<List<GenesisSaldoEstoqueDto>>.Erro(
            "O CNPJ do tenant não corresponde a uma empresa ativa no Genesis."));
    }
    if (empresas.Count > 1)
    {
        return Results.Conflict(ApiResponse<List<GenesisSaldoEstoqueDto>>.Erro(
            "Mais de uma empresa ativa no Genesis usa o CNPJ do tenant. Corrija a duplicidade antes de consultar saldos."));
    }

    var saldos = await genesisDb.Database.SqlQueryRaw<GenesisSaldoEstoqueDto>(
        """
        SELECT
            i.itm_codigo AS Sku,
            i.itm_descricao AS Produto,
            SUM(s.sld_quantidade) AS QuantidadeFisica,
            SUM(COALESCE(s.sld_quantidade_reservada, 0)) AS QuantidadeReservada,
            SUM(s.sld_quantidade_disponivel) AS QuantidadeDisponivel
        FROM vnd_itens AS i
        INNER JOIN est_saldos AS s ON s.sld_itm_id = i.itm_id
        WHERE i.itm_emp_id = {0}
          AND i.itm_ativo = 1
          AND i.itm_controla_estoque = 1
          AND ({1} IS NULL OR i.itm_codigo LIKE CONCAT('%', {1}, '%') OR i.itm_descricao LIKE CONCAT('%', {1}, '%'))
        GROUP BY i.itm_codigo, i.itm_descricao
        ORDER BY i.itm_descricao, i.itm_codigo
        LIMIT 500
        """,
        empresas[0].Id,
        (object?)pesquisa ?? DBNull.Value)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<GenesisSaldoEstoqueDto>>.Ok(
        saldos,
        "Saldo físico Genesis carregado para a empresa vinculada ao tenant.",
        saldos.Count));
})
.WithName("GenesisEstoqueFisicoListar");

app.MapGet("/api/erp/estoque-movimentacoes", [Authorize(Policy = "Gerente")] async (
    string? termo,
    NexumDbContext nexumDb,
    GenesisDbContext genesisDb,
    CancellationToken ct) =>
{
    var pesquisa = TrimOrNull(termo);
    if (pesquisa?.Length > 100)
    {
        return Results.BadRequest(ApiResponse<List<GenesisMovimentacaoEstoqueDto>>.Erro(
            "A pesquisa aceita no máximo 100 caracteres."));
    }

    var tenantId = nexumDb.CurrentTenantId.ToString();
    var tenant = await nexumDb.Database.SqlQueryRaw<TenantDocumentoLookup>(
        "SELECT documento AS Documento FROM sys_tenants WHERE id = {0} AND tenant_id = {0} AND ativo = 1 AND is_deleted = 0 LIMIT 1",
        tenantId)
        .FirstOrDefaultAsync(ct);
    var cnpj = OnlyDigits(tenant?.Documento) ?? string.Empty;
    if (cnpj.Length != 14 || !IsValidCnpj(cnpj))
    {
        return Results.Conflict(ApiResponse<List<GenesisMovimentacaoEstoqueDto>>.Erro(
            "Tenant sem CNPJ empresarial válido. Cadastre o CNPJ em Administração > Tenants para vincular as movimentações Genesis."));
    }

    var empresas = await genesisDb.Database.SqlQueryRaw<GenesisEmpresaLookup>(
        "SELECT emp_id AS Id FROM adm_empresas WHERE emp_ativo = 1 AND REPLACE(REPLACE(REPLACE(REPLACE(emp_cnpj, '.', ''), '/', ''), '-', ''), ' ', '') = {0} ORDER BY emp_id LIMIT 2",
        cnpj)
        .ToListAsync(ct);
    if (empresas.Count == 0)
    {
        return Results.Conflict(ApiResponse<List<GenesisMovimentacaoEstoqueDto>>.Erro(
            "O CNPJ do tenant não corresponde a uma empresa ativa no Genesis."));
    }
    if (empresas.Count > 1)
    {
        return Results.Conflict(ApiResponse<List<GenesisMovimentacaoEstoqueDto>>.Erro(
            "Mais de uma empresa ativa no Genesis usa o CNPJ do tenant. Corrija a duplicidade antes de consultar movimentações."));
    }

    var movimentos = await genesisDb.Database.SqlQueryRaw<GenesisMovimentacaoEstoqueDto>(
        """
        SELECT
            m.mov_id AS Id,
            i.itm_codigo AS Sku,
            i.itm_descricao AS Produto,
            m.mov_tipo AS Tipo,
            m.mov_data AS DataMovimentacao,
            m.mov_quantidade AS Quantidade,
            m.mov_custo_unitario AS CustoUnitario,
            m.mov_custo_total AS CustoTotal,
            m.mov_dep_origem_id AS DepositoOrigemId,
            m.mov_dep_destino_id AS DepositoDestinoId,
            m.mov_lote AS Lote,
            m.mov_serie AS Serie,
            m.mov_documento_tipo AS DocumentoTipo,
            m.mov_documento_id AS DocumentoId,
            m.mov_observacoes AS Observacoes
        FROM est_movimentacoes AS m
        INNER JOIN vnd_itens AS i ON i.itm_id = m.mov_itm_id
        WHERE m.mov_emp_id = {0}
          AND i.itm_emp_id = {0}
          AND ({1} IS NULL OR i.itm_codigo LIKE CONCAT('%', {1}, '%')
               OR i.itm_descricao LIKE CONCAT('%', {1}, '%')
               OR m.mov_documento_tipo LIKE CONCAT('%', {1}, '%')
               OR CAST(m.mov_documento_id AS CHAR) LIKE CONCAT('%', {1}, '%')
               OR m.mov_lote LIKE CONCAT('%', {1}, '%')
               OR m.mov_serie LIKE CONCAT('%', {1}, '%'))
        ORDER BY m.mov_data DESC, m.mov_id DESC
        LIMIT 500
        """,
        empresas[0].Id,
        (object?)pesquisa ?? DBNull.Value)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<GenesisMovimentacaoEstoqueDto>>.Ok(
        movimentos,
        movimentos.Count == 500
            ? "Exibidas até 500 movimentações mais recentes. Refine a pesquisa para reduzir o resultado."
            : "Movimentações do estoque Genesis carregadas para a empresa vinculada ao tenant.",
        movimentos.Count));
})
.WithName("GenesisEstoqueMovimentacoesListar");

app.MapPost("/api/itens-servico", [Authorize(Policy = "Gerente")] async (
    ItemServicoRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var codigo = NormalizeBusinessCode(request.Codigo);
    var descricao = TrimOrNull(request.Descricao);
    var tipo = NormalizeItemTipo(request.Tipo);
    if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(descricao) || string.IsNullOrWhiteSpace(tipo))
    {
        return Results.BadRequest(ApiResponse<ItemServicoDto>.Erro("Codigo, tipo e descricao do item/servico sao obrigatorios."));
    }

    await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        INSERT INTO vnd_itens
            (itm_emp_id, itm_codigo, itm_tipo, itm_descricao, itm_descricao_detalhada, itm_unidade, itm_ncm, itm_cest,
             itm_peso_bruto, itm_peso_liquido, itm_altura, itm_largura, itm_profundidade, itm_controla_estoque,
             itm_controla_lote, itm_controla_serie, itm_ativo, itm_data_cadastro)
        VALUES
            ({request.EmpresaId}, {codigo}, {tipo}, {descricao}, {TrimOrNull(request.DescricaoDetalhada)}, {TrimOrNull(request.Unidade) ?? "UN"},
             {TrimOrNull(request.Ncm)}, {TrimOrNull(request.Cest)}, {request.PesoBruto}, {request.PesoLiquido}, {request.Altura},
             {request.Largura}, {request.Profundidade}, {request.ControlaEstoque}, {request.ControlaLote}, {request.ControlaSerie},
             {request.Ativo}, UTC_TIMESTAMP())
        """,
        ct);

    var id = await db.Database.SqlQueryRaw<int>("SELECT LAST_INSERT_ID() AS Value").SingleAsync(ct);
    return Results.Created($"/api/itens-servico/{id}", ApiResponse<ItemServicoDto>.Ok((await LoadItemServicoAsync(db, id, ct))!, "Item/servico criado."));
})
.WithName("MasterDataItensServicoCriar");

app.MapPut("/api/itens-servico/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    ItemServicoRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (await LoadItemServicoAsync(db, id, ct) is null)
    {
        return Results.NotFound(ApiResponse<ItemServicoDto>.Erro("Item/servico nao encontrado."));
    }

    await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        UPDATE vnd_itens
        SET itm_emp_id = {request.EmpresaId},
            itm_codigo = {NormalizeBusinessCode(request.Codigo)},
            itm_tipo = {NormalizeItemTipo(request.Tipo)},
            itm_descricao = {TrimOrNull(request.Descricao)},
            itm_descricao_detalhada = {TrimOrNull(request.DescricaoDetalhada)},
            itm_unidade = {TrimOrNull(request.Unidade) ?? "UN"},
            itm_ncm = {TrimOrNull(request.Ncm)},
            itm_cest = {TrimOrNull(request.Cest)},
            itm_peso_bruto = {request.PesoBruto},
            itm_peso_liquido = {request.PesoLiquido},
            itm_altura = {request.Altura},
            itm_largura = {request.Largura},
            itm_profundidade = {request.Profundidade},
            itm_controla_estoque = {request.ControlaEstoque},
            itm_controla_lote = {request.ControlaLote},
            itm_controla_serie = {request.ControlaSerie},
            itm_ativo = {request.Ativo}
        WHERE itm_id = {id}
        """,
        ct);

    return Results.Ok(ApiResponse<ItemServicoDto>.Ok((await LoadItemServicoAsync(db, id, ct))!, "Item/servico atualizado."));
})
.WithName("MasterDataItensServicoAtualizar");

app.MapDelete("/api/itens-servico/{id:int}", [Authorize(Policy = "Gerente")] async (int id, NexumDbContext db, CancellationToken ct) =>
{
    var item = await LoadItemServicoAsync(db, id, ct);
    if (item is null)
    {
        return Results.NotFound(ApiResponse<ItemServicoDto>.Erro("Item/servico nao encontrado."));
    }

    await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE vnd_itens SET itm_ativo = 0 WHERE itm_id = {id}", ct);
    return Results.Ok(ApiResponse<ItemServicoDto>.Ok(item with { Ativo = false }, "Item/servico desativado."));
})
.WithName("MasterDataItensServicoExcluir");

app.MapGet("/api/produtos/{id:int}/precos-por-loja", [Authorize(Policy = "Gerente")] async (int id, NexumDbContext db, CancellationToken ct) =>
{
    if (await LoadItemServicoAsync(db, id, ct) is null)
    {
        return Results.NotFound(ApiResponse<List<ProdutoPrecoLojaDto>>.Erro("Produto/item nao encontrado."));
    }

    var precos = await db.Database.SqlQueryRaw<ProdutoPrecoLojaDto>(
        """
        SELECT
            ppl_id AS Id,
            ppl_item_id AS ProdutoId,
            ppl_loja_id AS LojaId,
            ppl_preco_venda AS PrecoVenda,
            ppl_preco_promocional AS PrecoPromocional,
            ppl_preco_custo AS PrecoCusto,
            ppl_margem_percentual AS MargemPercentual,
            ppl_ativo AS Ativo,
            ppl_atualizado_em AS AtualizadoEm
        FROM md_produtos_precos_loja
        WHERE ppl_item_id = {0}
        ORDER BY ppl_loja_id
        """,
        id)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<ProdutoPrecoLojaDto>>.Ok(precos, "Precos por loja carregados.", precos.Count));
})
.WithName("MasterDataProdutosPrecosPorLojaListar");

app.MapPut("/api/produtos/{id:int}/precos-por-loja", [Authorize(Policy = "Gerente")] async (
    int id,
    List<ProdutoPrecoLojaRequest> request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (await LoadItemServicoAsync(db, id, ct) is null)
    {
        return Results.NotFound(ApiResponse<List<ProdutoPrecoLojaDto>>.Erro("Produto/item nao encontrado."));
    }

    foreach (var preco in request)
    {
        if (preco.LojaId <= 0 || preco.PrecoVenda < 0)
        {
            return Results.BadRequest(ApiResponse<List<ProdutoPrecoLojaDto>>.Erro("Loja e preco de venda valido sao obrigatorios."));
        }

        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO md_produtos_precos_loja
                (ppl_item_id, ppl_loja_id, ppl_preco_venda, ppl_preco_promocional, ppl_preco_custo, ppl_margem_percentual, ppl_ativo, ppl_atualizado_em)
            VALUES
                ({id}, {preco.LojaId}, {preco.PrecoVenda}, {preco.PrecoPromocional}, {preco.PrecoCusto}, {preco.MargemPercentual}, {preco.Ativo}, UTC_TIMESTAMP())
            ON DUPLICATE KEY UPDATE
                ppl_preco_venda = VALUES(ppl_preco_venda),
                ppl_preco_promocional = VALUES(ppl_preco_promocional),
                ppl_preco_custo = VALUES(ppl_preco_custo),
                ppl_margem_percentual = VALUES(ppl_margem_percentual),
                ppl_ativo = VALUES(ppl_ativo),
                ppl_atualizado_em = UTC_TIMESTAMP()
            """,
            ct);
    }

    var precos = await db.Database.SqlQueryRaw<ProdutoPrecoLojaDto>(
        """
        SELECT ppl_id AS Id, ppl_item_id AS ProdutoId, ppl_loja_id AS LojaId, ppl_preco_venda AS PrecoVenda,
               ppl_preco_promocional AS PrecoPromocional, ppl_preco_custo AS PrecoCusto,
               ppl_margem_percentual AS MargemPercentual, ppl_ativo AS Ativo, ppl_atualizado_em AS AtualizadoEm
        FROM md_produtos_precos_loja
        WHERE ppl_item_id = {0}
        ORDER BY ppl_loja_id
        """,
        id)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<ProdutoPrecoLojaDto>>.Ok(precos, "Precos por loja atualizados.", precos.Count));
})
.WithName("MasterDataProdutosPrecosPorLojaAtualizar");

app.MapGet("/api/fornecedores/{id:int}/contatos", [Authorize(Policy = "Gerente")] async (int id, NexumDbContext db, CancellationToken ct) =>
{
    if (!await db.Fornecedores.AsNoTracking().AnyAsync(item => item.Id == id, ct))
    {
        return Results.NotFound(ApiResponse<List<FornecedorContatoDto>>.Erro("Fornecedor nao encontrado."));
    }

    var tenantId = db.CurrentTenantId.ToString();
    var contatos = await db.Database.SqlQueryRaw<FornecedorContatoDto>(
        """
        SELECT
            fco_id AS Id,
            fco_fornecedor_id AS FornecedorId,
            fco_nome AS Nome,
            fco_cargo AS Cargo,
            fco_email AS Email,
            fco_telefone AS Telefone,
            fco_celular AS Celular,
            fco_principal AS Principal,
            fco_ativo AS Ativo,
            fco_atualizado_em AS AtualizadoEm,
            TO_BASE64(fco_row_version) AS RowVersion
        FROM md_fornecedor_contatos
        WHERE fco_fornecedor_id = {0}
          AND fco_tenant_id = {1}
          AND fco_is_deleted = 0
          AND fco_ativo = 1
        ORDER BY fco_principal DESC, fco_nome
        """,
        id,
        tenantId)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<FornecedorContatoDto>>.Ok(contatos, "Contatos do fornecedor carregados.", contatos.Count));
})
.WithName("MasterDataFornecedoresContatosListar");

app.MapPost("/api/fornecedores/{id:int}/contatos", [Authorize(Policy = "Gerente")] async (
    int id,
    FornecedorContatoRequest request,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (!await db.Fornecedores.AsNoTracking().AnyAsync(item => item.Id == id, ct))
    {
        return Results.NotFound(ApiResponse<FornecedorContatoDto>.Erro("Fornecedor nao encontrado."));
    }

    var nome = TrimOrNull(request.Nome);
    if (string.IsNullOrWhiteSpace(nome))
    {
        return Results.BadRequest(ApiResponse<FornecedorContatoDto>.Erro("Nome do contato e obrigatorio."));
    }

    var executionStrategy = db.Database.CreateExecutionStrategy();
    var contatoId = 0;
    await executionStrategy.ExecuteAsync(async () =>
    {
        db.ChangeTracker.Clear();
        var tenantId = db.CurrentTenantId.ToString();
        var userId = db.CurrentUserId?.ToString();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (request.Principal)
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE md_fornecedor_contatos
                SET fco_principal = 0,
                    fco_row_version = UNHEX(REPLACE(UUID(), '-', '')),
                    fco_updated_by_user_id = {userId},
                    fco_atualizado_em = UTC_TIMESTAMP()
                WHERE fco_fornecedor_id = {id}
                  AND fco_tenant_id = {tenantId}
                  AND fco_is_deleted = 0
                """,
                ct);
        }

        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO md_fornecedor_contatos
                (fco_fornecedor_id, fco_nome, fco_cargo, fco_email, fco_telefone, fco_celular, fco_principal, fco_ativo,
                 fco_tenant_id, fco_row_version, fco_created_by_user_id, fco_updated_by_user_id, fco_criado_em, fco_atualizado_em, fco_is_deleted)
            VALUES
                ({id}, {nome}, {TrimOrNull(request.Cargo)}, {NormalizeEmail(request.Email)}, {TrimOrNull(request.Telefone)},
                 {TrimOrNull(request.Celular)}, {request.Principal}, {request.Ativo}, {tenantId}, UNHEX(REPLACE(UUID(), '-', '')),
                 {userId}, {userId}, UTC_TIMESTAMP(), UTC_TIMESTAMP(), 0)
            """,
            ct);

        contatoId = await ExecuteScalarAsync<int>(db, "SELECT LAST_INSERT_ID();", ct);
        var created = await LoadFornecedorContatoAsync(db, contatoId, ct)
            ?? throw new InvalidOperationException("Contato gravado sem releitura na transacao.");
        db.LogsAuditoria.Add(CreateIamAuditLog(
            principal,
            httpContext,
            "md_fornecedor_contatos",
            contatoId,
            AcaoAuditoria.INSERT,
            null,
            created));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    });

    var persisted = await LoadFornecedorContatoAsync(db, contatoId, ct);
    return persisted is null
        ? Results.Problem("O contato foi gravado, mas a releitura de confirmacao falhou. Nao repita a operacao sem consultar o fornecedor.", statusCode: StatusCodes.Status500InternalServerError)
        : Results.Created($"/api/fornecedores/{id}/contatos/{contatoId}", ApiResponse<FornecedorContatoDto>.Ok(persisted, "Contato do fornecedor criado e relido."));
})
.WithName("MasterDataFornecedoresContatosCriar");

app.MapPut("/api/fornecedores/{id:int}/contatos/{contatoId:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    int contatoId,
    FornecedorContatoRequest request,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (!await db.Fornecedores.AsNoTracking().AnyAsync(item => item.Id == id, ct))
    {
        return Results.NotFound(ApiResponse<FornecedorContatoDto>.Erro("Fornecedor nao encontrado."));
    }

    if (!TryDecodeRowVersion(request.RowVersion, out var expectedRowVersion))
    {
        return Results.BadRequest(ApiResponse<FornecedorContatoDto>.Erro("RowVersion valido e obrigatorio para atualizar o contato."));
    }

    var previous = await LoadFornecedorContatoAsync(db, contatoId, ct);
    if (previous is null || previous.FornecedorId != id)
    {
        return Results.NotFound(ApiResponse<FornecedorContatoDto>.Erro("Contato do fornecedor nao encontrado."));
    }

    var nome = TrimOrNull(request.Nome);
    if (string.IsNullOrWhiteSpace(nome))
    {
        return Results.BadRequest(ApiResponse<FornecedorContatoDto>.Erro("Nome do contato e obrigatorio."));
    }

    var executionStrategy = db.Database.CreateExecutionStrategy();
    var updated = await executionStrategy.ExecuteAsync(async () =>
    {
        db.ChangeTracker.Clear();
        var tenantId = db.CurrentTenantId.ToString();
        var userId = db.CurrentUserId?.ToString();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (request.Principal)
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE md_fornecedor_contatos
                SET fco_principal = 0,
                    fco_row_version = UNHEX(REPLACE(UUID(), '-', '')),
                    fco_updated_by_user_id = {userId},
                    fco_atualizado_em = UTC_TIMESTAMP()
                WHERE fco_fornecedor_id = {id}
                  AND fco_id <> {contatoId}
                  AND fco_tenant_id = {tenantId}
                  AND fco_is_deleted = 0
                """,
                ct);
        }

        var affected = await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE md_fornecedor_contatos
            SET fco_nome = {nome},
                fco_cargo = {TrimOrNull(request.Cargo)},
                fco_email = {NormalizeEmail(request.Email)},
                fco_telefone = {TrimOrNull(request.Telefone)},
                fco_celular = {TrimOrNull(request.Celular)},
                fco_principal = {request.Principal},
                fco_ativo = {request.Ativo},
                fco_row_version = UNHEX(REPLACE(UUID(), '-', '')),
                fco_updated_by_user_id = {userId},
                fco_atualizado_em = UTC_TIMESTAMP()
            WHERE fco_id = {contatoId}
              AND fco_fornecedor_id = {id}
              AND fco_tenant_id = {tenantId}
              AND fco_is_deleted = 0
              AND fco_row_version = {expectedRowVersion}
            """,
            ct);
        if (affected != 1)
        {
            await transaction.RollbackAsync(ct);
            return (Success: false, Contact: (FornecedorContatoDto?)null);
        }

        var current = await LoadFornecedorContatoAsync(db, contatoId, ct)
            ?? throw new InvalidOperationException("Contato atualizado sem releitura na transacao.");
        db.LogsAuditoria.Add(CreateIamAuditLog(
            principal,
            httpContext,
            "md_fornecedor_contatos",
            contatoId,
            AcaoAuditoria.UPDATE,
            previous,
            current));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return (Success: true, Contact: current);
    });

    return updated.Success && updated.Contact is not null
        ? Results.Ok(ApiResponse<FornecedorContatoDto>.Ok(updated.Contact, "Contato do fornecedor atualizado e relido."))
        : Results.Conflict(ApiResponse<FornecedorContatoDto>.Erro("O contato foi alterado por outra sessao. Recarregue os contatos antes de salvar."));
})
.WithName("MasterDataFornecedoresContatosAtualizar");

app.MapDelete("/api/fornecedores/{id:int}/contatos/{contatoId:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    int contatoId,
    string? rowVersion,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (!await db.Fornecedores.AsNoTracking().AnyAsync(item => item.Id == id, ct))
    {
        return Results.NotFound(ApiResponse<FornecedorContatoDto>.Erro("Fornecedor nao encontrado."));
    }

    if (!TryDecodeRowVersion(rowVersion, out var expectedRowVersion))
    {
        return Results.BadRequest(ApiResponse<FornecedorContatoDto>.Erro("RowVersion valido e obrigatorio para desativar o contato."));
    }

    var contato = await LoadFornecedorContatoAsync(db, contatoId, ct);
    if (contato is null || contato.FornecedorId != id)
    {
        return Results.NotFound(ApiResponse<FornecedorContatoDto>.Erro("Contato do fornecedor nao encontrado."));
    }

    var executionStrategy = db.Database.CreateExecutionStrategy();
    var deleted = await executionStrategy.ExecuteAsync(async () =>
    {
        db.ChangeTracker.Clear();
        var tenantId = db.CurrentTenantId.ToString();
        var userId = db.CurrentUserId?.ToString();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var affected = await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE md_fornecedor_contatos
            SET fco_ativo = 0,
                fco_is_deleted = 1,
                fco_deleted_at = UTC_TIMESTAMP(),
                fco_row_version = UNHEX(REPLACE(UUID(), '-', '')),
                fco_updated_by_user_id = {userId},
                fco_atualizado_em = UTC_TIMESTAMP()
            WHERE fco_id = {contatoId}
              AND fco_fornecedor_id = {id}
              AND fco_tenant_id = {tenantId}
              AND fco_is_deleted = 0
              AND fco_row_version = {expectedRowVersion}
            """,
            ct);
        if (affected != 1)
        {
            await transaction.RollbackAsync(ct);
            return false;
        }

        db.LogsAuditoria.Add(CreateIamAuditLog(
            principal,
            httpContext,
            "md_fornecedor_contatos",
            contatoId,
            AcaoAuditoria.DELETE,
            contato,
            null));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return true;
    });

    return deleted
        ? Results.Ok(ApiResponse<FornecedorContatoDto>.Ok(contato with { Ativo = false }, "Contato do fornecedor desativado com soft-delete."))
        : Results.Conflict(ApiResponse<FornecedorContatoDto>.Erro("O contato foi alterado por outra sessao. Recarregue os contatos antes de desativar."));
})
.WithName("MasterDataFornecedoresContatosExcluir");

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

app.MapGet("/api/relatorios/vendas", [Authorize(Policy = "Gerente")] async (
    NexumDbContext db,
    CancellationToken ct) =>
{
    var dashboard = await BuildAdminDashboardAsync(db, ct);
    var relatorio = new RelatorioVendasDto(
        dashboard.Kpis.FaturamentoHoje,
        dashboard.Kpis.FaturamentoMes,
        dashboard.Kpis.FaturamentoAno,
        dashboard.Kpis.PedidosHoje,
        dashboard.Kpis.PedidosMes,
        dashboard.Kpis.TicketMedio,
        dashboard.FaturamentoSemanal,
        dashboard.FaturamentoMensal,
        dashboard.VendasPorLoja,
        dashboard.ProdutosMaisVendidos);

    return Results.Ok(ApiResponse<RelatorioVendasDto>.Ok(relatorio, "Relatorio de vendas consolidado."));
})
.WithName("RelatorioVendas")
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

app.MapGet("/api/lojas/{id:int}", async (int id, NexumDbContext db, CancellationToken ct) =>
{
    var loja = await db.Lojas
        .AsNoTracking()
        .Where(item => item.Id == id)
        .Select(item => new LojaDto(
            item.Id,
            item.Nome,
            item.Slug,
            item.Segmento,
            item.Descricao,
            item.CorPrimaria,
            item.CorSecundaria,
            item.Ativa,
            item.OrdemExibicao))
        .FirstOrDefaultAsync(ct);

    return loja is null
        ? Results.NotFound(ApiResponse<string>.Erro("Loja nao encontrada."))
        : Results.Ok(ApiResponse<LojaDto>.Ok(loja));
})
.AllowAnonymous()
.WithName("LojaPorId")
;

app.MapGet("/api/site/configuracoes/publico", async (NexumDbContext db, CancellationToken ct) =>
{
    var configs = await db.ConfiguracoesSistema
        .AsNoTracking()
        .ToListAsync(ct);

    var configMap = configs
        .GroupBy(item => item.Chave, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(group => group.Key, group => group.Last().Valor, StringComparer.OrdinalIgnoreCase);

    if (!TryParseHeroSlidesConfiguration(
            GetConfigValue(configMap, "home_hero_slides", string.Empty),
            out var heroSlides,
            out var heroSlidesError))
    {
        return Results.Problem(
            title: "Configuração pública de banners inválida",
            detail: heroSlidesError,
            statusCode: StatusCodes.Status500InternalServerError);
    }

    var publicProfiles = await db.SitePerfisPublicos
        .AsNoTracking()
        .Where(perfil => perfil.Publicado)
        .OrderBy(perfil => perfil.OrdemExibicao)
        .ThenBy(perfil => perfil.Nome)
        .ToListAsync(ct);

    var storeCards = publicProfiles
        .Where(perfil => perfil.TipoPerfil == TipoPerfilPublico.Loja)
        .Select(ToStoreCardSiteDto)
        .ToList();
    var partnerCards = publicProfiles
        .Where(perfil => perfil.TipoPerfil != TipoPerfilPublico.Loja)
        .Select(ToPartnerCardSiteDto)
        .ToList();

    var publicConfig = BuildPublicSiteConfig(configMap, heroSlides, storeCards, partnerCards);
    return Results.Ok(ApiResponse<SiteConfiguracaoPublicaDto>.Ok(publicConfig));
})
.AllowAnonymous()
.WithName("SiteConfiguracoesPublicas")
;

app.MapGet("/api/site/lojas", async (NexumDbContext db, CancellationToken ct) =>
{
    var profiles = await db.SitePerfisPublicos
        .AsNoTracking()
        .Where(perfil => perfil.Publicado && perfil.TipoPerfil == TipoPerfilPublico.Loja)
        .OrderBy(perfil => perfil.OrdemExibicao)
        .ThenBy(perfil => perfil.Nome)
        .ToListAsync(ct);
    var items = profiles.Select(ToStoreCardSiteDto).ToList();
    return Results.Ok(ApiResponse<List<StoreCardSiteDto>>.Ok(items, "Lojas públicas carregadas da base oficial.", items.Count));
})
.AllowAnonymous()
.WithName("SiteLojasPublicas")
;

app.MapGet("/api/site/lojas/{slug}", async (string slug, NexumDbContext db, CancellationToken ct) =>
{
    var normalizedSlug = NormalizePublicProfileSlug(slug);
    if (string.IsNullOrWhiteSpace(normalizedSlug))
    {
        return Results.BadRequest(ApiResponse<StoreCardSiteDto>.Erro("Identificador da loja inválido."));
    }

    var profile = await db.SitePerfisPublicos
        .AsNoTracking()
        .FirstOrDefaultAsync(perfil => perfil.Publicado
            && perfil.TipoPerfil == TipoPerfilPublico.Loja
            && perfil.Slug == normalizedSlug, ct);
    return profile is null
        ? Results.NotFound(ApiResponse<StoreCardSiteDto>.Erro("Loja pública não encontrada."))
        : Results.Ok(ApiResponse<StoreCardSiteDto>.Ok(ToStoreCardSiteDto(profile), "Loja pública carregada da base oficial."));
})
.AllowAnonymous()
.WithName("SiteLojaPublicaPorSlug")
;

app.MapGet("/api/site/parceiros", async (string? tipo, NexumDbContext db, CancellationToken ct) =>
{
    TipoPerfilPublico? parsedType = null;
    if (!string.IsNullOrWhiteSpace(tipo))
    {
        if (!Enum.TryParse<TipoPerfilPublico>(tipo.Trim(), true, out var typeValue)
            || typeValue == TipoPerfilPublico.Loja)
        {
            return Results.BadRequest(ApiResponse<List<PartnerCardSiteDto>>.Erro(
                "Tipo inválido. Use Fornecedor, ParceiroVenda, ParceiroCompra ou Marketplace."));
        }

        parsedType = typeValue;
    }

    var query = db.SitePerfisPublicos
        .AsNoTracking()
        .Where(perfil => perfil.Publicado && perfil.TipoPerfil != TipoPerfilPublico.Loja);
    if (parsedType.HasValue)
    {
        query = query.Where(perfil => perfil.TipoPerfil == parsedType.Value);
    }

    var profiles = await query
        .OrderBy(perfil => perfil.OrdemExibicao)
        .ThenBy(perfil => perfil.Nome)
        .ToListAsync(ct);
    var items = profiles.Select(ToPartnerCardSiteDto).ToList();
    return Results.Ok(ApiResponse<List<PartnerCardSiteDto>>.Ok(items, "Parceiros públicos carregados da base oficial.", items.Count));
})
.AllowAnonymous()
.WithName("SiteParceirosPublicos")
;

app.MapGet("/api/site/parceiros/{slug}", async (string slug, NexumDbContext db, CancellationToken ct) =>
{
    var normalizedSlug = NormalizePublicProfileSlug(slug);
    if (string.IsNullOrWhiteSpace(normalizedSlug))
    {
        return Results.BadRequest(ApiResponse<PartnerDetailSiteDto>.Erro("Identificador do parceiro inválido."));
    }

    var profile = await db.SitePerfisPublicos
        .AsNoTracking()
        .FirstOrDefaultAsync(perfil => perfil.Publicado
            && perfil.TipoPerfil != TipoPerfilPublico.Loja
            && perfil.Slug == normalizedSlug, ct);
    if (profile is null)
    {
        return Results.NotFound(ApiResponse<PartnerDetailSiteDto>.Erro("Parceiro público não encontrado."));
    }

    var produtos = await db.SitePerfisPublicosProdutos
        .AsNoTracking()
        .Where(vinculo => vinculo.PerfilPublicoId == profile.Id && vinculo.Publicado)
        .Include(vinculo => vinculo.Produto)
        .ThenInclude(produto => produto.Categoria)
        .OrderBy(vinculo => vinculo.OrdemExibicao)
        .ThenBy(vinculo => vinculo.Produto.Nome)
        .Select(vinculo => vinculo.Produto)
        .ToListAsync(ct);
    var produtosPublicaveis = produtos
        .Where(IsProdutoPublicavel)
        .Select(MapearProdutoLojaDto)
        .ToList();
    return Results.Ok(ApiResponse<PartnerDetailSiteDto>.Ok(
        ToPartnerDetailSiteDto(profile, produtosPublicaveis),
        "Parceiro e produtos autorizados carregados da base oficial."));
})
.AllowAnonymous()
.WithName("SiteParceiroPublicoPorSlug")
;

app.MapGet("/api/site/perfis-publicos", [Authorize(Policy = "Gerente")] async (NexumDbContext db, CancellationToken ct) =>
{
    var profiles = await db.SitePerfisPublicos
        .AsNoTracking()
        .OrderBy(perfil => perfil.TipoPerfil)
        .ThenBy(perfil => perfil.OrdemExibicao)
        .ThenBy(perfil => perfil.Nome)
        .ToListAsync(ct);
    var profileIds = profiles.Select(item => item.Id).ToList();
    var productLinks = await db.SitePerfisPublicosProdutos
        .AsNoTracking()
        .Where(item => profileIds.Contains(item.PerfilPublicoId))
        .OrderBy(item => item.OrdemExibicao)
        .ToListAsync(ct);
    var productIdsByProfile = productLinks
        .GroupBy(item => item.PerfilPublicoId)
        .ToDictionary(group => group.Key, group => group.Select(item => item.ProdutoId).ToList());
    var items = profiles
        .Select(profile => ToSitePerfilPublicoAdminDto(
            profile,
            productIdsByProfile.GetValueOrDefault(profile.Id) ?? []))
        .ToList();
    return Results.Ok(ApiResponse<List<SitePerfilPublicoAdminDto>>.Ok(items, "Perfis públicos carregados da base oficial.", items.Count));
})
.WithName("ListarSitePerfisPublicos")
;

app.MapGet("/api/site/perfis-publicos/origens", [Authorize(Policy = "Gerente")] async (NexumDbContext db, CancellationToken ct) =>
{
    var lojas = await db.Lojas.AsNoTracking()
        .OrderBy(item => item.Nome)
        .Select(item => new SitePerfilOrigemDto(OrigemPerfilPublico.Loja.ToString(), item.Id, item.Nome, item.Slug, item.Ativa ? "Ativo" : "Inativo"))
        .ToListAsync(ct);
    var fornecedores = await db.Fornecedores.AsNoTracking()
        .OrderBy(item => item.NomeFantasia ?? item.RazaoSocial)
        .Select(item => new SitePerfilOrigemDto(
            OrigemPerfilPublico.Fornecedor.ToString(),
            item.Id,
            item.NomeFantasia ?? item.RazaoSocial,
            $"fornecedor-{item.Id}",
            item.Status.ToString()))
        .ToListAsync(ct);
    var marketplaces = await db.Marketplaces.AsNoTracking()
        .OrderBy(item => item.Nome)
        .Select(item => new SitePerfilOrigemDto(
            OrigemPerfilPublico.Marketplace.ToString(),
            item.Id,
            item.Nome,
            item.Slug,
            item.Ativo ? "Ativo" : "Inativo"))
        .ToListAsync(ct);
    var items = lojas.Concat(fornecedores).Concat(marketplaces).ToList();
    return Results.Ok(ApiResponse<List<SitePerfilOrigemDto>>.Ok(items, "Origens comerciais carregadas da base oficial.", items.Count));
})
.WithName("ListarOrigensSitePerfisPublicos")
;

app.MapGet("/api/site/perfis-publicos/produtos-disponiveis", [Authorize(Policy = "Gerente")] async (
    NexumDbContext db,
    CancellationToken ct) =>
{
    var products = await FiltrarProdutosPublicaveis(db.Produtos.AsNoTracking())
        .OrderBy(item => item.Nome)
        .Select(item => new SitePerfilProdutoDisponivelDto(
            item.Id,
            item.Sku,
            item.Nome,
            item.Slug,
            item.PrecoPromocional.HasValue && item.PrecoPromocional > 0 && item.PrecoPromocional < item.Preco
                ? item.PrecoPromocional.Value
                : item.Preco,
            item.ImagemPrincipal!,
            item.LojaId,
            item.FornecedorId,
            item.TipoProduto.ToString()))
        .ToListAsync(ct);
    return Results.Ok(ApiResponse<List<SitePerfilProdutoDisponivelDto>>.Ok(
        products,
        "Produtos elegíveis para divulgação carregados da base oficial.",
        products.Count));
})
.WithName("ListarProdutosDisponiveisSitePerfisPublicos")
;

app.MapPost("/api/site/perfis-publicos", [Authorize(Policy = "Gerente")] async (
    SitePerfilPublicoRequest request,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var validation = await ValidateSitePerfilPublicoRequestAsync(request, null, db, ct);
    if (validation is not null)
    {
        return validation;
    }

    var profile = new SitePerfilPublico();
    ApplySitePerfilPublicoRequest(profile, request);
    db.SitePerfisPublicos.Add(profile);
    await SyncSitePerfilPublicoProdutosAsync(profile.Id, request.ProdutoIds, db, ct);
    await db.SaveChangesAsync(ct);

    var created = ToSitePerfilPublicoAdminDto(profile, NormalizeSiteProfileProductIds(request.ProdutoIds));
    db.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "site_perfis_publicos", 0, AcaoAuditoria.INSERT, null, created));
    await db.SaveChangesAsync(ct);

    var persisted = await db.SitePerfisPublicos.AsNoTracking().FirstOrDefaultAsync(item => item.Id == profile.Id, ct);
    var persistedProductIds = await LoadSitePerfilPublicoProductIdsAsync(profile.Id, db, ct);
    return persisted is null
        ? Results.Problem("O perfil público foi gravado, mas a releitura de confirmação falhou.", statusCode: StatusCodes.Status500InternalServerError)
        : Results.Created($"/api/site/perfis-publicos/{profile.Id}", ApiResponse<SitePerfilPublicoAdminDto>.Ok(
            ToSitePerfilPublicoAdminDto(persisted, persistedProductIds),
            "Perfil público criado e relido do banco oficial."));
})
.WithName("CriarSitePerfilPublico")
;

app.MapPut("/api/site/perfis-publicos/{id:guid}", [Authorize(Policy = "Gerente")] async (
    Guid id,
    SitePerfilPublicoRequest request,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (!TryDecodeRowVersion(request.RowVersion, out var expectedRowVersion))
    {
        return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro("RowVersion válido é obrigatório para atualizar o perfil público."));
    }

    var validation = await ValidateSitePerfilPublicoRequestAsync(request, id, db, ct);
    if (validation is not null)
    {
        return validation;
    }

    try
    {
        var profile = await db.SitePerfisPublicos.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (profile is null)
        {
            return Results.NotFound(ApiResponse<SitePerfilPublicoAdminDto>.Erro("Perfil público não encontrado."));
        }

        if (!profile.RowVersion.SequenceEqual(expectedRowVersion))
        {
            return Results.Conflict(ApiResponse<SitePerfilPublicoAdminDto>.Erro("O perfil público foi alterado por outra sessão. Recarregue antes de salvar."));
        }

        var previousProductIds = await LoadSitePerfilPublicoProductIdsAsync(profile.Id, db, ct);
        var previous = ToSitePerfilPublicoAdminDto(profile, previousProductIds);
        ApplySitePerfilPublicoRequest(profile, request);
        db.Entry(profile).Property(item => item.RowVersion).OriginalValue = expectedRowVersion;
        await SyncSitePerfilPublicoProdutosAsync(profile.Id, request.ProdutoIds, db, ct);
        await db.SaveChangesAsync(ct);

        var updated = ToSitePerfilPublicoAdminDto(profile, NormalizeSiteProfileProductIds(request.ProdutoIds));
        db.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "site_perfis_publicos", 0, AcaoAuditoria.UPDATE, previous, updated));
        await db.SaveChangesAsync(ct);
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict(ApiResponse<SitePerfilPublicoAdminDto>.Erro("O perfil público foi alterado por outra sessão. Recarregue antes de salvar."));
    }

    var persisted = await db.SitePerfisPublicos.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, ct);
    var persistedProductIds = await LoadSitePerfilPublicoProductIdsAsync(id, db, ct);
    return persisted is null
        ? Results.Problem("O perfil público foi atualizado, mas a releitura de confirmação falhou.", statusCode: StatusCodes.Status500InternalServerError)
        : Results.Ok(ApiResponse<SitePerfilPublicoAdminDto>.Ok(
            ToSitePerfilPublicoAdminDto(persisted, persistedProductIds),
            "Perfil público atualizado e relido do banco oficial."));
})
.WithName("AtualizarSitePerfilPublico")
;

app.MapDelete("/api/site/perfis-publicos/{id:guid}", [Authorize(Policy = "Gerente")] async (
    Guid id,
    string rowVersion,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (!TryDecodeRowVersion(rowVersion, out var expectedRowVersion))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("RowVersion válido é obrigatório para remover o perfil público."));
    }

    try
    {
        var profile = await db.SitePerfisPublicos.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (profile is null)
        {
            return Results.NotFound(ApiResponse<string>.Erro("Perfil público não encontrado."));
        }

        if (!profile.RowVersion.SequenceEqual(expectedRowVersion))
        {
            return Results.Conflict(ApiResponse<string>.Erro("O perfil público foi alterado por outra sessão. Recarregue antes de remover."));
        }

        var previousProductIds = await LoadSitePerfilPublicoProductIdsAsync(profile.Id, db, ct);
        var previous = ToSitePerfilPublicoAdminDto(profile, previousProductIds);
        db.Entry(profile).Property(item => item.RowVersion).OriginalValue = expectedRowVersion;
        var productLinks = await db.SitePerfisPublicosProdutos
            .Where(item => item.PerfilPublicoId == profile.Id)
            .ToListAsync(ct);
        db.SitePerfisPublicosProdutos.RemoveRange(productLinks);
        db.SitePerfisPublicos.Remove(profile);
        await db.SaveChangesAsync(ct);
        db.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "site_perfis_publicos", 0, AcaoAuditoria.DELETE, previous, null));
        await db.SaveChangesAsync(ct);
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict(ApiResponse<string>.Erro("O perfil público foi alterado por outra sessão. Recarregue antes de remover."));
    }

    var stillExists = await db.SitePerfisPublicos.AsNoTracking().AnyAsync(item => item.Id == id, ct);
    return stillExists
        ? Results.Problem("A exclusão lógica do perfil público não foi confirmada.", statusCode: StatusCodes.Status500InternalServerError)
        : Results.Ok(ApiResponse<string>.Ok("Perfil público removido e exclusão confirmada no banco oficial."));
})
.WithName("ExcluirSitePerfilPublico")
;

app.MapGet("/api/site/configuracoes", [Authorize(Policy = "Gerente")] async (NexumDbContext db, CancellationToken ct) =>
{
    var items = await db.ConfiguracoesSistema
        .AsNoTracking()
        .Where(item => item.Chave.StartsWith("site_") || item.Chave.StartsWith("home_"))
        .Where(item => item.Chave != "home_lojas_cards" && item.Chave != "home_partner_cards")
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

app.MapPut("/api/site/configuracoes", [Authorize(Policy = "Gerente")] async (
    SiteConfiguracaoUpdateRequest request,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (request.Itens is null || request.Itens.Count == 0)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Nenhuma configuração foi enviada."));
    }

    var invalidItem = request.Itens.FirstOrDefault(item =>
        string.IsNullOrWhiteSpace(item.Chave)
        || item.Chave.Trim().Length > 100
        || (!item.Chave.Trim().StartsWith("site_", StringComparison.OrdinalIgnoreCase)
            && !item.Chave.Trim().StartsWith("home_", StringComparison.OrdinalIgnoreCase))
        || item.Descricao?.Trim().Length > 255
        || item.Grupo?.Trim().Length > 50);

    if (invalidItem is not null)
    {
        return Results.BadRequest(ApiResponse<string>.Erro(
            "A solicitação contém uma chave, descrição ou grupo inválido para as configurações públicas do site."));
    }

    var requestedKeys = request.Itens
        .Select(item => item.Chave.Trim())
        .ToList();

    if (requestedKeys.Any(key => string.Equals(key, "home_lojas_cards", StringComparison.OrdinalIgnoreCase)
        || string.Equals(key, "home_partner_cards", StringComparison.OrdinalIgnoreCase)))
    {
        return Results.BadRequest(ApiResponse<string>.Erro(
            "Lojas e parceiros devem ser alterados pelo cadastro estruturado de perfis públicos."));
    }

    if (requestedKeys.Distinct(StringComparer.OrdinalIgnoreCase).Count() != requestedKeys.Count)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("A solicitação contém chaves de configuração duplicadas."));
    }

    var normalizedJsonValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var item in request.Itens)
    {
        if (string.Equals(item.Chave.Trim(), "home_hero_interval_seconds", StringComparison.OrdinalIgnoreCase)
            && (!int.TryParse(item.Valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intervalSeconds)
                || intervalSeconds is < 3 or > 30))
        {
            return Results.BadRequest(ApiResponse<string>.Erro("O intervalo automático dos banners deve estar entre 3 e 30 segundos."));
        }

        if (string.Equals(item.Chave.Trim(), "site_partner_rotation_seconds", StringComparison.OrdinalIgnoreCase)
            && (!int.TryParse(item.Valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out var partnerRotationSeconds)
                || partnerRotationSeconds is < 5 or > 60))
        {
            return Results.BadRequest(ApiResponse<string>.Erro("O intervalo do rodízio de parceiros deve estar entre 5 e 60 segundos."));
        }

        if (IsSiteColorConfigurationKey(item.Chave.Trim()) && !IsValidHexColor(item.Valor))
        {
            return Results.BadRequest(ApiResponse<string>.Erro(
                $"A cor da configuração '{item.Chave}' deve usar o formato hexadecimal #RRGGBB."));
        }

        var isJson = string.Equals(item.Tipo?.Trim(), TipoConfiguracao.JSON.ToString(), StringComparison.OrdinalIgnoreCase)
            || LooksLikeJson(item.Valor);

        if (!isJson)
        {
            continue;
        }

        try
        {
            using var document = JsonDocument.Parse(item.Valor ?? string.Empty);
            if (document.RootElement.ValueKind is not JsonValueKind.Array and not JsonValueKind.Object)
            {
                return Results.BadRequest(ApiResponse<string>.Erro($"O valor JSON da chave '{item.Chave}' deve ser um objeto ou uma lista."));
            }
        }
        catch (JsonException)
        {
            return Results.BadRequest(ApiResponse<string>.Erro($"O valor JSON da chave '{item.Chave}' é inválido."));
        }

        if (string.Equals(item.Chave.Trim(), "home_hero_slides", StringComparison.OrdinalIgnoreCase))
        {
            if (!TryParseHeroSlidesConfiguration(item.Valor, out var normalizedSlides, out var validationError))
            {
                return Results.BadRequest(ApiResponse<string>.Erro(validationError));
            }

            normalizedJsonValues[item.Chave.Trim()] = JsonSerializer.Serialize(normalizedSlides, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

    }

    var existing = await db.ConfiguracoesSistema
        .AsNoTracking()
        .Where(item => requestedKeys.Contains(item.Chave))
        .ToListAsync(ct);

    var protectedKey = existing.FirstOrDefault(item => !item.Editavel);
    if (protectedKey is not null)
    {
        return Results.BadRequest(ApiResponse<string>.Erro($"A configuração '{protectedKey.Chave}' não permite edição."));
    }

    var persistedItems = new List<SiteConfiguracaoItemDto>();
    SiteConfiguracaoUpdateItemDto? unconfirmedKey = null;
    var executionStrategy = db.Database.CreateExecutionStrategy();
    await executionStrategy.ExecuteAsync(async () =>
    {
        db.ChangeTracker.Clear();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var transactionalExisting = await db.ConfiguracoesSistema
            .Where(item => requestedKeys.Contains(item.Chave))
            .ToListAsync(ct);
        var previousItems = transactionalExisting
            .Select(item => new SiteConfiguracaoItemDto(
                item.Id,
                item.Chave,
                item.Valor,
                item.Tipo.ToString(),
                item.Descricao,
                item.Grupo,
                item.Editavel,
                item.UpdatedAt))
            .ToList();

        foreach (var item in request.Itens)
        {
            var chave = item.Chave.Trim();
            var entity = transactionalExisting.FirstOrDefault(config =>
                string.Equals(config.Chave, chave, StringComparison.OrdinalIgnoreCase));
            if (entity is null)
            {
                entity = new ConfiguracaoSistema
                {
                    Chave = chave,
                    CreatedAt = DateTime.UtcNow
                };
                db.ConfiguracoesSistema.Add(entity);
                transactionalExisting.Add(entity);
            }

            entity.Valor = normalizedJsonValues.TryGetValue(chave, out var normalizedJson)
                ? normalizedJson
                : item.Valor?.Trim();
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
        var changedItems = transactionalExisting
            .Where(item => requestedKeys.Contains(item.Chave, StringComparer.OrdinalIgnoreCase))
            .Select(item => new SiteConfiguracaoItemDto(
                item.Id,
                item.Chave,
                item.Valor,
                item.Tipo.ToString(),
                item.Descricao,
                item.Grupo,
                item.Editavel,
                item.UpdatedAt))
            .ToList();
        db.LogsAuditoria.Add(CreateIamAuditLog(
            principal,
            httpContext,
            "configuracoes_sistema",
            0,
            AcaoAuditoria.UPDATE,
            previousItems,
            changedItems));
        await db.SaveChangesAsync(ct);

        persistedItems = await db.ConfiguracoesSistema
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
        var persistedByKey = persistedItems.ToDictionary(item => item.Chave, StringComparer.OrdinalIgnoreCase);
        unconfirmedKey = request.Itens.FirstOrDefault(item =>
        {
            var key = item.Chave.Trim();
            var expectedValue = normalizedJsonValues.TryGetValue(key, out var normalizedJson)
                ? normalizedJson
                : item.Valor?.Trim();
            return !persistedByKey.TryGetValue(key, out var persisted)
                || !string.Equals(persisted.Valor, expectedValue, StringComparison.Ordinal);
        });
        if (unconfirmedKey is null)
        {
            await transaction.CommitAsync(ct);
        }
    });

    if (unconfirmedKey is not null)
    {
        return Results.Problem(
            title: "Persistência de configuração não confirmada",
            detail: $"A configuração '{unconfirmedKey.Chave}' não foi relida do banco com o valor solicitado.",
            statusCode: StatusCodes.Status500InternalServerError);
    }

    return Results.Ok(ApiResponse<List<SiteConfiguracaoItemDto>>.Ok(
        persistedItems,
        $"{request.Itens.Count} configurações públicas foram gravadas e confirmadas no banco."));
})
.WithName("AtualizarSiteConfiguracoes")
;

app.MapGet("/api/site/midias", [Authorize(Policy = "Gerente")] async (
    string? tipo,
    NexumDbContext db,
    HttpContext http,
    CancellationToken ct) =>
{
    var query = db.SiteMidias.AsNoTracking();
    if (!string.IsNullOrWhiteSpace(tipo))
    {
        if (!Enum.TryParse<TipoMidiaSite>(tipo.Trim(), true, out var parsedType))
        {
            return Results.BadRequest(ApiResponse<List<SiteMidiaDto>>.Erro("Tipo de midia invalido. Use Logo, Banner, Loja ou Institucional."));
        }

        query = query.Where(midia => midia.Tipo == parsedType);
    }

    var entities = await query
        .OrderByDescending(midia => midia.CreatedAt)
        .ToListAsync(ct);
    var items = entities.Select(midia => MapSiteMidiaDto(midia, http)).ToList();
    return Results.Ok(ApiResponse<List<SiteMidiaDto>>.Ok(items));
})
.WithName("ListarSiteMidias")
;

app.MapPost("/api/site/midias", [Authorize(Policy = "Gerente")] async (
    SiteMidiaUploadRequest request,
    NexumDbContext db,
    IWebHostEnvironment environment,
    HttpContext http,
    CancellationToken ct) =>
{
    var nome = request.Nome?.Trim();
    var textoAlternativo = request.TextoAlternativo?.Trim();
    var originalFileName = Path.GetFileName(request.FileName?.Trim());
    if (string.IsNullOrWhiteSpace(nome) || nome.Length is < 3 or > 150)
    {
        return Results.BadRequest(ApiResponse<SiteMidiaDto>.Erro("Nome da midia deve ter entre 3 e 150 caracteres."));
    }

    if (string.IsNullOrWhiteSpace(textoAlternativo) || textoAlternativo.Length is < 3 or > 240)
    {
        return Results.BadRequest(ApiResponse<SiteMidiaDto>.Erro("Texto alternativo deve ter entre 3 e 240 caracteres."));
    }

    if (string.IsNullOrWhiteSpace(originalFileName) || originalFileName.Length > 255)
    {
        return Results.BadRequest(ApiResponse<SiteMidiaDto>.Erro("Nome do arquivo e obrigatorio e deve ter ate 255 caracteres."));
    }

    if (!Enum.TryParse<TipoMidiaSite>(request.Tipo?.Trim(), true, out var tipoMidia))
    {
        return Results.BadRequest(ApiResponse<SiteMidiaDto>.Erro("Tipo de midia invalido. Use Logo, Banner, Loja ou Institucional."));
    }

    if (!TryDecodeSiteImage(request, out var decoded, out var decodeError))
    {
        return Results.BadRequest(ApiResponse<SiteMidiaDto>.Erro(decodeError));
    }

    var webRoot = environment.WebRootPath;
    var uploadRoot = string.IsNullOrWhiteSpace(webRoot) ? null : Path.Combine(webRoot, "uploads");
    if (string.IsNullOrWhiteSpace(uploadRoot) || !Directory.Exists(uploadRoot))
    {
        return Results.Problem(
            title: "Storage publico indisponivel",
            detail: "Storage:PublicWebRoot nao aponta para o diretorio oficial com a pasta uploads.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    var id = Guid.NewGuid();
    var storedFileName = $"site-{id:N}{decoded.Extension}";
    var relativePath = $"/uploads/{storedFileName}";
    var fullPath = Path.Combine(uploadRoot, storedFileName);

    await using (var stream = new FileStream(
        fullPath,
        FileMode.CreateNew,
        FileAccess.Write,
        FileShare.None,
        65_536,
        FileOptions.Asynchronous | FileOptions.WriteThrough))
    {
        await stream.WriteAsync(decoded.Bytes, ct);
    }

    var entity = new SiteMidia
    {
        Id = id,
        Nome = nome,
        Tipo = tipoMidia,
        TextoAlternativo = textoAlternativo,
        NomeArquivoOriginal = originalFileName,
        CaminhoRelativo = relativePath,
        ContentType = decoded.ContentType,
        TamanhoBytes = decoded.Bytes.LongLength,
        Largura = decoded.Width,
        Altura = decoded.Height
    };

    db.SiteMidias.Add(entity);
    try
    {
        await db.SaveChangesAsync(ct);
    }
    catch
    {
        File.Delete(fullPath);
        throw;
    }

    var response = MapSiteMidiaDto(entity, http);
    return Results.Created($"/api/site/midias/{entity.Id}", ApiResponse<SiteMidiaDto>.Ok(response, "Midia validada e persistida."));
})
.WithName("CriarSiteMidia")
;

app.MapPut("/api/site/midias/{id:guid}", [Authorize(Policy = "Gerente")] async (
    Guid id,
    SiteMidiaUpdateRequest request,
    NexumDbContext db,
    HttpContext http,
    CancellationToken ct) =>
{
    var nome = request.Nome?.Trim();
    var textoAlternativo = request.TextoAlternativo?.Trim();
    if (string.IsNullOrWhiteSpace(nome) || nome.Length is < 3 or > 150)
    {
        return Results.BadRequest(ApiResponse<SiteMidiaDto>.Erro("Nome da midia deve ter entre 3 e 150 caracteres."));
    }

    if (string.IsNullOrWhiteSpace(textoAlternativo) || textoAlternativo.Length is < 3 or > 240)
    {
        return Results.BadRequest(ApiResponse<SiteMidiaDto>.Erro("Texto alternativo deve ter entre 3 e 240 caracteres."));
    }

    if (!Enum.TryParse<TipoMidiaSite>(request.Tipo?.Trim(), true, out var tipoMidia))
    {
        return Results.BadRequest(ApiResponse<SiteMidiaDto>.Erro("Tipo de midia invalido. Use Logo, Banner, Loja ou Institucional."));
    }

    if (!TryParseSiteMediaRowVersion(request.RowVersion, out var rowVersion))
    {
        return Results.BadRequest(ApiResponse<SiteMidiaDto>.Erro("RowVersion invalida."));
    }

    var entity = await db.SiteMidias.FirstOrDefaultAsync(midia => midia.Id == id, ct);
    if (entity is null)
    {
        return Results.NotFound(ApiResponse<SiteMidiaDto>.Erro("Midia nao encontrada."));
    }

    if (!entity.RowVersion.SequenceEqual(rowVersion))
    {
        return Results.Conflict(ApiResponse<SiteMidiaDto>.Erro("A midia foi alterada por outra sessao. Recarregue a biblioteca."));
    }

    entity.Nome = nome;
    entity.Tipo = tipoMidia;
    entity.TextoAlternativo = textoAlternativo;

    try
    {
        await db.SaveChangesAsync(ct);
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict(ApiResponse<SiteMidiaDto>.Erro("A midia foi alterada por outra sessao. Recarregue a biblioteca."));
    }

    return Results.Ok(ApiResponse<SiteMidiaDto>.Ok(MapSiteMidiaDto(entity, http), "Metadados da midia atualizados."));
})
.WithName("AtualizarSiteMidia")
;

app.MapDelete("/api/site/midias/{id:guid}", [Authorize(Policy = "Gerente")] async (
    Guid id,
    string rowVersion,
    NexumDbContext db,
    IWebHostEnvironment environment,
    CancellationToken ct) =>
{
    if (!TryParseSiteMediaRowVersion(rowVersion, out var parsedRowVersion))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("RowVersion invalida."));
    }

    var entity = await db.SiteMidias.FirstOrDefaultAsync(midia => midia.Id == id, ct);
    if (entity is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Midia nao encontrada."));
    }

    if (!entity.RowVersion.SequenceEqual(parsedRowVersion))
    {
        return Results.Conflict(ApiResponse<string>.Erro("A midia foi alterada por outra sessao. Recarregue a biblioteca."));
    }

    var referencedByConfiguration = await db.ConfiguracoesSistema
        .AsNoTracking()
        .AnyAsync(config => config.Valor != null && config.Valor.Contains(entity.CaminhoRelativo), ct);
    var referencedByProfile = await db.SitePerfisPublicos
        .AsNoTracking()
        .AnyAsync(profile => profile.LogoUrl == entity.CaminhoRelativo || profile.BannerUrl == entity.CaminhoRelativo, ct);
    if (referencedByConfiguration || referencedByProfile)
    {
        return Results.Conflict(ApiResponse<string>.Erro("Midia vinculada a configuracao ou perfil publico. Remova o vinculo e salve antes de excluir."));
    }

    if (!TryResolvePublicMediaFile(environment.WebRootPath, entity.CaminhoRelativo, out var physicalPath))
    {
        return Results.Problem("O caminho fisico da midia persistida e invalido.", statusCode: StatusCodes.Status500InternalServerError);
    }

    byte[]? physicalContent = null;
    if (File.Exists(physicalPath))
    {
        physicalContent = await File.ReadAllBytesAsync(physicalPath, ct);
        File.Delete(physicalPath);
    }

    db.SiteMidias.Remove(entity);
    try
    {
        await db.SaveChangesAsync(ct);
    }
    catch (DbUpdateConcurrencyException)
    {
        if (physicalContent is not null)
        {
            await File.WriteAllBytesAsync(physicalPath, physicalContent, CancellationToken.None);
        }

        return Results.Conflict(ApiResponse<string>.Erro("A midia foi alterada por outra sessao. Recarregue a biblioteca."));
    }
    catch
    {
        if (physicalContent is not null)
        {
            await File.WriteAllBytesAsync(physicalPath, physicalContent, CancellationToken.None);
        }

        throw;
    }

    return Results.NoContent();
})
.WithName("ExcluirSiteMidia")
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
            categoria.Ativa,
            categoria.LojaId))
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

    if (request.LojaId <= 0)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Selecione a loja da categoria."));
    }

    var lojaExiste = await db.Lojas
        .AsNoTracking()
        .AnyAsync(loja => loja.Id == request.LojaId && loja.Ativa, ct);
    if (!lojaExiste)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("A loja selecionada nao existe ou esta inativa."));
    }

    var exists = await db.Categorias.AnyAsync(
        categoria => categoria.LojaId == request.LojaId && categoria.Slug == slug, ct);
    if (exists)
    {
        return Results.Conflict(ApiResponse<string>.Erro("Categoria ja existe nesta loja."));
    }

    int? categoriaPaiId = null;
    if (!string.IsNullOrWhiteSpace(request.CategoriaPaiId))
    {
        var categoriaPaiSlug = request.CategoriaPaiId.Trim();
        categoriaPaiId = await db.Categorias
            .AsNoTracking()
            .Where(categoria => categoria.Slug == categoriaPaiSlug
                && categoria.LojaId == request.LojaId
                && categoria.Ativa)
            .Select(categoria => (int?)categoria.Id)
            .FirstOrDefaultAsync(ct);
        if (!categoriaPaiId.HasValue)
        {
            return Results.BadRequest(ApiResponse<string>.Erro("A categoria pai nao existe ou pertence a outra loja."));
        }
    }

    var categoria = new Categoria
    {
        LojaId = request.LojaId,
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
        categoria.Ativa,
        categoria.LojaId), "Categoria cadastrada."));
})
.WithName("CriarCategoria")
;

app.MapGet("/api/produtos", async (
    string? categoria_id,
    string? categoriaIdFiltro,
    string? busca,
    int? lojaId,
    int? pagina,
    int? itensPorPagina,
    int? Pagina,
    int? ItensPorPagina,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var termoBusca = TrimOrNull(busca);
    if (termoBusca?.Length > 256)
    {
        return Results.BadRequest(ApiResponse<List<ProdutoLojaDto>>.Erro(
            "A pesquisa do catálogo aceita no máximo 256 caracteres."));
    }

    IQueryable<Produto> query = FiltrarProdutosPublicaveis(db.Produtos.AsNoTracking());
    if (lojaId is > 0)
    {
        query = query.Where(produto => produto.LojaId == lojaId.Value);
    }
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

    if (!string.IsNullOrWhiteSpace(termoBusca))
    {
        var buscaSomenteDigitos = termoBusca.All(char.IsDigit);
        query = query.Where(produto =>
            produto.Nome.Contains(termoBusca) ||
            produto.Sku.Contains(termoBusca) ||
            (produto.DescricaoCurta != null && produto.DescricaoCurta.Contains(termoBusca)) ||
            (produto.Categoria != null && produto.Categoria.Nome.Contains(termoBusca)) ||
            (produto.CodigoBarras != null && (
                produto.CodigoBarras.Contains(termoBusca) ||
                (buscaSomenteDigitos && produto.CodigoBarras
                    .Replace(".", "")
                    .Replace("/", "")
                    .Replace("-", "")
                    .Replace(" ", "")
                    .Contains(termoBusca)))) ||
            (produto.QrCode != null && produto.QrCode.Contains(termoBusca)));
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

    if (!TryGetImageDimensions(bytes, contentType, out var imageWidth, out var imageHeight)
        || Math.Min(imageWidth, imageHeight) < 720
        || (long)imageWidth * imageHeight < 1_000_000)
    {
        return Results.BadRequest(ApiResponse<string>.Erro(
            "A imagem precisa ter pelo menos 1 megapixel e 720 px no lado menor."));
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

    if (!TryGetGalleryImageCount(request.ImagensGaleria, out var galleryCount) || galleryCount > 4)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("A galeria aceita no maximo 4 imagens opcionais."));
    }

    if (request.LojaId <= 0)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Selecione a loja do produto."));
    }

    var lojaExiste = await db.Lojas
        .AsNoTracking()
        .AnyAsync(loja => loja.Id == request.LojaId && loja.Ativa, ct);
    if (!lojaExiste)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("A loja selecionada nao existe ou esta inativa."));
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

    int? categoriaId = null;
    var categoriaSlug = !string.IsNullOrWhiteSpace(request.SubcategoriaId)
        ? request.SubcategoriaId
        : request.CategoriaId;
    if (!string.IsNullOrWhiteSpace(categoriaSlug))
    {
        categoriaId = await db.Categorias
            .AsNoTracking()
            .Where(categoria => categoria.Slug == categoriaSlug
                && categoria.LojaId == request.LojaId
                && categoria.Ativa)
            .Select(categoria => (int?)categoria.Id)
            .FirstOrDefaultAsync(ct);
        if (!categoriaId.HasValue)
        {
            return Results.BadRequest(ApiResponse<string>.Erro("A categoria selecionada nao existe, esta inativa ou pertence a outra loja."));
        }
    }
    else
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Selecione uma categoria do produto."));
    }

    var produto = new Produto
    {
        LojaId = request.LojaId,
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

    if (!TryGetGalleryImageCount(request.ImagensGaleria, out var galleryCount) || galleryCount > 4)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("A galeria aceita no maximo 4 imagens opcionais."));
    }

    var produto = await db.Produtos.FirstOrDefaultAsync(item => item.Slug == id, ct);
    if (produto is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Produto nao encontrado."));
    }

    var lojaAlvoId = request.LojaId > 0 ? request.LojaId : produto.LojaId;
    var lojaAlvoExiste = await db.Lojas
        .AsNoTracking()
        .AnyAsync(loja => loja.Id == lojaAlvoId && loja.Ativa, ct);
    if (!lojaAlvoExiste)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("A loja selecionada nao existe ou esta inativa."));
    }

    var skuAnterior = produto.Sku;
    var nomeAnterior = produto.Nome;
    var tipoAnterior = produto.TipoProduto.ToString();
    var fornecedorAnteriorId = produto.FornecedorId;
    var codigoBarrasEraGerado = string.IsNullOrWhiteSpace(produto.CodigoBarras)
        || string.Equals(produto.CodigoBarras, GerarCodigoBarrasProdutoPorSku(skuAnterior), StringComparison.Ordinal);
    var qrCodeEraGerado = string.IsNullOrWhiteSpace(produto.QrCode)
        || IsGeneratedProductQrCode(produto.QrCode, skuAnterior, nomeAnterior, tipoAnterior, fornecedorAnteriorId);

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
    if (codigoBarrasEraGerado)
    {
        produto.CodigoBarras = GerarCodigoBarrasProdutoPorSku(produto.Sku);
    }
    if (qrCodeEraGerado)
    {
        produto.QrCode = GerarQrCodeProdutoCadastro(
            produto.Sku,
            request.Nome,
            produto.TipoProduto.ToString(),
            produto.FornecedorId);
    }
    produto.IdentificacaoEstoque = GerarIdentificacaoEstoqueCadastro(produto.Sku, request.Nome, request.TipoProduto, request.FornecedorId);
    produto.Destaque = request.Destaque;
    produto.LojaId = lojaAlvoId;
    produto.UpdatedAt = DateTime.UtcNow;

    var categoriaAtualizacaoSlug = !string.IsNullOrWhiteSpace(request.SubcategoriaId)
        ? request.SubcategoriaId
        : request.CategoriaId;
    if (!string.IsNullOrWhiteSpace(categoriaAtualizacaoSlug))
    {
        var categoriaId = await db.Categorias
            .AsNoTracking()
            .Where(categoria => categoria.Slug == categoriaAtualizacaoSlug
                && categoria.LojaId == lojaAlvoId
                && categoria.Ativa)
            .Select(categoria => (int?)categoria.Id)
            .FirstOrDefaultAsync(ct);
        if (!categoriaId.HasValue)
        {
            return Results.BadRequest(ApiResponse<string>.Erro("A categoria selecionada nao existe, esta inativa ou pertence a outra loja."));
        }
        produto.CategoriaId = categoriaId.Value;
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

app.MapGet("/api/admin/produtos/identificadores", [Authorize(Policy = "Gerente")] async (
    string? termo,
    int? pagina,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var search = TrimOrNull(termo);
    if (search?.Length > 100)
    {
        return Results.BadRequest(ApiResponse<List<ProdutoIdentificadorDto>>.Erro(
            "A pesquisa aceita no máximo 100 caracteres."));
    }

    var page = Math.Max(1, pagina ?? 1);
    const int pageSize = 100;
    IQueryable<Produto> query = db.Produtos.AsNoTracking();
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(item => item.Sku.Contains(search)
            || item.Nome.Contains(search)
            || (item.CodigoBarras != null && item.CodigoBarras.Contains(search))
            || (item.QrCode != null && item.QrCode.Contains(search)));
    }

    var total = await query.CountAsync(ct);
    var totalPaginas = (int)Math.Ceiling(total / (double)pageSize);
    page = Math.Min(page, Math.Max(1, totalPaginas));
    var registros = await query
        .OrderBy(item => item.LojaId)
        .ThenBy(item => item.Sku)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(item => new
        {
            item.Id,
            item.LojaId,
            item.Nome,
            item.Sku,
            item.CodigoBarras,
            item.QrCode,
            item.Ativo,
            RowVersion = EF.Property<byte[]?>(item, "RowVersion")
        })
        .ToListAsync(ct);

    var produtos = registros.Select(item => new ProdutoIdentificadorDto(
        item.Id,
        item.LojaId,
        item.Nome,
        item.Sku,
        item.CodigoBarras,
        item.QrCode,
        item.Ativo,
        Convert.ToBase64String(item.RowVersion ?? Array.Empty<byte>()))).ToList();
    return Results.Ok(ApiResponse<List<ProdutoIdentificadorDto>>.Ok(
        produtos,
        "Identificadores de produtos carregados para o tenant autenticado.",
        total,
        page,
        totalPaginas));
})
.WithName("ProdutosIdentificadoresListar");

app.MapPut("/api/admin/produtos/{id:int}/identificadores", [Authorize(Policy = "Gerente")] async (
    int id,
    ProdutoIdentificadorUpdateRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var codigoBarras = TrimOrNull(request.CodigoBarras);
    var qrCode = TrimOrNull(request.QrCode);
    if (codigoBarras is null && qrCode is null)
    {
        return Results.BadRequest(ApiResponse<ProdutoIdentificadorDto>.Erro(
            "Informe o código de barras, o QR Code ou ambos; os dois identificadores não podem ficar vazios."));
    }
    if (codigoBarras?.Length > 64 || qrCode?.Length > 500)
    {
        return Results.BadRequest(ApiResponse<ProdutoIdentificadorDto>.Erro(
            "Código de barras aceita até 64 caracteres e QR Code até 500 caracteres."));
    }
    if ((codigoBarras?.Any(char.IsControl) ?? false) || (qrCode?.Any(char.IsControl) ?? false))
    {
        return Results.BadRequest(ApiResponse<ProdutoIdentificadorDto>.Erro(
            "Os identificadores não podem conter caracteres de controle."));
    }
    if (!TryDecodeProdutoRowVersion(request.RowVersion, out var requestedRowVersion))
    {
        return Results.BadRequest(ApiResponse<ProdutoIdentificadorDto>.Erro(
            "Versão de produto ausente ou inválida. Atualize a lista antes de salvar."));
    }

    var produto = await db.Produtos.FirstOrDefaultAsync(item => item.Id == id, ct);
    if (produto is null)
    {
        return Results.NotFound(ApiResponse<ProdutoIdentificadorDto>.Erro(
            "Produto não encontrado no tenant autenticado."));
    }

    var rowVersionProperty = db.Entry(produto).Property<byte[]?>("RowVersion");
    var currentRowVersion = rowVersionProperty.CurrentValue ?? Array.Empty<byte>();
    if (!CryptographicOperations.FixedTimeEquals(currentRowVersion, requestedRowVersion))
    {
        return Results.Conflict(ApiResponse<ProdutoIdentificadorDto>.Erro(
            "O produto foi alterado por outra operação. Recarregue a lista e reaplique a alteração."));
    }

    if (codigoBarras is not null && await db.Produtos.AsNoTracking().AnyAsync(
            item => item.Id != id && item.LojaId == produto.LojaId && item.CodigoBarras == codigoBarras,
            ct))
    {
        return Results.Conflict(ApiResponse<ProdutoIdentificadorDto>.Erro(
            "Este código de barras já está associado a outro produto da mesma loja."));
    }
    if (qrCode is not null && await db.Produtos.AsNoTracking().AnyAsync(
            item => item.Id != id && item.LojaId == produto.LojaId && item.QrCode == qrCode,
            ct))
    {
        return Results.Conflict(ApiResponse<ProdutoIdentificadorDto>.Erro(
            "Este QR Code já está associado a outro produto da mesma loja."));
    }

    produto.CodigoBarras = codigoBarras;
    produto.QrCode = qrCode;
    produto.UpdatedAt = DateTime.UtcNow;
    try
    {
        await db.SaveChangesAsync(ct);
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict(ApiResponse<ProdutoIdentificadorDto>.Erro(
            "O produto foi alterado durante a gravação. Recarregue a lista antes de tentar novamente."));
    }

    var updatedRowVersion = db.Entry(produto).Property<byte[]?>("RowVersion").CurrentValue;
    var result = new ProdutoIdentificadorDto(
        produto.Id,
        produto.LojaId,
        produto.Nome,
        produto.Sku,
        produto.CodigoBarras,
        produto.QrCode,
        produto.Ativo,
        Convert.ToBase64String(updatedRowVersion ?? Array.Empty<byte>()));
    return Results.Ok(ApiResponse<ProdutoIdentificadorDto>.Ok(result, "Identificadores do produto atualizados e auditados."));
})
.WithName("ProdutoIdentificadoresAtualizar");

app.MapGet("/api/cupons/{codigo}", async (string codigo, decimal? subtotal, NexumDbContext db, CancellationToken ct) =>
{
    var codigoNormalizado = codigo.Trim().ToUpperInvariant();
    if (codigoNormalizado.Length == 0)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Codigo do cupom obrigatorio."));
    }

    var cupom = await db.Cupons
        .AsNoTracking()
        .FirstOrDefaultAsync(item => item.Codigo == codigoNormalizado && item.Ativo, ct);

    if (cupom is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Cupom invalido."));
    }

    if (cupom.ValidoDe.HasValue && cupom.ValidoDe.Value.Date > DateTime.UtcNow.Date)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Cupom invalido."));
    }

    if (cupom.ValidoAte.HasValue && cupom.ValidoAte.Value.Date < DateTime.UtcNow.Date)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Cupom invalido."));
    }

    if (cupom.QuantidadeUsos.HasValue && cupom.UsosAtuais >= cupom.QuantidadeUsos.Value)
    {
        return Results.Conflict(ApiResponse<string>.Erro("Cupom esgotado."));
    }

    if (subtotal.HasValue && subtotal.Value < cupom.ValorMinimoPedido)
    {
        return Results.BadRequest(ApiResponse<string>.Erro(
            $"Valor minimo para este cupom: {cupom.ValorMinimoPedido.ToString("C", CultureInfo.GetCultureInfo("pt-BR"))}."));
    }

    var dto = cupom.Tipo switch
    {
        TipoCupom.Percentual => new CupomDto(cupom.Codigo, cupom.Valor, null, cupom.ValorMinimoPedido, cupom.ValorMaximoDesconto, false),
        TipoCupom.ValorFixo => new CupomDto(cupom.Codigo, null, cupom.Valor, cupom.ValorMinimoPedido, cupom.ValorMaximoDesconto, false),
        TipoCupom.FreteGratis => new CupomDto(cupom.Codigo, null, null, cupom.ValorMinimoPedido, null, true),
        _ => throw new InvalidOperationException($"Tipo de cupom nao suportado: {cupom.Tipo}.")
    };

    return Results.Ok(ApiResponse<CupomDto>.Ok(dto));
})
.AllowAnonymous()
.WithName("CupomPorCodigo")
;

app.MapGet("/api/admin/cupons", [Authorize(Policy = "Gerente")] async (NexumDbContext db, CancellationToken ct) =>
{
    var cuponsDb = await db.Cupons
        .AsNoTracking()
        .OrderByDescending(cupom => cupom.Ativo)
        .ThenByDescending(cupom => cupom.UpdatedAt)
        .ToListAsync(ct);
    var cupons = cuponsDb.Select(ToCupomAdminDto).ToList();

    return Results.Ok(ApiResponse<List<CupomAdminDto>>.Ok(cupons, total: cupons.Count));
})
.WithName("ListarCuponsAdministracao")
;

app.MapPost("/api/admin/cupons", [Authorize(Policy = "Gerente")] async (
    CupomAdminRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var erroValidacao = ValidateCupomAdminRequest(request);
    if (erroValidacao is not null)
    {
        return Results.BadRequest(ApiResponse<string>.Erro(erroValidacao));
    }

    var codigo = request.Codigo.Trim().ToUpperInvariant();
    if (await db.Cupons.AnyAsync(cupom => cupom.Codigo == codigo, ct))
    {
        return Results.Conflict(ApiResponse<string>.Erro("Ja existe cupom com este codigo."));
    }

    var cupom = new Cupom { CreatedAt = DateTime.UtcNow };
    ApplyCupomAdminRequest(cupom, request, codigo);
    db.Cupons.Add(cupom);
    await db.SaveChangesAsync(ct);

    return Results.Created(
        $"/api/admin/cupons/{cupom.Id}",
        ApiResponse<CupomAdminDto>.Ok(ToCupomAdminDto(cupom), "Cupom cadastrado no banco de dados."));
})
.WithName("CriarCupomAdministracao")
;

app.MapPut("/api/admin/cupons/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    CupomAdminRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var erroValidacao = ValidateCupomAdminRequest(request);
    if (erroValidacao is not null)
    {
        return Results.BadRequest(ApiResponse<string>.Erro(erroValidacao));
    }

    var cupom = await db.Cupons.FirstOrDefaultAsync(item => item.Id == id, ct);
    if (cupom is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Cupom nao encontrado."));
    }

    var codigo = request.Codigo.Trim().ToUpperInvariant();
    if (await db.Cupons.AnyAsync(item => item.Id != id && item.Codigo == codigo, ct))
    {
        return Results.Conflict(ApiResponse<string>.Erro("Ja existe cupom com este codigo."));
    }

    ApplyCupomAdminRequest(cupom, request, codigo);
    await db.SaveChangesAsync(ct);

    return Results.Ok(ApiResponse<CupomAdminDto>.Ok(ToCupomAdminDto(cupom), "Cupom atualizado no banco de dados."));
})
.WithName("AtualizarCupomAdministracao")
;

app.MapDelete("/api/admin/cupons/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var cupom = await db.Cupons.FirstOrDefaultAsync(item => item.Id == id, ct);
    if (cupom is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Cupom nao encontrado."));
    }

    cupom.Ativo = false;
    cupom.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync(ct);

    return Results.Ok(ApiResponse<CupomAdminDto>.Ok(ToCupomAdminDto(cupom), "Cupom desativado no banco de dados."));
})
.WithName("DesativarCupomAdministracao")
;

app.MapGet("/api/clientes", [Authorize(Policy = "Gerente")] async (string? busca, NexumDbContext db, CancellationToken ct) =>
{
    var termo = busca?.Trim();
    if (!string.IsNullOrEmpty(termo) && termo.Length < 2)
    {
        return Results.BadRequest(ApiResponse<List<ClienteLojaDto>>.Erro(
            "Informe pelo menos dois caracteres para filtrar clientes."));
    }

    IQueryable<Cliente> query = db.Clientes.AsNoTracking();
    if (!string.IsNullOrEmpty(termo))
    {
        query = query.Where(cliente =>
            cliente.Nome.Contains(termo)
            || cliente.Email.Contains(termo)
            || (cliente.CpfCnpj != null && cliente.CpfCnpj.Contains(termo)));
    }

    var clientesDb = await query
        .OrderByDescending(cliente => cliente.CreatedAt)
        .Take(500)
        .Select(cliente => new
        {
            Cliente = cliente,
            RowVersion = EF.Property<byte[]>(cliente, "RowVersion")
        })
        .ToListAsync(ct);
    var clientes = clientesDb.Select(item => ToClienteLojaDto(item.Cliente, item.RowVersion)).ToList();

    var mensagem = string.IsNullOrEmpty(termo)
        ? "Clientes recentes carregados do banco oficial."
        : "Clientes filtrados no banco oficial.";
    return Results.Ok(ApiResponse<List<ClienteLojaDto>>.Ok(clientes, mensagem, clientes.Count));
})
.WithName("Clientes")
;

app.MapGet("/api/clientes/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var cliente = await LoadClienteDtoAsync(db, id, ct);
    return cliente is null
        ? Results.NotFound(ApiResponse<ClienteLojaDto>.Erro("Cliente nao encontrado para o tenant autenticado."))
        : Results.Ok(ApiResponse<ClienteLojaDto>.Ok(cliente, "Cliente relido do banco oficial."));
})
.WithName("ObterCliente")
;

app.MapPost("/api/clientes/admin", [Authorize(Policy = "Gerente")] async (
    ClienteRequest request,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var validationError = await ValidateClienteAdminRequestAsync(request, null, db, ct);
    if (validationError is not null)
    {
        return validationError;
    }

    var clienteId = 0;
    var strategy = db.Database.CreateExecutionStrategy();
    var transactionResult = await strategy.ExecuteAsync<IResult?>(async () =>
    {
        db.ChangeTracker.Clear();
        var email = NormalizeEmail(request.Email)!;
        var documento = NormalizeDocument(request.Cpf ?? request.CpfCnpj);
        if (await ClienteDuplicadoAsync(db, null, email, documento, ct))
        {
            return Results.Conflict(ApiResponse<ClienteLojaDto>.Erro("Cliente ja cadastrado com este e-mail ou CPF/CNPJ."));
        }

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var cliente = CreateClienteAdmin(request, email, documento);
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync(ct);
        clienteId = cliente.Id;

        var rowVersion = db.Entry(cliente).Property<byte[]>("RowVersion").CurrentValue ?? [];
        var created = ToClienteLojaDto(cliente, rowVersion);
        db.LogsAuditoria.Add(CreateIamAuditLog(
            principal,
            httpContext,
            "clientes",
            cliente.Id,
            AcaoAuditoria.INSERT,
            null,
            created));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return null;
    });
    if (transactionResult is not null)
    {
        return transactionResult;
    }

    var persisted = await LoadClienteDtoAsync(db, clienteId, ct);
    return persisted is null
        ? Results.Problem("O cliente foi gravado, mas a releitura de confirmacao falhou. Nao repita a operacao sem consultar o identificador retornado.", statusCode: StatusCodes.Status500InternalServerError)
        : Results.Created($"/api/clientes/{clienteId}", ApiResponse<ClienteLojaDto>.Ok(persisted, "Cliente cadastrado e relido do banco oficial."));
})
.WithName("CriarClienteAdministracao")
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
            var linkExistente = $"{baseUrlExistente}/confirmar-cadastro?token={Uri.EscapeDataString(clienteExistente.TokenConfirmacaoEmail)}";
            await notificacaoService.EnviarConfirmacaoCadastroAsync(clienteExistente, linkExistente, ct);
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
    var linkConfirmacao = $"{baseUrl}/confirmar-cadastro?token={Uri.EscapeDataString(cliente.TokenConfirmacaoEmail ?? string.Empty)}";
    await notificacaoService.EnviarConfirmacaoCadastroAsync(cliente, linkConfirmacao, ct);

    var dto = ToClienteLojaDto(cliente);
    return Results.Ok(ApiResponse<ClienteLojaDto>.Ok(dto, "Cliente registrado. Enviamos um link de confirmação por e-mail."));
})
.AllowAnonymous()
.WithName("CriarCliente")
;

app.MapPost("/api/clientes/reenviar-confirmacao", async (ReenviarConfirmacaoClienteRequest request, IConfiguration configuration, INotificacaoService notificacaoService, NexumDbContext db, CancellationToken ct) =>
{
    var email = NormalizeEmail(request.Email);
    if (string.IsNullOrWhiteSpace(email))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Email é obrigatório para reenviar a confirmação."));
    }

    var cliente = await db.Clientes.FirstOrDefaultAsync(item => item.Email == email, ct);
    if (cliente is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Cliente não encontrado para o email informado."));
    }

    if (cliente.Status == StatusCliente.Ativo && cliente.ConfirmadoEm is not null)
    {
        return Results.Ok(ApiResponse<ClienteLojaDto>.Ok(
            ToClienteLojaDto(cliente),
            "Cadastro já confirmado. Acesso liberado para a área do cliente."));
    }

    cliente.TokenConfirmacaoEmail = Guid.NewGuid().ToString("N");
    cliente.Status = StatusCliente.Pendente;
    cliente.UpdatedAt = DateTime.UtcNow;

    var baseUrl = configuration["PublicSite:BaseUrl"]?.TrimEnd('/') ?? "https://nexumaltivon.com.br";
    var linkConfirmacao = $"{baseUrl}/confirmar-cadastro?token={Uri.EscapeDataString(cliente.TokenConfirmacaoEmail)}";
    await notificacaoService.EnviarConfirmacaoCadastroAsync(cliente, linkConfirmacao, ct);
    await db.SaveChangesAsync(ct);

    return Results.Ok(ApiResponse<ClienteLojaDto>.Ok(
        ToClienteLojaDto(cliente),
        "Link de confirmação reenviado para o email cadastrado."));
})
.AllowAnonymous()
.WithName("ReenviarConfirmacaoCliente")
;

app.MapPut("/api/clientes/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    ClienteRequest request,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (!TryDecodeRowVersion(request.RowVersion, out var expectedRowVersion))
    {
        return Results.BadRequest(ApiResponse<ClienteLojaDto>.Erro("RowVersion valido e obrigatorio para atualizar o cliente."));
    }

    var validationError = await ValidateClienteAdminRequestAsync(request, id, db, ct);
    if (validationError is not null)
    {
        return validationError;
    }

    try
    {
        var strategy = db.Database.CreateExecutionStrategy();
        var transactionResult = await strategy.ExecuteAsync<IResult?>(async () =>
        {
            db.ChangeTracker.Clear();
            var cliente = await db.Clientes.FirstOrDefaultAsync(item => item.Id == id, ct);
            if (cliente is null)
            {
                return Results.NotFound(ApiResponse<ClienteLojaDto>.Erro("Cliente nao encontrado para o tenant autenticado."));
            }

            var currentRowVersion = db.Entry(cliente).Property<byte[]>("RowVersion").CurrentValue ?? [];
            if (!currentRowVersion.SequenceEqual(expectedRowVersion))
            {
                return Results.Conflict(ApiResponse<ClienteLojaDto>.Erro("O cliente foi alterado por outra sessao. Recarregue o cadastro antes de salvar."));
            }

            var email = NormalizeEmail(request.Email)!;
            var documento = NormalizeDocument(request.Cpf ?? request.CpfCnpj);
            if (await ClienteDuplicadoAsync(db, id, email, documento, ct))
            {
                return Results.Conflict(ApiResponse<ClienteLojaDto>.Erro("Cliente ja cadastrado com este e-mail ou CPF/CNPJ."));
            }

            var previous = ToClienteLojaDto(cliente, currentRowVersion);
            ApplyClienteAdminRequest(cliente, request, email, documento);
            db.Entry(cliente).Property<byte[]>("RowVersion").OriginalValue = expectedRowVersion;

            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            await db.SaveChangesAsync(ct);
            var updatedRowVersion = db.Entry(cliente).Property<byte[]>("RowVersion").CurrentValue ?? [];
            var updated = ToClienteLojaDto(cliente, updatedRowVersion);
            db.LogsAuditoria.Add(CreateIamAuditLog(
                principal,
                httpContext,
                "clientes",
                cliente.Id,
                AcaoAuditoria.UPDATE,
                previous,
                updated));
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return null;
        });
        if (transactionResult is not null)
        {
            return transactionResult;
        }
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict(ApiResponse<ClienteLojaDto>.Erro("O cliente foi alterado por outra sessao. Recarregue o cadastro antes de salvar."));
    }

    var persisted = await LoadClienteDtoAsync(db, id, ct);
    return persisted is null
        ? Results.Problem("O cliente foi atualizado, mas a releitura de confirmacao falhou. Nao repita a operacao sem consultar o identificador.", statusCode: StatusCodes.Status500InternalServerError)
        : Results.Ok(ApiResponse<ClienteLojaDto>.Ok(persisted, "Cliente atualizado e relido do banco oficial."));
})
.WithName("AtualizarCliente")
;

app.MapGet("/api/clientes/{id:int}/enderecos", [Authorize(Policy = "Gerente")] async (
    int id,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (!await db.Clientes.AsNoTracking().AnyAsync(cliente => cliente.Id == id, ct))
    {
        return Results.NotFound(ApiResponse<List<ClientePortalEnderecoDto>>.Erro("Cliente nao encontrado para o tenant autenticado."));
    }

    var rows = await db.Enderecos
        .AsNoTracking()
        .Where(endereco => endereco.ClienteId == id)
        .OrderByDescending(endereco => endereco.Padrao)
        .ThenBy(endereco => endereco.Apelido)
        .ThenBy(endereco => endereco.Id)
        .Select(endereco => new
        {
            Endereco = endereco,
            RowVersion = EF.Property<byte[]>(endereco, "RowVersion")
        })
        .ToListAsync(ct);
    var enderecos = rows.Select(row => ToClientePortalEnderecoDto(row.Endereco, row.RowVersion)).ToList();
    return Results.Ok(ApiResponse<List<ClientePortalEnderecoDto>>.Ok(
        enderecos,
        "Enderecos do cliente carregados do banco oficial.",
        enderecos.Count));
})
.WithName("ListarEnderecosClienteAdministracao")
;

app.MapPost("/api/clientes/{id:int}/enderecos", [Authorize(Policy = "Gerente")] async (
    int id,
    ClientePortalEnderecoRequest request,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var validationError = ValidateClienteEnderecoAdminRequest(request);
    if (validationError is not null)
    {
        return Results.BadRequest(ApiResponse<ClientePortalEnderecoDto>.Erro(validationError));
    }

    var enderecoId = 0;
    var strategy = db.Database.CreateExecutionStrategy();
    var transactionResult = await strategy.ExecuteAsync<IResult?>(async () =>
    {
        db.ChangeTracker.Clear();
        if (!await db.Clientes.AnyAsync(cliente => cliente.Id == id, ct))
        {
            return Results.NotFound(ApiResponse<ClientePortalEnderecoDto>.Erro("Cliente nao encontrado para o tenant autenticado."));
        }

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var outrosPrincipais = request.Padrao
            ? await db.Enderecos.Where(endereco => endereco.ClienteId == id && endereco.Padrao).ToListAsync(ct)
            : [];
        var anteriores = outrosPrincipais.ToDictionary(
            endereco => endereco.Id,
            endereco => ToClientePortalEnderecoDto(
                endereco,
                db.Entry(endereco).Property<byte[]>("RowVersion").CurrentValue ?? []));
        foreach (var endereco in outrosPrincipais)
        {
            endereco.Padrao = false;
            endereco.UpdatedAt = DateTime.UtcNow;
        }

        var novo = new Endereco
        {
            ClienteId = id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        ApplyClienteEnderecoAdminRequest(novo, request);
        db.Enderecos.Add(novo);
        await db.SaveChangesAsync(ct);
        enderecoId = novo.Id;

        var created = ToClientePortalEnderecoDto(
            novo,
            db.Entry(novo).Property<byte[]>("RowVersion").CurrentValue ?? []);
        db.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "enderecos", novo.Id, AcaoAuditoria.INSERT, null, created));
        foreach (var endereco in outrosPrincipais)
        {
            var updated = ToClientePortalEnderecoDto(
                endereco,
                db.Entry(endereco).Property<byte[]>("RowVersion").CurrentValue ?? []);
            db.LogsAuditoria.Add(CreateIamAuditLog(
                principal,
                httpContext,
                "enderecos",
                endereco.Id,
                AcaoAuditoria.UPDATE,
                anteriores[endereco.Id],
                updated));
        }

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return null;
    });
    if (transactionResult is not null)
    {
        return transactionResult;
    }

    var persisted = await LoadClienteEnderecoDtoAsync(db, id, enderecoId, ct);
    return persisted is null
        ? Results.Problem("O endereco foi gravado, mas a releitura de confirmacao falhou. Nao repita a operacao sem consultar o identificador.", statusCode: StatusCodes.Status500InternalServerError)
        : Results.Created($"/api/clientes/{id}/enderecos/{enderecoId}", ApiResponse<ClientePortalEnderecoDto>.Ok(persisted, "Endereco cadastrado e relido do banco oficial."));
})
.WithName("CriarEnderecoClienteAdministracao")
;

app.MapPut("/api/clientes/{id:int}/enderecos/{enderecoId:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    int enderecoId,
    ClientePortalEnderecoRequest request,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (!TryDecodeRowVersion(request.RowVersion, out var expectedRowVersion))
    {
        return Results.BadRequest(ApiResponse<ClientePortalEnderecoDto>.Erro("RowVersion valido e obrigatorio para atualizar o endereco."));
    }

    var validationError = ValidateClienteEnderecoAdminRequest(request);
    if (validationError is not null)
    {
        return Results.BadRequest(ApiResponse<ClientePortalEnderecoDto>.Erro(validationError));
    }

    try
    {
        var strategy = db.Database.CreateExecutionStrategy();
        var transactionResult = await strategy.ExecuteAsync<IResult?>(async () =>
        {
            db.ChangeTracker.Clear();
            var endereco = await db.Enderecos.FirstOrDefaultAsync(
                item => item.Id == enderecoId && item.ClienteId == id,
                ct);
            if (endereco is null)
            {
                return Results.NotFound(ApiResponse<ClientePortalEnderecoDto>.Erro("Endereco nao encontrado para o cliente e tenant autenticados."));
            }

            var currentRowVersion = db.Entry(endereco).Property<byte[]>("RowVersion").CurrentValue ?? [];
            if (!currentRowVersion.SequenceEqual(expectedRowVersion))
            {
                return Results.Conflict(ApiResponse<ClientePortalEnderecoDto>.Erro("O endereco foi alterado por outra sessao. Recarregue antes de salvar."));
            }

            var previous = ToClientePortalEnderecoDto(endereco, currentRowVersion);
            var outrosPrincipais = request.Padrao
                ? await db.Enderecos.Where(item => item.ClienteId == id && item.Id != enderecoId && item.Padrao).ToListAsync(ct)
                : [];
            var anteriores = outrosPrincipais.ToDictionary(
                item => item.Id,
                item => ToClientePortalEnderecoDto(
                    item,
                    db.Entry(item).Property<byte[]>("RowVersion").CurrentValue ?? []));
            foreach (var outro in outrosPrincipais)
            {
                outro.Padrao = false;
                outro.UpdatedAt = DateTime.UtcNow;
            }

            ApplyClienteEnderecoAdminRequest(endereco, request);
            db.Entry(endereco).Property<byte[]>("RowVersion").OriginalValue = expectedRowVersion;

            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            await db.SaveChangesAsync(ct);
            var updated = ToClientePortalEnderecoDto(
                endereco,
                db.Entry(endereco).Property<byte[]>("RowVersion").CurrentValue ?? []);
            db.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "enderecos", endereco.Id, AcaoAuditoria.UPDATE, previous, updated));
            foreach (var outro in outrosPrincipais)
            {
                var otherUpdated = ToClientePortalEnderecoDto(
                    outro,
                    db.Entry(outro).Property<byte[]>("RowVersion").CurrentValue ?? []);
                db.LogsAuditoria.Add(CreateIamAuditLog(
                    principal,
                    httpContext,
                    "enderecos",
                    outro.Id,
                    AcaoAuditoria.UPDATE,
                    anteriores[outro.Id],
                    otherUpdated));
            }

            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return null;
        });
        if (transactionResult is not null)
        {
            return transactionResult;
        }
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict(ApiResponse<ClientePortalEnderecoDto>.Erro("O endereco foi alterado por outra sessao. Recarregue antes de salvar."));
    }

    var persisted = await LoadClienteEnderecoDtoAsync(db, id, enderecoId, ct);
    return persisted is null
        ? Results.Problem("O endereco foi atualizado, mas a releitura de confirmacao falhou. Nao repita a operacao sem consultar o identificador.", statusCode: StatusCodes.Status500InternalServerError)
        : Results.Ok(ApiResponse<ClientePortalEnderecoDto>.Ok(persisted, "Endereco atualizado e relido do banco oficial."));
})
.WithName("AtualizarEnderecoClienteAdministracao")
;

app.MapDelete("/api/clientes/{id:int}/enderecos/{enderecoId:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    int enderecoId,
    string? rowVersion,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (!TryDecodeRowVersion(rowVersion, out var expectedRowVersion))
    {
        return Results.BadRequest(ApiResponse<ClientePortalEnderecoDto>.Erro("RowVersion valido e obrigatorio para arquivar o endereco."));
    }

    try
    {
        var strategy = db.Database.CreateExecutionStrategy();
        var transactionResult = await strategy.ExecuteAsync<IResult?>(async () =>
        {
            db.ChangeTracker.Clear();
            var endereco = await db.Enderecos.FirstOrDefaultAsync(
                item => item.Id == enderecoId && item.ClienteId == id,
                ct);
            if (endereco is null)
            {
                return Results.NotFound(ApiResponse<ClientePortalEnderecoDto>.Erro("Endereco nao encontrado para o cliente e tenant autenticados."));
            }

            var currentRowVersion = db.Entry(endereco).Property<byte[]>("RowVersion").CurrentValue ?? [];
            if (!currentRowVersion.SequenceEqual(expectedRowVersion))
            {
                return Results.Conflict(ApiResponse<ClientePortalEnderecoDto>.Erro("O endereco foi alterado por outra sessao. Recarregue antes de arquivar."));
            }

            var previous = ToClientePortalEnderecoDto(endereco, currentRowVersion);
            db.Entry(endereco).Property<byte[]>("RowVersion").OriginalValue = expectedRowVersion;
            db.Enderecos.Remove(endereco);

            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            await db.SaveChangesAsync(ct);
            db.LogsAuditoria.Add(CreateIamAuditLog(principal, httpContext, "enderecos", endereco.Id, AcaoAuditoria.DELETE, previous, null));
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return null;
        });
        if (transactionResult is not null)
        {
            return transactionResult;
        }
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict(ApiResponse<ClientePortalEnderecoDto>.Erro("O endereco foi alterado por outra sessao. Recarregue antes de arquivar."));
    }

    return await LoadClienteEnderecoDtoAsync(db, id, enderecoId, ct) is null
        ? Results.NoContent()
        : Results.Problem("O endereco foi arquivado, mas permaneceu visivel na releitura. Consulte o registro antes de repetir a operacao.", statusCode: StatusCodes.Status500InternalServerError);
})
.WithName("ArquivarEnderecoClienteAdministracao")
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
            item.Padrao,
            null))
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
        .OrderByDescending(fornecedor => fornecedor.UpdatedAt)
        .Take(500)
        .Select(fornecedor => new
        {
            Fornecedor = fornecedor,
            RowVersion = EF.Property<byte[]>(fornecedor, "RowVersion")
        })
        .ToListAsync(ct);
    var fornecedores = fornecedoresDb
        .Select(item => ToFornecedorDto(item.Fornecedor, item.RowVersion))
        .ToList();

    return Results.Ok(ApiResponse<List<FornecedorDto>>.Ok(fornecedores, "Fornecedores carregados do banco oficial.", fornecedores.Count));
})
.WithName("Fornecedores")
;

app.MapGet("/api/fornecedores/{id:int}", [Authorize(Policy = "Gerente")] async (int id, NexumDbContext db, CancellationToken ct) =>
{
    var fornecedor = await LoadFornecedorDtoAsync(db, id, ct);
    return fornecedor is null
        ? Results.NotFound(ApiResponse<FornecedorDto>.Erro("Fornecedor nao encontrado."))
        : Results.Ok(ApiResponse<FornecedorDto>.Ok(fornecedor, "Fornecedor relido do banco oficial."));
})
.WithName("ObterFornecedor")
;

app.MapPost("/api/fornecedores", [Authorize(Policy = "Gerente")] async (
    FornecedorRequest request,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var validationError = await ValidateFornecedorRequestAsync(request, null, db, ct);
    if (validationError is not null)
    {
        return validationError;
    }

    var executionStrategy = db.Database.CreateExecutionStrategy();
    var fornecedorId = 0;
    var transactionResult = await executionStrategy.ExecuteAsync<IResult?>(async () =>
    {
        db.ChangeTracker.Clear();
        var documento = NormalizeDocument(request.Documento);
        var email = NormalizeEmail(request.Email);
        if (await FornecedorDuplicadoAsync(db, null, documento, email, ct))
        {
            return Results.Conflict(ApiResponse<FornecedorDto>.Erro("Fornecedor ja cadastrado com este documento ou e-mail."));
        }

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var fornecedor = CreateFornecedor(request, documento, email);
        db.Fornecedores.Add(fornecedor);
        await db.SaveChangesAsync(ct);
        fornecedorId = fornecedor.Id;

        var rowVersion = db.Entry(fornecedor).Property<byte[]>("RowVersion").CurrentValue ?? [];
        var created = ToFornecedorDto(fornecedor, rowVersion);
        db.LogsAuditoria.Add(CreateIamAuditLog(
            principal,
            httpContext,
            "fornecedores",
            fornecedor.Id,
            AcaoAuditoria.INSERT,
            null,
            created));
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return null;
    });
    if (transactionResult is not null)
    {
        return transactionResult;
    }

    var persisted = await LoadFornecedorDtoAsync(db, fornecedorId, ct);
    return persisted is null
        ? Results.Problem("O fornecedor foi gravado, mas a releitura de confirmacao falhou. Nao repita a operacao sem consultar o identificador retornado.", statusCode: StatusCodes.Status500InternalServerError)
        : Results.Created($"/api/fornecedores/{fornecedorId}", ApiResponse<FornecedorDto>.Ok(persisted, "Fornecedor cadastrado e relido do banco oficial."));
})
.WithName("CriarFornecedor")
;

app.MapPut("/api/fornecedores/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    FornecedorRequest request,
    ClaimsPrincipal principal,
    HttpContext httpContext,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (!TryDecodeRowVersion(request.RowVersion, out var expectedRowVersion))
    {
        return Results.BadRequest(ApiResponse<FornecedorDto>.Erro("RowVersion valido e obrigatorio para atualizar o fornecedor."));
    }

    var validationError = await ValidateFornecedorRequestAsync(request, id, db, ct);
    if (validationError is not null)
    {
        return validationError;
    }

    try
    {
        var executionStrategy = db.Database.CreateExecutionStrategy();
        var transactionResult = await executionStrategy.ExecuteAsync<IResult?>(async () =>
        {
            db.ChangeTracker.Clear();
            var fornecedor = await db.Fornecedores.FirstOrDefaultAsync(item => item.Id == id, ct);
            if (fornecedor is null)
            {
                return Results.NotFound(ApiResponse<FornecedorDto>.Erro("Fornecedor nao encontrado."));
            }

            var currentRowVersion = db.Entry(fornecedor).Property<byte[]>("RowVersion").CurrentValue ?? [];
            if (!currentRowVersion.SequenceEqual(expectedRowVersion))
            {
                return Results.Conflict(ApiResponse<FornecedorDto>.Erro("O fornecedor foi alterado por outra sessao. Recarregue o cadastro antes de salvar."));
            }

            var documento = NormalizeDocument(request.Documento);
            var email = NormalizeEmail(request.Email);
            if (await FornecedorDuplicadoAsync(db, id, documento, email, ct))
            {
                return Results.Conflict(ApiResponse<FornecedorDto>.Erro("Fornecedor ja cadastrado com este documento ou e-mail."));
            }

            var previous = ToFornecedorDto(fornecedor, currentRowVersion);
            ApplyFornecedorRequest(fornecedor, request, documento, email);
            db.Entry(fornecedor).Property<byte[]>("RowVersion").OriginalValue = expectedRowVersion;

            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            await db.SaveChangesAsync(ct);
            var updatedRowVersion = db.Entry(fornecedor).Property<byte[]>("RowVersion").CurrentValue ?? [];
            var updated = ToFornecedorDto(fornecedor, updatedRowVersion);
            db.LogsAuditoria.Add(CreateIamAuditLog(
                principal,
                httpContext,
                "fornecedores",
                fornecedor.Id,
                AcaoAuditoria.UPDATE,
                previous,
                updated));
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return null;
        });
        if (transactionResult is not null)
        {
            return transactionResult;
        }
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict(ApiResponse<FornecedorDto>.Erro("O fornecedor foi alterado por outra sessao. Recarregue o cadastro antes de salvar."));
    }

    var persisted = await LoadFornecedorDtoAsync(db, id, ct);
    return persisted is null
        ? Results.Problem("O fornecedor foi atualizado, mas a releitura de confirmacao falhou. Consulte o cadastro antes de repetir a operacao.", statusCode: StatusCodes.Status500InternalServerError)
        : Results.Ok(ApiResponse<FornecedorDto>.Ok(persisted, "Fornecedor atualizado e relido do banco oficial."));
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

app.MapPost("/api/compras/solicitacoes", [Authorize(Policy = "Gerente")] async (CompraSolicitacaoRequest request, NexumDbContext db, HttpContext http, CancellationToken ct) =>
{
    var executionStrategy = db.Database.CreateExecutionStrategy();
    var solicitacaoId = 0;
    var transactionResult = await executionStrategy.ExecuteAsync<IResult?>(async () =>
    {
    db.ChangeTracker.Clear();
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
    var tenantId = db.CurrentTenantId.ToString();
    var userId = db.CurrentUserId?.ToString();

    await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
    await db.Database.ExecuteSqlInterpolatedAsync($"""
        INSERT INTO compras_solicitacoes
            (produto_id, produto_nome, quantidade_solicitada, finalidade, origem, status, prioridade, observacoes,
             tenant_id, row_version, created_by_user_id, updated_by_user_id, created_at, updated_at)
        VALUES
            ({produto?.Id}, {produtoNome}, {request.Quantidade}, {finalidade}, {origem}, {"Aberta"}, {prioridade}, {observacoes},
             {tenantId}, UNHEX(REPLACE(UUID(), '-', '')), {userId}, {userId}, {now}, {now});
        """, ct);

    solicitacaoId = await ExecuteScalarAsync<int>(db, "SELECT LAST_INSERT_ID();", ct);
    db.LogsAuditoria.Add(CreatePurchaseAudit(
        http,
        "compras_solicitacoes",
        solicitacaoId,
        AcaoAuditoria.INSERT,
        null,
        new { produtoId = produto?.Id, produtoNome, request.Quantidade, finalidade, origem, status = "Aberta", prioridade }));
    await db.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);
    return null;
    });
    if (transactionResult is not null)
    {
        return transactionResult;
    }

    var painel = await BuildComprasPainelAsync(db, ct);
    return Results.Created($"/api/compras/solicitacoes/{solicitacaoId}", ApiResponse<ComprasPainelDto>.Ok(painel, "Solicitacao de compra registrada para cotacao."));
})
.WithName("RegistrarCompraSolicitacao")
;

app.MapPost("/api/compras/cotacoes", [Authorize(Policy = "Gerente")] async (CompraCotacaoRequest request, NexumDbContext db, HttpContext http, CancellationToken ct) =>
{
    var executionStrategy = db.Database.CreateExecutionStrategy();
    var cotacaoId = 0;
    var transactionResult = await executionStrategy.ExecuteAsync<IResult?>(async () =>
    {
    db.ChangeTracker.Clear();
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
    var tenantId = db.CurrentTenantId.ToString();
    var userId = db.CurrentUserId?.ToString();
    var solicitacaoId = request.SolicitacaoId;

    await using var transaction = await db.Database.BeginTransactionAsync(ct);

    if (solicitacaoId.HasValue)
    {
        var existingRequestStatus = await ExecuteScalarAsync<string>(
            db,
            "SELECT status FROM compras_solicitacoes WHERE id = @id AND tenant_id = @tenantId AND is_deleted = 0 LIMIT 1 FOR UPDATE",
            ct,
            ("@id", solicitacaoId.Value),
            ("@tenantId", tenantId));
        if (string.IsNullOrWhiteSpace(existingRequestStatus))
        {
            await transaction.RollbackAsync(ct);
            return Results.NotFound(ApiResponse<string>.Erro("Solicitacao de compra nao encontrada para o tenant autenticado."));
        }

        if (!IsAllowedCompraSolicitacaoTransition(existingRequestStatus, "Cotado"))
        {
            await transaction.RollbackAsync(ct);
            return Results.Conflict(ApiResponse<string>.Erro($"Solicitacao em status {existingRequestStatus} nao pode receber nova cotacao."));
        }

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE compras_solicitacoes
            SET status = {"Cotado"},
                produto_id = COALESCE(produto_id, {produtoIdCotacao}),
                produto_nome = {produtoNome},
                quantidade_solicitada = {request.Quantidade},
                finalidade = {finalidade},
                origem = {origem},
                prioridade = {prioridade},
                observacoes = {observacoesCotacao},
                updated_by_user_id = {userId},
                row_version = UNHEX(REPLACE(UUID(), '-', '')),
                updated_at = {now}
            WHERE id = {solicitacaoId.Value} AND tenant_id = {tenantId} AND is_deleted = 0;
            """, ct);
    }
    else
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO compras_solicitacoes
                (produto_id, produto_nome, quantidade_solicitada, finalidade, origem, status, prioridade, observacoes,
                 tenant_id, row_version, created_by_user_id, updated_by_user_id, created_at, updated_at)
            VALUES
                ({produtoIdCotacao}, {produtoNome}, {request.Quantidade}, {finalidade}, {origem}, {"Cotado"}, {prioridade}, {observacoesCotacao},
                 {tenantId}, UNHEX(REPLACE(UUID(), '-', '')), {userId}, {userId}, {now}, {now});
            """, ct);

        solicitacaoId = await ExecuteScalarAsync<int>(db, "SELECT LAST_INSERT_ID();", ct);
    }

    await db.Database.ExecuteSqlInterpolatedAsync($"""
        INSERT INTO compras_cotacoes
            (solicitacao_id, fornecedor_id, produto_id, produto_nome, quantidade, custo_unitario, valor_total,
             prazo_entrega_dias, origem, status, observacoes, tenant_id, row_version, created_by_user_id,
             updated_by_user_id, created_at, updated_at)
        VALUES
            ({solicitacaoId.Value}, {request.FornecedorId}, {produtoIdCotacao}, {produtoNome}, {request.Quantidade},
             {request.CustoUnitario}, {total}, {prazoEntrega}, {origem}, {"Selecionada"}, {observacoesCotacao},
             {tenantId}, UNHEX(REPLACE(UUID(), '-', '')), {userId}, {userId}, {now}, {now});
        """, ct);

    cotacaoId = await ExecuteScalarAsync<int>(db, "SELECT LAST_INSERT_ID();", ct);
    db.LogsAuditoria.Add(CreatePurchaseAudit(
        http,
        "compras_cotacoes",
        cotacaoId,
        AcaoAuditoria.INSERT,
        null,
        new { solicitacaoId, request.FornecedorId, produtoIdCotacao, produtoNome, request.Quantidade, request.CustoUnitario, total, prazoEntrega, status = "Selecionada" }));
    db.LogsAuditoria.Add(CreatePurchaseAudit(
        http,
        "compras_solicitacoes",
        solicitacaoId.Value,
        AcaoAuditoria.UPDATE,
        null,
        new { status = "Cotado", cotacaoId }));
    await db.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);
    return null;
    });
    if (transactionResult is not null)
    {
        return transactionResult;
    }

    var painel = await BuildComprasPainelAsync(db, ct);
    return Results.Created($"/api/compras/cotacoes/{cotacaoId}", ApiResponse<ComprasPainelDto>.Ok(painel, "Cotacao registrada e disponivel para pedido de compra."));
})
.WithName("RegistrarCompraCotacao")
;

app.MapPost("/api/compras/pedidos", [Authorize(Policy = "Gerente")] async (
    CompraPedidoRequest request,
    NexumDbContext db,
    IServiceProvider services,
    HttpContext http,
    ILoggerFactory loggerFactory,
    CancellationToken ct) =>
{
    var executionStrategy = db.Database.CreateExecutionStrategy();
    var compraPedidoId = 0;
    var numeroPedido = string.Empty;
    var fornecedorIdIntegracao = 0;
    var fornecedorNomeIntegracao = string.Empty;
    var totalCompra = 0m;
    var emissaoCompra = default(DateTime);
    var vencimentoCompra = default(DateTime);
    var transactionResult = await executionStrategy.ExecuteAsync<IResult?>(async () =>
    {
    db.ChangeTracker.Clear();
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
    fornecedorIdIntegracao = fornecedor.Id;
    fornecedorNomeIntegracao = fornecedor.RazaoSocial;

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

    await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

    if (request.SolicitacaoId.HasValue)
    {
        var solicitacaoStatus = await ExecuteScalarAsync<string>(
            db,
            "SELECT status FROM compras_solicitacoes WHERE id = @id AND tenant_id = @tenantId AND is_deleted = 0 LIMIT 1 FOR UPDATE",
            ct,
            ("@id", request.SolicitacaoId.Value),
            ("@tenantId", db.CurrentTenantId.ToString()));
        if (string.IsNullOrWhiteSpace(solicitacaoStatus))
        {
            return Results.NotFound(ApiResponse<string>.Erro("Solicitacao de compra nao encontrada para o tenant autenticado."));
        }

        if (!solicitacaoStatus.Equals("Aprovada", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Conflict(ApiResponse<string>.Erro($"Solicitacao em status {solicitacaoStatus} deve ser aprovada antes de gerar o pedido de compra."));
        }
    }

    emissaoCompra = DateTime.UtcNow;
    var now = emissaoCompra;
    numeroPedido = $"COMP-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..28];
    var origem = NormalizeCompraOrigem(request.Origem);
    var finalidade = TrimOrNull(request.Finalidade) ?? "Reposicao/operacao";
    vencimentoCompra = request.DataVencimento ?? now.Date.AddDays(7);
    var vencimento = vencimentoCompra;
    totalCompra = request.Itens.Sum(item => item.Quantidade * item.CustoUnitario);
    var total = totalCompra;
    var tenantId = db.CurrentTenantId.ToString();
    var userId = db.CurrentUserId?.ToString();

    await db.Database.ExecuteSqlInterpolatedAsync($"""
        INSERT INTO compras_pedidos
            (numero, fornecedor_id, solicitacao_id, origem, finalidade, status, status_fiscal, valor_total,
             data_prevista_entrega, observacoes, tenant_id, row_version, created_by_user_id,
             updated_by_user_id, created_at, updated_at)
        VALUES
            ({numeroPedido}, {request.FornecedorId}, {request.SolicitacaoId}, {origem}, {finalidade}, {"Aberto"},
             {"Pendente"}, {total}, {request.DataPrevistaEntrega}, {TrimOrNull(request.Observacoes)}, {tenantId},
             UNHEX(REPLACE(UUID(), '-', '')), {userId}, {userId}, {now}, {now});
        """, ct);

    compraPedidoId = await ExecuteScalarAsync<int>(db, "SELECT LAST_INSERT_ID();", ct);

    foreach (var item in request.Itens)
    {
        var produto = item.ProdutoId.HasValue ? produtos[item.ProdutoId.Value] : null;
        var produtoNome = produto?.Nome ?? TrimOrNull(item.ProdutoNome);
        int? itemProdutoId = produto?.Id;
        var itemSku = produto?.Sku ?? TrimOrNull(item.Sku);
        if (string.IsNullOrWhiteSpace(produtoNome))
        {
            await transaction.RollbackAsync(ct);
            return Results.BadRequest(ApiResponse<string>.Erro("Cada item sem produto vinculado precisa de descricao."));
        }

        var itemTotal = item.Quantidade * item.CustoUnitario;
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO compras_pedido_itens
                (compra_pedido_id, produto_id, produto_nome, sku, quantidade, quantidade_recebida,
                 custo_unitario, valor_total, origem, finalidade, tenant_id, row_version,
                 created_by_user_id, updated_by_user_id, created_at, updated_at)
            VALUES
                ({compraPedidoId}, {itemProdutoId}, {produtoNome}, {itemSku}, {item.Quantidade}, 0,
                 {item.CustoUnitario}, {itemTotal}, {origem}, {finalidade}, {tenantId},
                 UNHEX(REPLACE(UUID(), '-', '')), {userId}, {userId}, {now}, {now});
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

    if (request.SolicitacaoId.HasValue)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE compras_solicitacoes
            SET status = {"Atendida"},
                updated_by_user_id = {userId},
                row_version = UNHEX(REPLACE(UUID(), '-', '')),
                updated_at = {now}
            WHERE id = {request.SolicitacaoId.Value} AND tenant_id = {tenantId} AND is_deleted = 0;
            """, ct);
    }

    db.LogsAuditoria.Add(CreatePurchaseAudit(
        http,
        "compras_pedidos",
        compraPedidoId,
        AcaoAuditoria.INSERT,
        null,
        new { numeroPedido, request.FornecedorId, request.SolicitacaoId, origem, finalidade, status = "Aberto", total, vencimento }));
    if (request.SolicitacaoId.HasValue)
    {
        db.LogsAuditoria.Add(CreatePurchaseAudit(
            http,
            "compras_solicitacoes",
            request.SolicitacaoId.Value,
            AcaoAuditoria.UPDATE,
            new { status = "Aprovada" },
            new { status = "Atendida", compraPedidoId }));
    }

    await db.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);
    return null;
    });
    if (transactionResult is not null)
    {
        return transactionResult;
    }

    var genesisResult = await TryCreateGenesisContaPagarCompraAsync(
        services,
        numeroPedido,
        fornecedorIdIntegracao,
        fornecedorNomeIntegracao,
        totalCompra,
        emissaoCompra,
        vencimentoCompra,
        request.MeioPagamento,
        ct);

    var painel = await BuildComprasPainelAsync(db, ct);
    if (!genesisResult.Succeeded)
    {
        var logger = loggerFactory.CreateLogger("NexumAltivon.Compras");
        logger.LogError(
            "Conta a pagar Genesis nao foi confirmada para o pedido de compra {CompraPedidoId} ({NumeroPedido}). Codigo {FailureCode}.",
            compraPedidoId,
            numeroPedido,
            genesisResult.FailureCode);

        try
        {
            using var incidentTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var incidentReference = await RegisterPurchasePostCommitIncidentAsync(
                services,
                compraPedidoId,
                numeroPedido,
                genesisResult.FailureCode ?? "GENESIS-UNKNOWN",
                http,
                incidentTimeout.Token);
            return Results.Accepted(
                $"/api/compras/pedidos/{compraPedidoId}",
                ApiResponse<ComprasPainelDto>.Ok(
                    painel,
                    $"Pedido de compra persistido. A integracao da conta a pagar Genesis requer conciliacao sob a referencia {incidentReference}."));
        }
        catch (Exception incidentException)
        {
            logger.LogCritical(
                incidentException,
                "Pedido de compra {CompraPedidoId} ({NumeroPedido}) foi commitado, mas o incidente Genesis nao foi persistido.",
                compraPedidoId,
                numeroPedido);
            return Results.Problem(
                detail: $"O pedido de compra {numeroPedido} foi registrado, mas a integracao financeira e o incidente falharam. Nao repita a operacao; informe este numero ao suporte.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Pedido de compra registrado com falha operacional critica");
        }
    }

    return Results.Created($"/api/compras/pedidos/{compraPedidoId}", ApiResponse<ComprasPainelDto>.Ok(painel, "Pedido de compra gerado com conta a pagar."));
})
.WithName("CriarCompraPedido")
;

app.MapPost("/api/compras/pedidos/{id:int}/entradas", [Authorize(Policy = "Gerente")] async (int id, CompraEntradaRequest request, NexumDbContext db, HttpContext http, CancellationToken ct) =>
{
    var executionStrategy = db.Database.CreateExecutionStrategy();
    var transactionResult = await executionStrategy.ExecuteAsync<IResult?>(async () =>
    {
    db.ChangeTracker.Clear();
    var tenantId = db.CurrentTenantId.ToString();
    var userId = db.CurrentUserId?.ToString();
    await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
    var pedidoRows = await db.Database.SqlQueryRaw<CompraPedidoLookupRow>(
        "SELECT id AS Id, numero AS Numero, status AS Status, origem AS Origem, fornecedor_id AS FornecedorId FROM compras_pedidos WHERE id = {0} AND tenant_id = {1} AND is_deleted = 0 LIMIT 1 FOR UPDATE",
        id,
        tenantId)
        .ToListAsync(ct);
    var pedidoAtual = pedidoRows.FirstOrDefault();

    if (pedidoAtual is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Pedido de compra nao encontrado."));
    }

    if (!pedidoAtual.Status.Equals("Aprovado", StringComparison.OrdinalIgnoreCase) &&
        !pedidoAtual.Status.Equals("RecebidoParcial", StringComparison.OrdinalIgnoreCase))
    {
        return Results.Conflict(ApiResponse<string>.Erro($"Pedido em status {pedidoAtual.Status} nao permite entrada. Aprove o pedido antes do recebimento."));
    }

    var itens = await db.Database.SqlQueryRaw<CompraPedidoItemLookupRow>(
        "SELECT id AS Id, produto_id AS ProdutoId, produto_nome AS ProdutoNome, quantidade AS Quantidade, quantidade_recebida AS QuantidadeRecebida, custo_unitario AS CustoUnitario FROM compras_pedido_itens WHERE compra_pedido_id = {0} AND tenant_id = {1} AND is_deleted = 0 FOR UPDATE",
        id,
        tenantId)
        .ToListAsync(ct);

    if (itens.Count == 0)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Pedido de compra sem itens para entrada."));
    }

    var now = DateTime.UtcNow;
    var documento = TrimOrNull(request.NumeroDocumento);
    var chaveNfe = NormalizeDocument(request.ChaveNfeEntrada);
    if (documento is null && chaveNfe is null)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Informe o numero do documento do fornecedor ou a chave da NF-e de entrada."));
    }

    if (chaveNfe is not null && chaveNfe.Length != 44)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Chave da NF-e de entrada deve conter 44 digitos."));
    }

    var requestedItems = request.Itens ?? [];
    if (requestedItems.Any(item => item.ItemId <= 0 || item.QuantidadeRecebida <= 0))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Itens de entrada devem informar item e quantidade recebida maiores que zero."));
    }

    if (requestedItems.GroupBy(item => item.ItemId).Any(group => group.Count() > 1))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("O mesmo item de pedido nao pode ser informado mais de uma vez na entrada."));
    }

    var knownItemIds = itens.Select(item => item.Id).ToHashSet();
    if (requestedItems.Any(item => !knownItemIds.Contains(item.ItemId)))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("A entrada contem item que nao pertence ao pedido autenticado."));
    }

    var hasExplicitItems = request.Itens is not null;
    var quantidades = requestedItems.ToDictionary(item => item.ItemId, item => item.QuantidadeRecebida);
    var itensPorId = itens.ToDictionary(item => item.Id);
    if (hasExplicitItems && requestedItems.Any(requested =>
            requested.QuantidadeRecebida > Math.Max(
                0,
                itensPorId[requested.ItemId].Quantidade - itensPorId[requested.ItemId].QuantidadeRecebida)))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Quantidade recebida nao pode superar o saldo pendente do item."));
    }

    var hasReceivableQuantity = itens.Any(item =>
    {
        var pendente = Math.Max(0, item.Quantidade - item.QuantidadeRecebida);
        var requested = hasExplicitItems ? quantidades.GetValueOrDefault(item.Id) : pendente;
        return Math.Min(Math.Max(0, requested), pendente) > 0;
    });
    if (!hasReceivableQuantity)
    {
        return Results.Conflict(ApiResponse<string>.Erro("Pedido sem quantidade pendente para a entrada informada."));
    }

    var duplicateEntry = await ExecuteScalarAsync<int>(
        db,
        """
        SELECT COUNT(*)
        FROM compras_entradas
        WHERE compra_pedido_id = @pedidoId
          AND tenant_id = @tenantId
          AND is_deleted = 0
          AND ((@documento IS NOT NULL AND numero_documento = @documento)
            OR (@chaveNfe IS NOT NULL AND chave_nfe_entrada = @chaveNfe))
        """,
        ct,
        ("@pedidoId", id),
        ("@tenantId", tenantId),
        ("@documento", documento),
        ("@chaveNfe", chaveNfe));
    if (duplicateEntry > 0)
    {
        return Results.Conflict(ApiResponse<string>.Erro("Documento de entrada ja registrado para este pedido."));
    }

    var totalEntrada = 0m;

    await db.Database.ExecuteSqlInterpolatedAsync($"""
        INSERT INTO compras_entradas
            (compra_pedido_id, fornecedor_id, numero_documento, chave_nfe_entrada, tipo_entrada,
             status_fiscal, valor_total, recebido_por, observacoes, tenant_id, row_version,
             created_by_user_id, updated_by_user_id, created_at, updated_at)
        VALUES
            ({id}, {pedidoAtual.FornecedorId}, {documento}, {chaveNfe},
              {NormalizeCompraOrigem(request.TipoEntrada ?? pedidoAtual.Origem)},
              {(!string.IsNullOrWhiteSpace(chaveNfe) ? "NFeInformada" : "FiscalPendente")}, 0,
             {TrimOrNull(request.RecebidoPor) ?? "Operacao"}, {TrimOrNull(request.Observacoes)},
             {tenantId}, UNHEX(REPLACE(UUID(), '-', '')), {userId}, {userId}, {now}, {now});
        """, ct);

    var entradaId = await ExecuteScalarAsync<int>(db, "SELECT LAST_INSERT_ID();", ct);
    var todosRecebidos = true;

    foreach (var item in itens)
    {
        var pendente = Math.Max(0, item.Quantidade - item.QuantidadeRecebida);
        var recebidaAgora = hasExplicitItems
            ? quantidades.GetValueOrDefault(item.Id)
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
                (compra_entrada_id, compra_pedido_item_id, produto_id, produto_nome, quantidade_recebida,
                 custo_unitario, valor_total, tenant_id, row_version, created_by_user_id,
                 updated_by_user_id, created_at, updated_at)
            VALUES
                ({entradaId}, {item.Id}, {item.ProdutoId}, {item.ProdutoNome}, {recebidaAgora},
                 {item.CustoUnitario}, {valorItem}, {tenantId}, UNHEX(REPLACE(UUID(), '-', '')),
                 {userId}, {userId}, {now}, {now});
            """, ct);

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE compras_pedido_itens
            SET quantidade_recebida = {novaQuantidadeRecebida},
                updated_by_user_id = {userId},
                row_version = UNHEX(REPLACE(UUID(), '-', '')),
                updated_at = {now}
            WHERE id = {item.Id} AND tenant_id = {tenantId} AND is_deleted = 0;
            """, ct);

        if (item.ProdutoId.HasValue)
        {
            var produto = await db.Produtos.FirstOrDefaultAsync(produto => produto.Id == item.ProdutoId.Value, ct);
            if (produto is not null)
            {
                produto.EstoqueAtual += recebidaAgora;
                produto.Custo = item.CustoUnitario;
                produto.CodigoBarras ??= GerarCodigoBarrasProduto(produto);
                produto.QrCode = GerarQrCodeProduto(produto, pedidoAtual, entradaId, documento ?? chaveNfe);
                produto.IdentificacaoEstoque = GerarIdentificacaoEstoqueProduto(produto, pedidoAtual, entradaId, recebidaAgora, item.CustoUnitario, documento ?? chaveNfe);
                produto.UpdatedAt = now;

                await db.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO estoque_movimentos
                        (produto_id, compra_entrada_id, tipo, quantidade, saldo_resultante, custo_unitario,
                         origem, documento, observacoes, tenant_id, row_version, created_by_user_id,
                         updated_by_user_id, created_at, updated_at)
                    VALUES
                        ({produto.Id}, {entradaId}, {"EntradaCompra"}, {recebidaAgora}, {produto.EstoqueAtual},
                         {item.CustoUnitario}, {pedidoAtual.Origem}, {documento ?? chaveNfe}, {TrimOrNull(request.Observacoes)},
                         {tenantId}, UNHEX(REPLACE(UUID(), '-', '')), {userId}, {userId}, {now}, {now});
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
        SET valor_total = {totalEntrada},
            updated_by_user_id = {userId},
            row_version = UNHEX(REPLACE(UUID(), '-', '')),
            updated_at = {now}
        WHERE id = {entradaId} AND tenant_id = {tenantId} AND is_deleted = 0;
        """, ct);

    await db.Database.ExecuteSqlInterpolatedAsync($"""
        UPDATE compras_pedidos
        SET status = {(todosRecebidos ? "Recebido" : "RecebidoParcial")},
            status_fiscal = {(!string.IsNullOrWhiteSpace(chaveNfe) ? "NFeEntradaInformada" : "FiscalPendente")},
            updated_by_user_id = {userId},
            row_version = UNHEX(REPLACE(UUID(), '-', '')),
            updated_at = {now}
        WHERE id = {id} AND tenant_id = {tenantId} AND is_deleted = 0;
        """, ct);

    db.LogsAuditoria.Add(CreatePurchaseAudit(
        http,
        "compras_entradas",
        entradaId,
        AcaoAuditoria.INSERT,
        null,
        new { compraPedidoId = id, documento, chaveNfe, totalEntrada, statusFiscal = !string.IsNullOrWhiteSpace(chaveNfe) ? "NFeEntradaInformada" : "FiscalPendente" }));
    db.LogsAuditoria.Add(CreatePurchaseAudit(
        http,
        "compras_pedidos",
        id,
        AcaoAuditoria.UPDATE,
        new { pedidoAtual.Status },
        new { status = todosRecebidos ? "Recebido" : "RecebidoParcial", entradaId }));
    await db.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);
    return null;
    });
    if (transactionResult is not null)
    {
        return transactionResult;
    }

    var painel = await BuildComprasPainelAsync(db, ct);
    return Results.Ok(ApiResponse<ComprasPainelDto>.Ok(painel, "Entrada registrada, estoque atualizado e fiscal sinalizado."));
})
.WithName("RegistrarCompraEntrada")
;

app.MapPatch("/api/compras/solicitacoes/{id:int}/status", [Authorize(Policy = "Gerente")] async (int id, CompraStatusRequest request, NexumDbContext db, HttpContext http, CancellationToken ct) =>
{
    var executionStrategy = db.Database.CreateExecutionStrategy();
    var transactionResult = await executionStrategy.ExecuteAsync<IResult?>(async () =>
    {
    db.ChangeTracker.Clear();
    var novoStatus = NormalizeCompraSolicitacaoStatus(request.Status ?? request.NovoStatus);
    if (novoStatus is null)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Status de solicitacao invalido."));
    }

    var tenantId = db.CurrentTenantId.ToString();
    var userId = db.CurrentUserId?.ToString();
    await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
    var atual = await ExecuteScalarAsync<string>(
        db,
        "SELECT status FROM compras_solicitacoes WHERE id = @id AND tenant_id = @tenantId AND is_deleted = 0 LIMIT 1 FOR UPDATE",
        ct,
        ("@id", id),
        ("@tenantId", tenantId));

    if (string.IsNullOrWhiteSpace(atual))
    {
        return Results.NotFound(ApiResponse<string>.Erro("Solicitacao de compra nao encontrada."));
    }

    if (!IsAllowedCompraSolicitacaoTransition(atual, novoStatus))
    {
        return Results.Conflict(ApiResponse<string>.Erro($"Transicao de solicitacao {atual} -> {novoStatus} nao permitida."));
    }

    var now = DateTime.UtcNow;
    var observacao = TrimOrNull(request.Observacoes);
    if (observacao is null)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE compras_solicitacoes
            SET status = {novoStatus},
                updated_by_user_id = {userId},
                row_version = UNHEX(REPLACE(UUID(), '-', '')),
                updated_at = {now}
            WHERE id = {id} AND tenant_id = {tenantId} AND is_deleted = 0;
            """, ct);
    }
    else
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE compras_solicitacoes
            SET status = {novoStatus},
                observacoes = CONCAT_WS('\n', NULLIF(observacoes, ''), {observacao}),
                updated_by_user_id = {userId},
                row_version = UNHEX(REPLACE(UUID(), '-', '')),
                updated_at = {now}
            WHERE id = {id} AND tenant_id = {tenantId} AND is_deleted = 0;
            """, ct);
    }

    db.LogsAuditoria.Add(CreatePurchaseAudit(
        http,
        "compras_solicitacoes",
        id,
        AcaoAuditoria.UPDATE,
        new { status = atual },
        new { status = novoStatus, observacao }));
    await db.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);
    return null;
    });
    if (transactionResult is not null)
    {
        return transactionResult;
    }

    var painel = await BuildComprasPainelAsync(db, ct);
    return Results.Ok(ApiResponse<ComprasPainelDto>.Ok(painel, "Status da solicitacao atualizado."));
})
.WithName("AtualizarCompraSolicitacaoStatus")
;

app.MapPatch("/api/compras/pedidos/{id:int}/status", [Authorize(Policy = "Gerente")] async (int id, CompraStatusRequest request, NexumDbContext db, HttpContext http, CancellationToken ct) =>
{
    var executionStrategy = db.Database.CreateExecutionStrategy();
    var transactionResult = await executionStrategy.ExecuteAsync<IResult?>(async () =>
    {
    db.ChangeTracker.Clear();
    var novoStatus = NormalizeCompraPedidoStatus(request.Status ?? request.NovoStatus);
    if (novoStatus is null)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Status de pedido de compra invalido."));
    }

    var tenantId = db.CurrentTenantId.ToString();
    var userId = db.CurrentUserId?.ToString();
    await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
    var atual = await ExecuteScalarAsync<string>(
        db,
        "SELECT status FROM compras_pedidos WHERE id = @id AND tenant_id = @tenantId AND is_deleted = 0 LIMIT 1 FOR UPDATE",
        ct,
        ("@id", id),
        ("@tenantId", tenantId));

    if (string.IsNullOrWhiteSpace(atual))
    {
        return Results.NotFound(ApiResponse<string>.Erro("Pedido de compra nao encontrado."));
    }

    if (!IsAllowedCompraPedidoTransition(atual, novoStatus))
    {
        return Results.Conflict(ApiResponse<string>.Erro($"Transicao de pedido {atual} -> {novoStatus} nao permitida."));
    }

    var now = DateTime.UtcNow;
    var observacao = TrimOrNull(request.Observacoes);
    if (observacao is null)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE compras_pedidos
            SET status = {novoStatus},
                updated_by_user_id = {userId},
                row_version = UNHEX(REPLACE(UUID(), '-', '')),
                updated_at = {now}
            WHERE id = {id} AND tenant_id = {tenantId} AND is_deleted = 0;
            """, ct);
    }
    else
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE compras_pedidos
            SET status = {novoStatus},
                observacoes = CONCAT_WS('\n', NULLIF(observacoes, ''), {observacao}),
                updated_by_user_id = {userId},
                row_version = UNHEX(REPLACE(UUID(), '-', '')),
                updated_at = {now}
            WHERE id = {id} AND tenant_id = {tenantId} AND is_deleted = 0;
            """, ct);
    }

    db.LogsAuditoria.Add(CreatePurchaseAudit(
        http,
        "compras_pedidos",
        id,
        AcaoAuditoria.UPDATE,
        new { status = atual },
        new { status = novoStatus, observacao }));
    await db.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);
    return null;
    });
    if (transactionResult is not null)
    {
        return transactionResult;
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

app.MapGet("/api/pedidos/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var pedido = await db.Pedidos
        .AsNoTracking()
        .Include(item => item.Pagamentos)
        .FirstOrDefaultAsync(item => item.Id == id, ct);

    return pedido is null
        ? Results.NotFound(ApiResponse<string>.Erro("Pedido nao encontrado."))
        : Results.Ok(ApiResponse<PedidoLojaDto>.Ok(BuildPedidoLojaDto(pedido)));
})
.WithName("PedidoPorId")
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

app.MapGet("/api/financeiro/faturamento", [Authorize(Policy = "Financeiro")] async (
    NexumDbContext db,
    CancellationToken ct) =>
{
    var hoje = DateTime.UtcNow.Date;
    var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
    var inicioAno = new DateTime(hoje.Year, 1, 1);

    var receitasConfirmadas = db.Financeiros.AsNoTracking()
        .Where(item => item.Tipo == TipoLancamento.Receita && item.Status == StatusLancamento.Pago);

    var despesasConfirmadas = db.Financeiros.AsNoTracking()
        .Where(item => item.Tipo == TipoLancamento.Despesa && item.Status == StatusLancamento.Pago);

    var receitaHoje = await receitasConfirmadas
        .Where(item => (item.DataPagamento ?? item.CreatedAt) >= hoje)
        .SumAsync(item => item.Valor, ct);
    var receitaMes = await receitasConfirmadas
        .Where(item => (item.DataPagamento ?? item.CreatedAt) >= inicioMes)
        .SumAsync(item => item.Valor, ct);
    var receitaAno = await receitasConfirmadas
        .Where(item => (item.DataPagamento ?? item.CreatedAt) >= inicioAno)
        .SumAsync(item => item.Valor, ct);
    var despesasMes = await despesasConfirmadas
        .Where(item => (item.DataPagamento ?? item.CreatedAt) >= inicioMes)
        .SumAsync(item => item.Valor, ct);
    var pendenteReceber = await db.Financeiros.AsNoTracking()
        .Where(item => item.Tipo == TipoLancamento.Receita && (item.Status == StatusLancamento.Pendente || item.Status == StatusLancamento.Atrasado))
        .SumAsync(item => item.Valor, ct);
    var pendentePagar = await db.Financeiros.AsNoTracking()
        .Where(item => item.Tipo == TipoLancamento.Despesa && (item.Status == StatusLancamento.Pendente || item.Status == StatusLancamento.Atrasado))
        .SumAsync(item => item.Valor, ct);

    var resumo = new FinanceiroFaturamentoDto(
        receitaHoje,
        receitaMes,
        receitaAno,
        despesasMes,
        receitaMes - despesasMes,
        pendenteReceber,
        pendentePagar,
        DateTime.UtcNow);

    return Results.Ok(ApiResponse<FinanceiroFaturamentoDto>.Ok(resumo, "Faturamento financeiro consolidado."));
})
.WithName("FinanceiroFaturamento")
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

app.MapGet("/api/financeiro/contabil/lancamentos", [Authorize(Policy = "Financeiro")] async (
    int? empresaId,
    DateTime? inicio,
    DateTime? fim,
    string? lote,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var lancamentos = await db.Database.SqlQueryRaw<ContabilLancamentoDto>(
        """
        SELECT
            lcn_id AS Id,
            lcn_emp_id AS EmpresaId,
            lcn_lote AS Lote,
            lcn_sublote AS Sublote,
            lcn_data AS Data,
            lcn_historico_padrao AS HistoricoPadrao,
            lcn_complemento AS Complemento,
            lcn_valor AS Valor,
            lcn_tipo AS Tipo,
            lcn_origem_modulo AS OrigemModulo,
            lcn_origem_id AS OrigemId,
            lcn_estornado AS Estornado,
            lcn_lcn_estorno_id AS LancamentoEstornoId,
            lcn_usr_cadastro AS UsuarioCadastroId,
            lcn_data_cadastro AS CriadoEm
        FROM cnt_lancamentos
        WHERE ({0} IS NULL OR lcn_emp_id = {0})
          AND ({1} IS NULL OR lcn_data >= {1})
          AND ({2} IS NULL OR lcn_data <= {2})
          AND ({3} IS NULL OR lcn_lote = {3})
        ORDER BY lcn_data DESC, lcn_id DESC
        LIMIT 500
        """,
        (object?)empresaId ?? DBNull.Value,
        (object?)inicio?.Date ?? DBNull.Value,
        (object?)fim?.Date ?? DBNull.Value,
        (object?)TrimOrNull(lote) ?? DBNull.Value)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<ContabilLancamentoDto>>.Ok(lancamentos, "Lancamentos contabeis carregados.", lancamentos.Count));
})
.WithName("FicoContabilLancamentosListar");

app.MapPost("/api/financeiro/contabil/lancamentos", [Authorize(Policy = "Financeiro")] async (
    ContabilLancamentoRequest request,
    NexumDbContext db,
    ClaimsPrincipal principal,
    CancellationToken ct) =>
{
    if (request.EmpresaId <= 0 || request.Valor <= 0 || request.Partidas is null || request.Partidas.Count < 2)
    {
        return Results.BadRequest(ApiResponse<ContabilLancamentoDto>.Erro("Empresa, valor positivo e ao menos duas partidas sao obrigatorios."));
    }

    var partidas = new List<ContabilPartidaRequest>(request.Partidas.Count);
    foreach (var partida in request.Partidas)
    {
        var tipo = NormalizePartidaTipo(partida.Tipo);
        if (string.IsNullOrWhiteSpace(tipo) || partida.PlanoContaId <= 0 || partida.Valor <= 0)
        {
            return Results.BadRequest(ApiResponse<ContabilLancamentoDto>.Erro("Partidas exigem tipo, conta contabil e valor positivo."));
        }

        partidas.Add(partida with { Tipo = tipo });
    }

    var debitos = partidas.Where(item => string.Equals(item.Tipo, "DEBITO", StringComparison.Ordinal)).Sum(item => item.Valor);
    var creditos = partidas.Where(item => string.Equals(item.Tipo, "CREDITO", StringComparison.Ordinal)).Sum(item => item.Valor);
    if (debitos <= 0 || creditos <= 0 || Math.Abs(debitos - creditos) > 0.01m || Math.Abs(debitos - request.Valor) > 0.01m)
    {
        return Results.BadRequest(ApiResponse<ContabilLancamentoDto>.Erro("Lancamento contabil deve fechar debitos e creditos com o mesmo valor."));
    }

    var lote = NormalizeBusinessCode(request.Lote);
    if (string.IsNullOrWhiteSpace(lote))
    {
        lote = $"LCT_{DateTime.UtcNow:yyyyMMddHHmmss}";
    }

    await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
    try
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO cnt_lancamentos
                (lcn_emp_id, lcn_lote, lcn_sublote, lcn_data, lcn_historico_padrao, lcn_complemento, lcn_valor, lcn_tipo,
                 lcn_origem_modulo, lcn_origem_id, lcn_estornado, lcn_usr_cadastro, lcn_data_cadastro)
            VALUES
                ({request.EmpresaId}, {lote}, {TrimOrNull(request.Sublote)}, {request.Data.Date}, {TrimOrNull(request.HistoricoPadrao)},
                 {TrimOrNull(request.Complemento)}, {request.Valor}, {NormalizeContabilLancamentoTipo(request.Tipo)},
                 {NormalizeBusinessKey(request.OrigemModulo)}, {request.OrigemId}, 0, {GetCurrentUserId(principal)}, UTC_TIMESTAMP())
            """,
            ct);

        var lancamentoId = await db.Database.SqlQueryRaw<int>("SELECT LAST_INSERT_ID() AS Value").SingleAsync(ct);
        foreach (var partida in partidas)
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"""
                INSERT INTO cnt_partidas (prt_lcn_id, prt_tipo, prt_pct_id, prt_ccu_id, prt_valor, prt_historico)
                VALUES ({lancamentoId}, {partida.Tipo}, {partida.PlanoContaId}, {partida.CentroCustoId}, {partida.Valor}, {TrimOrNull(partida.Historico)})
                """,
                ct);
        }

        var persisted = await LoadContabilLancamentoAsync(db, lancamentoId, ct)
            ?? throw new InvalidOperationException("Lancamento contabil criado sem releitura do banco.");
        await transaction.CommitAsync(ct);
        return Results.Created($"/api/financeiro/contabil/lancamentos/{lancamentoId}", ApiResponse<ContabilLancamentoDto>.Ok(persisted, "Lancamento contabil criado e relido."));
    }
    catch
    {
        await transaction.RollbackAsync(ct);
        throw;
    }
})
.WithName("FicoContabilLancamentosCriar");

app.MapGet("/api/financeiro/razao", [Authorize(Policy = "Financeiro")] async (
    int? empresaId,
    int? planoContaId,
    DateTime? inicio,
    DateTime? fim,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var razao = await db.Database.SqlQueryRaw<RazaoContabilDto>(
        """
        SELECT
            l.lcn_emp_id AS EmpresaId,
            p.prt_pct_id AS PlanoContaId,
            l.lcn_data AS Data,
            l.lcn_lote AS Lote,
            p.prt_tipo AS Tipo,
            p.prt_valor AS Valor,
            COALESCE(p.prt_historico, l.lcn_complemento, l.lcn_historico_padrao) AS Historico,
            l.lcn_origem_modulo AS OrigemModulo,
            l.lcn_origem_id AS OrigemId
        FROM cnt_partidas p
        INNER JOIN cnt_lancamentos l ON l.lcn_id = p.prt_lcn_id
        WHERE l.lcn_estornado = 0
          AND ({0} IS NULL OR l.lcn_emp_id = {0})
          AND ({1} IS NULL OR p.prt_pct_id = {1})
          AND ({2} IS NULL OR l.lcn_data >= {2})
          AND ({3} IS NULL OR l.lcn_data <= {3})
        ORDER BY l.lcn_data, l.lcn_id, p.prt_id
        LIMIT 1000
        """,
        (object?)empresaId ?? DBNull.Value,
        (object?)planoContaId ?? DBNull.Value,
        (object?)inicio?.Date ?? DBNull.Value,
        (object?)fim?.Date ?? DBNull.Value)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<RazaoContabilDto>>.Ok(razao, "Razao contabil carregado.", razao.Count));
})
.WithName("FicoRazaoContabil");

app.MapGet("/api/financeiro/conciliacao", [Authorize(Policy = "Financeiro")] async (
    DateTime? inicio,
    DateTime? fim,
    string? status,
    NexumDbContext db,
    ITenantContext tenantContext,
    CancellationToken ct) =>
    await ListarConciliacoesFinanceirasAsync(inicio, fim, status, db, tenantContext, ct))
.WithName("FicoConciliacaoListar");

app.MapPost("/api/financeiro/conciliacao", [Authorize(Policy = "Financeiro")] async (
    ConciliacaoFinanceiraRequest request,
    NexumDbContext db,
    ClaimsPrincipal principal,
    ITenantContext tenantContext,
    HttpContext httpContext,
    CancellationToken ct) =>
    await RegistrarConciliacaoFinanceiraAsync(request, db, principal, tenantContext, httpContext, ct))
.WithName("FicoConciliacaoRegistrar");

app.MapGet("/api/financeiro/dre", [Authorize(Policy = "Financeiro")] async (
    int? empresaId,
    string? competencia,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var dre = await db.Database.SqlQueryRaw<DreGerencialDto>(
        """
        SELECT
            empresa_id AS EmpresaId,
            competencia AS Competencia,
            classe_conta AS ClasseConta,
            pct_codigo AS ContaCodigo,
            pct_nome AS ContaNome,
            valor_debito AS ValorDebito,
            valor_credito AS ValorCredito,
            saldo_conta AS SaldoConta
        FROM vw_dre_gerencial
        WHERE ({0} IS NULL OR empresa_id = {0})
          AND ({1} IS NULL OR competencia = {1})
        ORDER BY competencia DESC, classe_conta, pct_codigo
        LIMIT 1000
        """,
        (object?)empresaId ?? DBNull.Value,
        (object?)TrimOrNull(competencia) ?? DBNull.Value)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<DreGerencialDto>>.Ok(dre, "DRE gerencial carregada.", dre.Count));
})
.WithName("FicoDreGerencial");

app.MapGet("/api/financeiro/fechamento", [Authorize(Policy = "Financeiro")] async (
    int? empresaId,
    DateTime? periodo,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var fechamentos = await db.Database.SqlQueryRaw<FechamentoContabilDto>(
        """
        SELECT
            fec_id AS Id,
            fec_emp_id AS EmpresaId,
            fec_periodo AS Periodo,
            fec_data_fechamento AS DataFechamento,
            fec_usr_responsavel AS UsuarioResponsavelId,
            fec_bloqueado AS Bloqueado,
            fec_observacoes AS Observacoes
        FROM cnt_fechamentos
        WHERE ({0} IS NULL OR fec_emp_id = {0})
          AND ({1} IS NULL OR fec_periodo = {1})
        ORDER BY fec_periodo DESC, fec_id DESC
        """,
        (object?)empresaId ?? DBNull.Value,
        (object?)periodo?.Date ?? DBNull.Value)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<FechamentoContabilDto>>.Ok(fechamentos, "Fechamentos contabeis carregados.", fechamentos.Count));
})
.WithName("FicoFechamentoListar");

app.MapPost("/api/financeiro/fechamento", [Authorize(Policy = "Financeiro")] async (
    FechamentoContabilRequest request,
    NexumDbContext db,
    ClaimsPrincipal principal,
    CancellationToken ct) =>
{
    if (request.EmpresaId <= 0)
    {
        return Results.BadRequest(ApiResponse<FechamentoContabilDto>.Erro("Empresa e obrigatoria para fechamento contabil."));
    }

    var periodo = new DateTime(request.Periodo.Year, request.Periodo.Month, 1);
    await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
    try
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO cnt_fechamentos (fec_emp_id, fec_periodo, fec_usr_responsavel, fec_bloqueado, fec_observacoes)
            VALUES ({request.EmpresaId}, {periodo}, {GetCurrentUserId(principal)}, {request.Bloqueado}, {TrimOrNull(request.Observacoes)})
            ON DUPLICATE KEY UPDATE
                fec_usr_responsavel = VALUES(fec_usr_responsavel),
                fec_bloqueado = VALUES(fec_bloqueado),
                fec_observacoes = VALUES(fec_observacoes),
                fec_data_fechamento = UTC_TIMESTAMP()
            """,
            ct);

        var fechamento = await db.Database.SqlQueryRaw<FechamentoContabilDto>(
            """
            SELECT fec_id AS Id, fec_emp_id AS EmpresaId, fec_periodo AS Periodo, fec_data_fechamento AS DataFechamento,
                   fec_usr_responsavel AS UsuarioResponsavelId, fec_bloqueado AS Bloqueado, fec_observacoes AS Observacoes
            FROM cnt_fechamentos
            WHERE fec_emp_id = {0} AND fec_periodo = {1}
            """,
            request.EmpresaId,
            periodo)
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("Fechamento contabil gravado sem releitura do banco.");

        await transaction.CommitAsync(ct);
        return Results.Ok(ApiResponse<FechamentoContabilDto>.Ok(fechamento, "Fechamento contabil registrado e relido."));
    }
    catch
    {
        await transaction.RollbackAsync(ct);
        throw;
    }
})
.WithName("FicoFechamentoRegistrar");

app.MapGet("/api/financeiro/contabil/razao", [Authorize(Policy = "Financeiro")] async (
    int? empresaId,
    int? planoContaId,
    DateTime? inicio,
    DateTime? fim,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var razao = await db.Database.SqlQueryRaw<RazaoContabilDto>(
        """
        SELECT
            l.lcn_emp_id AS EmpresaId,
            p.prt_pct_id AS PlanoContaId,
            l.lcn_data AS Data,
            l.lcn_lote AS Lote,
            p.prt_tipo AS Tipo,
            p.prt_valor AS Valor,
            COALESCE(p.prt_historico, l.lcn_complemento, l.lcn_historico_padrao) AS Historico,
            l.lcn_origem_modulo AS OrigemModulo,
            l.lcn_origem_id AS OrigemId
        FROM cnt_partidas p
        INNER JOIN cnt_lancamentos l ON l.lcn_id = p.prt_lcn_id
        WHERE l.lcn_estornado = 0
          AND ({0} IS NULL OR l.lcn_emp_id = {0})
          AND ({1} IS NULL OR p.prt_pct_id = {1})
          AND ({2} IS NULL OR l.lcn_data >= {2})
          AND ({3} IS NULL OR l.lcn_data <= {3})
        ORDER BY l.lcn_data, l.lcn_id, p.prt_id
        LIMIT 1000
        """,
        (object?)empresaId ?? DBNull.Value,
        (object?)planoContaId ?? DBNull.Value,
        (object?)inicio?.Date ?? DBNull.Value,
        (object?)fim?.Date ?? DBNull.Value)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<RazaoContabilDto>>.Ok(razao, "Razao contabil carregado.", razao.Count));
})
.WithName("FicoContabilRazao");

app.MapGet("/api/financeiro/contabil/conciliacao", [Authorize(Policy = "Financeiro")] async (
    DateTime? inicio,
    DateTime? fim,
    string? status,
    NexumDbContext db,
    ITenantContext tenantContext,
    CancellationToken ct) =>
    await ListarConciliacoesFinanceirasAsync(inicio, fim, status, db, tenantContext, ct))
.WithName("FicoContabilConciliacaoListar");

app.MapPost("/api/financeiro/contabil/conciliacao", [Authorize(Policy = "Financeiro")] async (
    ConciliacaoFinanceiraRequest request,
    NexumDbContext db,
    ClaimsPrincipal principal,
    ITenantContext tenantContext,
    HttpContext httpContext,
    CancellationToken ct) =>
    await RegistrarConciliacaoFinanceiraAsync(request, db, principal, tenantContext, httpContext, ct))
.WithName("FicoContabilConciliacaoRegistrar");

app.MapGet("/api/financeiro/contabil/dre", [Authorize(Policy = "Financeiro")] async (
    int? empresaId,
    string? competencia,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var dre = await db.Database.SqlQueryRaw<DreGerencialDto>(
        """
        SELECT
            empresa_id AS EmpresaId,
            competencia AS Competencia,
            classe_conta AS ClasseConta,
            pct_codigo AS ContaCodigo,
            pct_nome AS ContaNome,
            valor_debito AS ValorDebito,
            valor_credito AS ValorCredito,
            saldo_conta AS SaldoConta
        FROM vw_dre_gerencial
        WHERE ({0} IS NULL OR empresa_id = {0})
          AND ({1} IS NULL OR competencia = {1})
        ORDER BY competencia DESC, classe_conta, pct_codigo
        LIMIT 1000
        """,
        (object?)empresaId ?? DBNull.Value,
        (object?)TrimOrNull(competencia) ?? DBNull.Value)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<DreGerencialDto>>.Ok(dre, "DRE gerencial carregada.", dre.Count));
})
.WithName("FicoContabilDre");

app.MapGet("/api/financeiro/contabil/fechamento", [Authorize(Policy = "Financeiro")] async (
    int? empresaId,
    DateTime? periodo,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var fechamentos = await db.Database.SqlQueryRaw<FechamentoContabilDto>(
        """
        SELECT
            fec_id AS Id,
            fec_emp_id AS EmpresaId,
            fec_periodo AS Periodo,
            fec_data_fechamento AS DataFechamento,
            fec_usr_responsavel AS UsuarioResponsavelId,
            fec_bloqueado AS Bloqueado,
            fec_observacoes AS Observacoes
        FROM cnt_fechamentos
        WHERE ({0} IS NULL OR fec_emp_id = {0})
          AND ({1} IS NULL OR fec_periodo = {1})
        ORDER BY fec_periodo DESC, fec_id DESC
        """,
        (object?)empresaId ?? DBNull.Value,
        (object?)periodo?.Date ?? DBNull.Value)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<FechamentoContabilDto>>.Ok(fechamentos, "Fechamentos contabeis carregados.", fechamentos.Count));
})
.WithName("FicoContabilFechamentoListar");

app.MapPost("/api/financeiro/contabil/fechamento", [Authorize(Policy = "Financeiro")] async (
    FechamentoContabilRequest request,
    NexumDbContext db,
    ClaimsPrincipal principal,
    CancellationToken ct) =>
{
    if (request.EmpresaId <= 0)
    {
        return Results.BadRequest(ApiResponse<FechamentoContabilDto>.Erro("Empresa e obrigatoria para fechamento contabil."));
    }

    var periodo = new DateTime(request.Periodo.Year, request.Periodo.Month, 1);
    await db.Database.ExecuteSqlInterpolatedAsync(
        $"""
        INSERT INTO cnt_fechamentos (fec_emp_id, fec_periodo, fec_usr_responsavel, fec_bloqueado, fec_observacoes)
        VALUES ({request.EmpresaId}, {periodo}, {GetCurrentUserId(principal)}, {request.Bloqueado}, {TrimOrNull(request.Observacoes)})
        ON DUPLICATE KEY UPDATE
            fec_usr_responsavel = VALUES(fec_usr_responsavel),
            fec_bloqueado = VALUES(fec_bloqueado),
            fec_observacoes = VALUES(fec_observacoes),
            fec_data_fechamento = UTC_TIMESTAMP()
        """,
        ct);

    var fechamento = await db.Database.SqlQueryRaw<FechamentoContabilDto>(
        """
        SELECT fec_id AS Id, fec_emp_id AS EmpresaId, fec_periodo AS Periodo, fec_data_fechamento AS DataFechamento,
               fec_usr_responsavel AS UsuarioResponsavelId, fec_bloqueado AS Bloqueado, fec_observacoes AS Observacoes
        FROM cnt_fechamentos
        WHERE fec_emp_id = {0} AND fec_periodo = {1}
        """,
        request.EmpresaId,
        periodo)
        .FirstAsync(ct);

    return Results.Ok(ApiResponse<FechamentoContabilDto>.Ok(fechamento, "Fechamento contabil registrado."));
})
.WithName("FicoContabilFechamentoRegistrar");

app.MapGet("/api/fiscal/sped", [Authorize(Policy = "Fiscal")] async (
    int? empresaId,
    string? tipo,
    int? ano,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var arquivos = await db.Database.SqlQueryRaw<SpedArquivoDto>(
        """
        SELECT spc_id AS Id, spc_emp_id AS EmpresaId, CAST(spc_ano AS CHAR) AS Periodo, spc_tipo AS Tipo, spc_nome_arquivo AS NomeArquivo,
               spc_status AS Status, spc_protocolo AS Protocolo, spc_mensagem_erro AS MensagemErro, spc_data_geracao AS CriadoEm
        FROM fis_sped_contabil
        WHERE ({0} IS NULL OR spc_emp_id = {0})
          AND ({1} IS NULL OR spc_tipo = {1})
          AND ({2} IS NULL OR spc_ano = {2})
        UNION ALL
        SELECT spf_id AS Id, spf_emp_id AS EmpresaId, DATE_FORMAT(spf_periodo, '%Y-%m') AS Periodo, spf_tipo AS Tipo, spf_nome_arquivo AS NomeArquivo,
               spf_status AS Status, spf_protocolo AS Protocolo, spf_mensagem_erro AS MensagemErro, spf_data_geracao AS CriadoEm
        FROM fis_sped_fiscal
        WHERE ({0} IS NULL OR spf_emp_id = {0})
          AND ({1} IS NULL OR spf_tipo = {1})
          AND ({2} IS NULL OR YEAR(spf_periodo) = {2})
        ORDER BY CriadoEm DESC
        LIMIT 500
        """,
        (object?)empresaId ?? DBNull.Value,
        (object?)NormalizeSpedTipo(tipo) ?? DBNull.Value,
        (object?)ano ?? DBNull.Value)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<SpedArquivoDto>>.Ok(arquivos, "Arquivos SPED carregados.", arquivos.Count));
})
.WithName("FiscalSpedListar");

app.MapPost("/api/fiscal/sped", [Authorize(Policy = "Fiscal")] async (
    SpedGeracaoRequest request,
    NexumDbContext db,
    ClaimsPrincipal principal,
    CancellationToken ct) =>
{
    if (request.EmpresaId <= 0)
    {
        return Results.BadRequest(ApiResponse<SpedArquivoDto>.Erro("Empresa e obrigatoria para geracao SPED."));
    }

    var tipo = NormalizeSpedTipo(request.Tipo);
    if (tipo is "ECD" or "ECF")
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO fis_sped_contabil
                (spc_emp_id, spc_ano, spc_tipo, spc_arquivo, spc_nome_arquivo, spc_status, spc_usr_responsavel)
            VALUES
                ({request.EmpresaId}, {request.Ano ?? DateTime.UtcNow.Year}, {tipo}, {BuildSpedArquivo(request)}, {BuildSpedNomeArquivo(request, tipo)},
                 'GERADO', {GetCurrentUserId(principal)})
            """,
            ct);
    }
    else
    {
        var periodo = request.Periodo ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO fis_sped_fiscal
                (spf_emp_id, spf_periodo, spf_tipo, spf_arquivo, spf_nome_arquivo, spf_status, spf_usr_responsavel)
            VALUES
                ({request.EmpresaId}, {new DateTime(periodo.Year, periodo.Month, 1)}, {tipo ?? "ICMS_IPI"}, {BuildSpedArquivo(request)},
                 {BuildSpedNomeArquivo(request, tipo ?? "ICMS_IPI")}, 'GERADO', {GetCurrentUserId(principal)})
            """,
            ct);
    }

    return Results.Created("/api/fiscal/sped", ApiResponse<object>.Ok(new { request.EmpresaId, Tipo = tipo }, "Arquivo SPED gerado e registrado."));
})
.WithName("FiscalSpedGerar");

app.MapGet("/api/fiscal/ecf", [Authorize(Policy = "Fiscal")] async (
    int? empresaId,
    int? ano,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var arquivos = await db.Database.SqlQueryRaw<SpedArquivoDto>(
        """
        SELECT spc_id AS Id, spc_emp_id AS EmpresaId, CAST(spc_ano AS CHAR) AS Periodo, spc_tipo AS Tipo, spc_nome_arquivo AS NomeArquivo,
               spc_status AS Status, spc_protocolo AS Protocolo, spc_mensagem_erro AS MensagemErro, spc_data_geracao AS CriadoEm
        FROM fis_sped_contabil
        WHERE spc_tipo = 'ECF'
          AND ({0} IS NULL OR spc_emp_id = {0})
          AND ({1} IS NULL OR spc_ano = {1})
        ORDER BY spc_ano DESC, spc_id DESC
        """,
        (object?)empresaId ?? DBNull.Value,
        (object?)ano ?? DBNull.Value)
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<SpedArquivoDto>>.Ok(arquivos, "ECF carregada.", arquivos.Count));
})
.WithName("FiscalEcfListar");

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
    var rastreamentoPersistido = await db.LogisticaRastreamentoConsultas
        .AsNoTracking()
        .Where(item => item.PedidoId == pedido.Id && item.Operacional)
        .OrderByDescending(item => item.ConsultadoAt)
        .FirstOrDefaultAsync(ct);
    List<LogisticaRastreamentoEventoDto> eventosRastreamento = rastreamentoPersistido is null
        ? []
        : JsonSerializer.Deserialize<List<LogisticaRastreamentoEventoDto>>(rastreamentoPersistido.EventosJson) ?? [];

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
        pedido.UpdatedAt,
        rastreamentoPersistido?.StatusExterno,
        eventosRastreamento,
        rastreamentoPersistido?.ConsultadoAt);

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
    if (status == StatusPedido.Enviado
        && (string.IsNullOrWhiteSpace(pedido.FreteTransportadora)
            || string.IsNullOrWhiteSpace(pedido.FreteMetodo)
            || string.IsNullOrWhiteSpace(pedido.FreteCodigoRastreio)
            || pedido.FretePrazoDias <= 0))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Configure transportadora, metodo, codigo de rastreio real e prazo antes de marcar o pedido como enviado."));
    }
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
    if (status == StatusPedido.Enviado)
    {
        pedido.DataEnvio ??= DateTime.UtcNow;
    }

    if (status == StatusPedido.Entregue)
    {
        pedido.DataEntrega ??= DateTime.UtcNow;
    }
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
            await notificacaoService.EnviarConfirmacaoPagamentoAsync(pedido.Cliente, pedido, ct);
        }

        if (status is StatusPedido.EmSeparacao or StatusPedido.Enviado or StatusPedido.Entregue or StatusPedido.Cancelado or StatusPedido.Devolvido or StatusPedido.Reembolsado)
        {
            var mensagem = BuildMensagemAtualizacaoPedido(pedido);
            await notificacaoService.EnviarStatusPedidoAsync(pedido.Cliente, pedido, mensagem, ct);
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
            await notificacaoService.EnviarConfirmacaoPagamentoAsync(pedido.Cliente, pedido, ct);
        }

        if (proximoStatus is StatusPedido.EmSeparacao or StatusPedido.Enviado or StatusPedido.Entregue)
        {
            await notificacaoService.EnviarStatusPedidoAsync(pedido.Cliente, pedido, BuildMensagemAtualizacaoPedido(pedido), ct);
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

    var freteMetodo = TrimOrNull(request.FreteMetodo);
    var transportadora = TrimOrNull(request.FreteTransportadora);
    var codigoRastreio = TrimOrNull(request.FreteCodigoRastreio);
    if (freteMetodo?.Length > 50 || transportadora?.Length > 50 || codigoRastreio?.Length > 50)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Metodo, transportadora e codigo de rastreio aceitam no maximo 50 caracteres."));
    }

    if (request.FretePrazoDias is < 1 or > 120)
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Prazo de entrega deve estar entre 1 e 120 dias."));
    }

    if (!string.IsNullOrWhiteSpace(request.RowVersion))
    {
        if (!TryDecodeRowVersion(request.RowVersion, out var expectedRowVersion))
        {
            return Results.BadRequest(ApiResponse<string>.Erro("RowVersion invalida."));
        }

        var currentRowVersion = db.Entry(pedido).Property<byte[]>("RowVersion").CurrentValue ?? [];
        if (!currentRowVersion.SequenceEqual(expectedRowVersion))
        {
            return Results.Conflict(ApiResponse<string>.Erro("O pedido foi alterado por outro operador. Recarregue a expedicao antes de salvar."));
        }

        db.Entry(pedido).Property<byte[]>("RowVersion").OriginalValue = expectedRowVersion;
    }

    pedido.FreteMetodo = freteMetodo ?? pedido.FreteMetodo;
    pedido.FreteTransportadora = transportadora ?? pedido.FreteTransportadora;
    pedido.FreteCodigoRastreio = codigoRastreio;
    pedido.FretePrazoDias = request.FretePrazoDias is > 0 ? Math.Min(request.FretePrazoDias.Value, 120) : pedido.FretePrazoDias;
    pedido.UpdatedAt = DateTime.UtcNow;

    try
    {
        await db.SaveChangesAsync(ct);
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict(ApiResponse<string>.Erro("O pedido foi alterado por outro operador. Recarregue a expedicao antes de salvar."));
    }

    if (pedido.Status == StatusPedido.Enviado && pedido.Cliente is not null)
    {
        await notificacaoService.EnviarStatusPedidoAsync(pedido.Cliente, pedido, BuildMensagemAtualizacaoPedido(pedido), ct);
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
    ILoggerFactory loggerFactory,
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
        .GroupBy(item => $"{item.ProdutoId.Trim().ToUpperInvariant()}\u001f{item.Variacao?.Trim()}", StringComparer.Ordinal)
        .Select(group =>
        {
            var first = group.First();
            return new PedidoItemRequest(first.ProdutoId.Trim(), group.Sum(item => item.Quantidade), first.Variacao?.Trim());
        })
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

    var checkoutExecutionStrategy = db.Database.CreateExecutionStrategy();
    var checkoutCreation = await checkoutExecutionStrategy.ExecuteAsync(async () =>
    {
    await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
    var produtoSlugs = itensSolicitados.Select(item => item.ProdutoId).ToList();
    var produtosMap = await FiltrarProdutosPublicaveis(db.Produtos)
        .Include(produto => produto.Fornecedor)
        .Where(produto => produtoSlugs.Contains(produto.Slug))
        .ToDictionaryAsync(produto => produto.Slug, ct);

    if (produtosMap.Count != produtoSlugs.Distinct(StringComparer.OrdinalIgnoreCase).Count())
    {
        return CheckoutCreationResult.Fail(Results.BadRequest(ApiResponse<string>.Erro("Um ou mais produtos nao estao liberados para venda.")));
    }

    decimal subtotal = 0m;
    var itens = new List<PedidoItem>(itensSolicitados.Count);
    var abastecimentoResumo = new List<string>(itensSolicitados.Count);
    foreach (var item in itensSolicitados)
    {
        if (!produtosMap.TryGetValue(item.ProdutoId, out var produto))
        {
            return CheckoutCreationResult.Fail(Results.BadRequest(ApiResponse<string>.Erro("Produto invalido.")));
        }

        if (!TryFormatProductVariant(produto, item.Variacao, out var variantDescription, out var variantError))
        {
            return CheckoutCreationResult.Fail(Results.BadRequest(ApiResponse<string>.Erro(variantError)));
        }

        if (produto.TipoProduto == NexumAltivon.API.Models.TipoProduto.Dropshipping && produto.FornecedorId is null)
        {
            return CheckoutCreationResult.Fail(Results.BadRequest(ApiResponse<string>.Erro(
                $"O produto {produto.Nome} está marcado como dropshipping, mas ainda não possui fornecedor vinculado.")));
        }

        var estoqueDisponivel = produto.EstoqueAtual - produto.EstoqueReservado;
        if (estoqueDisponivel < item.Quantidade)
        {
            return CheckoutCreationResult.Fail(Results.BadRequest(ApiResponse<string>.Erro(
                $"Estoque insuficiente para {produto.Nome}. Disponivel: {Math.Max(0, estoqueDisponivel)}.")));
        }

        var precoUnitario = produto.PrecoPromocional ?? produto.Preco;
        var precoTotal = precoUnitario * item.Quantidade;
        subtotal += precoTotal;
        produto.EstoqueReservado += item.Quantidade;
        produto.UpdatedAt = DateTime.UtcNow;

        itens.Add(new PedidoItem
        {
            ProdutoId = produto.Id,
            NomeProduto = LimitProductName(string.IsNullOrWhiteSpace(variantDescription)
                ? produto.Nome
                : $"{produto.Nome} ({variantDescription})"),
            SkuProduto = produto.Sku,
            ImagemProduto = produto.ImagemPrincipal,
            Quantidade = item.Quantidade,
            PrecoUnitario = precoUnitario,
            PrecoTotal = precoTotal,
            CreatedAt = DateTime.UtcNow
        });

        abastecimentoResumo.Add(BuildAbastecimentoResumo(produto, item.Quantidade));
    }

    Cupom? cupomAplicado = null;
    decimal desconto = 0m;
    if (!string.IsNullOrWhiteSpace(request.CupomCodigo))
    {
        var agora = DateTime.UtcNow;
        var codigoCupom = request.CupomCodigo.Trim().ToUpperInvariant();
        cupomAplicado = await db.Cupons.FirstOrDefaultAsync(c => c.Codigo == codigoCupom && c.Ativo, ct);
        if (cupomAplicado is null)
        {
            return CheckoutCreationResult.Fail(Results.BadRequest(ApiResponse<string>.Erro("Cupom invalido ou inativo.")));
        }

        if (cupomAplicado.ValidoDe.HasValue && cupomAplicado.ValidoDe.Value.Date > agora.Date)
        {
            return CheckoutCreationResult.Fail(Results.BadRequest(ApiResponse<string>.Erro("Cupom ainda nao esta vigente.")));
        }

        if (cupomAplicado.ValidoAte.HasValue && cupomAplicado.ValidoAte.Value.Date < agora.Date)
        {
            return CheckoutCreationResult.Fail(Results.BadRequest(ApiResponse<string>.Erro("Cupom expirado.")));
        }

        if (cupomAplicado.QuantidadeUsos.HasValue && cupomAplicado.UsosAtuais >= cupomAplicado.QuantidadeUsos.Value)
        {
            return CheckoutCreationResult.Fail(Results.Conflict(ApiResponse<string>.Erro("Cupom esgotado.")));
        }

        if (subtotal < cupomAplicado.ValorMinimoPedido)
        {
            return CheckoutCreationResult.Fail(Results.BadRequest(ApiResponse<string>.Erro(
                $"Subtotal minimo para este cupom: {cupomAplicado.ValorMinimoPedido.ToString("C", CultureInfo.GetCultureInfo("pt-BR"))}.")));
        }

        if (cupomAplicado.QuantidadePorCliente > 0)
        {
            var usosCliente = await db.Pedidos.CountAsync(
                pedido => pedido.ClienteId == cliente.Id &&
                    pedido.CupomCodigo == codigoCupom &&
                    pedido.Status != StatusPedido.Cancelado,
                ct);
            if (usosCliente >= cupomAplicado.QuantidadePorCliente)
            {
                return CheckoutCreationResult.Fail(Results.Conflict(ApiResponse<string>.Erro("Limite de uso deste cupom por cliente atingido.")));
            }
        }

        if (cupomAplicado.PrimeiroCompraOnly)
        {
            var possuiPedido = await db.Pedidos.AnyAsync(
                pedido => pedido.ClienteId == cliente.Id && pedido.Status != StatusPedido.Cancelado,
                ct);
            if (possuiPedido)
            {
                return CheckoutCreationResult.Fail(Results.Conflict(ApiResponse<string>.Erro("Cupom exclusivo para a primeira compra.")));
            }
        }

        desconto = cupomAplicado.Tipo switch
        {
            TipoCupom.Percentual => subtotal * (cupomAplicado.Valor / 100m),
            TipoCupom.ValorFixo => cupomAplicado.Valor,
            TipoCupom.FreteGratis => 0m,
            _ => throw new InvalidOperationException($"Tipo de cupom nao suportado: {cupomAplicado.Tipo}.")
        };
        if (cupomAplicado.ValorMaximoDesconto.HasValue)
        {
            desconto = Math.Min(desconto, cupomAplicado.ValorMaximoDesconto.Value);
        }

        desconto = Math.Min(desconto, subtotal);
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
        return CheckoutCreationResult.Fail(Results.BadRequest(ApiResponse<string>.Erro("CEP de entrega obrigatorio para calcular o frete.")));
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
        return CheckoutCreationResult.Fail(Results.BadRequest(ApiResponse<string>.Erro("Opcao de frete invalida ou indisponivel. Refaca a cotacao.")));
    }

    if (cupomAplicado?.Tipo == TipoCupom.FreteGratis)
    {
        desconto = freteSelecionado.Valor;
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
        CupomCodigo = cupomAplicado?.Codigo,
        CupomDesconto = desconto,
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
    if (cupomAplicado is not null)
    {
        cupomAplicado.UsosAtuais++;
        cupomAplicado.UpdatedAt = DateTime.UtcNow;
    }
    await db.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);

    return CheckoutCreationResult.Success(pedido);
    });

    if (checkoutCreation.Error is not null)
    {
        return checkoutCreation.Error;
    }

    var pedido = checkoutCreation.Pedido
        ?? throw new InvalidOperationException("A transacao de checkout terminou sem retornar o pedido persistido.");

    StatusNfe? checkoutFiscalStatus = null;
    string? fiscalOperationalAlert = null;
    string? genesisOperationalAlert = null;
    string? notificationOperationalAlert = null;
    var checkoutLogger = loggerFactory.CreateLogger("NexumAltivon.Checkout");
    try
    {
        checkoutFiscalStatus = await EnsureCheckoutFiscalAutomationAsync(
            services,
            pedido,
            cliente,
            request,
            fiscalRoutingEngine,
            ct);
    }
    catch (Exception fiscalException)
    {
        checkoutLogger.LogError(
            fiscalException,
            "Falha na preparacao fiscal posterior ao commit do pedido {PedidoId} ({NumeroPedido}).",
            pedido.Id,
            pedido.NumeroPedido);

        try
        {
            using var incidentTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var incidentReference = await RegisterCheckoutFiscalIncidentAsync(
                services,
                pedido,
                fiscalException,
                http,
                incidentTimeout.Token);
            fiscalOperationalAlert = $"Pedido registrado. A preparacao fiscal requer revisao operacional sob a referencia {incidentReference}.";
            checkoutFiscalStatus = StatusNfe.Pendente;
        }
        catch (Exception incidentException)
        {
            checkoutLogger.LogCritical(
                incidentException,
                "Pedido {PedidoId} ({NumeroPedido}) foi commitado, mas o incidente fiscal nao foi persistido.",
                pedido.Id,
                pedido.NumeroPedido);

            return Results.Problem(
                detail: $"O pedido {pedido.NumeroPedido} foi registrado, mas a preparacao fiscal e o registro do incidente falharam. Nao repita a compra; informe este numero ao atendimento.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Pedido registrado com falha operacional critica");
        }

        if (fiscalException is OperationCanceledException && ct.IsCancellationRequested)
        {
            throw;
        }
    }

    var genesisResult = await TryCreateGenesisContaReceberPedidoAsync(services, pedido, cliente, ct);
    if (!genesisResult.Succeeded)
    {
        checkoutLogger.LogError(
            "Conta a receber Genesis nao foi confirmada para o pedido {PedidoId} ({NumeroPedido}). Codigo {FailureCode}.",
            pedido.Id,
            pedido.NumeroPedido,
            genesisResult.FailureCode);

        try
        {
            using var incidentTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var incidentReference = await RegisterCheckoutPostCommitIncidentAsync(
                services,
                pedido,
                cliente,
                "GENESIS",
                genesisResult.FailureCode ?? "GENESIS-UNKNOWN",
                "Integracao financeira Genesis pendente",
                "A conta a receber do pedido deve ser reconciliada com o financeiro Genesis.",
                http,
                incidentTimeout.Token);
            genesisOperationalAlert = $"Integracao financeira pendente sob a referencia {incidentReference}.";
        }
        catch (Exception incidentException)
        {
            checkoutLogger.LogCritical(
                incidentException,
                "Pedido {PedidoId} ({NumeroPedido}) foi commitado, mas o incidente Genesis nao foi persistido.",
                pedido.Id,
                pedido.NumeroPedido);
            return Results.Problem(
                detail: $"O pedido {pedido.NumeroPedido} foi registrado, mas a integracao financeira e o registro do incidente falharam. Nao repita a compra; informe este numero ao atendimento.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Pedido registrado com falha operacional critica");
        }
    }

    try
    {
        await notificacaoService.EnviarConfirmacaoPedidoAsync(cliente, pedido, ct);
    }
    catch (Exception notificationException)
    {
        var failureCode = BuildOperationalFailureCode(notificationException);
        checkoutLogger.LogError(
            notificationException,
            "Confirmacao externa nao enviada para o pedido {PedidoId} ({NumeroPedido}). Codigo {FailureCode}.",
            pedido.Id,
            pedido.NumeroPedido,
            failureCode);

        try
        {
            using var incidentTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var incidentReference = await RegisterCheckoutPostCommitIncidentAsync(
                services,
                pedido,
                cliente,
                "NOTIFICACAO",
                failureCode,
                "Confirmacao externa do pedido pendente",
                "O pedido foi registrado, mas a confirmacao por e-mail ainda precisa ser reenviada.",
                http,
                incidentTimeout.Token);
            notificationOperationalAlert = $"Confirmacao por e-mail pendente sob a referencia {incidentReference}.";
        }
        catch (Exception incidentException)
        {
            checkoutLogger.LogCritical(
                incidentException,
                "Pedido {PedidoId} ({NumeroPedido}) foi commitado, mas o incidente de notificacao nao foi persistido.",
                pedido.Id,
                pedido.NumeroPedido);
            return Results.Problem(
                detail: $"O pedido {pedido.NumeroPedido} foi registrado, mas a confirmacao externa e o registro do incidente falharam. Nao repita a compra; informe este numero ao atendimento.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Pedido registrado com falha operacional critica");
        }

        if (notificationException is OperationCanceledException && ct.IsCancellationRequested)
        {
            throw;
        }
    }

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

    var checkoutOperationalAlert = string.Join(
        " ",
        new[] { fiscalOperationalAlert, genesisOperationalAlert, notificationOperationalAlert }
            .Where(alert => !string.IsNullOrWhiteSpace(alert)));
    if (string.IsNullOrWhiteSpace(checkoutOperationalAlert))
    {
        checkoutOperationalAlert = null;
    }

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
        pagamentoFinal?.BoletoUrl,
        checkoutFiscalStatus?.ToString(),
        checkoutOperationalAlert);

    var checkoutResponse = ApiResponse<PedidoLojaDto>.Ok(
        dto,
        checkoutOperationalAlert ?? "Pedido criado com sucesso.");
    return checkoutOperationalAlert is null
        ? Results.Ok(checkoutResponse)
        : Results.Json(checkoutResponse, statusCode: StatusCodes.Status202Accepted);
})
.AllowAnonymous()
.WithName("CriarPedido")
;

app.MapGet("/api/dropshipping/canais", [Authorize(Policy = "Gerente")] async (
    NexumDbContext db,
    IConfiguration configuration,
    bool? ativo,
    string? busca,
    CancellationToken ct) =>
{
    var query = db.DropshippingConfigs.AsQueryable();
    if (ativo.HasValue)
    {
        query = query.Where(config => config.Ativo == ativo.Value);
    }

    var termo = TrimOrNull(busca);
    if (termo is not null)
    {
        query = query.Where(config => config.Nome.Contains(termo) || config.Slug.Contains(termo));
    }

    var canais = await query.OrderBy(config => config.Nome).ToListAsync(ct);
    var dados = canais.Select(config => ToDropshippingCanalDto(db, configuration, config)).ToList();
    return Results.Ok(ApiResponse<List<DropshippingCanalDto>>.Ok(dados, "Canais de dropshipping carregados.", dados.Count));
})
.WithName("DropshippingCanaisListar");

app.MapGet("/api/dropshipping/canais/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    NexumDbContext db,
    IConfiguration configuration,
    CancellationToken ct) =>
{
    var canal = await db.DropshippingConfigs.FirstOrDefaultAsync(config => config.Id == id, ct);
    return canal is null
        ? Results.NotFound(ApiResponse<DropshippingCanalDto>.Erro("Canal de dropshipping nao encontrado."))
        : Results.Ok(ApiResponse<DropshippingCanalDto>.Ok(ToDropshippingCanalDto(db, configuration, canal)));
})
.WithName("DropshippingCanaisObter");

app.MapPost("/api/dropshipping/canais", [Authorize(Policy = "Gerente")] async (
    DropshippingCanalRequest request,
    NexumDbContext db,
    IConfiguration configuration,
    CancellationToken ct) =>
{
    var validation = ValidateDropshippingCanalRequest(request, configuration, out var nome, out var slug, out var tipo);
    if (validation is not null)
    {
        return validation.Value.StatusCode == StatusCodes.Status424FailedDependency
            ? Results.Json(ApiResponse<DropshippingCanalDto>.Erro(validation.Value.Message), statusCode: validation.Value.StatusCode)
            : Results.BadRequest(ApiResponse<DropshippingCanalDto>.Erro(validation.Value.Message));
    }

    if (await db.DropshippingConfigs.AnyAsync(config => config.Slug == slug, ct))
    {
        return Results.Conflict(ApiResponse<DropshippingCanalDto>.Erro("Ja existe canal com o slug informado."));
    }

    var canal = new DropshippingConfig
    {
        Nome = nome,
        Slug = slug,
        Tipo = tipo,
        ApiEndpoint = TrimOrNull(request.ApiEndpoint),
        Ativo = request.Ativo
    };
    db.DropshippingConfigs.Add(canal);
    await db.SaveChangesAsync(ct);

    var response = ToDropshippingCanalDto(db, configuration, canal);
    return Results.Created($"/api/dropshipping/canais/{canal.Id}", ApiResponse<DropshippingCanalDto>.Ok(response, "Canal de dropshipping criado."));
})
.WithName("DropshippingCanaisCriar");

app.MapPut("/api/dropshipping/canais/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    DropshippingCanalRequest request,
    NexumDbContext db,
    IConfiguration configuration,
    CancellationToken ct) =>
{
    if (!TryDecodeRowVersion(request.RowVersion, out var expectedRowVersion))
    {
        return Results.BadRequest(ApiResponse<DropshippingCanalDto>.Erro("RowVersion valido e obrigatorio para atualizar o canal."));
    }

    var canal = await db.DropshippingConfigs.FirstOrDefaultAsync(config => config.Id == id, ct);
    if (canal is null)
    {
        return Results.NotFound(ApiResponse<DropshippingCanalDto>.Erro("Canal de dropshipping nao encontrado."));
    }

    var validation = ValidateDropshippingCanalRequest(request, configuration, out var nome, out var slug, out var tipo);
    if (validation is not null)
    {
        return validation.Value.StatusCode == StatusCodes.Status424FailedDependency
            ? Results.Json(ApiResponse<DropshippingCanalDto>.Erro(validation.Value.Message), statusCode: validation.Value.StatusCode)
            : Results.BadRequest(ApiResponse<DropshippingCanalDto>.Erro(validation.Value.Message));
    }

    if (await db.DropshippingConfigs.AnyAsync(config => config.Id != id && config.Slug == slug, ct))
    {
        return Results.Conflict(ApiResponse<DropshippingCanalDto>.Erro("Ja existe canal com o slug informado."));
    }
=======
app.Run();
>>>>>>> 0b859b83b843d38e6b609297964ac23e3fe66577

// Helper Methods
static string GetReleaseVersion() => "1.1.5.7225";

static string ResolveJwtSecret(IConfiguration config)
{
<<<<<<< HEAD
    var campanha = await db.CrmCampanhas
        .AsNoTracking()
        .Include(item => item.Segmento)
        .FirstOrDefaultAsync(item => item.Id == id, ct);

    return campanha is null
        ? Results.NotFound(ApiResponse<string>.Erro("Campanha nao encontrada."))
        : Results.Ok(ApiResponse<CrmCampanhaDto>.Ok(ToCrmCampanhaDto(campanha)));
})
.WithName("CrmCampanhasObter")
;

app.MapPost("/api/crm/campanhas", [Authorize(Policy = "Gerente")] async (
    CrmCampanhaRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var validation = ValidateCrmCampanhaRequest(request, null, out var tipo, out var status);
    if (validation is not null)
    {
        return Results.BadRequest(ApiResponse<string>.Erro(validation));
    }

    if (request.SegmentoId.HasValue && !await db.CrmSegmentos.AnyAsync(item => item.Id == request.SegmentoId.Value && item.Ativo, ct))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Segmento ativo nao encontrado no tenant atual."));
    }

    var nome = request.Nome.Trim();
    if (await db.CrmCampanhas.AnyAsync(item => item.Nome == nome, ct))
    {
        return Results.Conflict(ApiResponse<string>.Erro("Ja existe campanha com este nome no tenant atual."));
    }

    var campanha = new CrmCampanha();
    ApplyCrmCampanhaRequest(campanha, request, nome, tipo, status);
    db.CrmCampanhas.Add(campanha);
    await db.SaveChangesAsync(ct);
    await db.Entry(campanha).Reference(item => item.Segmento).LoadAsync(ct);

    return Results.Created(
        $"/api/crm/campanhas/{campanha.Id}",
        ApiResponse<CrmCampanhaDto>.Ok(ToCrmCampanhaDto(campanha), "Campanha gravada no banco de dados."));
})
.WithName("CrmCampanhasCriar")
;

app.MapPut("/api/crm/campanhas/{id:guid}", [Authorize(Policy = "Gerente")] async (
    Guid id,
    CrmCampanhaRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (!TryDecodeRowVersion(request.RowVersion, out var expectedRowVersion))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("RowVersion valido e obrigatorio para atualizar a campanha."));
    }

    var campanha = await db.CrmCampanhas.Include(item => item.Segmento).FirstOrDefaultAsync(item => item.Id == id, ct);
    if (campanha is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Campanha nao encontrada."));
    }

    var validation = ValidateCrmCampanhaRequest(request, campanha.Status, out var tipo, out var status);
    if (validation is not null)
    {
        return Results.BadRequest(ApiResponse<string>.Erro(validation));
    }

    if (request.SegmentoId.HasValue && !await db.CrmSegmentos.AnyAsync(item => item.Id == request.SegmentoId.Value && item.Ativo, ct))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Segmento ativo nao encontrado no tenant atual."));
    }

    var nome = request.Nome.Trim();
    if (await db.CrmCampanhas.AnyAsync(item => item.Id != id && item.Nome == nome, ct))
    {
        return Results.Conflict(ApiResponse<string>.Erro("Ja existe campanha com este nome no tenant atual."));
    }

    db.Entry(campanha).Property(item => item.RowVersion).OriginalValue = expectedRowVersion;
    ApplyCrmCampanhaRequest(campanha, request, nome, tipo, status);

    try
    {
        await db.SaveChangesAsync(ct);
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict(ApiResponse<string>.Erro("A campanha foi alterada por outro usuario. Recarregue os dados antes de salvar."));
    }

    await db.Entry(campanha).Reference(item => item.Segmento).LoadAsync(ct);
    return Results.Ok(ApiResponse<CrmCampanhaDto>.Ok(ToCrmCampanhaDto(campanha), "Campanha atualizada no banco de dados."));
})
.WithName("CrmCampanhasAtualizar")
;

app.MapDelete("/api/crm/campanhas/{id:guid}", [Authorize(Policy = "Gerente")] async (
    Guid id,
    NexumDbContext db,
    CancellationToken ct) =>
{
    var campanha = await db.CrmCampanhas.FirstOrDefaultAsync(item => item.Id == id, ct);
    if (campanha is null)
    {
        return Results.NotFound(ApiResponse<string>.Erro("Campanha nao encontrada."));
    }

    db.CrmCampanhas.Remove(campanha);
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
})
.WithName("CrmCampanhasExcluir")
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
            WHERE tenant_id = {0} AND is_deleted = 0
            ORDER BY created_at DESC
            LIMIT 500
            """,
            db.CurrentTenantId.ToString())
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
        BuildLeadNotificationEmail(lead),
        ct);

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

app.MapGet("/api/erp/empresas/{id:int}", [Authorize(Policy = "Gerente")] async (int id, NexumDbContext db, CancellationToken ct) =>
{
    var empresa = await db.EmpresasGrupo
        .AsNoTracking()
        .SingleOrDefaultAsync(item => item.Id == id, ct);
    return empresa is null
        ? Results.NotFound(ApiResponse<EmpresaGrupoDto>.Erro("Empresa do grupo nao encontrada."))
        : Results.Ok(ApiResponse<EmpresaGrupoDto>.Ok(MapearEmpresaGrupoDto(empresa), "Empresa do grupo carregada."));
})
.WithName("EmpresaGrupoDetalhar")
;

app.MapPut("/api/erp/empresas/{id:int}", [Authorize(Policy = "Gerente")] async (
    int id,
    EmpresaGrupoRequest request,
    NexumDbContext db,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.RazaoSocial) || string.IsNullOrWhiteSpace(request.Cnpj))
    {
        return Results.BadRequest(ApiResponse<EmpresaGrupoDto>.Erro("Razao social e CNPJ sao obrigatorios."));
    }

    var cnpj = NormalizeDocument(request.Cnpj);
    if (string.IsNullOrWhiteSpace(cnpj))
    {
        return Results.BadRequest(ApiResponse<EmpresaGrupoDto>.Erro("CNPJ invalido."));
    }

    var empresa = await db.EmpresasGrupo.FirstOrDefaultAsync(item => item.Id == id, ct);
    if (empresa is null)
    {
        return Results.NotFound(ApiResponse<EmpresaGrupoDto>.Erro("Empresa do grupo nao encontrada."));
    }

    var codigoEmpresa = TrimOrNull(request.CodigoEmpresa);
    var duplicada = await db.EmpresasGrupo.AsNoTracking().FirstOrDefaultAsync(item =>
        item.Id != id
        && (item.Cnpj == cnpj
            || (!string.IsNullOrWhiteSpace(codigoEmpresa) && item.CodigoEmpresa == codigoEmpresa)), ct);
    if (duplicada is not null)
    {
        return Results.BadRequest(ApiResponse<EmpresaGrupoDto>.Erro("Ja existe outra empresa com o CNPJ ou codigo interno informado."));
    }

    AplicarEmpresaGrupoRequest(empresa, request, cnpj);
    empresa.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync(ct);
    return Results.Ok(ApiResponse<EmpresaGrupoDto>.Ok(MapearEmpresaGrupoDto(empresa), "Empresa do grupo atualizada."));
})
.WithName("AtualizarEmpresaGrupo")
;

app.MapDelete("/api/erp/empresas/{id:int}", [Authorize(Policy = "Gerente")] async (int id, NexumDbContext db, CancellationToken ct) =>
{
    var empresa = await db.EmpresasGrupo.FirstOrDefaultAsync(item => item.Id == id, ct);
    if (empresa is null)
    {
        return Results.NotFound(ApiResponse<EmpresaGrupoDto>.Erro("Empresa do grupo nao encontrada."));
    }

    empresa.Ativa = false;
    empresa.EmitentePreferencial = false;
    empresa.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync(ct);
    return Results.Ok(ApiResponse<EmpresaGrupoDto>.Ok(MapearEmpresaGrupoDto(empresa), "Empresa do grupo desativada."));
})
.WithName("DesativarEmpresaGrupo")
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

app.MapGet("/api/pdv/cockpit", [Authorize(Policy = "Gerente")] async (NexumDbContext db, CancellationToken ct) =>
{
    var hoje = DateTime.UtcNow.Date;
    var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

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

    var pedidosHoje = await db.Pedidos.AsNoTracking().CountAsync(item => item.CreatedAt >= hoje, ct);
    var faturamentoHoje = await db.Pedidos.AsNoTracking()
        .Where(item => item.CreatedAt >= hoje && item.Status != StatusPedido.Cancelado && item.Status != StatusPedido.Reembolsado)
        .SumAsync(item => (decimal?)item.Total, ct) ?? 0m;
    var faturamentoMes = await db.Pedidos.AsNoTracking()
        .Where(item => item.CreatedAt >= inicioMes && item.Status != StatusPedido.Cancelado && item.Status != StatusPedido.Reembolsado)
        .SumAsync(item => (decimal?)item.Total, ct) ?? 0m;
    var vendasPresenciaisHoje = await db.Pedidos.AsNoTracking()
        .CountAsync(item => item.CreatedAt >= hoje && (item.Origem == OrigemPedido.Mobile || item.Origem == OrigemPedido.API || item.MeioPagamento == "Dinheiro" || item.MeioPagamento == "CartaoPDV" || item.MeioPagamento == "Cartão PDV"), ct);
    var produtosAtivos = await db.Produtos.AsNoTracking().CountAsync(item => item.Ativo, ct);
    var produtosComCodigoFisico = await db.Produtos.AsNoTracking()
        .CountAsync(item => item.Ativo && item.CodigoBarras != null && item.CodigoBarras != "" && item.QrCode != null && item.QrCode != "", ct);
    var fiscalPdvPendente = await db.Fiscais.AsNoTracking()
        .CountAsync(item => (item.ModeloDocumento == "NFCe" || item.ModeloDocumento == "SAT" || item.ModeloDocumento == "MFe") && item.StatusNfe != StatusNfe.Autorizada && item.StatusNfe != StatusNfe.Cancelada, ct);
    var financeiroCaixaAberto = await db.Financeiros.AsNoTracking()
        .CountAsync(item => item.Status == StatusLancamento.Pendente && (item.MeioPagamento == "Dinheiro" || item.MeioPagamento == "Pix" || item.MeioPagamento == "Cartao" || item.MeioPagamento == "Cartão" || item.MeioPagamento == "CartaoPDV"), ct);

    var pendencias = new List<PdvPendenciaDto>();
    AddPdvPendencia(pendencias, configuracoes.Count == 0 ? 1 : 0, "Sem empresa habilitada para PDV", "Cadastre ou habilite ao menos uma empresa ativa com emissão de saída.", "critico", "erp-empresas");
    AddPdvPendencia(pendencias, configuracoes.Count(item => string.IsNullOrWhiteSpace(item.SerieNfce) && string.Equals(item.ModeloDocumentoPdv, "NFCe", StringComparison.OrdinalIgnoreCase)), "NFC-e sem série", "Empresas NFC-e precisam de série e numeração antes do caixa real.", "alta", "erp-empresas");
    AddPdvPendencia(pendencias, configuracoes.Count(item => !item.PossuiCscConfigurado && string.Equals(item.ModeloDocumentoPdv, "NFCe", StringComparison.OrdinalIgnoreCase)), "CSC NFC-e ausente", "Configure CSC e ID Token das empresas que irão emitir NFC-e.", "alta", "erp-empresas");
    AddPdvPendencia(pendencias, produtosAtivos - produtosComCodigoFisico, "Produtos sem leitura física", "Itens ativos precisam de código de barras e QR para operação de caixa.", "alta", "cadastro-produtos");
    AddPdvPendencia(pendencias, fiscalPdvPendente, "Fiscal PDV pendente", "Documentos NFC-e/SAT/MFe ainda precisam de autorização ou contingência.", "media", "erp-fiscal");
    AddPdvPendencia(pendencias, financeiroCaixaAberto, "Caixa financeiro pendente", "Lançamentos de caixa aguardam baixa ou conciliação.", "media", "erp-financeiro");

    if (pendencias.Count == 0)
    {
        pendencias.Add(new PdvPendenciaDto("PDV operacional", "Configuração fiscal, produtos e caixa não retornaram pendências críticas neste momento.", "ok", "erp-pdv"));
    }

    var indicadores = new List<PdvIndicadorDto>
    {
        new("empresas", "Empresas PDV", configuracoes.Count.ToString(CultureInfo.InvariantCulture), $"{configuracoes.Count(item => item.PdvContingenciaOffline)} com contingência offline", BuildHealthStatus(configuracoes.Count == 0 ? 1 : 0), "erp-empresas"),
        new("vendasHoje", "Pedidos hoje", pedidosHoje.ToString(CultureInfo.InvariantCulture), $"Presencial/API: {vendasPresenciaisHoje}", "ok", "pedidos"),
        new("faturamentoHoje", "Faturamento hoje", faturamentoHoje.ToString("C", CultureInfo.GetCultureInfo("pt-BR")), $"Mês: {faturamentoMes.ToString("C", CultureInfo.GetCultureInfo("pt-BR"))}", "ok", "erp-financeiro"),
        new("produtosFisicos", "Itens com leitura", $"{produtosComCodigoFisico}/{produtosAtivos}", "Código de barras e QR Code para operação de caixa", BuildHealthStatus(produtosAtivos - produtosComCodigoFisico), "cadastro-produtos"),
        new("fiscalPdv", "Fiscal PDV pendente", fiscalPdvPendente.ToString(CultureInfo.InvariantCulture), "NFC-e/SAT/MFe em fila ou contingência", BuildHealthStatus(fiscalPdvPendente), "erp-fiscal"),
        new("caixa", "Caixa a conciliar", financeiroCaixaAberto.ToString(CultureInfo.InvariantCulture), "Recebimentos de balcão, PIX, cartão e dinheiro", BuildHealthStatus(financeiroCaixaAberto), "erp-financeiro")
    };

    return Results.Ok(ApiResponse<PdvCockpitDto>.Ok(new PdvCockpitDto(configuracoes, indicadores, pendencias, DateTime.UtcNow)));
})
.WithName("PdvCockpit")
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

    if (novoStatus is StatusNfe.Emitida or StatusNfe.Autorizada &&
        (string.IsNullOrWhiteSpace(fiscal.ChaveAcesso) ||
         string.IsNullOrWhiteSpace(fiscal.Protocolo) ||
         fiscal.DataEmissao is null))
    {
        return Results.BadRequest(ApiResponse<string>.Erro("Status fiscal de emissao/autorizacao exige chave, protocolo e data gerados por provedor SEFAZ real. Use /api/fiscal/nfe/emitir ou /api/fiscal/nfce/emitir."));
    }

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
        await notificacaoService.EnviarNotaFiscalEmitidaAsync(fiscal.Pedido.Cliente, fiscal.Pedido, fiscal, ct);
        fiscal.EmailClienteNotificadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    return Results.Ok(ApiResponse<string>.Ok("Status fiscal atualizado com sucesso."));
})
.WithName("AtualizarStatusFiscal")
;

app.MapPost("/api/fiscal/nfe/emitir", [Authorize(Policy = "Fiscal")] async (
    FiscalEmissaoRequest request,
    IConfiguration configuration,
    NexumDbContext db,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct) =>
    await EmitirDocumentoFiscalAsync("NFe", request, configuration, db, httpClientFactory, ct))
.WithName("FiscalNfeEmitir")
;

app.MapPost("/api/fiscal/nfce/emitir", [Authorize(Policy = "Fiscal")] async (
    FiscalEmissaoRequest request,
    IConfiguration configuration,
    NexumDbContext db,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct) =>
    await EmitirDocumentoFiscalAsync("NFCe", request, configuration, db, httpClientFactory, ct))
.WithName("FiscalNfceEmitir")
;

app.MapPost("/api/fiscal/nfe/cancelar", [Authorize(Policy = "Fiscal")] async (
    FiscalOperacaoRequest request,
    IConfiguration configuration,
    NexumDbContext db,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct) =>
    await ExecutarOperacaoDocumentoFiscalAsync("NFe", "cancelar", request, configuration, db, httpClientFactory, ct))
.WithName("FiscalNfeCancelar")
;

app.MapPost("/api/fiscal/nfce/cancelar", [Authorize(Policy = "Fiscal")] async (
    FiscalOperacaoRequest request,
    IConfiguration configuration,
    NexumDbContext db,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct) =>
    await ExecutarOperacaoDocumentoFiscalAsync("NFCe", "cancelar", request, configuration, db, httpClientFactory, ct))
.WithName("FiscalNfceCancelar")
;

app.MapPost("/api/fiscal/nfe/inutilizar", [Authorize(Policy = "Fiscal")] async (
    FiscalOperacaoRequest request,
    IConfiguration configuration,
    NexumDbContext db,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct) =>
    await ExecutarOperacaoDocumentoFiscalAsync("NFe", "inutilizar", request, configuration, db, httpClientFactory, ct))
.WithName("FiscalNfeInutilizar")
;

app.MapPost("/api/fiscal/nfce/inutilizar", [Authorize(Policy = "Fiscal")] async (
    FiscalOperacaoRequest request,
    IConfiguration configuration,
    NexumDbContext db,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct) =>
    await ExecutarOperacaoDocumentoFiscalAsync("NFCe", "inutilizar", request, configuration, db, httpClientFactory, ct))
.WithName("FiscalNfceInutilizar")
;

app.MapPost("/api/fiscal/nfe/cartacorrecao", [Authorize(Policy = "Fiscal")] async (
    FiscalOperacaoRequest request,
    IConfiguration configuration,
    NexumDbContext db,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct) =>
    await ExecutarOperacaoDocumentoFiscalAsync("NFe", "cartacorrecao", request, configuration, db, httpClientFactory, ct))
.WithName("FiscalNfeCartaCorrecao")
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

static async Task<IResult> EmitirDocumentoFiscalAsync(
    string modeloDocumento,
    FiscalEmissaoRequest request,
    IConfiguration configuration,
    NexumDbContext db,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct)
{
    var modelo = NormalizeFiscalModel(modeloDocumento);
    var fiscal = await LoadFiscalAggregateAsync(db, request.FiscalId, request.PedidoId, ct);
    if (fiscal is null && request.PedidoId is not null)
    {
        fiscal = await CreateFiscalFromPedidoAsync(db, request.PedidoId.Value, request.EmpresaGrupoId, modelo, ct);
    }

    if (fiscal is null)
    {
        return Results.NotFound(ApiResponse<FiscalOperacaoResultadoDto>.Erro("Informe um fiscal_id ou pedido_id existente para emissao fiscal."));
    }

    if (fiscal.Pedido is null)
    {
        return Results.BadRequest(ApiResponse<FiscalOperacaoResultadoDto>.Erro("Registro fiscal sem pedido vinculado. Corrija o cadastro antes da emissao."));
    }

    if (fiscal.StatusNfe == StatusNfe.Autorizada && request.ForceReissue != true)
    {
        return Results.Conflict(ApiResponse<FiscalOperacaoResultadoDto>.Erro("Documento fiscal ja autorizado. Reemissao exige force_reissue=true e decisao operacional registrada."));
    }

    var empresa = await ResolveFiscalCompanyAsync(db, fiscal, request.EmpresaGrupoId, ct);
    var certificado = TestCertificadoNFe(configuration);
    var pendencias = ValidateFiscalEmission(fiscal, fiscal.Pedido, empresa, certificado, modelo);
    var provider = ResolveFiscalProviderConfiguration(configuration, modelo, "emitir");
    pendencias.AddRange(provider.Pendencias);

    if (pendencias.Count > 0)
    {
        var bloqueio = BuildFiscalOperationResult(
            false,
            fiscal,
            modelo,
            "emitir",
            provider.Provider,
            provider.Endpoint?.Host,
            null,
            pendencias,
            DateTime.UtcNow);

        fiscal.StatusAutomacao = "Emissao bloqueada por pendencia operacional real.";
        fiscal.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Results.Json(
            new ApiResponse<FiscalOperacaoResultadoDto>(false, "Emissao fiscal bloqueada. Nenhum sucesso foi registrado.", bloqueio, pendencias),
            statusCode: StatusCodes.Status424FailedDependency);
    }

    var payload = BuildFiscalEmissionPayload(fiscal, fiscal.Pedido, empresa!, certificado, modelo, request);
    var providerResult = await SendFiscalProviderRequestAsync(provider, payload, httpClientFactory, ct);

    if (!providerResult.HttpSucceeded)
    {
        var falha = BuildFiscalOperationResult(
            false,
            fiscal,
            modelo,
            "emitir",
            provider.Provider,
            provider.Endpoint?.Host,
            providerResult.StatusCode,
            providerResult.Pendencias,
            DateTime.UtcNow);

        fiscal.StatusAutomacao = "Provedor fiscal recusou a emissao.";
        fiscal.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Results.Json(
            new ApiResponse<FiscalOperacaoResultadoDto>(false, "Provedor fiscal recusou a emissao. Banco preservado sem sucesso fiscal.", falha, providerResult.Pendencias),
            statusCode: StatusCodes.Status502BadGateway);
    }

    var parsed = ParseFiscalProviderResponse(providerResult.Body);
    ApplyFiscalEmissionResult(fiscal, empresa!, modelo, parsed, providerResult);
    await db.SaveChangesAsync(ct);

    var resultado = BuildFiscalOperationResult(
        true,
        fiscal,
        modelo,
        "emitir",
        provider.Provider,
        provider.Endpoint?.Host,
        providerResult.StatusCode,
        parsed.Pendencias,
        DateTime.UtcNow);

    return Results.Ok(ApiResponse<FiscalOperacaoResultadoDto>.Ok(resultado, fiscal.StatusNfe == StatusNfe.Autorizada
        ? "Documento fiscal autorizado por provedor externo."
        : "Documento fiscal transmitido ao provedor externo."));
=======
    return config["JwtSettings:SecretKey"]
        ?? config["JwtSettings__SecretKey"]
        ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
        ?? "S3cur3S3cr3tK3yF0rJ3wT1A2B3C4D5E6F7G8H9I0!";
>>>>>>> 0b859b83b843d38e6b609297964ac23e3fe66577
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
<<<<<<< HEAD
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

static bool IsGeneratedProductQrCode(
    string qrCode,
    string sku,
    string nome,
    string? tipoProduto,
    int? fornecedorId)
{
    try
    {
        using var document = JsonDocument.Parse(qrCode);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("tipo", out var tipo)
            || tipo.ValueKind != JsonValueKind.String
            || !root.TryGetProperty("sku", out var skuValue)
            || skuValue.ValueKind != JsonValueKind.String
            || !root.TryGetProperty("nome", out var nomeValue)
            || nomeValue.ValueKind != JsonValueKind.String
            || !root.TryGetProperty("aquisicao", out var aquisicao)
            || aquisicao.ValueKind != JsonValueKind.String
            || !root.TryGetProperty("fornecedorId", out var fornecedor)
            || !root.TryGetProperty("criadoEm", out var criadoEm))
        {
            return false;
        }

        var fornecedorCorreto = fornecedorId.HasValue
            ? fornecedor.ValueKind == JsonValueKind.Number
                && fornecedor.TryGetInt32(out var fornecedorJson)
                && fornecedorJson == fornecedorId.Value
            : fornecedor.ValueKind == JsonValueKind.Null;
        var dataCriacaoValida = criadoEm.ValueKind == JsonValueKind.String
            && DateTimeOffset.TryParse(
                criadoEm.GetString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);

        return tipo.GetString() == "PRODUTO"
            && skuValue.GetString() == sku
            && nomeValue.GetString() == nome
            && aquisicao.GetString() == ObterCodigoAquisicao(tipoProduto, fornecedorId)
            && fornecedorCorreto
            && dataCriacaoValida;
    }
    catch (JsonException)
    {
        return false;
    }
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
=======
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO ops_ordem_servico_itens
                (tenant_id, oso_id, osi_tipo, osi_codigo, osi_descricao, osi_quantidade, osi_unidade, osi_custo_unitario, osi_total)
            VALUES
                ({tenantId.ToString()}, {ordemId}, {NormalizeBusinessKey(item.Tipo)}, {NormalizeBusinessKey(item.Codigo)}, {TrimOrNull(item.Descricao)}, {item.Quantidade}, {NormalizeBusinessKey(item.Unidade)}, {item.CustoUnitario}, {item.Quantidade * item.CustoUnitario})
            """, ct);
>>>>>>> 0b859b83b843d38e6b609297964ac23e3fe66577
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
<<<<<<< HEAD
    if (string.IsNullOrWhiteSpace(value))
    {
        return null;
    }

    var digits = new string(value.Where(char.IsDigit).ToArray());
    return digits.Length == 0 ? null : digits;
}

static string? TrimOrNull(string? value) =>
    string.IsNullOrWhiteSpace(value) ? null : value.Trim();

static bool TryDecodeProdutoRowVersion(string? value, out byte[] rowVersion)
{
    rowVersion = Array.Empty<byte>();
    if (value is null)
    {
        return false;
    }

    try
    {
        rowVersion = Convert.FromBase64String(value);
        return rowVersion.Length is 0 or 16;
    }
    catch (FormatException)
    {
        return false;
    }
}

static string? ValidateCupomAdminRequest(CupomAdminRequest request)
{
    var codigo = request.Codigo?.Trim() ?? string.Empty;
    if (codigo.Length is < 3 or > 50)
    {
        return "Codigo do cupom deve ter entre 3 e 50 caracteres.";
    }

    if (codigo.Any(character => !char.IsLetterOrDigit(character) && character is not '-' and not '_'))
    {
        return "Codigo do cupom aceita somente letras, numeros, hifen e sublinhado.";
    }

    if (!Enum.TryParse<TipoCupom>(request.Tipo, true, out var tipo))
    {
        return "Tipo de cupom invalido.";
    }

    if (tipo == TipoCupom.Percentual && (request.Valor <= 0m || request.Valor > 100m))
    {
        return "Cupom percentual deve ter valor maior que zero e menor ou igual a 100.";
    }

    if (tipo == TipoCupom.ValorFixo && request.Valor <= 0m)
    {
        return "Cupom de valor fixo deve ter valor maior que zero.";
    }

    if (tipo == TipoCupom.FreteGratis && request.Valor != 0m)
    {
        return "Cupom de frete gratis deve ter valor zero; o desconto sera o frete selecionado.";
    }

    if (request.ValorMinimoPedido < 0m || request.ValorMaximoDesconto is <= 0m)
    {
        return "Valores minimo e maximo de desconto devem ser validos.";
    }

    if (request.QuantidadeUsos is <= 0 || request.QuantidadePorCliente < 0)
    {
        return "Limites de uso devem ser maiores que zero; use zero apenas para remover o limite por cliente.";
    }

    if (request.ValidoDe.HasValue && request.ValidoAte.HasValue && request.ValidoAte.Value < request.ValidoDe.Value)
    {
        return "Data final da vigencia deve ser igual ou posterior a data inicial.";
    }

    return null;
}

static void ApplyCupomAdminRequest(Cupom cupom, CupomAdminRequest request, string codigo)
{
    cupom.Codigo = codigo;
    cupom.Tipo = Enum.Parse<TipoCupom>(request.Tipo, true);
    cupom.Valor = request.Valor;
    cupom.ValorMinimoPedido = request.ValorMinimoPedido;
    cupom.ValorMaximoDesconto = request.ValorMaximoDesconto;
    cupom.QuantidadeUsos = request.QuantidadeUsos;
    cupom.QuantidadePorCliente = request.QuantidadePorCliente;
    cupom.ValidoDe = request.ValidoDe;
    cupom.ValidoAte = request.ValidoAte;
    cupom.PrimeiroCompraOnly = request.PrimeiroCompraOnly;
    cupom.Ativo = request.Ativo;
    cupom.UpdatedAt = DateTime.UtcNow;
}

static CupomAdminDto ToCupomAdminDto(Cupom cupom) => new(
    cupom.Id,
    cupom.Codigo,
    cupom.Tipo.ToString(),
    cupom.Valor,
    cupom.ValorMinimoPedido,
    cupom.ValorMaximoDesconto,
    cupom.QuantidadeUsos,
    cupom.UsosAtuais,
    cupom.QuantidadePorCliente,
    cupom.ValidoDe,
    cupom.ValidoAte,
    cupom.PrimeiroCompraOnly,
    cupom.Ativo,
    cupom.CreatedAt,
    cupom.UpdatedAt);

static string? ValidateCrmSegmentoRequest(CrmSegmentoRequest request)
{
    var nome = request.Nome?.Trim() ?? string.Empty;
    if (nome.Length is < 3 or > 100)
    {
        return "Nome do segmento deve ter entre 3 e 100 caracteres.";
    }

    var cor = request.Cor?.Trim() ?? string.Empty;
    if (cor.Length != 7 || cor[0] != '#' || !cor.Skip(1).All(Uri.IsHexDigit))
    {
        return "Cor do segmento deve usar o formato hexadecimal #RRGGBB.";
    }

    if (request.Prioridade is < 1 or > 999)
    {
        return "Prioridade do segmento deve estar entre 1 e 999.";
    }

    if (request.TicketMedioMinimo is < 0m || request.TicketMedioMaximo is < 0m)
    {
        return "Faixas de ticket medio nao podem ser negativas.";
    }

    if (request.TicketMedioMinimo.HasValue
        && request.TicketMedioMaximo.HasValue
        && request.TicketMedioMaximo < request.TicketMedioMinimo)
    {
        return "Ticket medio maximo deve ser igual ou superior ao minimo.";
    }

    if (request.FrequenciaMinimaDias is < 0 || request.FrequenciaMaximaDias is < 0)
    {
        return "Frequencias de compra nao podem ser negativas.";
    }

    if (request.FrequenciaMinimaDias.HasValue
        && request.FrequenciaMaximaDias.HasValue
        && request.FrequenciaMaximaDias < request.FrequenciaMinimaDias)
    {
        return "Frequencia maxima deve ser igual ou superior a minima.";
    }

    return null;
}

static void ApplyCrmSegmentoRequest(CrmSegmento segmento, CrmSegmentoRequest request, string nome)
{
    segmento.Nome = nome;
    segmento.Descricao = TrimOrNull(request.Descricao);
    segmento.Cor = request.Cor.Trim().ToUpperInvariant();
    segmento.Prioridade = request.Prioridade;
    segmento.TicketMedioMinimo = request.TicketMedioMinimo;
    segmento.TicketMedioMaximo = request.TicketMedioMaximo;
    segmento.FrequenciaMinimaDias = request.FrequenciaMinimaDias;
    segmento.FrequenciaMaximaDias = request.FrequenciaMaximaDias;
    segmento.Ativo = request.Ativo;
}

static CrmSegmentoDto ToCrmSegmentoDto(CrmSegmento segmento) => new(
    segmento.Id,
    segmento.Nome,
    segmento.Descricao,
    segmento.Cor,
    segmento.Prioridade,
    segmento.TicketMedioMinimo,
    segmento.TicketMedioMaximo,
    segmento.FrequenciaMinimaDias,
    segmento.FrequenciaMaximaDias,
    segmento.Ativo,
    Convert.ToBase64String(segmento.RowVersion),
    segmento.CreatedAt,
    segmento.UpdatedAt);

static string? ValidateCrmCampanhaRequest(
    CrmCampanhaRequest request,
    StatusCampanha? currentStatus,
    out TipoCampanha tipo,
    out StatusCampanha status)
{
    tipo = default;
    status = default;

    var nome = request.Nome?.Trim() ?? string.Empty;
    if (nome.Length is < 3 or > 200)
    {
        return "Nome da campanha deve ter entre 3 e 200 caracteres.";
    }

    if (!Enum.TryParse(request.Tipo, true, out tipo))
    {
        return "Tipo de campanha invalido.";
    }

    if (!Enum.TryParse(request.Status, true, out status))
    {
        return "Status de campanha invalido.";
    }

    if (currentStatus is null && status is not (StatusCampanha.Rascunho or StatusCampanha.Agendada))
    {
        return "Campanha nova deve iniciar como Rascunho ou Agendada.";
    }

    if (currentStatus.HasValue && !IsValidCampaignStatusTransition(currentStatus.Value, status))
    {
        return $"Transicao de {currentStatus.Value} para {status} nao permitida.";
    }

    if (request.DataInicio == default)
    {
        return "Data de inicio da campanha obrigatoria.";
    }

    if (request.DataFim.HasValue && request.DataFim.Value < request.DataInicio)
    {
        return "Data final deve ser igual ou posterior a data inicial.";
    }

    if (request.Orcamento < 0m || request.CustoAtual < 0m || request.ReceitaGerada < 0m)
    {
        return "Valores financeiros da campanha nao podem ser negativos.";
    }

    if (request.Alcance < 0
        || request.Cliques < 0
        || request.LeadsGerados < 0
        || request.OportunidadesGeradas < 0
        || request.VendasGeradas < 0)
    {
        return "Metricas da campanha nao podem ser negativas.";
    }

    if (request.Cliques > request.Alcance && request.Alcance > 0)
    {
        return "Cliques nao podem superar o alcance informado.";
    }

    if (request.VendasGeradas > request.OportunidadesGeradas && request.OportunidadesGeradas > 0)
    {
        return "Vendas geradas nao podem superar as oportunidades informadas.";
    }

    if ((status is StatusCampanha.Agendada or StatusCampanha.EmAndamento)
        && (string.IsNullOrWhiteSpace(request.PublicoAlvo) || string.IsNullOrWhiteSpace(request.Conteudo)))
    {
        return "Campanha agendada ou em andamento exige publico-alvo e conteudo.";
    }

    return null;
}

static bool IsValidCampaignStatusTransition(StatusCampanha current, StatusCampanha next)
{
    if (current == next)
    {
        return true;
    }

    return current switch
    {
        StatusCampanha.Rascunho => next is StatusCampanha.Agendada or StatusCampanha.EmAndamento or StatusCampanha.Cancelada,
        StatusCampanha.Agendada => next is StatusCampanha.EmAndamento or StatusCampanha.Pausada or StatusCampanha.Cancelada,
        StatusCampanha.EmAndamento => next is StatusCampanha.Pausada or StatusCampanha.Concluida or StatusCampanha.Cancelada,
        StatusCampanha.Pausada => next is StatusCampanha.EmAndamento or StatusCampanha.Concluida or StatusCampanha.Cancelada,
        StatusCampanha.Concluida or StatusCampanha.Cancelada => false,
        _ => false
    };
}

static void ApplyCrmCampanhaRequest(
    CrmCampanha campanha,
    CrmCampanhaRequest request,
    string nome,
    TipoCampanha tipo,
    StatusCampanha status)
{
    campanha.Nome = nome;
    campanha.Descricao = TrimOrNull(request.Descricao);
    campanha.Tipo = tipo;
    campanha.Status = status;
    campanha.SegmentoId = request.SegmentoId;
    campanha.DataInicio = request.DataInicio;
    campanha.DataFim = request.DataFim;
    campanha.Orcamento = request.Orcamento;
    campanha.CustoAtual = request.CustoAtual;
    campanha.Alcance = request.Alcance;
    campanha.Cliques = request.Cliques;
    campanha.LeadsGerados = request.LeadsGerados;
    campanha.OportunidadesGeradas = request.OportunidadesGeradas;
    campanha.VendasGeradas = request.VendasGeradas;
    campanha.ReceitaGerada = request.ReceitaGerada;
    campanha.PublicoAlvo = TrimOrNull(request.PublicoAlvo);
    campanha.Conteudo = TrimOrNull(request.Conteudo);
}

static CrmCampanhaDto ToCrmCampanhaDto(CrmCampanha campanha) => new(
    campanha.Id,
    campanha.Nome,
    campanha.Descricao,
    campanha.Tipo.ToString(),
    campanha.Status.ToString(),
    campanha.SegmentoId,
    campanha.Segmento?.Nome,
    campanha.DataInicio,
    campanha.DataFim,
    campanha.Orcamento,
    campanha.CustoAtual,
    campanha.Alcance,
    campanha.Cliques,
    campanha.LeadsGerados,
    campanha.OportunidadesGeradas,
    campanha.VendasGeradas,
    campanha.ReceitaGerada,
    campanha.CustoAtual > 0m ? decimal.Round(campanha.ReceitaGerada / campanha.CustoAtual, 4) : 0m,
    campanha.Cliques > 0 ? decimal.Round(campanha.CustoAtual / campanha.Cliques, 4) : 0m,
    campanha.LeadsGerados > 0 ? decimal.Round(campanha.CustoAtual / campanha.LeadsGerados, 4) : 0m,
    campanha.VendasGeradas > 0 ? decimal.Round(campanha.CustoAtual / campanha.VendasGeradas, 4) : 0m,
    campanha.PublicoAlvo,
    campanha.Conteudo,
    Convert.ToBase64String(campanha.RowVersion),
    campanha.CreatedAt,
    campanha.UpdatedAt);

static bool TryDecodeRowVersion(string? value, out byte[] rowVersion)
{
    rowVersion = Array.Empty<byte>();
    if (string.IsNullOrWhiteSpace(value))
    {
        return false;
    }

    try
    {
        rowVersion = Convert.FromBase64String(value.Trim());
        return rowVersion.Length > 0;
    }
    catch (FormatException)
    {
        return false;
    }
}

static bool TryResolveRedisEndpoint(string connectionString, out string host, out int port, out string? error)
{
    host = string.Empty;
    port = 6379;
    error = null;

    var endpoint = connectionString
        .Split(',', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        .FirstOrDefault();

    if (string.IsNullOrWhiteSpace(endpoint))
    {
        error = "Connection string Redis vazia.";
        return false;
    }

    if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) && !string.IsNullOrWhiteSpace(uri.Host))
    {
        host = uri.Host;
        port = uri.IsDefaultPort ? 6379 : uri.Port;
        return true;
    }

    var atIndex = endpoint.LastIndexOf('@');
    if (atIndex >= 0 && atIndex + 1 < endpoint.Length)
    {
        endpoint = endpoint[(atIndex + 1)..];
    }

    var colonIndex = endpoint.LastIndexOf(':');
    if (colonIndex > 0 && colonIndex + 1 < endpoint.Length && int.TryParse(endpoint[(colonIndex + 1)..], out var parsedPort))
    {
        host = endpoint[..colonIndex];
        port = parsedPort;
    }
    else
    {
        host = endpoint;
    }

    if (string.IsNullOrWhiteSpace(host))
    {
        error = "Host Redis ausente.";
        return false;
    }

    if (port <= 0 || port > 65535)
    {
        error = "Porta Redis invalida.";
        return false;
    }

    return true;
}

static bool ValidateDesktopTerminalAccess(
    HttpRequest request,
    IConfiguration configuration,
    out string terminalIdentity,
    out string rejection)
{
    var terminal = request.Headers["X-Nexum-Terminal"].FirstOrDefault();
    var store = request.Headers["X-Nexum-Store"].FirstOrDefault();
    var token = request.Headers["X-Nexum-Desktop-Token"].FirstOrDefault();
    var remoteIp = request.HttpContext.Connection.RemoteIpAddress;
    var tokens = GetConfiguredDesktopTokens(configuration);

    terminalIdentity = $"{TrimOrNull(store) ?? "loja-nao-informada"}:{TrimOrNull(terminal) ?? "terminal-nao-informado"}";
    rejection = string.Empty;

    if (tokens.Count > 0)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            rejection = "Token de terminal ausente.";
            return false;
        }

        if (!tokens.Contains(token.Trim()))
        {
            rejection = "Token de terminal inválido.";
            return false;
        }

        terminalIdentity = $"{terminalIdentity}:token-validado";
        return true;
    }

    if (IsPrivateOrLoopbackAddress(remoteIp))
    {
        terminalIdentity = $"{terminalIdentity}:rede-interna-sem-token";
        return true;
    }

    rejection = "Acesso desktop público exige token configurado no servidor.";
    return false;
}

static HashSet<string> GetConfiguredDesktopTokens(IConfiguration configuration)
{
    var tokens = new HashSet<string>(StringComparer.Ordinal);
    var masterToken = TrimOrNull(configuration["DesktopAccess:MasterToken"]);
    if (masterToken is not null)
    {
        tokens.Add(masterToken);
    }

    foreach (var token in configuration.GetSection("DesktopAccess:TerminalTokens").Get<string[]>() ?? Array.Empty<string>())
    {
        var value = TrimOrNull(token);
        if (value is not null)
        {
            tokens.Add(value);
        }
    }

    return tokens;
}

static bool IsPrivateOrLoopbackAddress(IPAddress? address)
{
    if (address is null)
    {
        return false;
    }

    if (IPAddress.IsLoopback(address))
    {
        return true;
    }

    if (address.IsIPv4MappedToIPv6)
    {
        address = address.MapToIPv4();
    }

    var bytes = address.GetAddressBytes();
    if (bytes.Length != 4)
    {
        return false;
    }

    return bytes[0] == 10
        || bytes[0] == 192 && bytes[1] == 168
        || bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31;
}

static async Task ReplaceOpsOrdemItensAsync(NexumDbContext db, Guid tenantId, int ordemServicoId, List<OpsOrdemServicoItemRequest>? itens, CancellationToken ct)
{
    await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM ops_ordem_servico_itens WHERE tenant_id = {tenantId.ToString()} AND oso_id = {ordemServicoId}", ct);

    foreach (var item in itens ?? [])
    {
        var tipo = NormalizeBusinessKey(item.Tipo) ?? "SERVICO";
        var descricao = TrimOrNull(item.Descricao);
        if (string.IsNullOrWhiteSpace(descricao) || item.Quantidade <= 0)
        {
            continue;
        }

        var unidade = NormalizeBusinessKey(item.Unidade) ?? "UN";
        var total = item.CustoUnitario.HasValue ? item.Quantidade * item.CustoUnitario.Value : (decimal?)null;
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO ops_ordem_servico_itens
                (tenant_id, oso_id, osi_tipo, osi_codigo, osi_descricao, osi_quantidade, osi_unidade, osi_custo_unitario, osi_total)
            VALUES
                ({tenantId.ToString()}, {ordemServicoId}, {tipo}, {NormalizeBusinessKey(item.Codigo)}, {descricao}, {item.Quantidade}, {unidade}, {item.CustoUnitario}, {total})
            """,
            ct);
    }
}
static int GetCurrentUserId(ClaimsPrincipal principal)
{
    var idRaw = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? "0";

    return int.TryParse(idRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? id : 0;
}

static Guid? GetCurrentUserGuidOrNull(ClaimsPrincipal principal)
{
    var idRaw = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

    return Guid.TryParse(idRaw, out var id) && id != Guid.Empty ? id : null;
}

static async Task<WorkflowDefinicaoDto?> LoadWorkflowDefinitionAsync(
    NexumDbContext db,
    Guid tenantId,
    Guid definitionId,
    CancellationToken ct)
{
    var definitions = await db.Database.SqlQueryRaw<WorkflowDefinicaoDto>(
=======
    return await db.Database.SqlQueryRaw<WorkflowDefinicaoDto>(
>>>>>>> 0b859b83b843d38e6b609297964ac23e3fe66577
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
<<<<<<< HEAD
        """,
        instanceId.ToString(),
        tenantId.ToString())
        .ToListAsync(ct);
    return instances.SingleOrDefault();
}

static WorkflowDefinicaoNormalizada? NormalizeWorkflowDefinition(
    WorkflowDefinicaoRequest request,
    out string validationError)
{
    validationError = string.Empty;
    var entidade = NormalizeBusinessKey(request.Entidade);
    var codigo = NormalizeBusinessKey(request.Codigo);
    var nome = TrimOrNull(request.Nome);
    if (string.IsNullOrWhiteSpace(entidade) || string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nome))
    {
        validationError = "Entidade, codigo e nome sao obrigatorios.";
        return null;
    }
    if (entidade.Length > 120 || codigo.Length > 100 || nome.Length > 180)
    {
        validationError = "Entidade, codigo ou nome excede o limite permitido.";
        return null;
    }

    var estados = (request.Estados ?? [])
        .Select(NormalizeBusinessKey)
        .Where(item => !string.IsNullOrWhiteSpace(item))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();
    if (estados.Count < 2 || estados.Count > 50)
    {
        validationError = "A definicao deve possuir entre 2 e 50 estados distintos.";
        return null;
    }
    if (estados.Any(estado => estado.Length > 80))
    {
        validationError = "Cada estado deve ter no maximo 80 caracteres.";
        return null;
    }

    var transicoes = new List<WorkflowTransicaoRegraDto>();
    foreach (var regra in request.Transicoes ?? [])
    {
        var origem = NormalizeBusinessKey(regra.Origem);
        var destino = NormalizeBusinessKey(regra.Destino);
        var acao = NormalizeBusinessKey(regra.Acao);
        if (string.IsNullOrWhiteSpace(origem) || string.IsNullOrWhiteSpace(destino) || string.IsNullOrWhiteSpace(acao))
        {
            validationError = "Toda transicao deve informar origem, destino e acao.";
            return null;
        }
        if (!estados.Contains(origem, StringComparer.OrdinalIgnoreCase)
            || !estados.Contains(destino, StringComparer.OrdinalIgnoreCase))
        {
            validationError = $"A transicao {origem} -> {destino} referencia um estado inexistente.";
            return null;
        }
        if (acao.Length > 80)
        {
            validationError = "Cada acao de transicao deve ter no maximo 80 caracteres.";
            return null;
        }

        var perfis = (regra.PerfisAutorizados ?? [])
            .Select(TrimOrNull)
            .Where(perfil => !string.IsNullOrWhiteSpace(perfil))
            .Select(perfil => perfil!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (perfis.Any(perfil => perfil.Length > 80))
        {
            validationError = "Cada perfil autorizado deve ter no maximo 80 caracteres.";
            return null;
        }

        transicoes.Add(new WorkflowTransicaoRegraDto(origem, destino, acao, perfis.Count == 0 ? null : perfis));
    }

    if (transicoes.Count == 0 || transicoes.Count > 250)
    {
        validationError = "A definicao deve possuir entre 1 e 250 transicoes.";
        return null;
    }

    var duplicateTransition = transicoes
        .GroupBy(regra => $"{regra.Origem}\u001f{regra.Destino}\u001f{regra.Acao}", StringComparer.OrdinalIgnoreCase)
        .FirstOrDefault(group => group.Count() > 1);
    if (duplicateTransition is not null)
    {
        validationError = "A definicao possui transicoes duplicadas para a mesma origem, destino e acao.";
        return null;
    }

    var estadosAlcancaveis = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { estados[0] };
    var pendentes = new Queue<string>();
    pendentes.Enqueue(estados[0]);
    while (pendentes.TryDequeue(out var origemAtual))
    {
        foreach (var destino in transicoes
                     .Where(regra => string.Equals(regra.Origem, origemAtual, StringComparison.OrdinalIgnoreCase))
                     .Select(regra => regra.Destino))
        {
            if (estadosAlcancaveis.Add(destino))
            {
                pendentes.Enqueue(destino);
            }
        }
    }

    var estadosInalcancaveis = estados.Where(estado => !estadosAlcancaveis.Contains(estado)).ToList();
    if (estadosInalcancaveis.Count > 0)
    {
        validationError = $"Estados sem caminho a partir do estado inicial {estados[0]}: {string.Join(", ", estadosInalcancaveis)}.";
        return null;
    }

    return new WorkflowDefinicaoNormalizada(entidade, codigo, nome, estados, transicoes, request.Ativo);
}

static bool TryParseWorkflowDefinition(
    WorkflowDefinicaoDto definition,
    out WorkflowDefinicaoNormalizada? normalized,
    out string validationError)
{
    normalized = null;
    try
    {
        var estados = JsonSerializer.Deserialize<List<string>>(definition.EstadosJson);
        var transicoes = JsonSerializer.Deserialize<List<WorkflowTransicaoRegraDto>>(definition.TransicoesJson);
        if (estados is null || transicoes is null)
        {
            validationError = "Os estados ou as transicoes armazenados nao possuem formato valido.";
            return false;
        }

        normalized = NormalizeWorkflowDefinition(
            new WorkflowDefinicaoRequest(
                definition.Entidade,
                definition.Codigo,
                definition.Nome,
                estados,
                transicoes,
                definition.Ativo),
            out validationError);
        return normalized is not null;
    }
    catch (JsonException ex)
    {
        validationError = $"JSON de workflow invalido: {ex.Message}";
        return false;
    }
}

static bool IsWorkflowProfileAuthorized(ClaimsPrincipal principal, IReadOnlyCollection<string>? authorizedProfiles)
{
    if (authorizedProfiles is null || authorizedProfiles.Count == 0)
    {
        return true;
    }

    var currentProfiles = principal.Claims
        .Where(claim => claim.Type == ClaimTypes.Role || claim.Type == "perfil")
        .Select(claim => claim.Value)
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
    return authorizedProfiles.Any(currentProfiles.Contains);
}

static UsuarioAcessoDto ToUsuarioAcessoDto(Usuario usuario) =>
    new(
        usuario.Id,
        usuario.Nome,
        usuario.Email,
        usuario.Perfil.ToString(),
        usuario.Ativo,
        usuario.Telefone,
        usuario.UltimoLogin,
        usuario.UpdatedAt);

static LogAuditoria CreateIamAuditLog(
    ClaimsPrincipal principal,
    HttpContext httpContext,
    string tabela,
    int registroId,
    AcaoAuditoria acao,
    object? dadosAnteriores,
    object? dadosNovos)
{
    var usuarioId = GetCurrentUserId(principal);
    var userAgent = httpContext.Request.Headers.UserAgent.ToString();
    return new LogAuditoria
    {
        Tabela = tabela,
        RegistroId = registroId,
        Acao = acao,
        UsuarioId = usuarioId > 0 ? usuarioId : null,
        UsuarioTipo = TipoUsuarioAuditoria.Usuario,
        IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
        UserAgent = userAgent.Length <= 255 ? userAgent : userAgent[..255],
        DadosAnteriores = dadosAnteriores is null ? null : JsonSerializer.Serialize(dadosAnteriores),
        DadosNovos = dadosNovos is null ? null : JsonSerializer.Serialize(dadosNovos),
        Endpoint = httpContext.Request.Path.Value,
        CreatedAt = DateTime.UtcNow
    };
}

static Task<T> ExecuteWithDatabaseStrategyAsync<T>(NexumDbContext db, Func<Task<T>> operation) =>
    db.Database.CreateExecutionStrategy().ExecuteAsync(operation);

static async Task<PerfilAcessoDto?> LoadPerfilAsync(NexumDbContext db, int id, CancellationToken ct) =>
    await db.Database.SqlQueryRaw<PerfilAcessoDto>(
        """
        SELECT
            prf_id AS Id,
            prf_nome AS Nome,
            prf_descricao AS Descricao,
            prf_alcada_maxima AS AlcadaMaxima,
            prf_nivel_hierarquico AS NivelHierarquico,
            prf_ativo AS Ativo,
            prf_data_cadastro AS CriadoEm
        FROM adm_perfis
        WHERE prf_id = {0}
        """,
        id)
        .FirstOrDefaultAsync(ct);

static async Task<PermissaoAcessoDto?> LoadPermissaoAsync(NexumDbContext db, int id, CancellationToken ct) =>
    await db.Database.SqlQueryRaw<PermissaoAcessoDto>(
        """
        SELECT
            prm_id AS Id,
            prm_modulo AS Modulo,
            prm_funcionalidade AS Funcionalidade,
            prm_chave AS Chave,
            prm_descricao AS Descricao,
            prm_ativo AS Ativo
        FROM adm_permissoes
        WHERE prm_id = {0}
        """,
        id)
        .FirstOrDefaultAsync(ct);

static async Task<List<PerfilPermissaoDto>> LoadPerfilPermissoesAsync(NexumDbContext db, int perfilId, CancellationToken ct) =>
    await db.Database.SqlQueryRaw<PerfilPermissaoDto>(
        """
        SELECT
            ppr.ppr_id AS Id,
            ppr.ppr_perfil_id AS PerfilId,
            ppr.ppr_permissao_id AS PermissaoId,
            prm.prm_modulo AS Modulo,
            prm.prm_funcionalidade AS Funcionalidade,
            prm.prm_chave AS Chave,
            ppr.ppr_leitura AS Leitura,
            ppr.ppr_escrita AS Escrita,
            ppr.ppr_exclusao AS Exclusao,
            ppr.ppr_impressao AS Impressao
        FROM adm_perfil_permissoes ppr
        INNER JOIN adm_permissoes prm ON prm.prm_id = ppr.ppr_permissao_id
        WHERE ppr.ppr_perfil_id = {0}
        ORDER BY prm.prm_modulo, prm.prm_funcionalidade, prm.prm_chave
        """,
        perfilId)
        .ToListAsync(ct);

static async Task<PessoaMasterDataDto?> LoadPessoaAsync(NexumDbContext db, int id, CancellationToken ct) =>
    await db.Database.SqlQueryRaw<PessoaMasterDataDto>(
        """
        SELECT
            pes_id AS Id,
            pes_tipo AS Tipo,
            pes_nome_razao AS NomeRazao,
            pes_nome_fantasia AS NomeFantasia,
            pes_cpf_cnpj AS CpfCnpj,
            pes_rg_ie AS RgIe,
            pes_cliente AS Cliente,
            pes_fornecedor AS Fornecedor,
            pes_colaborador AS Colaborador,
            pes_transportadora AS Transportadora,
            pes_email AS Email,
            pes_telefone AS Telefone,
            pes_celular AS Celular,
            pes_cidade AS Cidade,
            pes_uf AS Uf,
            pes_ativo AS Ativo,
            pes_data_cadastro AS CriadoEm,
            pes_data_atualizacao AS AtualizadoEm
        FROM adm_pessoas_empresas
        WHERE pes_id = {0}
        """,
        id)
        .FirstOrDefaultAsync(ct);

static async Task<CentroCustoDto?> LoadCentroCustoAsync(NexumDbContext db, int id, CancellationToken ct) =>
    await db.Database.SqlQueryRaw<CentroCustoDto>(
        """
        SELECT
            ccu_id AS Id,
            ccu_codigo AS Codigo,
            ccu_nome AS Nome,
            ccu_descricao AS Descricao,
            ccu_tipo AS Tipo,
            ccu_pai_id AS PaiId,
            ccu_responsavel_usr_id AS ResponsavelUsuarioId,
            ccu_status AS Status,
            ccu_data_cadastro AS CriadoEm,
            ccu_data_alteracao AS AtualizadoEm
        FROM fin_centros_custo
        WHERE ccu_id = {0} AND ccu_data_exclusao IS NULL
        """,
        id)
        .FirstOrDefaultAsync(ct);

static async Task<ItemServicoDto?> LoadItemServicoAsync(NexumDbContext db, int id, CancellationToken ct) =>
    await db.Database.SqlQueryRaw<ItemServicoDto>(
        """
        SELECT
            itm_id AS Id,
            itm_emp_id AS EmpresaId,
            itm_codigo AS Codigo,
            itm_tipo AS Tipo,
            itm_descricao AS Descricao,
            itm_descricao_detalhada AS DescricaoDetalhada,
            itm_unidade AS Unidade,
            itm_ncm AS Ncm,
            itm_cest AS Cest,
            itm_controla_estoque AS ControlaEstoque,
            itm_controla_lote AS ControlaLote,
            itm_controla_serie AS ControlaSerie,
            itm_ativo AS Ativo,
            itm_data_cadastro AS CriadoEm
        FROM vnd_itens
        WHERE itm_id = {0}
        """,
        id)
        .FirstOrDefaultAsync(ct);

static async Task<FornecedorContatoDto?> LoadFornecedorContatoAsync(NexumDbContext db, int id, CancellationToken ct) =>
    await db.Database.SqlQueryRaw<FornecedorContatoDto>(
        """
        SELECT
            fco_id AS Id,
            fco_fornecedor_id AS FornecedorId,
            fco_nome AS Nome,
            fco_cargo AS Cargo,
            fco_email AS Email,
            fco_telefone AS Telefone,
            fco_celular AS Celular,
            fco_principal AS Principal,
            fco_ativo AS Ativo,
            fco_atualizado_em AS AtualizadoEm,
            TO_BASE64(fco_row_version) AS RowVersion
        FROM md_fornecedor_contatos
        WHERE fco_id = {0}
          AND fco_tenant_id = {1}
          AND fco_is_deleted = 0
        """,
        id,
        db.CurrentTenantId.ToString())
        .FirstOrDefaultAsync(ct);

static async Task<ContabilLancamentoDto?> LoadContabilLancamentoAsync(NexumDbContext db, int id, CancellationToken ct) =>
    await db.Database.SqlQueryRaw<ContabilLancamentoDto>(
        """
        SELECT
            lcn_id AS Id,
            lcn_emp_id AS EmpresaId,
            lcn_lote AS Lote,
            lcn_sublote AS Sublote,
            lcn_data AS Data,
            lcn_historico_padrao AS HistoricoPadrao,
            lcn_complemento AS Complemento,
            lcn_valor AS Valor,
            lcn_tipo AS Tipo,
            lcn_origem_modulo AS OrigemModulo,
            lcn_origem_id AS OrigemId,
            lcn_estornado AS Estornado,
            lcn_lcn_estorno_id AS LancamentoEstornoId,
            lcn_usr_cadastro AS UsuarioCadastroId,
            lcn_data_cadastro AS CriadoEm
        FROM cnt_lancamentos
        WHERE lcn_id = {0}
        """,
        id)
        .FirstOrDefaultAsync(ct);

static void AplicarEmpresaGrupoRequest(EmpresaGrupo empresa, EmpresaGrupoRequest request, string cnpj)
{
    empresa.TipoCadastro = TrimOrNull(request.TipoCadastro) ?? "GrupoSocietario";
    empresa.RazaoSocial = request.RazaoSocial.Trim();
    empresa.NomeFantasia = TrimOrNull(request.NomeFantasia);
    empresa.Cnpj = cnpj;
    empresa.InscricaoEstadual = TrimOrNull(request.InscricaoEstadual);
    empresa.InscricaoMunicipal = TrimOrNull(request.InscricaoMunicipal);
    empresa.MatrizFilial = TrimOrNull(request.MatrizFilial);
    empresa.CodigoEmpresa = TrimOrNull(request.CodigoEmpresa);
    empresa.RegimeTributario = TrimOrNull(request.RegimeTributario);
    empresa.Crt = TrimOrNull(request.Crt);
    empresa.CnaePrincipal = TrimOrNull(request.CnaePrincipal);
    empresa.CnaesSecundarios = TrimOrNull(request.CnaesSecundarios);
    empresa.CategoriaFiscal = TrimOrNull(request.CategoriaFiscal);
    empresa.SubcategoriaFiscal = TrimOrNull(request.SubcategoriaFiscal);
    empresa.NcmPadrao = TrimOrNull(request.NcmPadrao);
    empresa.NaturezaOperacaoPadrao = TrimOrNull(request.NaturezaOperacaoPadrao);
    empresa.ResponsavelLegal = TrimOrNull(request.ResponsavelLegal);
    empresa.ResponsavelFiscal = TrimOrNull(request.ResponsavelFiscal);
    empresa.EmailFiscal = NormalizeEmail(request.EmailFiscal);
    empresa.EmailComercial = NormalizeEmail(request.EmailComercial);
    empresa.Telefone = NormalizePhone(request.Telefone);
    empresa.Whatsapp = NormalizePhone(request.Whatsapp);
    empresa.Cep = TrimOrNull(request.Cep);
    empresa.Logradouro = TrimOrNull(request.Logradouro);
    empresa.Numero = TrimOrNull(request.Numero);
    empresa.Complemento = TrimOrNull(request.Complemento);
    empresa.Bairro = TrimOrNull(request.Bairro);
    empresa.Cidade = TrimOrNull(request.Cidade);
    empresa.Estado = TrimOrNull(request.Estado)?.ToUpperInvariant();
    empresa.Pais = TrimOrNull(request.Pais) ?? "Brasil";
    empresa.AmbienteNfe = TrimOrNull(request.AmbienteNfe);
    empresa.SerieNfe = TrimOrNull(request.SerieNfe);
    empresa.SerieNfce = TrimOrNull(request.SerieNfce);
    empresa.ModeloDocumentoPdv = TrimOrNull(request.ModeloDocumentoPdv) ?? "NFCe";
    empresa.AmbienteNfce = TrimOrNull(request.AmbienteNfce) ?? TrimOrNull(request.AmbienteNfe);
    empresa.ProximaNfceNumero = request.ProximaNfceNumero;
    empresa.NfceCsc = TrimOrNull(request.NfceCsc);
    empresa.NfceCscIdToken = TrimOrNull(request.NfceCscIdToken);
    empresa.PdvSerieSat = TrimOrNull(request.PdvSerieSat);
    empresa.PdvImpressoraFiscal = TrimOrNull(request.PdvImpressoraFiscal);
    empresa.PdvNomeCaixaPadrao = TrimOrNull(request.PdvNomeCaixaPadrao);
    empresa.PdvContingenciaOffline = request.PdvContingenciaOffline ?? false;
    empresa.ProximaNfeNumero = request.ProximaNfeNumero;
    empresa.CfopPadraoInterno = TrimOrNull(request.CfopPadraoInterno);
    empresa.CfopPadraoInterestadual = TrimOrNull(request.CfopPadraoInterestadual);
    empresa.AliquotaIcmsInterna = request.AliquotaIcmsInterna;
    empresa.AliquotaIcmsInterestadual = request.AliquotaIcmsInterestadual;
    empresa.AliquotaPis = request.AliquotaPis;
    empresa.AliquotaCofins = request.AliquotaCofins;
    empresa.AliquotaIss = request.AliquotaIss;
    empresa.AliquotaIpi = request.AliquotaIpi;
    empresa.CargaTributariaPercentual = request.CargaTributariaPercentual;
    empresa.PerfilTributacao = TrimOrNull(request.PerfilTributacao) ?? "TributacaoAtual";
    empresa.UsaStLegado = request.UsaStLegado ?? false;
    empresa.DestacaIcmsStSeparado = request.DestacaIcmsStSeparado ?? false;
    empresa.CustoOperacionalPercentual = request.CustoOperacionalPercentual;
    empresa.MargemMinimaPercentual = request.MargemMinimaPercentual;
    empresa.PrioridadeFiscal = request.PrioridadeFiscal ?? 100;
    empresa.PermiteNfeEntrada = request.PermiteNfeEntrada ?? true;
    empresa.PermiteNfeSaida = request.PermiteNfeSaida ?? true;
    empresa.PermiteDropshipping = request.PermiteDropshipping ?? false;
    empresa.PermiteMarketplace = request.PermiteMarketplace ?? false;
    empresa.EmitentePreferencial = request.EmitentePreferencial ?? false;
    empresa.Ativa = request.Ativa ?? true;
    empresa.BeneficiosEstrategicos = TrimOrNull(request.BeneficiosEstrategicos);
    empresa.ContratoResumo = TrimOrNull(request.ContratoResumo);
    empresa.Observacoes = TrimOrNull(request.Observacoes);
}

static EmpresaGrupoDto MapearEmpresaGrupoDto(EmpresaGrupo empresa) =>
    new(
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

static string NormalizeBusinessKey(string? value)
{
    var normalized = TrimOrNull(value)?.ToUpperInvariant();
    return string.IsNullOrWhiteSpace(normalized) ? string.Empty : normalized;
}

static string NormalizeBusinessCode(string? value)
{
    var normalized = TrimOrNull(value)?.ToUpperInvariant();
    return string.IsNullOrWhiteSpace(normalized) ? string.Empty : normalized;
}

static string? OnlyDigitsOrNull(string? value)
{
    var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
    return string.IsNullOrWhiteSpace(digits) ? null : digits;
}

static string? NormalizeUf(string? value)
{
    var uf = TrimOrNull(value)?.ToUpperInvariant();
    return string.IsNullOrWhiteSpace(uf) ? null : uf[..Math.Min(2, uf.Length)];
}

static string? NormalizePessoaTipo(string? value)
{
    var normalized = NormalizeBusinessKey(value);
    return normalized switch
    {
        "F" or "PF" or "FISICA" or "PESSOA_FISICA" => "FISICA",
        "J" or "PJ" or "JURIDICA" or "PESSOA_JURIDICA" => "JURIDICA",
        _ => null
    };
}

static string NormalizeCentroCustoTipo(string? value)
{
    var normalized = NormalizeBusinessKey(value);
    return normalized switch
    {
        "LUCRO" => "LUCRO",
        "INVESTIMENTO" => "INVESTIMENTO",
        "OPERACIONAL" => "OPERACIONAL",
        "ADMINISTRATIVO" => "ADMINISTRATIVO",
        _ => "CUSTO"
    };
}

static string NormalizeStatusChar(string? value)
{
    var normalized = NormalizeBusinessKey(value);
    return normalized switch
    {
        "I" or "INATIVO" or "INATIVA" => "I",
        _ => "A"
    };
}

static string? NormalizeItemTipo(string? value)
{
    var normalized = NormalizeBusinessKey(value);
    return normalized switch
    {
        "PRODUTO" => "PRODUTO",
        "SERVICO" or "SERVIÇO" => "SERVICO",
        "MATERIA_PRIMA" or "MATERIAPRIMA" or "MP" => "MATERIA_PRIMA",
        "INSUMO" => "INSUMO",
        _ => null
    };
}

static string NormalizeContabilLancamentoTipo(string? value)
{
    var normalized = NormalizeBusinessKey(value);
    return normalized == "AUTOMATICO" || normalized == "AUTOMÁTICO" ? "AUTOMATICO" : "MANUAL";
}

static string? NormalizePartidaTipo(string? value)
{
    var normalized = NormalizeBusinessKey(value);
    return normalized switch
    {
        "D" or "DEBITO" or "DÉBITO" => "DEBITO",
        "C" or "CREDITO" or "CRÉDITO" => "CREDITO",
        _ => null
    };
}

static string? NormalizeConciliacaoStatus(string? value)
{
    var normalized = NormalizeBusinessKey(value);
    return normalized switch
    {
        "" => null,
        "PENDENTE" => "PENDENTE",
        "CONCILIADO" or "CONCILIADA" => "CONCILIADO",
        "DIVERGENTE" => "DIVERGENTE",
        "IGNORADO" or "IGNORADA" => "IGNORADO",
        _ => null
    };
}

static async Task<IResult> ListarConciliacoesFinanceirasAsync(
    DateTime? inicio,
    DateTime? fim,
    string? status,
    NexumDbContext db,
    ITenantContext tenantContext,
    CancellationToken ct)
{
    if (inicio.HasValue && fim.HasValue && fim.Value.Date < inicio.Value.Date)
    {
        return Results.BadRequest(ApiResponse<List<ConciliacaoFinanceiraDto>>.Erro("Data final nao pode ser anterior a data inicial."));
    }

    if (inicio.HasValue && fim.HasValue && (fim.Value.Date - inicio.Value.Date).TotalDays > 366)
    {
        return Results.BadRequest(ApiResponse<List<ConciliacaoFinanceiraDto>>.Erro("Periodo de conciliacao deve ter no maximo 366 dias."));
    }

    var normalizedStatus = NormalizeConciliacaoStatus(status);
    if (!string.IsNullOrWhiteSpace(status) && normalizedStatus is null)
    {
        return Results.BadRequest(ApiResponse<List<ConciliacaoFinanceiraDto>>.Erro("Status deve ser PENDENTE, CONCILIADO, DIVERGENTE ou IGNORADO."));
    }

    var conciliacoes = await db.Database.SqlQueryRaw<ConciliacaoFinanceiraDto>(
        """
        SELECT
            f.id AS LancamentoFinanceiroId,
            f.descricao AS Descricao,
            f.valor AS Valor,
            f.data_pagamento AS DataPagamento,
            f.meio_pagamento AS MeioPagamento,
            f.conta_bancaria AS ContaBancaria,
            CASE
                WHEN f.status = 'Pago' THEN COALESCE(c.cnc_status, 'PENDENTE')
                ELSE 'NAO_APLICAVEL'
            END AS Status,
            c.cnc_id AS ConciliacaoId,
            c.cnc_referencia_bancaria AS ReferenciaBancaria,
            c.cnc_observacoes AS Observacoes,
            c.cnc_data_conciliacao AS DataConciliacao,
            CASE WHEN c.row_version IS NULL THEN NULL ELSE LOWER(HEX(c.row_version)) END AS RowVersion
        FROM financeiro f
        LEFT JOIN ctb_conciliacoes c
          ON c.cnc_financeiro_id = f.id
         AND c.tenant_id = f.tenant_id
         AND c.is_deleted = 0
        WHERE f.tenant_id = {3}
          AND f.is_deleted = 0
          AND ({0} IS NULL OR f.data_pagamento >= {0})
          AND ({1} IS NULL OR f.data_pagamento < {1})
          AND ({2} IS NULL OR CASE
                WHEN f.status = 'Pago' THEN COALESCE(c.cnc_status, 'PENDENTE')
                ELSE 'NAO_APLICAVEL'
              END = {2})
        ORDER BY COALESCE(f.data_pagamento, f.created_at) DESC, f.id DESC
        LIMIT 500
        """,
        (object?)inicio?.Date ?? DBNull.Value,
        (object?)fim?.Date.AddDays(1) ?? DBNull.Value,
        (object?)normalizedStatus ?? DBNull.Value,
        tenantContext.TenantId.ToString())
        .ToListAsync(ct);

    return Results.Ok(ApiResponse<List<ConciliacaoFinanceiraDto>>.Ok(
        conciliacoes,
        "Conciliacao financeira carregada do banco oficial.",
        conciliacoes.Count));
}

static async Task<IResult> RegistrarConciliacaoFinanceiraAsync(
    ConciliacaoFinanceiraRequest request,
    NexumDbContext db,
    ClaimsPrincipal principal,
    ITenantContext tenantContext,
    HttpContext httpContext,
    CancellationToken ct)
{
    if (request.LancamentoFinanceiroId <= 0)
    {
        return Results.BadRequest(ApiResponse<ConciliacaoFinanceiraDto>.Erro("Lancamento financeiro e obrigatorio."));
    }

    var normalizedStatus = NormalizeConciliacaoStatus(request.Status);
    if (normalizedStatus is null)
    {
        return Results.BadRequest(ApiResponse<ConciliacaoFinanceiraDto>.Erro("Status deve ser PENDENTE, CONCILIADO, DIVERGENTE ou IGNORADO."));
    }

    var referencia = TrimOrNull(request.ReferenciaBancaria);
    var observacoes = TrimOrNull(request.Observacoes);
    if (referencia?.Length > 120)
    {
        return Results.BadRequest(ApiResponse<ConciliacaoFinanceiraDto>.Erro("Referencia bancaria deve ter no maximo 120 caracteres."));
    }

    if (observacoes?.Length > 2000)
    {
        return Results.BadRequest(ApiResponse<ConciliacaoFinanceiraDto>.Erro("Observacoes devem ter no maximo 2000 caracteres."));
    }

    if (normalizedStatus == "CONCILIADO" && string.IsNullOrWhiteSpace(referencia))
    {
        return Results.BadRequest(ApiResponse<ConciliacaoFinanceiraDto>.Erro("Referencia bancaria e obrigatoria para concluir a conciliacao."));
    }

    var lancamento = await db.Database.SqlQueryRaw<FinanceiroConciliavelRow>(
        """
        SELECT id AS Id, status AS Status, data_pagamento AS DataPagamento
        FROM financeiro
        WHERE id = {0}
          AND tenant_id = {1}
          AND is_deleted = 0
=======
>>>>>>> 0b859b83b843d38e6b609297964ac23e3fe66577
        LIMIT 1
        """,
        id.ToString(),
        tenantId.ToString())
        .FirstOrDefaultAsync(ct);
}

<<<<<<< HEAD
static FornecedorDto ToFornecedorDto(Fornecedor fornecedor, byte[]? rowVersion) => new(
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
    fornecedor.UpdatedAt,
    Convert.ToBase64String(rowVersion ?? []));

static ClientePortalEnderecoDto ToClientePortalEnderecoDto(Endereco endereco, byte[]? rowVersion = null) => new(
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
    endereco.Padrao,
    rowVersion is null ? null : Convert.ToBase64String(rowVersion));

static string? ValidateClienteEnderecoAdminRequest(ClientePortalEnderecoRequest request)
{
    var baseValidation = ValidateEnderecoRequest(request);
    if (baseValidation is not null)
    {
        return baseValidation;
    }

    if (!Enum.TryParse<TipoEndereco>(request.Tipo, true, out _))
    {
        return "Tipo do endereco deve ser Entrega, Cobranca ou Ambos.";
    }

    if (TrimOrNull(request.Apelido)?.Length > 50 ||
        TrimOrNull(request.Logradouro)?.Length > 200 ||
        TrimOrNull(request.Numero)?.Length > 20 ||
        TrimOrNull(request.Complemento)?.Length > 100 ||
        TrimOrNull(request.Bairro)?.Length > 100 ||
        TrimOrNull(request.Cidade)?.Length > 100 ||
        TrimOrNull(request.Pais)?.Length > 50)
    {
        return "Um ou mais campos do endereco excedem o limite permitido.";
    }

    return null;
}

static void ApplyClienteEnderecoAdminRequest(Endereco endereco, ClientePortalEnderecoRequest request)
{
    endereco.Apelido = TrimOrNull(request.Apelido) ?? "Principal";
    endereco.Tipo = Enum.Parse<TipoEndereco>(request.Tipo!, true);
    endereco.Cep = NormalizeDocument(request.Cep)!;
    endereco.Logradouro = request.Logradouro!.Trim();
    endereco.Numero = request.Numero!.Trim();
    endereco.Complemento = TrimOrNull(request.Complemento);
    endereco.Bairro = request.Bairro!.Trim();
    endereco.Cidade = request.Cidade!.Trim();
    endereco.Estado = request.Estado!.Trim().ToUpperInvariant();
    endereco.Pais = TrimOrNull(request.Pais) ?? "Brasil";
    endereco.Padrao = request.Padrao;
    endereco.UpdatedAt = DateTime.UtcNow;
}

static async Task<ClientePortalEnderecoDto?> LoadClienteEnderecoDtoAsync(
    NexumDbContext db,
    int clienteId,
    int enderecoId,
    CancellationToken ct)
{
    var row = await db.Enderecos
        .AsNoTracking()
        .Where(endereco => endereco.Id == enderecoId && endereco.ClienteId == clienteId)
        .Select(endereco => new
        {
            Endereco = endereco,
            RowVersion = EF.Property<byte[]>(endereco, "RowVersion")
        })
        .SingleOrDefaultAsync(ct);
    return row is null ? null : ToClientePortalEnderecoDto(row.Endereco, row.RowVersion);
}

static async Task<string> BuildYaraOperationalContextAsync(
    NexumDbContext db,
    string? mensagem,
    CancellationToken ct)
{
    var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "a", "ao", "aos", "as", "com", "como", "da", "das", "de", "do", "dos", "e", "em",
        "eu", "me", "meu", "minha", "na", "nas", "no", "nos", "o", "os", "para", "por", "qual",
        "disponivel", "disponiveis", "gostaria", "loja", "lojas", "preciso", "produto", "produtos",
        "que", "quero", "saber", "sobre", "tem", "uma", "um", "voce", "voces"
    };
    var normalizedMessage = Slugify(mensagem) ?? string.Empty;
    var searchTerm = normalizedMessage
        .Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(term => term.Length >= 3 && !stopWords.Contains(term))
        .OrderByDescending(term => term.Length)
        .FirstOrDefault();

    var stores = await db.Lojas
        .AsNoTracking()
        .Where(store => store.Ativa)
        .OrderBy(store => store.OrdemExibicao)
        .Select(store => new
        {
            store.Nome,
            store.Segmento,
            store.Slug,
            store.Dominio
        })
        .ToListAsync(ct);

    var productsQuery = FiltrarProdutosPublicaveis(db.Produtos.AsNoTracking());
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        productsQuery = productsQuery.Where(product =>
            product.Nome.Contains(searchTerm) ||
            product.Sku.Contains(searchTerm) ||
            (product.Marca != null && product.Marca.Contains(searchTerm)) ||
            (product.Tags != null && product.Tags.Contains(searchTerm)) ||
            (product.DescricaoCurta != null && product.DescricaoCurta.Contains(searchTerm)));
    }

    var persistedProducts = await productsQuery
        .OrderByDescending(product => product.Destaque)
        .ThenByDescending(product => product.UpdatedAt)
        .Take(8)
        .Select(product => new
        {
            product.Nome,
            product.Sku,
            product.Slug,
            Loja = product.Loja != null ? product.Loja.Nome : string.Empty,
            product.Preco,
            product.PrecoPromocional,
            product.EstoqueAtual,
            product.EstoqueReservado
        })
        .ToListAsync(ct);

    var products = persistedProducts.Select(product => new
    {
        product.Nome,
        product.Sku,
        product.Slug,
        product.Loja,
        PrecoAtual = product.PrecoPromocional.HasValue && product.PrecoPromocional.Value > 0 && product.PrecoPromocional.Value < product.Preco
            ? product.PrecoPromocional.Value
            : product.Preco,
        EstoqueDisponivel = Math.Max(0, product.EstoqueAtual - product.EstoqueReservado)
    });

    return JsonSerializer.Serialize(new
    {
        Fonte = "nexum_altivon",
        ConsultadoEm = DateTime.UtcNow,
        TermoCatalogo = searchTerm,
        LojasAtivas = stores,
        ProdutosPublicaveis = products
    });
}

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

static bool TryGetGalleryImageCount(string? value, out int count)
{
    count = 0;
    if (string.IsNullOrWhiteSpace(value)) return true;

    var trimmed = value.Trim();
    if (trimmed.StartsWith("[", StringComparison.Ordinal))
    {
        try
        {
            var urls = JsonSerializer.Deserialize<string[]>(trimmed);
            if (urls is null) return false;
            count = urls.Count(url => !string.IsNullOrWhiteSpace(url));
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    count = trimmed.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
    return true;
}

static string LimitProductName(string value) => value.Length <= 200 ? value : value[..200];

static bool TryFormatProductVariant(Produto produto, string? requestedVariant, out string description, out string error)
{
    description = string.Empty;
    error = string.Empty;
    Dictionary<string, string[]> options;
    var tags = produto.Tags?.Trim();
    if (string.IsNullOrWhiteSpace(tags) || !tags.StartsWith("{", StringComparison.Ordinal))
    {
        if (!string.IsNullOrWhiteSpace(requestedVariant))
        {
            error = $"O produto {produto.Nome} nao possui opcoes cadastradas.";
            return false;
        }
        return true;
    }

    try
    {
        using var metadata = JsonDocument.Parse(tags);
        if (!metadata.RootElement.TryGetProperty("opcoes", out var optionsElement))
        {
            if (!string.IsNullOrWhiteSpace(requestedVariant))
            {
                error = $"O produto {produto.Nome} nao possui opcoes cadastradas.";
                return false;
            }
            return true;
        }
        if (optionsElement.ValueKind != JsonValueKind.Object)
        {
            error = $"As opcoes cadastradas para {produto.Nome} estao invalidas.";
            return false;
        }

        options = optionsElement.EnumerateObject().ToDictionary(
            option => option.Name,
            option => option.Value.ValueKind == JsonValueKind.Array
                ? option.Value.EnumerateArray().Where(value => value.ValueKind == JsonValueKind.String)
                    .Select(value => value.GetString() ?? string.Empty).Where(value => value.Length > 0).ToArray()
                : [],
            StringComparer.OrdinalIgnoreCase);
    }
    catch (JsonException)
    {
        error = $"Os metadados de opcoes do produto {produto.Nome} estao invalidos.";
        return false;
    }

    if (options.Count == 0)
    {
        if (!string.IsNullOrWhiteSpace(requestedVariant))
        {
            error = $"O produto {produto.Nome} nao possui opcoes cadastradas.";
            return false;
        }
        return true;
    }

    if (string.IsNullOrWhiteSpace(requestedVariant))
    {
        error = $"Selecione todas as opcoes de {produto.Nome} antes de concluir o pedido.";
        return false;
    }

    try
    {
        using var selection = JsonDocument.Parse(requestedVariant);
        if (selection.RootElement.ValueKind != JsonValueKind.Object)
        {
            error = $"As opcoes escolhidas para {produto.Nome} estao invalidas.";
            return false;
        }

        var selected = selection.RootElement.EnumerateObject().ToArray();
        if (selected.Length != options.Count)
        {
            error = $"Selecione todas as opcoes de {produto.Nome} antes de concluir o pedido.";
            return false;
        }

        var formatted = new List<string>(options.Count);
        foreach (var option in options)
        {
            var chosen = selected.FirstOrDefault(property => string.Equals(property.Name, option.Key, StringComparison.OrdinalIgnoreCase));
            var chosenValue = chosen.Value.ValueKind == JsonValueKind.String ? chosen.Value.GetString() : null;
            if (chosenValue is null || !option.Value.Contains(chosenValue, StringComparer.OrdinalIgnoreCase))
            {
                error = $"A opcao selecionada para {option.Key} em {produto.Nome} nao e valida.";
                return false;
            }
            formatted.Add($"{option.Key}: {chosenValue}");
        }

        if (selected.Any(property => !options.ContainsKey(property.Name)))
        {
            error = $"Foram enviadas opcoes nao cadastradas para {produto.Nome}.";
            return false;
        }

        description = string.Join("; ", formatted);
        return true;
    }
    catch (JsonException)
    {
        error = $"As opcoes escolhidas para {produto.Nome} estao invalidas.";
        return false;
    }
}

static bool TryGetImageDimensions(byte[] bytes, string contentType, out int width, out int height)
{
    width = height = 0;
    if (contentType is "image/png" && bytes.Length >= 24
        && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
    {
        width = ReadBigEndianInt32(bytes, 16);
        height = ReadBigEndianInt32(bytes, 20);
        return width > 0 && height > 0;
    }

    if (contentType is "image/gif" && bytes.Length >= 10
        && bytes[0] == (byte)'G' && bytes[1] == (byte)'I' && bytes[2] == (byte)'F')
    {
        width = bytes[6] | bytes[7] << 8;
        height = bytes[8] | bytes[9] << 8;
        return width > 0 && height > 0;
    }

    if (contentType is "image/webp" && bytes.Length >= 30
        && Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF"
        && Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP")
    {
        var chunk = Encoding.ASCII.GetString(bytes, 12, 4);
        if (chunk == "VP8X")
        {
            width = 1 + ReadLittleEndian24(bytes, 24);
            height = 1 + ReadLittleEndian24(bytes, 27);
            return width > 0 && height > 0;
        }
        if (chunk == "VP8 " && bytes.Length >= 30 && bytes[23] == 0x9D && bytes[24] == 0x01 && bytes[25] == 0x2A)
        {
            width = (bytes[26] | bytes[27] << 8) & 0x3FFF;
            height = (bytes[28] | bytes[29] << 8) & 0x3FFF;
            return width > 0 && height > 0;
        }
        if (chunk == "VP8L" && bytes.Length >= 25 && bytes[20] == 0x2F)
        {
            width = 1 + bytes[21] + ((bytes[22] & 0x3F) << 8);
            height = 1 + (bytes[22] >> 6) + (bytes[23] << 2) + ((bytes[24] & 0x0F) << 10);
            return width > 0 && height > 0;
        }
        return false;
    }

    if ((contentType is "image/jpeg" or "image/jpg") && bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xD8)
    {
        var offset = 2;
        while (offset + 4 < bytes.Length)
        {
            if (bytes[offset] != 0xFF) { offset++; continue; }
            while (offset < bytes.Length && bytes[offset] == 0xFF) offset++;
            if (offset >= bytes.Length) break;
            var marker = bytes[offset++];
            if (marker is 0xD8 or 0xD9 or 0x01 || marker is >= 0xD0 and <= 0xD7) continue;
            if (offset + 2 > bytes.Length) break;
            var segmentLength = bytes[offset] << 8 | bytes[offset + 1];
            if (segmentLength < 2 || offset + segmentLength > bytes.Length) break;
            if (marker is 0xC0 or 0xC1 or 0xC2 or 0xC3 or 0xC5 or 0xC6 or 0xC7 or 0xC9 or 0xCA or 0xCB or 0xCD or 0xCE or 0xCF)
            {
                if (segmentLength < 7) break;
                height = bytes[offset + 3] << 8 | bytes[offset + 4];
                width = bytes[offset + 5] << 8 | bytes[offset + 6];
                return width > 0 && height > 0;
            }
            offset += segmentLength;
        }
    }

    return false;
}

static int ReadBigEndianInt32(byte[] bytes, int offset) =>
    bytes[offset] << 24 | bytes[offset + 1] << 16 | bytes[offset + 2] << 8 | bytes[offset + 3];

static int ReadLittleEndian24(byte[] bytes, int offset) =>
    bytes[offset] | bytes[offset + 1] << 8 | bytes[offset + 2] << 16;

static bool IsProdutoPublicavel(Produto produto) =>
    produto.Ativo
    && produto.LojaId > 0
    && produto.CategoriaId.HasValue
    && !string.IsNullOrWhiteSpace(produto.Nome)
    && !string.IsNullOrWhiteSpace(produto.Sku)
    && !string.IsNullOrWhiteSpace(produto.Slug)
    && (!string.IsNullOrWhiteSpace(produto.DescricaoCurta) || !string.IsNullOrWhiteSpace(produto.DescricaoLonga))
    && !string.IsNullOrWhiteSpace(produto.ImagemPrincipal)
    && produto.Preco > 0
    && produto.Peso > 0
    && produto.Altura > 0
    && produto.Largura > 0
    && produto.Comprimento > 0;

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

static string ResolveJwtSecret(IConfiguration configuration)
{
    var candidates = new[]
    {
        configuration["JwtSettings:SecretKey"],
        configuration["JwtSettings:Secret"],
        configuration["JWT_SECRET_KEY"],
        Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    };

    var secret = candidates.FirstOrDefault(IsConfiguredSecret);
    if (string.IsNullOrWhiteSpace(secret))
    {
        throw new InvalidOperationException("JwtSettings:SecretKey nao configurada. Configure JwtSettings__SecretKey ou JWT_SECRET_KEY no runtime; valores CHANGE_ME/USE_ENV nao sao aceitos.");
    }

    return secret.Trim();
}

static string? ResolveConfiguredConnectionString(IConfiguration configuration, params string[] names)
{
    foreach (var name in names)
    {
        var value = configuration.GetConnectionString(name);
        if (IsConfiguredSecret(value))
        {
            return value!.Trim();
        }
    }

    return null;
}

static async Task<IResult> CheckMySqlHealthAsync(
    string? configuredConnectionString,
    string emptyConfigurationStatus,
    string expectedDatabase,
    CancellationToken ct)
{
    const int healthProbeTimeoutSeconds = 10;

    if (string.IsNullOrWhiteSpace(configuredConnectionString))
    {
        return Results.Ok(new { status = emptyConfigurationStatus });
    }

    try
    {
        var connectionBuilder = new MySqlConnectionStringBuilder(configuredConnectionString)
        {
            ConnectionTimeout = healthProbeTimeoutSeconds,
            DefaultCommandTimeout = healthProbeTimeoutSeconds,
            Pooling = true,
            MinimumPoolSize = 0,
            MaximumPoolSize = 8,
            ConnectionIdleTimeout = 60
        };

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(healthProbeTimeoutSeconds));

        await using var connection = new MySqlConnection(connectionBuilder.ConnectionString);
        await connection.OpenAsync(timeoutCts.Token);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT DATABASE(), 1";
        command.CommandTimeout = healthProbeTimeoutSeconds;

        await using var reader = await command.ExecuteReaderAsync(timeoutCts.Token);
        if (!await reader.ReadAsync(timeoutCts.Token))
        {
            return Results.Problem(
                title: $"Falha no healthcheck do banco {expectedDatabase}.",
                detail: "A consulta de validacao nao retornou linha.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var currentDatabase = reader.GetString(0);
        var probe = reader.GetInt32(1);

        return string.Equals(currentDatabase, expectedDatabase, StringComparison.OrdinalIgnoreCase) && probe == 1
            ? Results.Ok(new { status = "Healthy", database = currentDatabase })
            : Results.Problem(
                title: $"Falha no healthcheck do banco {expectedDatabase}.",
                detail: $"Conexao abriu no banco '{currentDatabase}', mas era esperado '{expectedDatabase}'.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (OperationCanceledException) when (!ct.IsCancellationRequested)
    {
        return Results.Problem(
            title: $"Timeout no healthcheck do banco {expectedDatabase}.",
            detail: $"A conexao MySQL nao respondeu em ate {healthProbeTimeoutSeconds} segundos.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: $"Falha no healthcheck do banco {expectedDatabase}.",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}

static string[] GetCorsOrigins(IConfiguration configuration)
{
    var configured = configuration.GetSection("ApiSettings:CorsOrigins").Get<string[]>() ?? Array.Empty<string>();
    var requiredPublicOrigins = new[]
    {
        "https://nexumaltivon.com.br",
        "https://www.nexumaltivon.com.br",
        "https://admin.nexumaltivon.com.br",
        "https://api.nexumaltivon.com.br",
        "https://back.nexumaltivon.com.br",
        "https://erp.nexumaltivon.com.br",
        "https://crm.nexumaltivon.com.br",
        "https://pdv.nexumaltivon.com.br"
    };

    return configured
        .Concat(requiredPublicOrigins)
        .Where(IsConfiguredSecret)
        .Select(origin => origin!.Trim().TrimEnd('/'))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
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
        var certificadoAberto = false;
        var certificadoComChave = false;
        var certificadoValido = false;
        var detalheCertificado = string.Empty;

        if (configurada)
        {
            try
            {
                using var certificado = new X509Certificate2(
                    arquivoPfx!,
                    senha,
                    X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);
                certificadoAberto = true;
                certificadoComChave = certificado.HasPrivateKey;
                certificadoValido = certificado.NotBefore.ToUniversalTime() <= DateTime.UtcNow &&
                    certificado.NotAfter.ToUniversalTime() >= DateTime.UtcNow;
                detalheCertificado = $" | Validade real: {certificado.NotAfter:yyyy-MM-dd}";
            }
            catch (Exception ex)
            {
                detalheCertificado = $" | Erro PFX: {ex.GetType().Name}";
            }
        }

        var operacional = configurada && certificadoAberto && certificadoComChave && certificadoValido && !string.IsNullOrWhiteSpace(cnpj);
        var pendencias = new List<string>();
        if (!arquivoExiste) pendencias.Add("CertificadoNFe__ArquivoPfx");
        if (!IsConfiguredSecret(senha)) pendencias.Add("CertificadoNFe__Senha");
        if (configurada && !certificadoAberto) pendencias.Add("Certificado A1 nao abriu com a senha configurada.");
        if (certificadoAberto && !certificadoComChave) pendencias.Add("Certificado A1 nao possui chave privada.");
        if (certificadoAberto && !certificadoValido) pendencias.Add("Certificado A1 fora do periodo de validade.");
        if (string.IsNullOrWhiteSpace(cnpj)) pendencias.Add("CertificadoNFe__Cnpj");

        return new IntegracaoDiagnosticoDto(
            "Certificado NF-e",
            "certificado",
            operacional ? "Pronto" : "Aguardando arquivo",
            configurada,
            operacional,
            operacional
                ? "Certificado A1 localizado e apto para uso no emissor."
                : "Certificado A1 ainda nao esta apto para emissao real.",
            operacional
                ? []
                : pendencias,
            DateTime.UtcNow,
            $"Tipo: A1 | CNPJ: {cnpj ?? "nao informado"} | Validade informada: {validoAte ?? "nao informada"}{detalheCertificado}");
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
    var contatoTecnico = GetIntegrationValue(
        configuration,
        "MelhorEnvio:ContatoTecnico",
        "Integracoes:MelhorEnvio:ContatoTecnico");
    var sandbox = configuration.GetValue("MelhorEnvio:Sandbox", configuration.GetValue("Integracoes:MelhorEnvio:Sandbox", true));
    var pendencias = new List<string>();
    if (!IsConfiguredSecret(token))
    {
        pendencias.Add("MelhorEnvio__Token");
    }

    if (string.IsNullOrWhiteSpace(contatoTecnico) || !contatoTecnico.Contains('@', StringComparison.Ordinal))
    {
        pendencias.Add("MelhorEnvio__ContatoTecnico");
    }

    if (pendencias.Count > 0)
    {
        return MissingIntegration(
            "Melhor Envio",
            "melhorenvio",
            "Integração oficial indisponível enquanto token e contato tecnico não estiverem configurados.",
            pendencias);
    }

    try
    {
        var client = httpClientFactory.CreateClient("melhor-envio");
        client.BaseAddress = new Uri(sandbox ? "https://sandbox.melhorenvio.com.br/" : "https://www.melhorenvio.com.br/");
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v2/me/shipment/services");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.UserAgent.ParseAdd($"GenesisGest.Net ({contatoTecnico!.Trim()})");
        using var response = await client.SendAsync(request, ct);

        return new IntegracaoDiagnosticoDto(
            "Melhor Envio",
            "melhorenvio",
            response.IsSuccessStatusCode ? "Conectado" : "Credencial recusada",
            true,
            response.IsSuccessStatusCode,
            response.IsSuccessStatusCode
                ? "Melhor Envio aceitou as credenciais no endpoint autenticado de serviços."
                : $"Melhor Envio retornou {(int)response.StatusCode}. Revise token, sandbox/produção e permissões.",
            response.IsSuccessStatusCode ? [] : ["Validar MelhorEnvio__Token.", "Validar MelhorEnvio__ContatoTecnico.", "Confirmar se MelhorEnvio__Sandbox está no ambiente correto."],
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

static (int StatusCode, string Message)? ValidateDropshippingCanalRequest(
    DropshippingCanalRequest request,
    IConfiguration configuration,
    out string nome,
    out string slug,
    out TipoDropshipping tipo)
{
    nome = request.Nome?.Trim() ?? string.Empty;
    slug = Slugify(request.Slug) ?? string.Empty;
    tipo = default;

    if (nome.Length is < 3 or > 100)
    {
        return (StatusCodes.Status400BadRequest, "Nome do canal deve ter entre 3 e 100 caracteres.");
    }

    if (slug.Length is < 2 or > 50)
    {
        return (StatusCodes.Status400BadRequest, "Slug do canal deve ter entre 2 e 50 caracteres validos.");
    }

    if (!Enum.TryParse(request.Tipo, true, out tipo))
    {
        return (StatusCodes.Status400BadRequest, "Tipo de dropshipping invalido.");
    }

    var endpoint = TrimOrNull(request.ApiEndpoint);
    if (endpoint is not null
        && (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri)
            || endpointUri.Scheme != Uri.UriSchemeHttps))
    {
        return (StatusCodes.Status400BadRequest, "Endpoint do canal deve ser uma URL HTTPS absoluta.");
    }

    var credentialStatus = GetDropshippingCredentialStatus(configuration, slug, tipo);
    if (request.Ativo && !credentialStatus.Configured)
    {
        return (StatusCodes.Status424FailedDependency, credentialStatus.Detail);
    }

    return null;
}

static DropshippingCanalDto ToDropshippingCanalDto(
    NexumDbContext db,
    IConfiguration configuration,
    DropshippingConfig canal)
{
    var credentialStatus = GetDropshippingCredentialStatus(configuration, canal.Slug, canal.Tipo);
    var rowVersion = db.Entry(canal).Property<byte[]>("RowVersion").CurrentValue ?? Array.Empty<byte>();
    return new DropshippingCanalDto(
        canal.Id,
        canal.Nome,
        canal.Slug,
        canal.Tipo.ToString(),
        canal.ApiEndpoint,
        canal.Ativo,
        credentialStatus.Configured,
        credentialStatus.Status,
        credentialStatus.Detail,
        Convert.ToBase64String(rowVersion),
        canal.CreatedAt,
        canal.UpdatedAt);
}

static (bool Configured, string Status, string Detail) GetDropshippingCredentialStatus(
    IConfiguration configuration,
    string slug,
    TipoDropshipping tipo)
{
    var normalized = NormalizeIntegrationSlug(slug);
    if (tipo == TipoDropshipping.Shopify || normalized == "shopify")
    {
        var configured = HasAllIntegrationValues(
            configuration,
            ("Shopify:StoreDomain", "Integracoes:Shopify:StoreDomain"),
            ("Shopify:AdminApiAccessToken", "Integracoes:Shopify:AdminApiAccessToken"),
            ("Shopify:ApiVersion", "Integracoes:Shopify:ApiVersion"));
        return configured
            ? (true, "Configurado", "Dominio, versao e token privado Shopify carregados no servidor.")
            : (false, "Aguardando credenciais", "Shopify exige dominio, versao da Admin API e token privado configurados no servidor.");
    }

    if (tipo == TipoDropshipping.CJDropshipping || normalized is "cjdropshipping" or "cjdropship")
    {
        var configured = HasAllIntegrationValues(
            configuration,
            ("CJDropshipping:ApiEndpoint", "Integracoes:CJDropshipping:ApiEndpoint"),
            ("CJDropshipping:AccessToken", "Integracoes:CJDropshipping:AccessToken"));
        return configured
            ? (true, "Configurado", "Endpoint e token CJ Dropshipping carregados no servidor.")
            : (false, "Aguardando credenciais", "CJ Dropshipping exige endpoint e token da conta configurados no servidor.");
    }

    if (normalized is "dropshippingprincipal" or "dropshippingsecundario")
    {
        var section = normalized == "dropshippingprincipal" ? "DropshippingPrincipal" : "DropshippingSecundario";
        var configured = HasAllIntegrationValues(
            configuration,
            ($"{section}:Provider", $"Integracoes:{section}:Provider"),
            ($"{section}:ApiEndpoint", $"Integracoes:{section}:ApiEndpoint"),
            ($"{section}:ApiKey", $"Integracoes:{section}:ApiKey"),
            ($"{section}:ApiSecret", $"Integracoes:{section}:ApiSecret"));
        return configured
            ? (true, "Configurado", $"Credenciais privadas de {section} carregadas no servidor.")
            : (false, "Aguardando credenciais", $"{section} exige provedor, endpoint, chave e segredo configurados no servidor.");
    }

    return (false, "Conector indisponivel", $"O tipo {tipo} ainda nao possui conector externo oficial implementado e nao pode ser ativado.");
}

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

static async Task<MarketplaceSyncResultadoDto> ExecutarMarketplaceSyncAsync(
    string canal,
    MarketplaceSyncRequest request,
    IConfiguration configuration,
    NexumDbContext db,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct)
{
    var canalNormalizado = NormalizeMarketplaceChannel(canal);
    if (canalNormalizado is null)
    {
        return new MarketplaceSyncResultadoDto(false, canal, "pull", "pedidos", false, false, 0, 0, 0, null, ["Canal marketplace suportado: mercadolivre, b2w ou via."], DateTime.UtcNow);
    }

    var direcao = NormalizeMarketplaceDirection(request.Direcao);
    var entidade = string.IsNullOrWhiteSpace(request.Entidade) ? "pedidos" : request.Entidade.Trim().ToLowerInvariant();
    var limite = Math.Clamp(request.Limite ?? 20, 1, 100);
    var marketplace = await ResolveMarketplaceConfigAsync(db, canalNormalizado, ct);
    var syncConfig = ResolveMarketplaceSyncConfiguration(configuration, marketplace, canalNormalizado, request, limite);
    var produtoIds = request.ProdutoIds?.Where(id => id > 0).Distinct().ToList() ?? [];
    var produtosQuery = db.Produtos.AsNoTracking().Where(produto => produto.Ativo && produto.Preco > 0);
    if (produtoIds.Count > 0)
    {
        produtosQuery = produtosQuery.Where(produto => produtoIds.Contains(produto.Id));
    }

    var produtos = await produtosQuery
        .OrderBy(produto => produto.Id)
        .Take(limite)
        .ToListAsync(ct);
    var sincronizaPedidos = ShouldSyncMarketplacePedidos(direcao, entidade);
    var sincronizaProdutos = ShouldSyncMarketplaceProdutos(direcao, entidade);
    var configuradaParaOperacao = syncConfig.Pendencias.Count == 0 &&
        (!sincronizaPedidos || syncConfig.PedidosEndpoint is not null) &&
        (!sincronizaProdutos || syncConfig.ProdutosEndpoint is not null);
    var pendencias = ValidateMarketplaceSyncRequest(syncConfig, direcao, entidade, sincronizaPedidos, sincronizaProdutos, produtos, produtoIds);
    if (pendencias.Count > 0)
    {
        return new MarketplaceSyncResultadoDto(false, canalNormalizado, direcao, entidade, configuradaParaOperacao, false, produtos.Count, 0, 0, syncConfig.ProviderHost, pendencias, DateTime.UtcNow);
    }

    var externosLidos = 0;
    var externosEnviados = 0;
    var operacoesExecutadas = new List<string>();
    var client = httpClientFactory.CreateClient(canalNormalizado == "mercadolivre" ? "mercado-livre" : "marketplace-sync");

    if (sincronizaPedidos)
    {
        var pedidoResult = await SendMarketplaceRequestAsync(client, HttpMethod.Get, syncConfig.PedidosEndpoint!, syncConfig.Token!, null, ct);
        if (!pedidoResult.Sucesso)
        {
            return new MarketplaceSyncResultadoDto(false, canalNormalizado, direcao, entidade, configuradaParaOperacao, false, produtos.Count, externosLidos, externosEnviados, syncConfig.ProviderHost, pedidoResult.Pendencias, DateTime.UtcNow);
        }

        externosLidos = CountMarketplaceJsonItems(pedidoResult.Body);
        operacoesExecutadas.Add($"pedidos_lidos={externosLidos.ToString(CultureInfo.InvariantCulture)}");
    }

    if (sincronizaProdutos)
    {
        var payload = BuildMarketplaceProductPayload(canalNormalizado, produtos, request.CategoriaExterna);
        var produtoResult = await SendMarketplaceRequestAsync(client, HttpMethod.Post, syncConfig.ProdutosEndpoint!, syncConfig.Token!, payload, ct);
        if (!produtoResult.Sucesso)
        {
            return new MarketplaceSyncResultadoDto(false, canalNormalizado, direcao, entidade, configuradaParaOperacao, false, produtos.Count, externosLidos, externosEnviados, syncConfig.ProviderHost, produtoResult.Pendencias, DateTime.UtcNow);
        }

        externosEnviados = produtos.Count;
        operacoesExecutadas.Add($"produtos_enviados={externosEnviados.ToString(CultureInfo.InvariantCulture)}");
    }

    if (marketplace is not null)
    {
        marketplace.UltimaSincronizacao = DateTime.UtcNow;
        marketplace.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    return new MarketplaceSyncResultadoDto(true, canalNormalizado, direcao, entidade, configuradaParaOperacao, true, produtos.Count, externosLidos, externosEnviados, syncConfig.ProviderHost, operacoesExecutadas, DateTime.UtcNow);
}

static async Task<Marketplace?> ResolveMarketplaceConfigAsync(NexumDbContext db, string canalNormalizado, CancellationToken ct)
{
    var aliases = canalNormalizado switch
    {
        "mercadolivre" => new[] { "mercadolivre", "mercado-livre", "mercado livre" },
        "b2w" => new[] { "b2w", "americanas" },
        "via" => new[] { "via", "viavarejo", "casasbahia", "ponto" },
        _ => [canalNormalizado]
    };

    return await db.Marketplaces
        .FirstOrDefaultAsync(item =>
            item.Ativo &&
            (aliases.Contains(item.Slug.ToLower()) || aliases.Contains(item.Nome.ToLower())), ct);
}

static MarketplaceSyncConfiguration ResolveMarketplaceSyncConfiguration(
    IConfiguration configuration,
    Marketplace? marketplace,
    string canalNormalizado,
    MarketplaceSyncRequest request,
    int limite)
{
    var pendencias = new List<string>();
    var token = ResolveMarketplaceToken(configuration, marketplace, canalNormalizado);
    var sellerId = ResolveMarketplaceSellerId(configuration, marketplace, canalNormalizado);
    var endpointBase = ResolveMarketplaceEndpointBase(configuration, canalNormalizado);
    var pedidosPath = ResolveMarketplacePath(configuration, canalNormalizado, "PedidosPath");
    var produtosPath = ResolveMarketplacePath(configuration, canalNormalizado, "ProdutosPath");

    if (canalNormalizado == "mercadolivre")
    {
        endpointBase ??= "https://api.mercadolibre.com/";
        pedidosPath ??= "orders/search?seller={sellerId}&order.status=paid&limit={limite}";
        produtosPath ??= "items";
    }

    if (!IsConfiguredSecret(token))
    {
        pendencias.Add($"Configure {MarketplaceConfigPrefix(canalNormalizado)}__AccessToken real.");
    }

    if (!IsConfiguredSecret(sellerId))
    {
        pendencias.Add($"Configure {MarketplaceConfigPrefix(canalNormalizado)}__SellerId real.");
    }

    if (!IsConfiguredSecret(endpointBase))
    {
        pendencias.Add($"Configure {MarketplaceConfigPrefix(canalNormalizado)}__EndpointBase real.");
    }

    if (!Uri.TryCreate((endpointBase ?? string.Empty).TrimEnd('/') + "/", UriKind.Absolute, out var baseUri))
    {
        pendencias.Add("EndpointBase do marketplace nao e uma URL absoluta valida.");
    }

    var pedidosEndpoint = BuildMarketplaceEndpoint(baseUri, pedidosPath, sellerId, limite);
    var produtosEndpoint = BuildMarketplaceEndpoint(baseUri, produtosPath, sellerId, limite);

    return new MarketplaceSyncConfiguration(
        canalNormalizado,
        endpointBase,
        pedidosEndpoint,
        produtosEndpoint,
        token,
        sellerId,
        baseUri?.Host,
        pendencias.Distinct(StringComparer.OrdinalIgnoreCase).ToList());
}

static List<string> ValidateMarketplaceSyncRequest(
    MarketplaceSyncConfiguration syncConfig,
    string direcao,
    string entidade,
    bool sincronizaPedidos,
    bool sincronizaProdutos,
    List<Produto> produtos,
    List<int> produtoIds)
{
    var pendencias = new List<string>(syncConfig.Pendencias);
    if (entidade is not ("pedidos" or "produtos" or "catalogo" or "todos"))
    {
        pendencias.Add("Entidade suportada: pedidos, produtos, catalogo ou todos.");
    }

    if (!sincronizaPedidos && !sincronizaProdutos)
    {
        pendencias.Add("Combinacao de direcao e entidade nao executa sincronizacao real. Use pull/pedidos, push/produtos, push/catalogo ou bidirecional/todos.");
    }

    if (sincronizaPedidos)
    {
        if (syncConfig.PedidosEndpoint is null)
        {
            pendencias.Add($"Configure {MarketplaceConfigPrefix(syncConfig.Canal)}__PedidosPath real.");
        }
    }

    if (sincronizaProdutos)
    {
        if (syncConfig.ProdutosEndpoint is null)
        {
            pendencias.Add($"Configure {MarketplaceConfigPrefix(syncConfig.Canal)}__ProdutosPath real.");
        }

        if (produtoIds.Count == 0)
        {
            pendencias.Add("Envio de produtos exige produto_ids explicitos para evitar publicacao acidental.");
        }

        if (produtos.Count == 0)
        {
            pendencias.Add("Nenhum produto ativo/publicavel encontrado para os produto_ids informados.");
        }

        if (syncConfig.Canal == "mercadolivre" && string.IsNullOrWhiteSpace(produtos.FirstOrDefault()?.Nome))
        {
            pendencias.Add("Produto Mercado Livre exige titulo real.");
        }
    }

    return pendencias.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
}

static bool ShouldSyncMarketplacePedidos(string direcao, string entidade) =>
    (direcao is "pull" or "bidirecional") && (entidade is "pedidos" or "todos");

static bool ShouldSyncMarketplaceProdutos(string direcao, string entidade) =>
    (direcao is "push" or "bidirecional") && (entidade is "produtos" or "catalogo" or "todos");

static async Task<MarketplaceHttpResult> SendMarketplaceRequestAsync(
    HttpClient client,
    HttpMethod method,
    Uri endpoint,
    string token,
    object? payload,
    CancellationToken ct)
{
    try
    {
        using var request = new HttpRequestMessage(method, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload, options: new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }

        using var response = await client.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            return new MarketplaceHttpResult(false, body, [$"Marketplace retornou HTTP {(int)response.StatusCode} em {endpoint.Host}."]);
        }

        return new MarketplaceHttpResult(true, body, []);
    }
    catch (Exception ex)
    {
        return new MarketplaceHttpResult(false, null, [$"Falha real ao chamar marketplace externo: {ex.Message}"]);
    }
}

static object BuildMarketplaceProductPayload(string canalNormalizado, List<Produto> produtos, string? categoriaExterna)
{
    if (canalNormalizado == "mercadolivre")
    {
        var produto = produtos.First();
        return new
        {
            title = produto.Nome,
            category_id = categoriaExterna,
            price = produto.PrecoPromocional ?? produto.Preco,
            currency_id = "BRL",
            available_quantity = Math.Max(0, produto.EstoqueAtual - produto.EstoqueReservado),
            buying_mode = "buy_it_now",
            listing_type_id = "gold_special",
            condition = "new"
        };
    }

    return new
    {
        produtos = produtos.Select(produto => new
        {
            sku = produto.Sku,
            nome = produto.Nome,
            descricao = produto.DescricaoLonga ?? produto.DescricaoCurta,
            preco = produto.PrecoPromocional ?? produto.Preco,
            estoque = Math.Max(0, produto.EstoqueAtual - produto.EstoqueReservado),
            peso = produto.Peso,
            altura = produto.Altura,
            largura = produto.Largura,
            comprimento = produto.Comprimento,
            codigo_barras = produto.CodigoBarras
        }).ToList()
    };
}

static Uri? BuildMarketplaceEndpoint(Uri? baseUri, string? path, string? sellerId, int limite)
{
    if (baseUri is null || !IsConfiguredSecret(path))
    {
        return null;
    }

    var resolved = path!
        .Replace("{sellerId}", Uri.EscapeDataString(sellerId ?? string.Empty), StringComparison.OrdinalIgnoreCase)
        .Replace("{seller_id}", Uri.EscapeDataString(sellerId ?? string.Empty), StringComparison.OrdinalIgnoreCase)
        .Replace("{limite}", limite.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
        .Replace("{limit}", limite.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);

    return Uri.TryCreate(resolved, UriKind.Absolute, out var absolute)
        ? absolute
        : new Uri(baseUri, resolved.TrimStart('/'));
}

static int CountMarketplaceJsonItems(string? body)
{
    if (string.IsNullOrWhiteSpace(body))
    {
        return 0;
    }

    try
    {
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        if (root.ValueKind == JsonValueKind.Array)
        {
            return root.GetArrayLength();
        }

        foreach (var name in new[] { "results", "orders", "items", "produtos", "pedidos", "data" })
        {
            if (root.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.Array)
            {
                return property.GetArrayLength();
            }
        }

        return root.ValueKind == JsonValueKind.Object ? 1 : 0;
    }
    catch
    {
        return 0;
    }
}

static string? ResolveMarketplaceToken(IConfiguration configuration, Marketplace? marketplace, string canalNormalizado) =>
    GetIntegrationValue(
        configuration,
        $"{MarketplaceConfigPrefix(canalNormalizado)}:AccessToken",
        $"Integracoes:{MarketplaceConfigPrefix(canalNormalizado)}:AccessToken",
        $"{MarketplaceConfigPrefix(canalNormalizado)}:Token",
        $"Integracoes:{MarketplaceConfigPrefix(canalNormalizado)}:Token") ?? marketplace?.AccessToken;

static string? ResolveMarketplaceSellerId(IConfiguration configuration, Marketplace? marketplace, string canalNormalizado) =>
    GetIntegrationValue(
        configuration,
        $"{MarketplaceConfigPrefix(canalNormalizado)}:SellerId",
        $"Integracoes:{MarketplaceConfigPrefix(canalNormalizado)}:SellerId",
        $"{MarketplaceConfigPrefix(canalNormalizado)}:ShopId",
        $"Integracoes:{MarketplaceConfigPrefix(canalNormalizado)}:ShopId") ?? marketplace?.SellerId;

static string? ResolveMarketplaceEndpointBase(IConfiguration configuration, string canalNormalizado) =>
    GetIntegrationValue(
        configuration,
        $"{MarketplaceConfigPrefix(canalNormalizado)}:EndpointBase",
        $"Integracoes:{MarketplaceConfigPrefix(canalNormalizado)}:EndpointBase",
        $"{MarketplaceConfigPrefix(canalNormalizado)}:ApiEndpoint",
        $"Integracoes:{MarketplaceConfigPrefix(canalNormalizado)}:ApiEndpoint");

static string? ResolveMarketplacePath(IConfiguration configuration, string canalNormalizado, string key) =>
    GetIntegrationValue(
        configuration,
        $"{MarketplaceConfigPrefix(canalNormalizado)}:{key}",
        $"Integracoes:{MarketplaceConfigPrefix(canalNormalizado)}:{key}");

static string MarketplaceConfigPrefix(string canalNormalizado) =>
    canalNormalizado switch
    {
        "mercadolivre" => "MercadoLivre",
        "b2w" => "B2W",
        "via" => "Via",
        _ => canalNormalizado
    };

static string? NormalizeMarketplaceChannel(string? canal)
{
    var normalized = NormalizeIntegrationSlug(canal ?? string.Empty);
    return normalized switch
    {
        "mercadolivre" or "ml" => "mercadolivre",
        "b2w" or "americanas" => "b2w",
        "via" or "viavarejo" or "casasbahia" or "ponto" => "via",
        _ => null
    };
}

static string NormalizeMarketplaceDirection(string? direcao)
{
    var normalized = NormalizeIntegrationSlug(direcao ?? "pull");
    return normalized switch
    {
        "push" or "envio" or "enviar" => "push",
        "bidirectional" or "bidirecional" or "ambos" => "bidirecional",
        _ => "pull"
    };
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

                throw new InvalidOperationException("Melhor Envio nao retornou cotacoes utilizaveis para o CEP informado.");
            }

            throw new InvalidOperationException($"Melhor Envio recusou a cotacao de frete com HTTP {(int)response.StatusCode}.");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            throw new InvalidOperationException("Melhor Envio configurado, mas indisponivel para cotacao de frete.", ex);
        }
    }

    var valorItens = request.Itens.Sum(item => Math.Max(1, item.Quantidade) * item.ValorUnitario);
    var cepDestino = request.CepDestino ?? string.Empty;
    var interiorSp = cepDestino.StartsWith("17", StringComparison.Ordinal);
    var freteBase = valorItens >= 1000 ? 0m : interiorSp ? 29.90m : 49.90m;

    return
    [
        new("interno-padrao", "Entrega padrão Nexum", "Tabela interna oficial", freteBase, interiorSp ? 3 : 7, "Tabela interna oficial"),
        new("interno-expresso", "Entrega expressa assistida", "Tabela interna oficial", freteBase + 35m, interiorSp ? 1 : 4, "Tabela interna oficial")
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

static SiteMidiaDto MapSiteMidiaDto(SiteMidia midia, HttpContext http) =>
    new(
        midia.Id,
        midia.TenantId,
        midia.Nome,
        midia.Tipo.ToString(),
        midia.TextoAlternativo,
        midia.NomeArquivoOriginal,
        midia.CaminhoRelativo,
        BuildPublicAssetUrl(http, midia.CaminhoRelativo),
        midia.ContentType,
        midia.TamanhoBytes,
        midia.Largura,
        midia.Altura,
        Convert.ToBase64String(midia.RowVersion),
        midia.CreatedAt,
        midia.UpdatedAt);

static string BuildPublicAssetUrl(HttpContext http, string relativePath)
{
    var forwardedProto = http.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
    var scheme = string.Equals(forwardedProto, "https", StringComparison.OrdinalIgnoreCase)
        ? "https"
        : http.Request.Scheme;
    var forwardedHost = http.Request.Headers["X-Forwarded-Host"].FirstOrDefault();
    var host = string.IsNullOrWhiteSpace(forwardedHost) ? http.Request.Host.Value : forwardedHost.Trim();
    return $"{scheme}://{host}{(relativePath.StartsWith('/') ? relativePath : $"/{relativePath}")}";
}

static bool TryResolvePublicMediaFile(string webRootPath, string relativePath, out string fullPath)
{
    fullPath = string.Empty;
    if (string.IsNullOrWhiteSpace(webRootPath) || string.IsNullOrWhiteSpace(relativePath))
    {
        return false;
    }

    var normalizedRelativePath = relativePath.Replace('\\', '/').Trim();
    if (!normalizedRelativePath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    var webRoot = Path.GetFullPath(webRootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    var relativeFile = normalizedRelativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
    var candidate = Path.GetFullPath(Path.Combine(webRoot, relativeFile));
    if (!candidate.StartsWith(webRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    fullPath = candidate;
    return true;
}

static bool TryParseSiteMediaRowVersion(string? value, out byte[] rowVersion)
{
    rowVersion = Array.Empty<byte>();
    if (string.IsNullOrWhiteSpace(value))
    {
        return false;
    }

    try
    {
        rowVersion = Convert.FromBase64String(value.Trim());
        return rowVersion.Length > 0;
    }
    catch (FormatException)
    {
        return false;
    }
}

static bool TryDecodeSiteImage(
    SiteMidiaUploadRequest request,
    out DecodedSiteImage decoded,
    out string error)
{
    decoded = new DecodedSiteImage(Array.Empty<byte>(), string.Empty, string.Empty, 0, 0);
    error = string.Empty;
    var dataUrl = request.DataUrl?.Trim();
    if (string.IsNullOrWhiteSpace(dataUrl))
    {
        error = "Conteudo da imagem e obrigatorio.";
        return false;
    }

    var separatorIndex = dataUrl.IndexOf(',', StringComparison.Ordinal);
    var base64 = separatorIndex >= 0 ? dataUrl[(separatorIndex + 1)..] : dataUrl;
    byte[] bytes;
    try
    {
        bytes = Convert.FromBase64String(base64);
    }
    catch (FormatException)
    {
        error = "Conteudo base64 da imagem e invalido.";
        return false;
    }

    const int maxBytes = 8 * 1024 * 1024;
    if (bytes.Length == 0 || bytes.Length > maxBytes)
    {
        error = "Imagem deve possuir conteudo e ter no maximo 8MB.";
        return false;
    }

    string contentType;
    string extension;
    int width;
    int height;
    if (TryReadPngDimensions(bytes, out width, out height))
    {
        contentType = "image/png";
        extension = ".png";
    }
    else if (TryReadJpegDimensions(bytes, out width, out height))
    {
        contentType = "image/jpeg";
        extension = ".jpg";
    }
    else if (TryReadWebpDimensions(bytes, out width, out height))
    {
        contentType = "image/webp";
        extension = ".webp";
    }
    else
    {
        error = "Arquivo nao e uma imagem PNG, JPEG ou WebP valida.";
        return false;
    }

    var declaredContentType = request.ContentType?.Trim().ToLowerInvariant();
    if (declaredContentType == "image/jpg")
    {
        declaredContentType = "image/jpeg";
    }

    if (!string.IsNullOrWhiteSpace(declaredContentType)
        && !string.Equals(declaredContentType, contentType, StringComparison.Ordinal))
    {
        error = $"Content-Type declarado ({declaredContentType}) diverge do arquivo validado ({contentType}).";
        return false;
    }

    const long maxPixels = 60_000_000;
    if (width is < 32 or > 12_000 || height is < 32 or > 12_000 || (long)width * height > maxPixels)
    {
        error = "Dimensoes da imagem devem ficar entre 32 e 12000 pixels, limitadas a 60 megapixels.";
        return false;
    }

    decoded = new DecodedSiteImage(bytes, contentType, extension, width, height);
    return true;
}

static bool TryReadPngDimensions(byte[] bytes, out int width, out int height)
{
    width = 0;
    height = 0;
    ReadOnlySpan<byte> signature = [137, 80, 78, 71, 13, 10, 26, 10];
    if (bytes.Length < 24 || !bytes.AsSpan(0, signature.Length).SequenceEqual(signature))
    {
        return false;
    }

    width = (bytes[16] << 24) | (bytes[17] << 16) | (bytes[18] << 8) | bytes[19];
    height = (bytes[20] << 24) | (bytes[21] << 16) | (bytes[22] << 8) | bytes[23];
    return width > 0 && height > 0;
}

static bool TryReadJpegDimensions(byte[] bytes, out int width, out int height)
{
    width = 0;
    height = 0;
    if (bytes.Length < 12 || bytes[0] != 0xFF || bytes[1] != 0xD8)
    {
        return false;
    }

    var offset = 2;
    while (offset + 8 < bytes.Length)
    {
        while (offset < bytes.Length && bytes[offset] == 0xFF)
        {
            offset++;
        }

        if (offset >= bytes.Length)
        {
            break;
        }

        var marker = bytes[offset++];
        if (marker is 0xD8 or 0xD9)
        {
            continue;
        }

        if (offset + 1 >= bytes.Length)
        {
            return false;
        }

        var segmentLength = (bytes[offset] << 8) | bytes[offset + 1];
        if (segmentLength < 2 || offset + segmentLength > bytes.Length)
        {
            return false;
        }

        var isStartOfFrame = marker is 0xC0 or 0xC1 or 0xC2 or 0xC3 or 0xC5 or 0xC6 or 0xC7 or 0xC9 or 0xCA or 0xCB or 0xCD or 0xCE or 0xCF;
        if (isStartOfFrame && segmentLength >= 7)
        {
            height = (bytes[offset + 3] << 8) | bytes[offset + 4];
            width = (bytes[offset + 5] << 8) | bytes[offset + 6];
            return width > 0 && height > 0;
        }

        offset += segmentLength;
    }

    return false;
}

static bool TryReadWebpDimensions(byte[] bytes, out int width, out int height)
{
    width = 0;
    height = 0;
    if (bytes.Length < 25
        || Encoding.ASCII.GetString(bytes, 0, 4) != "RIFF"
        || Encoding.ASCII.GetString(bytes, 8, 4) != "WEBP")
    {
        return false;
    }

    var chunkType = Encoding.ASCII.GetString(bytes, 12, 4);
    if (chunkType == "VP8X" && bytes.Length >= 30)
    {
        width = 1 + bytes[24] + (bytes[25] << 8) + (bytes[26] << 16);
        height = 1 + bytes[27] + (bytes[28] << 8) + (bytes[29] << 16);
        return width > 0 && height > 0;
    }

    if (chunkType == "VP8L" && bytes.Length >= 25 && bytes[20] == 0x2F)
    {
        width = 1 + bytes[21] + ((bytes[22] & 0x3F) << 8);
        height = 1 + (bytes[22] >> 6) + (bytes[23] << 2) + ((bytes[24] & 0x0F) << 10);
        return width > 0 && height > 0;
    }

    if (chunkType == "VP8 " && bytes.Length >= 30 && bytes[23] == 0x9D && bytes[24] == 0x01 && bytes[25] == 0x2A)
    {
        width = (bytes[26] | (bytes[27] << 8)) & 0x3FFF;
        height = (bytes[28] | (bytes[29] << 8)) & 0x3FFF;
        return width > 0 && height > 0;
    }

    return false;
}

static async Task<IResult?> ValidateSitePerfilPublicoRequestAsync(
    SitePerfilPublicoRequest request,
    Guid? profileId,
    NexumDbContext db,
    CancellationToken ct)
{
    if (!Enum.TryParse<TipoPerfilPublico>(request.TipoPerfil?.Trim(), true, out var profileType)
        || !Enum.IsDefined(profileType))
    {
        return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro(
            "Tipo de perfil inválido. Use Loja, Fornecedor, ParceiroVenda, ParceiroCompra ou Marketplace."));
    }

    if (!Enum.TryParse<OrigemPerfilPublico>(request.OrigemTipo?.Trim(), true, out var sourceType)
        || !Enum.IsDefined(sourceType))
    {
        return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro(
            "Origem inválida. Use Loja, Fornecedor, Marketplace ou Parceiro."));
    }

    if ((profileType == TipoPerfilPublico.Loja && sourceType != OrigemPerfilPublico.Loja)
        || (profileType == TipoPerfilPublico.Fornecedor && sourceType != OrigemPerfilPublico.Fornecedor)
        || (profileType == TipoPerfilPublico.Marketplace && sourceType != OrigemPerfilPublico.Marketplace))
    {
        return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro(
            "O tipo do perfil não corresponde ao cadastro de origem selecionado."));
    }

    if (sourceType == OrigemPerfilPublico.Parceiro)
    {
        if (profileType is not TipoPerfilPublico.ParceiroVenda and not TipoPerfilPublico.ParceiroCompra)
        {
            return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro(
                "A origem Parceiro só pode ser usada em ParceiroVenda ou ParceiroCompra."));
        }

        if (request.OrigemId.HasValue)
        {
            return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro(
                "Perfis de parceiros independentes não devem informar identificador de outra tabela."));
        }
    }
    else if (!request.OrigemId.HasValue || request.OrigemId.Value <= 0)
    {
        return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro(
            "Selecione um cadastro de origem real para publicar este perfil."));
    }

    if (sourceType == OrigemPerfilPublico.Loja
        && !await db.Lojas.AsNoTracking().AnyAsync(item => item.Id == request.OrigemId, ct))
    {
        return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro("A loja de origem não existe no tenant atual."));
    }

    if (sourceType == OrigemPerfilPublico.Fornecedor
        && !await db.Fornecedores.AsNoTracking().AnyAsync(item => item.Id == request.OrigemId, ct))
    {
        return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro("O fornecedor de origem não existe no tenant atual."));
    }

    if (sourceType == OrigemPerfilPublico.Marketplace
        && !await db.Marketplaces.AsNoTracking().AnyAsync(item => item.Id == request.OrigemId, ct))
    {
        return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro("O marketplace de origem não existe no tenant atual."));
    }

    var name = request.Nome?.Trim() ?? string.Empty;
    var slug = NormalizePublicProfileSlug(request.Slug);
    var segment = request.Segmento?.Trim() ?? string.Empty;
    var activity = request.Atividade?.Trim() ?? string.Empty;
    var description = request.Descricao?.Trim() ?? string.Empty;
    var logo = request.LogoUrl?.Trim();
    var banner = request.BannerUrl?.Trim();
    var ctaText = request.CtaTexto?.Trim();
    var ctaUrl = request.CtaUrl?.Trim();
    var siteUrl = request.SiteUrl?.Trim();
    var publicEmail = request.EmailPublico?.Trim();
    var publicPhone = request.TelefonePublico?.Trim();
    var publicAddress = request.EnderecoPublico?.Trim();
    var profileColors = new[]
    {
        request.CorPrimaria,
        request.CorSecundaria,
        request.CorFundo,
        request.CorTexto
    };

    if (name.Length is < 2 or > 160)
    {
        return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro("Nome público deve possuir entre 2 e 160 caracteres."));
    }

    if (slug.Length is < 3 or > 80)
    {
        return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro("Slug deve possuir entre 3 e 80 letras, números ou hífens."));
    }

    if (segment.Length is < 2 or > 120 || activity.Length is < 5 or > 240 || description.Length is < 20 or > 2000)
    {
        return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro(
            "Segmento, atividade ou descrição estão fora dos limites de publicação permitidos."));
    }

    if (string.IsNullOrWhiteSpace(logo) && string.IsNullOrWhiteSpace(banner))
    {
        return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro("Informe ao menos uma logomarca ou um banner oficial."));
    }

    if ((!string.IsNullOrWhiteSpace(logo) && !IsValidPublicSiteImageReference(logo))
        || (!string.IsNullOrWhiteSpace(banner) && !IsValidPublicSiteImageReference(banner)))
    {
        return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro(
            "Logomarca e banner devem apontar para /uploads, /imagens ou /assets no storage oficial."));
    }

    if (request.OrdemExibicao is < 0 or > 9999)
    {
        return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro("Ordem de exibição deve estar entre 0 e 9999."));
    }

    if (string.IsNullOrWhiteSpace(ctaText) != string.IsNullOrWhiteSpace(ctaUrl)
        || (!string.IsNullOrWhiteSpace(ctaText) && ctaText.Length > 80)
        || (!string.IsNullOrWhiteSpace(ctaUrl) && !IsValidPublicCallToActionUrl(ctaUrl)))
    {
        return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro(
            "Chamada comercial deve possuir texto e URL válidos, ou ambos devem permanecer vazios."));
    }

    if ((!string.IsNullOrWhiteSpace(siteUrl)
            && (!Uri.TryCreate(siteUrl, UriKind.Absolute, out var siteUri) || siteUri.Scheme != Uri.UriSchemeHttps))
        || (!string.IsNullOrWhiteSpace(publicEmail)
            && (publicEmail.Length > 254 || !System.Net.Mail.MailAddress.TryCreate(publicEmail, out _)))
        || (!string.IsNullOrWhiteSpace(publicPhone) && !IsValidPublicPhone(publicPhone))
        || (!string.IsNullOrWhiteSpace(publicAddress) && publicAddress.Length > 300))
    {
        return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro(
            "Site, e-mail, telefone ou endereço público possuem formato inválido."));
    }

    if (profileColors.Any(color => !string.IsNullOrWhiteSpace(color) && !IsValidHexColor(color.Trim())))
    {
        return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro(
            "As cores do perfil devem usar o formato hexadecimal #RRGGBB."));
    }

    var productIds = NormalizeSiteProfileProductIds(request.ProdutoIds);
    if (productIds.Count > 200)
    {
        return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro(
            "Cada perfil pode divulgar no máximo 200 produtos."));
    }

    if (productIds.Count > 0)
    {
        var eligibleProducts = await FiltrarProdutosPublicaveis(db.Produtos.AsNoTracking())
            .Where(item => productIds.Contains(item.Id))
            .Select(item => new { item.Id, item.LojaId, item.FornecedorId })
            .ToListAsync(ct);
        if (eligibleProducts.Count != productIds.Count)
        {
            return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro(
                "Um ou mais produtos selecionados não existem ou não atendem aos requisitos de publicação."));
        }

        if (profileType == TipoPerfilPublico.Loja
            && eligibleProducts.Any(item => item.LojaId != request.OrigemId))
        {
            return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro(
                "A loja só pode divulgar produtos pertencentes ao próprio cadastro."));
        }

        if (sourceType == OrigemPerfilPublico.Fornecedor
            && eligibleProducts.Any(item => item.FornecedorId != request.OrigemId))
        {
            return Results.BadRequest(ApiResponse<SitePerfilPublicoAdminDto>.Erro(
                "O fornecedor só pode divulgar produtos vinculados ao próprio cadastro."));
        }
    }

    if (await db.SitePerfisPublicos.AsNoTracking().AnyAsync(
        item => (!profileId.HasValue || item.Id != profileId.Value) && item.Slug == slug,
        ct))
    {
        return Results.Conflict(ApiResponse<SitePerfilPublicoAdminDto>.Erro("Já existe um perfil público com este slug."));
    }

    if (sourceType != OrigemPerfilPublico.Parceiro
        && await db.SitePerfisPublicos.AsNoTracking().AnyAsync(
            item => (!profileId.HasValue || item.Id != profileId.Value)
                && item.OrigemTipo == sourceType
                && item.OrigemId == request.OrigemId,
            ct))
    {
        return Results.Conflict(ApiResponse<SitePerfilPublicoAdminDto>.Erro(
            "O cadastro de origem selecionado já possui um perfil público."));
    }

    return null;
}

static void ApplySitePerfilPublicoRequest(SitePerfilPublico profile, SitePerfilPublicoRequest request)
{
    profile.TipoPerfil = Enum.Parse<TipoPerfilPublico>(request.TipoPerfil.Trim(), true);
    profile.OrigemTipo = Enum.Parse<OrigemPerfilPublico>(request.OrigemTipo.Trim(), true);
    profile.OrigemId = profile.OrigemTipo == OrigemPerfilPublico.Parceiro ? null : request.OrigemId;
    profile.Nome = request.Nome.Trim();
    profile.Slug = NormalizePublicProfileSlug(request.Slug);
    profile.Segmento = request.Segmento.Trim();
    profile.Atividade = request.Atividade.Trim();
    profile.Descricao = request.Descricao.Trim();
    profile.LogoUrl = TrimOrNull(request.LogoUrl);
    profile.BannerUrl = TrimOrNull(request.BannerUrl);
    profile.Icone = TrimOrNull(request.Icone);
    profile.CtaTexto = TrimOrNull(request.CtaTexto);
    profile.CtaUrl = TrimOrNull(request.CtaUrl);
    profile.SiteUrl = TrimOrNull(request.SiteUrl);
    profile.EmailPublico = TrimOrNull(request.EmailPublico);
    profile.TelefonePublico = TrimOrNull(request.TelefonePublico);
    profile.EnderecoPublico = TrimOrNull(request.EnderecoPublico);
    profile.CorPrimaria = TrimOrNull(request.CorPrimaria)?.ToUpperInvariant();
    profile.CorSecundaria = TrimOrNull(request.CorSecundaria)?.ToUpperInvariant();
    profile.CorFundo = TrimOrNull(request.CorFundo)?.ToUpperInvariant();
    profile.CorTexto = TrimOrNull(request.CorTexto)?.ToUpperInvariant();
    profile.Publicado = request.Publicado;
    profile.OrdemExibicao = request.OrdemExibicao;
}

static string NormalizePublicProfileSlug(string? value)
{
    var normalized = (value ?? string.Empty).Trim().Normalize(NormalizationForm.FormD);
    var builder = new StringBuilder(normalized.Length);
    var previousWasSeparator = false;
    foreach (var character in normalized)
    {
        if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
        {
            continue;
        }

        if (char.IsAsciiLetterOrDigit(character))
        {
            builder.Append(char.ToLowerInvariant(character));
            previousWasSeparator = false;
            continue;
        }

        if (!previousWasSeparator && builder.Length > 0)
        {
            builder.Append('-');
            previousWasSeparator = true;
        }
    }

    return builder.ToString().Trim('-');
}

static bool IsValidPublicCallToActionUrl(string value)
{
    if (value.StartsWith("/", StringComparison.Ordinal) && !value.StartsWith("//", StringComparison.Ordinal))
    {
        return true;
    }

    return Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps
            || uri.Scheme == Uri.UriSchemeMailto
            || uri.Scheme == "tel");
}

static bool IsValidPublicPhone(string value)
{
    if (value.Length > 30 || value.Any(character => !char.IsDigit(character) && character is not ' ' and not '+' and not '-' and not '(' and not ')'))
    {
        return false;
    }

    return value.Count(char.IsDigit) is >= 8 and <= 15;
}

static bool IsValidHexColor(string? value)
{
    if (string.IsNullOrWhiteSpace(value) || value.Length != 7 || value[0] != '#')
    {
        return false;
    }

    for (var index = 1; index < value.Length; index++)
    {
        if (!Uri.IsHexDigit(value[index]))
        {
            return false;
        }
    }

    return true;
}

static bool IsSiteColorConfigurationKey(string key) =>
    key.Equals("site_cor_primaria", StringComparison.OrdinalIgnoreCase)
    || key.Equals("site_cor_secundaria", StringComparison.OrdinalIgnoreCase)
    || key.Equals("site_cor_fundo", StringComparison.OrdinalIgnoreCase)
    || key.Equals("site_cor_superficie", StringComparison.OrdinalIgnoreCase)
    || key.Equals("site_cor_texto", StringComparison.OrdinalIgnoreCase)
    || key.Equals("site_cor_texto_suave", StringComparison.OrdinalIgnoreCase);

static List<int> NormalizeSiteProfileProductIds(IEnumerable<int>? productIds) =>
    productIds?
        .Where(id => id > 0)
        .Distinct()
        .ToList()
    ?? [];

static async Task<List<int>> LoadSitePerfilPublicoProductIdsAsync(
    Guid profileId,
    NexumDbContext db,
    CancellationToken ct) =>
    await db.SitePerfisPublicosProdutos
        .AsNoTracking()
        .Where(item => item.PerfilPublicoId == profileId && item.Publicado)
        .OrderBy(item => item.OrdemExibicao)
        .Select(item => item.ProdutoId)
        .ToListAsync(ct);

static async Task SyncSitePerfilPublicoProdutosAsync(
    Guid profileId,
    IEnumerable<int>? productIds,
    NexumDbContext db,
    CancellationToken ct)
{
    var desiredIds = NormalizeSiteProfileProductIds(productIds);
    var desiredOrder = desiredIds
        .Select((productId, index) => new { productId, index })
        .ToDictionary(item => item.productId, item => item.index);
    var currentTenantId = db.CurrentTenantId;
    var existing = await db.SitePerfisPublicosProdutos
        .IgnoreQueryFilters()
        .Where(item => item.TenantId == currentTenantId && item.PerfilPublicoId == profileId)
        .ToListAsync(ct);

    foreach (var link in existing)
    {
        if (!desiredOrder.TryGetValue(link.ProdutoId, out var order))
        {
            if (!link.IsDeleted)
            {
                db.SitePerfisPublicosProdutos.Remove(link);
            }

            continue;
        }

        link.IsDeleted = false;
        link.DeletedAt = null;
        link.Publicado = true;
        link.OrdemExibicao = order;
        desiredOrder.Remove(link.ProdutoId);
    }

    foreach (var item in desiredOrder)
    {
        db.SitePerfisPublicosProdutos.Add(new SitePerfilPublicoProduto
        {
            PerfilPublicoId = profileId,
            ProdutoId = item.Key,
            Publicado = true,
            OrdemExibicao = item.Value
        });
    }
}

static SitePerfilPublicoAdminDto ToSitePerfilPublicoAdminDto(SitePerfilPublico profile, List<int>? productIds = null) => new(
    profile.Id,
    profile.TipoPerfil.ToString(),
    profile.OrigemTipo.ToString(),
    profile.OrigemId,
    profile.Nome,
    profile.Slug,
    profile.Segmento,
    profile.Atividade,
    profile.Descricao,
    profile.LogoUrl,
    profile.BannerUrl,
    profile.Icone,
    profile.CtaTexto,
    profile.CtaUrl,
    profile.SiteUrl,
    profile.EmailPublico,
    profile.TelefonePublico,
    profile.EnderecoPublico,
    profile.CorPrimaria,
    profile.CorSecundaria,
    profile.CorFundo,
    profile.CorTexto,
    productIds ?? [],
    profile.Publicado,
    profile.OrdemExibicao,
    Convert.ToBase64String(profile.RowVersion ?? []),
    profile.CreatedAt,
    profile.UpdatedAt);

static StoreCardSiteDto ToStoreCardSiteDto(SitePerfilPublico profile) => new(
    profile.Id,
    profile.OrigemId,
    profile.Nome,
    profile.Slug,
    profile.Segmento,
    profile.Atividade,
    profile.Descricao,
    profile.BannerUrl ?? profile.LogoUrl,
    profile.LogoUrl,
    profile.Icone ?? "Store",
    profile.CtaTexto,
    profile.CtaUrl,
    profile.CorPrimaria,
    profile.CorSecundaria,
    profile.CorFundo,
    profile.CorTexto,
    profile.OrdemExibicao);

static PartnerCardSiteDto ToPartnerCardSiteDto(SitePerfilPublico profile) => new(
    profile.Id,
    profile.OrigemId,
    profile.TipoPerfil.ToString(),
    profile.OrigemTipo.ToString(),
    profile.Nome,
    profile.Slug,
    profile.Descricao,
    "Ver perfil",
    $"/parceiros/{profile.Slug}",
    profile.Icone ?? "Building2",
    profile.BannerUrl ?? profile.LogoUrl,
    profile.LogoUrl,
    profile.Atividade,
    profile.Segmento,
    profile.CorPrimaria,
    profile.CorSecundaria,
    profile.CorFundo,
    profile.CorTexto,
    profile.OrdemExibicao);

static PartnerDetailSiteDto ToPartnerDetailSiteDto(
    SitePerfilPublico profile,
    List<ProdutoLojaDto> products)
{
    var card = ToPartnerCardSiteDto(profile);
    return new PartnerDetailSiteDto(
        card.Id,
        card.OrigemId,
        card.Tipo,
        card.OrigemTipo,
        card.Title,
        card.Slug,
        card.Text,
        card.Icon,
        card.Image,
        card.Logo,
        card.Activity,
        card.Segment,
        card.Order,
        profile.SiteUrl,
        profile.EmailPublico,
        profile.TelefonePublico,
        profile.EnderecoPublico,
        profile.CorPrimaria,
        profile.CorSecundaria,
        profile.CorFundo,
        profile.CorTexto,
        profile.CtaTexto,
        profile.CtaUrl,
        products);
}

static SiteConfiguracaoPublicaDto BuildPublicSiteConfig(
    IReadOnlyDictionary<string, string?> configMap,
    List<HeroSlideSiteDto> heroSlides,
    List<StoreCardSiteDto> storeCards,
    List<PartnerCardSiteDto> partnerCards)
{
    var contactEmail = GetConfigValue(configMap, "site_email_contato", "corporativo.gna@gmail.com");

    return new SiteConfiguracaoPublicaDto(
        GetConfigValue(configMap, "site_nome", "Grupo Nexum Altivon"),
        GetConfigValue(configMap, "site_url", "https://nexumaltivon.com.br"),
        contactEmail,
        GetConfigValue(configMap, "site_telefone", "(14) 99673-1879"),
        GetConfigValue(configMap, "site_telefone_secundario", "(14) 99673-1879"),
        GetConfigValue(configMap, "site_whatsapp", "5514996731879"),
        GetConfigValue(configMap, "site_whatsapp_secundario", "5514996731879"),
        GetConfigValue(configMap, "site_yara_email", contactEmail),
        GetConfigValue(configMap, "site_logo", "/imagens/homepage/Logo-2.png"),
        GetConfigValue(configMap, "site_subtitulo", "Participações societárias"),
        GetConfigValue(configMap, "site_institucional_url", "/institucional"),
        GetConfigValue(configMap, "site_politica_privacidade_url", "/politica-privacidade"),
        GetConfigValue(configMap, "site_politica_reembolso_url", "/politica-reembolso"),
        GetConfigValue(configMap, "site_cor_primaria", "#C9A227"),
        GetConfigValue(configMap, "site_cor_secundaria", "#0A0A0A"),
        GetConfigValue(configMap, "site_cor_fundo", "#F5F7FB"),
        GetConfigValue(configMap, "site_cor_superficie", "#FFFFFF"),
        GetConfigValue(configMap, "site_cor_texto", "#0F172A"),
        GetConfigValue(configMap, "site_cor_texto_suave", "#64748B"),
        GetConfigInt(configMap, "home_hero_interval_seconds", 7, 3, 30),
        GetConfigInt(configMap, "site_partner_rotation_seconds", 8, 5, 60),
        heroSlides.Where(slide => slide.Active != false).OrderBy(slide => slide.Order).ToList(),
        storeCards,
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
        partnerCards,
        GetConfigValue(configMap, "home_footer_texto", "Portal em evolução contínua para vendas, relacionamento, parceiros e operações integradas."));
}

static int GetConfigInt(
    IReadOnlyDictionary<string, string?> configMap,
    string key,
    int defaultValue,
    int minimum,
    int maximum)
{
    if (!configMap.TryGetValue(key, out var value)
        || !int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
    {
        return defaultValue;
    }

    return Math.Clamp(parsed, minimum, maximum);
}

static bool TryParseHeroSlidesConfiguration(
    string? json,
    out List<HeroSlideSiteDto> slides,
    out string error)
{
    slides = [];
    error = string.Empty;

    if (string.IsNullOrWhiteSpace(json))
    {
        error = "A configuração home_hero_slides é obrigatória.";
        return false;
    }

    List<HeroSlideSiteDto>? parsed;
    try
    {
        parsed = JsonSerializer.Deserialize<List<HeroSlideSiteDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
    catch (JsonException)
    {
        error = "A configuração home_hero_slides contém JSON inválido.";
        return false;
    }

    if (parsed is null || parsed.Count is < 1 or > 10)
    {
        error = "A home deve possuir entre 1 e 10 slides.";
        return false;
    }

    var identifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var activeCount = 0;
    for (var index = 0; index < parsed.Count; index++)
    {
        var slide = parsed[index];
        var id = slide.Id?.Trim().ToLowerInvariant() ?? string.Empty;
        var badge = slide.Badge?.Trim() ?? string.Empty;
        var title = slide.Title?.Trim() ?? string.Empty;
        var highlight = slide.Highlight?.Trim() ?? string.Empty;
        var description = slide.Description?.Trim() ?? string.Empty;
        var image = slide.Image?.Trim() ?? string.Empty;
        var imageAlt = slide.ImageAlt?.Trim();

        if (id.Length is < 3 or > 64
            || !id.All(character => char.IsAsciiLetterOrDigit(character) || character == '-'))
        {
            error = $"O slide {index + 1} possui identificador inválido. Use de 3 a 64 letras, números ou hífens.";
            return false;
        }

        if (!identifiers.Add(id))
        {
            error = $"O identificador de slide '{id}' está duplicado.";
            return false;
        }

        if (badge.Length is < 2 or > 80
            || title.Length is < 2 or > 120
            || highlight.Length is < 2 or > 120
            || description.Length is < 10 or > 500)
        {
            error = $"O slide {index + 1} possui textos fora dos limites permitidos.";
            return false;
        }

        if (!IsValidPublicSiteImageReference(image))
        {
            error = $"A imagem do slide {index + 1} deve apontar para /uploads, /imagens ou /assets no storage oficial.";
            return false;
        }

        if (!string.IsNullOrEmpty(imageAlt) && imageAlt.Length > 240)
        {
            error = $"O texto alternativo do slide {index + 1} deve possuir até 240 caracteres.";
            return false;
        }

        var isActive = slide.Active ?? true;
        if (isActive)
        {
            activeCount++;
        }

        slides.Add(new HeroSlideSiteDto(
            id,
            badge,
            title,
            highlight,
            description,
            image,
            string.IsNullOrWhiteSpace(imageAlt) ? highlight : imageAlt,
            isActive,
            index));
    }

    if (activeCount == 0)
    {
        slides = [];
        error = "Ao menos um slide deve permanecer ativo.";
        return false;
    }

    return true;
}

static bool IsValidPublicSiteImageReference(string value)
{
    if (!value.StartsWith("/", StringComparison.Ordinal))
    {
        return false;
    }

    return value.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)
        || value.StartsWith("/imagens/", StringComparison.OrdinalIgnoreCase)
        || value.StartsWith("/assets/", StringComparison.OrdinalIgnoreCase);
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

    await NormalizeLegacyZeroDateDefaultsAsync(db, logger);
    await EnsureMarketingSchemaAsync(db);
    await EnsureSiteMediaSchemaAsync(db);
    await EnsureSitePublicProfilesSchemaAsync(db);
    await EnsureAuditSchemaAsync(db, logger);
    await SeedSitePublicProfilesAsync(db);
    await EnsureUsuariosSchemaAsync(db);
    await EnsurePlatformSsoSchemaAsync(db);
    await EnsureGrcIamSchemaAsync(db);
    await EnsureMasterDataSchemaAsync(db);
    await EnsureFicoSchemaAsync(db);
    await EnsureOpsSchemaAsync(db);
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
        .OrderBy(produto => produto.Id)
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
            tipo ENUM('AliExpress','CJDropshipping','Dropi','Cartpanda','Nuvemshop','Shopify','Outro') NOT NULL,
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
        ALTER TABLE dropshipping_config
            MODIFY COLUMN tipo ENUM('AliExpress','CJDropshipping','Dropi','Cartpanda','Nuvemshop','Shopify','Outro') NOT NULL,
            MODIFY COLUMN ativo TINYINT(1) NOT NULL DEFAULT 0;
        UPDATE dropshipping_config SET tipo = 'Shopify' WHERE slug = 'shopify';
        UPDATE dropshipping_config SET tipo = 'CJDropshipping' WHERE slug IN ('cjdropshipping', 'cj-dropshipping');
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        INSERT INTO dropshipping_config (nome, slug, tipo, api_endpoint, ativo)
        VALUES
            ('Shopify', 'shopify', 'Shopify', NULL, 0),
            ('CJ Dropshipping', 'cjdropshipping', 'CJDropshipping', 'https://developers.cjdropshipping.com/api2.0/v1', 0)
        ON DUPLICATE KEY UPDATE
            nome = VALUES(nome),
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
            created_by_user_id CHAR(36) CHARACTER SET ascii COLLATE ascii_general_ci NULL,
            updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            updated_by_user_id CHAR(36) CHARACTER SET ascii COLLATE ascii_general_ci NULL,
            deleted_at DATETIME(6) NULL,
            is_deleted TINYINT(1) NOT NULL DEFAULT 0,
            row_version BLOB NULL,
            tenant_id CHAR(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001',
            PRIMARY KEY (id),
            UNIQUE KEY ux_configuracoes_sistema_tenant_chave (tenant_id, chave),
            KEY ix_configuracoes_sistema_grupo (grupo),
            KEY ix_configuracoes_sistema_tenant_deleted (tenant_id, is_deleted)
        );
        """);

    await db.Database.ExecuteSqlRawAsync(
        "ALTER TABLE configuracoes_sistema MODIFY COLUMN tipo VARCHAR(20) NOT NULL DEFAULT 'Texto';");

    await db.Database.ExecuteSqlRawAsync(
        """
        INSERT INTO configuracoes_sistema (chave, valor, tipo, descricao, grupo, editavel)
        VALUES
            ('site_nome', 'Grupo Nexum Altivon', 'Texto', 'Nome público principal exibido no site', 'Geral', 1),
            ('site_subtitulo', 'Participações societárias', 'Texto', 'Subtítulo discreto da marca no cabeçalho e rodapé', 'Geral', 1),
            ('site_logo', '/uploads/produtos/Logo-2.png', 'Imagem', 'Logo público carregado pela home, cabeçalho e rodapé', 'Geral', 1),
            ('site_institucional_url', '/institucional', 'Texto', 'Link da página institucional', 'SiteHome', 1),
            ('site_politica_privacidade_url', '/politica-privacidade', 'Texto', 'Link da política de privacidade', 'SiteHome', 1),
            ('site_politica_reembolso_url', '/politica-reembolso', 'Texto', 'Link da política de reembolso', 'SiteHome', 1),
            ('site_cor_primaria', '#C9A227', 'Cor', 'Cor principal de ações e destaques do portal', 'Aparencia', 1),
            ('site_cor_secundaria', '#0A0A0A', 'Cor', 'Cor secundária de cabeçalho e navegação', 'Aparencia', 1),
            ('site_cor_fundo', '#F5F7FB', 'Cor', 'Cor de fundo das páginas públicas', 'Aparencia', 1),
            ('site_cor_superficie', '#FFFFFF', 'Cor', 'Cor de superfícies e painéis públicos', 'Aparencia', 1),
            ('site_cor_texto', '#0F172A', 'Cor', 'Cor principal dos textos públicos', 'Aparencia', 1),
            ('site_cor_texto_suave', '#64748B', 'Cor', 'Cor secundária dos textos públicos', 'Aparencia', 1),
            ('site_telefone_secundario', '(14) 99673-1879', 'Texto', 'Telefone comercial secundário', 'Geral', 1),
            ('site_whatsapp_secundario', '5514996731879', 'Texto', 'WhatsApp comercial secundário', 'Geral', 1),
            ('site_yara_email', 'corporativo.gna@gmail.com', 'Texto', 'E-mail de atendimento da Yara', 'Atendimento', 1),
            ('home_intro_titulo', 'Uma Nova Era Começa', 'Texto', 'Título principal do bloco institucional da home', 'SiteHome', 1),
            ('home_intro_texto_1', 'O Grupo Nexum Altivon está chegando para transformar e inovar o mercado digital brasileiro.', 'Texto', 'Primeiro texto institucional da home', 'SiteHome', 1),
            ('home_intro_texto_2', 'Nosso compromisso é claro: entregar qualidade superior, atendimento que faz a diferença e preços acessíveis que respeitam o seu bolso.', 'Texto', 'Segundo texto institucional da home', 'SiteHome', 1),
            ('home_intro_badge', 'nexumaltivon.com.br', 'Texto', 'Texto do selo institucional da home', 'SiteHome', 1),
            ('home_footer_texto', 'Portal em evolução contínua para vendas, relacionamento, parceiros e operações integradas.', 'Texto', 'Texto do rodapé público da home', 'SiteHome', 1),
            ('home_quality_items', '["Curadoria rigorosa de fornecedores","Atendimento humano e especializado","Política de devolução simplificada","Preços justos e acessíveis"]', 'JSON', 'Itens do bloco de qualidade da home', 'SiteHome', 1),
            ('home_lojas_cards', '[{{"nome":"Gran Tur","slug":"gran-tur","segmento":"Viagens & Turismo","descricao":"Mochilas, malas, acessórios de viagem e produtos para explorar o mundo com estilo e conforto.","imagem":"/imagens/homepage/loja-gran-tur.svg","icon":"Plane"}},{{"nome":"Chronos","slug":"chronos","segmento":"Relógios & Acessórios","descricao":"Relógios e acessórios para quem valoriza precisão, presença e elegância.","imagem":"/imagens/homepage/loja-chronos.svg","icon":"Watch"}},{{"nome":"Moda Mim","slug":"moda-mim","segmento":"Moda & Vestuário","descricao":"Roupas, calçados e acessórios para uma experiência de compra prática e atual.","imagem":"/imagens/homepage/loja-moda-mim.svg","icon":"Shirt"}},{{"nome":"Geração Top+","slug":"geracao-top","segmento":"Tecnologia & Gadgets","descricao":"Smartphones, eletrônicos, acessórios e tecnologia para rotina, trabalho e lazer.","imagem":"/imagens/homepage/loja-geracao-top.svg","icon":"Smartphone"}},{{"nome":"Estruturaline","slug":"estruturaline","segmento":"Construção & Estruturas","descricao":"Materiais, ferramentas e soluções para quem constrói com seriedade.","imagem":"/imagens/homepage/loja-estruturaline.svg","icon":"Hammer"}},{{"nome":"Gran Festas","slug":"gran-festas","segmento":"Festas & Eventos","descricao":"Decoração, utensílios e produtos para encontros, comemorações e eventos.","imagem":"/imagens/homepage/loja-gran-festas.svg","icon":"Gift"}}]', 'JSON', 'Cards e imagens das lojas na Home e página Lojas', 'SiteHome', 1),
            ('home_partner_cards', '[{{"title":"Parceiros de Vendas","text":"Lojas físicas ou online podem ampliar seus horizontes de venda com nossa infraestrutura comercial e operação integrada.","cta":"Quero Vender","href":"https://wa.me/5514996731879?text=Olá! Tenho interesse em ser parceiro de vendas do Grupo Nexum Altivon.","icon":"Store"}},{{"title":"Fornecedores & Distribuidores","text":"Distribuidores e fabricantes encontram um canal de venda em crescimento, com visão de volume, relacionamento e longo prazo.","cta":"Quero Fornecer","href":"https://wa.me/5514996731879?text=Olá! Sou fornecedor/distribuidor e tenho interesse em parceria com o Grupo Nexum Altivon.","icon":"Truck"}},{{"title":"Dropshipping","text":"Integre seu catálogo às nossas lojas ou utilize nossa infraestrutura para conectar produtos, logística e novos canais.","cta":"Quero Fazer Dropship","href":"https://wa.me/5514996731879?text=Olá! Tenho interesse em parceria de dropshipping com o Grupo Nexum Altivon.","icon":"Building2"}}]', 'JSON', 'Cards de parceria da home', 'SiteHome', 1),
            ('home_hero_interval_seconds', '7', 'Numero', 'Intervalo automático dos slides da home em segundos', 'SiteHome', 1),
            ('site_partner_rotation_seconds', '8', 'Numero', 'Intervalo do rodízio de parceiros em segundos', 'SiteHome', 1),
            ('home_hero_slides', '[{{"id":"ecommerce","badge":"Grupo Nexum Altivon","title":"O Futuro do","highlight":"E-Commerce","description":"Seis lojas, uma operação conectada e uma proposta premium para transformar a experiência de compra online.","image":"/uploads/Imagens/Gemini_Generated_Image_aowvz5aowvz5aowv.jfif","imageAlt":"Tecnologia comercializada pelo Grupo Nexum Altivon","active":true,"order":0}},{{"id":"marcas","badge":"6 marcas em expansão","title":"Uma operação,","highlight":"múltiplos mercados","description":"Turismo, relógios, moda, tecnologia, construção e festas com a mesma curadoria comercial do Grupo Nexum Altivon.","image":"/uploads/Imagens/Gemini_Generated_Image_miqkz6miqkz6miqk.jfif","imageAlt":"Experiências oferecidas pelas marcas do Grupo Nexum Altivon","active":true,"order":1}},{{"id":"tecnologia","badge":"Experiência tecnológica","title":"Compra segura com","highlight":"atendimento humano","description":"Fluxos preparados para catálogo, clientes, pedidos, integrações e relacionamento com visão de crescimento contínuo.","image":"/uploads/Imagens/Gemini_Generated_Image_19y5zi19y5zi19y5.jfif","imageAlt":"Jornada integrada do Grupo Nexum Altivon","active":true,"order":2}}]', 'JSON', 'Slides principais da home', 'SiteHome', 1)
        ON DUPLICATE KEY UPDATE
            valor = CASE
                WHEN chave = 'home_hero_slides'
                    AND (valor IS NULL OR TRIM(valor) IN ('', '[]') OR JSON_VALID(valor) = 0)
                THEN VALUES(valor)
                ELSE valor
            END,
            descricao = VALUES(descricao),
            grupo = VALUES(grupo),
            editavel = VALUES(editavel),
            updated_at = CURRENT_TIMESTAMP;
        """);

    await EnsureComprasSchemaAsync(db);
    await EnsureValidationTokensAsync(db);
    await EnsurePedidosReceberAsync(db);
    await EnsureGenesisSharedSchemaAsync(scope.ServiceProvider, logger);
}

static async Task EnsureAuditSchemaAsync(NexumDbContext db, ILogger logger)
{
    var tableNames = db.Model.GetEntityTypes()
        .Select(entity => entity.GetTableName())
        .Where(tableName => !string.IsNullOrWhiteSpace(tableName))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(tableName => tableName, StringComparer.OrdinalIgnoreCase)
        .ToList();

    foreach (var tableName in tableNames)
    {
        var tableIdentifier = QuoteMySqlIdentifier(tableName);
        var alterTableSql =
            """
            ALTER TABLE
            """ + " " + tableIdentifier +
            """
                ADD COLUMN IF NOT EXISTS `tenant_id` CHAR(36) NOT NULL DEFAULT '
            """ + TenantContext.DefaultTenantId +
            """
            ',
                ADD COLUMN IF NOT EXISTS `row_version` BLOB NULL,
                ADD COLUMN IF NOT EXISTS `created_by_user_id` CHAR(36) NULL,
                ADD COLUMN IF NOT EXISTS `updated_by_user_id` CHAR(36) NULL,
                ADD COLUMN IF NOT EXISTS `is_deleted` TINYINT(1) NOT NULL DEFAULT 0,
                ADD COLUMN IF NOT EXISTS `deleted_at` DATETIME NULL
            """;

        await db.Database.ExecuteSqlRawAsync(alterTableSql);

        var indexName = $"ix_{tableName}_tenant_deleted";
        var indexExists = await ExecuteScalarAsync<long>(
            db,
            """
            SELECT COUNT(*)
            FROM information_schema.statistics
            WHERE table_schema = DATABASE()
              AND table_name = @tableName
              AND index_name = @indexName
            """,
            CancellationToken.None,
            ("@tableName", tableName),
            ("@indexName", indexName));

        if (indexExists == 0)
        {
            var createIndexSql = "CREATE INDEX " + QuoteMySqlIdentifier(indexName) + " ON " + tableIdentifier + " (`tenant_id`, `is_deleted`)";
            await db.Database.ExecuteSqlRawAsync(createIndexSql);
        }
    }

    logger.LogInformation("Auditoria, multitenancy e soft-delete verificados em {Total} tabelas EF.", tableNames.Count);
}

static async Task EnsureSiteMediaSchemaAsync(NexumDbContext db)
{
    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS site_midias (
            id CHAR(36) NOT NULL,
            tenant_id CHAR(36) NOT NULL,
            row_version BLOB NOT NULL,
            created_at DATETIME(6) NOT NULL,
            created_by_user_id CHAR(36) NULL,
            updated_at DATETIME(6) NULL,
            updated_by_user_id CHAR(36) NULL,
            is_deleted TINYINT(1) NOT NULL DEFAULT 0,
            deleted_at DATETIME(6) NULL,
            nome VARCHAR(150) NOT NULL,
            tipo VARCHAR(30) NOT NULL,
            texto_alternativo VARCHAR(240) NOT NULL,
            nome_arquivo_original VARCHAR(255) NOT NULL,
            caminho_relativo VARCHAR(255) NOT NULL,
            content_type VARCHAR(80) NOT NULL,
            tamanho_bytes BIGINT NOT NULL,
            largura INT NOT NULL,
            altura INT NOT NULL,
            PRIMARY KEY (id),
            UNIQUE KEY ux_site_midias_tenant_path (tenant_id, caminho_relativo),
            KEY ix_site_midias_tenant_created (tenant_id, created_at),
            KEY ix_site_midias_tenant_deleted (tenant_id, is_deleted)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);
}

static async Task EnsureSitePublicProfilesSchemaAsync(NexumDbContext db)
{
    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS site_perfis_publicos (
            id CHAR(36) NOT NULL,
            tenant_id CHAR(36) NOT NULL,
            row_version BLOB NOT NULL,
            created_at DATETIME(6) NOT NULL,
            created_by_user_id CHAR(36) NULL,
            updated_at DATETIME(6) NULL,
            updated_by_user_id CHAR(36) NULL,
            is_deleted TINYINT(1) NOT NULL DEFAULT 0,
            deleted_at DATETIME(6) NULL,
            tipo_perfil VARCHAR(30) NOT NULL,
            origem_tipo VARCHAR(30) NOT NULL,
            origem_id INT NULL,
            nome VARCHAR(160) NOT NULL,
            slug VARCHAR(80) NOT NULL,
            segmento VARCHAR(120) NOT NULL,
            atividade VARCHAR(240) NOT NULL,
            descricao VARCHAR(2000) NOT NULL,
            logo_url VARCHAR(500) NULL,
            banner_url VARCHAR(500) NULL,
            icone VARCHAR(80) NULL,
            cta_texto VARCHAR(80) NULL,
            cta_url VARCHAR(500) NULL,
            site_url VARCHAR(500) NULL,
            email_publico VARCHAR(254) NULL,
            telefone_publico VARCHAR(30) NULL,
            endereco_publico VARCHAR(300) NULL,
            cor_primaria VARCHAR(7) NULL,
            cor_secundaria VARCHAR(7) NULL,
            cor_fundo VARCHAR(7) NULL,
            cor_texto VARCHAR(7) NULL,
            publicado TINYINT(1) NOT NULL DEFAULT 0,
            ordem_exibicao INT NOT NULL DEFAULT 0,
            PRIMARY KEY (id),
            UNIQUE KEY ux_site_perfis_tenant_slug (tenant_id, slug),
            UNIQUE KEY ux_site_perfis_tenant_origem (tenant_id, origem_tipo, origem_id),
            KEY ix_site_perfis_tenant_tipo_publicado_ordem (tenant_id, tipo_perfil, publicado, ordem_exibicao),
            KEY ix_site_perfis_tenant_deleted (tenant_id, is_deleted)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        ALTER TABLE site_perfis_publicos
            ADD COLUMN IF NOT EXISTS site_url VARCHAR(500) NULL AFTER cta_url,
            ADD COLUMN IF NOT EXISTS email_publico VARCHAR(254) NULL AFTER site_url,
            ADD COLUMN IF NOT EXISTS telefone_publico VARCHAR(30) NULL AFTER email_publico,
            ADD COLUMN IF NOT EXISTS endereco_publico VARCHAR(300) NULL AFTER telefone_publico,
            ADD COLUMN IF NOT EXISTS cor_primaria VARCHAR(7) NULL AFTER endereco_publico,
            ADD COLUMN IF NOT EXISTS cor_secundaria VARCHAR(7) NULL AFTER cor_primaria,
            ADD COLUMN IF NOT EXISTS cor_fundo VARCHAR(7) NULL AFTER cor_secundaria,
            ADD COLUMN IF NOT EXISTS cor_texto VARCHAR(7) NULL AFTER cor_fundo;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS site_perfis_publicos_produtos (
            id CHAR(36) NOT NULL,
            tenant_id CHAR(36) NOT NULL,
            row_version BLOB NOT NULL,
            created_at DATETIME(6) NOT NULL,
            created_by_user_id CHAR(36) NULL,
            updated_at DATETIME(6) NULL,
            updated_by_user_id CHAR(36) NULL,
            is_deleted TINYINT(1) NOT NULL DEFAULT 0,
            deleted_at DATETIME(6) NULL,
            perfil_publico_id CHAR(36) NOT NULL,
            produto_id INT NOT NULL,
            publicado TINYINT(1) NOT NULL DEFAULT 1,
            ordem_exibicao INT NOT NULL DEFAULT 0,
            PRIMARY KEY (id),
            UNIQUE KEY ux_site_perfis_produtos_tenant_perfil_produto (tenant_id, perfil_publico_id, produto_id),
            KEY ix_site_perfis_produtos_tenant_perfil_publicado_ordem (tenant_id, perfil_publico_id, publicado, ordem_exibicao),
            KEY ix_site_perfis_produtos_tenant_deleted (tenant_id, is_deleted),
            CONSTRAINT fk_site_perfis_produtos_perfil FOREIGN KEY (perfil_publico_id) REFERENCES site_perfis_publicos (id) ON DELETE RESTRICT,
            CONSTRAINT fk_site_perfis_produtos_produto FOREIGN KEY (produto_id) REFERENCES produtos (id) ON DELETE RESTRICT
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);
}

static async Task SeedSitePublicProfilesAsync(NexumDbContext db)
{
    await db.Database.ExecuteSqlRawAsync(
        """
        INSERT INTO site_perfis_publicos (
            id,
            tenant_id,
            row_version,
            created_at,
            tipo_perfil,
            origem_tipo,
            origem_id,
            nome,
            slug,
            segmento,
            atividade,
            descricao,
            logo_url,
            banner_url,
            icone,
            cta_texto,
            cta_url,
            publicado,
            ordem_exibicao)
        SELECT
            UUID(),
            l.tenant_id,
            UNHEX(REPLACE(UUID(), '-', '')),
            UTC_TIMESTAMP(6),
            'Loja',
            'Loja',
            l.id,
            l.nome,
            l.slug,
            l.segmento,
            CONCAT('Atuação comercial em ', l.segmento),
            COALESCE(NULLIF(TRIM(l.descricao), ''), CONCAT(l.nome, ' integra a operação comercial do Grupo Nexum Altivon.')),
            NULLIF(TRIM(l.logo), ''),
            CASE l.slug
                WHEN 'chronos' THEN '/uploads/Imagens/Gemini_Generated_Image_shv10ashv10ashv1.png'
                WHEN 'grann-tur' THEN '/uploads/Imagens/Gemini_Generated_Image_cnu682cnu682cnu6.jfif'
                WHEN 'gran-tur' THEN '/uploads/Imagens/Gemini_Generated_Image_cnu682cnu682cnu6.jfif'
                WHEN 'moda-mim' THEN '/uploads/Imagens/Gemini_Generated_Image_lbd948lbd948lbd9.jfif'
                WHEN 'geracao-top' THEN '/uploads/Imagens/Gemini_Generated_Image_aowvz5aowvz5aowv.jfif'
                WHEN 'estruturaline' THEN '/uploads/Imagens/Gemini_Generated_Image_s9ijb8s9ijb8s9ij.jfif'
                WHEN 'gran-fest' THEN '/uploads/Imagens/Gemini_Generated_Image_miqkz6miqkz6miqk.jfif'
                WHEN 'gran-festas' THEN '/uploads/Imagens/Gemini_Generated_Image_miqkz6miqkz6miqk.jfif'
                ELSE NULLIF(TRIM(l.banner), '')
            END,
            CASE l.slug
                WHEN 'chronos' THEN 'Watch'
                WHEN 'grann-tur' THEN 'Plane'
                WHEN 'gran-tur' THEN 'Plane'
                WHEN 'moda-mim' THEN 'Shirt'
                WHEN 'geracao-top' THEN 'Smartphone'
                WHEN 'estruturaline' THEN 'Hammer'
                WHEN 'gran-fest' THEN 'Gift'
                WHEN 'gran-festas' THEN 'Gift'
                ELSE 'Store'
            END,
            'Conhecer loja',
            CONCAT('/lojas/', l.slug),
            l.ativa,
            l.ordem_exibicao
        FROM lojas l
        WHERE l.is_deleted = 0
          AND NOT EXISTS (
              SELECT 1
              FROM site_perfis_publicos p
              WHERE p.tenant_id = l.tenant_id
                AND p.origem_tipo = 'Loja'
                AND p.origem_id = l.id
                AND p.is_deleted = 0
          );
        """);
}

static async Task EnsureMarketingSchemaAsync(NexumDbContext db)
{
    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS crm_segmentos (
            id CHAR(36) NOT NULL,
            tenant_id CHAR(36) NOT NULL,
            row_version BLOB NOT NULL,
            nome VARCHAR(100) NOT NULL,
            descricao VARCHAR(500) NULL,
            cor VARCHAR(20) NOT NULL DEFAULT '#C9A227',
            prioridade INT NOT NULL DEFAULT 1,
            ticket_medio_minimo DECIMAL(15,2) NULL,
            ticket_medio_maximo DECIMAL(15,2) NULL,
            frequencia_minima_dias INT NULL,
            frequencia_maxima_dias INT NULL,
            ativo TINYINT(1) NOT NULL DEFAULT 1,
            created_at DATETIME NOT NULL,
            created_by_user_id CHAR(36) NULL,
            updated_at DATETIME NULL,
            updated_by_user_id CHAR(36) NULL,
            is_deleted TINYINT(1) NOT NULL DEFAULT 0,
            deleted_at DATETIME NULL,
            PRIMARY KEY (id),
            UNIQUE KEY ux_crm_segmentos_tenant_nome (tenant_id, nome),
            KEY ix_crm_segmentos_tenant_deleted (tenant_id, is_deleted)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS crm_campanhas (
            id CHAR(36) NOT NULL,
            tenant_id CHAR(36) NOT NULL,
            row_version BLOB NOT NULL,
            nome VARCHAR(200) NOT NULL,
            descricao VARCHAR(1000) NULL,
            tipo VARCHAR(40) NOT NULL,
            status VARCHAR(40) NOT NULL,
            segmento_id CHAR(36) NULL,
            data_inicio DATETIME NOT NULL,
            data_fim DATETIME NULL,
            orcamento DECIMAL(15,2) NOT NULL DEFAULT 0.00,
            custo_atual DECIMAL(15,2) NOT NULL DEFAULT 0.00,
            alcance INT NOT NULL DEFAULT 0,
            cliques INT NOT NULL DEFAULT 0,
            leads_gerados INT NOT NULL DEFAULT 0,
            oportunidades_geradas INT NOT NULL DEFAULT 0,
            vendas_geradas INT NOT NULL DEFAULT 0,
            receita_gerada DECIMAL(15,2) NOT NULL DEFAULT 0.00,
            publico_alvo VARCHAR(1000) NULL,
            conteudo LONGTEXT NULL,
            created_at DATETIME NOT NULL,
            created_by_user_id CHAR(36) NULL,
            updated_at DATETIME NULL,
            updated_by_user_id CHAR(36) NULL,
            is_deleted TINYINT(1) NOT NULL DEFAULT 0,
            deleted_at DATETIME NULL,
            PRIMARY KEY (id),
            UNIQUE KEY ux_crm_campanhas_tenant_nome (tenant_id, nome),
            KEY ix_crm_campanhas_tenant_deleted (tenant_id, is_deleted),
            KEY ix_crm_campanhas_segmento (segmento_id),
            CONSTRAINT fk_crm_campanhas_segmento FOREIGN KEY (segmento_id) REFERENCES crm_segmentos (id)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);
}

static async Task EnsureGrcIamSchemaAsync(NexumDbContext db)
{
    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS adm_perfis (
            prf_id INT NOT NULL AUTO_INCREMENT,
            prf_nome VARCHAR(100) NOT NULL,
            prf_descricao TEXT NULL,
            prf_alcada_maxima DECIMAL(15,2) NOT NULL DEFAULT 0.00,
            prf_nivel_hierarquico INT NOT NULL DEFAULT 1,
            prf_ativo TINYINT(1) NOT NULL DEFAULT 1,
            prf_data_cadastro TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            PRIMARY KEY (prf_id),
            UNIQUE KEY ux_adm_perfis_nome (prf_nome)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS adm_permissoes (
            prm_id INT NOT NULL AUTO_INCREMENT,
            prm_modulo VARCHAR(50) NOT NULL,
            prm_funcionalidade VARCHAR(100) NOT NULL,
            prm_chave VARCHAR(100) NOT NULL,
            prm_descricao TEXT NULL,
            prm_ativo TINYINT(1) NOT NULL DEFAULT 1,
            PRIMARY KEY (prm_id),
            UNIQUE KEY ux_adm_permissoes_chave (prm_chave),
            KEY ix_adm_permissoes_modulo (prm_modulo)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS adm_perfil_permissoes (
            ppr_id INT NOT NULL AUTO_INCREMENT,
            ppr_perfil_id INT NOT NULL,
            ppr_permissao_id INT NOT NULL,
            ppr_leitura TINYINT(1) NOT NULL DEFAULT 0,
            ppr_escrita TINYINT(1) NOT NULL DEFAULT 0,
            ppr_exclusao TINYINT(1) NOT NULL DEFAULT 0,
            ppr_impressao TINYINT(1) NOT NULL DEFAULT 0,
            PRIMARY KEY (ppr_id),
            UNIQUE KEY ux_adm_perfil_permissoes_perfil_permissao (ppr_perfil_id, ppr_permissao_id),
            KEY ix_adm_perfil_permissoes_permissao (ppr_permissao_id)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync("ALTER TABLE adm_perfis ADD COLUMN IF NOT EXISTS prf_alcada_maxima DECIMAL(15,2) NOT NULL DEFAULT 0.00;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE adm_perfis ADD COLUMN IF NOT EXISTS prf_nivel_hierarquico INT NOT NULL DEFAULT 1;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE adm_perfis ADD COLUMN IF NOT EXISTS prf_ativo TINYINT(1) NOT NULL DEFAULT 1;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE adm_perfis ADD COLUMN IF NOT EXISTS prf_data_cadastro TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE adm_permissoes ADD COLUMN IF NOT EXISTS prm_ativo TINYINT(1) NOT NULL DEFAULT 1;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE adm_perfil_permissoes ADD COLUMN IF NOT EXISTS ppr_leitura TINYINT(1) NOT NULL DEFAULT 0;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE adm_perfil_permissoes ADD COLUMN IF NOT EXISTS ppr_escrita TINYINT(1) NOT NULL DEFAULT 0;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE adm_perfil_permissoes ADD COLUMN IF NOT EXISTS ppr_exclusao TINYINT(1) NOT NULL DEFAULT 0;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE adm_perfil_permissoes ADD COLUMN IF NOT EXISTS ppr_impressao TINYINT(1) NOT NULL DEFAULT 0;");

    await EnsureGrcIamKeysAsync(db);

    await db.Database.ExecuteSqlRawAsync(
        """
        INSERT IGNORE INTO adm_perfis (prf_nome, prf_descricao, prf_alcada_maxima, prf_nivel_hierarquico, prf_ativo)
        VALUES
            ('Admin', 'Administracao geral do GenesisGest.Net e Nexum.', 999999999.99, 1, 1),
            ('Gerente', 'Gestao operacional, comercial, compras, estoque e aprovacoes.', 250000.00, 2, 1),
            ('Financeiro', 'Tesouraria, contas a pagar, receber, conciliacao e DRE.', 100000.00, 3, 1),
            ('Fiscal', 'Emissao, conferencia fiscal, SPED, ECF e documentos fiscais.', 100000.00, 3, 1),
            ('RH', 'Departamento pessoal, folha, ponto, admissoes e desligamentos.', 50000.00, 3, 1),
            ('Vendedor', 'Venda, atendimento, CRM inicial e pedidos.', 10000.00, 5, 1)
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        INSERT IGNORE INTO adm_permissoes (prm_modulo, prm_funcionalidade, prm_chave, prm_descricao, prm_ativo)
        VALUES
            ('GRC', 'Administrar perfis', 'GRC_PERFIS_ADMIN', 'Cria e ajusta perfis corporativos.', 1),
            ('GRC', 'Administrar permissoes', 'GRC_PERMISSOES_ADMIN', 'Mantem catalogo de permissoes e matriz RBAC.', 1),
            ('AUDITORIA', 'Consultar trilha', 'AUDITORIA_CONSULTAR', 'Consulta trilha de auditoria operacional.', 1),
            ('FINANCEIRO', 'Aprovar pagamentos', 'FIN_PAGAMENTOS_APROVAR', 'Autoriza pagamentos conforme alcada.', 1),
            ('FINANCEIRO', 'Executar pagamentos', 'FIN_PAGAMENTOS_EXECUTAR', 'Executa remessas e baixas financeiras.', 1),
            ('FISCAL', 'Emitir documentos fiscais', 'FIS_DOCUMENTOS_EMITIR', 'Emite NF-e, NFC-e, CT-e e MDF-e.', 1),
            ('ESTOQUE', 'Movimentar estoque', 'EST_MOVIMENTACOES_ADMIN', 'Administra entradas, saidas, inventario e transferencias.', 1),
            ('COMPRAS', 'Aprovar compras', 'SCM_COMPRAS_APROVAR', 'Aprova cotacoes, pedidos e entradas.', 1),
            ('RH', 'Gerenciar folha', 'RH_FOLHA_ADMIN', 'Acessa folha, ponto e eventos de pessoal.', 1)
        """);
}

static async Task EnsureGrcIamKeysAsync(NexumDbContext db)
{
    var tables = new[]
    {
        new GrcIdentityTable(
            "adm_perfis",
            "prf_id",
            "SELECT COUNT(*) AS Value FROM `adm_perfis`",
            "ALTER TABLE `adm_perfis` MODIFY COLUMN `prf_id` INT NOT NULL AUTO_INCREMENT, ADD PRIMARY KEY (`prf_id`);",
            "ALTER TABLE `adm_perfis` MODIFY COLUMN `prf_id` INT NOT NULL AUTO_INCREMENT;"),
        new GrcIdentityTable(
            "adm_permissoes",
            "prm_id",
            "SELECT COUNT(*) AS Value FROM `adm_permissoes`",
            "ALTER TABLE `adm_permissoes` MODIFY COLUMN `prm_id` INT NOT NULL AUTO_INCREMENT, ADD PRIMARY KEY (`prm_id`);",
            "ALTER TABLE `adm_permissoes` MODIFY COLUMN `prm_id` INT NOT NULL AUTO_INCREMENT;"),
        new GrcIdentityTable(
            "adm_perfil_permissoes",
            "ppr_id",
            "SELECT COUNT(*) AS Value FROM `adm_perfil_permissoes`",
            "ALTER TABLE `adm_perfil_permissoes` MODIFY COLUMN `ppr_id` INT NOT NULL AUTO_INCREMENT, ADD PRIMARY KEY (`ppr_id`);",
            "ALTER TABLE `adm_perfil_permissoes` MODIFY COLUMN `ppr_id` INT NOT NULL AUTO_INCREMENT;")
    };

    foreach (var table in tables)
    {
        var hasPrimaryKey = await GrcSchemaCountAsync(
            db,
            "SELECT COUNT(*) AS Value FROM information_schema.statistics WHERE table_schema = DATABASE() AND table_name = {0} AND index_name = 'PRIMARY'",
            table.Name) > 0;
        var hasAutoIncrement = await GrcSchemaCountAsync(
            db,
            "SELECT COUNT(*) AS Value FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = {0} AND column_name = {1} AND extra LIKE '%auto_increment%'",
            table.Name,
            table.IdentityColumn) > 0;

        if (!hasPrimaryKey)
        {
            var rowCount = await db.Database.SqlQueryRaw<long>(table.RowCountSql).SingleAsync();
            if (rowCount > 0)
            {
                throw new InvalidOperationException(
                    $"A tabela {table.Name} possui {rowCount} linha(s), mas nao possui chave primaria. A inicializacao foi bloqueada para impedir reenumeracao e perda de relacionamento.");
            }

            await db.Database.ExecuteSqlRawAsync(table.AddPrimaryKeySql);
            hasAutoIncrement = true;
        }

        if (!hasAutoIncrement)
        {
            await db.Database.ExecuteSqlRawAsync(table.EnsureAutoIncrementSql);
        }
    }

    await EnsureGrcIndexAsync(db, "adm_perfis", "ux_adm_perfis_nome", "ALTER TABLE `adm_perfis` ADD UNIQUE KEY `ux_adm_perfis_nome` (`prf_nome`);");
    await EnsureGrcIndexAsync(db, "adm_permissoes", "ux_adm_permissoes_chave", "ALTER TABLE `adm_permissoes` ADD UNIQUE KEY `ux_adm_permissoes_chave` (`prm_chave`);");
    await EnsureGrcIndexAsync(db, "adm_permissoes", "ix_adm_permissoes_modulo", "ALTER TABLE `adm_permissoes` ADD KEY `ix_adm_permissoes_modulo` (`prm_modulo`);");
    await EnsureGrcIndexAsync(db, "adm_perfil_permissoes", "ux_adm_perfil_permissoes_perfil_permissao", "ALTER TABLE `adm_perfil_permissoes` ADD UNIQUE KEY `ux_adm_perfil_permissoes_perfil_permissao` (`ppr_perfil_id`, `ppr_permissao_id`);");
    await EnsureGrcIndexAsync(db, "adm_perfil_permissoes", "ix_adm_perfil_permissoes_permissao", "ALTER TABLE `adm_perfil_permissoes` ADD KEY `ix_adm_perfil_permissoes_permissao` (`ppr_permissao_id`);");
    await EnsureGrcForeignKeyAsync(db, "fk_adm_perfil_permissoes_perfil", "ALTER TABLE `adm_perfil_permissoes` ADD CONSTRAINT `fk_adm_perfil_permissoes_perfil` FOREIGN KEY (`ppr_perfil_id`) REFERENCES `adm_perfis` (`prf_id`);");
    await EnsureGrcForeignKeyAsync(db, "fk_adm_perfil_permissoes_permissao", "ALTER TABLE `adm_perfil_permissoes` ADD CONSTRAINT `fk_adm_perfil_permissoes_permissao` FOREIGN KEY (`ppr_permissao_id`) REFERENCES `adm_permissoes` (`prm_id`);");
}

static async Task<long> GrcSchemaCountAsync(NexumDbContext db, string sql, params object[] parameters) =>
    await db.Database.SqlQueryRaw<long>(sql, parameters).SingleAsync();

static async Task EnsureGrcIndexAsync(NexumDbContext db, string table, string index, string command)
{
    var exists = await GrcSchemaCountAsync(
        db,
        "SELECT COUNT(*) AS Value FROM information_schema.statistics WHERE table_schema = DATABASE() AND table_name = {0} AND index_name = {1}",
        table,
        index) > 0;
    if (!exists)
    {
        await db.Database.ExecuteSqlRawAsync(command);
    }
}

static async Task EnsureGrcForeignKeyAsync(NexumDbContext db, string constraint, string command)
{
    var exists = await GrcSchemaCountAsync(
        db,
        "SELECT COUNT(*) AS Value FROM information_schema.referential_constraints WHERE constraint_schema = DATABASE() AND constraint_name = {0}",
        constraint) > 0;
    if (!exists)
    {
        await db.Database.ExecuteSqlRawAsync(command);
    }
}

static async Task EnsureMasterDataSchemaAsync(NexumDbContext db)
{
    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS adm_pessoas_empresas (
            pes_id INT NOT NULL AUTO_INCREMENT,
            pes_tipo ENUM('FISICA','JURIDICA') NOT NULL,
            pes_nome_razao VARCHAR(200) NOT NULL,
            pes_nome_fantasia VARCHAR(200) NULL,
            pes_cpf_cnpj VARCHAR(20) NULL,
            pes_rg_ie VARCHAR(30) NULL,
            pes_cliente TINYINT(1) NOT NULL DEFAULT 0,
            pes_fornecedor TINYINT(1) NOT NULL DEFAULT 0,
            pes_colaborador TINYINT(1) NOT NULL DEFAULT 0,
            pes_transportadora TINYINT(1) NOT NULL DEFAULT 0,
            pes_endereco VARCHAR(200) NULL,
            pes_numero VARCHAR(20) NULL,
            pes_complemento VARCHAR(100) NULL,
            pes_bairro VARCHAR(100) NULL,
            pes_cidade VARCHAR(100) NULL,
            pes_uf CHAR(2) NULL,
            pes_cep VARCHAR(12) NULL,
            pes_telefone VARCHAR(30) NULL,
            pes_celular VARCHAR(30) NULL,
            pes_email VARCHAR(150) NULL,
            pes_site VARCHAR(150) NULL,
            pes_observacoes TEXT NULL,
            pes_ativo TINYINT(1) NOT NULL DEFAULT 1,
            pes_data_cadastro TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            pes_data_atualizacao TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (pes_id),
            KEY ix_adm_pessoas_empresas_documento (pes_cpf_cnpj),
            KEY ix_adm_pessoas_empresas_tipo (pes_tipo, pes_ativo)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS fin_centros_custo (
            ccu_id INT NOT NULL AUTO_INCREMENT,
            ccu_codigo VARCHAR(50) NOT NULL,
            ccu_nome VARCHAR(150) NOT NULL,
            ccu_descricao TEXT NULL,
            ccu_observacoes TEXT NULL,
            ccu_tipo ENUM('CUSTO','LUCRO','INVESTIMENTO','OPERACIONAL','ADMINISTRATIVO') NOT NULL DEFAULT 'CUSTO',
            ccu_pai_id INT NULL,
            ccu_responsavel_usr_id INT NULL,
            ccu_status CHAR(1) NOT NULL DEFAULT 'A',
            ccu_data_cadastro TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            ccu_data_alteracao TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            ccu_data_exclusao TIMESTAMP NULL,
            ccu_data_inclusao TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
            PRIMARY KEY (ccu_id),
            UNIQUE KEY ux_fin_centros_custo_codigo (ccu_codigo),
            KEY ix_fin_centros_custo_status (ccu_status, ccu_data_exclusao)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS vnd_itens (
            itm_id INT NOT NULL AUTO_INCREMENT,
            itm_emp_id INT NOT NULL DEFAULT 1,
            itm_codigo VARCHAR(50) NOT NULL,
            itm_tipo ENUM('PRODUTO','SERVICO','MATERIA_PRIMA','INSUMO') NOT NULL,
            itm_descricao VARCHAR(200) NOT NULL,
            itm_descricao_detalhada TEXT NULL,
            itm_unidade VARCHAR(10) NOT NULL DEFAULT 'UN',
            itm_ncm VARCHAR(10) NULL,
            itm_cest VARCHAR(10) NULL,
            itm_peso_bruto DECIMAL(10,3) NULL,
            itm_peso_liquido DECIMAL(10,3) NULL,
            itm_altura DECIMAL(10,2) NULL,
            itm_largura DECIMAL(10,2) NULL,
            itm_profundidade DECIMAL(10,2) NULL,
            itm_controla_estoque TINYINT(1) NOT NULL DEFAULT 1,
            itm_controla_lote TINYINT(1) NOT NULL DEFAULT 0,
            itm_controla_serie TINYINT(1) NOT NULL DEFAULT 0,
            itm_ativo TINYINT(1) NOT NULL DEFAULT 1,
            itm_data_cadastro TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            PRIMARY KEY (itm_id),
            UNIQUE KEY ux_vnd_itens_codigo_empresa (itm_emp_id, itm_codigo),
            KEY ix_vnd_itens_tipo (itm_tipo, itm_ativo)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS md_produtos_precos_loja (
            ppl_id INT NOT NULL AUTO_INCREMENT,
            ppl_item_id INT NOT NULL,
            ppl_loja_id INT NOT NULL,
            ppl_preco_venda DECIMAL(15,2) NOT NULL DEFAULT 0.00,
            ppl_preco_promocional DECIMAL(15,2) NULL,
            ppl_preco_custo DECIMAL(15,2) NULL,
            ppl_margem_percentual DECIMAL(5,2) NULL,
            ppl_ativo TINYINT(1) NOT NULL DEFAULT 1,
            ppl_atualizado_em TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (ppl_id),
            UNIQUE KEY ux_md_produtos_precos_loja_item_loja (ppl_item_id, ppl_loja_id),
            KEY ix_md_produtos_precos_loja_loja (ppl_loja_id, ppl_ativo)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS md_fornecedor_contatos (
            fco_id INT NOT NULL AUTO_INCREMENT,
            fco_fornecedor_id INT NOT NULL,
            fco_nome VARCHAR(150) NOT NULL,
            fco_cargo VARCHAR(100) NULL,
            fco_email VARCHAR(150) NULL,
            fco_telefone VARCHAR(30) NULL,
            fco_celular VARCHAR(30) NULL,
            fco_principal TINYINT(1) NOT NULL DEFAULT 0,
            fco_ativo TINYINT(1) NOT NULL DEFAULT 1,
            fco_tenant_id CHAR(36) NOT NULL,
            fco_row_version BINARY(16) NOT NULL,
            fco_created_by_user_id CHAR(36) NULL,
            fco_updated_by_user_id CHAR(36) NULL,
            fco_criado_em TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            fco_atualizado_em TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            fco_is_deleted TINYINT(1) NOT NULL DEFAULT 0,
            fco_deleted_at DATETIME NULL,
            PRIMARY KEY (fco_id),
            KEY ix_md_fornecedor_contatos_fornecedor (fco_fornecedor_id, fco_ativo),
            KEY ix_md_fornecedor_contatos_tenant_deleted (fco_tenant_id, fco_is_deleted)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await EnsureIntPrimaryKeyAsync(db, "adm_pessoas_empresas", "pes_id");
    await EnsureIntPrimaryKeyAsync(db, "fin_centros_custo", "ccu_id");
    await EnsureIntPrimaryKeyAsync(db, "vnd_itens", "itm_id");

    await db.Database.ExecuteSqlRawAsync("ALTER TABLE adm_pessoas_empresas ADD COLUMN IF NOT EXISTS pes_ativo TINYINT(1) NOT NULL DEFAULT 1;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE adm_pessoas_empresas ADD COLUMN IF NOT EXISTS pes_data_atualizacao TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE fin_centros_custo ADD COLUMN IF NOT EXISTS ccu_data_exclusao TIMESTAMP NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE vnd_itens ADD COLUMN IF NOT EXISTS itm_ativo TINYINT(1) NOT NULL DEFAULT 1;");
    await db.Database.ExecuteSqlRawAsync(
        """
        ALTER TABLE md_fornecedor_contatos
            ADD COLUMN IF NOT EXISTS fco_tenant_id CHAR(36) NULL,
            ADD COLUMN IF NOT EXISTS fco_row_version BINARY(16) NULL,
            ADD COLUMN IF NOT EXISTS fco_created_by_user_id CHAR(36) NULL,
            ADD COLUMN IF NOT EXISTS fco_updated_by_user_id CHAR(36) NULL,
            ADD COLUMN IF NOT EXISTS fco_criado_em TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
            ADD COLUMN IF NOT EXISTS fco_is_deleted TINYINT(1) NOT NULL DEFAULT 0,
            ADD COLUMN IF NOT EXISTS fco_deleted_at DATETIME NULL;
        """);
    var defaultTenantId = TenantContext.DefaultTenantId.ToString();
    await db.Database.ExecuteSqlInterpolatedAsync(
        $"UPDATE md_fornecedor_contatos SET fco_tenant_id = {defaultTenantId} WHERE fco_tenant_id IS NULL OR fco_tenant_id = '';");
    await db.Database.ExecuteSqlRawAsync(
        "UPDATE md_fornecedor_contatos SET fco_row_version = UNHEX(REPLACE(UUID(), '-', '')) WHERE fco_row_version IS NULL OR OCTET_LENGTH(fco_row_version) <> 16;");
    await db.Database.ExecuteSqlRawAsync(
        "ALTER TABLE md_fornecedor_contatos MODIFY COLUMN fco_tenant_id CHAR(36) NOT NULL, MODIFY COLUMN fco_row_version BINARY(16) NOT NULL, MODIFY COLUMN fco_criado_em TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP;");
    await EnsureGrcIndexAsync(
        db,
        "md_fornecedor_contatos",
        "ix_md_fornecedor_contatos_tenant_deleted",
        "ALTER TABLE md_fornecedor_contatos ADD KEY ix_md_fornecedor_contatos_tenant_deleted (fco_tenant_id, fco_is_deleted);");
}

static async Task EnsureFicoSchemaAsync(NexumDbContext db)
{
    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS cnt_lancamentos (
            lcn_id INT NOT NULL AUTO_INCREMENT,
            lcn_emp_id INT NOT NULL,
            lcn_lote VARCHAR(20) NOT NULL,
            lcn_sublote VARCHAR(10) NULL,
            lcn_data DATE NOT NULL,
            lcn_historico_padrao VARCHAR(200) NULL,
            lcn_complemento TEXT NULL,
            lcn_valor DECIMAL(15,2) NOT NULL,
            lcn_tipo ENUM('MANUAL','AUTOMATICO') NOT NULL DEFAULT 'MANUAL',
            lcn_origem_modulo VARCHAR(50) NULL,
            lcn_origem_id INT NULL,
            lcn_estornado TINYINT(1) NOT NULL DEFAULT 0,
            lcn_lcn_estorno_id INT NULL,
            lcn_usr_cadastro INT NOT NULL,
            lcn_data_cadastro TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            PRIMARY KEY (lcn_id),
            KEY ix_cnt_lancamentos_empresa_data (lcn_emp_id, lcn_data),
            KEY ix_cnt_lancamentos_lote (lcn_lote)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS cnt_partidas (
            prt_id INT NOT NULL AUTO_INCREMENT,
            prt_lcn_id INT NOT NULL,
            prt_tipo ENUM('DEBITO','CREDITO') NOT NULL,
            prt_pct_id INT NOT NULL,
            prt_ccu_id INT NULL,
            prt_valor DECIMAL(15,2) NOT NULL,
            prt_historico TEXT NULL,
            PRIMARY KEY (prt_id),
            KEY ix_cnt_partidas_lancamento (prt_lcn_id),
            KEY ix_cnt_partidas_conta (prt_pct_id, prt_tipo)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS cnt_fechamentos (
            fec_id INT NOT NULL AUTO_INCREMENT,
            fec_emp_id INT NOT NULL,
            fec_periodo DATE NOT NULL,
            fec_data_fechamento TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            fec_usr_responsavel INT NOT NULL,
            fec_bloqueado TINYINT(1) NOT NULL DEFAULT 1,
            fec_observacoes TEXT NULL,
            PRIMARY KEY (fec_id),
            UNIQUE KEY ux_cnt_fechamentos_empresa_periodo (fec_emp_id, fec_periodo)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS ctb_conciliacoes (
            cnc_id INT NOT NULL AUTO_INCREMENT,
            cnc_financeiro_id INT NOT NULL,
            cnc_status ENUM('PENDENTE','CONCILIADO','DIVERGENTE','IGNORADO') NOT NULL DEFAULT 'PENDENTE',
            cnc_referencia_bancaria VARCHAR(120) NULL,
            cnc_observacoes TEXT NULL,
            cnc_usuario_id INT NULL,
            cnc_data_conciliacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            tenant_id CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001',
            row_version BINARY(16) NOT NULL,
            created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            created_by_user_id CHAR(36) NULL,
            updated_at DATETIME NULL,
            updated_by_user_id CHAR(36) NULL,
            is_deleted TINYINT(1) NOT NULL DEFAULT 0,
            deleted_at DATETIME NULL,
            PRIMARY KEY (cnc_id),
            UNIQUE KEY ux_ctb_conciliacoes_financeiro (cnc_financeiro_id),
            KEY ix_ctb_conciliacoes_status (cnc_status),
            KEY ix_ctb_conciliacoes_tenant_deleted (tenant_id, is_deleted)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync("ALTER TABLE ctb_conciliacoes ADD COLUMN IF NOT EXISTS tenant_id CHAR(36) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE ctb_conciliacoes ADD COLUMN IF NOT EXISTS row_version BINARY(16) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE ctb_conciliacoes ADD COLUMN IF NOT EXISTS created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE ctb_conciliacoes ADD COLUMN IF NOT EXISTS created_by_user_id CHAR(36) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE ctb_conciliacoes ADD COLUMN IF NOT EXISTS updated_at DATETIME NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE ctb_conciliacoes ADD COLUMN IF NOT EXISTS updated_by_user_id CHAR(36) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE ctb_conciliacoes ADD COLUMN IF NOT EXISTS is_deleted TINYINT(1) NOT NULL DEFAULT 0;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE ctb_conciliacoes ADD COLUMN IF NOT EXISTS deleted_at DATETIME NULL;");
    await db.Database.ExecuteSqlRawAsync(
        """
        UPDATE ctb_conciliacoes c
        INNER JOIN financeiro f ON f.id = c.cnc_financeiro_id
        SET c.tenant_id = f.tenant_id
        WHERE c.tenant_id IS NULL OR c.tenant_id = '';
        """);
    await db.Database.ExecuteSqlRawAsync("UPDATE ctb_conciliacoes SET tenant_id = '00000000-0000-0000-0000-000000000001' WHERE tenant_id IS NULL OR tenant_id = '';");
    await db.Database.ExecuteSqlRawAsync("UPDATE ctb_conciliacoes SET row_version = UNHEX(REPLACE(UUID(), '-', '')) WHERE row_version IS NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE ctb_conciliacoes MODIFY COLUMN tenant_id CHAR(36) NOT NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE ctb_conciliacoes MODIFY COLUMN row_version BINARY(16) NOT NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE ctb_conciliacoes ADD INDEX IF NOT EXISTS ix_ctb_conciliacoes_tenant_deleted (tenant_id, is_deleted);");

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS fis_sped_contabil (
            spc_id INT NOT NULL AUTO_INCREMENT,
            spc_emp_id INT NOT NULL,
            spc_ano INT NOT NULL,
            spc_tipo ENUM('ECD','ECF') NOT NULL,
            spc_arquivo LONGTEXT NULL,
            spc_nome_arquivo VARCHAR(200) NULL,
            spc_data_geracao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            spc_status ENUM('GERADO','VALIDADO','TRANSMITIDO','ERRO') NOT NULL DEFAULT 'GERADO',
            spc_protocolo VARCHAR(50) NULL,
            spc_mensagem_erro TEXT NULL,
            spc_usr_responsavel INT NOT NULL,
            PRIMARY KEY (spc_id),
            KEY ix_fis_sped_contabil_empresa_ano (spc_emp_id, spc_ano, spc_tipo)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS fis_sped_fiscal (
            spf_id INT NOT NULL AUTO_INCREMENT,
            spf_emp_id INT NOT NULL,
            spf_periodo DATE NOT NULL,
            spf_tipo ENUM('ICMS_IPI','CONTRIBUICOES') NOT NULL,
            spf_arquivo LONGTEXT NULL,
            spf_nome_arquivo VARCHAR(200) NULL,
            spf_data_geracao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
            spf_status ENUM('GERADO','VALIDADO','TRANSMITIDO','ERRO') NOT NULL DEFAULT 'GERADO',
            spf_protocolo VARCHAR(50) NULL,
            spf_mensagem_erro TEXT NULL,
            spf_usr_responsavel INT NOT NULL,
            PRIMARY KEY (spf_id),
            KEY ix_fis_sped_fiscal_empresa_periodo (spf_emp_id, spf_periodo, spf_tipo)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await EnsureIntPrimaryKeyAsync(db, "cnt_lancamentos", "lcn_id");
    await EnsureIntPrimaryKeyAsync(db, "cnt_partidas", "prt_id");
    await EnsureIntPrimaryKeyAsync(db, "cnt_fechamentos", "fec_id");
    await EnsureIntPrimaryKeyAsync(db, "fis_sped_contabil", "spc_id");
    await EnsureIntPrimaryKeyAsync(db, "fis_sped_fiscal", "spf_id");
}

static async Task EnsureIntPrimaryKeyAsync(NexumDbContext db, string tableName, string columnName)
{
    var table = QuoteMySqlIdentifier(tableName);
    var column = QuoteMySqlIdentifier(columnName);

    var primaryKeys = await db.Database.SqlQueryRaw<int>(
        """
        SELECT COUNT(*) AS Value
        FROM information_schema.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_SCHEMA = DATABASE()
          AND TABLE_NAME = {0}
          AND CONSTRAINT_TYPE = 'PRIMARY KEY'
        """,
        tableName)
        .SingleAsync();

    if (primaryKeys == 0)
    {
        try
        {
            var addPrimaryKeySql = "ALTER TABLE " + table + " ADD PRIMARY KEY (" + column + ");";
            await db.Database.ExecuteSqlRawAsync(addPrimaryKeySql);
        }
        catch (MySqlException exception) when (exception.Number == 1068)
        {
            // Outra instancia concluiu a criacao da chave primaria entre a consulta e o ALTER TABLE.
        }
    }

    var modifyIdentitySql = "ALTER TABLE " + table + " MODIFY COLUMN " + column + " INT NOT NULL AUTO_INCREMENT;";
    await db.Database.ExecuteSqlRawAsync(modifyIdentitySql);
}

static async Task NormalizeLegacyZeroDateDefaultsAsync(DbContext db, ILogger logger)
{
    var columns = await db.Database.SqlQueryRaw<string>(
        "SELECT CONCAT(c.TABLE_NAME, '|', c.COLUMN_NAME, '|', c.COLUMN_TYPE, '|', c.IS_NULLABLE) AS Value FROM information_schema.COLUMNS c INNER JOIN information_schema.TABLES t ON t.TABLE_SCHEMA = c.TABLE_SCHEMA AND t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_TYPE = 'BASE TABLE' WHERE c.TABLE_SCHEMA = DATABASE() AND c.DATA_TYPE IN ('datetime', 'timestamp') AND CAST(c.COLUMN_DEFAULT AS CHAR) LIKE '%0000-00-00%' ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION")
        .ToListAsync();

    var alterationsByTable = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
    foreach (var value in columns)
    {
        var metadata = value.Split('|', 4);
        if (metadata.Length != 4 || !System.Text.RegularExpressions.Regex.IsMatch(metadata[2], "^(datetime|timestamp)(\\([0-6]\\))?$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            throw new InvalidOperationException($"Metadados de data zero inesperados no schema MySQL: {value}.");
        }

        var table = QuoteMySqlIdentifier(metadata[0]);
        var column = QuoteMySqlIdentifier(metadata[1]);
        var nullable = string.Equals(metadata[3], "YES", StringComparison.OrdinalIgnoreCase);
        var nullability = nullable ? " NULL DEFAULT NULL" : " NOT NULL DEFAULT CURRENT_TIMESTAMP";
        if (!alterationsByTable.TryGetValue(metadata[0], out var tableAlterations))
        {
            tableAlterations = new List<string>();
            alterationsByTable.Add(metadata[0], tableAlterations);
        }

        tableAlterations.Add("MODIFY COLUMN " + column + " " + metadata[2].ToUpperInvariant() + nullability);
    }

    foreach (var tableAlteration in alterationsByTable)
    {
        var alterSql = "ALTER TABLE " + QuoteMySqlIdentifier(tableAlteration.Key) + " " + string.Join(", ", tableAlteration.Value) + ";";
        await db.Database.ExecuteSqlRawAsync(alterSql);
    }

    if (columns.Count > 0)
    {
        logger.LogInformation("Defaults zero-date corrigidos em {ColumnCount} colunas de data do banco {Database}.", columns.Count, db.Database.GetDbConnection().Database);
    }
}

static string QuoteMySqlIdentifier(string? identifier)
{
    if (string.IsNullOrWhiteSpace(identifier) || identifier.Any(character => !char.IsLetterOrDigit(character) && character != '_'))
    {
        throw new InvalidOperationException($"Identificador MySQL invalido para schema operacional: {identifier}");
    }

    return "`" + identifier + "`";
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
            throw new InvalidOperationException("Banco GenesisGest.Net indisponivel durante a verificacao obrigatoria do schema financeiro.");
        }

        await NormalizeLegacyZeroDateDefaultsAsync(genesisDb, logger);

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
        await BackfillLegacyGenesisReceivablesAsync(genesisDb);

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
        await BackfillLegacyGenesisPayablesAsync(genesisDb);

        await genesisDb.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS erp_fluxo_caixa (
                id INT NOT NULL AUTO_INCREMENT,
                data DATETIME NOT NULL,
                tipo VARCHAR(50) NOT NULL,
                descricao VARCHAR(100) NOT NULL,
                valor DECIMAL(18,2) NOT NULL,
                categoria VARCHAR(50) NULL,
                conta_pagar_id INT NULL,
                conta_receber_id INT NULL,
                forma_pagamento VARCHAR(50) NULL,
                conta_bancaria VARCHAR(100) NULL,
                observacoes VARCHAR(500) NULL,
                criado_em DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY (id)
            );
            """);

        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_fluxo_caixa ADD COLUMN IF NOT EXISTS conta_pagar_id INT NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_fluxo_caixa ADD COLUMN IF NOT EXISTS conta_receber_id INT NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_fluxo_caixa ADD COLUMN IF NOT EXISTS forma_pagamento VARCHAR(50) NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_fluxo_caixa ADD COLUMN IF NOT EXISTS conta_bancaria VARCHAR(100) NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync("ALTER TABLE erp_fluxo_caixa ADD COLUMN IF NOT EXISTS criado_em DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;");
        await BackfillLegacyGenesisCashFlowAsync(genesisDb);

        await EnsureGenesisFinanceAuditSchemaAsync(genesisDb);

        await SyncGenesisReceivablesFromNexumAsync(services, genesisDb);
        await SyncGenesisPayablesFromNexumAsync(services, genesisDb);
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Falha obrigatoria ao ajustar o schema financeiro compartilhado GenesisGest.Net.");
        throw;
    }
}

static async Task EnsureGenesisFinanceAuditSchemaAsync(GenesisDbContext genesisDb)
{
    foreach (var tableName in new[] { "erp_contas_pagar", "erp_contas_receber", "erp_fluxo_caixa" })
    {
        var table = QuoteMySqlIdentifier(tableName);
        var createdAtSource = tableName switch
        {
            "erp_contas_pagar" => "data_emissao",
            "erp_contas_receber" => "data_emissao",
            "erp_fluxo_caixa" => "COALESCE(criado_em, data)",
            _ => throw new InvalidOperationException($"Tabela financeira sem fonte de auditoria configurada: {tableName}.")
        };
        await genesisDb.Database.ExecuteSqlRawAsync(
            "ALTER TABLE " + table + " " +
            "ADD COLUMN IF NOT EXISTS tenant_id CHAR(36) NULL, " +
            "ADD COLUMN IF NOT EXISTS row_version BINARY(16) NULL, " +
            "ADD COLUMN IF NOT EXISTS created_at DATETIME(6) NULL, " +
            "ADD COLUMN IF NOT EXISTS created_by_user_id CHAR(36) NULL, " +
            "ADD COLUMN IF NOT EXISTS updated_at DATETIME NULL, " +
            "ADD COLUMN IF NOT EXISTS updated_by_user_id CHAR(36) NULL, " +
            "ADD COLUMN IF NOT EXISTS is_deleted TINYINT(1) NOT NULL DEFAULT 0, " +
            "ADD COLUMN IF NOT EXISTS deleted_at DATETIME NULL;");

        await genesisDb.Database.ExecuteSqlRawAsync(
            "UPDATE " + table + " SET tenant_id = '" + TenantContext.DefaultTenantId +
            "' WHERE tenant_id IS NULL OR tenant_id = '' OR tenant_id = '00000000-0000-0000-0000-000000000000';");
        await genesisDb.Database.ExecuteSqlRawAsync(
            "UPDATE " + table + " SET created_at = COALESCE(" + createdAtSource + ", UTC_TIMESTAMP(6)) WHERE created_at IS NULL;");
        await genesisDb.Database.ExecuteSqlRawAsync(
            "UPDATE " + table + " SET row_version = UNHEX(REPLACE(UUID(), '-', '')) WHERE row_version IS NULL OR OCTET_LENGTH(row_version) <> 16;");
        await genesisDb.Database.ExecuteSqlRawAsync(
            "ALTER TABLE " + table +
            " MODIFY COLUMN tenant_id CHAR(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL," +
            " MODIFY COLUMN row_version BINARY(16) NOT NULL," +
            " MODIFY COLUMN created_at DATETIME(6) NOT NULL;");

        var indexName = $"ix_{tableName}_tenant_deleted";
        var indexExists = await ExecuteScalarAsync<long>(
            genesisDb,
            """
            SELECT COUNT(*)
            FROM information_schema.statistics
            WHERE table_schema = DATABASE()
              AND table_name = @tableName
              AND index_name = @indexName
            """,
            CancellationToken.None,
            ("@tableName", tableName),
            ("@indexName", indexName));
        if (indexExists == 0)
        {
            await genesisDb.Database.ExecuteSqlRawAsync(
                "CREATE INDEX " + QuoteMySqlIdentifier(indexName) + " ON " + table + " (tenant_id, is_deleted);");
        }
    }
}

static async Task BackfillLegacyGenesisReceivablesAsync(GenesisDbContext genesisDb)
{
    if (!await GenesisColumnExistsAsync(genesisDb, "erp_contas_receber", "NumeroDocumento"))
    {
        return;
    }

    await genesisDb.Database.ExecuteSqlRawAsync(
        """
        UPDATE erp_contas_receber
        SET numero_documento = NumeroDocumento,
            cliente_id = ClienteId,
            valor_original = ValorOriginal,
            valor_recebido = ValorRecebido,
            valor_multa = ValorMulta,
            valor_juros = ValorJuros,
            valor_desconto = ValorDesconto,
            data_emissao = DataEmissao,
            data_vencimento = DataVencimento,
            data_recebimento = DataRecebimento,
            forma_recebimento = FormaRecebimento,
            numero_pedido_referencia = NumeroPedidoReferencia
        WHERE (numero_documento IS NULL OR TRIM(numero_documento) = '')
          AND NumeroDocumento IS NOT NULL
          AND TRIM(NumeroDocumento) <> '';
        """);
}

static async Task BackfillLegacyGenesisPayablesAsync(GenesisDbContext genesisDb)
{
    if (!await GenesisColumnExistsAsync(genesisDb, "erp_contas_pagar", "NumeroDocumento"))
    {
        return;
    }

    await genesisDb.Database.ExecuteSqlRawAsync(
        """
        UPDATE erp_contas_pagar
        SET numero_documento = NumeroDocumento,
            fornecedor_id = FornecedorId,
            valor_original = ValorOriginal,
            valor_pago = ValorPago,
            valor_multa = ValorMulta,
            valor_juros = ValorJuros,
            valor_desconto = ValorDesconto,
            data_emissao = DataEmissao,
            data_vencimento = DataVencimento,
            data_pagamento = DataPagamento,
            forma_pagamento = FormaPagamento,
            numero_boleto = NumeroBoleto
        WHERE (numero_documento IS NULL OR TRIM(numero_documento) = '')
          AND NumeroDocumento IS NOT NULL
          AND TRIM(NumeroDocumento) <> '';
        """);
}

static async Task BackfillLegacyGenesisCashFlowAsync(GenesisDbContext genesisDb)
{
    if (!await GenesisColumnExistsAsync(genesisDb, "erp_fluxo_caixa", "ContaPagarId"))
    {
        return;
    }

    await genesisDb.Database.ExecuteSqlRawAsync(
        """
        UPDATE erp_fluxo_caixa
        SET conta_pagar_id = COALESCE(conta_pagar_id, ContaPagarId),
            conta_receber_id = COALESCE(conta_receber_id, ContaReceberId),
            forma_pagamento = COALESCE(NULLIF(forma_pagamento, ''), FormaPagamento),
            conta_bancaria = COALESCE(NULLIF(conta_bancaria, ''), ContaBancaria),
            criado_em = CASE
                WHEN CriadoEm IS NOT NULL AND YEAR(CriadoEm) > 0 THEN CriadoEm
                ELSE criado_em
            END
        WHERE conta_pagar_id IS NULL
           OR conta_receber_id IS NULL
           OR forma_pagamento IS NULL
           OR conta_bancaria IS NULL;
        """);
}

static async Task<bool> GenesisColumnExistsAsync(GenesisDbContext genesisDb, string tableName, string columnName)
{
    var count = await genesisDb.Database.SqlQueryRaw<int>(
        """
        SELECT COUNT(*) AS Value
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = {0}
          AND BINARY COLUMN_NAME = {1}
        """,
        tableName,
        columnName)
        .SingleAsync();

    return count > 0;
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
        LEFT JOIN fornecedores f
          ON f.id = p.fornecedor_id
         AND f.tenant_id = {0}
         AND f.is_deleted = 0
        WHERE p.tenant_id = {0}
          AND p.is_deleted = 0
          AND p.valor_total > 0
        ORDER BY p.created_at DESC
        LIMIT 200
        """,
        nexumDb.CurrentTenantId.ToString())
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

    var transactionalTables = new[]
    {
        "compras_solicitacoes",
        "compras_cotacoes",
        "compras_pedidos",
        "compras_pedido_itens",
        "compras_entradas",
        "compras_entrada_itens",
        "estoque_movimentos"
    };

    foreach (var table in transactionalTables)
    {
        var quotedTable = QuoteMySqlIdentifier(table);
        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE " + quotedTable +
            " ADD COLUMN IF NOT EXISTS `tenant_id` CHAR(36) NOT NULL DEFAULT '" + TenantContext.DefaultTenantId + "'," +
            " ADD COLUMN IF NOT EXISTS `row_version` BLOB NULL," +
            " ADD COLUMN IF NOT EXISTS `created_by_user_id` CHAR(36) NULL," +
            " ADD COLUMN IF NOT EXISTS `updated_by_user_id` CHAR(36) NULL," +
            " ADD COLUMN IF NOT EXISTS `is_deleted` TINYINT(1) NOT NULL DEFAULT 0," +
            " ADD COLUMN IF NOT EXISTS `deleted_at` DATETIME NULL," +
            " ADD COLUMN IF NOT EXISTS `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;");

        await db.Database.ExecuteSqlRawAsync(
            "UPDATE " + quotedTable +
            " SET `tenant_id` = '" + TenantContext.DefaultTenantId + "'" +
            " WHERE `tenant_id` IS NULL OR `tenant_id` = '';");
        await db.Database.ExecuteSqlRawAsync(
            "UPDATE " + quotedTable +
            " SET `row_version` = UNHEX(REPLACE(UUID(), '-', ''))" +
            " WHERE `row_version` IS NULL OR OCTET_LENGTH(`row_version`) = 0;");

        await EnsureProcurementIndexAsync(
            db,
            table,
            $"ix_{table}_tenant_deleted",
            "CREATE INDEX " + QuoteMySqlIdentifier($"ix_{table}_tenant_deleted") +
            " ON " + quotedTable + " (`tenant_id`, `is_deleted`);");
    }

    var globalOrderNumberIndexExists = await ExecuteScalarAsync<long>(
        db,
        """
        SELECT COUNT(*)
        FROM information_schema.statistics
        WHERE table_schema = DATABASE()
          AND table_name = 'compras_pedidos'
          AND index_name = 'ux_compras_pedidos_numero'
        """,
        CancellationToken.None);
    if (globalOrderNumberIndexExists > 0)
    {
        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE `compras_pedidos` DROP INDEX `ux_compras_pedidos_numero`;");
    }

    await EnsureProcurementIndexAsync(
        db,
        "compras_pedidos",
        "ux_compras_pedidos_tenant_numero",
        "CREATE UNIQUE INDEX `ux_compras_pedidos_tenant_numero` ON `compras_pedidos` (`tenant_id`, `numero`);");
}

static async Task EnsureProcurementIndexAsync(
    NexumDbContext db,
    string table,
    string index,
    string command)
{
    var exists = await ExecuteScalarAsync<long>(
        db,
        """
        SELECT COUNT(*)
        FROM information_schema.statistics
        WHERE table_schema = DATABASE()
          AND table_name = @tableName
          AND index_name = @indexName
        """,
        CancellationToken.None,
        ("@tableName", table),
        ("@indexName", index));
    if (exists == 0)
    {
        await db.Database.ExecuteSqlRawAsync(command);
    }
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
            token_refresh_expira_em DATETIME NULL,
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
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS token_refresh_expira_em DATETIME NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;");
}

static async Task EnsurePlatformSsoSchemaAsync(NexumDbContext db)
{
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS mfa_habilitado TINYINT(1) NOT NULL DEFAULT 0;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS mfa_secret VARCHAR(1024) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE usuarios MODIFY COLUMN mfa_secret VARCHAR(1024) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS mfa_confirmado_em DATETIME NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS mfa_ultimo_passo BIGINT NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE clientes ADD COLUMN IF NOT EXISTS token_refresh VARCHAR(255) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE clientes ADD COLUMN IF NOT EXISTS token_refresh_expira_em DATETIME NULL;");
    await db.Database.ExecuteSqlRawAsync(
        "UPDATE usuarios SET token_refresh = NULL, token_refresh_expira_em = NULL WHERE token_refresh IS NOT NULL AND (CHAR_LENGTH(token_refresh) <> 64 OR token_refresh_expira_em IS NULL);");
    await db.Database.ExecuteSqlRawAsync(
        "UPDATE clientes SET token_refresh = NULL, token_refresh_expira_em = NULL WHERE token_refresh IS NOT NULL AND (CHAR_LENGTH(token_refresh) <> 64 OR token_refresh_expira_em IS NULL);");

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS sys_tenants (
            id CHAR(36) NOT NULL,
            tenant_id CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001',
            codigo VARCHAR(80) NOT NULL,
            nome VARCHAR(180) NOT NULL,
            documento VARCHAR(20) NULL,
            ativo TINYINT(1) NOT NULL DEFAULT 1,
            row_version BLOB NULL,
            created_by_user_id CHAR(36) NULL,
            updated_by_user_id CHAR(36) NULL,
            is_deleted TINYINT(1) NOT NULL DEFAULT 0,
            deleted_at DATETIME NULL,
            created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            UNIQUE KEY ux_sys_tenants_codigo (codigo),
            KEY ix_sys_tenants_tenant_deleted (tenant_id, is_deleted),
            KEY ix_sys_tenants_ativo (ativo, is_deleted)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync("ALTER TABLE sys_tenants ADD COLUMN IF NOT EXISTS tenant_id CHAR(36) NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001';");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE sys_tenants ADD COLUMN IF NOT EXISTS row_version BLOB NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE sys_tenants ADD COLUMN IF NOT EXISTS created_by_user_id CHAR(36) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE sys_tenants ADD COLUMN IF NOT EXISTS updated_by_user_id CHAR(36) NULL;");
    await db.Database.ExecuteSqlRawAsync("ALTER TABLE sys_tenants ADD COLUMN IF NOT EXISTS deleted_at DATETIME NULL;");
    var tenantDeletedIndexExists = await db.Database.SqlQueryRaw<int>(
        """
        SELECT COUNT(*) AS Value
        FROM information_schema.statistics
        WHERE table_schema = DATABASE()
          AND table_name = 'sys_tenants'
          AND index_name = 'ix_sys_tenants_tenant_deleted'
        """)
        .SingleAsync();
    if (tenantDeletedIndexExists == 0)
    {
        await db.Database.ExecuteSqlRawAsync("CREATE INDEX ix_sys_tenants_tenant_deleted ON sys_tenants (tenant_id, is_deleted);");
    }

    await db.Database.ExecuteSqlRawAsync(
        """
        INSERT INTO sys_tenants (id, tenant_id, codigo, nome, documento, ativo)
        VALUES ('00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'GRUPO-NEXUM-ALTIVON', 'Grupo Nexum Altivon', NULL, 1)
        ON DUPLICATE KEY UPDATE
            tenant_id = VALUES(tenant_id),
            nome = VALUES(nome),
            ativo = 1,
            is_deleted = 0,
            updated_at = CURRENT_TIMESTAMP;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS sys_workflow_definicoes (
            id CHAR(36) NOT NULL,
            tenant_id CHAR(36) NOT NULL,
            entidade VARCHAR(120) NOT NULL,
            codigo VARCHAR(100) NOT NULL,
            nome VARCHAR(180) NOT NULL,
            estados_json JSON NOT NULL,
            transicoes_json JSON NOT NULL,
            ativo TINYINT(1) NOT NULL DEFAULT 1,
            is_deleted TINYINT(1) NOT NULL DEFAULT 0,
            deleted_at DATETIME NULL,
            created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            UNIQUE KEY ux_sys_workflow_def_tenant_codigo (tenant_id, codigo),
            KEY ix_sys_workflow_def_tenant_entidade (tenant_id, entidade, ativo, is_deleted)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS sys_workflow_instancias (
            id CHAR(36) NOT NULL,
            tenant_id CHAR(36) NOT NULL,
            definicao_id CHAR(36) NOT NULL,
            entidade VARCHAR(120) NOT NULL,
            registro_chave VARCHAR(120) NOT NULL,
            estado_atual VARCHAR(80) NOT NULL,
            solicitante_user_id INT NULL,
            observacao TEXT NULL,
            is_deleted TINYINT(1) NOT NULL DEFAULT 0,
            deleted_at DATETIME NULL,
            created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            KEY ix_sys_workflow_inst_tenant_entidade (tenant_id, entidade, registro_chave, is_deleted),
            KEY ix_sys_workflow_inst_definicao (definicao_id)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS sys_workflow_transicoes (
            id CHAR(36) NOT NULL,
            tenant_id CHAR(36) NOT NULL,
            instancia_id CHAR(36) NOT NULL,
            estado_origem VARCHAR(80) NOT NULL,
            estado_destino VARCHAR(80) NOT NULL,
            acao VARCHAR(80) NOT NULL,
            usuario_id INT NULL,
            observacao TEXT NULL,
            created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            PRIMARY KEY (id),
            KEY ix_sys_workflow_trans_instancia (instancia_id, created_at),
            KEY ix_sys_workflow_trans_tenant (tenant_id, created_at)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);
}

static async Task EnsureOpsSchemaAsync(NexumDbContext db)
{
    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS ops_ativos (
            oat_id INT NOT NULL AUTO_INCREMENT,
            tenant_id CHAR(36) NOT NULL,
            oat_codigo VARCHAR(80) NOT NULL,
            oat_nome VARCHAR(180) NOT NULL,
            oat_tipo VARCHAR(60) NOT NULL DEFAULT 'EQUIPAMENTO',
            oat_localizacao VARCHAR(180) NULL,
            oat_status VARCHAR(40) NOT NULL DEFAULT 'ATIVO',
            oat_fabricante VARCHAR(120) NULL,
            oat_modelo VARCHAR(120) NULL,
            oat_numero_serie VARCHAR(120) NULL,
            oat_proxima_manutencao DATETIME NULL,
            is_deleted TINYINT(1) NOT NULL DEFAULT 0,
            deleted_at DATETIME NULL,
            oat_created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            oat_updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (oat_id),
            UNIQUE KEY ux_ops_ativos_tenant_codigo (tenant_id, oat_codigo),
            KEY ix_ops_ativos_tenant_status (tenant_id, oat_status, is_deleted)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS ops_ordens_servico (
            oso_id INT NOT NULL AUTO_INCREMENT,
            tenant_id CHAR(36) NOT NULL,
            oso_numero VARCHAR(80) NOT NULL,
            oso_ativo_id INT NULL,
            oso_titulo VARCHAR(180) NOT NULL,
            oso_descricao TEXT NULL,
            oso_status VARCHAR(40) NOT NULL DEFAULT 'ABERTA',
            oso_prioridade VARCHAR(40) NOT NULL DEFAULT 'NORMAL',
            oso_responsavel_user_id INT NULL,
            oso_data_abertura DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            oso_data_prevista DATETIME NULL,
            oso_data_conclusao DATETIME NULL,
            oso_tempo_estimado_minutos INT NULL,
            oso_tempo_real_minutos INT NULL,
            oso_custo_previsto DECIMAL(15,2) NULL,
            oso_custo_real DECIMAL(15,2) NULL,
            oso_observacoes TEXT NULL,
            is_deleted TINYINT(1) NOT NULL DEFAULT 0,
            deleted_at DATETIME NULL,
            oso_created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            oso_updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (oso_id),
            UNIQUE KEY ux_ops_os_tenant_numero (tenant_id, oso_numero),
            KEY ix_ops_os_tenant_status (tenant_id, oso_status, is_deleted),
            KEY ix_ops_os_ativo (oso_ativo_id)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS ops_ordem_servico_itens (
            osi_id INT NOT NULL AUTO_INCREMENT,
            tenant_id CHAR(36) NOT NULL,
            oso_id INT NOT NULL,
            osi_tipo VARCHAR(40) NOT NULL,
            osi_codigo VARCHAR(80) NULL,
            osi_descricao VARCHAR(220) NOT NULL,
            osi_quantidade DECIMAL(15,4) NOT NULL DEFAULT 1,
            osi_unidade VARCHAR(20) NOT NULL DEFAULT 'UN',
            osi_custo_unitario DECIMAL(15,2) NULL,
            osi_total DECIMAL(15,2) NULL,
            PRIMARY KEY (osi_id),
            KEY ix_ops_os_itens_os (oso_id),
            KEY ix_ops_os_itens_tenant (tenant_id, oso_id)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS ops_producao_apontamentos (
            opa_id INT NOT NULL AUTO_INCREMENT,
            tenant_id CHAR(36) NOT NULL,
            oso_id INT NULL,
            produto_id INT NULL,
            produto_codigo VARCHAR(80) NULL,
            produto_nome VARCHAR(180) NOT NULL,
            quantidade_produzida DECIMAL(15,4) NOT NULL,
            quantidade_refugo DECIMAL(15,4) NOT NULL DEFAULT 0,
            tempo_minutos INT NULL,
            operador_user_id INT NULL,
            data_apontamento DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            insumos_json JSON NULL,
            observacoes TEXT NULL,
            is_deleted TINYINT(1) NOT NULL DEFAULT 0,
            deleted_at DATETIME NULL,
            created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (opa_id),
            KEY ix_ops_apontamentos_tenant_data (tenant_id, data_apontamento, is_deleted),
            KEY ix_ops_apontamentos_os (oso_id)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);

    await db.Database.ExecuteSqlRawAsync(
        """
        CREATE TABLE IF NOT EXISTS ops_manutencoes (
            omt_id INT NOT NULL AUTO_INCREMENT,
            tenant_id CHAR(36) NOT NULL,
            oat_id INT NULL,
            omt_tipo VARCHAR(40) NOT NULL DEFAULT 'PREVENTIVA',
            omt_titulo VARCHAR(180) NOT NULL,
            omt_status VARCHAR(40) NOT NULL DEFAULT 'PROGRAMADA',
            omt_data_programada DATETIME NULL,
            omt_data_inicio DATETIME NULL,
            omt_data_fim DATETIME NULL,
            omt_responsavel_user_id INT NULL,
            omt_recorrencia VARCHAR(80) NULL,
            omt_custo DECIMAL(15,2) NULL,
            omt_observacoes TEXT NULL,
            is_deleted TINYINT(1) NOT NULL DEFAULT 0,
            deleted_at DATETIME NULL,
            omt_created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            omt_updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            PRIMARY KEY (omt_id),
            KEY ix_ops_manutencoes_tenant_status (tenant_id, omt_status, is_deleted),
            KEY ix_ops_manutencoes_ativo (oat_id)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """);
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
        ("validacao_token_02", "512cddda6828e66ba75f47b0f64f3433a2267b61faaa5685270154adba3ecc8a", "TOKEN02 - Validacao operacional GenesisGest.Net"),
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
    var tenantId = db.CurrentTenantId.ToString();

    var solicitacoesAbertas = await ExecuteScalarAsync<int>(
        db,
        "SELECT COUNT(*) FROM compras_solicitacoes WHERE tenant_id = @tenantId AND is_deleted = 0 AND status IN ('Aberta', 'Cotado', 'EmAprovacao')",
        ct,
        ("@tenantId", tenantId));

    var pedidosAbertos = await ExecuteScalarAsync<int>(
        db,
        "SELECT COUNT(*) FROM compras_pedidos WHERE tenant_id = @tenantId AND is_deleted = 0 AND status IN ('Aberto', 'EmAprovacao', 'Aprovado', 'RecebidoParcial')",
        ct,
        ("@tenantId", tenantId));

    var entradasMes = await ExecuteScalarAsync<int>(
        db,
        "SELECT COUNT(*) FROM compras_entradas WHERE tenant_id = @tenantId AND is_deleted = 0 AND created_at >= @inicioMes",
        ct,
        ("@tenantId", tenantId),
        ("@inicioMes", inicioMes));

    var valorComprasAbertas = await ExecuteScalarAsync<decimal>(
        db,
        "SELECT COALESCE(SUM(valor_total), 0) FROM compras_pedidos WHERE tenant_id = @tenantId AND is_deleted = 0 AND status IN ('Aberto', 'EmAprovacao', 'Aprovado', 'RecebidoParcial')",
        ct,
        ("@tenantId", tenantId));

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
        WHERE s.tenant_id = {0} AND s.is_deleted = 0
        ORDER BY s.created_at DESC
        LIMIT 50
        """,
        tenantId)
        .ToListAsync(ct);

    var cotacoes = await db.Database.SqlQueryRaw<CompraCotacaoRow>(
        """
        SELECT
            c.id AS Id,
            c.solicitacao_id AS SolicitacaoId,
            c.fornecedor_id AS FornecedorId,
            COALESCE(NULLIF(f.nome_fantasia, ''), f.razao_social) AS FornecedorNome,
            c.produto_id AS ProdutoId,
            c.produto_nome AS ProdutoNome,
            c.quantidade AS Quantidade,
            c.custo_unitario AS CustoUnitario,
            c.valor_total AS ValorTotal,
            c.prazo_entrega_dias AS PrazoEntregaDias,
            c.origem AS Origem,
            c.status AS Status,
            c.created_at AS CreatedAt
        FROM compras_cotacoes c
        LEFT JOIN fornecedores f
          ON f.id = c.fornecedor_id
         AND f.tenant_id = {0}
         AND f.is_deleted = 0
        WHERE c.tenant_id = {0} AND c.is_deleted = 0
        ORDER BY c.created_at DESC
        LIMIT 100
        """,
        tenantId)
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
        LEFT JOIN fornecedores f
          ON f.id = p.fornecedor_id
         AND f.tenant_id = {0}
         AND f.is_deleted = 0
        WHERE p.tenant_id = {0} AND p.is_deleted = 0
        ORDER BY p.created_at DESC
        LIMIT 50
        """,
        tenantId)
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
        INNER JOIN compras_pedidos p
          ON p.id = i.compra_pedido_id
         AND p.tenant_id = {0}
         AND p.is_deleted = 0
        WHERE i.tenant_id = {0} AND i.is_deleted = 0
        ORDER BY p.created_at DESC, i.id ASC
        LIMIT 250
        """,
        tenantId)
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
        LEFT JOIN compras_pedidos p
          ON p.id = e.compra_pedido_id
         AND p.tenant_id = {0}
         AND p.is_deleted = 0
        LEFT JOIN fornecedores f
          ON f.id = e.fornecedor_id
         AND f.tenant_id = {0}
         AND f.is_deleted = 0
        WHERE e.tenant_id = {0} AND e.is_deleted = 0
        ORDER BY e.created_at DESC
        LIMIT 50
        """,
        tenantId)
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
        cotacoes.Select(item => new CompraCotacaoDto(
            item.Id,
            item.SolicitacaoId,
            item.FornecedorId,
            item.FornecedorNome ?? "Fornecedor nao identificado",
            item.ProdutoId,
            item.ProdutoNome,
            item.Quantidade,
            item.CustoUnitario,
            item.ValorTotal,
            item.PrazoEntregaDias,
            item.Origem,
            item.Status,
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

public sealed record OpsAtivoDto(
    int Id,
    string Codigo,
    string Nome,
    string Tipo,
    string? Localizacao,
    string Status,
    string? Fabricante,
    string? Modelo,
    string? NumeroSerie,
    DateTime? ProximaManutencao,
    DateTime CriadoEm,
    DateTime AtualizadoEm);

public sealed record OpsAtivoRequest(
    string Codigo,
    string Nome,
    string? Tipo,
    string? Localizacao,
    string? Status,
    string? Fabricante,
    string? Modelo,
    string? NumeroSerie,
    DateTime? ProximaManutencao);

public sealed record OpsOrdemServicoDto(
    int Id,
    string Numero,
    int? AtivoId,
    string Titulo,
    string? Descricao,
    string Status,
    string Prioridade,
    int? ResponsavelUserId,
    DateTime DataAbertura,
    DateTime? DataPrevista,
    DateTime? DataConclusao,
    int? TempoEstimadoMinutos,
    int? TempoRealMinutos,
    decimal? CustoPrevisto,
    decimal? CustoReal,
    string? Observacoes,
    DateTime CriadoEm,
    DateTime AtualizadoEm);

public sealed record OpsOrdemServicoRequest(
    string? Numero,
    int? AtivoId,
    string Titulo,
    string? Descricao,
    string? Status,
    string? Prioridade,
    int? ResponsavelUserId,
    DateTime? DataAbertura,
    DateTime? DataPrevista,
    DateTime? DataConclusao,
    int? TempoEstimadoMinutos,
    int? TempoRealMinutos,
    decimal? CustoPrevisto,
    decimal? CustoReal,
    string? Observacoes,
    List<OpsOrdemServicoItemRequest>? Itens);

public sealed record OpsOrdemServicoItemRequest(
    string? Tipo,
    string? Codigo,
    string Descricao,
    decimal Quantidade,
    string? Unidade,
    decimal? CustoUnitario);

public sealed record OpsOrdemServicoItemDto(
    int Id,
    string Tipo,
    string? Codigo,
    string Descricao,
    decimal Quantidade,
    string Unidade,
    decimal? CustoUnitario,
    decimal? Total);

public sealed record OpsOrdemServicoDetalheDto(
    OpsOrdemServicoDto Ordem,
    List<OpsOrdemServicoItemDto> Itens);

public sealed record OpsProducaoInsumoRequest(
    string? Codigo,
    string Descricao,
    decimal Quantidade,
    string? Unidade);

public sealed record OpsProducaoApontamentoDto(
    int Id,
    int? OrdemServicoId,
    int? ProdutoId,
    string? ProdutoCodigo,
    string ProdutoNome,
    decimal QuantidadeProduzida,
    decimal QuantidadeRefugo,
    int? TempoMinutos,
    int? OperadorUserId,
    DateTime DataApontamento,
    string? InsumosJson,
    string? Observacoes);

public sealed record OpsProducaoApontamentoRequest(
    int? OrdemServicoId,
    int? ProdutoId,
    string? ProdutoCodigo,
    string ProdutoNome,
    decimal QuantidadeProduzida,
    decimal QuantidadeRefugo,
    int? TempoMinutos,
    int? OperadorUserId,
    DateTime? DataApontamento,
    List<OpsProducaoInsumoRequest>? Insumos,
    string? Observacoes);

public sealed record OpsManutencaoDto(
    int Id,
    int? AtivoId,
    string Tipo,
    string Titulo,
    string Status,
    DateTime? DataProgramada,
    DateTime? DataInicio,
    DateTime? DataFim,
    int? ResponsavelUserId,
    string? Recorrencia,
    decimal? Custo,
    string? Observacoes,
    DateTime CriadoEm,
    DateTime AtualizadoEm);

public sealed record OpsManutencaoRequest(
    int? AtivoId,
    string? Tipo,
    string Titulo,
    string? Status,
    DateTime? DataProgramada,
    DateTime? DataInicio,
    DateTime? DataFim,
    int? ResponsavelUserId,
    string? Recorrencia,
    decimal? Custo,
    string? Observacoes);
public sealed record LoginRequest(string Email, string Senha, string? MfaCode = null);

public sealed class RefreshTokenRequest
{
    public string? Token { get; init; }
    public string? AccessToken { get; init; }

    [JsonPropertyName("access_token")]
    public string? AccessTokenSnake { get; init; }

    public string? RefreshToken { get; init; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshTokenSnake { get; init; }

    public string? ResolveToken() => Token ?? AccessToken ?? AccessTokenSnake;

    public string? ResolveRefreshToken() => RefreshToken ?? RefreshTokenSnake;
}

public sealed record MfaVerifyRequest(string Codigo);

public sealed record MfaEnableResponse(string Secret, string OtpAuthUri, bool Habilitado);

public sealed record MfaStatusResponse(bool Habilitado, DateTime? ConfirmadoEm);

public sealed record TenantDto(
    string Id,
    string Codigo,
    string Nome,
    string? Documento,
    bool Ativo,
    DateTime CriadoEm,
    DateTime AtualizadoEm);

public sealed record TenantUpsertRequest(
    string Codigo,
    string Nome,
    string? Documento,
    bool Ativo);

public sealed record WorkflowTransicaoRegraDto(
    string Origem,
    string Destino,
    string Acao,
    List<string>? PerfisAutorizados);

public sealed record WorkflowDefinicaoDto(
    string Id,
    string Entidade,
    string Codigo,
    string Nome,
    string EstadosJson,
    string TransicoesJson,
    bool Ativo,
    DateTime CriadoEm,
    DateTime AtualizadoEm);

public sealed record WorkflowDefinicaoRequest(
    string Entidade,
    string Codigo,
    string Nome,
    List<string> Estados,
    List<WorkflowTransicaoRegraDto> Transicoes,
    bool Ativo);

public sealed record WorkflowDefinicaoNormalizada(
    string Entidade,
    string Codigo,
    string Nome,
    List<string> Estados,
    List<WorkflowTransicaoRegraDto> Transicoes,
    bool Ativo);

public sealed record WorkflowInstanciaDto(
    string Id,
    string DefinicaoId,
    string Entidade,
    string RegistroChave,
    string EstadoAtual,
    int? SolicitanteUserId,
    DateTime CriadoEm,
    DateTime AtualizadoEm);

public sealed record WorkflowInstanciaRequest(
    Guid DefinicaoId,
    string Entidade,
    string RegistroChave,
    string? EstadoInicial,
    string? Observacao);

public sealed record WorkflowTransicaoDto(
    string Id,
    string InstanciaId,
    string EstadoOrigem,
    string EstadoDestino,
    string Acao,
    int? UsuarioId,
    DateTime CriadoEm);

public sealed record WorkflowTransicaoRequest(
    string EstadoDestino,
    string? Acao,
    string? Observacao);

public sealed record LoginResponse(string Token, string RefreshToken, DateTime ExpiraEm, UsuarioDto Usuario);

public sealed record UsuarioDto(int Id, string Nome, string Email, string Perfil);

public sealed record UsuarioAcessoDto(
    int Id,
    string Nome,
    string Email,
    string Perfil,
    bool Ativo,
    string? Telefone,
    DateTime? UltimoLogin,
    DateTime UpdatedAt);

public sealed record UsuarioAcessoUpsertRequest(
    string Nome,
    string Email,
    string Perfil,
    bool Ativo,
    string? Telefone,
    string? Senha);

public sealed record PerfilAcessoDto(
    int Id,
    string Nome,
    string? Descricao,
    decimal AlcadaMaxima,
    int NivelHierarquico,
    bool Ativo,
    DateTime CriadoEm);

public sealed record PerfilAcessoUpsertRequest(
    string Nome,
    string? Descricao,
    decimal AlcadaMaxima,
    int NivelHierarquico,
    bool Ativo);

public sealed record PermissaoAcessoDto(
    int Id,
    string Modulo,
    string Funcionalidade,
    string Chave,
    string? Descricao,
    bool Ativo);

public sealed record PermissaoAcessoUpsertRequest(
    string Modulo,
    string Funcionalidade,
    string Chave,
    string? Descricao,
    bool Ativo);

public sealed record PerfilPermissaoDto(
    int Id,
    int PerfilId,
    int PermissaoId,
    string Modulo,
    string Funcionalidade,
    string Chave,
    bool Leitura,
    bool Escrita,
    bool Exclusao,
    bool Impressao);

public sealed record PerfilPermissaoUpsertRequest(
    int PermissaoId,
    bool Leitura,
    bool Escrita,
    bool Exclusao,
    bool Impressao);

public sealed record GrcIdentityTable(
    string Name,
    string IdentityColumn,
    string RowCountSql,
    string AddPrimaryKeySql,
    string EnsureAutoIncrementSql);

public sealed record AuditoriaOperacionalDto(
    long Id,
    string Tabela,
    int RegistroId,
    string Acao,
    int? UsuarioId,
    string UsuarioTipo,
    string? IpAddress,
    string? Endpoint,
    DateTime CriadoEm);

public sealed record AuditoriaDetalheDto(
    long Id,
    string Tabela,
    int RegistroId,
    string Acao,
    int? UsuarioId,
    string UsuarioTipo,
    string? IpAddress,
    string? UserAgent,
    string? Endpoint,
    string? DadosAnteriores,
    string? DadosNovos,
    DateTime CriadoEm);

public sealed record SoDRegraDto(
    string Codigo,
    string Modulo,
    string PermissaoPrimaria,
    string PermissaoConflitante,
    string Descricao);

public sealed record SoDValidacaoRequest(
    string? Perfil,
    List<string> Permissoes);

public sealed record SoDValidacaoResponse(
    bool Aprovado,
    List<string> Conflitos);

public sealed record PessoaMasterDataDto(
    int Id,
    string Tipo,
    string NomeRazao,
    string? NomeFantasia,
    string? CpfCnpj,
    string? RgIe,
    bool Cliente,
    bool Fornecedor,
    bool Colaborador,
    bool Transportadora,
    string? Email,
    string? Telefone,
    string? Celular,
    string? Cidade,
    string? Uf,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record PessoaMasterDataRequest(
    string Tipo,
    string NomeRazao,
    string? NomeFantasia,
    string? CpfCnpj,
    string? RgIe,
    bool Cliente,
    bool Fornecedor,
    bool Colaborador,
    bool Transportadora,
    string? Endereco,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Uf,
    string? Cep,
    string? Telefone,
    string? Celular,
    string? Email,
    string? Site,
    string? Observacoes,
    bool Ativo);

public sealed record CentroCustoDto(
    int Id,
    string Codigo,
    string Nome,
    string? Descricao,
    string Tipo,
    int? PaiId,
    int? ResponsavelUsuarioId,
    string Status,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CentroCustoRequest(
    string Codigo,
    string Nome,
    string? Descricao,
    string? Observacoes,
    string? Tipo,
    int? PaiId,
    int? ResponsavelUsuarioId,
    string? Status);

public sealed record ItemServicoDto(
    int Id,
    int EmpresaId,
    string Codigo,
    string Tipo,
    string Descricao,
    string? DescricaoDetalhada,
    string Unidade,
    string? Ncm,
    string? Cest,
    bool ControlaEstoque,
    bool ControlaLote,
    bool ControlaSerie,
    bool Ativo,
    DateTime CriadoEm);

public sealed record ItemServicoRequest(
    int EmpresaId,
    string Codigo,
    string Tipo,
    string Descricao,
    string? DescricaoDetalhada,
    string? Unidade,
    string? Ncm,
    string? Cest,
    decimal? PesoBruto,
    decimal? PesoLiquido,
    decimal? Altura,
    decimal? Largura,
    decimal? Profundidade,
    bool ControlaEstoque,
    bool ControlaLote,
    bool ControlaSerie,
    bool Ativo);

public sealed record ProdutoPrecoLojaDto(
    int Id,
    int ProdutoId,
    int LojaId,
    decimal PrecoVenda,
    decimal? PrecoPromocional,
    decimal? PrecoCusto,
    decimal? MargemPercentual,
    bool Ativo,
    DateTime AtualizadoEm);

public sealed record ProdutoPrecoLojaRequest(
    int LojaId,
    decimal PrecoVenda,
    decimal? PrecoPromocional,
    decimal? PrecoCusto,
    decimal? MargemPercentual,
    bool Ativo);

public sealed record FornecedorContatoDto(
    int Id,
    int FornecedorId,
    string Nome,
    string? Cargo,
    string? Email,
    string? Telefone,
    string? Celular,
    bool Principal,
    bool Ativo,
    DateTime AtualizadoEm,
    string RowVersion);

public sealed record FornecedorContatoRequest(
    string Nome,
    string? Cargo,
    string? Email,
    string? Telefone,
    string? Celular,
    bool Principal,
    bool Ativo,
    string? RowVersion = null);

public sealed record ContabilLancamentoDto(
    int Id,
    int EmpresaId,
    string Lote,
    string? Sublote,
    DateTime Data,
    string? HistoricoPadrao,
    string? Complemento,
    decimal Valor,
    string Tipo,
    string? OrigemModulo,
    int? OrigemId,
    bool Estornado,
    int? LancamentoEstornoId,
    int UsuarioCadastroId,
    DateTime CriadoEm);

public sealed record ContabilLancamentoRequest(
    int EmpresaId,
    string? Lote,
    string? Sublote,
    DateTime Data,
    string? HistoricoPadrao,
    string? Complemento,
    decimal Valor,
    string? Tipo,
    string? OrigemModulo,
    int? OrigemId,
    List<ContabilPartidaRequest> Partidas);

public sealed record ContabilPartidaRequest(
    string Tipo,
    int PlanoContaId,
    int? CentroCustoId,
    decimal Valor,
    string? Historico);

public sealed record RazaoContabilDto(
    int EmpresaId,
    int PlanoContaId,
    DateTime Data,
    string Lote,
    string Tipo,
    decimal Valor,
    string? Historico,
    string? OrigemModulo,
    int? OrigemId);

public sealed record ConciliacaoFinanceiraDto(
    int LancamentoFinanceiroId,
    string? Descricao,
    decimal Valor,
    DateTime? DataPagamento,
    string? MeioPagamento,
    string? ContaBancaria,
    string Status,
    int? ConciliacaoId,
    string? ReferenciaBancaria,
    string? Observacoes,
    DateTime? DataConciliacao,
    string? RowVersion);

public sealed record ConciliacaoFinanceiraRequest(
    int LancamentoFinanceiroId,
    string? Status,
    string? ReferenciaBancaria,
    string? Observacoes,
    string? RowVersion);

public sealed record FinanceiroConciliavelRow(
    int Id,
    string Status,
    DateTime? DataPagamento);

public sealed record DreGerencialDto(
    int EmpresaId,
    string? Competencia,
    string? ClasseConta,
    string? ContaCodigo,
    string? ContaNome,
    decimal? ValorDebito,
    decimal? ValorCredito,
    decimal? SaldoConta);

public sealed record FechamentoContabilDto(
    int Id,
    int EmpresaId,
    DateTime Periodo,
    DateTime DataFechamento,
    int UsuarioResponsavelId,
    bool Bloqueado,
    string? Observacoes);

public sealed record FechamentoContabilRequest(
    int EmpresaId,
    DateTime Periodo,
    bool Bloqueado,
    string? Observacoes);

public sealed record SpedArquivoDto(
    int Id,
    int EmpresaId,
    string Periodo,
    string Tipo,
    string? NomeArquivo,
    string Status,
    string? Protocolo,
    string? MensagemErro,
    DateTime CriadoEm);

public sealed record SpedGeracaoRequest(
    int EmpresaId,
    string Tipo,
    int? Ano,
    DateTime? Periodo);

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

public sealed record CrmSegmentoRequest(
    string Nome,
    string? Descricao,
    string Cor,
    int Prioridade,
    decimal? TicketMedioMinimo,
    decimal? TicketMedioMaximo,
    int? FrequenciaMinimaDias,
    int? FrequenciaMaximaDias,
    bool Ativo,
    string? RowVersion);

public sealed record CrmSegmentoDto(
    Guid Id,
    string Nome,
    string? Descricao,
    string Cor,
    int Prioridade,
    decimal? TicketMedioMinimo,
    decimal? TicketMedioMaximo,
    int? FrequenciaMinimaDias,
    int? FrequenciaMaximaDias,
    bool Ativo,
    string RowVersion,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CrmCampanhaRequest(
    string Nome,
    string? Descricao,
    string Tipo,
    string Status,
    Guid? SegmentoId,
    DateTime DataInicio,
    DateTime? DataFim,
    decimal Orcamento,
    decimal CustoAtual,
    int Alcance,
    int Cliques,
    int LeadsGerados,
    int OportunidadesGeradas,
    int VendasGeradas,
    decimal ReceitaGerada,
    string? PublicoAlvo,
    string? Conteudo,
    string? RowVersion);

public sealed record CrmCampanhaDto(
    Guid Id,
    string Nome,
    string? Descricao,
    string Tipo,
    string Status,
    Guid? SegmentoId,
    string? SegmentoNome,
    DateTime DataInicio,
    DateTime? DataFim,
    decimal Orcamento,
    decimal CustoAtual,
    int Alcance,
    int Cliques,
    int LeadsGerados,
    int OportunidadesGeradas,
    int VendasGeradas,
    decimal ReceitaGerada,
    decimal Roas,
    decimal Cpc,
    decimal Cpl,
    decimal Cpa,
    string? PublicoAlvo,
    string? Conteudo,
    string RowVersion,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

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

public sealed record RelatorioVendasDto(
    decimal FaturamentoHoje,
    decimal FaturamentoMes,
    decimal FaturamentoAno,
    int PedidosHoje,
    int PedidosMes,
    decimal TicketMedio,
    List<FaturamentoPorPeriodoDto> FaturamentoSemanal,
    List<FaturamentoPorPeriodoDto> FaturamentoMensal,
    List<VendasPorLojaDto> VendasPorLoja,
    List<ProdutosMaisVendidosDto> ProdutosMaisVendidos);

public sealed record FinanceiroFaturamentoDto(
    decimal ReceitaHoje,
    decimal ReceitaMes,
    decimal ReceitaAno,
    decimal DespesasMes,
    decimal ResultadoMes,
    decimal PendenteReceber,
    decimal PendentePagar,
    DateTime AtualizadoEm);

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
    bool Ativa = true,
    int LojaId = 0);

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

public sealed record ProdutoIdentificadorDto(
    int Id,
    int LojaId,
    string Nome,
    string Sku,
    string? CodigoBarras,
    string? QrCode,
    bool Ativo,
    string RowVersion);

public sealed record ProdutoIdentificadorUpdateRequest(
    string? CodigoBarras,
    string? QrCode,
    string? RowVersion);

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
    string? EmpresaAquisicaoCodigo = null,
    int LojaId = 0)
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

public sealed record CupomDto(
    string Codigo,
    decimal? DescontoPercentual,
    decimal? DescontoValor,
    decimal? ValorMinimo,
    decimal? ValorMaximoDesconto,
    bool FreteGratis);

public sealed record CupomAdminRequest(
    string Codigo,
    string Tipo,
    decimal Valor,
    decimal ValorMinimoPedido,
    decimal? ValorMaximoDesconto,
    int? QuantidadeUsos,
    int QuantidadePorCliente,
    DateTime? ValidoDe,
    DateTime? ValidoAte,
    bool PrimeiroCompraOnly,
    bool Ativo);

public sealed record CupomAdminDto(
    int Id,
    string Codigo,
    string Tipo,
    decimal Valor,
    decimal ValorMinimoPedido,
    decimal? ValorMaximoDesconto,
    int? QuantidadeUsos,
    int UsosAtuais,
    int QuantidadePorCliente,
    DateTime? ValidoDe,
    DateTime? ValidoAte,
    bool PrimeiroCompraOnly,
    bool Ativo,
    DateTime CreatedAt,
    DateTime UpdatedAt);

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
    string? Tipo = null,
    string? RowVersion = null);

public sealed record ReenviarConfirmacaoClienteRequest(string Email);

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
    DateTime? UpdatedAt = null,
    string? RowVersion = null);

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
    bool Padrao,
    string? RowVersion = null);

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
    bool Padrao,
    string? RowVersion = null);

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
    string? Observacoes = null,
    string? RowVersion = null);

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
    DateTime? UpdatedAt = null,
    string RowVersion = "");

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
    int? SolicitacaoId,
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
    List<CompraCotacaoDto> Cotacoes,
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

public sealed record CompraCotacaoDto(
    int Id,
    int SolicitacaoId,
    int FornecedorId,
    string FornecedorNome,
    int? ProdutoId,
    string ProdutoNome,
    int Quantidade,
    decimal CustoUnitario,
    decimal ValorTotal,
    int PrazoEntregaDias,
    string Origem,
    string Status,
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

public sealed class CompraCotacaoRow
{
    public int Id { get; set; }
    public int SolicitacaoId { get; set; }
    public int FornecedorId { get; set; }
    public string? FornecedorNome { get; set; }
    public int? ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal CustoUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public int PrazoEntregaDias { get; set; }
    public string Origem { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
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

public sealed record PedidoItemRequest(string ProdutoId, int Quantidade, string? Variacao = null);

public sealed record CheckoutCreationResult(Pedido? Pedido, IResult? Error)
{
    public static CheckoutCreationResult Success(Pedido pedido) => new(pedido, null);

    public static CheckoutCreationResult Fail(IResult error) => new(null, error);
}

public sealed record CheckoutIntegrationResult(bool Succeeded, string? FailureCode)
{
    public static CheckoutIntegrationResult Success() => new(true, null);

    public static CheckoutIntegrationResult Failure(string failureCode) => new(false, failureCode);
}

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
    string? PaymentUrl,
    string? StatusFiscal = null,
    string? AlertaOperacional = null);

public sealed record PedidoLogisticaRequest(
    [property: JsonPropertyName("frete_metodo")] string? FreteMetodo,
    [property: JsonPropertyName("frete_transportadora")] string? FreteTransportadora,
    [property: JsonPropertyName("frete_prazo_dias")] int? FretePrazoDias,
    [property: JsonPropertyName("frete_codigo_rastreio")] string? FreteCodigoRastreio,
    [property: JsonPropertyName("row_version")] string? RowVersion);

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
    DateTime UpdatedAt,
    string? RastreamentoStatusExterno,
    List<LogisticaRastreamentoEventoDto> RastreamentoEventos,
    DateTime? RastreamentoConsultadoEm);

public sealed record DropshippingCanalRequest(
    string Nome,
    string Slug,
    string Tipo,
    string? ApiEndpoint,
    bool Ativo,
    string? RowVersion);

public sealed record DropshippingCanalDto(
    int Id,
    string Nome,
    string Slug,
    string Tipo,
    string? ApiEndpoint,
    bool Ativo,
    bool CredenciaisConfiguradas,
    string StatusCredenciais,
    string DetalheCredenciais,
    string RowVersion,
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

public sealed record MarketplaceSyncRequest(
    [property: JsonPropertyName("direcao")] string? Direcao,
    [property: JsonPropertyName("entidade")] string? Entidade,
    [property: JsonPropertyName("limite")] int? Limite,
    [property: JsonPropertyName("produto_ids")] List<int>? ProdutoIds,
    [property: JsonPropertyName("categoria_externa")] string? CategoriaExterna,
    [property: JsonPropertyName("forcar")] bool? Forcar);

public sealed record MarketplaceSyncResultadoDto(
    bool Sucesso,
    string Canal,
    string Direcao,
    string Entidade,
    bool Configurada,
    bool Operacional,
    int ProdutosLocaisElegiveis,
    int RegistrosExternosLidos,
    int ProdutosExternosEnviados,
    string? ProviderHost,
    List<string> Pendencias,
    DateTime ExecutadoEm);

public sealed record MarketplaceSyncConfiguration(
    string Canal,
    string? EndpointBase,
    Uri? PedidosEndpoint,
    Uri? ProdutosEndpoint,
    string? Token,
    string? SellerId,
    string? ProviderHost,
    List<string> Pendencias)
{
    public bool Configurada => PedidosEndpoint is not null &&
        !string.IsNullOrWhiteSpace(Token) &&
        !string.IsNullOrWhiteSpace(SellerId) &&
        Pendencias.Count == 0;
}

public sealed record MarketplaceHttpResult(
    bool Sucesso,
    string? Body,
    List<string> Pendencias);

public sealed record SiteMidiaUploadRequest(
    string? Nome,
    string? Tipo,
    string? TextoAlternativo,
    string? FileName,
    string? ContentType,
    string? DataUrl);

public sealed record SiteMidiaUpdateRequest(
    string? Nome,
    string? Tipo,
    string? TextoAlternativo,
    string? RowVersion);

public sealed record SiteMidiaDto(
    Guid Id,
    Guid TenantId,
    string Nome,
    string Tipo,
    string TextoAlternativo,
    string NomeArquivoOriginal,
    string CaminhoRelativo,
    string Url,
    string ContentType,
    long TamanhoBytes,
    int Largura,
    int Altura,
    string RowVersion,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record DecodedSiteImage(
    byte[] Bytes,
    string ContentType,
    string Extension,
    int Width,
    int Height);

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
    string Image,
    string? ImageAlt = null,
    bool? Active = null,
    int Order = 0);

public sealed record StoreCardSiteDto(
    Guid Id,
    int? LojaId,
    string Nome,
    string Slug,
    string Segmento,
    string Atividade,
    string Descricao,
    string? Imagem,
    string? Logo,
    string Icon,
    string? CtaTexto,
    string? CtaUrl,
    string? CorPrimaria,
    string? CorSecundaria,
    string? CorFundo,
    string? CorTexto,
    int OrdemExibicao);

public sealed record PartnerCardSiteDto(
    Guid Id,
    int? OrigemId,
    string Tipo,
    string OrigemTipo,
    string Title,
    string Slug,
    string Text,
    string Cta,
    string Href,
    string Icon,
    string? Image,
    string? Logo,
    string Activity,
    string Segment,
    string? CorPrimaria,
    string? CorSecundaria,
    string? CorFundo,
    string? CorTexto,
    int Order);

public sealed record SitePerfilPublicoRequest(
    string TipoPerfil,
    string OrigemTipo,
    int? OrigemId,
    string Nome,
    string Slug,
    string Segmento,
    string Atividade,
    string Descricao,
    string? LogoUrl,
    string? BannerUrl,
    string? Icone,
    string? CtaTexto,
    string? CtaUrl,
    string? SiteUrl,
    string? EmailPublico,
    string? TelefonePublico,
    string? EnderecoPublico,
    string? CorPrimaria,
    string? CorSecundaria,
    string? CorFundo,
    string? CorTexto,
    List<int>? ProdutoIds,
    bool Publicado,
    int OrdemExibicao,
    string? RowVersion = null);

public sealed record SitePerfilPublicoAdminDto(
    Guid Id,
    string TipoPerfil,
    string OrigemTipo,
    int? OrigemId,
    string Nome,
    string Slug,
    string Segmento,
    string Atividade,
    string Descricao,
    string? LogoUrl,
    string? BannerUrl,
    string? Icone,
    string? CtaTexto,
    string? CtaUrl,
    string? SiteUrl,
    string? EmailPublico,
    string? TelefonePublico,
    string? EnderecoPublico,
    string? CorPrimaria,
    string? CorSecundaria,
    string? CorFundo,
    string? CorTexto,
    List<int> ProdutoIds,
    bool Publicado,
    int OrdemExibicao,
    string RowVersion,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record SitePerfilOrigemDto(
    string Tipo,
    int Id,
    string Nome,
    string Slug,
    string Status);

public sealed record SitePerfilProdutoDisponivelDto(
    int Id,
    string Sku,
    string Nome,
    string Slug,
    decimal Preco,
    string ImagemUrl,
    int LojaId,
    int? FornecedorId,
    string TipoProduto);

public sealed record PartnerDetailSiteDto(
    Guid Id,
    int? OrigemId,
    string Tipo,
    string OrigemTipo,
    string Title,
    string Slug,
    string Text,
    string Icon,
    string? Image,
    string? Logo,
    string Activity,
    string Segment,
    int Order,
    string? SiteUrl,
    string? EmailPublico,
    string? TelefonePublico,
    string? EnderecoPublico,
    string? CorPrimaria,
    string? CorSecundaria,
    string? CorFundo,
    string? CorTexto,
    string? Cta,
    string? Href,
    List<ProdutoLojaDto> Produtos);

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
    string PrimaryColor,
    string SecondaryColor,
    string BackgroundColor,
    string SurfaceColor,
    string TextColor,
    string MutedTextColor,
    int HeroIntervalSeconds,
    int PartnerRotationSeconds,
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

public sealed record LogisticaRastreamentoDto(
    Guid ConsultaId,
    string CodigoRastreio,
    int PedidoId,
    string NumeroPedido,
    string? Transportadora,
    string? MetodoFrete,
    string StatusInterno,
    DateTime? DataEnvio,
    DateTime? DataEntrega,
    DateTime? PrevisaoEntrega,
    bool RastreamentoExternoConfigurado,
    bool RastreamentoExternoOperacional,
    string FonteExterna,
    int? HttpStatusCode,
    string? StatusExterno,
    List<LogisticaRastreamentoEventoDto> Eventos,
    List<string> Pendencias,
    string? RespostaSha256,
    DateTime ConsultadoEm);

public sealed record LogisticaExpedicaoDto(
    int PedidoId,
    string NumeroPedido,
    string ClienteNome,
    string Status,
    string? FreteMetodo,
    string? FreteTransportadora,
    int FretePrazoDias,
    string? FreteCodigoRastreio,
    DateTime? DataEnvio,
    DateTime? DataEntrega,
    DateTime? PrevisaoEntrega,
    bool UltimoRastreamentoOperacional,
    string? UltimoStatusExterno,
    DateTime? UltimaConsultaEm,
    string RowVersion,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record LogisticaRastreamentoEventoDto(
    DateTime? DataHora,
    string Status,
    string? Local,
    string? Descricao);

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
    decimal TicketMedio,
    int PeriodoDias,
    DateOnly PeriodoInicio,
    DateOnly PeriodoFim,
    DateTime AtualizadoEm,
    IReadOnlyList<DashboardVendaDiaDto> VendasPeriodo);

public sealed record DashboardVendaDiaDto(
    DateOnly Data,
    int PedidosConfirmados,
    decimal ReceitaConfirmada);

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

public sealed record CicloOperacionalCorporativoDto(
    CicloOperacionalResumoDto Resumo,
    List<CicloOperacionalEtapaDto> Etapas,
    List<CicloOperacionalAlertaDto> Alertas,
    DateTime AtualizadoEm);

public sealed record CicloOperacionalResumoDto(
    int PedidosAbertos,
    int PedidosPagos,
    int FinanceiroPendente,
    int FiscalPendente,
    int LogisticaPendente,
    int ComprasPendentes,
    int EstoqueRisco);

public sealed record CicloOperacionalEtapaDto(
    string Chave,
    string Titulo,
    int Total,
    int Pendentes,
    string Status,
    string Detalhe,
    string Acao);

public sealed record CicloOperacionalAlertaDto(
    string Severidade,
    string Titulo,
    string Detalhe,
    string Acao);

public sealed record DicionarioDadosCorporativoDto(
    DicionarioDadosResumoDto Resumo,
    List<DicionarioModuloDto> Modulos,
    List<DicionarioTabelaDto> Tabelas,
    DateTime AtualizadoEm);

public sealed record DicionarioDadosResumoDto(
    int Bancos,
    int Tabelas,
    int Colunas,
    int Relacionamentos,
    int TabelasComPendencias,
    int ColunasPendentes);

public sealed record DicionarioModuloDto(
    string Nome,
    int Tabelas,
    int Colunas,
    int ColunasPendentes,
    int Relacionamentos,
    string Status);

public sealed record DicionarioTabelaDto(
    string Banco,
    string Tabela,
    string Modulo,
    string FormularioDestino,
    int TotalColunas,
    int ColunasCobertas,
    int ColunasPendentes,
    int TotalRelacionamentos,
    string Status,
    List<string> CamposPendentes,
    List<DicionarioColunaDto> Colunas,
    List<DicionarioRelacionamentoDto> Relacionamentos);

public sealed record DicionarioColunaDto(
    string Nome,
    string Tipo,
    string TipoCompleto,
    bool PermiteNulo,
    string Chave,
    string? Padrao,
    string Extra,
    bool CobertaPorFormulario,
    string UsoOperacional);

public sealed record DicionarioRelacionamentoDto(
    string NomeConstraint,
    string Coluna,
    string BancoReferencia,
    string TabelaReferencia,
    string ColunaReferencia,
    string ModuloReferencia);

public sealed class DatabaseColumnInventoryRow
{
    public string Banco { get; set; } = string.Empty;
    public string Tabela { get; set; } = string.Empty;
    public string Coluna { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string TipoCompleto { get; set; } = string.Empty;
    public string PermiteNulo { get; set; } = string.Empty;
    public string Chave { get; set; } = string.Empty;
    public string? Padrao { get; set; }
    public string Extra { get; set; } = string.Empty;
    public int Ordem { get; set; }
}

public sealed class DatabaseRelationshipInventoryRow
{
    public string NomeConstraint { get; set; } = string.Empty;
    public string Banco { get; set; } = string.Empty;
    public string Tabela { get; set; } = string.Empty;
    public string Coluna { get; set; } = string.Empty;
    public string BancoReferencia { get; set; } = string.Empty;
    public string TabelaReferencia { get; set; } = string.Empty;
    public string ColunaReferencia { get; set; } = string.Empty;
}

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
=======
static bool IsWorkflowProfileAuthorized(ClaimsPrincipal principal, List<string> perfisAutorizados)
>>>>>>> 0b859b83b843d38e6b609297964ac23e3fe66577
{
    if (perfisAutorizados == null || perfisAutorizados.Count == 0) return true;
    return perfisAutorizados.Any(p => principal.IsInRole(p));
}
<<<<<<< HEAD

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

public sealed record PdvCockpitDto(
    List<PdvFiscalConfigDto> Configuracoes,
    List<PdvIndicadorDto> Indicadores,
    List<PdvPendenciaDto> Pendencias,
    DateTime AtualizadoEm);

public sealed record PdvIndicadorDto(
    string Chave,
    string Titulo,
    string Valor,
    string Detalhe,
    string Status,
    string Modulo);

public sealed record PdvPendenciaDto(
    string Titulo,
    string Detalhe,
    string Severidade,
    string Acao);

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

public sealed record FiscalEmissaoRequest(
    [property: JsonPropertyName("fiscal_id")] int? FiscalId,
    [property: JsonPropertyName("pedido_id")] int? PedidoId,
    [property: JsonPropertyName("empresa_grupo_id")] int? EmpresaGrupoId,
    [property: JsonPropertyName("force_reissue")] bool? ForceReissue,
    [property: JsonPropertyName("observacao_operacional")] string? ObservacaoOperacional);

public sealed record FiscalOperacaoRequest(
    [property: JsonPropertyName("fiscal_id")] int? FiscalId,
    [property: JsonPropertyName("pedido_id")] int? PedidoId,
    [property: JsonPropertyName("empresa_grupo_id")] int? EmpresaGrupoId,
    [property: JsonPropertyName("chave_acesso")] string? ChaveAcesso,
    [property: JsonPropertyName("protocolo")] string? Protocolo,
    [property: JsonPropertyName("motivo")] string? Motivo,
    [property: JsonPropertyName("numero_inicial")] int? NumeroInicial,
    [property: JsonPropertyName("numero_final")] int? NumeroFinal,
    [property: JsonPropertyName("serie")] string? Serie,
    [property: JsonPropertyName("texto_correcao")] string? TextoCorrecao);

public sealed record FiscalOperacaoResultadoDto(
    bool Sucesso,
    int FiscalId,
    int PedidoId,
    string ModeloDocumento,
    string Operacao,
    string StatusNfe,
    string? StatusAutomacao,
    string? Numero,
    string? Serie,
    string? ChaveAcesso,
    string? Protocolo,
    string? XmlUrl,
    string? DanfeUrl,
    string Provider,
    string? ProviderHost,
    int? ProviderStatusCode,
    List<string> Pendencias,
    DateTime ExecutadoEm);

public sealed record FiscalProviderConfiguration(
    string Provider,
    Uri? Endpoint,
    string? Token,
    List<string> Pendencias);

public sealed record FiscalProviderResult(
    bool HttpSucceeded,
    int? StatusCode,
    string? Body,
    List<string> Pendencias,
    string? ResponseSnippet);

public sealed record FiscalProviderParsedResult(
    string? ChaveAcesso,
    string? Protocolo,
    string? Numero,
    string? Serie,
    string? XmlUrl,
    string? DanfeUrl,
    bool Autorizado,
    bool Denegado,
    List<string> Pendencias);

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

public sealed record TenantDocumentoLookup(string? Documento);

public sealed record GenesisEmpresaLookup(int Id);

public sealed record GenesisSaldoEstoqueDto(
    string Sku,
    string Produto,
    decimal QuantidadeFisica,
    decimal QuantidadeReservada,
    decimal QuantidadeDisponivel);
public sealed record GenesisMovimentacaoEstoqueDto(
    int Id,
    string Sku,
    string Produto,
    string Tipo,
    DateTime DataMovimentacao,
    decimal Quantidade,
    decimal? CustoUnitario,
    decimal? CustoTotal,
    int? DepositoOrigemId,
    int? DepositoDestinoId,
    string? Lote,
    string? Serie,
    string? DocumentoTipo,
    int? DocumentoId,
    string? Observacoes);

public partial class Program { }
=======
>>>>>>> 0b859b83b843d38e6b609297964ac23e3fe66577
