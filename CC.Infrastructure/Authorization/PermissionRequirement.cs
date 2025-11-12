using Microsoft.AspNetCore.Authorization;

namespace CC.Infrastructure.Authorization
{
    /// <summary>
    /// Requisito de autorización basado en permisos
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Nombre del permiso requerido
        /// </summary>
        public string Permission { get; }

        /// <summary>
        /// Constructor del requisito de permiso
        /// </summary>
        /// <param name="permission">Nombre del permiso (ej: "Requests.Create")</param>
        public PermissionRequirement(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                throw new ArgumentException("El permiso no puede ser nulo o vacío", nameof(permission));

            Permission = permission;
        }
    }
}
