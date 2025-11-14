using Api_Portar_Paciente.HealthChecks;
using Api_Portar_Paciente.Services;
using AspNetCoreRateLimit;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;
using CC.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

// ===== CONFIGURAR SERILOG ANTES DE CONSTRUIR EL HOST =====
Log.Logger = new LoggerConfiguration()
 .ReadFrom.Configuration(new ConfigurationBuilder()
 .SetBasePath(Directory.GetCurrentDirectory())
 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
 .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
 .Build())
 .Enrich.FromLogContext()
 .Enrich.WithMachineName()
 .Enrich.WithEnvironmentName()
 .Enrich.WithProperty("Application", "PortalPacientesAPI")
 .CreateLogger();

try
{
    Log.Information("Iniciando aplicación Portal Pacientes API");

    var builder = WebApplication.CreateBuilder(args);

    // ===== USAR SERILOG COMO LOGGER =====
    builder.Host.UseSerilog();

    // Configurar el entorno y las configuraciones específicas
    var environment = builder.Environment.EnvironmentName;
    Log.Information("Ejecutándose en ambiente: {Environment}", environment);

    // Cargar configuraciones específicas del ambiente
    Log.Information("Cargar configuraciones especificas del ambiente");

    builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

    // ===== CORS CONFIGURATION =====
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
     {
        policy.AllowAnyOrigin()
     .AllowAnyMethod()
     .AllowAnyHeader();
    });
    });

    // Add services to the container.
    builder.Services.AddControllers();

    // ===== LOG CLEANUP SERVICE =====
    builder.Services.AddHostedService<LogCleanupService>();
    Log.Information("LogCleanupService registrado como Hosted Service");

    // ===== AUTH CLEANUP SERVICE =====
    builder.Services.AddHostedService<AuthCleanupService>();
    Log.Information("AuthCleanupService registrado como Hosted Service");

    // ===== RATE LIMITING CONFIGURATION =====
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
    builder.Services.AddInMemoryRateLimiting();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

    // JWT Authentication
    var jwtSecret = builder.Configuration["Authentication:JwtSecret"];
    if (!string.IsNullOrWhiteSpace(jwtSecret))
    {
        if (jwtSecret.Length < 32)
        {
            Log.Warning("JWT Secret es demasiado corto. Se recomienda mínimo32 caracteres.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Authentication:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Authentication:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        Log.Information("Autenticación JWT configurada correctamente");
    }
    else
    {
        Log.Warning("JWT Secret no configurado. La autenticación JWT no estará disponible.");
    }

    // ===== IDENTITY CONFIGURATION =====
    builder.Services.AddIdentity<CC.Domain.Entities.User, CC.Domain.Entities.Role>(options =>
    {
        // Password settings (puedes ajustarlas según tus necesidades)
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<DBContext>()
    .AddDefaultTokenProviders();

    Log.Information("Identity configurado correctamente");

    // ===== AUTHORIZATION POLICIES CONFIGURATION =====
    builder.Services.AddHttpContextAccessor();

    // Registrar Authorization Handler (Scoped porque usa IAuthorizationService que es Scoped)
    builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, 
        CC.Infrastructure.Authorization.PermissionHandler>();

    // Configurar políticas usando configuración centralizada
    builder.Services.AddAuthorization(Api_Portar_Paciente.Configuration.AuthorizationPoliciesConfiguration.ConfigurePolicies);

    Log.Information("Políticas de autorización configuradas: 27 políticas (PatientOnly, AdminOnly y 25 permisos granulares)");

    // Custom DI registrations (DbContext, Repositories, Services, Logging, Auditing)
    Api_Portar_Paciente.Handlers.DependencyInyectionHandler.DepencyInyectionConfig(
    builder.Services,
    builder.Configuration,
    environment);

    // ===== CONFIGURAR SWAGGER CON DOCUMENTACIÓN XML =====
    var enableSwagger = builder.Configuration.GetValue<bool>("Features:EnableSwagger");
    if (enableSwagger || true)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Portal Pacientes API - Cardioinfantil",
                Version = "v1.0",
                Description = "API REST para el Portal de Pacientes de la Fundación Cardioinfantil. " +
     "Proporciona endpoints para autenticación, gestión de contenido y servicios al paciente.",
                Contact = new OpenApiContact
                {
                    Name = "Equipo de Desarrollo Cardioinfantil",
                    Email = "desarrollo@cardioinfantil.org"
                }
            });

            // Incluir comentarios XML de todos los proyectos
            var xmlFiles = new[]
     {
 $"{Assembly.GetExecutingAssembly().GetName().Name}.xml",
 "CC.Domain.xml",
 "CC.Aplication.xml"
        };

            foreach (var xmlFile in xmlFiles)
            {
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                    Log.Information("Documentación XML cargada: {XmlFile}", xmlFile);
                }
                else
                {
                    Log.Warning("Archivo XML no encontrado: {XmlFile}", xmlFile);
                }
            }

            // Configurar autenticación JWT en Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header usando el esquema Bearer. \r\n\r\n" +
     "Ingresa 'Bearer' [espacio] y luego tu token en el campo de texto.\r\n\r\n" +
     "Ejemplo: 'Bearer12345abcdef'",
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
 },
 Scheme = "oauth2",
 Name = "Bearer",
 In = ParameterLocation.Header
 },
 new List<string>()
 }
        });
        });

        Log.Information("Swagger configurado y habilitado");
    }

    // Configurar Health Checks basado en el ambiente
    ConfigureHealthChecks(builder.Services, builder.Configuration);

    var app = builder.Build();

    // ===== APLICAR MIGRACIONES Y SEED =====
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var db = services.GetRequiredService<DBContext>();
            await db.Database.MigrateAsync();

            var seeder = services.GetRequiredService<SeedDB>();
            await seeder.SeedAsync();
            Log.Information("✅ Migraciones aplicadas e inicialización de datos completada");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Error al aplicar migraciones/seed al iniciar la aplicación");
            // No lanzar excepción para permitir que la aplicación inicie
        }
    }

    // ===== USAR SERILOG PARA REQUESTS HTTP =====
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
     {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
    };
    });

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment() || enableSwagger)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", $"Portal Pacientes API - {environment} v1.0");
            c.DocumentTitle = $"Portal Pacientes API - {environment}";
            c.RoutePrefix = "swagger";
            c.DisplayRequestDuration();
            c.EnableDeepLinking();
            c.EnableFilter();
            c.EnableTryItOutByDefault();

            c.InjectJavascript("/swagger-ext/polyfills.js");
        });

        Log.Information("Swagger UI disponible en /swagger");
    }

    // Middleware de manejo de errores basado en el ambiente
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    // ===== RATE LIMITING MIDDLEWARE (ANTES DE AUTHENTICATION) =====
    app.UseIpRateLimiting();

    // ===== CORS =====
    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    // ===== ERROR HANDLING MIDDLEWARE =====
    app.UseMiddleware<Api_Portar_Paciente.Handlers.ErrorHandlingMiddleware>();

    // Heartbeat de sesión (actualiza LastSeenAt con throttling)
    app.UseMiddleware<Api_Portar_Paciente.Handlers.SessionHeartbeatMiddleware>();

    app.MapControllers();

    // Configurar endpoints de Health Checks
    ConfigureHealthCheckEndpoints(app);

    // Mostrar información del ambiente al iniciar
    Log.Information("✅ Aplicación iniciada en ambiente: {Environment}", environment);
    Log.Information("📚 Swagger habilitado: {SwaggerEnabled}", enableSwagger);
    Log.Information("🚦 Rate Limiting habilitado: {RateLimitingEnabled}", true);
    Log.Information("🧹 Log Cleanup: Retención de {RetentionDays} días, limpieza a las {CleanupHour}:00",
    builder.Configuration.GetValue<int>("Logging:RetentionDays", 30),
    builder.Configuration.GetValue<int>("Logging:CleanupHour", 3));

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ La aplicación terminó unexpectedly");
    throw;
}
finally
{
    Log.Information("🛑 Aplicación detenida");
    Log.CloseAndFlush();
}

// Constantes para Health Check tags
const string ApplicationTag = "application";
const string ConfigurationTag = "configuration";
const string ExternalServicesTag = "external-services";

// Método para configurar Health Checks
static void ConfigureHealthChecks(IServiceCollection services, IConfiguration configuration)
{
    var healthChecksBuilder = services.AddHealthChecks();

    // Health Check básico de la aplicación
    healthChecksBuilder.AddCheck<ApplicationHealthCheck>("application", HealthStatus.Degraded, new[] { ApplicationTag });

    // Health Check de configuración
    healthChecksBuilder.AddCheck<ConfigurationHealthCheck>("configuration", HealthStatus.Degraded, new[] { ConfigurationTag });

    // Health Checks de servicios externos (solo si están configurados)
    var emailServiceUrl = configuration["ExternalServices:EmailService:BaseUrl"];
    if (!string.IsNullOrEmpty(emailServiceUrl))
    {
        services.AddHttpClient("EmailServiceHealthCheck");
        healthChecksBuilder.AddTypeActivatedCheck<ExternalServiceHealthCheck>(
        "email-service",
        HealthStatus.Degraded,
        new[] { ExternalServicesTag },
        args: new object[] { emailServiceUrl, "Email Service" });
    }

    var notificationServiceUrl = configuration["ExternalServices:NotificationService:BaseUrl"];
    if (!string.IsNullOrEmpty(notificationServiceUrl))
    {
        services.AddHttpClient("NotificationServiceHealthCheck");
        healthChecksBuilder.AddTypeActivatedCheck<ExternalServiceHealthCheck>(
        "notification-service",
        HealthStatus.Degraded,
        new[] { ExternalServicesTag },
        args: new object[] { notificationServiceUrl, "Notification Service" });
    }
}

// Método para configurar endpoints de Health Checks
static void ConfigureHealthCheckEndpoints(WebApplication app)
{
    const string ApplicationTag = "application";
    const string ConfigurationTag = "configuration";
    const string ExternalServicesTag = "external-services";

    // Endpoint básico de health check
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
        ResultStatusCodes =
 {
 [HealthStatus.Healthy] = StatusCodes.Status200OK,
 [HealthStatus.Degraded] = StatusCodes.Status200OK,
 [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
 }
    });

    // Endpoint simple para load balancers (solo healthy/unhealthy)
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains(ApplicationTag) || check.Tags.Contains(ConfigurationTag),
        AllowCachingResponses = false
    });
}