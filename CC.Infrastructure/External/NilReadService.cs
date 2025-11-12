using CC.Domain.Contracts;
using CC.Domain.Interfaces.External;
using CC.Infrastructure.External.Base;
using CC.Infrastructure.External.NilRead;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CC.Infrastructure.External
{
    /// <summary>
    /// Servicio para integración con NilRead (Informes PDF e Imágenes Diagnósticas)
    /// </summary>
    public class NilReadService : ExternalServiceBase<NilReadOptions>, INilReadService
    {
        public NilReadService(
            HttpClient httpClient,
            IOptions<NilReadOptions> options,
            ILogger<NilReadService> logger)
            : base(httpClient, options.Value, logger)
        {
            if (!Options.IsValid())
            {
                throw new InvalidOperationException(
                    $"Configuración inválida para NilReadService: {Options.GetValidationErrors()}");
            }

            Logger.LogInformation("NilReadService inicializado correctamente");
        }

        protected override void ConfigureHttpClient()
        {
            base.ConfigureHttpClient();

            // API Key como header X-API-Key
            if (!string.IsNullOrWhiteSpace(Options.ApiKey))
            {
                try
                {
                    HttpClient.DefaultRequestHeaders.Remove("X-API-Key");
                    HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-API-Key", Options.ApiKey);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "No fue posible establecer header X-API-Key");
                }
            }

            if (!HttpClient.DefaultRequestHeaders.Accept.Any())
                HttpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        }

        public async Task<NilReadExamsResult> GetPatientExamsAsync(
            string patientId,
            int limit = 10,
            int offset = 0,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                throw new ArgumentException("Patient ID es requerido", nameof(patientId));

            if (limit < 1 || limit > Options.MaxLimit)
                limit = Options.DefaultLimit;

            if (offset < 0)
                offset = 0;

            Logger.LogInformation(
                "Obteniendo exámenes de paciente {PatientId}: limit={Limit}, offset={Offset}",
                patientId, limit, offset);

            try
            {
                var endpoint = Options.ExamsEndpoint.Replace("{patient_id}", patientId);
                var queryParams = new Dictionary<string, string?>
                {
                    ["limit"] = limit.ToString(),
                    ["offset"] = offset.ToString()
                };

                var url = BuildUrl(endpoint, queryParams);
                var response = await GetAsync<NilReadExamsResponse>(url, cancellationToken);

                if (response == null)
                {
                    Logger.LogWarning("Respuesta nula al obtener exámenes de paciente {PatientId}", patientId);
                    return new NilReadExamsResult(
                        false, patientId, offset, limit, 0, 0, null, null,
                        "Sin respuesta del servidor NilRead");
                }

                var exams = response.Exams?.Select(e => new NilReadExamInfo(
                    e.Accession,
                    e.Description,
                    e.DateTime,
                    e.ReportAvailable,
                    e.ReportUrl,
                    e.ImagesAvailable
                )).ToList();

                int? nextOffset = (response.Offset + response.Count < response.Total)
                    ? response.Offset + response.Count
                    : null;

                Logger.LogInformation(
                    "Exámenes obtenidos: {Count} de {Total} para paciente {PatientId}",
                    response.Count, response.Total, patientId);

                return new NilReadExamsResult(
                    true, response.PatientId, response.Offset, response.Limit,
                    response.Count, response.Total, nextOffset, exams, null);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al obtener exámenes de paciente {PatientId}", patientId);
                return new NilReadExamsResult(
                    false, patientId, offset, limit, 0, 0, null, null,
                    $"Error: {ex.Message}");
            }
        }

        public async Task<NilReadReportResult> GetReportPdfAsync(
            string dateFolder,
            string filename,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dateFolder))
                throw new ArgumentException("dateFolder es requerido", nameof(dateFolder));

            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("filename es requerido", nameof(filename));

            Logger.LogInformation("Obteniendo informe PDF: {DateFolder}/{Filename}", dateFolder, filename);

            try
            {
                var endpoint = Options.ReportEndpoint
                    .Replace("{dateFolder}", dateFolder)
                    .Replace("{filename}", filename);

                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                using var response = await HttpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var statusCode = (int)response.StatusCode;
                    Logger.LogWarning(
                        "Error al obtener informe: {DateFolder}/{Filename} - Status: {StatusCode}",
                        dateFolder, filename, statusCode);

                    if (statusCode == 404)
                    {
                        return new NilReadReportResult(
                            false, null, null,
                            "Informe no encontrado");
                    }

                    return new NilReadReportResult(
                        false, null, null,
                        $"Error HTTP {statusCode}");
                }

                var pdfBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                Logger.LogInformation(
                    "Informe PDF obtenido: {DateFolder}/{Filename}, Tamaño: {Size} bytes",
                    dateFolder, filename, pdfBytes.Length);

                return new NilReadReportResult(
                    true, pdfBytes, filename, null);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al obtener informe PDF: {DateFolder}/{Filename}", dateFolder, filename);
                return new NilReadReportResult(
                    false, null, null,
                    $"Error: {ex.Message}");
            }
        }

        public async Task<NilReadViewerLinkResult> GenerateViewerLinkAsync(
            string accession,
            string patientId,
            string? dateTime = null,
            List<string>? accNumbers = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(accession))
                throw new ArgumentException("accession es requerido", nameof(accession));

            if (string.IsNullOrWhiteSpace(patientId))
                throw new ArgumentException("patientId es requerido", nameof(patientId));

            Logger.LogInformation(
                "Generando enlace del visor para accession {Accession}, paciente {PatientId}",
                accession, patientId);

            try
            {
                var endpoint = Options.ViewerLinkEndpoint.Replace("{accession}", accession);

                var request = new NilReadViewerLinkRequest
                {
                    PatientID = patientId,
                    DateTime = dateTime,
                    AccNumbers = accNumbers ?? new List<string> { accession }
                };

                var response = await PostAsync<NilReadViewerLinkRequest, NilReadViewerLinkResponse>(
                    endpoint, request, cancellationToken);

                if (response == null)
                {
                    Logger.LogWarning("Respuesta nula al generar enlace para {Accession}", accession);
                    return new NilReadViewerLinkResult(
                        false, null, null, null, null, null, null, null, null,
                        "Sin respuesta del servidor NilRead");
                }

                Logger.LogInformation(
                    "Enlace del visor generado: {Accession}, URL: {Url}",
                    accession, response.ViewerUrl);

                return new NilReadViewerLinkResult(
                    true,
                    response.Accession,
                    response.PatientId,
                    response.DateTime,
                    response.Provider,
                    response.AccNumbersUsed,
                    response.ViewerUrl,
                    response.Result?.Code,
                    response.Result?.Description,
                    null);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al generar enlace del visor para {Accession}", accession);
                return new NilReadViewerLinkResult(
                    false, null, null, null, null, null, null, null, null,
                    $"Error: {ex.Message}");
            }
        }

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogDebug("Verificando disponibilidad de NilRead");

                var response = await GetAsync<NilReadHealthResponse>(
                    Options.HealthEndpoint,
                    cancellationToken);

                var isAvailable = response?.Status?.ToLowerInvariant() == "ok";

                Logger.LogInformation("NilReadService disponible: {Available}", isAvailable);

                return isAvailable;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al verificar disponibilidad de NilReadService");
                return false;
            }
        }
    }
}