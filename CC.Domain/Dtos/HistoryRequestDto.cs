namespace CC.Domain.Dtos
{
    /// <summary>
    /// DTO para historial de cambios de solicitud
    /// </summary>
    public class HistoryRequestDto : BaseDto<Guid>
    {
        /// <summary>
        /// ID de la solicitud
        /// </summary>
        public Guid RequestId { get; set; }

        /// <summary>
        /// ID del estado anterior
        /// </summary>
        public Guid? OldStateId { get; set; }

        /// <summary>
        /// Nombre del estado anterior
        /// </summary>
        public string? OldStateName { get; set; }

        /// <summary>
        /// ID del nuevo estado
        /// </summary>
        public Guid? NewStateId { get; set; }

        /// <summary>
        /// Nombre del nuevo estado
        /// </summary>
        public string? NewStateName { get; set; }

        /// <summary>
        /// ID del usuario que realizó el cambio
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Nombre del usuario que realizó el cambio
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Descripción de los cambios
        /// </summary>
        public string Changes { get; set; }
    }
}
