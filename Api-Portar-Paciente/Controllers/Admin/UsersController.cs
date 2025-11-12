using CC.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AuthService = CC.Domain.Interfaces.Services.IAuthorizationService;

namespace Api_Portar_Paciente.Controllers.Admin
{
    /// <summary>
    /// Controlador para gestión de usuarios administrativos
    /// </summary>
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly AuthService _authService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            AuthService authService,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los usuarios administrativos del sistema
        /// </summary>
        /// <returns>Lista de usuarios con sus roles</returns>
        [HttpGet]
        [Authorize(Policy = "CanViewUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los usuarios administrativos");

                var users = await _userManager.Users
                    .OrderBy(u => u.UserName)
                    .ToListAsync();

                var userDtos = new List<object>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    userDtos.Add(new
                    {
                        id = user.Id,
                        userName = user.UserName,
                        email = user.Email,
                        emailConfirmed = user.EmailConfirmed,
                        phoneNumber = user.PhoneNumber,
                        roles = roles,
                        lockoutEnd = user.LockoutEnd,
                        accessFailedCount = user.AccessFailedCount
                    });
                }

                _logger.LogInformation("Usuarios obtenidos: {UserCount} usuarios", userDtos.Count);

                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(500, new { error = "Error al obtener usuarios" });
            }
        }

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Usuario con sus roles y permisos</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "CanViewUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            try
            {
                _logger.LogInformation("Obteniendo usuario {UserId}", id);

                var user = await _userManager.FindByIdAsync(id.ToString());

                if (user == null)
                {
                    _logger.LogWarning("Usuario {UserId} no encontrado", id);
                    return NotFound(new { error = "Usuario no encontrado" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                var permissions = await _authService.GetUserPermissionsAsync(user.Id);

                var userDto = new
                {
                    id = user.Id,
                    userName = user.UserName,
                    email = user.Email,
                    emailConfirmed = user.EmailConfirmed,
                    phoneNumber = user.PhoneNumber,
                    roles = roles,
                    permissions = permissions,
                    lockoutEnd = user.LockoutEnd,
                    accessFailedCount = user.AccessFailedCount
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario {UserId}", id);
                return StatusCode(500, new { error = "Error al obtener el usuario" });
            }
        }

        /// <summary>
        /// Crea un nuevo usuario administrativo
        /// </summary>
        /// <param name="dto">Datos del usuario a crear</param>
        /// <returns>Usuario creado</returns>
        [HttpPost]
        [Authorize(Policy = "CanManageUsers")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.UserName))
                    return BadRequest(new { error = "El nombre de usuario es requerido" });

                if (string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest(new { error = "El email es requerido" });

                if (string.IsNullOrWhiteSpace(dto.Password))
                    return BadRequest(new { error = "La contraseña es requerida" });

                _logger.LogInformation("Creando nuevo usuario administrativo: {UserName}", dto.UserName);

                var user = new User
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Error al crear usuario {UserName}: {Errors}", dto.UserName, errors);
                    return BadRequest(new { error = $"Error al crear usuario: {errors}" });
                }

                _logger.LogInformation("Usuario {UserName} creado exitosamente con ID {UserId}", dto.UserName, user.Id);

                var userDto = new
                {
                    id = user.Id,
                    userName = user.UserName,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber
                };

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return StatusCode(500, new { error = "Error al crear el usuario" });
            }
        }

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="dto">Datos actualizados del usuario</param>
        /// <returns>Usuario actualizado</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = "CanManageUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                _logger.LogInformation("Actualizando usuario {UserId}", id);

                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    _logger.LogWarning("Usuario {UserId} no encontrado", id);
                    return NotFound(new { error = "Usuario no encontrado" });
                }

                if (!string.IsNullOrWhiteSpace(dto.Email))
                    user.Email = dto.Email;

                if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                    user.PhoneNumber = dto.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Error al actualizar usuario {UserId}: {Errors}", id, errors);
                    return BadRequest(new { error = $"Error al actualizar usuario: {errors}" });
                }

                // Actualizar contraseña si se proporciona
                if (!string.IsNullOrWhiteSpace(dto.NewPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

                    if (!passwordResult.Succeeded)
                    {
                        var errors = string.Join(", ", passwordResult.Errors.Select(e => e.Description));
                        _logger.LogWarning("Error al actualizar contraseña de usuario {UserId}: {Errors}", id, errors);
                    }
                }

                _logger.LogInformation("Usuario {UserId} actualizado exitosamente", id);

                var userDto = new
                {
                    id = user.Id,
                    userName = user.UserName,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario {UserId}", id);
                return StatusCode(500, new { error = "Error al actualizar el usuario" });
            }
        }

        /// <summary>
        /// Elimina un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CanManageUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                _logger.LogInformation("Eliminando usuario {UserId}", id);

                // Prevenir auto-eliminación
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId == id.ToString())
                {
                    _logger.LogWarning("Usuario {UserId} intentó eliminarse a sí mismo", id);
                    return BadRequest(new { error = "No puede eliminarse a sí mismo" });
                }

                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    _logger.LogWarning("Usuario {UserId} no encontrado", id);
                    return NotFound(new { error = "Usuario no encontrado" });
                }

                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Error al eliminar usuario {UserId}: {Errors}", id, errors);
                    return BadRequest(new { error = $"Error al eliminar usuario: {errors}" });
                }

                _logger.LogInformation("Usuario {UserId} eliminado exitosamente", id);

                return Ok(new { message = "Usuario eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario {UserId}", id);
                return StatusCode(500, new { error = "Error al eliminar el usuario" });
            }
        }

        /// <summary>
        /// Asigna roles a un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="dto">Lista de nombres de roles a asignar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPut("{id}/roles")]
        [Authorize(Policy = "CanAssignRoles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignRolesToUser(Guid id, [FromBody] AssignRolesDto dto)
        {
            try
            {
                if (dto == null || dto.RoleNames == null || !dto.RoleNames.Any())
                    return BadRequest(new { error = "La lista de roles es requerida" });

                _logger.LogInformation(
                    "Asignando roles al usuario {UserId}: {RoleCount} roles",
                    id, dto.RoleNames.Count);

                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    _logger.LogWarning("Usuario {UserId} no encontrado", id);
                    return NotFound(new { error = "Usuario no encontrado" });
                }

                // Verificar que todos los roles existen
                foreach (var roleName in dto.RoleNames)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        _logger.LogWarning("Rol {RoleName} no existe", roleName);
                        return BadRequest(new { error = $"El rol '{roleName}' no existe" });
                    }
                }

                // Remover roles actuales
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                        _logger.LogWarning("Error al remover roles actuales del usuario {UserId}: {Errors}", id, errors);
                        return BadRequest(new { error = $"Error al remover roles actuales: {errors}" });
                    }
                }

                // Asignar nuevos roles
                var addResult = await _userManager.AddToRolesAsync(user, dto.RoleNames);
                if (!addResult.Succeeded)
                {
                    var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Error al asignar roles al usuario {UserId}: {Errors}", id, errors);
                    return BadRequest(new { error = $"Error al asignar roles: {errors}" });
                }

                // Invalidar cache de permisos del usuario
                _authService.InvalidateUserCache(id);

                _logger.LogInformation(
                    "Roles asignados exitosamente al usuario {UserId}: {Roles}",
                    id, string.Join(", ", dto.RoleNames));

                return Ok(new
                {
                    message = "Roles asignados exitosamente",
                    roles = dto.RoleNames
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar roles al usuario {UserId}", id);
                return StatusCode(500, new { error = "Error al asignar roles" });
            }
        }

        /// <summary>
        /// Obtiene los roles de un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Lista de roles del usuario</returns>
        [HttpGet("{id}/roles")]
        [Authorize(Policy = "CanViewUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserRoles(Guid id)
        {
            try
            {
                _logger.LogInformation("Obteniendo roles del usuario {UserId}", id);

                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    _logger.LogWarning("Usuario {UserId} no encontrado", id);
                    return NotFound(new { error = "Usuario no encontrado" });
                }

                var roles = await _userManager.GetRolesAsync(user);

                _logger.LogInformation(
                    "Roles del usuario {UserId} obtenidos: {RoleCount} roles",
                    id, roles.Count);

                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener roles del usuario {UserId}", id);
                return StatusCode(500, new { error = "Error al obtener roles del usuario" });
            }
        }
    }

    /// <summary>
    /// DTO para crear usuario
    /// </summary>
    public class CreateUserDto
    {
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public required string Password { get; set; }
    }

    /// <summary>
    /// DTO para actualizar usuario
    /// </summary>
    public class UpdateUserDto
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? NewPassword { get; set; }
    }

    /// <summary>
    /// DTO para asignar roles
    /// </summary>
    public class AssignRolesDto
    {
        public required List<string> RoleNames { get; set; }
    }
}
