using CC.Domain.Dtos;

namespace CC.Domain.Interfaces.Services
{
    /// <summary>
    /// Servicio para recuperación de contraseñas de usuarios administrativos
    /// </summary>
    public interface IPasswordResetService
    {
        /// <summary>
        /// Genera un código de recuperación y lo envía por SMS y Email
        /// </summary>
        /// <param name="request">Request con username o email</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Response con información enmascarada de envío</returns>
        Task<ForgotPasswordResponse> RequestPasswordResetAsync(
            ForgotPasswordRequest request, 
            CancellationToken ct = default);

        /// <summary>
        /// Verifica el código y establece nueva contraseña
        /// </summary>
        /// <param name="request">Request con código y nueva contraseña</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Response indicando éxito o fallo</returns>
        Task<ResetPasswordResponse> ResetPasswordAsync(
            ResetPasswordRequest request, 
            CancellationToken ct = default);

        /// <summary>
        /// Invalida todos los tokens de reset activos de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Task</returns>
        Task InvalidateUserTokensAsync(Guid userId);
    }
}
