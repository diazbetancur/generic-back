namespace CC.Infrastructure.External.Base
{
    /// <summary>
    /// Opciones base para configuración de servicios externos
    /// </summary>
    public class ExternalServiceOptions
    {
        /// <summary>
        /// Nombre identificador del servicio
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// URL base del servicio externo
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Timeout en segundos para las peticiones
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// API Key para autenticación (si aplica)
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Headers personalizados adicionales
        /// </summary>
        public Dictionary<string, string>? CustomHeaders { get; set; }

        /// <summary>
        /// Habilitar reintentos automáticos
        /// </summary>
        public bool EnableRetry { get; set; } = false;

        /// <summary>
        /// Número máximo de reintentos
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Validar que la configuración sea correcta
        /// </summary>
        public virtual bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(ServiceName) &&
                   !string.IsNullOrWhiteSpace(BaseUrl) &&
                   TimeoutSeconds > 0;
        }

        /// <summary>
        /// Obtener descripción de errores de validación
        /// </summary>
        public virtual string GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ServiceName))
                errors.Add("ServiceName es requerido");

            if (string.IsNullOrWhiteSpace(BaseUrl))
                errors.Add("BaseUrl es requerido");

            if (TimeoutSeconds <= 0)
                errors.Add("TimeoutSeconds debe ser mayor a 0");

            return string.Join(", ", errors);
        }
    }
}
