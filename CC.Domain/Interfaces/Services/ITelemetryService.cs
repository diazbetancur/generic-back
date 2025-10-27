using CC.Domain.Dtos;
using CC.Domain.Entities;

namespace CC.Domain.Interfaces.Services
{
  public interface ITelemetryService : IServiceBase<TelemetryLog, TelemetryDto>
  {
  }
}