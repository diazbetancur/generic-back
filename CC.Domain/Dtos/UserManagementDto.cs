namespace CC.Domain.Dtos
{
    /// <summary>
    /// DTO para crear usuario administrativo
    /// </summary>
    public record CreateUserDto
    {
        /// <summary>
        /// Nombre de usuario (requerido)
        /// </summary>
        public required string UserName { get; init; }

        /// <summary>
        /// Email del usuario (requerido)
        /// </summary>
        public required string Email { get; init; }

        /// <summary>
        /// Número de teléfono (opcional)
        /// </summary>
        public string? PhoneNumber { get; init; }

        /// <summary>
        /// Contraseña inicial (requerida)
        /// </summary>
        public required string Password { get; init; }
    }

    /// <summary>
    /// DTO para actualizar usuario administrativo
    /// </summary>
    public record UpdateUserDto
    {
        /// <summary>
        /// Email del usuario (opcional)
        /// </summary>
        public string? Email { get; init; }

        /// <summary>
        /// Número de teléfono (opcional)
        /// </summary>
        public string? PhoneNumber { get; init; }

        /// <summary>
        /// Nueva contraseña (opcional)
        /// </summary>
        public string? NewPassword { get; init; }
    }

    /// <summary>
    /// DTO para asignar roles a un usuario
    /// </summary>
    public record AssignRolesDto
    {
        /// <summary>
        /// Lista de nombres de roles a asignar
        /// </summary>
        public required List<string> RoleNames { get; init; }
    }
}
