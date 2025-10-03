using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;

namespace CC.Infrastructure.Repositories
{
    public class CardioTVRepository : ERepositoryBase<CardioTV>, ICardioTVRepository
    {
        public CardioTVRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
