using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api_Portar_Paciente.HealthChecks;

public class ConfigurationHealthCheck : IHealthCheck
{
  private readonly IConfiguration _configuration;

  public ConfigurationHealthCheck(IConfiguration configuration)
  {
    _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
  }

  public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
  {
    try
    {
      var issues = new List<string>();

      // Verificar configuraciones críticas
      var connectionString = _configuration.GetConnectionString("DefaultConnection");
      if (string.IsNullOrEmpty(connectionString))
      {
        issues.Add("ConnectionStrings:DefaultConnection no está configurado");
      }

      var jwtSecret = _configuration["Authentication:JwtSecret"];
      if (string.IsNullOrEmpty(jwtSecret))
      {
        issues.Add("Authentication:JwtSecret no está configurado");
      }
      else if (jwtSecret.Length < 32)
      {
        issues.Add("Authentication:JwtSecret es demasiado corto (mínimo 32 caracteres)");
      }

      var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
      if (string.IsNullOrEmpty(apiBaseUrl))
      {
        issues.Add("ApiSettings:BaseUrl no está configurado");
      }

      // Verificar configuraciones de servicios externos
      var emailServiceUrl = _configuration["ExternalServices:EmailService:BaseUrl"];
      var emailServiceKey = _configuration["ExternalServices:EmailService:ApiKey"];

      if (string.IsNullOrEmpty(emailServiceUrl))
      {
        issues.Add("ExternalServices:EmailService:BaseUrl no está configurado");
      }

      if (string.IsNullOrEmpty(emailServiceKey))
      {
        issues.Add("ExternalServices:EmailService:ApiKey no está configurado");
      }

      var notificationServiceUrl = _configuration["ExternalServices:NotificationService:BaseUrl"];
      var notificationServiceKey = _configuration["ExternalServices:NotificationService:ApiKey"];

      if (string.IsNullOrEmpty(notificationServiceUrl))
      {
        issues.Add("ExternalServices:NotificationService:BaseUrl no está configurado");
      }

      if (string.IsNullOrEmpty(notificationServiceKey))
      {
        issues.Add("ExternalServices:NotificationService:ApiKey no está configurado");
      }

      if (issues.Any())
      {
        return Task.FromResult(HealthCheckResult.Degraded($"Configuraciones faltantes o incorrectas: {string.Join(", ", issues)}"));
      }

      return Task.FromResult(HealthCheckResult.Healthy("Todas las configuraciones críticas están presentes"));
    }
    catch (Exception ex)
    {
      return Task.FromResult(HealthCheckResult.Unhealthy($"Error verificando configuraciones: {ex.Message}"));
    }
  }
}