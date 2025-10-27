using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;

namespace CC.Infrastructure.Repositories
{
  public class TelemetryRepository : ERepositoryBase<TelemetryLog>, ITelemetryRepository
  {
    public TelemetryRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
  }
}