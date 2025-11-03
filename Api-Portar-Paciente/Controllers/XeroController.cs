using CC.Domain.Interfaces.External;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    /// <summary>
    /// Controlador para testing del servicio de Xero Viewer (Solo para desarrollo/QA)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class XeroController : ControllerBase
    {
        private readonly IXeroViewerService _xeroService;
        private readonly ILogger<XeroController> _logger;

        public XeroController(IXeroViewerService xeroService, ILogger<XeroController> logger)
        {
            _xeroService = xeroService;
            _logger = logger;
        }

        /// <summary>
        /// Verifica disponibilidad del servicio de Xero Viewer
        /// </summary>
        /// <returns>Estado del servicio</returns>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckHealth()
        {
            var isAvailable = await _xeroService.IsAvailableAsync();

            return Ok(new
            {
                service = "XeroViewerService",
                available = isAvailable,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Obtiene la lista de estudios de un paciente
        /// </summary>
        /// <param name="patientId">ID del paciente</param>
        /// <param name="limit">Límite de estudios por página (1-50)</param>
        /// <param name="offset">Offset para paginación</param>
        /// <returns>Lista de estudios</returns>
        [HttpGet("patients/{patientId}/studies")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPatientStudies(
            string patientId,
            [FromQuery] int limit = 10,
            [FromQuery] int offset = 0)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                return BadRequest("Patient ID es requerido");

            _logger.LogInformation("Testing GetPatientStudies for patient {PatientId}", patientId);

            var result = await _xeroService.GetPatientStudiesAsync(patientId, limit, offset);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    patientId = result.PatientId,
                    offset = result.Offset,
                    limit = result.Limit,
                    count = result.Count,
                    total = result.Total,
                    nextOffset = result.NextOffset,
                    studies = result.Studies?.Select(s => new
                    {
                        uid = s.Uid,
                        description = s.Description,
                        accession = s.Accession,
                        dateTime = s.DateTime
                    })
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = result.ErrorMessage
                });
            }
        }

        /// <summary>
        /// Genera un enlace temporal para visualizar un estudio
        /// </summary>
        /// <param name="studyUid">UID del estudio</param>
        /// <param name="patientId">ID del paciente (opcional, para auditoría)</param>
        /// <returns>Token y URL del visor</returns>
        [HttpPost("studies/{studyUid}/viewer-link")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenerateViewerLink(
            string studyUid,
            [FromQuery] string? patientId = null)
        {
            if (string.IsNullOrWhiteSpace(studyUid))
                return BadRequest("Study UID es requerido");

            _logger.LogInformation("Testing GenerateViewerLink for study {StudyUid}", studyUid);

            var result = await _xeroService.GenerateViewerLinkAsync(studyUid, patientId);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    token = result.Token,
                    viewerUrl = result.ViewerUrl,
                    expiresAt = result.ExpiresAt
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = result.ErrorMessage
                });
            }
        }

        /// <summary>
        /// Flujo completo: obtener estudios y generar enlace del primero
        /// </summary>
        /// <param name="patientId">ID del paciente</param>
        /// <returns>Lista de estudios con enlace del primero</returns>
        [HttpGet("patients/{patientId}/studies-with-viewer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetStudiesWithViewer(string patientId)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                return BadRequest("Patient ID es requerido");

            _logger.LogInformation("Testing complete flow for patient {PatientId}", patientId);

            // 1. Obtener estudios
            var studiesResult = await _xeroService.GetPatientStudiesAsync(patientId, limit: 10);

            if (!studiesResult.Success)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = studiesResult.ErrorMessage
                });
            }

            if (studiesResult.Studies == null || !studiesResult.Studies.Any())
            {
                return Ok(new
                {
                    success = true,
                    message = "No se encontraron estudios para este paciente",
                    patientId,
                    studies = new List<object>()
                });
            }

            // 2. Generar enlace del primer estudio
            var firstStudy = studiesResult.Studies.First();
            var viewerLinkResult = await _xeroService.GenerateViewerLinkAsync(
                firstStudy.Uid!,
                patientId);

            return Ok(new
            {
                success = true,
                patientId = studiesResult.PatientId,
                total = studiesResult.Total,
                studies = studiesResult.Studies.Select(s => new
                {
                    uid = s.Uid,
                    description = s.Description,
                    accession = s.Accession,
                    dateTime = s.DateTime
                }),
                firstStudyViewer = viewerLinkResult.Success ? new
                {
                    studyUid = firstStudy.Uid,
                    token = viewerLinkResult.Token,
                    viewerUrl = viewerLinkResult.ViewerUrl,
                    expiresAt = viewerLinkResult.ExpiresAt
                } : null
            });
        }
    }
}