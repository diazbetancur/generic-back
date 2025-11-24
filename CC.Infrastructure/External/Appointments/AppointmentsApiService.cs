using CC.Domain.Dtos.ExternalApis;
using CC.Domain.Interfaces.External;
using CC.Infrastructure.External.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CC.Infrastructure.External.Appointments
{
    /// <summary>
    /// Servicio para integración con API externa de Citas Médicas
    /// </summary>
    public class AppointmentsApiService : ExternalServiceBase<AppointmentsApiOptions>, IAppointmentsApiService
    {
        public AppointmentsApiService(
            HttpClient httpClient,
            IOptions<AppointmentsApiOptions> options,
            ILogger<AppointmentsApiService> logger)
            : base(httpClient, options.Value, logger)
        {
            if (!Options.IsValid())
            {
                throw new InvalidOperationException(
                    $"Configuración inválida para AppointmentsApiService: {Options.GetValidationErrors()}");
            }

            Logger.LogInformation("AppointmentsApiService inicializado correctamente. BaseUrl={BaseUrl}", Options.BaseUrl);
        }

        /// <summary>
        /// Configura el HttpClient con el header X-API-Key específico de esta API
        /// </summary>
        protected override void ConfigureHttpClient()
        {
            base.ConfigureHttpClient();

            // Esta API usa X-API-Key en lugar de X-Api-Key
            if (!string.IsNullOrWhiteSpace(Options.ApiKey))
            {
                HttpClient.DefaultRequestHeaders.Remove("X-Api-Key");
                HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-API-Key", Options.ApiKey);
            }
        }

        public async Task<ApiHealthDto?> CheckHealthAsync(CancellationToken ct = default)
        {
            Logger.LogDebug("Verificando salud de API de Citas");
            return await GetAsync<ApiHealthDto>(Options.HealthEndpoint, ct);
        }

        public async Task<AppointmentsResponseDto?> GetPatientAppointmentsAsync(
            string patientId,
            CancellationToken ct = default)
        {
            Logger.LogInformation("Obteniendo citas del paciente: {PatientId}", patientId);

            var endpoint = Options.PatientAppointmentsEndpoint.Replace("{patient_id}", patientId);
            return await GetAsync<AppointmentsResponseDto>(endpoint, ct);
        }

        public async Task<AppointmentsPaginatedResponseDto?> GetPatientAppointmentsPaginatedAsync(
            string patientId,
            int limit = 50,
            int offset = 0,
            CancellationToken ct = default)
        {
            // Validar límites
            if (limit <= 0) limit = Options.DefaultLimit;
            if (limit > Options.MaxLimit) limit = Options.MaxLimit;
            if (offset < 0) offset = 0;

            Logger.LogInformation(
                "Obteniendo citas paginadas del paciente: {PatientId}, Limit={Limit}, Offset={Offset}",
                patientId, limit, offset);

            var endpoint = Options.PatientAppointmentsPaginatedEndpoint.Replace("{patient_id}", patientId);
            
            var queryParams = new Dictionary<string, string?>
            {
                ["limit"] = limit.ToString(),
                ["offset"] = offset.ToString()
            };

            var url = BuildUrl(endpoint, queryParams);
            return await GetAsync<AppointmentsPaginatedResponseDto>(url, ct);
        }

        public async Task<ApiVersionDto?> GetVersionAsync(CancellationToken ct = default)
        {
            Logger.LogDebug("Obteniendo versión de API de Citas");
            return await GetAsync<ApiVersionDto>(Options.VersionEndpoint, ct);
        }
    }
}
