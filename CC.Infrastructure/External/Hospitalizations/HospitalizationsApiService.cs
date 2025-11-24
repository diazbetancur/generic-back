using CC.Domain.Dtos.ExternalApis;
using CC.Domain.Interfaces.External;
using CC.Infrastructure.External.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CC.Infrastructure.External.Hospitalizations
{
    /// <summary>
    /// Servicio para integración con API externa de Hospitalizaciones
    /// </summary>
    public class HospitalizationsApiService : ExternalServiceBase<HospitalizationsApiOptions>, IHospitalizationsApiService
    {
        public HospitalizationsApiService(
            HttpClient httpClient,
            IOptions<HospitalizationsApiOptions> options,
            ILogger<HospitalizationsApiService> logger)
            : base(httpClient, options.Value, logger)
        {
            if (!Options.IsValid())
            {
                throw new InvalidOperationException(
                    $"Configuración inválida para HospitalizationsApiService: {Options.GetValidationErrors()}");
            }

            Logger.LogInformation("HospitalizationsApiService inicializado correctamente. BaseUrl={BaseUrl}", Options.BaseUrl);
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
            Logger.LogDebug("Verificando salud de API de Hospitalizaciones");
            return await GetAsync<ApiHealthDto>(Options.HealthEndpoint, ct);
        }

        public async Task<HospitalizationsResponseDto?> GetPatientHospitalizationsAsync(
            string patientId,
            CancellationToken ct = default)
        {
            Logger.LogInformation("Obteniendo hospitalizaciones del paciente: {PatientId}", patientId);

            var endpoint = Options.PatientHospitalizationsEndpoint.Replace("{patient_id}", patientId);
            return await GetAsync<HospitalizationsResponseDto>(endpoint, ct);
        }

        public async Task<HospitalizationsPaginatedResponseDto?> GetPatientHospitalizationsPaginatedAsync(
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
                "Obteniendo hospitalizaciones paginadas del paciente: {PatientId}, Limit={Limit}, Offset={Offset}",
                patientId, limit, offset);

            var endpoint = Options.PatientHospitalizationsPaginatedEndpoint.Replace("{patient_id}", patientId);

            var queryParams = new Dictionary<string, string?>
            {
                ["limit"] = limit.ToString(),
                ["offset"] = offset.ToString()
            };

            var url = BuildUrl(endpoint, queryParams);
            return await GetAsync<HospitalizationsPaginatedResponseDto>(url, ct);
        }

        public async Task<ApiVersionDto?> GetVersionAsync(CancellationToken ct = default)
        {
            Logger.LogDebug("Obteniendo versión de API de Hospitalizaciones");
            return await GetAsync<ApiVersionDto>(Options.VersionEndpoint, ct);
        }
    }
}
