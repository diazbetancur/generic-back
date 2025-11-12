namespace CC.Domain.Interfaces.Services
{
    /// <summary>
    /// Servicio para verificar permisos de usuarios
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Verifica si un usuario tiene un permiso específico
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="permissionName">Nombre del permiso (ej: "Requests.Create")</param>
        /// <returns>True si tiene el permiso, False si no</returns>
        Task<bool> UserHasPermissionAsync(Guid userId, string permissionName);

        /// <summary>
        /// Obtiene todos los permisos de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Lista de nombres de permisos</returns>
        Task<List<string>> GetUserPermissionsAsync(Guid userId);

        /// <summary>
        /// Verifica si un rol tiene un permiso específico
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionName">Nombre del permiso</param>
        /// <returns>True si el rol tiene el permiso</returns>
        Task<bool> RoleHasPermissionAsync(Guid roleId, string permissionName);

        /// <summary>
        /// Invalida el cache de permisos de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        void InvalidateUserCache(Guid userId);

        /// <summary>
        /// Invalida el cache de permisos de todos los usuarios con un rol específico
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        Task InvalidateRoleCacheAsync(Guid roleId);
    }
}
