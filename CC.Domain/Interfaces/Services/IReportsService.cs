using CC.Domain.Dtos;

namespace CC.Domain.Interfaces.Services
{
 public interface IReportsService
 {
 Task<int> GetEffectiveLoginsAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
 Task<int> GetIneffectiveLoginsAsync(DateTime fromUtc, DateTime toUtc, string? reason = null, CancellationToken ct = default);
 Task<double> GetAvgSessionMinutesAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
 Task<TimeSliceKpiDto> GetLoginsByTimeSlicesAsync(DateTime fromUtc, DateTime toUtc, TimeZoneInfo? tz = null, CancellationToken ct = default);
 }
}
