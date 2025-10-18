namespace CC.Domain.Interfaces.External
{
    public interface ISmsSender
    {
        Task SendAsync(string destination, string message, CancellationToken ct = default);
    }

    public interface IEmailSender
    {
        Task SendAsync(string destination, string subject, string htmlBody, CancellationToken ct = default);
    }
}
