using CC.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CC.Domain.Entities
{
  /// <summary>
  /// Entidad para registrar telemetría de la aplicación
  /// (consultas y descargas de documentos, y futuras métricas)
  /// </summary>
  public class TelemetryLog : EntityBase<Guid>
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
    /// Tipo de documento consultado/descargado (viene del servicio externo)
    /// Ej: "Examen", "ResultadoLab", "Historia", etc.
    /// </summary>
    [Required]
    [MaxLength(100)]
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
    /// Fuente o contexto de donde provino la acción (dato entregado por el front)
    /// Ej: "Portal Web", "Mobile App", "Dashboard", etc.
    /// </summary>
    [MaxLength(100)]
    public string? Source { get; set; }

    /// <summary>
    /// Tipo de telemetría (para futuras expansiones)
    /// </summary>
    [MaxLength(50)]
    public string TelemetryType { get; set; } = "DocumentActivity";

    /// <summary>
    /// Datos adicionales en formato JSON (para futuras expansiones)
    /// </summary>
    public string? AdditionalData { get; set; }
  }
}