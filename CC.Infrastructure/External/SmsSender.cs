using CC.Domain.Interfaces.External;
using Microsoft.Extensions.Logging;

namespace CC.Infrastructure.External
{
    /// <summary>
    /// Adaptador para mantener compatibilidad con ISmsSender legacy usando LiwaSmsService
    /// </summary>
    public class SmsSender : ISmsSender
    {
        private readonly ILiwaSmsService _liwaSmsService;
        private readonly ILogger<SmsSender> _logger;

        public SmsSender(ILiwaSmsService liwaSmsService, ILogger<SmsSender> logger)
        {
            _liwaSmsService = liwaSmsService ?? throw new ArgumentNullException(nameof(liwaSmsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendAsync(string destination, string message, CancellationToken ct = default)
        {
            try
            {
                var result = await _liwaSmsService.SendSmsAsync(destination, message, ct);

                if (!result.Success)
                {
                    throw new InvalidOperationException($"Error al enviar SMS");
                }

                _logger.LogInformation(
                    "SmsSender: SMS enviado exitosamente a {Destination}, MessageId: {MessageId}",
                    MaskPhone(destination), result.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SmsSender: Excepción al enviar SMS a {Destination}", MaskPhone(destination));
            }
        }

        private static string MaskPhone(string phone)
        {
            if (phone.Length <= 4) return "****";
            return phone[..^4] + "****";
        }
    }
}