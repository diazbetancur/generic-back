using CC.Infrastructure.External.Base;

namespace CC.Infrastructure.External.Hospitalizations
{
    /// <summary>
    /// Opciones de configuración para el servicio de API de Hospitalizaciones
    /// </summary>
    public class HospitalizationsApiOptions : ExternalServiceOptions
    {
        /// <summary>
        /// Endpoint para health check
        /// </summary>
        public string HealthEndpoint { get; set; } = "/health";

        /// <summary>
        /// Endpoint para obtener hospitalizaciones de un paciente
        /// </summary>
        public string PatientHospitalizationsEndpoint { get; set; } = "/patients/{patient_id}/hospitalizations";

        /// <summary>
        /// Endpoint para obtener hospitalizaciones con paginación
        /// </summary>
        public string PatientHospitalizationsPaginatedEndpoint { get; set; } = "/patients/{patient_id}/hospitalizations-with-pagination";

        /// <summary>
        /// Endpoint para información de versión
        /// </summary>
        public string VersionEndpoint { get; set; } = "/version";

        /// <summary>
        /// Límite por defecto de resultados
        /// </summary>
        public int DefaultLimit { get; set; } = 50;

        /// <summary>
        /// Límite máximo permitido
        /// </summary>
        public int MaxLimit { get; set; } = 200;

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(ApiKey) &&
                   DefaultLimit > 0 &&
                   MaxLimit > 0 &&
                   DefaultLimit <= MaxLimit;
        }

        public override string GetValidationErrors()
        {
            var errors = new List<string>();
            var baseErrors = base.GetValidationErrors();
            if (!string.IsNullOrEmpty(baseErrors))
                errors.Add(baseErrors);

            if (string.IsNullOrWhiteSpace(ApiKey))
                errors.Add("ApiKey es requerido");

            if (DefaultLimit <= 0)
                errors.Add("DefaultLimit debe ser mayor a 0");

            if (MaxLimit <= 0)
                errors.Add("MaxLimit debe ser mayor a 0");

            if (DefaultLimit > MaxLimit)
                errors.Add("DefaultLimit no puede ser mayor que MaxLimit");

            return string.Join(", ", errors);
        }
    }
}
