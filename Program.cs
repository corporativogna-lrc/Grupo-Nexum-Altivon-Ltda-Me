using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation.AspNetCore;
using Hangfire;
using NexumAltivon.ERP.Configurations;
using NexumAltivon.ERP.Data;
using Quartz;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.MySQL(
        connectionString: builder.Configuration.GetConnectionString("NexumDb"),
        tableName: "erp_logs",
        storeTimestampInUtc: true)
    .CreateLogger();

builder.Host.UseSerilog();

// DB Context
builder.Services.AddDbContext<NexumDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("NexumDb"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("NexumDb")),
        b => b.MigrationsAssembly("NexumAltivon.ERP")));

// JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret não configurado.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ERP.Admin", policy => policy.RequireRole("SuperAdmin", "Admin", "Gerente"));
    options.AddPolicy("ERP.Financeiro", policy => policy.RequireRole("SuperAdmin", "Admin", "Financeiro"));
    options.AddPolicy("ERP.Fiscal", policy => policy.RequireRole("SuperAdmin", "Admin", "Fiscal"));
    options.AddPolicy("ERP.Operacional", policy => policy.RequireRole("SuperAdmin", "Admin", "Gerente", "Operacional"));
});

// AutoMapper
builder.Services.AddAutoMapper(_ => { }, typeof(Program).Assembly);

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();

// Services & DI
builder.Services.AddERPServices(builder.Configuration);

// Hangfire
builder.Services.AddHangfire(config =>
    config.UseStorage(new Hangfire.MySql.MySqlStorage(
        builder.Configuration.GetConnectionString("NexumDb"),
        new Hangfire.MySql.MySqlStorageOptions())));
builder.Services.AddHangfireServer();

// Quartz
builder.Services.AddQuartz(q =>
{
    q.UsePersistentStore(store =>
    {
        store.UseMySql(builder.Configuration.GetConnectionString("NexumDb"));
    });
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Nexum Altivon ERP API",
        Version = "v1",
        Description = "ERP/CRM GenesisGest.Net — Gestão integrada do Grupo Nexum Altivon"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT: Bearer {seu_token}"
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
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ErpCors", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nexum Altivon ERP v1"));
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("ErpCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseHangfireDashboard("/hangfire", new Hangfire.DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Garante migrações na primeira execução
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NexumDbContext>();
    db.Database.Migrate();
}

app.Run();
