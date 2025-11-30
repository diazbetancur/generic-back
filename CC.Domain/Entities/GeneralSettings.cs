namespace CC.Domain.Entities
{
  /// <summary>
  /// General application settings (key-value configuration)
  /// </summary>
  public class GeneralSettings : EntityBase<Guid>
  {
    /// <summary>
    /// Setting key (unique identifier)
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Setting value
    /// </summary>
    public required string Value { get; set; }

    /// <summary>
    /// Description of the setting
    /// </summary>
    public string? Description { get; set; }
  }
}
