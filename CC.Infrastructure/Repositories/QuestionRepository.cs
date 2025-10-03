using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;

namespace CC.Infrastructure.Repositories
{
  public class QuestionRepository : ERepositoryBase<Question>, IQuestionRepository
  {
    public QuestionRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
  }
}
