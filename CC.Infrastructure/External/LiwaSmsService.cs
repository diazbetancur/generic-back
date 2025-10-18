using CC.Domain.Interfaces.External;
using CC.Infrastructure.External.Base;
using CC.Infrastructure.External.Sms;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CC.Infrastructure.External
{
    /// <summary>
    /// Servicio para envío de SMS a través de Liwa API con autenticación dinámica
    /// </summary>
    public class LiwaSmsService : ExternalServiceBase<SmsServiceOptions>, ILiwaSmsService
    {
        private readonly IMemoryCache _cache;
        private const string TokenCacheKey = "LiwaApiToken";

        public LiwaSmsService(
            HttpClient httpClient,
            IOptions<SmsServiceOptions> options,
            ILogger<LiwaSmsService> logger,
            IMemoryCache cache)
            : base(httpClient, options.Value, logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            if (!Options.IsValid())
            {
                throw new InvalidOperationException(
                    $"Configuración inválida para LiwaSmsService: {Options.GetValidationErrors()}");
            }

            Logger.LogInformation("LiwaSmsService inicializado correctamente");
        }

        protected override void ConfigureHttpClient()
        {
            base.ConfigureHttpClient();

            // Agregar API Key estática como header
            if (!string.IsNullOrWhiteSpace(Options.LiwaApiKey))
            {
                HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("api-key", Options.LiwaApiKey);
                Logger.LogDebug("API Key de Liwa configurada en headers");
            }
        }

        /// <summary>
        /// Envía un SMS individual
        /// </summary>
        public async Task<SmsResult> SendSmsAsync(
            string phoneNumber,
            string message,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Número de teléfono es requerido", nameof(phoneNumber));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Mensaje es requerido", nameof(message));

            Logger.LogInformation(
                "Enviando SMS a {PhoneNumber}, Longitud mensaje: {Length}",
                MaskPhoneNumber(phoneNumber), message.Length);

            try
            {
                // Obtener token de autenticación
                var token = await GetAuthTokenAsync(cancellationToken);
                if (string.IsNullOrEmpty(token))
                {
                    Logger.LogError("No se pudo obtener token de autenticación de Liwa");
                    return new SmsResult(false, null, null, "Error de autenticación");
                }

                // Formatear número (agregar código de país si no lo tiene)
                var formattedNumber = FormatPhoneNumber(phoneNumber);

                // Crear request
                var request = new LiwaSendSingleSmsRequest
                {
                    Number = formattedNumber,
                    Message = message,
                    Type = Options.DefaultMessageType
                };

                // Enviar SMS con token Bearer
                var response = await SendAuthenticatedRequestAsync<LiwaSendSingleSmsRequest, LiwaSendSmsResponse>(
                    HttpMethod.Post,
                    Options.SendSingleEndpoint,
                    request,
                    token,
                    cancellationToken);

                if (response == null)
                {
                    Logger.LogWarning("Respuesta nula al enviar SMS a {PhoneNumber}", MaskPhoneNumber(phoneNumber));
                    return new SmsResult(false, null, null, "Sin respuesta del servidor");
                }

                if (response.Success)
                {
                    Logger.LogInformation(
                        "SMS enviado exitosamente a {PhoneNumber}, MessageId: {MessageId}, Status: {Status}",
                        MaskPhoneNumber(phoneNumber), response.MessageId, response.Status);

                    return new SmsResult(true, response.MessageId, response.Status);
                }
                else
                {
                    Logger.LogWarning(
                        "Error al enviar SMS a {PhoneNumber}: {Error}",
                        MaskPhoneNumber(phoneNumber), response.Message);

                    return new SmsResult(false, null, response.Status, response.Message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Excepción al enviar SMS a {PhoneNumber}", MaskPhoneNumber(phoneNumber));
                return new SmsResult(false, null, null, ex.Message);
            }
        }

        /// <summary>
        /// Envía múltiples SMS (campaña)
        /// </summary>
        public async Task<SmsBulkResult> SendBulkSmsAsync(
            string campaignName,
            Dictionary<string, string> messages,
            DateTime? scheduledDate = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(campaignName))
                throw new ArgumentException("Nombre de campaña es requerido", nameof(campaignName));

            if (messages == null || !messages.Any())
                throw new ArgumentException("Debe haber al menos un mensaje", nameof(messages));

            Logger.LogInformation(
                "Enviando campaña SMS '{CampaignName}' con {Count} mensajes",
                campaignName, messages.Count);

            try
            {
                // Obtener token de autenticación
                var token = await GetAuthTokenAsync(cancellationToken);
                if (string.IsNullOrEmpty(token))
                {
                    Logger.LogError("No se pudo obtener token de autenticación de Liwa");
                    return new SmsBulkResult(false, null, 0, "Error de autenticación");
                }

                // Construir lista de mensajes
                var smsMessages = messages.Select(kvp => new LiwaSmsMessage
                {
                    CodeCountry = Options.DefaultCountryCode,
                    Number = CleanPhoneNumber(kvp.Key),
                    Message = kvp.Value,
                    Type = Options.DefaultMessageType
                }).ToList();

                // Formatear fecha si se proporciona
                var sendingDate = scheduledDate?.ToString("yyyy-MM-dd HH:mm:ss");

                var request = new LiwaSendMultipleSmsRequest
                {
                    Name = campaignName,
                    SendingDate = sendingDate,
                    Messages = smsMessages
                };

                // Enviar campaña
                var response = await SendAuthenticatedRequestAsync<LiwaSendMultipleSmsRequest, LiwaSendMultipleSmsResponse>(
                    HttpMethod.Post,
                    Options.SendMultipleEndpoint,
                    request,
                    token,
                    cancellationToken);

                if (response == null)
                {
                    Logger.LogWarning("Respuesta nula al enviar campaña '{CampaignName}'", campaignName);
                    return new SmsBulkResult(false, null, 0, "Sin respuesta del servidor");
                }

                if (response.Success)
                {
                    Logger.LogInformation(
                        "Campaña '{CampaignName}' enviada exitosamente. CampaignId: {CampaignId}, Total: {Total}",
                        campaignName, response.CampaignId, response.TotalSent);

                    return new SmsBulkResult(true, response.CampaignId, response.TotalSent);
                }
                else
                {
                    Logger.LogWarning(
                        "Error al enviar campaña '{CampaignName}': {Error}",
                        campaignName, response.Message);

                    return new SmsBulkResult(false, null, 0, response.Message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Excepción al enviar campaña '{CampaignName}'", campaignName);
                return new SmsBulkResult(false, null, 0, ex.Message);
            }
        }

        /// <summary>
        /// Verifica disponibilidad del servicio autenticándose
        /// </summary>
        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var token = await GetAuthTokenAsync(cancellationToken);
                return !string.IsNullOrEmpty(token);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al verificar disponibilidad de LiwaSmsService");
                return false;
            }
        }

        /// <summary>
        /// Obtiene el token de autenticación (con cache)
        /// </summary>
        private async Task<string?> GetAuthTokenAsync(CancellationToken cancellationToken)
        {
            // Intentar obtener de cache
            if (_cache.TryGetValue<string>(TokenCacheKey, out var cachedToken))
            {
                Logger.LogDebug("Token de Liwa obtenido de cache");
                return cachedToken;
            }

            Logger.LogInformation("Obteniendo nuevo token de autenticación de Liwa");

            try
            {
                var authRequest = new LiwaAuthRequest
                {
                    Account = Options.Account,
                    Password = Options.Password
                };

                // NO usar SendAuthenticatedRequestAsync aquí (no tenemos token aún)
                var response = await PostAsync<LiwaAuthRequest, LiwaAuthResponse>(
                    Options.AuthEndpoint,
                    authRequest,
                    cancellationToken);

                if (!string.IsNullOrEmpty(response?.Token))
                {
                    // Guardar en cache
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Options.TokenExpirationMinutes)
                    };

                    _cache.Set(TokenCacheKey, response.Token, cacheOptions);

                    Logger.LogInformation(
                        "Token de Liwa obtenido exitosamente. Expira en {Minutes} minutos",
                        Options.TokenExpirationMinutes);

                    return response.Token;
                }
                else
                {
                    Logger.LogError(
                        "Error al autenticar con Liwa: {Message}",
                        response?.Message ?? "Respuesta vacía");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Excepción al obtener token de Liwa");
                return null;
            }
        }

        /// <summary>
        /// Envía request con autenticación Bearer
        /// </summary>
        private async Task<TResponse?> SendAuthenticatedRequestAsync<TRequest, TResponse>(
            HttpMethod method,
            string endpoint,
            TRequest requestData,
            string bearerToken,
            CancellationToken cancellationToken)
            where TResponse : class
        {
            try
            {
                using var request = new HttpRequestMessage(method, endpoint);

                // Agregar Bearer token
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                // Agregar contenido si es POST/PUT
                if (method == HttpMethod.Post || method == HttpMethod.Put)
                {
                    request.Content = JsonContent.Create(requestData, options: JsonOptions);
                }

                Logger.LogDebug(
                    "Enviando {Method} request autenticado a {Endpoint}",
                    method.Method, endpoint);

                var startTime = DateTime.UtcNow;
                using var response = await HttpClient.SendAsync(request, cancellationToken);
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    Logger.LogWarning(
                        "Request autenticado fallido: {Method} {Endpoint} - Status: {StatusCode}, Duration: {Duration}ms, Error: {Error}",
                        method.Method, endpoint, (int)response.StatusCode, duration, errorContent);

                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);

                Logger.LogInformation(
                    "Request autenticado exitoso: {Method} {Endpoint} - Status: {StatusCode}, Duration: {Duration}ms",
                    method.Method, endpoint, (int)response.StatusCode, duration);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error en request autenticado: {Method} {Endpoint}", method.Method, endpoint);
                return null;
            }
        }

        /// <summary>
        /// Formatea número de teléfono con código de país
        /// </summary>
        private string FormatPhoneNumber(string phoneNumber)
        {
            var cleaned = CleanPhoneNumber(phoneNumber);

            // Si no tiene código de país, agregarlo
            if (!cleaned.StartsWith(Options.DefaultCountryCode))
            {
                return Options.DefaultCountryCode + cleaned;
            }

            return cleaned;
        }

        /// <summary>
        /// Limpia número de teléfono (solo dígitos)
        /// </summary>
        private static string CleanPhoneNumber(string phoneNumber)
        {
            return new string(phoneNumber.Where(char.IsDigit).ToArray());
        }

        /// <summary>
        /// Enmascara número de teléfono para logging
        /// </summary>
        private static string MaskPhoneNumber(string phoneNumber)
        {
            if (phoneNumber.Length <= 4)
                return "****";

            return phoneNumber[..^4] + "****";
        }
    }
}