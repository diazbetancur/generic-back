using CC.Domain.Dtos.ExternalApis;
using CC.Domain.Interfaces.External;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    /// <summary>
    /// Controlador para gestión de historia clínica de pacientes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClinicalHistoryController : ControllerBase
    {
        private readonly IClinicalHistoryApiService _clinicalHistoryService;
        private readonly ILogger<ClinicalHistoryController> _logger;

        public ClinicalHistoryController(
            IClinicalHistoryApiService clinicalHistoryService,
            ILogger<ClinicalHistoryController> logger)
        {
            _clinicalHistoryService = clinicalHistoryService;
            _logger = logger;
        }

        /// <summary>
        /// Verifica el estado de salud de la API de Historia Clínica
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiHealthDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> CheckHealth()
        {
            try
            {
                var health = await _clinicalHistoryService.CheckHealthAsync();

                if (health?.Status == "ok")
                    return Ok(health);

                return StatusCode(503, new { error = "API de historia clínica no disponible" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar salud de API de historia clínica");
                return StatusCode(503, new { error = "Error al conectar con API de historia clínica" });
            }
        }

        /// <summary>
        /// Obtiene los episodios médicos de un paciente
        /// </summary>
        /// <param name="patientId">Número de documento del paciente</param>
        /// <param name="onlyPdf">Retornar solo episodios con PDF disponible</param>
        /// <param name="limit">Límite de resultados (opcional)</param>
        /// <param name="offset">Offset para paginación (opcional)</param>
        [HttpGet("patient/{patientId}/episodes")]
        [ProducesResponseType(typeof(EpisodesResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPatientEpisodes(
            string patientId,
            [FromQuery] bool onlyPdf = false,
            [FromQuery] int? limit = null,
            [FromQuery] int? offset = null)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                return BadRequest(new { error = "patientId es requerido" });

            try
            {
                _logger.LogInformation(
                    "Consultando episodios del paciente: {PatientId}, OnlyPdf={OnlyPdf}",
                    patientId, onlyPdf);

                var episodes = await _clinicalHistoryService
                    .GetPatientEpisodesAsync(patientId, onlyPdf, limit, offset);

                if (episodes == null)
                {
                    _logger.LogWarning("No se encontraron episodios para el paciente: {PatientId}", patientId);
                    return NotFound(new { error = "No se encontraron episodios para este paciente" });
                }

                return Ok(episodes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar episodios del paciente {PatientId}", patientId);
                return StatusCode(500, new { error = "Error al consultar episodios médicos" });
            }
        }

        /// <summary>
        /// Descarga el PDF de historia clínica de un episodio específico
        /// </summary>
        /// <param name="patientId">Número de documento del paciente</param>
        /// <param name="episode">Número de episodio</param>
        [HttpGet("patient/{patientId}/episodes/{episode}/pdf")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadClinicalHistoryPdf(string patientId, string episode)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                return BadRequest(new { error = "patientId es requerido" });

            if (string.IsNullOrWhiteSpace(episode))
                return BadRequest(new { error = "episode es requerido" });

            try
            {
                _logger.LogInformation(
                    "Descargando PDF de historia clínica: PatientId={PatientId}, Episode={Episode}",
                    patientId, episode);

                var pdfStream = await _clinicalHistoryService
                    .DownloadClinicalHistoryPdfAsync(patientId, episode);

                if (pdfStream == null)
                {
                    _logger.LogWarning(
                        "PDF de historia clínica no encontrado: PatientId={PatientId}, Episode={Episode}",
                        patientId, episode);
                    return NotFound(new { error = "PDF de historia clínica no encontrado" });
                }

                return File(pdfStream, "application/pdf", $"historia_clinica_{episode}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al descargar PDF de historia clínica: PatientId={PatientId}, Episode={Episode}",
                    patientId, episode);
                return StatusCode(500, new { error = "Error al descargar el documento" });
            }
        }

        /// <summary>
        /// Descarga el documento PDF de incapacidad de un episodio
        /// </summary>
        /// <param name="patientId">Número de documento del paciente</param>
        /// <param name="episode">Número de episodio</param>
        [HttpGet("patient/{patientId}/episodes/{episode}/incapacity")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadIncapacityPdf(string patientId, string episode)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                return BadRequest(new { error = "patientId es requerido" });

            if (string.IsNullOrWhiteSpace(episode))
                return BadRequest(new { error = "episode es requerido" });

            try
            {
                _logger.LogInformation(
                    "Descargando PDF de incapacidad: PatientId={PatientId}, Episode={Episode}",
                    patientId, episode);

                var pdfStream = await _clinicalHistoryService
                    .DownloadIncapacityPdfAsync(patientId, episode);

                if (pdfStream == null)
                {
                    _logger.LogWarning(
                        "PDF de incapacidad no encontrado: PatientId={PatientId}, Episode={Episode}",
                        patientId, episode);
                    return NotFound(new { error = "Documento de incapacidad no encontrado" });
                }

                return File(pdfStream, "application/pdf", $"incapacidad_{episode}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al descargar PDF de incapacidad: PatientId={PatientId}, Episode={Episode}",
                    patientId, episode);
                return StatusCode(500, new { error = "Error al descargar el documento" });
            }
        }

        /// <summary>
        /// Obtiene información de la versión de la API de Historia Clínica
        /// </summary>
        [HttpGet("version")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiVersionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetVersion()
        {
            try
            {
                var version = await _clinicalHistoryService.GetVersionAsync();

                if (version == null)
                    return StatusCode(503, new { error = "No se pudo obtener versión de la API" });

                return Ok(version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener versión de API de historia clínica");
                return StatusCode(500, new { error = "Error al consultar versión" });
            }
        }
    }
}
