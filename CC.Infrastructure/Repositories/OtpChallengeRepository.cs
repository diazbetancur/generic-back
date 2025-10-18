using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;

namespace CC.Infrastructure.Repositories
{
    public class OtpChallengeRepository : ERepositoryBase<OtpChallenge>, IOtpChallengeRepository
    {
        public OtpChallengeRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
