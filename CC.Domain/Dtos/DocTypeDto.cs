namespace CC.Domain.Dtos
{
    public class DocTypeDto : BaseDto<Guid>
    {
        public string Code { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}