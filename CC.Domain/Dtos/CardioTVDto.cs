
namespace CC.Domain.Dtos
{
    public class CardioTVDto : BaseDto<Guid>
    {
        public required string Title { get; set; }
        public required string Url { get; set; }
        public required string Description { get; set; }
        public string? ImageUrl { get; set; }
    }
}
