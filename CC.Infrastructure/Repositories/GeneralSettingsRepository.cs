using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;

namespace CC.Infrastructure.Repositories
{
    public class GeneralSettingsRepository : ERepositoryBase<GeneralSettings>, IGeneralSettingsRepository
    {
        public GeneralSettingsRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}