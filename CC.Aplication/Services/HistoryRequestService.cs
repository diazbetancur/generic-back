using AutoMapper;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CC.Aplication.Services
{
    /// <summary>
    /// Servicio de aplicación para HistoryRequest
    /// </summary>
    public class HistoryRequestService : ServiceBase<HistoryRequest, HistoryRequestDto>, IHistoryRequestService
    {
        public HistoryRequestService(
            IHistoryRequestRepository repository,
            IMapper mapper,
            ILogger<HistoryRequestService> logger)
            : base(repository, mapper, logger)
        {
        }
    }
}
