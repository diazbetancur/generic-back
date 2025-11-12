namespace CC.Domain.Dtos
{
    /// <summary>
    /// DTO para permiso
    /// </summary>
    public class PermissionDto : BaseDto<Guid>
    {
        /// <summary>
        /// Nombre único del permiso
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Módulo al que pertenece
        /// </summary>
        public string Module { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del permiso
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Indica si está activo
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO agrupado por módulo (para frontend)
    /// </summary>
    public class PermissionsByModuleDto
    {
        /// <summary>
        /// Nombre del módulo
        /// </summary>
        public string Module { get; set; } = string.Empty;

        /// <summary>
        /// Lista de permisos del módulo
        /// </summary>
        public List<PermissionDto> Permissions { get; set; } = new();
    }
}
