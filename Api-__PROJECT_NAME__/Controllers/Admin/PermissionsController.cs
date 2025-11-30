using CC.Domain.Dtos;
using CC.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api_Portar_Paciente.Controllers.Admin
{
    /// <summary>
    /// Controlador para gestión de permisos del sistema
    /// </summary>
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionRepository _permissionRepo;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(
            IPermissionRepository permissionRepo,
            ILogger<PermissionsController> logger)
        {
            _permissionRepo = permissionRepo;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los permisos del sistema agrupados por módulo
        /// </summary>
        /// <returns>Lista de permisos agrupados por módulo</returns>
        [HttpGet]
        [Authorize(Policy = "CanViewRoles")]
        [ProducesResponseType(typeof(IEnumerable<PermissionsByModuleDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPermissions()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los permisos del sistema");

                var permissions = await _permissionRepo.GetAllAsync(
                    filter: p => p.IsActive,
                    orderBy: q => q.OrderBy(p => p.Module).ThenBy(p => p.Name)
                );

                var grouped = permissions
                    .GroupBy(p => p.Module)
                    .Select(g => new PermissionsByModuleDto
                    {
                        Module = g.Key,
                        Permissions = g.Select(p => new PermissionDto
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Module = p.Module,
                            Description = p.Description,
                            IsActive = p.IsActive,
                            DateCreated = p.DateCreated
                        }).ToList()
                    })
                    .OrderBy(g => g.Module)
                    .ToList();

                _logger.LogInformation(
                    "Permisos obtenidos exitosamente: {ModuleCount} módulos, {PermissionCount} permisos totales",
                    grouped.Count, permissions.Count());

                return Ok(grouped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos del sistema");
                return StatusCode(500, new { error = "Error al obtener permisos" });
            }
        }

        /// <summary>
        /// Obtiene los permisos de un módulo específico
        /// </summary>
        /// <param name="module">Nombre del módulo (ej: "Requests", "Users")</param>
        /// <returns>Lista de permisos del módulo</returns>
        [HttpGet("module/{module}")]
        [Authorize(Policy = "CanViewRoles")]
        [ProducesResponseType(typeof(IEnumerable<PermissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPermissionsByModule(string module)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(module))
                    return BadRequest(new { error = "El nombre del módulo es requerido" });

                _logger.LogInformation("Obteniendo permisos del módulo {Module}", module);

                var permissions = await _permissionRepo.GetByModuleAsync(module);

                if (!permissions.Any())
                {
                    _logger.LogWarning("No se encontraron permisos para el módulo {Module}", module);
                    return NotFound(new { error = $"No se encontraron permisos para el módulo '{module}'" });
                }

                var dtos = permissions.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Module = p.Module,
                    Description = p.Description,
                    IsActive = p.IsActive,
                    DateCreated = p.DateCreated
                }).ToList();

                _logger.LogInformation(
                    "Permisos del módulo {Module} obtenidos: {Count} permisos",
                    module, dtos.Count);

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos del módulo {Module}", module);
                return StatusCode(500, new { error = "Error al obtener permisos del módulo" });
            }
        }

        /// <summary>
        /// Obtiene un permiso por su ID
        /// </summary>
        /// <param name="id">ID del permiso</param>
        /// <returns>Permiso encontrado</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "CanViewRoles")]
        [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPermissionById(Guid id)
        {
            try
            {
                _logger.LogInformation("Obteniendo permiso {PermissionId}", id);

                var permission = await _permissionRepo.FindByIdAsync(id);

                if (permission == null)
                {
                    _logger.LogWarning("Permiso {PermissionId} no encontrado", id);
                    return NotFound(new { error = "Permiso no encontrado" });
                }

                var dto = new PermissionDto
                {
                    Id = permission.Id,
                    Name = permission.Name,
                    Module = permission.Module,
                    Description = permission.Description,
                    IsActive = permission.IsActive,
                    DateCreated = permission.DateCreated
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permiso {PermissionId}", id);
                return StatusCode(500, new { error = "Error al obtener el permiso" });
            }
        }

        /// <summary>
        /// Obtiene la lista de módulos disponibles
        /// </summary>
        /// <returns>Lista de nombres de módulos</returns>
        [HttpGet("modules")]
        [Authorize(Policy = "CanViewRoles")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetModules()
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de módulos");

                var permissions = await _permissionRepo.GetAllAsync(
                    filter: p => p.IsActive
                );

                var modules = permissions
                    .Select(p => p.Module)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();

                _logger.LogInformation("Módulos obtenidos: {ModuleCount} módulos", modules.Count);

                return Ok(modules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista de módulos");
                return StatusCode(500, new { error = "Error al obtener módulos" });
            }
        }
    }
}