using CC.Domain.Interfaces.External;
using CC.Infrastructure.External.Base;
using CC.Infrastructure.External.Xero;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace CC.Infrastructure.External
{
    /// <summary>
    /// Servicio para integración con Xero Viewer (Visor de Imágenes Diagnósticas)
    /// </summary>
    public class XeroViewerService : ExternalServiceBase<XeroViewerOptions>, IXeroViewerService
    {
        public XeroViewerService(
            HttpClient httpClient,
            IOptions<XeroViewerOptions> options,
            ILogger<XeroViewerService> logger)
            : base(httpClient, options.Value, logger)
        {
            if (!Options.IsValid())
            {
                throw new InvalidOperationException(
                    $"Configuración inválida para XeroViewerService: {Options.GetValidationErrors()}");
            }

            Logger.LogInformation("XeroViewerService inicializado correctamente");
        }

        protected override void ConfigureHttpClient()
        {
            base.ConfigureHttpClient();

            // Agregar API Key como header X-API-Key (evitar duplicados)
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

        /// <summary>
        /// Obtiene la lista de estudios de un paciente con paginación
        /// </summary>
        public async Task<StudiesResult> GetPatientStudiesAsync(
            string patientId,
            int limit = 10,
            int offset = 0,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                throw new ArgumentException("Patient ID es requerido", nameof(patientId));

            // Validar límites
            if (limit < 1 || limit > Options.MaxLimit)
                limit = Options.DefaultLimit;

            if (offset < 0)
                offset = 0;

            Logger.LogInformation(
                "Obteniendo estudios de paciente {PatientId}: limit={Limit}, offset={Offset}",
                patientId, limit, offset);

            try
            {
                // Construir URL
                var endpoint = Options.StudiesEndpoint.Replace("{patient_id}", patientId);
                var queryParams = new Dictionary<string, string?>
                {
                    ["limit"] = limit.ToString(),
                    ["offset"] = offset.ToString()
                };

                var url = BuildUrl(endpoint, queryParams);

                var response = await GetAsync<XeroStudiesResponse>(url, cancellationToken);

                if (response == null)
                {
                    Logger.LogWarning("Respuesta nula al obtener estudios de paciente {PatientId} reespuesta {response}", patientId, response);
                    return new StudiesResult(
                        false, patientId, offset, limit, 0, 0, null, null,
                        "Sin respuesta del servidor Xero");
                }

                Logger.LogInformation(
                    "Estudios obtenidos: {Count} de {Total} para paciente {PatientId}",
                    response.Count, response.Total, patientId);

                return new StudiesResult(
                    Success: true,
                    PatientId: response.PatientId,
                    Offset: response.Offset,
                    Limit: response.Limit,
                    Count: response.Count,
                    Total: response.Total,
                    NextOffset: response.NextOffset,
                    Studies: response.Studies,
                    ErrorMessage: null
                );
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex, "Error de red al obtener estudios de paciente {PatientId}", patientId);
                return new StudiesResult(
                    false, patientId, offset, limit, 0, 0, null, null,
                    $"Error de conexión con Xero: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Excepción al obtener estudios de paciente {PatientId}", patientId);
                return new StudiesResult(
                    false, patientId, offset, limit, 0, 0, null, null,
                    ex.Message);
            }
        }

        /// <summary>
        /// Genera un enlace temporal para visualizar un estudio en el visor
        /// </summary>
        public async Task<ViewerLinkResult> GenerateViewerLinkAsync(
            string studyUid,
            string? patientId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(studyUid))
                throw new ArgumentException("Study UID es requerido", nameof(studyUid));

            Logger.LogInformation(
                "Generando enlace del visor para estudio {StudyUid}, paciente {PatientId}",
                studyUid, patientId ?? "N/A");

            try
            {
                // Construir URL
                var endpoint = Options.ViewerLinkEndpoint.Replace("{study_uid}", studyUid);

                // Crear request body
                var request = new XeroViewerLinkRequest
                {
                    PatientId = patientId
                };

                // Realizar petición POST directa
                var response = await PostAsync<XeroViewerLinkRequest, XeroViewerLinkResponse>(
                    endpoint,
                    request,
                    cancellationToken);

                if (response == null)
                {
                    Logger.LogWarning("Respuesta nula al generar enlace para estudio {StudyUid}", studyUid);
                    return new ViewerLinkResult(
                        false, null, null, null,
                        "Sin respuesta del servidor Xero");
                }

                // Parsear fecha de expiración
                DateTime? expiresAt = null;
                if (!string.IsNullOrEmpty(response.ExpiresAt))
                {
                    if (DateTime.TryParse(response.ExpiresAt, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
                    {
                        expiresAt = parsed;
                    }
                }

                Logger.LogInformation(
                    "Enlace del visor generado exitosamente para estudio {StudyUid}, expira: {ExpiresAt}",
                    studyUid, expiresAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A");

                return new ViewerLinkResult(
                    Success: true,
                    Token: response.Token,
                    ViewerUrl: response.ViewerUrl,
                    ExpiresAt: expiresAt,
                    ErrorMessage: null
                );
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex, "Error de red al generar enlace para estudio {StudyUid}", studyUid);
                return new ViewerLinkResult(
                    false, null, null, null,
                    $"Error de conexión con Xero: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Excepción al generar enlace para estudio {StudyUid}", studyUid);
                return new ViewerLinkResult(
                    false, null, null, null,
                    ex.Message);
            }
        }

        /// <summary>
        /// Verifica disponibilidad del servicio
        /// </summary>
        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogDebug("Verificando disponibilidad de Xero Viewer");

                var response = await GetAsync<XeroHealthResponse>(
                    Options.HealthEndpoint,
                    cancellationToken);

                var isAvailable = response?.Status == "ok";

                Logger.LogInformation(
                    "XeroViewerService disponible: {Available}, Server: {Server}",
                    isAvailable, response?.Server ?? "N/A");

                return isAvailable;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al verificar disponibilidad de XeroViewerService");
                return false;
            }
        }
    }
}