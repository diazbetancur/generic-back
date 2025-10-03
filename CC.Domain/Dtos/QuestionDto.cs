using CC.Domain.Enums;

namespace CC.Domain.Dtos
{
    public class QuestionDto : BaseDto<Guid>
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public SurveyQuestionType QuestionType { get; set; }
        public ICollection<ResponseQuestionDto>? Responses { get; set; }
    }
    public class ResponseQuestionDto : BaseDto<Guid>
    {
        public string? Text { get; set; }
        public string? Placeholder { get; set; }
        public string? Emoji { get; set; }
        public Guid QuestionId { get; set; }

    }
}
