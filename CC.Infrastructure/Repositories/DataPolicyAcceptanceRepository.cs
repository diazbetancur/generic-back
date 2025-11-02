using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;

namespace CC.Infrastructure.Repositories
{
    public class DataPolicyAcceptanceRepository : ERepositoryBase<DataPolicyAcceptance>, IDataPolicyAcceptanceRepository
    {
        public DataPolicyAcceptanceRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}