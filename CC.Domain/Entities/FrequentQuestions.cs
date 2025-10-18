namespace CC.Domain.Entities
{
    public class FrequentQuestions : EntityBase<Guid>
    {
        public string Question { get; set; }
        public string Response { get; set; }
    }
}