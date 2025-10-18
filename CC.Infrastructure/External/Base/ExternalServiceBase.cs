using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace CC.Infrastructure.External.Base
{
    /// <summary>
    /// Clase base abstracta para servicios externos con funcionalidades comunes
    /// </summary>
    /// <typeparam name="TOptions">Tipo de opciones de configuración del servicio</typeparam>
    public abstract class ExternalServiceBase<TOptions> where TOptions : ExternalServiceOptions
    {
        protected readonly HttpClient HttpClient;
        protected readonly TOptions Options;
        protected readonly ILogger Logger;
        protected readonly JsonSerializerOptions JsonOptions;

        protected ExternalServiceBase(
            HttpClient httpClient,
            TOptions options,
            ILogger logger)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Configurar HttpClient
            ConfigureHttpClient();

            // Opciones JSON por defecto
            JsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
        }

        /// <summary>
        /// Configura el HttpClient con base URL, timeout y headers comunes
        /// </summary>
        protected virtual void ConfigureHttpClient()
        {
            if (!string.IsNullOrWhiteSpace(Options.BaseUrl))
            {
                HttpClient.BaseAddress = new Uri(Options.BaseUrl.TrimEnd('/'));
            }

            if (Options.TimeoutSeconds > 0)
            {
                HttpClient.Timeout = TimeSpan.FromSeconds(Options.TimeoutSeconds);
            }

            // Headers comunes
            if (!string.IsNullOrWhiteSpace(Options.ApiKey))
            {
                HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Key", Options.ApiKey);
            }

            if (Options.CustomHeaders != null)
            {
                foreach (var header in Options.CustomHeaders)
                {
                    HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            Logger.LogInformation(
                "HttpClient configurado para {ServiceName}: BaseUrl={BaseUrl}, Timeout={Timeout}s",
                Options.ServiceName, Options.BaseUrl, Options.TimeoutSeconds);
        }

        /// <summary>
        /// Realiza una petición GET y deserializa la respuesta
        /// </summary>
        protected async Task<TResponse?> GetAsync<TResponse>(
            string endpoint,
            CancellationToken cancellationToken = default) where TResponse : class
        {
            return await ExecuteRequestAsync<TResponse>(
                HttpMethod.Get,
                endpoint,
                content: null,
                cancellationToken);
        }

        /// <summary>
        /// Realiza una petición POST y deserializa la respuesta
        /// </summary>
        protected async Task<TResponse?> PostAsync<TRequest, TResponse>(
            string endpoint,
            TRequest requestData,
            CancellationToken cancellationToken = default) where TResponse : class
        {
            var content = JsonContent.Create(requestData, options: JsonOptions);
            return await ExecuteRequestAsync<TResponse>(
                HttpMethod.Post,
                endpoint,
                content,
                cancellationToken);
        }

        /// <summary>
        /// Realiza una petición PUT y deserializa la respuesta
        /// </summary>
        protected async Task<TResponse?> PutAsync<TRequest, TResponse>(
            string endpoint,
            TRequest requestData,
            CancellationToken cancellationToken = default) where TResponse : class
        {
            var content = JsonContent.Create(requestData, options: JsonOptions);
            return await ExecuteRequestAsync<TResponse>(
                HttpMethod.Put,
                endpoint,
                content,
                cancellationToken);
        }

        /// <summary>
        /// Realiza una petición DELETE
        /// </summary>
        protected async Task<bool> DeleteAsync(
            string endpoint,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
                using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error en DELETE request a {Endpoint}", endpoint);
                return false;
            }
        }

        /// <summary>
        /// Ejecuta una petición HTTP con manejo de errores estandarizado
        /// </summary>
        private async Task<TResponse?> ExecuteRequestAsync<TResponse>(
            HttpMethod method,
            string endpoint,
            HttpContent? content,
            CancellationToken cancellationToken) where TResponse : class
        {
            var startTime = DateTime.UtcNow;

            try
            {
                using var request = new HttpRequestMessage(method, endpoint)
                {
                    Content = content
                };

                Logger.LogDebug(
                    "Iniciando {Method} request a {ServiceName}: {Endpoint}",
                    method.Method, Options.ServiceName, endpoint);

                using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    
                    Logger.LogWarning(
                        "Request fallida a {ServiceName}: {Method} {Endpoint} - Status: {StatusCode}, Duration: {Duration}ms, Error: {Error}",
                        Options.ServiceName, method.Method, endpoint, (int)response.StatusCode, duration, errorContent);

                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken).ConfigureAwait(false);

                Logger.LogInformation(
                    "Request exitosa a {ServiceName}: {Method} {Endpoint} - Status: {StatusCode}, Duration: {Duration}ms",
                    Options.ServiceName, method.Method, endpoint, (int)response.StatusCode, duration);

                return result;
            }
            catch (HttpRequestException ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                Logger.LogError(ex,
                    "Error de red en request a {ServiceName}: {Method} {Endpoint}, Duration: {Duration}ms",
                    Options.ServiceName, method.Method, endpoint, duration);
                return null;
            }
            catch (TaskCanceledException ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                Logger.LogError(ex,
                    "Timeout en request a {ServiceName}: {Method} {Endpoint}, Duration: {Duration}ms",
                    Options.ServiceName, method.Method, endpoint, duration);
                return null;
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                Logger.LogError(ex,
                    "Error inesperado en request a {ServiceName}: {Method} {Endpoint}, Duration: {Duration}ms",
                    Options.ServiceName, method.Method, endpoint, duration);
                return null;
            }
        }

        /// <summary>
        /// Construye query string desde un diccionario
        /// </summary>
        protected static string BuildQueryString(Dictionary<string, string?> parameters)
        {
            var validParams = parameters
                .Where(p => !string.IsNullOrWhiteSpace(p.Value))
                .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!)}");

            return string.Join("&", validParams);
        }

        /// <summary>
        /// Construye URL con query parameters
        /// </summary>
        protected static string BuildUrl(string endpoint, Dictionary<string, string?> parameters)
        {
            var queryString = BuildQueryString(parameters);
            return string.IsNullOrEmpty(queryString) ? endpoint : $"{endpoint}?{queryString}";
        }
    }
}
