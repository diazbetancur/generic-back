using CC.Domain.Entities;

namespace CC.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz de repositorio para RolePermission
    /// </summary>
    public interface IRolePermissionRepository : IERepositoryBase<RolePermission>
    {
        /// <summary>
        /// Obtiene todos los permisos de un rol
        /// </summary>
        Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId);

        /// <summary>
        /// Elimina todos los permisos de un rol
        /// </summary>
        Task DeleteByRoleIdAsync(Guid roleId);

        /// <summary>
        /// Verifica si un rol tiene un permiso específico
        /// </summary>
        Task<bool> RoleHasPermissionAsync(Guid roleId, string permissionName);
    }
}
