using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;

namespace CC.Infrastructure.Repositories
{
    public class FrequentQuestionsRepository : ERepositoryBase<FrequentQuestions>, IFrequentQuestionsRepository
    {
        public FrequentQuestionsRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}