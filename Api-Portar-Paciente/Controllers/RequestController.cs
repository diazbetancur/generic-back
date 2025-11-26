using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Services;
using CC.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    /// <summary>
    /// Controlador para gestión de solicitudes de pacientes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RequestController : ControllerBase
    {
        private readonly IRequestService _service;
        private readonly ILogger<RequestController> _logger;
        private readonly IHistoryRequestService _historyRequestService;

        public RequestController(IRequestService service, ILogger<RequestController> logger, IHistoryRequestService historyRequestService)
        {
            _service = service;
            _logger = logger;
            _historyRequestService = historyRequestService;
        }

        /// <summary>
        /// Crea una nueva solicitud (frontend paciente - SIN AUTENTICACIÓN)
        /// </summary>
        /// <param name="dto">Datos de la solicitud a crear</param>
        /// <returns>Solicitud creada con su ID y estado inicial</returns>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] RequestCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.CreateRequestAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear solicitud");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear solicitud");
                return StatusCode(500, new { error = "Error interno al crear la solicitud" });
            }
        }

        /// <summary>
        /// Actualiza la descripción de una solicitud (paciente - SIN AUTENTICACIÓN)
        /// </summary>
        /// <param name="id">ID de la solicitud</param>
        /// <param name="dto">Nueva descripción</param>
        /// <returns>Solicitud actualizada</returns>
        [HttpPut("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] RequestUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var request = await _service.FindByIdAsync(id);
                if (request == null)
                {
                    _logger.LogWarning("Solicitud no encontrada: {RequestId}", id);
                    return NotFound(new { error = "Solicitud no encontrada" });
                }

                request.Description = dto.Description;
                request.LastUpdateDate = DateTime.UtcNow;

                await _service.UpdateAsync(request);

                var historyDto = new HistoryRequestDto
                {
                    Id = Guid.NewGuid(),
                    RequestId = id,
                    OldStateId = request.StateId,
                    NewStateId = null,
                    UserId = null,
                    Changes = dto.Description,
                    DateCreated = DateTime.UtcNow
                };

                await _historyRequestService.AddAsync(historyDto);

                var result = await _service.FindByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar solicitud {RequestId}", id);
                return StatusCode(500, new { error = "Error interno al actualizar la solicitud" });
            }
        }

        /// <summary>
        /// Actualiza el estado de una solicitud (asesor/admin - REQUIERE PERMISOS)
        /// </summary>
        /// <param name="id">ID de la solicitud</param>
        /// <param name="dto">Datos de actualización por asesor</param>
        /// <returns>Solicitud actualizada</returns>
        [HttpPut("{id}/advisor")]
        [Authorize(Policy = PermissionConstants.Policies.CanChangeRequestState)]
        [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateByAdvisor(Guid id, [FromBody] RequestUpdateByAdvisorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var request = await _service.FindByIdAsync(id);
                if (request == null)
                {
                    _logger.LogWarning("Solicitud no encontrada: {RequestId}", id);
                    return NotFound(new { error = "Solicitud no encontrada" });
                }

                var oldStateId = request.StateId;

                request.StateId = dto.StateId;
                request.LastUpdateDate = DateTime.UtcNow;
                request.RequestUpdated = true;

                if (dto.AssignedUserId.HasValue)
                    request.AssignedUserId = dto.AssignedUserId.Value;

                await _service.UpdateAsync(request);

                var historyDto = new HistoryRequestDto
                {
                    Id = Guid.NewGuid(),
                    RequestId = id,
                    OldStateId = oldStateId,
                    NewStateId = dto.StateId,
                    UserId = dto.UserId,
                    Changes = dto.Observations,
                    DateCreated = DateTime.UtcNow
                };

                await _historyRequestService.AddAsync(historyDto);

                var result = await _service.FindByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar solicitud por asesor {RequestId}", id);
                return StatusCode(500, new { error = "Error interno al actualizar la solicitud" });
            }
        }

        /// <summary>
        /// Obtiene solicitudes de un paciente específico por documento (SIN AUTENTICACIÓN)
        /// </summary>
        /// <param name="docTypeCode">Código del tipo de documento (ej: CC, TI)</param>
        /// <param name="docNumber">Número de documento</param>
        /// <param name="from">Fecha desde (opcional, default: último año)</param>
        /// <param name="to">Fecha hasta (opcional, default: hoy)</param>
        /// <returns>Lista de solicitudes del paciente</returns>
        [HttpGet("patient")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<RequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByPatient(
            [FromQuery] string docTypeCode,
            [FromQuery] string docNumber,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            if (string.IsNullOrWhiteSpace(docTypeCode))
                return BadRequest(new { error = "docTypeCode es requerido" });

            if (string.IsNullOrWhiteSpace(docNumber))
                return BadRequest(new { error = "docNumber es requerido" });

            try
            {
                var results = await _service.GetByPatientAsync(docTypeCode, docNumber, from, to);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar solicitudes del paciente {DocType}-{DocNumber}",
                    docTypeCode, docNumber);
                return StatusCode(500, new { error = "Error al consultar las solicitudes" });
            }
        }

        /// <summary>
        /// Obtiene solicitudes con filtros generales (admin - REQUIERE PERMISOS)
        /// </summary>
        /// <param name="query">Filtros de búsqueda</param>
        /// <returns>Lista paginada de solicitudes con total</returns>
        [HttpGet]
        [Authorize(Policy = PermissionConstants.Policies.CanViewRequests)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetFiltered([FromQuery] RequestListQueryDto query)
        {
            try
            {
                var (items, totalCount) = await _service.GetFilteredAsync(query);
                return Ok(new
                {
                    items,
                    totalCount,
                    skip = query.Skip,
                    take = query.Take,
                    hasMore = (query.Skip + query.Take) < totalCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar solicitudes con filtros");
                return StatusCode(500, new { error = "Error al consultar las solicitudes" });
            }
        }

        /// <summary>
        /// Obtiene una solicitud por ID (permite acceso a pacientes y admins)
        /// </summary>
        /// <param name="id">ID de la solicitud</param>
        /// <returns>Detalles de la solicitud</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _service.FindByIdAsync(id);
                if (result == null)
                    return NotFound(new { error = "Solicitud no encontrada" });

                var history = await _historyRequestService.GetAllAsync(x => x.RequestId == result.Id);

                result.LastObservation = history
                    .OrderByDescending(h => h.DateCreated)
                    .FirstOrDefault()?.Changes;

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar solicitud {RequestId}", id);
                return StatusCode(500, new { error = "Error al consultar la solicitud" });
            }
        }

        /// <summary>
        /// Obtiene el historial completo de cambios de una solicitud (permite acceso a pacientes y admins)
        /// </summary>
        /// <param name="id">ID de la solicitud</param>
        /// <returns>Lista de cambios históricos</returns>
        [HttpGet("{id}/history")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<HistoryRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHistory(Guid id)
        {
            try
            {
                var history = await _service.GetHistoryAsync(id);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar historial de solicitud {RequestId}", id);
                return StatusCode(500, new { error = "Error al consultar el historial" });
            }
        }
    }
}