using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;

namespace CC.Infrastructure.Repositories
{
    public class FrecuentQuestionsRepository : ERepositoryBase<FrecuentQuestions>, IFrecuentQuestionsRepository
    {
        public FrecuentQuestionsRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}