
namespace CC.Domain.Enums
{
    public enum SurveyQuestionType
    {
        SingleChoice = 1,            // Radio button (single answer)
        SingleChoiceEmoji = 2,       // Radio button with emoji + text
        MultipleChoice = 3,          // Checkbox
        TextArea = 4                 // Multi-line free text (uses Placeholder)
    }
}
