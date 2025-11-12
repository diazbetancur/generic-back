using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;

namespace CC.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio para RequestType
    /// </summary>
    public class RequestTypeRepository : ERepositoryBase<RequestType>, IRequestTypeRepository
    {
        public RequestTypeRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
