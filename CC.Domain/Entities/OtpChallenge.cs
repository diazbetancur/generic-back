namespace CC.Domain.Entities
{
    public class OtpChallenge : EntityBase<Guid>
    {
        public Guid DocTypeId { get; set; }
        public virtual DocType DocType { get; set; }
        public string DocNumber { get; set; }
        public string UserId { get; set; }
        public string CodeHash { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public int FailedAttempts { get; set; } = 0;
        public bool DeliveredToSms { get; set; }
        public bool DeliveredToEmail { get; set; }
        public string? ClientIp { get; set; }
    }
}