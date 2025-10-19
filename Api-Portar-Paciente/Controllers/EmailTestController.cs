using CC.Domain.Interfaces.External;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Portar_Paciente.Controllers
{
    /// <summary>
    /// Controlador para testing del servicio de Email (Solo para desarrollo/QA)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EmailTestController : ControllerBase
    {
        private readonly IGraphEmailService _emailService;
        private readonly ILogger<EmailTestController> _logger;

        public EmailTestController(IGraphEmailService emailService, ILogger<EmailTestController> logger)
        {
            _emailService = emailService;
            _logger = logger;
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

            var result = await _emailService.SendEmailAsync(to, subject, body, isHtml);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    message = "Email enviado exitosamente"
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

            var htmlBody = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px; }
        .container { background-color: white; padding: 30px; border-radius: 10px; max-width: 600px; margin: 0 auto; }
        .header { background-color: #0066cc; color: white; padding: 20px; border-radius: 5px; text-align: center; }
        .content { padding: 20px; color: #333; }
        .footer { text-align: center; color: #666; font-size: 12px; margin-top: 20px; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Portal Pacientes Cardioinfantil</h1>
        </div>
        <div class='content'>
            <h2>Email de Prueba</h2>
            <p>Estimado paciente,</p>
            <p>Este es un email de prueba del sistema Portal Pacientes.</p>
            <p>Si recibió este correo, significa que el servicio de envío de emails está funcionando correctamente.</p>
            <p><strong>Fecha:</strong> " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + @"</p>
        </div>
        <div class='footer'>
            <p>Fundación Cardioinfantil - Portal Pacientes</p>
            <p>Este es un correo automático, por favor no responder.</p>
        </div>
    </div>
</body>
</html>";

            var result = await _emailService.SendEmailAsync(
                to,
                "Prueba Portal Pacientes - Email Template",
                htmlBody,
                isHtml: true);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    message = "Email template enviado exitosamente"
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
        /// Envía un email con múltiples destinatarios
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

            var result = await _emailService.SendEmailAsync(
                request.ToRecipients,
                request.Subject ?? "Email de prueba",
                request.Body ?? "Contenido de prueba",
                request.IsHtml,
                request.CcRecipients,
                request.BccRecipients);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    message = $"Email enviado a {request.ToRecipients.Count} destinatarios"
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
        /// Verifica disponibilidad del servicio de Email
        /// </summary>
        /// <returns>Estado del servicio</returns>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckHealth()
        {
            var isAvailable = await _emailService.IsAvailableAsync();

            return Ok(new
            {
                service = "GraphEmailService",
                available = isAvailable,
                timestamp = DateTime.UtcNow
            });
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
        /// Destinatarios en CC (opcional)
        /// </summary>
        public List<string>? CcRecipients { get; set; }

        /// <summary>
        /// Destinatarios en BCC (opcional)
        /// </summary>
        public List<string>? BccRecipients { get; set; }
    }
}