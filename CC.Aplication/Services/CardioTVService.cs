using AutoMapper;
using CC.Domain.Dtos;
using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Domain.Interfaces.Services;

namespace CC.Aplication.Services
{
    public class CardioTVService : ServiceBase<CardioTV, CardioTVDto>, ICardioTVService
    {
        public CardioTVService(ICardioTVRepository repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}
