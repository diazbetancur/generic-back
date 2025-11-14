namespace CC.Domain.Entities
{
    /// <summary>
    /// Entidad para almacenar tokens de recuperación de contraseña
    /// </summary>
    public class PasswordResetToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string CodeHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? UsedAt { get; set; }
        public string? ClientIp { get; set; }
        public int FailedAttempts { get; set; }
        public bool DeliveredToEmail { get; set; }
        public bool DeliveredToSms { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
