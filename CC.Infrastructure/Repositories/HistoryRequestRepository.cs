using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;

namespace CC.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio para HistoryRequest
    /// </summary>
    public class HistoryRequestRepository : ERepositoryBase<HistoryRequest>, IHistoryRequestRepository
    {
        public HistoryRequestRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
