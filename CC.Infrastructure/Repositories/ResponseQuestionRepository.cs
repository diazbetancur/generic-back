using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;

namespace CC.Infrastructure.Repositories
{
  public class ResponseQuestionRepository : ERepositoryBase<ResponseQuestion>, IResponseQuestionRepository
  {
    public ResponseQuestionRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
  }
}
