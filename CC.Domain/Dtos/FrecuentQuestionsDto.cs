namespace CC.Domain.Dtos
{
    public class FrecuentQuestionsDto : BaseDto<Guid>
    {
        public string Question { get; set; }
        public string Response { get; set; }
    }
}