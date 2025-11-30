namespace CC.Domain.Entities
{
    /// <summary>
    /// Permiso del sistema (granular)
    /// </summary>
    public class Permission : EntityBase<Guid>
    {
        /// <summary>
        /// Nombre �nico del permiso (ej: "Requests.Create", "Users.Delete")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// M�dulo al que pertenece (ej: "Requests", "Users", "Reports")
        /// </summary>
        public string Module { get; set; } = string.Empty;

        /// <summary>
        /// Descripci�n legible del permiso
        /// </summary>
        public string? Description { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el permiso est� activo
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Relaci�n con roles que tienen este permiso
        /// </summary>
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
