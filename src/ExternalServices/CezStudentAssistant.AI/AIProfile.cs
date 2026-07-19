using AutoMapper;
using CezStudentAssistant.AI.Responses.Quiz;
using CezStudentAssistant.Application.Enums;
using CezStudentAssistant.Application.Responses.AI.Quiz;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("CezStudentAssistant.UnitTests")]

namespace CezStudentAssistant.AI;

internal class AIProfile : Profile
{
    public AIProfile()
    {
        CreateMap<ExternalAIQuestionOption, AIQuestionOption>();

        CreateMap<ExternalAIQuestion, AIQuestion>()
            .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => MapQuestionType(src.QuestionType)));

        CreateMap<ExternalAIQuiz, AIQuiz>();
    }

    private static QuestionType MapQuestionType(string type)
    {
        return Enum.TryParse<QuestionType>(type, true, out var result) ? result : default;
    }
}
