using CC.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CC.Domain.Dtos
{
  /// <summary>
  /// DTO para la entidad TelemetryLog
  /// </summary>
  public class TelemetryDto : BaseDto<Guid>
  {
    /// <summary>
    /// Tipo de documento del usuario (CC, TI, etc.)
    /// </summary>
    public string UserDocType { get; set; }

    /// <summary>
    /// Número de documento del usuario
    /// </summary>
    public string UserDocNumber { get; set; }

    /// <summary>
    /// Tipo de documento consultado/descargado
    /// </summary>
    public string DocumentType { get; set; }

    /// <summary>
    /// Tipo de actividad realizada
    /// </summary>
    public ActivityType ActivityType { get; set; }

    /// <summary>
    /// Fecha y hora de la actividad
    /// </summary>
    public DateTime ActivityDate { get; set; }

    /// <summary>
    /// Fuente de donde provino la acción
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Tipo de telemetría
    /// </summary>
    public string TelemetryType { get; set; } = "DocumentActivity";

    /// <summary>
    /// Datos adicionales en formato JSON
    /// </summary>
    public string? AdditionalData { get; set; }
  }

  /// <summary>
  /// DTO para crear un nuevo registro de telemetría
  /// </summary>
  public class TelemetryRequestDto
  {
    /// <summary>
    /// Tipo de documento del usuario (CC, TI, etc.)
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string UserDocType { get; set; }

    /// <summary>
    /// Número de documento del usuario
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string UserDocNumber { get; set; }

    /// <summary>
    /// Tipo de documento consultado/descargado
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string DocumentType { get; set; }

    /// <summary>
    /// Tipo de actividad realizada
    /// </summary>
    [Required]
    public ActivityType ActivityType { get; set; }

    /// <summary>
    /// Fuente de donde provino la acción (opcional)
    /// </summary>
    [MaxLength(100)]
    public string? Source { get; set; }

    /// <summary>
    /// Tipo de telemetría (opcional, default: DocumentActivity)
    /// </summary>
    [MaxLength(50)]
    public string? TelemetryType { get; set; }

    /// <summary>
    /// Datos adicionales en formato JSON (opcional)
    /// </summary>
    public string? AdditionalData { get; set; }
  }

  /// <summary>
  /// DTO para filtros de consulta de telemetría
  /// </summary>
  public class TelemetryFilterDto
  {
    /// <summary>
    /// Filtrar por tipo de documento de usuario
    /// </summary>
    public string? UserDocType { get; set; }

    /// <summary>
    /// Filtrar por número de documento de usuario
    /// </summary>
    public string? UserDocNumber { get; set; }

    /// <summary>
    /// Filtrar por tipo de documento
    /// </summary>
    public string? DocumentType { get; set; }

    /// <summary>
    /// Filtrar por tipo de actividad
    /// </summary>
    public ActivityType? ActivityType { get; set; }

    /// <summary>
    /// Fecha de inicio del rango
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Fecha final del rango
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Filtrar por fuente
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Filtrar por tipo de telemetría
    /// </summary>
    public string? TelemetryType { get; set; }
  }
}