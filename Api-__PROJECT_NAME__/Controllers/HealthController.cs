using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace Api_Portar_Paciente.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
  private readonly HealthCheckService _healthCheckService;
  private readonly IWebHostEnvironment _environment;
  private readonly IConfiguration _configuration;

  public HealthController(
      HealthCheckService healthCheckService,
      IWebHostEnvironment environment,
      IConfiguration configuration)
  {
    _healthCheckService = healthCheckService;
    _environment = environment;
    _configuration = configuration;
  }

  /// <summary>
  /// Obtiene el estado detallado de salud de la aplicación
  /// </summary>
  [HttpGet]
  public async Task<IActionResult> Get()
  {
    var healthReport = await _healthCheckService.CheckHealthAsync();

    var response = new
    {
      Status = healthReport.Status.ToString(),
      Environment = _environment.EnvironmentName,
      Timestamp = DateTime.UtcNow,
      TotalDuration = healthReport.TotalDuration,
      Checks = healthReport.Entries.Select(entry => new
      {
        Name = entry.Key,
        Status = entry.Value.Status.ToString(),
        Duration = entry.Value.Duration,
        Description = entry.Value.Description,
        Tags = entry.Value.Tags,
        Data = entry.Value.Data,
        Exception = entry.Value.Exception?.Message
      })
    };

    var statusCode = healthReport.Status switch
    {
      HealthStatus.Healthy => 200,
      HealthStatus.Degraded => 200,
      HealthStatus.Unhealthy => 503,
      _ => 500
    };

    return StatusCode(statusCode, response);
  }

  /// <summary>
  /// Obtiene un resumen simple del estado de salud (para load balancers)
  /// </summary>
  [HttpGet("simple")]
  public async Task<IActionResult> GetSimple()
  {
    var healthReport = await _healthCheckService.CheckHealthAsync();

    var response = new
    {
      Status = healthReport.Status.ToString(),
      Timestamp = DateTime.UtcNow
    };

    var statusCode = healthReport.Status == HealthStatus.Unhealthy ? 503 : 200;
    return StatusCode(statusCode, response);
  }

  /// <summary>
  /// Obtiene información del ambiente y configuración
  /// </summary>
  [HttpGet("info")]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
  public IActionResult GetInfo()
  {
    var response = new
    {
      Environment = _environment.EnvironmentName,
      ApplicationName = _environment.ApplicationName,
      ContentRoot = _environment.ContentRootPath,
      MachineName = Environment.MachineName,
      OSVersion = Environment.OSVersion.ToString(),
      ProcessorCount = Environment.ProcessorCount,
      Features = new
      {
        SwaggerEnabled = _configuration.GetValue<bool>("Features:EnableSwagger"),
        DetailedLoggingEnabled = _configuration.GetValue<bool>("Features:EnableDetailedLogging"),
        MetricsEnabled = _configuration.GetValue<bool>("Features:EnableMetrics"),
        HealthChecksEnabled = _configuration.GetValue<bool>("HealthChecks:Enabled"),
        HealthChecksUIEnabled = _configuration.GetValue<bool>("HealthChecks:UIEnabled")
      },
      Timestamp = DateTime.UtcNow
    };

    return Ok(response);
  }

  /// <summary>
  /// Endpoint específico para verificar conectividad (ping)
  /// </summary>
  [HttpGet("ping")]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
  public IActionResult Ping()
  {
    return Ok(new
    {
      Message = "Pong",
      Environment = _environment.EnvironmentName,
      Timestamp = DateTime.UtcNow
    });
  }
}