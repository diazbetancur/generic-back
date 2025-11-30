using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.NetworkInformation;

namespace Api_Portar_Paciente.HealthChecks;

public class ExternalServiceHealthCheck : IHealthCheck
{
  private readonly HttpClient _httpClient;
  private readonly string _serviceUrl;
  private readonly string _serviceName;

  public ExternalServiceHealthCheck(HttpClient httpClient, string serviceUrl, string serviceName)
  {
    _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    _serviceUrl = serviceUrl ?? throw new ArgumentNullException(nameof(serviceUrl));
    _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
  }

  public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
  {
    try
    {
      // Verificar si la URL es válida
      if (!Uri.TryCreate(_serviceUrl, UriKind.Absolute, out var uri))
      {
        return HealthCheckResult.Unhealthy($"URL inválida para {_serviceName}: {_serviceUrl}");
      }

      // Si es localhost o una URL de desarrollo, hacer ping en lugar de HTTP request
      if (uri.Host.Contains("localhost") || uri.Host.Contains("127.0.0.1"))
      {
        var ping = new Ping();
        var reply = await ping.SendPingAsync(uri.Host, 5000);

        if (reply.Status == IPStatus.Success)
        {
          return HealthCheckResult.Healthy($"{_serviceName} está disponible (ping exitoso)");
        }
        else
        {
          return HealthCheckResult.Degraded($"{_serviceName} no responde al ping: {reply.Status}");
        }
      }

      // Para URLs externas, hacer un HTTP request
      using var response = await _httpClient.GetAsync(_serviceUrl, cancellationToken);

      if (response.IsSuccessStatusCode)
      {
        return HealthCheckResult.Healthy($"{_serviceName} está disponible - Status: {response.StatusCode}");
      }
      else
      {
        return HealthCheckResult.Degraded($"{_serviceName} respondió con código: {response.StatusCode}");
      }
    }
    catch (TaskCanceledException)
    {
      return HealthCheckResult.Unhealthy($"{_serviceName} no respondió en el tiempo esperado (timeout)");
    }
    catch (HttpRequestException ex)
    {
      return HealthCheckResult.Unhealthy($"{_serviceName} no está disponible: {ex.Message}");
    }
    catch (Exception ex)
    {
      return HealthCheckResult.Unhealthy($"Error inesperado verificando {_serviceName}: {ex.Message}");
    }
  }
}