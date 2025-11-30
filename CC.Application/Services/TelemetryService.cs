using AutoMapper;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CC.Aplication.Services
{
  /// <summary>
  /// Servicio para gestión de telemetría de la aplicación
  /// (consultas, descargas y futuras métricas)
  /// </summary>
  public class TelemetryService : ServiceBase<TelemetryLog, TelemetryDto>, ITelemetryService
  {
    public TelemetryService(
        ITelemetryRepository repository,
        IMapper mapper,
        ILogger<TelemetryService> logger)
        : base(repository, mapper, logger)
    {
    }
  }
}