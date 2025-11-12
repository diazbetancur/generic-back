namespace CC.Domain.Entities
{
    /// <summary>
    /// Permiso del sistema (granular)
    /// </summary>
    public class Permission : EntityBase<Guid>
    {
        /// <summary>
        /// Nombre único del permiso (ej: "Requests.Create", "Users.Delete")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Módulo al que pertenece (ej: "Requests", "Users", "Reports")
        /// </summary>
        public string Module { get; set; } = string.Empty;

        /// <summary>
        /// Descripción legible del permiso
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el permiso está activo
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Relación con roles que tienen este permiso
        /// </summary>
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
