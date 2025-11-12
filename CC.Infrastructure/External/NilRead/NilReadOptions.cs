using CC.Infrastructure.External.Base;

namespace CC.Infrastructure.External.NilRead
{
    /// <summary>
    /// Opciones de configuración para el servicio NilRead
    /// </summary>
    public class NilReadOptions : ExternalServiceOptions
    {
        /// <summary>
        /// Endpoint para health check
        /// </summary>
        public string HealthEndpoint { get; set; } = "/health";

        /// <summary>
        /// Endpoint para obtener exámenes de un paciente
        /// </summary>
        public string ExamsEndpoint { get; set; } = "/patients/{patient_id}/exams";

        /// <summary>
        /// Endpoint para obtener informe PDF
        /// </summary>
        public string ReportEndpoint { get; set; } = "/reports/{dateFolder}/{filename}";

        /// <summary>
        /// Endpoint para generar enlace del visor NilRead
        /// </summary>
        public string ViewerLinkEndpoint { get; set; } = "/exams/{accession}/viewer-link";

        /// <summary>
        /// Límite por defecto de registros a retornar
        /// </summary>
        public int DefaultLimit { get; set; } = 10;

        /// <summary>
        /// Límite máximo permitido de registros
        /// </summary>
        public int MaxLimit { get; set; } = 50;

        /// <summary>
        /// Permitir certificados SSL inválidos (solo para desarrollo/test)
        /// </summary>
        public bool AllowInvalidCerts { get; set; }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(HealthEndpoint) &&
                   !string.IsNullOrWhiteSpace(ExamsEndpoint) &&
                   !string.IsNullOrWhiteSpace(ReportEndpoint) &&
                   !string.IsNullOrWhiteSpace(ViewerLinkEndpoint) &&
                   DefaultLimit > 0 &&
                   MaxLimit > 0 &&
                   MaxLimit >= DefaultLimit;
        }

        public override string GetValidationErrors()
        {
            var errors = new List<string>();

            if (!base.IsValid())
                errors.Add(base.GetValidationErrors());

            if (string.IsNullOrWhiteSpace(HealthEndpoint))
                errors.Add("HealthEndpoint no configurado");

            if (string.IsNullOrWhiteSpace(ExamsEndpoint))
                errors.Add("ExamsEndpoint no configurado");

            if (string.IsNullOrWhiteSpace(ReportEndpoint))
                errors.Add("ReportEndpoint no configurado");

            if (string.IsNullOrWhiteSpace(ViewerLinkEndpoint))
                errors.Add("ViewerLinkEndpoint no configurado");

            if (DefaultLimit <= 0)
                errors.Add("DefaultLimit debe ser mayor a 0");

            if (MaxLimit <= 0)
                errors.Add("MaxLimit debe ser mayor a 0");

            if (MaxLimit < DefaultLimit)
                errors.Add("MaxLimit debe ser mayor o igual a DefaultLimit");

            return string.Join("; ", errors);
        }
    }
}
