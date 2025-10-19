namespace CC.Domain.Interfaces.External
{
    /// <summary>
    /// Resultado del envío de email
    /// </summary>
    public record EmailResult(
        bool Success,
        string? ErrorMessage = null,
        string? MessageId = null
    );

    /// <summary>
    /// Archivo adjunto para email
    /// </summary>
    public record EmailAttachment(
        string FileName,
        byte[] Content,
        string? ContentType = null
    );

    /// <summary>
    /// Servicio para envío de emails a través de Microsoft Graph
    /// </summary>
    public interface IGraphEmailService
    {
        /// <summary>
        /// Envía un email simple
        /// </summary>
        /// <param name="to">Destinatario</param>
        /// <param name="subject">Asunto</param>
        /// <param name="body">Cuerpo del mensaje</param>
        /// <param name="isHtml">Si el cuerpo es HTML</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado del envío</returns>
        Task<EmailResult> SendEmailAsync(
            string to,
            string subject,
            string body,
            bool isHtml = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía un email con múltiples destinatarios
        /// </summary>
        /// <param name="toRecipients">Lista de destinatarios</param>
        /// <param name="subject">Asunto</param>
        /// <param name="body">Cuerpo del mensaje</param>
        /// <param name="isHtml">Si el cuerpo es HTML</param>
        /// <param name="ccRecipients">Destinatarios en CC</param>
        /// <param name="bccRecipients">Destinatarios en BCC</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado del envío</returns>
        Task<EmailResult> SendEmailAsync(
            List<string> toRecipients,
            string subject,
            string body,
            bool isHtml = true,
            List<string>? ccRecipients = null,
            List<string>? bccRecipients = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía un email con archivos adjuntos
        /// </summary>
        /// <param name="to">Destinatario</param>
        /// <param name="subject">Asunto</param>
        /// <param name="body">Cuerpo del mensaje</param>
        /// <param name="attachments">Archivos adjuntos</param>
        /// <param name="isHtml">Si el cuerpo es HTML</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado del envío</returns>
        Task<EmailResult> SendEmailWithAttachmentsAsync(
            string to,
            string subject,
            string body,
            List<EmailAttachment> attachments,
            bool isHtml = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica que el servicio esté disponible
        /// </summary>
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    }
}
