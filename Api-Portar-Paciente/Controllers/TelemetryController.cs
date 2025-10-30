using CC.Domain.Dtos;
using CC.Domain.Enums;
using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using CC.Domain.Entities;

namespace Api_Portar_Paciente.Controllers
{
    /// <summary>
    /// Controller para gestión de telemetría de la aplicación
    /// (consultas, descargas de documentos y futuras métricas)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TelemetryController : ControllerBase
    {
        private readonly ITelemetryService _telemetryService;

        public TelemetryController(ITelemetryService telemetryService)
        {
            _telemetryService = telemetryService;
        }

        /// <summary>
        /// Registra un nuevo evento de telemetría
        /// POST api/Telemetry
        /// </summary>
        /// <param name="request">Datos del evento de telemetría a registrar</param>
        /// <returns>Respuesta con el registro creado</returns>
        [HttpPost]
        public async Task<IActionResult> LogTelemetry([FromBody] TelemetryRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var telemetryDto = new TelemetryDto
                {
                    UserDocType = request.UserDocType,
                    UserDocNumber = request.UserDocNumber,
                    DocumentType = request.DocumentType,
                    ActivityType = request.ActivityType,
                    ActivityDate = DateTime.UtcNow,
                    Source = request.Source,
                    TelemetryType = request.TelemetryType ?? "DocumentActivity",
                    AdditionalData = request.AdditionalData
                };

                var response = await _telemetryService.AddAsync(telemetryDto).ConfigureAwait(false);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al registrar telemetría: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene toda la telemetría sin filtros
        /// GET api/Telemetry
        /// </summary>
        /// <returns>Lista de todos los eventos de telemetría</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllTelemetry()
        {
            var response = await _telemetryService.GetAllAsync().ConfigureAwait(false);
            return Ok(response);
        }

        /// <summary>
        /// Obtiene telemetría filtrada por parámetros
        /// GET api/Telemetry/report
        /// </summary>
        /// <param name="userDocType">Filtrar por tipo de documento de usuario</param>
        /// <param name="userDocNumber">Filtrar por número de documento de usuario</param>
        /// <param name="documentType">Filtrar por tipo de documento</param>
        /// <param name="activityType">Filtrar por tipo de actividad (1=Consulta, 2=Descarga)</param>
        /// <param name="startDate">Fecha de inicio del rango</param>
        /// <param name="endDate">Fecha final del rango</param>
        /// <param name="source">Filtrar por fuente</param>
        /// <param name="telemetryType">Filtrar por tipo de telemetría</param>
        /// <returns>Lista filtrada de eventos de telemetría</returns>
        [HttpGet("report")]
        public async Task<IActionResult> GetFilteredReport(
            [FromQuery] string? userDocType = null,
            [FromQuery] string? userDocNumber = null,
            [FromQuery] string? documentType = null,
            [FromQuery] ActivityType? activityType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? source = null,
            [FromQuery] string? telemetryType = null)
        {
            try
            {
                // Construir la expresión de filtro dinámicamente
                Expression<Func<TelemetryLog, bool>> filter = x => true;

                if (!string.IsNullOrEmpty(userDocType))
                {
                    filter = CombineExpressions(filter, x => x.UserDocType == userDocType);
                }

                if (!string.IsNullOrEmpty(userDocNumber))
                {
                    filter = CombineExpressions(filter, x => x.UserDocNumber == userDocNumber);
                }

                if (!string.IsNullOrEmpty(documentType))
                {
                    filter = CombineExpressions(filter, x => x.DocumentType == documentType);
                }

                if (activityType.HasValue)
                {
                    filter = CombineExpressions(filter, x => x.ActivityType == activityType.Value);
                }

                if (startDate.HasValue)
                {
                    filter = CombineExpressions(filter, x => x.ActivityDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    filter = CombineExpressions(filter, x => x.ActivityDate <= endDate.Value);
                }

                if (!string.IsNullOrEmpty(source))
                {
                    filter = CombineExpressions(filter, x => x.Source == source);
                }

                if (!string.IsNullOrEmpty(telemetryType))
                {
                    filter = CombineExpressions(filter, x => x.TelemetryType == telemetryType);
                }

                var response = await _telemetryService.GetAllAsync(filter).ConfigureAwait(false);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener reporte de telemetría: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene estadísticas agrupadas de telemetría
        /// GET api/Telemetry/statistics
        /// </summary>
        /// <param name="activityType">Filtrar por tipo de actividad (opcional)</param>
        /// <param name="telemetryType">Filtrar por tipo de telemetría (opcional)</param>
        /// <param name="startDate">Fecha de inicio del rango (opcional)</param>
        /// <param name="endDate">Fecha final del rango (opcional)</param>
        /// <returns>Estadísticas agrupadas</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics(
            [FromQuery] ActivityType? activityType = null,
            [FromQuery] string? telemetryType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Construir filtro para estadísticas
                Expression<Func<TelemetryLog, bool>> filter = x => true;

                if (activityType.HasValue)
                {
                    filter = CombineExpressions(filter, x => x.ActivityType == activityType.Value);
                }

                if (!string.IsNullOrEmpty(telemetryType))
                {
                    filter = CombineExpressions(filter, x => x.TelemetryType == telemetryType);
                }

                if (startDate.HasValue)
                {
                    filter = CombineExpressions(filter, x => x.ActivityDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    filter = CombineExpressions(filter, x => x.ActivityDate <= endDate.Value);
                }

                var response = await _telemetryService.GetAllAsync(filter).ConfigureAwait(false);

                if (response.Any())
                {
                    var statistics = response
                        .GroupBy(x => new { x.DocumentType, x.ActivityType, x.TelemetryType })
                        .Select(g => new
                        {
                            DocumentType = g.Key.DocumentType,
                            ActivityType = g.Key.ActivityType,
                            TelemetryType = g.Key.TelemetryType,
                            Count = g.Count(),
                            LastActivity = g.Max(x => x.ActivityDate)
                        })
                        .OrderByDescending(x => x.Count)
                        .ToList();

                    return Ok(new { Success = true, Data = statistics });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener estadísticas de telemetría: {ex.Message}");
            }
        }

        /// <summary>
        /// Método auxiliar para combinar expresiones lambda con AND
        /// </summary>
        private static Expression<Func<T, bool>> CombineExpressions<T>(
            Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));
            var body = Expression.AndAlso(
                Expression.Invoke(expr1, parameter),
                Expression.Invoke(expr2, parameter));
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }
}