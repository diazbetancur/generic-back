using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;

namespace CC.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio para State
    /// </summary>
    public class StateRepository : ERepositoryBase<State>, IStateRepository
    {
        public StateRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
