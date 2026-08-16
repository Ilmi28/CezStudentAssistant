using NUnit.Framework;
using CezStudentAssistant.AI;
using CezStudentAssistant.AI.Services;
using CezStudentAssistant.AI.Responses.Quiz;
using CezStudentAssistant.Domain.Enums;
using CezStudentAssistant.Application.Responses.AI.Quiz;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Google.GenAI;
using System.Collections.Generic;
using System.Linq;

namespace CezStudentAssistant.UnitTests.AI;

[TestFixture]
public class GeminiAIClientTests
{
    private IConfiguration _configuration;
    private ILogger<GeminiAIClient> _clientLogger;
    private ILogger<AIQuizService> _serviceLogger;
    private Client _client;
    private IAIQuizService _quizService;

    [SetUp]
    public void SetUp()
    {
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "Gemini:DefaultModel", "gemini-2.0-flash" },
            { "Gemini:MaxAttempts", "6" },
            { "Gemini:RetryDelaysMs:0", "1000" }
        }).Build();
        _clientLogger = Substitute.For<ILogger<GeminiAIClient>>();
        _serviceLogger = Substitute.For<ILogger<AIQuizService>>();
        _client = new Client(apiKey: "dummy-api-key");
        _quizService = Substitute.For<IAIQuizService>();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
    }

    [Test]
    public void GeminiAIClient_Constructor_ShouldInitializeCorrectly()
    {
        var processor = Substitute.For<CezStudentAssistant.Application.Interfaces.Services.IFileContentProcessorService>();
        var aiClient = new GeminiAIClient(_client, _configuration, _clientLogger, _quizService, processor);
        Assert.That(aiClient, Is.Not.Null);
    }

    [Test]
    public void AIQuizService_Constructor_ShouldInitializeCorrectly()
    {
        var mapper = Substitute.For<IMapper>();
        var service = new AIQuizService(mapper, _serviceLogger);
        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public void Mapper_ShouldMapExternalQuizToAIQuizCorrectly()
    {
        var loggerFactory = Substitute.For<ILoggerFactory>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AIProfile>(), loggerFactory);
        var mapper = config.CreateMapper();

        var externalQuiz = new ExternalAIQuiz
        {
            Title = "Math Quiz",
            Description = "Basic algebra",
            Questions = new List<ExternalAIQuestion>
            {
                new ExternalAIQuestion
                {
                    Content = "1 + 1 = ?",
                    QuestionType = "SingleChoice",
                    Difficulty = "Easy",
                    Options = new List<ExternalAIQuestionOption>
                    {
                        new ExternalAIQuestionOption { Content = "2", IsCorrect = true },
                        new ExternalAIQuestionOption { Content = "3", IsCorrect = false }
                    }
                }
            }
        };

        var quiz = mapper.Map<AIQuiz>(externalQuiz);

        Assert.That(quiz, Is.Not.Null);
        Assert.That(quiz.Title, Is.EqualTo("Math Quiz"));
        Assert.That(quiz.Description, Is.EqualTo("Basic algebra"));
        Assert.That(quiz.Questions, Has.Count.EqualTo(1));
        Assert.That(quiz.Questions[0].Content, Is.EqualTo("1 + 1 = ?"));
        Assert.That(quiz.Questions[0].QuestionType, Is.EqualTo(QuestionType.SingleChoice));
        Assert.That(quiz.Questions[0].Difficulty, Is.EqualTo(QuestionDifficulty.Easy));
        Assert.That(quiz.Questions[0].Options, Has.Count.EqualTo(2));
        Assert.That(quiz.Questions[0].Options.First().Content, Is.EqualTo("2"));
        Assert.That(quiz.Questions[0].Options.First().IsCorrect, Is.True);
    }
}
