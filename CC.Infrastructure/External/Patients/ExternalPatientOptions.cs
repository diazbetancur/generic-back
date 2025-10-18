using CC.Infrastructure.External.Base;

namespace CC.Infrastructure.External.Patients
{
    /// <summary>
    /// Opciones de configuración específicas para el servicio de pacientes
    /// </summary>
    public class ExternalPatientOptions : ExternalServiceOptions
    {
        /// <summary>
        /// Endpoint para consulta de pacientes
        /// </summary>
        public string PatientEndpoint { get; set; } = "/api/Paciente";

        /// <summary>
        /// Permitir certificados SSL inválidos (solo para dev/qa)
        /// </summary>
        public bool AllowInvalidCerts { get; set; } = false;

        public override bool IsValid()
        {
            return base.IsValid() && !string.IsNullOrWhiteSpace(PatientEndpoint);
        }

        public override string GetValidationErrors()
        {
            var errors = base.GetValidationErrors();
            
            if (string.IsNullOrWhiteSpace(PatientEndpoint))
            {
                errors += string.IsNullOrEmpty(errors) ? "" : ", ";
                errors += "PatientEndpoint es requerido";
            }

            return errors;
        }
    }
}
