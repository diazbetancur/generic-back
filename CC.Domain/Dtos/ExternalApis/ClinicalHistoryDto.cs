namespace CC.Domain.Dtos.ExternalApis
{
    /// <summary>
    /// DTO para respuesta de lista de episodios
    /// </summary>
    public class EpisodesResponseDto
    {
        /// <summary>
        /// ID del paciente
        /// </summary>
        public string PatientId { get; set; } = string.Empty;

        /// <summary>
        /// Lista de episodios
        /// </summary>
        public List<EpisodeDto> Episodes { get; set; } = new();
    }

    /// <summary>
    /// DTO para un episodio médico
    /// </summary>
    public class EpisodeDto
    {
        /// <summary>
        /// Número de episodio
        /// </summary>
        public string Episode { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de ingreso
        /// </summary>
        public string admission_date { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el PDF está disponible
        /// </summary>
        public bool pdf_available { get; set; }

        /// <summary>
        /// URL para descargar el PDF
        /// </summary>
        public string? pdf_url { get; set; }
    }

    /// <summary>
    /// DTO para mensaje de error de la API
    /// </summary>
    public class ApiErrorMessageDto
    {
        /// <summary>
        /// Mensaje de error
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}