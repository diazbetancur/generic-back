using CC.Domain.Dtos.ExternalApis;
using CC.Domain.Interfaces.External;
using CC.Infrastructure.External.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CC.Infrastructure.External.ClinicalHistory
{
    /// <summary>
    /// Servicio para integración con API externa de Historia Clínica
    /// </summary>
    public class ClinicalHistoryApiService : ExternalServiceBase<ClinicalHistoryApiOptions>, IClinicalHistoryApiService
    {
        public ClinicalHistoryApiService(
            HttpClient httpClient,
            IOptions<ClinicalHistoryApiOptions> options,
            ILogger<ClinicalHistoryApiService> logger)
            : base(httpClient, options.Value, logger)
        {
            if (!Options.IsValid())
            {
                throw new InvalidOperationException(
                    $"Configuración inválida para ClinicalHistoryApiService: {Options.GetValidationErrors()}");
            }

            Logger.LogInformation("ClinicalHistoryApiService inicializado correctamente. BaseUrl={BaseUrl}", Options.BaseUrl);
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
            Logger.LogDebug("Verificando salud de API de Historia Clínica");
            return await GetAsync<ApiHealthDto>(Options.HealthEndpoint, ct);
        }

        public async Task<EpisodesResponseDto?> GetPatientEpisodesAsync(
            string patientId,
            bool onlyPdf = false,
            int? limit = null,
            int? offset = null,
            CancellationToken ct = default)
        {
            Logger.LogInformation(
                "Obteniendo episodios del paciente: {PatientId}, OnlyPdf={OnlyPdf}",
                patientId, onlyPdf);

            var endpoint = Options.PatientEpisodesEndpoint.Replace("{patient_id}", patientId);

            var queryParams = new Dictionary<string, string?>();

            if (onlyPdf)
                queryParams["only_pdf"] = "true";

            if (limit.HasValue)
                queryParams["limit"] = limit.Value.ToString();

            if (offset.HasValue)
                queryParams["offset"] = offset.Value.ToString();

            var url = queryParams.Any() ? BuildUrl(endpoint, queryParams) : endpoint;
            
            return await GetAsync<EpisodesResponseDto>(url, ct);
        }

        public async Task<Stream?> DownloadClinicalHistoryPdfAsync(
            string patientId,
            string episode,
            CancellationToken ct = default)
        {
            Logger.LogInformation(
                "Descargando PDF de historia clínica: PatientId={PatientId}, Episode={Episode}",
                patientId, episode);

            var endpoint = Options.ClinicalHistoryPdfEndpoint
                .Replace("{patient_id}", patientId)
                .Replace("{episode}", episode);

            return await DownloadFileAsync(endpoint, ct);
        }

        public async Task<Stream?> DownloadIncapacityPdfAsync(
            string patientId,
            string episode,
            CancellationToken ct = default)
        {
            Logger.LogInformation(
                "Descargando PDF de incapacidad: PatientId={PatientId}, Episode={Episode}",
                patientId, episode);

            var endpoint = Options.IncapacityPdfEndpoint
                .Replace("{patient_id}", patientId)
                .Replace("{episode}", episode);

            return await DownloadFileAsync(endpoint, ct);
        }

        public async Task<ApiVersionDto?> GetVersionAsync(CancellationToken ct = default)
        {
            Logger.LogDebug("Obteniendo versión de API de Historia Clínica");
            return await GetAsync<ApiVersionDto>(Options.VersionEndpoint, ct);
        }

        /// <summary>
        /// Descarga un archivo PDF como Stream
        /// </summary>
        private async Task<Stream?> DownloadFileAsync(string endpoint, CancellationToken ct)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                Logger.LogDebug("Descargando archivo desde: {Endpoint}", endpoint);

                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                using var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct)
                    .ConfigureAwait(false);

                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                if (!response.IsSuccessStatusCode)
                {
                    Logger.LogWarning(
                        "Descarga fallida: {Endpoint} - Status: {StatusCode}, Duration: {Duration}ms",
                        endpoint, (int)response.StatusCode, duration);
                    return null;
                }

                // Copiar el contenido a un MemoryStream antes de que se dispose el response
                var memoryStream = new MemoryStream();
                await response.Content.CopyToAsync(memoryStream, ct).ConfigureAwait(false);
                
                // Resetear la posición del stream al inicio
                memoryStream.Position = 0;

                Logger.LogInformation(
                    "Descarga exitosa: {Endpoint} - Status: {StatusCode}, Duration: {Duration}ms, Size: {Size} bytes",
                    endpoint, (int)response.StatusCode, duration, memoryStream.Length);

                return memoryStream;
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                Logger.LogError(ex,
                    "Error al descargar archivo: {Endpoint}, Duration: {Duration}ms",
                    endpoint, duration);
                return null;
            }
        }
    }
}
