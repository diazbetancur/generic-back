using CC.Domain.Entities;

namespace CC.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para tokens de recuperación de contraseña
    /// </summary>
    public interface IPasswordResetTokenRepository : IERepositoryBase<PasswordResetToken>
    {
        /// <summary>
        /// Obtiene tokens activos de un usuario
        /// </summary>
        Task<IEnumerable<PasswordResetToken>> GetActiveTokensByUserIdAsync(Guid userId);

        /// <summary>
        /// Invalida todos los tokens activos de un usuario
        /// </summary>
        Task InvalidateUserTokensAsync(Guid userId);
    }
}
