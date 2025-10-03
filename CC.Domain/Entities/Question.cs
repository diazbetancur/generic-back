using CC.Domain.Enums;

namespace CC.Domain.Entities
{
  public class Question : EntityBase<Guid>
  {
    public required string Title { get; set; }
    public string? Description { get; set; }
    public SurveyQuestionType QuestionType { get; set; }
    public ICollection<ResponseQuestion> ResponseQuestions { get; set; } = new List<ResponseQuestion>();
  }

  public class ResponseQuestion : EntityBase<Guid>
  {
    public string? Text { get; set; }
    public string? Placeholder { get; set; }
    public string? Emoji { get; set; }
    public Guid QuestionId { get; set; }
    public virtual Question Question { get; set; }

  }
}
