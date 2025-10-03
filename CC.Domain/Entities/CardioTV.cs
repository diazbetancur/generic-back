
namespace CC.Domain.Entities
{
    public class CardioTV : EntityBase<Guid>
    {
        public required string Title { get; set; }
        public required string Url { get; set; }
        public required string Description { get; set; }
        public string? ImageUrl { get; set; }
    }
}
