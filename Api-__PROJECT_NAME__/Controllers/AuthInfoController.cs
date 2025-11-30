using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AuthService = CC.Domain.Interfaces.Services.IAuthorizationService;

namespace Api_Portar_Paciente.Controllers
{
    /// <summary>
    /// Controlador para obtener información del usuario autenticado
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuthInfoController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthInfoController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Obtiene información del usuario actual y sus permisos
        /// </summary>
        /// <returns>Información del usuario con lista de permisos</returns>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value 
                ?? User.FindFirst("unique_name")?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value 
                ?? User.FindFirst("email")?.Value;
            var userType = User.FindFirst("UserType")?.Value;
            var firstName = User.FindFirst("FirstName")?.Value;
            var lastName = User.FindFirst("LastName")?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // Obtener permisos del usuario
            List<string> permissions = new();
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                permissions = await _authService.GetUserPermissionsAsync(userGuid);
            }

            return Ok(new
            {
                userId,
                userName,
                email,
                userType,
                firstName,
                lastName,
                roles,
                permissions,
                isAuthenticated = true
            });
        }
    }
}
