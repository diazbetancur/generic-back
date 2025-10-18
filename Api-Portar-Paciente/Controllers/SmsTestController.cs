using CC.Domain.Interfaces.External;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    /// <summary>
    /// Controlador para testing del servicio de SMS (Solo para desarrollo/QA)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SmsTestController : ControllerBase
    {
        private readonly ILiwaSmsService _smsService;
        private readonly ILogger<SmsTestController> _logger;

        public SmsTestController(ILiwaSmsService smsService, ILogger<SmsTestController> logger)
        {
            _smsService = smsService;
            _logger = logger;
        }

        /// <summary>
        /// Envía un SMS de prueba
        /// </summary>
        /// <param name="phoneNumber">Número de teléfono (sin código de país)</param>
        /// <param name="message">Mensaje a enviar</param>
        /// <returns>Resultado del envío</returns>
        [HttpPost("send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendTestSms(
            [FromQuery] string phoneNumber,
            [FromQuery] string message)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return BadRequest("Número de teléfono es requerido");

            if (string.IsNullOrWhiteSpace(message))
                return BadRequest("Mensaje es requerido");

            _logger.LogInformation("Testing SMS send to {Phone}", MaskPhone(phoneNumber));

            var result = await _smsService.SendSmsAsync(phoneNumber, message);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    messageId = result.MessageId,
                    status = result.Status,
                    message = "SMS enviado exitosamente"
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
        /// Envía múltiples SMS de prueba
        /// </summary>
        /// <param name="request">Datos de la campaña</param>
        /// <returns>Resultado del envío masivo</returns>
        [HttpPost("send-bulk")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendBulkSms([FromBody] BulkSmsRequest request)
        {
            if (request?.Messages == null || !request.Messages.Any())
                return BadRequest("Debe proporcionar al menos un mensaje");

            _logger.LogInformation("Testing bulk SMS send: {Count} messages", request.Messages.Count);

            var result = await _smsService.SendBulkSmsAsync(
                request.CampaignName ?? "Test Campaign",
                request.Messages,
                request.ScheduledDate);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    campaignId = result.CampaignId,
                    totalSent = result.TotalSent,
                    message = "Campaña enviada exitosamente"
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
        /// Verifica disponibilidad del servicio de SMS
        /// </summary>
        /// <returns>Estado del servicio</returns>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckHealth()
        {
            var isAvailable = await _smsService.IsAvailableAsync();

            return Ok(new
            {
                service = "LiwaSmsService",
                available = isAvailable,
                timestamp = DateTime.UtcNow
            });
        }

        private static string MaskPhone(string phone)
        {
            if (phone.Length <= 4) return "****";
            return phone[..^4] + "****";
        }
    }

    /// <summary>
    /// Request para envío masivo de SMS
    /// </summary>
    public class BulkSmsRequest
    {
        /// <summary>
        /// Nombre de la campaña
        /// </summary>
        public string? CampaignName { get; set; }

        /// <summary>
        /// Diccionario de número ? mensaje
        /// </summary>
        public Dictionary<string, string> Messages { get; set; }

        /// <summary>
        /// Fecha programada (opcional, null = inmediato)
        /// </summary>
        public DateTime? ScheduledDate { get; set; }
    }
}