namespace CC.Domain.Dtos
{
  /// <summary>
  /// DTO para datos de usuario
  /// </summary>
  public class UserDto
  {
    /// <summary>
    /// ID del usuario
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre de usuario
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Primer nombre
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Apellido
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Número de teléfono
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Email confirmado
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Roles del usuario
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Indica si el usuario está eliminado
    /// </summary>
    public bool IsDeleted { get; set; }
  }
}
