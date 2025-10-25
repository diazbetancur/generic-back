using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using CC.Infrastructure.Configurations;

namespace CC.Infrastructure.Repositories
{
 public class LoginAttemptRepository : ERepositoryBase<LoginAttempt>, ILoginAttemptRepository
 {
 public LoginAttemptRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork) { }
 }
}
