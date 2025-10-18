namespace CC.Domain.Dtos
{
    public class GeneralSettingsDto : BaseDto<Guid>
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string? Description { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}