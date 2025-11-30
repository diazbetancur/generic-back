using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;

namespace Api_Portar_Paciente.HealthChecks;

public class ApplicationHealthCheck : IHealthCheck
{
  private readonly IWebHostEnvironment _environment;
  private readonly IConfiguration _configuration;

  public ApplicationHealthCheck(IWebHostEnvironment environment, IConfiguration configuration)
  {
    _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
  }

  public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
  {
    try
    {
      var appInfo = new Dictionary<string, object>
      {
        ["Environment"] = _environment.EnvironmentName,
        ["ApplicationName"] = _environment.ApplicationName,
        ["ContentRoot"] = _environment.ContentRootPath,
        ["MachineName"] = Environment.MachineName,
        ["OSVersion"] = Environment.OSVersion.ToString(),
        ["ProcessorCount"] = Environment.ProcessorCount,
        ["WorkingSet"] = Environment.WorkingSet,
        ["TickCount"] = Environment.TickCount64,
        ["StartTime"] = DateTime.Now.AddMilliseconds(-Environment.TickCount64),
        ["Version"] = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown",
        ["TargetFramework"] = Assembly.GetExecutingAssembly().GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()?.FrameworkName ?? "Unknown"
      };

      // Información básica de memoria y rendimiento
      var process = System.Diagnostics.Process.GetCurrentProcess();
      appInfo["WorkingSet64"] = process.WorkingSet64;
      appInfo["PrivateMemorySize64"] = process.PrivateMemorySize64;
      appInfo["ProcessStartTime"] = process.StartTime;

      // Verificar features habilitadas
      var features = new Dictionary<string, object>
      {
        ["SwaggerEnabled"] = _configuration.GetValue<bool>("Features:EnableSwagger"),
        ["DetailedLoggingEnabled"] = _configuration.GetValue<bool>("Features:EnableDetailedLogging"),
        ["MetricsEnabled"] = _configuration.GetValue<bool>("Features:EnableMetrics")
      };

      appInfo["Features"] = features;

      var message = $"Aplicación ejecutándose correctamente en ambiente {_environment.EnvironmentName}";

      return Task.FromResult(HealthCheckResult.Healthy(message, appInfo));
    }
    catch (Exception ex)
    {
      return Task.FromResult(HealthCheckResult.Unhealthy($"Error verificando estado de la aplicación: {ex.Message}"));
    }
  }
}