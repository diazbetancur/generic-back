namespace CC.Infrastructure.External.Email
{
    public class SmtpEmailOptions
    {
        public string ServiceName { get; set; } = "SmtpEmailService";
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587; // STARTTLS
        public bool UseSsl { get; set; } = true; // STARTTLS when true for port 587
        public int TimeoutSeconds { get; set; } = 30;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "Portal Pacientes";
    }
}
