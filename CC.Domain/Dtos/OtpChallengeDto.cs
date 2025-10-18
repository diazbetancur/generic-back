using CC.Domain.Entities;

namespace CC.Domain.Dtos
{
    public class OtpChallengeDto : BaseDto<Guid>
    {
        public Guid DocTypeId { get; set; }
        public string DocNumber { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public int FailedAttempts { get; set; }
        public bool DeliveredToSms { get; set; }
        public bool DeliveredToEmail { get; set; }
        public string? ClientIp { get; set; }
    }
}