using CC.Domain.Contracts;
using CC.Domain.Interfaces.External;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    /// <summary>
    /// Controlador para servicios de NilRead (Informes PDF e Imágenes Diagnósticas)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class NilReadController : ControllerBase
    {
        private readonly INilReadService _nilReadService;
        private readonly ILogger<NilReadController> _logger;

        public NilReadController(INilReadService nilReadService, ILogger<NilReadController> logger)
        {
            _nilReadService = nilReadService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la lista paginada de exámenes de un paciente
        /// </summary>
        /// <param name="patientId">ID del paciente (ej: CC1014306921)</param>
        /// <param name="limit">Cantidad de registros a retornar (máx 50, default 10)</param>
        /// <param name="offset">Posición inicial para paginación (default 0)</param>
        /// <returns>Lista de exámenes con indicadores de informe/imágenes</returns>
        [HttpGet("patients/{patientId}/exams")]
        [ProducesResponseType(typeof(NilReadExamsResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPatientExams(
            string patientId,
            [FromQuery] int limit = 10,
            [FromQuery] int offset = 0)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                return BadRequest(new { error = "patientId es requerido" });

            if (limit < 1 || limit > 50)
                return BadRequest(new { error = "limit debe estar entre 1 y 50" });

            if (offset < 0)
                return BadRequest(new { error = "offset debe ser mayor o igual a 0" });

            try
            {
                var result = await _nilReadService.GetPatientExamsAsync(patientId, limit, offset);

                if (!result.Success)
                {
                    _logger.LogWarning("Error al obtener exámenes de paciente {PatientId}: {Error}",
                        patientId, result.ErrorMessage);
                    return StatusCode(500, new { error = result.ErrorMessage });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al obtener exámenes de paciente {PatientId}", patientId);
                return StatusCode(500, new { error = "Error interno al consultar exámenes" });
            }
        }

        /// <summary>
        /// Descarga el PDF de un informe
        /// </summary>
        /// <param name="dateFolder">Fecha en formato yyyyMMdd (ej: 20220503)</param>
        /// <param name="filename">Nombre del archivo PDF (ej: EXUS000157415.pdf)</param>
        /// <returns>Archivo PDF del informe</returns>
        [HttpGet("reports/{dateFolder}/{filename}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetReport(string dateFolder, string filename)
        {
            if (string.IsNullOrWhiteSpace(dateFolder) || string.IsNullOrWhiteSpace(filename))
                return BadRequest(new { error = "dateFolder y filename son requeridos" });

            // Validar formato de fecha
            if (!System.Text.RegularExpressions.Regex.IsMatch(dateFolder, @"^\d{8}$"))
                return BadRequest(new { error = "dateFolder debe tener formato yyyyMMdd" });

            // Validar extensión PDF
            if (!filename.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "Solo se permiten archivos PDF" });

            try
            {
                var result = await _nilReadService.GetReportPdfAsync(dateFolder, filename);

                if (!result.Success)
                {
                    if (result.ErrorMessage?.Contains("no encontrado") == true ||
                        result.ErrorMessage?.Contains("not found") == true)
                    {
                        return NotFound(new { error = result.ErrorMessage });
                    }

                    _logger.LogWarning("Error al obtener informe {DateFolder}/{Filename}: {Error}",
                        dateFolder, filename, result.ErrorMessage);
                    return StatusCode(500, new { error = result.ErrorMessage });
                }

                if (result.PdfContent == null || result.PdfContent.Length == 0)
                {
                    return NotFound(new { error = "Informe no encontrado o vacío" });
                }

                return File(result.PdfContent, "application/pdf", filename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al obtener informe {DateFolder}/{Filename}", dateFolder, filename);
                return StatusCode(500, new { error = "Error interno al obtener el informe" });
            }
        }

        /// <summary>
        /// Genera un enlace temporal para visualizar imágenes de un examen en NilRead Viewer
        /// </summary>
        /// <param name="accession">Número de accesión del examen (ej: EXUS000157415)</param>
        /// <param name="request">Datos necesarios para generar el enlace</param>
        /// <returns>URL del visor NilRead con token temporal</returns>
        [HttpPost("exams/{accession}/viewer-link")]
        [ProducesResponseType(typeof(NilReadViewerLinkResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateViewerLink(
            string accession,
            [FromBody] NilReadViewerLinkRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(accession))
                return BadRequest(new { error = "accession es requerido" });

            if (request == null)
                return BadRequest(new { error = "request body es requerido" });

            if (string.IsNullOrWhiteSpace(request.PatientID))
                return BadRequest(new { error = "patientID es requerido" });

            if (request.AccNumbers == null || !request.AccNumbers.Any())
                return BadRequest(new { error = "accNumbers debe contener al menos un elemento" });

            try
            {
                var result = await _nilReadService.GenerateViewerLinkAsync(
                    accession,
                    request.PatientID,
                    request.DateTime,
                    request.AccNumbers);

                if (!result.Success)
                {
                    _logger.LogWarning("Error al generar enlace del visor para {Accession}: {Error}",
                        accession, result.ErrorMessage);
                    return StatusCode(500, new { error = result.ErrorMessage });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al generar enlace del visor para {Accession}", accession);
                return StatusCode(500, new { error = "Error interno al generar enlace del visor" });
            }
        }

        /// <summary>
        /// Verifica el estado del servicio NilRead
        /// </summary>
        /// <returns>Estado del servicio</returns>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> CheckHealth()
        {
            try
            {
                var isAvailable = await _nilReadService.IsAvailableAsync();

                if (isAvailable)
                {
                    return Ok(new
                    {
                        service = "NilReadService",
                        status = "available",
                        timestamp = DateTime.UtcNow
                    });
                }

                return StatusCode(503, new
                {
                    service = "NilReadService",
                    status = "unavailable",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar health de NilRead");
                return StatusCode(503, new
                {
                    service = "NilReadService",
                    status = "error",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }

    /// <summary>
    /// DTO para request de generación de enlace del visor
    /// </summary>
    public class NilReadViewerLinkRequestDto
    {
        /// <summary>
        /// ID del paciente (con prefijo de tipo documento, ej: CC1014306921)
        /// </summary>
        public string PatientID { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp en formato YYYYMMDDHHmmss (opcional)
        /// </summary>
        public string? DateTime { get; set; }

        /// <summary>
        /// Lista de números de accesión (ej: ["EXUS000157415"])
        /// </summary>
        public List<string> AccNumbers { get; set; } = new();
    }
}
