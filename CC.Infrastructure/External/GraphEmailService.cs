using Azure.Core;
using Azure.Identity;
using CC.Domain.Interfaces.External;
using CC.Infrastructure.External.Base;
using CC.Infrastructure.External.Email;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

namespace CC.Infrastructure.External
{
    /// <summary>
    /// Servicio para envío de emails a través de Microsoft Graph con OAuth 2.0
    /// </summary>
    public class GraphEmailService : ExternalServiceBase<EmailServiceOptions>, IGraphEmailService
    {
        private readonly IMemoryCache _cache;
        private readonly GraphServiceClient _graphClient;
        private const string TokenCacheKey = "GraphApiAccessToken";

        public GraphEmailService(
            HttpClient httpClient,
            IOptions<EmailServiceOptions> options,
            ILogger<GraphEmailService> logger,
            IMemoryCache cache)
            : base(httpClient, options.Value, logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            if (!Options.IsValid())
            {
                throw new InvalidOperationException(
                    $"Configuración inválida para GraphEmailService: {Options.GetValidationErrors()}");
            }

            // Crear GraphServiceClient con autenticación
            var credential = new ClientSecretCredential(
                Options.TenantId,
                Options.ClientId,
                Options.ClientSecret);

            _graphClient = new GraphServiceClient(credential, Options.Scopes);

            Logger.LogInformation(
                "GraphEmailService inicializado correctamente. TenantId: {TenantId}, ClientId: {ClientId}",
                Options.TenantId, MaskClientId(Options.ClientId));
        }

        /// <summary>
        /// Envía un email simple
        /// </summary>
        public async Task<EmailResult> SendEmailAsync(
            string to,
            string subject,
            string body,
            bool isHtml = true,
            CancellationToken cancellationToken = default)
        {
            return await SendEmailAsync(
                new List<string> { to },
                subject,
                body,
                isHtml,
                null,
                null,
                cancellationToken);
        }

        /// <summary>
        /// Envía un email con múltiples destinatarios
        /// </summary>
        public async Task<EmailResult> SendEmailAsync(
            List<string> toRecipients,
            string subject,
            string body,
            bool isHtml = true,
            List<string>? ccRecipients = null,
            List<string>? bccRecipients = null,
            CancellationToken cancellationToken = default)
        {
            if (toRecipients == null || !toRecipients.Any())
                throw new ArgumentException("Debe haber al menos un destinatario", nameof(toRecipients));

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Asunto es requerido", nameof(subject));

            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentException("Cuerpo del mensaje es requerido", nameof(body));

            Logger.LogInformation(
                "Enviando email a {Count} destinatarios, Asunto: '{Subject}'",
                toRecipients.Count, subject);

            try
            {
                var message = new Message
                {
                    Subject = subject,
                    Body = new ItemBody
                    {
                        ContentType = isHtml ? BodyType.Html : BodyType.Text,
                        Content = body
                    },
                    ToRecipients = toRecipients.Select(email => new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = email
                        }
                    }).ToList()
                };

                // Agregar CC si existen
                if (ccRecipients != null && ccRecipients.Any())
                {
                    message.CcRecipients = ccRecipients.Select(email => new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = email
                        }
                    }).ToList();
                }

                // Agregar BCC si existen
                if (bccRecipients != null && bccRecipients.Any())
                {
                    message.BccRecipients = bccRecipients.Select(email => new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = email
                        }
                    }).ToList();
                }

                var sendMailBody = new SendMailPostRequestBody
                {
                    Message = message,
                    SaveToSentItems = true
                };

                await _graphClient.Users[Options.SendFromUserId]
                    .SendMail
                    .PostAsync(sendMailBody, cancellationToken: cancellationToken);

                Logger.LogInformation(
                    "Email enviado exitosamente a {Count} destinatarios",
                    toRecipients.Count);

                return new EmailResult(true, null);
            }
            catch (ServiceException ex)
            {
                Logger.LogError(ex,
                    "Error de Microsoft Graph al enviar email: {Code} - {Message}",
                    ex.ResponseStatusCode, ex.Message);

                return new EmailResult(false, $"Graph API Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Excepción al enviar email");
                return new EmailResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Envía un email con archivos adjuntos
        /// </summary>
        public async Task<EmailResult> SendEmailWithAttachmentsAsync(
            string to,
            string subject,
            string body,
            List<EmailAttachment> attachments,
            bool isHtml = true,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Destinatario es requerido", nameof(to));

            if (attachments == null || !attachments.Any())
                throw new ArgumentException("Debe haber al menos un adjunto", nameof(attachments));

            Logger.LogInformation(
                "Enviando email con {Count} adjuntos a {To}, Asunto: '{Subject}'",
                attachments.Count, MaskEmail(to), subject);

            try
            {
                var message = new Message
                {
                    Subject = subject,
                    Body = new ItemBody
                    {
                        ContentType = isHtml ? BodyType.Html : BodyType.Text,
                        Content = body
                    },
                    ToRecipients = new List<Recipient>
                    {
                        new Recipient
                        {
                            EmailAddress = new EmailAddress
                            {
                                Address = to
                            }
                        }
                    },
                    Attachments = attachments.Select(att => new FileAttachment
                    {
                        Name = att.FileName,
                        ContentType = att.ContentType ?? "application/octet-stream",
                        ContentBytes = att.Content
                    } as Attachment).ToList()
                };

                var sendMailBody = new SendMailPostRequestBody
                {
                    Message = message,
                    SaveToSentItems = true
                };

                await _graphClient.Users[Options.SendFromUserId]
                    .SendMail
                    .PostAsync(sendMailBody, cancellationToken: cancellationToken);

                Logger.LogInformation(
                    "Email con adjuntos enviado exitosamente a {To}",
                    MaskEmail(to));

                return new EmailResult(true, null);
            }
            catch (ServiceException ex)
            {
                Logger.LogError(ex,
                    "Error de Microsoft Graph al enviar email con adjuntos: {Code} - {Message}",
                    ex.ResponseStatusCode, ex.Message);

                return new EmailResult(false, $"Graph API Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Excepción al enviar email con adjuntos");
                return new EmailResult(false, ex.Message);
            }
        }

        /// <summary>
        /// Verifica disponibilidad del servicio
        /// </summary>
        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Intentar obtener perfil del usuario para verificar conectividad
                var user = await _graphClient.Users[Options.SendFromUserId]
                    .GetAsync(cancellationToken: cancellationToken);

                Logger.LogInformation(
                    "GraphEmailService disponible. Usuario: {Email}",
                    MaskEmail(user?.Mail ?? user?.UserPrincipalName ?? "N/A"));

                return user != null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al verificar disponibilidad de GraphEmailService");
                return false;
            }
        }

        /// <summary>
        /// Enmascara email para logging
        /// </summary>
        private static string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                return "****@****.***";

            var parts = email.Split('@');
            var localPart = parts[0];
            var domainPart = parts[1];

            var maskedLocal = localPart.Length > 2
                ? localPart[..2] + "****"
                : "****";

            var domainParts = domainPart.Split('.');
            var maskedDomain = domainParts.Length > 1
                ? "****." + domainParts[^1]
                : "****";

            return $"{maskedLocal}@{maskedDomain}";
        }

        /// <summary>
        /// Enmascara ClientId para logging
        /// </summary>
        private static string MaskClientId(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId) || clientId.Length < 8)
                return "****-****";

            return clientId[..4] + "****" + clientId[^4..];
        }
    }
}
