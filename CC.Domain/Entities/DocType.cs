namespace CC.Domain.Entities
{
    public class DocType : EntityBase<Guid>
    {
        public string Code { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}