namespace CC.Domain.Entities
{
    /// <summary>
    /// Relación muchos a muchos entre Roles y Permisos
    /// </summary>
    public class RolePermission : EntityBase<Guid>
    {
        /// <summary>
        /// ID del rol
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// Rol asociado
        /// </summary>
        public virtual Role Role { get; set; } = null!;

        /// <summary>
        /// ID del permiso
        /// </summary>
        public Guid PermissionId { get; set; }

        /// <summary>
        /// Permiso asociado
        /// </summary>
        public virtual Permission Permission { get; set; } = null!;
    }
}
