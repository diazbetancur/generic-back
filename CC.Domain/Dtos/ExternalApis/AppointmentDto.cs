namespace CC.Domain.Dtos.ExternalApis
{
    /// <summary>
    /// DTO para respuesta de la API de Citas Médicas
    /// </summary>
    public class AppointmentsResponseDto
    {
        /// <summary>
        /// ID del paciente
        /// </summary>
        public string PatientId { get; set; } = string.Empty;

        /// <summary>
        /// Total de citas
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Lista de citas
        /// </summary>
        public List<AppointmentDto> Appointments { get; set; } = new();
    }

    /// <summary>
    /// DTO para respuesta paginada de la API de Citas
    /// </summary>
    public class AppointmentsPaginatedResponseDto : AppointmentsResponseDto
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
    /// DTO para una cita médica individual
    /// </summary>
    public class AppointmentDto
    {
        /// <summary>
        /// ID del paciente
        /// </summary>
        public string PatientId { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del paciente
        /// </summary>
        public string PatientName { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de la cita (YYYY-MM-DD)
        /// </summary>
        public string AppointmentDate { get; set; } = string.Empty;

        /// <summary>
        /// Hora de la cita (HH:MM)
        /// </summary>
        public string AppointmentTime { get; set; } = string.Empty;

        /// <summary>
        /// Estado de la cita
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Código de la especialidad
        /// </summary>
        public string SpecialtyCode { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de la especialidad
        /// </summary>
        public string SpecialtyName { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del médico
        /// </summary>
        public string DoctorName { get; set; } = string.Empty;

        /// <summary>
        /// Número del consultorio
        /// </summary>
        public string Office { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para health check de la API
    /// </summary>
    public class ApiHealthDto
    {
        /// <summary>
        /// Estado del servicio
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para información de versión de la API
    /// </summary>
    public class ApiVersionDto
    {
        /// <summary>
        /// Nombre del servicio
        /// </summary>
        public string Service { get; set; } = string.Empty;

        /// <summary>
        /// Versión del servicio
        /// </summary>
        public string Version { get; set; } = string.Empty;
    }
}
