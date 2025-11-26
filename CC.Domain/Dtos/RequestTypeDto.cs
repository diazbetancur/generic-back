namespace CC.Domain.Dtos
{
    /// <summary>
    /// DTO para transferencia de datos de tipos de solicitud
    /// </summary>
    public class RequestTypeDto : BaseDto<Guid>
    {
        /// <summary>
        /// Nombre del tipo de solicitud
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Plantilla HTML o texto del tipo de solicitud
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// Indica si el registro está eliminado (soft delete)
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Indica si el tipo de solicitud está activo
        /// </summary>
        public bool IsActive { get; set; }

        public bool IsSystem { get; set; }
    }
}