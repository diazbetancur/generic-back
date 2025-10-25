namespace CC.Domain.Entities
{
 public class LoginAttempt : EntityBase<Guid>
 {
 public string DocTypeCode { get; set; } = string.Empty;
 public string DocNumber { get; set; } = string.Empty;
 public string? UserId { get; set; }
 public bool Success { get; set; }
 public string? Reason { get; set; }
 public string? Ip { get; set; }
 public string? UserAgent { get; set; }
 public string? TraceId { get; set; }
 }
}
