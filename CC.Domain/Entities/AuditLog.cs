using System;

namespace CC.Domain.Entities
{
  public class AuditLog : EntityBase<Guid>
  {
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Create | Update | Delete
    public string UserId { get; set; } = string.Empty; // Placeholder for now
    public string UserName { get; set; } = string.Empty; // Placeholder for now
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? ChangedColumns { get; set; }
    public string? RequestPath { get; set; }
    public string? SourceIp { get; set; }
    public string? TraceId { get; set; }
    public string Environment { get; set; } = string.Empty;
  }
}
