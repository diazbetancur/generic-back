namespace CC.Domain.Dtos
{
    /// <summary>
    /// DTO completo de solicitud (para respuestas)
    /// </summary>
    public class RequestDto : BaseDto<Guid>
    {
        /// <summary>
        /// ID del tipo de documento
        /// </summary>
        public Guid DocTypeId { get; set; }

        /// <summary>
        /// Código del tipo de documento (ej: "CC")
        /// </summary>
        public string DocTypeCode { get; set; }

        /// <summary>
        /// Descripción del tipo de documento
        /// </summary>
        public string DocTypeDescription { get; set; }

        /// <summary>
        /// Número de documento del paciente
        /// </summary>
        public string DocNumber { get; set; }

        /// <summary>
        /// ID del tipo de solicitud
        /// </summary>
        public Guid RequestTypeId { get; set; }

        /// <summary>
        /// Nombre del tipo de solicitud
        /// </summary>
        public string RequestTypeName { get; set; }

        /// <summary>
        /// Descripción de la solicitud
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// ID del estado actual
        /// </summary>
        public Guid StateId { get; set; }

        /// <summary>
        /// Nombre del estado actual
        /// </summary>
        public string StateName { get; set; }

        /// <summary>
        /// Color del estado en hexadecimal
        /// </summary>
        public string StateColor { get; set; }

        /// <summary>
        /// ID del usuario asignado (null si no está asignado)
        /// </summary>
        public Guid? AssignedUserId { get; set; }

        /// <summary>
        /// Nombre del usuario asignado
        /// </summary>
        public string? AssignedUserName { get; set; }
    }
}
