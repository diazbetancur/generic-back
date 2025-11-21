namespace CC.Domain.Dtos
{
    /// <summary>
    /// DTO para rol con permisos
    /// </summary>
    public class RoleDto
    {
        /// <summary>
        /// ID del rol
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre del rol
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del rol
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Lista de IDs de permisos asignados
        /// </summary>
        public List<Guid> PermissionIds { get; set; } = new();

        /// <summary>
        /// Lista de permisos completos (para lectura)
        /// </summary>
        public List<PermissionDto>? Permissions { get; set; }

        public bool isSystem { get; set; }
    }

    /// <summary>
    /// DTO para crear rol
    /// </summary>
    public class CreateRoleDto
    {
        /// <summary>
        /// Nombre del rol
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción del rol
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO para actualizar permisos de un rol
    /// </summary>
    public class UpdateRolePermissionsDto
    {
        /// <summary>
        /// Lista de IDs de permisos a asignar
        /// </summary>
        public List<Guid> PermissionIds { get; set; }
    }
}