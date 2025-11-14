using CC.Domain.Entities;

namespace CC.Domain.Dtos
{
    /// <summary>
    /// Request para solicitar recuperación de contraseña
    /// </summary>
    public record ForgotPasswordRequest
    {
        /// <summary>
        /// Username o email del usuario
        /// </summary>
        public required string UserNameOrEmail { get; init; }
    }

    /// <summary>
    /// Response de solicitud de recuperación de contraseña
    /// </summary>
    public record ForgotPasswordResponse
    {
        /// <summary>
        /// Indica si la solicitud fue exitosa
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// Mensaje informativo
        /// </summary>
        public required string Message { get; init; }

        /// <summary>
        /// Email enmascarado donde se envió el código (si aplica)
        /// </summary>
        public string? MaskedEmail { get; init; }

        /// <summary>
        /// Teléfono enmascarado donde se envió el código (si aplica)
        /// </summary>
        public string? MaskedPhone { get; init; }

        /// <summary>
        /// Identificador del reset token (usado para verificar el código)
        /// </summary>
        public Guid? ResetTokenId { get; init; }
    }

    /// <summary>
    /// Request para verificar código de recuperación y establecer nueva contraseña
    /// </summary>
    public record ResetPasswordRequest
    {
        /// <summary>
        /// ID del reset token generado
        /// </summary>
        public required Guid ResetTokenId { get; init; }

        /// <summary>
        /// Código de verificación enviado por SMS/Email (6 dígitos)
        /// </summary>
        public required string VerificationCode { get; init; }

        /// <summary>
        /// Nueva contraseña
        /// </summary>
        public required string NewPassword { get; init; }
    }

    /// <summary>
    /// Response de reset de contraseña
    /// </summary>
    public record ResetPasswordResponse
    {
        /// <summary>
        /// Indica si el reset fue exitoso
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// Mensaje informativo
        /// </summary>
        public required string Message { get; init; }
    }
}
