namespace CC.Domain.Enums
{
  /// <summary>
  /// Tipos de actividad que se pueden realizar con documentos
  /// </summary>
  public enum ActivityType
  {
    /// <summary>
    /// El usuario consultó/visualizó el documento
    /// </summary>
    Consulta = 1,

    /// <summary>
    /// El usuario descargó el documento
    /// </summary>
    Descarga = 2
  }
}