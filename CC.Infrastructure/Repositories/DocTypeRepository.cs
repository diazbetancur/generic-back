using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;

namespace CC.Infrastructure.Repositories
{
    public class DocTypeRepository : ERepositoryBase<DocType>, IDocTypeRepository
    {
        public DocTypeRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
