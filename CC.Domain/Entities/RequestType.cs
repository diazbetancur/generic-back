namespace CC.Domain.Entities
{
    public class RequestType : EntityBase<Guid>
    {
        public string Name { get; set; }
        public string Template { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public bool IsSystem { get; set; } = false;
    }
}