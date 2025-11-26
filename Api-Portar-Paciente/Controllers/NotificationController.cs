using CC.Domain.Dtos;
using CC.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    /// <summary>
    /// Controlador para gestión de preferencias de notificación de pacientes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IDocTypeService _docTypeService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            IDocTypeService docTypeService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _docTypeService = docTypeService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene las preferencias de notificación de un paciente
        /// GET api/Notification/patient/{docTypeCode}/{docNumber}
        /// </summary>
        /// <param name="docTypeCode">Código del tipo de documento (ej: CC, TI)</param>
        /// <param name="docNumber">Número de documento</param>
        /// <returns>Preferencias de notificación del paciente</returns>
        [HttpGet("patient/{docTypeCode}/{docNumber}")]
        [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByPatientDocument(string docTypeCode, string docNumber)
        {
            if (string.IsNullOrWhiteSpace(docTypeCode))
                return BadRequest(new { error = "docTypeCode es requerido" });

            if (string.IsNullOrWhiteSpace(docNumber))
                return BadRequest(new { error = "docNumber es requerido" });

            try
            {
                _logger.LogInformation(
                    "Consultando preferencias de notificación: {DocType}-{DocNumber}",
                    docTypeCode, docNumber);

                // Obtener el DocType por código usando el servicio
                var docTypes = await _docTypeService.GetAllAsync(dt => dt.Code == docTypeCode);
                var docType = docTypes.FirstOrDefault();

                if (docType == null)
                {
                    _logger.LogWarning("Tipo de documento no encontrado: {DocTypeCode}", docTypeCode);
                    return NotFound(new { error = "Tipo de documento no encontrado" });
                }

                // Buscar las preferencias usando GetAllAsync del ServiceBase
                var notifications = await _notificationService.GetAllAsync(
                    n => n.DocTypeId == docType.Id && n.DocNumber == docNumber);

                var preference = notifications.FirstOrDefault();

                if (preference == null)
                {
                    _logger.LogInformation(
                        "No se encontraron preferencias para: {DocType}-{DocNumber}",
                        docTypeCode, docNumber);
                    return NotFound(new { error = "No se encontraron preferencias de notificación" });
                }

                // Agregar el DocTypeCode al DTO
                preference.DocTypeCode = docTypeCode;

                return Ok(preference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al consultar preferencias de notificación: {DocType}-{DocNumber}",
                    docTypeCode, docNumber);
                return StatusCode(500, new { error = "Error al consultar preferencias" });
            }
        }

        /// <summary>
        /// Crea o actualiza las preferencias de notificación de un paciente
        /// POST api/Notification
        /// </summary>
        /// <param name="dto">Preferencias de notificación</param>
        /// <returns>Preferencias creadas o actualizadas</returns>
        [HttpPost]
        [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Upsert([FromBody] NotificationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.DocNumber))
                return BadRequest(new { error = "DocNumber es requerido" });

            try
            {
                var docType = await _docTypeService.GetAllAsync(dt => dt.Code == dto.DocTypeCode);
                _logger.LogInformation(
                    "Guardando preferencias de notificación: {DocTypeId}-{DocNumber}",
                    dto.DocTypeCode, dto.DocNumber);

                var existingNotifications = await _notificationService.GetAllAsync(
                    n => n.DocTypeId == docType.FirstOrDefault().Id && n.DocNumber == dto.DocNumber);

                var existing = existingNotifications.FirstOrDefault();

                NotificationDto result;

                if (existing != null)
                {
                    existing.Email = dto.Email;
                    existing.SMS = dto.SMS;
                    existing.NoReceiveNotifications = dto.NoReceiveNotifications;

                    await _notificationService.UpdateAsync(existing);
                    result = existing;

                    _logger.LogInformation(
                        "Preferencias actualizadas exitosamente: {DocTypeId}-{DocNumber}",
                        dto.DocTypeId, dto.DocNumber);
                }
                else
                {
                    dto.DocTypeId = docType.FirstOrDefault().Id;
                    result = await _notificationService.AddAsync(dto);

                    _logger.LogInformation(
                        "Preferencias creadas exitosamente: {DocTypeId}-{DocNumber}",
                        dto.DocTypeId, dto.DocNumber);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al guardar preferencias de notificación: {DocTypeId}-{DocNumber}",
                    dto.DocTypeId, dto.DocNumber);
                return StatusCode(500, new { error = "Error al guardar preferencias" });
            }
        }
    }
}