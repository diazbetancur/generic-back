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
    /// Servicio de aplicación para State
    /// </summary>
    public class StateService : ServiceBase<State, StateDto>, IStateService
    {
        public StateService(
            IStateRepository repository,
            IMapper mapper,
            ILogger<StateService> logger)
            : base(repository, mapper, logger)
        {
        }
    }
}
