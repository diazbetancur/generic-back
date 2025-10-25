namespace CC.Domain.Dtos
{
 public record LoginKpiDto(int Count);
 public record AvgSessionDurationDto(double AverageMinutes);
 public record TimeSliceKpiDto(int BusinessHours, int OffHours, int Weekend);
}
