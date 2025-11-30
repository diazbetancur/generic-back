namespace CC.Domain.Dtos
{
  /// <summary>
  /// DTO para permiso
  /// </summary>
  public class PermissionDto
  {
    /// <summary>
    /// ID del permiso
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre único del permiso
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Módulo al que pertenece
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del permiso
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indica si está activo
    /// </summary>
    public bool IsActive { get; set; }
  }

  /// <summary>
  /// DTO agrupado por módulo (para frontend)
  /// </summary>
  public class PermissionsByModuleDto
  {
    /// <summary>
    /// Nombre del módulo
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// Lista de permisos del módulo
    /// </summary>
    public List<PermissionDto> Permissions { get; set; } = new();
  }

  /// <summary>
  /// Request para crear un permiso
  /// </summary>
  public class CreatePermissionRequest
  {
    /// <summary>
    /// Nombre único del permiso (requerido)
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Módulo al que pertenece (requerido)
    /// </summary>
    public required string Module { get; set; }

    /// <summary>
    /// Descripción del permiso (opcional)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indica si está activo (por defecto true)
    /// </summary>
    public bool IsActive { get; set; } = true;
  }

  /// <summary>
  /// Request para actualizar un permiso
  /// </summary>
  public class UpdatePermissionRequest
  {
    /// <summary>
    /// Descripción del permiso
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indica si está activo
    /// </summary>
    public bool? IsActive { get; set; }
  }
}
