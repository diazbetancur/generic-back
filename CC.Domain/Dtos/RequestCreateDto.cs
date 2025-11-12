namespace CC.Domain.Dtos
{
    /// <summary>
    /// DTO para crear una nueva solicitud desde el frontend (paciente)
    /// </summary>
    public class RequestCreateDto
    {
        /// <summary>
        /// Código del tipo de documento (ej: "CC", "TI", "CE")
        /// </summary>
        public required string DocTypeCode { get; set; }

        /// <summary>
        /// Número de documento del paciente
        /// </summary>
        public required string DocNumber { get; set; }

        /// <summary>
        /// ID del tipo de solicitud
        /// </summary>
        public required Guid RequestTypeId { get; set; }

        /// <summary>
        /// Descripción detallada de la solicitud
        /// </summary>
        public required string Description { get; set; }
    }
}
