using CC.Domain.Dtos.ExternalApis;
using CC.Domain.Interfaces.External;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    /// <summary>
    /// Controlador para gestión de hospitalizaciones de pacientes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HospitalizationsController : ControllerBase
    {
        private readonly IHospitalizationsApiService _hospitalizationsService;
        private readonly ILogger<HospitalizationsController> _logger;

        public HospitalizationsController(
            IHospitalizationsApiService hospitalizationsService,
            ILogger<HospitalizationsController> logger)
        {
            _hospitalizationsService = hospitalizationsService;
            _logger = logger;
        }

        /// <summary>
        /// Verifica el estado de salud de la API de Hospitalizaciones
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiHealthDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> CheckHealth()
        {
            try
            {
                var health = await _hospitalizationsService.CheckHealthAsync();

                if (health?.Status == "ok")
                    return Ok(health);

                return StatusCode(503, new { error = "API de hospitalizaciones no disponible" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar salud de API de hospitalizaciones");
                return StatusCode(503, new { error = "Error al conectar con API de hospitalizaciones" });
            }
        }

        /// <summary>
        /// Obtiene todas las hospitalizaciones de un paciente
        /// </summary>
        /// <param name="patientId">Número de documento del paciente</param>
        [HttpGet("patient/{patientId}")]
        [ProducesResponseType(typeof(HospitalizationsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPatientHospitalizations(string patientId)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                return BadRequest(new { error = "patientId es requerido" });

            try
            {
                _logger.LogInformation("Consultando hospitalizaciones del paciente: {PatientId}", patientId);

                var hospitalizations = await _hospitalizationsService
                    .GetPatientHospitalizationsAsync(patientId);

                if (hospitalizations == null)
                {
                    _logger.LogWarning("No se encontraron hospitalizaciones para el paciente: {PatientId}", patientId);
                    return NotFound(new { error = "No se encontraron hospitalizaciones para este paciente" });
                }

                return Ok(hospitalizations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar hospitalizaciones del paciente {PatientId}", patientId);
                return StatusCode(500, new { error = "Error al consultar las hospitalizaciones" });
            }
        }

        /// <summary>
        /// Obtiene las hospitalizaciones de un paciente con paginación
        /// </summary>
        /// <param name="patientId">Número de documento del paciente</param>
        /// <param name="limit">Cantidad de registros por página (máximo 200)</param>
        /// <param name="offset">Número de registros a saltar</param>
        [HttpGet("patient/{patientId}/paginated")]
        [ProducesResponseType(typeof(HospitalizationsPaginatedResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPatientHospitalizationsPaginated(
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
                    "Consultando hospitalizaciones paginadas del paciente: {PatientId}, Limit={Limit}, Offset={Offset}",
                    patientId, limit, offset);

                var hospitalizations = await _hospitalizationsService
                    .GetPatientHospitalizationsPaginatedAsync(patientId, limit, offset);

                if (hospitalizations == null)
                {
                    _logger.LogWarning("No se encontraron hospitalizaciones para el paciente: {PatientId}", patientId);
                    return NotFound(new { error = "No se encontraron hospitalizaciones para este paciente" });
                }

                return Ok(hospitalizations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar hospitalizaciones paginadas del paciente {PatientId}", patientId);
                return StatusCode(500, new { error = "Error al consultar las hospitalizaciones" });
            }
        }

        /// <summary>
        /// Obtiene información de la versión de la API de Hospitalizaciones
        /// </summary>
        [HttpGet("version")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiVersionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetVersion()
        {
            try
            {
                var version = await _hospitalizationsService.GetVersionAsync();

                if (version == null)
                    return StatusCode(503, new { error = "No se pudo obtener versión de la API" });

                return Ok(version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener versión de API de hospitalizaciones");
                return StatusCode(500, new { error = "Error al consultar versión" });
            }
        }
    }
}
