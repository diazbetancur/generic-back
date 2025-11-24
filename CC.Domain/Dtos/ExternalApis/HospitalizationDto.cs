namespace CC.Domain.Dtos.ExternalApis
{
    /// <summary>
    /// DTO para respuesta de la API de Hospitalizaciones
    /// </summary>
    public class HospitalizationsResponseDto
    {
        /// <summary>
        /// ID del paciente
        /// </summary>
        public string PatientId { get; set; } = string.Empty;

        /// <summary>
        /// Total de hospitalizaciones
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Lista de hospitalizaciones
        /// </summary>
        public List<HospitalizationDto> Hospitalizations { get; set; } = new();
    }

    /// <summary>
    /// DTO para respuesta paginada de hospitalizaciones
    /// </summary>
    public class HospitalizationsPaginatedResponseDto : HospitalizationsResponseDto
    {
        /// <summary>
        /// Offset de paginación
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Límite de resultados
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Cantidad de resultados en la página actual
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// DTO para una hospitalización individual
    /// </summary>
    public class HospitalizationDto
    {
        /// <summary>
        /// ID del paciente
        /// </summary>
        public string PatientId { get; set; } = string.Empty;

        /// <summary>
        /// Número de historia clínica
        /// </summary>
        public string History { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del médico responsable
        /// </summary>
        public string Doctor { get; set; } = string.Empty;

        /// <summary>
        /// Especialidad médica
        /// </summary>
        public string Specialty { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del paciente
        /// </summary>
        public string PatientName { get; set; } = string.Empty;

        /// <summary>
        /// Número de admisión/ingreso
        /// </summary>
        public string Admission { get; set; } = string.Empty;

        /// <summary>
        /// Número de consultorio/habitación
        /// </summary>
        public string Room { get; set; } = string.Empty;
    }
}
