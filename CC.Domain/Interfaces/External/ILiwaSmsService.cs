namespace CC.Domain.Interfaces.External
{
    /// <summary>
    /// Resultado del envío de SMS
    /// </summary>
    public record SmsResult(
        bool Success,
        string? MessageId,
        string? Status,
        string? ErrorMessage = null
    );

    /// <summary>
    /// Resultado del envío masivo de SMS
    /// </summary>
    public record SmsBulkResult(
        bool Success,
        string? CampaignId,
        int TotalSent,
        string? ErrorMessage = null
    );

    /// <summary>
    /// Servicio para envío de SMS a través de Liwa API
    /// </summary>
    public interface ILiwaSmsService
    {
        /// <summary>
        /// Envía un SMS individual
        /// </summary>
        /// <param name="phoneNumber">Número de teléfono (sin código de país)</param>
        /// <param name="message">Mensaje a enviar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado del envío</returns>
        Task<SmsResult> SendSmsAsync(
            string phoneNumber, 
            string message, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía múltiples SMS (campaña)
        /// </summary>
        /// <param name="campaignName">Nombre de la campaña</param>
        /// <param name="messages">Diccionario de número ? mensaje</param>
        /// <param name="scheduledDate">Fecha programada (null = inmediato)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Resultado del envío masivo</returns>
        Task<SmsBulkResult> SendBulkSmsAsync(
            string campaignName,
            Dictionary<string, string> messages,
            DateTime? scheduledDate = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica que el servicio esté disponible (autenticación)
        /// </summary>
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    }
}
