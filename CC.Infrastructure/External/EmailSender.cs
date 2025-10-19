using CC.Domain.Interfaces.External;
using Microsoft.Extensions.Logging;

namespace CC.Infrastructure.External
{
    /// <summary>
    /// Adaptador para mantener compatibilidad con IEmailSender legacy usando GraphEmailService
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly IGraphEmailService _graphEmailService;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IGraphEmailService graphEmailService, ILogger<EmailSender> logger)
        {
            _graphEmailService = graphEmailService ?? throw new ArgumentNullException(nameof(graphEmailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendAsync(string destination, string subject, string message, CancellationToken ct = default)
        {
            try
            {
                var result = await _graphEmailService.SendEmailAsync(
                    destination,
                    subject,
                    message,
                    isHtml: true,
                    cancellationToken: ct);

                if (!result.Success)
                {
                    throw new InvalidOperationException($"Error al enviar email: {result.ErrorMessage}");
                }

                _logger.LogInformation(
                    "EmailSender: Email enviado exitosamente a {Destination}",
                    MaskEmail(destination));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmailSender: Excepción al enviar email a {Destination}", MaskEmail(destination));
            }
        }

        private static string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                return "****@****.***";

            var parts = email.Split('@');
            return $"{parts[0][..2]}****@****.{parts[1].Split('.')[^1]}";
        }
    }
}