using CC.Domain.Interfaces.External;
using CC.Infrastructure.External.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace CC.Infrastructure.External.Email
{
    /// <summary>
    /// Implementación SMTP básica usando System.Net.Mail para envío con usuario y contraseña.
    /// Compatible con la interfaz IEmailSender existente (htmlBody como cuerpo HTML).
    /// </summary>
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpEmailOptions _options;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<SmtpEmailOptions> options, ILogger<SmtpEmailSender> logger)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            ValidateOptions();
        }

        public async Task SendAsync(string destination, string subject, string htmlBody, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(destination)) throw new ArgumentException("Destination requerido", nameof(destination));
            if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException("Subject requerido", nameof(subject));
            if (string.IsNullOrWhiteSpace(htmlBody)) throw new ArgumentException("htmlBody requerido", nameof(htmlBody));

            using var client = CreateClient();
            using var mail = new MailMessage
            {
                From = new MailAddress(_options.FromEmail, _options.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mail.To.Add(destination);

            try
            {
                _logger.LogInformation("SMTP: Enviando email a {Dest}", MaskEmail(destination));
                await client.SendMailAsync(mail, ct).ConfigureAwait(false);
                _logger.LogInformation("SMTP: Email enviado correctamente a {Dest}", MaskEmail(destination));
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP: Error enviando email a {Dest}. StatusCode={StatusCode}", MaskEmail(destination), ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP: Excepción enviando email a {Dest}", MaskEmail(destination));
            }
        }

        private SmtpClient CreateClient()
        {
            return new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.UseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_options.Username, _options.Password),
                Timeout = _options.TimeoutSeconds * 1000
            };
        }

        private void ValidateOptions()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(_options.Host)) errors.Add("Host");
            if (string.IsNullOrWhiteSpace(_options.Username)) errors.Add("Username");
            if (string.IsNullOrWhiteSpace(_options.Password)) errors.Add("Password");
            if (string.IsNullOrWhiteSpace(_options.FromEmail)) errors.Add("FromEmail");
            if (errors.Any())
            {
                throw new InvalidOperationException($"SmtpEmailOptions inválidas. Faltan: {string.Join(", ", errors)}");
            }
        }

        private static string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) return "****@****";
            var parts = email.Split('@');
            var left = parts[0];
            return (left.Length > 2 ? left.Substring(0, 2) : left) + "****@" + parts[1];
        }
    }
}