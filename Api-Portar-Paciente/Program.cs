using Api_Portar_Paciente.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Configurar el entorno y las configuraciones específicas
var environment = builder.Environment.EnvironmentName;
Console.WriteLine($"Ejecutándose en ambiente: {environment}");

// Cargar configuraciones específicas del ambiente
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();

// Custom DI registrations (DbContext, Repositories, Services, Logging, Auditing)
Api_Portar_Paciente.Handlers.DependencyInyectionHandler.DepencyInyectionConfig(
    builder.Services,
    builder.Configuration,
    environment);

// Configurar Swagger basado en el ambiente
var enableSwagger = builder.Configuration.GetValue<bool>("Features:EnableSwagger");
if (enableSwagger)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

// Configurar logging basado en el ambiente
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

if (builder.Configuration.GetValue<bool>("Features:EnableDetailedLogging"))
{
    builder.Logging.AddDebug();
}

// Configurar Health Checks basado en el ambiente
ConfigureHealthChecks(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", $"Portal Pacientes API - {environment}");
        c.DocumentTitle = $"Portal Pacientes API - {environment}";
    });
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

app.UseAuthorization();

app.MapControllers();

// Configurar endpoints de Health Checks
ConfigureHealthCheckEndpoints(app);

// Mostrar información del ambiente al iniciar
app.Logger.LogInformation("Aplicación iniciada en ambiente: {Environment}", environment);
app.Logger.LogInformation("Swagger habilitado: {SwaggerEnabled}", enableSwagger);

await app.RunAsync();

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

    // Configurar UI de Health Checks (solo en Development y QA) - TEMPORALMENTE DESHABILITADO
    // if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase) ||
    //     environment.Equals("qa", StringComparison.OrdinalIgnoreCase))
    // {
    //     services.AddHealthChecksUI(options =>
    //     {
    //         options.SetEvaluationTimeInSeconds(30);
    //         options.MaximumHistoryEntriesPerEndpoint(50);
    //         options.AddHealthCheckEndpoint($"Portal Pacientes API - {environment}", "/health");
    //     });
    // }
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

    // Endpoint específico para aplicación
    app.MapHealthChecks("/health/application", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains(ApplicationTag),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Endpoint específico para configuración
    app.MapHealthChecks("/health/configuration", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains(ConfigurationTag),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Endpoint específico para servicios externos
    app.MapHealthChecks("/health/external-services", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains(ExternalServicesTag),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Endpoint simple para load balancers (solo healthy/unhealthy)
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains(ApplicationTag) || check.Tags.Contains(ConfigurationTag),
        AllowCachingResponses = false
    });

    // UI de Health Checks (solo en Development y QA) - TEMPORALMENTE DESHABILITADO
    // if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase) ||
    //     environment.Equals("qa", StringComparison.OrdinalIgnoreCase))
    // {
    //     app.MapHealthChecksUI(options =>
    //     {
    //         options.UIPath = "/health-ui";
    //         options.ApiPath = "/health-api";
    //     });
    // }
}
