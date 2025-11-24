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

    /// <summary>
    /// DTO para actualizar la descripción de una solicitud (paciente)
    /// </summary>
    public class RequestUpdateDto
    {
        /// <summary>
        /// Nueva descripción o información adicional de la solicitud
        /// </summary>
        public required string Description { get; set; }
    }

    /// <summary>
    /// DTO para actualizar una solicitud por parte del asesor (admin)
    /// </summary>
    public class RequestUpdateByAdvisorDto
    {
        /// <summary>
        /// Nuevo estado de la solicitud
        /// </summary>
        public required Guid StateId { get; set; }

        /// <summary>
        /// ID del usuario asesor que realiza la actualización
        /// </summary>
        public required Guid UserId { get; set; }

        /// <summary>
        /// Observaciones del asesor sobre el cambio de estado
        /// </summary>
        public required string Observations { get; set; }

        /// <summary>
        /// ID del usuario asignado para atender la solicitud (opcional)
        /// </summary>
        public Guid? AssignedUserId { get; set; }
    }
}
