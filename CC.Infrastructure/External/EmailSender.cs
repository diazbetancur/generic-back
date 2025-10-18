using CC.Domain.Interfaces.External;
using Microsoft.Extensions.Configuration;

namespace CC.Infrastructure.External
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string destination, string subject, string htmlBody, CancellationToken ct = default)
        {
            await Task.CompletedTask;
        }
    }
}