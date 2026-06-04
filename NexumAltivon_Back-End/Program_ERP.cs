using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using NexumAltivon.API.Data;
using NexumAltivon.API.Configurations;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// CONFIGURAÃ‡ÃƒO DE SERVIÃ‡OS â€” ERP GenesisGest.Net
// Grupo Nexum Altivon ME | www.nexumaltivon.com
// Fase 5 â€” ERP/CRM Completo
// ============================================

// 1. Controllers + JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// 2. DbContext â€” MySQL
var connectionString = builder.Configuration.GetConnectionString("NexumDb");
builder.Services.AddDbContext<NexumDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
        b => b.MigrationsAssembly("NexumAltivon.API")
              .EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)));

// 3. JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero
    };
});

// 4. Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdmin", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin", "SuperAdmin"));
    options.AddPolicy("Gerente", policy => policy.RequireRole("Gerente", "Admin", "SuperAdmin"));
    options.AddPolicy("Financeiro", policy => policy.RequireRole("Financeiro", "Admin", "SuperAdmin"));
    options.AddPolicy("Fiscal", policy => policy.RequireRole("Fiscal", "Admin", "SuperAdmin"));
    options.AddPolicy("Vendedor", policy => policy.RequireRole("Vendedor", "Gerente", "Admin", "SuperAdmin"));
});

// 5. AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// 6. DI Services â€” E-Commerce (Fases 1-4)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILojaService, LojaService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<ICarrinhoService, CarrinhoService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IMercadoPagoService, MercadoPagoService>();
builder.Services.AddScoped<IFreteService, FreteService>();
builder.Services.AddScoped<INotificacaoService, NotificacaoService>();
builder.Services.AddScoped<ILogAuditoriaService, LogAuditoriaService>();
builder.Services.AddScoped<IConfiguracaoService, ConfiguracaoService>();
builder.Services.AddScoped<IMercadoLivreService, MercadoLivreService>();
builder.Services.AddScoped<IMarketplaceHubService, MarketplaceHubService>();
builder.Services.AddScoped<IDropshippingService, DropshippingService>();
builder.Services.AddScoped<ILogisticaService, LogisticaService>();
builder.Services.AddScoped<IErpSyncService, ErpSyncService>();
builder.Services.AddScoped<IMarketplaceSyncService, MarketplaceSyncService>();

// 7. DI Services â€” ERP/CRM (Fase 5)
builder.Services.AddScoped<IFinanceiroService, FinanceiroService>();
builder.Services.AddScoped<ICrmService, CrmService>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();
builder.Services.AddScoped<IFiscalService, FiscalService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<ISyncErpService, SyncErpService>();
builder.Services.AddScoped<IFornecedorService, FornecedorService>();
builder.Services.AddScoped<IErpDashboardService, ErpDashboardService>();

// 8. Admin Dashboard (Fase 2)
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

// 9. Middlewares
builder.Services.AddTransient<ExceptionMiddleware>();
builder.Services.AddTransient<AuditoriaMiddleware>();

// 10. Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IRateLimitingService, RateLimitingService>();

// 11. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("NexumCors", policy =>
    {
        policy.WithOrigins(
                "https://www.nexumaltivon.com",
                "https://nexumaltivon.com",
                "https://admin.nexumaltivon.com",
                "https://erp.nexumaltivon.com",
                "http://localhost:3000",
                "http://localhost:5000",
                "http://localhost:5001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// 12. Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Nexum Altivon API â€” ERP GenesisGest.Net",
        Version = "v1.0.00.2600",
        Description = "API unificada E-Commerce + ERP/CRM do Grupo Nexum Altivon ME",
        Contact = new OpenApiContact
        {
            Name = "Grupo Nexum Altivon",
            Email = "corporativo.gna@gmail.com",
            Url = new Uri("https://www.nexumaltivon.com")
        }
    });

    // JWT Security Definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// 13. Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<NexumDbContext>("mysql-nexum")
    .AddCheck<ApiHealthCheck>("api-custom");

// 14. Hangfire (para sync automÃ¡tico ERP â†” E-Commerce)
builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseStorage(new Hangfire.MySql.MySqlStorage(
              connectionString,
              new Hangfire.MySql.MySqlStorageOptions
              {
                  TablesPrefix = "hangfire_",
                  QueuePollInterval = TimeSpan.FromSeconds(15)
              })));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 2;
    options.Queues = new[] { "default", "sync", "fiscal", "relatorios" };
    options.SchedulePollingInterval = TimeSpan.FromSeconds(15);
});

// ============================================
// BUILD APP
// ============================================
var app = builder.Build();

// Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nexum Altivon API v1");
        c.DocumentTitle = "Nexum Altivon â€” DocumentaÃ§Ã£o API";
        c.DefaultModelsExpandDepth(-1);
    });
}

app.UseHttpsRedirection();
app.UseCors("NexumCors");

// Middlewares customizados
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<AuditoriaMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Hangfire Dashboard (protegido)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Agendamentos automÃ¡ticos ERP
using (var scope = app.Services.CreateScope())
{
    var syncService = scope.ServiceProvider.GetRequiredService<ISyncErpService>();

    // Sync completo a cada 15 minutos
    RecurringJob.AddOrUpdate<ISyncErpService>(
        "sync-completo-erp",
        service => service.ExecutarSyncAgendadoAsync(),
        "*/15 * * * *",
        new RecurringJobOptions { QueueName = "sync" });

    // Sync de estoque a cada 30 minutos
    RecurringJob.AddOrUpdate<ISyncErpService>(
        "sync-estoque",
        service => service.SincronizarEstoqueAsync(),
        "*/30 * * * *",
        new RecurringJobOptions { QueueName = "sync" });

    // Alerta de contas vencidas â€” todo dia Ã s 8h
    RecurringJob.AddOrUpdate<IFinanceiroService>(
        "alerta-contas-vencidas",
        service => service.VerificarContasVencidasAsync(),
        "0 8 * * *",
        new RecurringJobOptions { QueueName = "default" });
}

// Seed inicial (desenvolvimento)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<NexumDbContext>();
    context.Database.EnsureCreated();
}

app.Run();

