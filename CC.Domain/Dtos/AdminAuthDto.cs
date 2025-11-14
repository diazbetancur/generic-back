namespace CC.Domain.Dtos
{
    /// <summary>
    /// Request para login de usuarios administrativos
    /// </summary>
    public record AdminLoginRequest
    {
        /// <summary>
        /// Nombre de usuario
        /// </summary>
        public required string UserName { get; init; }

        /// <summary>
        /// Contraseña
        /// </summary>
        public required string Password { get; init; }
    }

    /// <summary>
    /// Response de login admin exitoso
    /// </summary>
    public record AdminLoginResponse
    {
        /// <summary>
        /// Token JWT
        /// </summary>
        public required string Token { get; init; }

        /// <summary>
        /// Fecha de expiración del token
        /// </summary>
        public required DateTime ExpiresAt { get; init; }

        /// <summary>
        /// Información del usuario autenticado
        /// </summary>
        public required AdminUserInfo User { get; init; }
    }

    /// <summary>
    /// Información del usuario admin autenticado
    /// </summary>
    public record AdminUserInfo
    {
        /// <summary>
        /// ID del usuario
        /// </summary>
        public required Guid Id { get; init; }

        /// <summary>
        /// Nombre de usuario
        /// </summary>
        public required string UserName { get; init; }

        /// <summary>
        /// Email
        /// </summary>
        public required string Email { get; init; }

        /// <summary>
        /// Primer nombre
        /// </summary>
        public required string FirstName { get; init; }

        /// <summary>
        /// Apellido
        /// </summary>
        public required string LastName { get; init; }

        /// <summary>
        /// Roles del usuario
        /// </summary>
        public required List<string> Roles { get; init; }
    }
}
