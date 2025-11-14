using CC.Domain.Entities;
using CC.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CC.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio para tokens de recuperación de contraseña
    /// </summary>
    public class PasswordResetTokenRepository : ERepositoryBase<PasswordResetToken>, IPasswordResetTokenRepository
    {
        private readonly IQueryableUnitOfWork _unitOfWork;

        public PasswordResetTokenRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<PasswordResetToken>> GetActiveTokensByUserIdAsync(Guid userId)
        {
            return await _unitOfWork.GetSet<PasswordResetToken>()
                .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task InvalidateUserTokensAsync(Guid userId)
        {
            var tokens = await GetActiveTokensByUserIdAsync(userId);
            
            foreach (var token in tokens)
            {
                token.IsUsed = true;
                token.UsedAt = DateTime.UtcNow;
            }

            if (tokens.Any())
            {
                await _unitOfWork.CommitAsync();
            }
        }
    }
}
