using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthService = CC.Domain.Interfaces.Services.IAuthorizationService;

namespace Api_Portar_Paciente.Controllers.Admin
{
    /// <summary>
    /// Controlador para gestión de roles y sus permisos
    /// </summary>
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IPermissionRepository _permissionRepo;
        private readonly IRolePermissionRepository _rolePermissionRepo;
        private readonly AuthService _authService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(
            RoleManager<Role> roleManager,
            UserManager<User> userManager,
            IPermissionRepository permissionRepo,
            IRolePermissionRepository rolePermissionRepo,
            AuthService authService,
            ILogger<RolesController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _permissionRepo = permissionRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los roles del sistema
        /// </summary>
        /// <returns>Lista de roles</returns>
        [HttpGet]
        [Authorize(Policy = "CanViewRoles")]
        [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los roles del sistema");

                var roles = await _roleManager.Roles
                    .OrderBy(r => r.Name)
                    .ToListAsync();

                var roleDtos = new List<RoleDto>();

                foreach (var role in roles)
                {
                    var permissions = await _rolePermissionRepo.GetPermissionsByRoleIdAsync(role.Id);

                    roleDtos.Add(new RoleDto
                    {
                        Id = role.Id,
                        Name = role.Name!,
                        Description = role.Description,
                        PermissionIds = permissions.Select(p => p.Id).ToList(),
                        Permissions = permissions.Select(p => new PermissionDto
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Module = p.Module,
                            Description = p.Description,
                            IsActive = p.IsActive,
                            DateCreated = p.DateCreated
                        }).ToList()
                    });
                }

                _logger.LogInformation("Roles obtenidos: {RoleCount} roles", roleDtos.Count);

                return Ok(roleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener roles del sistema");
                return StatusCode(500, new { error = "Error al obtener roles" });
            }
        }

        /// <summary>
        /// Obtiene un rol por su ID
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <returns>Rol con sus permisos</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "CanViewRoles")]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoleById(Guid id)
        {
            try
            {
                _logger.LogInformation("Obteniendo rol {RoleId}", id);

                var role = await _roleManager.FindByIdAsync(id.ToString());

                if (role == null)
                {
                    _logger.LogWarning("Rol {RoleId} no encontrado", id);
                    return NotFound(new { error = "Rol no encontrado" });
                }

                var permissions = await _rolePermissionRepo.GetPermissionsByRoleIdAsync(role.Id);

                var roleDto = new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name!,
                    Description = role.Description,
                    PermissionIds = permissions.Select(p => p.Id).ToList(),
                    Permissions = permissions.Select(p => new PermissionDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Module = p.Module,
                        Description = p.Description,
                        IsActive = p.IsActive,
                        DateCreated = p.DateCreated
                    }).ToList()
                };

                return Ok(roleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rol {RoleId}", id);
                return StatusCode(500, new { error = "Error al obtener el rol" });
            }
        }

        /// <summary>
        /// Crea un nuevo rol
        /// </summary>
        /// <param name="dto">Datos del rol a crear</param>
        /// <returns>Rol creado</returns>
        [HttpPost]
        [Authorize(Policy = "CanManageRoles")]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                    return BadRequest(new { error = "El nombre del rol es requerido" });

                _logger.LogInformation("Creando nuevo rol: {RoleName}", dto.Name);

                // Verificar si el rol ya existe
                var existingRole = await _roleManager.FindByNameAsync(dto.Name);
                if (existingRole != null)
                {
                    _logger.LogWarning("El rol {RoleName} ya existe", dto.Name);
                    return BadRequest(new { error = $"El rol '{dto.Name}' ya existe" });
                }

                var role = new Role
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    NormalizedName = dto.Name.ToUpperInvariant()
                };

                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Error al crear rol {RoleName}: {Errors}", dto.Name, errors);
                    return BadRequest(new { error = $"Error al crear rol: {errors}" });
                }

                _logger.LogInformation("Rol {RoleName} creado exitosamente con ID {RoleId}", dto.Name, role.Id);

                var roleDto = new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    PermissionIds = new List<Guid>(),
                    Permissions = new List<PermissionDto>()
                };

                return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, roleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear rol");
                return StatusCode(500, new { error = "Error al crear el rol" });
            }
        }

        /// <summary>
        /// Actualiza un rol existente
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <param name="dto">Datos actualizados del rol</param>
        /// <returns>Rol actualizado</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = "CanManageRoles")]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] CreateRoleDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                    return BadRequest(new { error = "El nombre del rol es requerido" });

                _logger.LogInformation("Actualizando rol {RoleId}", id);

                var role = await _roleManager.FindByIdAsync(id.ToString());
                if (role == null)
                {
                    _logger.LogWarning("Rol {RoleId} no encontrado", id);
                    return NotFound(new { error = "Rol no encontrado" });
                }

                // Verificar si el nuevo nombre ya existe en otro rol
                if (role.Name != dto.Name)
                {
                    var existingRole = await _roleManager.FindByNameAsync(dto.Name);
                    if (existingRole != null && existingRole.Id != id)
                    {
                        _logger.LogWarning("El nombre de rol {RoleName} ya está en uso", dto.Name);
                        return BadRequest(new { error = $"El nombre '{dto.Name}' ya está en uso por otro rol" });
                    }
                }

                role.Name = dto.Name;
                role.Description = dto.Description;
                role.NormalizedName = dto.Name.ToUpperInvariant();

                var result = await _roleManager.UpdateAsync(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Error al actualizar rol {RoleId}: {Errors}", id, errors);
                    return BadRequest(new { error = $"Error al actualizar rol: {errors}" });
                }

                _logger.LogInformation("Rol {RoleId} actualizado exitosamente", id);

                var permissions = await _rolePermissionRepo.GetPermissionsByRoleIdAsync(role.Id);

                var roleDto = new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    PermissionIds = permissions.Select(p => p.Id).ToList(),
                    Permissions = permissions.Select(p => new PermissionDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Module = p.Module,
                        Description = p.Description,
                        IsActive = p.IsActive,
                        DateCreated = p.DateCreated
                    }).ToList()
                };

                return Ok(roleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar rol {RoleId}", id);
                return StatusCode(500, new { error = "Error al actualizar el rol" });
            }
        }

        /// <summary>
        /// Elimina un rol
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = "CanManageRoles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            try
            {
                _logger.LogInformation("Eliminando rol {RoleId}", id);

                var role = await _roleManager.FindByIdAsync(id.ToString());
                if (role == null)
                {
                    _logger.LogWarning("Rol {RoleId} no encontrado", id);
                    return NotFound(new { error = "Rol no encontrado" });
                }

                // Verificar si el rol tiene usuarios asignados
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                if (usersInRole.Any())
                {
                    _logger.LogWarning(
                        "No se puede eliminar rol {RoleId} porque tiene {UserCount} usuarios asignados",
                        id, usersInRole.Count);
                    return BadRequest(new
                    {
                        error = $"No se puede eliminar el rol porque tiene {usersInRole.Count} usuario(s) asignado(s)"
                    });
                }

                // Eliminar permisos del rol
                await _rolePermissionRepo.DeleteByRoleIdAsync(id);

                var result = await _roleManager.DeleteAsync(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Error al eliminar rol {RoleId}: {Errors}", id, errors);
                    return BadRequest(new { error = $"Error al eliminar rol: {errors}" });
                }

                _logger.LogInformation("Rol {RoleId} eliminado exitosamente", id);

                return Ok(new { message = "Rol eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar rol {RoleId}", id);
                return StatusCode(500, new { error = "Error al eliminar el rol" });
            }
        }

        /// <summary>
        /// Obtiene los permisos de un rol
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <returns>Lista de permisos del rol</returns>
        [HttpGet("{id}/permissions")]
        [Authorize(Policy = "CanViewRoles")]
        [ProducesResponseType(typeof(IEnumerable<PermissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRolePermissions(Guid id)
        {
            try
            {
                _logger.LogInformation("Obteniendo permisos del rol {RoleId}", id);

                var role = await _roleManager.FindByIdAsync(id.ToString());
                if (role == null)
                {
                    _logger.LogWarning("Rol {RoleId} no encontrado", id);
                    return NotFound(new { error = "Rol no encontrado" });
                }

                var permissions = await _rolePermissionRepo.GetPermissionsByRoleIdAsync(id);

                var permissionDtos = permissions.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Module = p.Module,
                    Description = p.Description,
                    IsActive = p.IsActive,
                    DateCreated = p.DateCreated
                }).ToList();

                _logger.LogInformation(
                    "Permisos del rol {RoleId} obtenidos: {PermissionCount} permisos",
                    id, permissionDtos.Count);

                return Ok(permissionDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos del rol {RoleId}", id);
                return StatusCode(500, new { error = "Error al obtener permisos del rol" });
            }
        }

        /// <summary>
        /// Actualiza los permisos de un rol
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <param name="dto">Lista de IDs de permisos a asignar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPut("{id}/permissions")]
        [Authorize(Policy = "CanManagePermissions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateRolePermissions(
            Guid id,
            [FromBody] UpdateRolePermissionsDto dto)
        {
            try
            {
                if (dto == null || dto.PermissionIds == null)
                    return BadRequest(new { error = "La lista de permisos es requerida" });

                _logger.LogInformation(
                    "Actualizando permisos del rol {RoleId}: {PermissionCount} permisos",
                    id, dto.PermissionIds.Count);

                var role = await _roleManager.FindByIdAsync(id.ToString());
                if (role == null)
                {
                    _logger.LogWarning("Rol {RoleId} no encontrado", id);
                    return NotFound(new { error = "Rol no encontrado" });
                }

                // Verificar que todos los permisos existen
                var permissions = await _permissionRepo.GetAllAsync(
                    filter: p => dto.PermissionIds.Contains(p.Id) && p.IsActive
                );

                if (permissions.Count() != dto.PermissionIds.Count)
                {
                    var invalidCount = dto.PermissionIds.Count - permissions.Count();
                    _logger.LogWarning(
                        "Se encontraron {InvalidCount} permisos inválidos o inactivos",
                        invalidCount);
                    return BadRequest(new
                    {
                        error = $"Se encontraron {invalidCount} permiso(s) inválido(s) o inactivo(s)"
                    });
                }

                // Eliminar permisos actuales del rol
                await _rolePermissionRepo.DeleteByRoleIdAsync(id);

                // Agregar nuevos permisos
                foreach (var permissionId in dto.PermissionIds)
                {
                    var rolePermission = new RolePermission
                    {
                        Id = Guid.NewGuid(),
                        RoleId = id,
                        PermissionId = permissionId,
                        DateCreated = DateTime.UtcNow
                    };

                    await _rolePermissionRepo.AddAsync(rolePermission);
                }

                // Invalidar cache de usuarios con este rol
                await _authService.InvalidateRoleCacheAsync(id);

                _logger.LogInformation(
                    "Permisos del rol {RoleId} actualizados exitosamente: {PermissionCount} permisos asignados",
                    id, dto.PermissionIds.Count);

                return Ok(new
                {
                    message = "Permisos actualizados exitosamente",
                    permissionCount = dto.PermissionIds.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar permisos del rol {RoleId}", id);
                return StatusCode(500, new { error = "Error al actualizar permisos del rol" });
            }
        }
    }
}
