using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CC.Aplication.Services
{
    /// <summary>
    /// Servicio para gestión de autorización basada en permisos
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IPermissionRepository _permissionRepo;
        private readonly IRolePermissionRepository _rolePermissionRepo;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AuthorizationService> _logger;

        private const string UserPermissionsCachePrefix = "UserPermissions_";
        private const string RolePermissionsCachePrefix = "RolePermissions_";
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

        public AuthorizationService(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IPermissionRepository permissionRepo,
            IRolePermissionRepository rolePermissionRepo,
            IMemoryCache cache,
            ILogger<AuthorizationService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _permissionRepo = permissionRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _cache = cache;
            _logger = logger;
        }

        public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionName)
        {
            if (string.IsNullOrWhiteSpace(permissionName))
            {
                _logger.LogWarning("Intento de verificar permiso con nombre nulo o vacío para usuario {UserId}", userId);
                return false;
            }

            try
            {
                var permissions = await GetUserPermissionsAsync(userId);
                var hasPermission = permissions.Contains(permissionName, StringComparer.OrdinalIgnoreCase);

                _logger.LogDebug(
                    "Verificación de permiso {Permission} para usuario {UserId}: {HasPermission}",
                    permissionName, userId, hasPermission);

                return hasPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al verificar permiso {Permission} para usuario {UserId}",
                    permissionName, userId);
                return false;
            }
        }

        public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
        {
            var cacheKey = $"{UserPermissionsCachePrefix}{userId}";

            // Intentar obtener del cache
            if (_cache.TryGetValue(cacheKey, out List<string>? cachedPermissions) && cachedPermissions != null)
            {
                _logger.LogDebug("Permisos obtenidos del cache para usuario {UserId}", userId);
                return cachedPermissions;
            }

            try
            {
                _logger.LogDebug("Obteniendo permisos de BD para usuario {UserId}", userId);

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("Usuario {UserId} no encontrado", userId);
                    return new List<string>();
                }

                // Obtener roles del usuario
                var userRoles = await _userManager.GetRolesAsync(user);

                if (!userRoles.Any())
                {
                    _logger.LogDebug("Usuario {UserId} no tiene roles asignados", userId);
                    return new List<string>();
                }

                // Obtener permisos de los roles del usuario
                var permissions = new List<string>();
                
                foreach (var roleName in userRoles)
                {
                    // Obtener el rol por nombre
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role != null)
                    {
                        // Obtener permisos del rol
                        var rolePermissions = await _rolePermissionRepo.GetPermissionsByRoleIdAsync(role.Id);
                        permissions.AddRange(rolePermissions.Select(p => p.Name));
                    }
                }

                var distinctPermissions = permissions.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                // Guardar en cache
                _cache.Set(cacheKey, distinctPermissions, CacheExpiration);

                _logger.LogInformation(
                    "Permisos cargados para usuario {UserId}: {PermissionCount} permisos únicos",
                    userId, distinctPermissions.Count);

                return distinctPermissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos de usuario {UserId}", userId);
                return new List<string>();
            }
        }

        public async Task<bool> RoleHasPermissionAsync(Guid roleId, string permissionName)
        {
            if (string.IsNullOrWhiteSpace(permissionName))
            {
                _logger.LogWarning("Intento de verificar permiso con nombre nulo o vacío para rol {RoleId}", roleId);
                return false;
            }

            try
            {
                var hasPermission = await _rolePermissionRepo.RoleHasPermissionAsync(roleId, permissionName);

                _logger.LogDebug(
                    "Verificación de permiso {Permission} para rol {RoleId}: {HasPermission}",
                    permissionName, roleId, hasPermission);

                return hasPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al verificar permiso {Permission} para rol {RoleId}",
                    permissionName, roleId);
                return false;
            }
        }

        public void InvalidateUserCache(Guid userId)
        {
            var cacheKey = $"{UserPermissionsCachePrefix}{userId}";
            _cache.Remove(cacheKey);

            _logger.LogInformation("Cache de permisos invalidado para usuario {UserId}", userId);
        }

        public async Task InvalidateRoleCacheAsync(Guid roleId)
        {
            try
            {
                // Obtener el rol
                var role = await _roleManager.FindByIdAsync(roleId.ToString());
                if (role == null)
                {
                    _logger.LogWarning("Rol {RoleId} no encontrado al invalidar cache", roleId);
                    return;
                }

                // Obtener todos los usuarios con este rol
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);

                foreach (var user in usersInRole)
                {
                    InvalidateUserCache(user.Id);
                }

                _logger.LogInformation(
                    "Cache de permisos invalidado para {UserCount} usuarios del rol {RoleId}",
                    usersInRole.Count, roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar cache para rol {RoleId}", roleId);
            }
        }
    }
}
