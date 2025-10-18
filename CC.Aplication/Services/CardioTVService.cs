using AutoMapper;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CC.Aplication.Services
{
    /// <summary>
    /// Servicio para gestión de contenido CardioTV
    /// </summary>
    public class CardioTVService : ServiceBase<CardioTV, CardioTVDto>, ICardioTVService
    {
        public CardioTVService(
            ICardioTVRepository repository, 
            IMapper mapper,
            ILogger<CardioTVService> logger) 
            : base(repository, mapper, logger)
        {
        }
    }
}
