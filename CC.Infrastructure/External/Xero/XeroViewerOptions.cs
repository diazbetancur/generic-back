using CC.Infrastructure.External.Base;

namespace CC.Infrastructure.External.Xero
{
    /// <summary>
    /// Opciones de configuración para Xero Viewer Service
    /// </summary>
    public class XeroViewerOptions : ExternalServiceOptions
    {
        /// <summary>
        /// Endpoint para health check
        /// </summary>
        public string HealthEndpoint { get; set; } = "/health";

        /// <summary>
        /// Endpoint para listar estudios de un paciente
        /// </summary>
        public string StudiesEndpoint { get; set; } = "/patients/{patient_id}/studies";

        /// <summary>
        /// Endpoint para generar enlace del visor
        /// </summary>
        public string ViewerLinkEndpoint { get; set; } = "/studies/{study_uid}/viewer-link";

        /// <summary>
        /// Límite por defecto de estudios por página
        /// </summary>
        public int DefaultLimit { get; set; } = 10;

        /// <summary>
        /// Límite máximo de estudios por página
        /// </summary>
        public int MaxLimit { get; set; } = 50;

        /// <summary>
        /// Permitir certificados SSL inválidos (solo para dev/qa on-premise)
        /// </summary>
        public bool AllowInvalidCerts { get; set; } = false;

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(HealthEndpoint) &&
                   !string.IsNullOrWhiteSpace(StudiesEndpoint) &&
                   !string.IsNullOrWhiteSpace(ViewerLinkEndpoint) &&
                   !string.IsNullOrWhiteSpace(ApiKey);
        }

        public override string GetValidationErrors()
        {
            var errors = new List<string>();

            if (!base.IsValid())
                errors.Add(base.GetValidationErrors());

            if (string.IsNullOrWhiteSpace(HealthEndpoint))
                errors.Add("HealthEndpoint es requerido");

            if (string.IsNullOrWhiteSpace(StudiesEndpoint))
                errors.Add("StudiesEndpoint es requerido");

            if (string.IsNullOrWhiteSpace(ViewerLinkEndpoint))
                errors.Add("ViewerLinkEndpoint es requerido");

            if (string.IsNullOrWhiteSpace(ApiKey))
                errors.Add("ApiKey es requerido");

            return string.Join(", ", errors);
        }
    }
}
