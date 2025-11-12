using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;

namespace CC.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio para Request
    /// </summary>
    public class RequestRepository : ERepositoryBase<Request>, IRequestRepository
    {
        public RequestRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}