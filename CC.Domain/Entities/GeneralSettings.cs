namespace CC.Domain.Entities
{
    public class GeneralSettings : EntityBase<Guid>
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string? Description { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}