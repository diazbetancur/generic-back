using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CC.Infrastructure.Authorization
{
    /// <summary>
    /// Handler para verificar permisos de usuarios
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IAuthorizationService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PermissionHandler> _logger;

        public PermissionHandler(
            IAuthorizationService authService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PermissionHandler> logger)
        {
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var user = context.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("Usuario no autenticado intentando acceder a recurso con permiso {Permission}", requirement.Permission);
                context.Fail();
                return;
            }

            // Verificar si es un paciente (los pacientes no tienen permisos de admin)
            var userType = user.FindFirst("UserType")?.Value;
            if (userType == "Patient")
            {
                _logger.LogDebug("Usuario tipo Patient no puede acceder a permisos de administración: {Permission}", requirement.Permission);
                context.Fail();
                return;
            }

            // Obtener ID del usuario del claim
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("No se pudo obtener el ID del usuario del token para permiso {Permission}", requirement.Permission);
                context.Fail();
                return;
            }

            try
            {
                // Verificar si el usuario tiene el permiso
                var hasPermission = await _authService.UserHasPermissionAsync(userId, requirement.Permission);

                if (hasPermission)
                {
                    _logger.LogDebug(
                        "Usuario {UserId} tiene permiso {Permission}",
                        userId, requirement.Permission);
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogWarning(
                        "Usuario {UserId} NO tiene permiso {Permission}",
                        userId, requirement.Permission);
                    context.Fail();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al verificar permiso {Permission} para usuario {UserId}",
                    requirement.Permission, userId);
                context.Fail();
            }
        }
    }
}
