using CC.Domain.Dtos.ExternalApis;
using CC.Domain.Interfaces.External;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    /// <summary>
    /// Controlador para gestión de citas médicas de pacientes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentsApiService _appointmentsService;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(
            IAppointmentsApiService appointmentsService,
            ILogger<AppointmentsController> logger)
        {
            _appointmentsService = appointmentsService;
            _logger = logger;
        }

        /// <summary>
        /// Verifica el estado de salud de la API de Citas
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiHealthDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> CheckHealth()
        {
            try
            {
                var health = await _appointmentsService.CheckHealthAsync();
                
                if (health?.Status == "ok")
                    return Ok(health);

                return StatusCode(503, new { error = "API de citas no disponible" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar salud de API de citas");
                return StatusCode(503, new { error = "Error al conectar con API de citas" });
            }
        }

        /// <summary>
        /// Obtiene todas las citas futuras de un paciente
        /// </summary>
        /// <param name="patientId">Número de documento del paciente</param>
        [HttpGet("patient/{patientId}")]
        [ProducesResponseType(typeof(AppointmentsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPatientAppointments(string patientId)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                return BadRequest(new { error = "patientId es requerido" });

            try
            {
                _logger.LogInformation("Consultando citas del paciente: {PatientId}", patientId);

                var appointments = await _appointmentsService.GetPatientAppointmentsAsync(patientId);

                if (appointments == null)
                {
                    _logger.LogWarning("No se encontraron citas para el paciente: {PatientId}", patientId);
                    return NotFound(new { error = "No se encontraron citas para este paciente" });
                }

                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar citas del paciente {PatientId}", patientId);
                return StatusCode(500, new { error = "Error al consultar las citas médicas" });
            }
        }

        /// <summary>
        /// Obtiene las citas de un paciente con paginación
        /// </summary>
        /// <param name="patientId">Número de documento del paciente</param>
        /// <param name="limit">Cantidad de registros por página (máximo 200)</param>
        /// <param name="offset">Número de registros a saltar</param>
        [HttpGet("patient/{patientId}/paginated")]
        [ProducesResponseType(typeof(AppointmentsPaginatedResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPatientAppointmentsPaginated(
            string patientId,
            [FromQuery] int limit = 50,
            [FromQuery] int offset = 0)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                return BadRequest(new { error = "patientId es requerido" });

            if (limit <= 0)
                return BadRequest(new { error = "limit debe ser mayor a 0" });

            if (offset < 0)
                return BadRequest(new { error = "offset debe ser mayor o igual a 0" });

            try
            {
                _logger.LogInformation(
                    "Consultando citas paginadas del paciente: {PatientId}, Limit={Limit}, Offset={Offset}",
                    patientId, limit, offset);

                var appointments = await _appointmentsService
                    .GetPatientAppointmentsPaginatedAsync(patientId, limit, offset);

                if (appointments == null)
                {
                    _logger.LogWarning("No se encontraron citas para el paciente: {PatientId}", patientId);
                    return NotFound(new { error = "No se encontraron citas para este paciente" });
                }

                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar citas paginadas del paciente {PatientId}", patientId);
                return StatusCode(500, new { error = "Error al consultar las citas médicas" });
            }
        }

        /// <summary>
        /// Obtiene información de la versión de la API de Citas
        /// </summary>
        [HttpGet("version")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiVersionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetVersion()
        {
            try
            {
                var version = await _appointmentsService.GetVersionAsync();
                
                if (version == null)
                    return StatusCode(503, new { error = "No se pudo obtener versión de la API" });

                return Ok(version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener versión de API de citas");
                return StatusCode(500, new { error = "Error al consultar versión" });
            }
        }
    }
}
