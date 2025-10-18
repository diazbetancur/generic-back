using CC.Infrastructure.External.Base;

namespace CC.Infrastructure.External.Sms
{
    /// <summary>
    /// Opciones de configuración para el servicio de SMS (Liwa API)
    /// </summary>
    public class SmsServiceOptions : ExternalServiceOptions
    {
        /// <summary>
        /// Endpoint para autenticación (obtener token)
        /// </summary>
        public string AuthEndpoint { get; set; } = "/v2/auth/login";

        /// <summary>
        /// Endpoint para envío de SMS individual
        /// </summary>
        public string SendSingleEndpoint { get; set; } = "/v2/sms/single";

        /// <summary>
        /// Endpoint para envío masivo de SMS
        /// </summary>
        public string SendMultipleEndpoint { get; set; } = "/v2/sms/multiple";

        /// <summary>
        /// Cuenta de usuario para autenticación
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña para autenticación
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// API Key estática de Liwa
        /// </summary>
        public string LiwaApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Código de país por defecto (57 para Colombia)
        /// </summary>
        public string DefaultCountryCode { get; set; } = "57";

        /// <summary>
        /// Tipo de mensaje por defecto (1 = Nacional)
        /// </summary>
        public int DefaultMessageType { get; set; } = 1;

        /// <summary>
        /// Duración del token en minutos (para cache)
        /// </summary>
        public int TokenExpirationMinutes { get; set; } = 55; // Renovar antes de 1 hora

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(AuthEndpoint) &&
                   !string.IsNullOrWhiteSpace(SendSingleEndpoint) &&
                   !string.IsNullOrWhiteSpace(Account) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(LiwaApiKey);
        }

        public override string GetValidationErrors()
        {
            var errors = new List<string>();

            if (!base.IsValid())
                errors.Add(base.GetValidationErrors());

            if (string.IsNullOrWhiteSpace(AuthEndpoint))
                errors.Add("AuthEndpoint es requerido");

            if (string.IsNullOrWhiteSpace(SendSingleEndpoint))
                errors.Add("SendSingleEndpoint es requerido");

            if (string.IsNullOrWhiteSpace(Account))
                errors.Add("Account es requerido");

            if (string.IsNullOrWhiteSpace(Password))
                errors.Add("Password es requerido");

            if (string.IsNullOrWhiteSpace(LiwaApiKey))
                errors.Add("LiwaApiKey es requerido");

            return string.Join(", ", errors);
        }
    }
}
