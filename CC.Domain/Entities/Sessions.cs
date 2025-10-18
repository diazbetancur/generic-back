namespace CC.Domain.Entities
{
    public class Sessions : EntityBase<Guid>
    {
        public Guid DocTypeId { get; set; }
        public string DocNumber { get; set; }
        public string UserId { get; set; }
        public string Jti { get; set; }
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? RevokedAt { get; set; }
        public string? ClientIp { get; set; }
        public DateTime? LastSeenAt { get; set; }
    }
}