namespace CC.Domain.Dtos
{
    /// <summary>
    /// DTO para transferencia de datos de estados
    /// </summary>
    public class StateDto : BaseDto<Guid>
    {
        /// <summary>
        /// Nombre del estado
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Color hexadecimal del estado (ej: #FF5733)
        /// </summary>
        public required string HexColor { get; set; }

        /// <summary>
        /// Indica si el registro está eliminado (soft delete)
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
