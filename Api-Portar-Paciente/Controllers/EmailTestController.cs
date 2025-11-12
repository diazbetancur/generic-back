using CC.Domain.Interfaces.External;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    /// <summary>
    /// Controlador para testing del servicio de Email (Solo para desarrollo/QA)
    /// Usa IEmailSender para soportar tanto Graph como SMTP según configuración.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EmailTestController : ControllerBase
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<EmailTestController> _logger;
        private readonly IConfiguration _configuration;

        public EmailTestController(IEmailSender emailSender, ILogger<EmailTestController> logger, IConfiguration configuration)
        {
            _emailSender = emailSender;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Envía un email de prueba
        /// </summary>
        /// <param name="to">Destinatario</param>
        /// <param name="subject">Asunto</param>
        /// <param name="body">Cuerpo del mensaje</param>
        /// <param name="isHtml">Si el cuerpo es HTML (default: true)</param>
        /// <returns>Resultado del envío</returns>
        [HttpPost("send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendTestEmail(
            [FromQuery] string to,
            [FromQuery] string subject,
            [FromQuery] string body,
            [FromQuery] bool isHtml = true)
        {
            if (string.IsNullOrWhiteSpace(to))
                return BadRequest("Destinatario es requerido");

            if (string.IsNullOrWhiteSpace(subject))
                return BadRequest("Asunto es requerido");

            if (string.IsNullOrWhiteSpace(body))
                return BadRequest("Cuerpo del mensaje es requerido");

            _logger.LogInformation("Testing email send to {To}", MaskEmail(to));

            try
            {
                await _emailSender.SendAsync(to, subject, body);
                return Ok(new { success = true, message = "Email enviado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email de prueba a {To}", MaskEmail(to));
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Envía un email HTML de prueba con template
        /// </summary>
        /// <param name="to">Destinatario</param>
        /// <returns>Resultado del envío</returns>
        [HttpPost("send-template")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendTemplateEmail([FromQuery] string to)
        {
            if (string.IsNullOrWhiteSpace(to))
                return BadRequest("Destinatario es requerido");

            var htmlBody = @"\n<!DOCTYPE html>\n<html>\n<head>\n    <style>\n        body { font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px; }\n        .container { background-color: white; padding: 30px; border-radius: 10px; max-width: 600px; margin: 0 auto; }\n        .header { background-color: #0066cc; color: white; padding: 20px; border-radius: 5px; text-align: center; }\n        .content { padding: 20px; color: #333; }\n        .footer { text-align: center; color: #666; font-size: 12px; margin-top: 20px; }\n    </style>\n</head>\n<body>\n    <div class='container'>\n        <div class='header'>\n            <h1>Portal Pacientes Cardioinfantil</h1>\n        </div>\n        <div class='content'>\n            <h2>Email de Prueba</h2>\n            <p>Estimado paciente,</p>\n            <p>Este es un email de prueba del sistema Portal Pacientes.</p>\n            <p>Si recibió este correo, significa que el servicio de envío de emails está funcionando correctamente.</p>\n            <p><strong>Fecha:</strong> " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + @"</p>\n        </div>\n        <div class='footer'>\n            <p>Fundación Cardioinfantil - Portal Pacientes</p>\n            <p>Este es un correo automático, por favor no responder.</p>\n        </div>\n    </div>\n</body>\n</html>";

            try
            {
                await _emailSender.SendAsync(to, "Prueba Portal Pacientes - Email Template", htmlBody);
                return Ok(new { success = true, message = "Email template enviado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email template a {To}", MaskEmail(to));
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Envía un email con múltiples destinatarios (secuencial)
        /// </summary>
        /// <param name="request">Datos del email</param>
        /// <returns>Resultado del envío</returns>
        [HttpPost("send-multiple")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendMultipleEmail([FromBody] MultipleEmailRequest request)
        {
            if (request?.ToRecipients == null || !request.ToRecipients.Any())
                return BadRequest("Debe proporcionar al menos un destinatario");

            _logger.LogInformation("Testing multiple email send to {Count} recipients", request.ToRecipients.Count);

            var errors = new List<string>();
            foreach (var to in request.ToRecipients)
            {
                try
                {
                    await _emailSender.SendAsync(to, request.Subject ?? "Email de prueba", request.Body ?? "Contenido de prueba");
                }
                catch (Exception ex)
                {
                    errors.Add($"{to}: {ex.Message}");
                }
            }

            if (errors.Count == 0)
            {
                return Ok(new { success = true, message = $"Email enviado a {request.ToRecipients.Count} destinatarios" });
            }
            else
            {
                return StatusCode(500, new { success = false, message = "Errores al enviar", errors });
            }
        }

        /// <summary>
        /// Muestra el modo de envío configurado (Graph o Smtp)
        /// </summary>
        [HttpGet("mode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetMode()
        {
            var mode = _configuration["Email:Mode"] ?? "Graph";
            return Ok(new { mode, timestamp = DateTime.UtcNow });
        }

        private static string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                return "****@****.***";

            var parts = email.Split('@');
            return $"{parts[0][..2]}****@****.{parts[1].Split('.')[^1]}";
        }
    }

    /// <summary>
    /// Request para envío múltiple de email
    /// </summary>
    public class MultipleEmailRequest
    {
        /// <summary>
        /// Lista de destinatarios principales
        /// </summary>
        public List<string> ToRecipients { get; set; }

        /// <summary>
        /// Asunto del email
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Cuerpo del mensaje
        /// </summary>
        public string? Body { get; set; }

        /// <summary>
        /// Si el cuerpo es HTML
        /// </summary>
        public bool IsHtml { get; set; } = true;

        /// <summary>
        /// Destinatarios en CC (no usado en IEmailSender básico)
        /// </summary>
        public List<string>? CcRecipients { get; set; }

        /// <summary>
        /// Destinatarios en BCC (no usado en IEmailSender básico)
        /// </summary>
        public List<string>? BccRecipients { get; set; }
    }
}