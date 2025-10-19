using CC.Infrastructure.External.Base;

namespace CC.Infrastructure.External.Email
{
    /// <summary>
    /// Opciones de configuración para Microsoft Graph Email Service
    /// </summary>
    public class EmailServiceOptions : ExternalServiceOptions
    {
        /// <summary>
        /// Tenant ID de Azure AD
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// Application (Client) ID
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Client Secret
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// User ID del buzón que enviará los correos
        /// </summary>
        public string SendFromUserId { get; set; } = string.Empty;

        /// <summary>
        /// Email por defecto del remitente (para display)
        /// </summary>
        public string DefaultFromEmail { get; set; } = string.Empty;

        /// <summary>
        /// Nombre por defecto del remitente
        /// </summary>
        public string DefaultFromName { get; set; } = string.Empty;

        /// <summary>
        /// Endpoint de Microsoft Graph
        /// </summary>
        public string GraphBaseUrl { get; set; } = "https://graph.microsoft.com/v1.0";

        /// <summary>
        /// Scopes requeridos para envío de email
        /// </summary>
        public string[] Scopes { get; set; } = new[] { "https://graph.microsoft.com/.default" };

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(TenantId) &&
                   !string.IsNullOrWhiteSpace(ClientId) &&
                   !string.IsNullOrWhiteSpace(ClientSecret) &&
                   !string.IsNullOrWhiteSpace(SendFromUserId);
        }

        public override string GetValidationErrors()
        {
            var errors = new List<string>();

            if (!base.IsValid())
                errors.Add(base.GetValidationErrors());

            if (string.IsNullOrWhiteSpace(TenantId))
                errors.Add("TenantId es requerido");

            if (string.IsNullOrWhiteSpace(ClientId))
                errors.Add("ClientId es requerido");

            if (string.IsNullOrWhiteSpace(ClientSecret))
                errors.Add("ClientSecret es requerido");

            if (string.IsNullOrWhiteSpace(SendFromUserId))
                errors.Add("SendFromUserId es requerido");

            return string.Join(", ", errors);
        }
    }
}
