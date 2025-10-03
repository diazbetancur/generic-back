namespace CC.Domain.Entities
{
    public class FrecuentQuestions : EntityBase<Guid>
    {
        public required string Question { get; set; }
        public required string Response { get; set; }
    }
}