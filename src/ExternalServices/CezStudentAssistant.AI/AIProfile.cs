using AutoMapper;
using CezStudentAssistant.AI.Responses.Quiz;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Application.Responses.AI.Quiz;

namespace CezStudentAssistant.AI;

public class AIProfile : Profile
{
    public AIProfile()
    {
        CreateMap<ExternalAIQuestionOption, AIQuestionOption>();

        CreateMap<ExternalAIQuestion, AIQuestion>()
            .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => MapQuestionType(src.QuestionType)))
            .ForMember(dest => dest.Difficulty, opt => opt.MapFrom(src => MapDifficulty(src.Difficulty)));

        CreateMap<ExternalAIQuiz, AIQuiz>();
    }

    private static QuestionType MapQuestionType(string type)
    {
        return Enum.TryParse<QuestionType>(type, true, out var result) ? result : default;
    }

    private static QuestionDifficulty MapDifficulty(string difficulty)
    {
        return Enum.TryParse<QuestionDifficulty>(difficulty, true, out var result) ? result : QuestionDifficulty.Medium;
    }
}
