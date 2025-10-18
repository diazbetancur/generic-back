using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;

namespace CC.Infrastructure.Repositories
{
    public class SessionsRepository : ERepositoryBase<Sessions>, ISessionsRepository
    {
        public SessionsRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}