namespace CC.Domain.Entities
{
    public class State : EntityBase<Guid>
    {
        public string Name { get; set; }
        public string HexColor { get; set; }
        public bool IsDeleted { get; set; }
    }
}