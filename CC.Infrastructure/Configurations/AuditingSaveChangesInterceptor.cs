using System.Text.Json;
using CC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace CC.Infrastructure.Configurations;

public class AuditingSaveChangesInterceptor : SaveChangesInterceptor
{
  private readonly string _environment;
  private readonly string _placeholderUserId;

  public AuditingSaveChangesInterceptor(IConfiguration configuration)
  {
    _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown";
    _placeholderUserId = configuration.GetSection("Auditing")?["PlaceholderUserId"] ?? "SYSTEM";
  }

  public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
      DbContextEventData eventData,
      InterceptionResult<int> result,
      CancellationToken cancellationToken = default)
  {
    if (eventData.Context is null)
      return base.SavingChangesAsync(eventData, result, cancellationToken);

    var context = eventData.Context;
    var auditEntries = new List<AuditLog>();

    foreach (var entry in context.ChangeTracker.Entries())
    {
      if (entry.Entity is AuditLog) continue; // skip self
      if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged) continue;

      string action = entry.State switch
      {
        EntityState.Added => "Create",
        EntityState.Modified => "Update",
        EntityState.Deleted => "Delete",
        _ => string.Empty
      };

      if (string.IsNullOrEmpty(action)) continue;

      var audit = new AuditLog
      {
        EntityName = entry.Entity.GetType().Name,
        EntityId = GetPrimaryKey(entry),
        Action = action,
        UserId = _placeholderUserId,
        UserName = _placeholderUserId,
        Environment = _environment,
        TraceId = System.Diagnostics.Activity.Current?.TraceId.ToString(),
      };

      if (entry.State == EntityState.Added)
      {
        audit.NewValues = SerializeValues(entry.CurrentValues);
      }
      else if (entry.State == EntityState.Deleted)
      {
        audit.OldValues = SerializeValues(entry.OriginalValues);
      }
      else if (entry.State == EntityState.Modified)
      {
        var changes = new Dictionary<string, object?>();
        var oldVals = new Dictionary<string, object?>();
        var newVals = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
          if (!property.IsModified) continue;
          var original = property.OriginalValue;
          var current = property.CurrentValue;
          if (Equals(original, current)) continue;
          oldVals[property.Metadata.Name] = original;
          newVals[property.Metadata.Name] = current;
        }

        if (newVals.Any())
        {
          audit.ChangedColumns = JsonSerializer.Serialize(newVals.Keys);
          audit.OldValues = JsonSerializer.Serialize(oldVals);
          audit.NewValues = JsonSerializer.Serialize(newVals);
        }
      }

      if (!string.IsNullOrEmpty(audit.NewValues) || !string.IsNullOrEmpty(audit.OldValues))
      {
        auditEntries.Add(audit);
      }
    }

    if (auditEntries.Any())
    {
      var set = context.Set<AuditLog>();
      set.AddRange(auditEntries);
    }

    return base.SavingChangesAsync(eventData, result, cancellationToken);
  }

  private static string SerializeValues(PropertyValues values)
  {
    var dict = new Dictionary<string, object?>();
    foreach (var propName in values.Properties.Select(p => p.Name))
    {
      dict[propName] = values[propName];
    }
    return JsonSerializer.Serialize(dict);
  }

  private static string GetPrimaryKey(EntityEntry entry)
  {
    var key = entry.Metadata.FindPrimaryKey();
    if (key == null) return string.Empty;
    var parts = key.Properties.Select(p => entry.Property(p.Name).CurrentValue?.ToString());
    return string.Join('|', parts);
  }
}
