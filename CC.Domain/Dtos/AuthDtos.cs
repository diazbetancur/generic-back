namespace CC.Domain.Dtos
{
    /// <summary>
    /// Request para login de usuario
    /// </summary>
    public record LoginRequest
    {
        /// <summary>
        /// Nombre de usuario o email
        /// </summary>
        public required string UserName { get; init; }

        /// <summary>
        /// Contraseña
        /// </summary>
        public required string Password { get; init; }
    }

    /// <summary>
    /// Response de login exitoso
    /// </summary>
    public record LoginResponse
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
        public required UserInfo User { get; init; }
    }

    /// <summary>
    /// Información del usuario autenticado
    /// </summary>
    public record UserInfo
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
        /// Nombre completo
        /// </summary>
        public required string FullName { get; init; }

        /// <summary>
        /// Roles del usuario
        /// </summary>
        public required List<string> Roles { get; init; }

        /// <summary>
        /// Permisos del usuario
        /// </summary>
        public required List<string> Permissions { get; init; }
    }

    /// <summary>
    /// Request para crear un nuevo usuario
    /// </summary>
    public record CreateUserRequest
    {
        /// <summary>
        /// Nombre de usuario (requerido, único)
        /// </summary>
        public required string UserName { get; init; }

        /// <summary>
        /// Email del usuario (requerido, único)
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
        /// Número de teléfono (opcional)
        /// </summary>
        public string? PhoneNumber { get; init; }

        /// <summary>
        /// Contraseña inicial (requerida)
        /// </summary>
        public required string Password { get; init; }

        /// <summary>
        /// Roles a asignar al usuario (opcional)
        /// </summary>
        public List<string>? Roles { get; init; }
    }

    /// <summary>
    /// Request para actualizar un usuario existente
    /// </summary>
    public record UpdateUserRequest
    {
        /// <summary>
        /// Email del usuario (opcional)
        /// </summary>
        public string? Email { get; init; }

        /// <summary>
        /// Primer nombre (opcional)
        /// </summary>
        public string? FirstName { get; init; }

        /// <summary>
        /// Apellido (opcional)
        /// </summary>
        public string? LastName { get; init; }

        /// <summary>
        /// Número de teléfono (opcional)
        /// </summary>
        public string? PhoneNumber { get; init; }

        /// <summary>
        /// Nueva contraseña (opcional)
        /// </summary>
        public string? Password { get; init; }
    }
}
