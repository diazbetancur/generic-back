namespace CC.Domain.Dtos
{
    /// <summary>
    /// DTO para filtros de consulta de solicitudes
    /// </summary>
    public class RequestListQueryDto
    {
        /// <summary>
        /// Código del tipo de documento para filtrar por paciente
        /// </summary>
        public string? DocTypeCode { get; set; }

        /// <summary>
        /// Número de documento para filtrar por paciente
        /// </summary>
        public string? DocNumber { get; set; }

        /// <summary>
        /// Fecha desde (por defecto: último año)
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Fecha hasta (por defecto: hoy)
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Filtrar por estado específico
        /// </summary>
        public Guid? StateId { get; set; }

        /// <summary>
        /// Filtrar por tipo de solicitud
        /// </summary>
        public Guid? RequestTypeId { get; set; }

        /// <summary>
        /// Filtrar por usuario asignado
        /// </summary>
        public Guid? AssignedUserId { get; set; }

        /// <summary>
        /// Registros a saltar (para paginación)
        /// </summary>
        public int Skip { get; set; } = 0;

        /// <summary>
        /// Registros a tomar (máximo por página)
        /// </summary>
        public int Take { get; set; } = 50;
    }
}