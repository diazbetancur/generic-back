using AutoMapper;
using CC.Aplication.Services;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CC.Aplication.Services
{
    /// <summary>
    /// Servicio de aplicación para RequestType
    /// </summary>
    public class RequestTypeService : ServiceBase<RequestType, RequestTypeDto>, IRequestTypeService
    {
        public RequestTypeService(
            IRequestTypeRepository repository,
            IMapper mapper,
            ILogger<RequestTypeService> logger)
            : base(repository, mapper, logger)
        {
        }
    }
}
