using NUnit.Framework;
using CezStudentAssistant.AI;
using CezStudentAssistant.AI.Responses.Quiz;
using CezStudentAssistant.Application.Enums;
using CezStudentAssistant.Application.Responses.AI.Quiz;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Google.GenAI;
using System.Collections.Generic;
using System.Linq;

namespace CezStudentAssistant.UnitTests;

[TestFixture]
public class GeminiAIClientTests
{
    private IMapper _mapper;
    private IConfiguration _configuration;
    private ILogger<GeminiAIClient> _logger;
    private Client _client;

    [SetUp]
    public void SetUp()
    {
        _mapper = Substitute.For<IMapper>();
        _configuration = Substitute.For<IConfiguration>();
        _logger = Substitute.For<ILogger<GeminiAIClient>>();
        _client = new Client(apiKey: "dummy-api-key");
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
    }

    [Test]
    public void Constructor_ShouldInitializeCorrectly()
    {
        var aiClient = new GeminiAIClient(_client, _mapper, _configuration, _logger);
        Assert.That(aiClient, Is.Not.Null);
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
                    Points = 5,
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
        Assert.That(quiz.Questions[0].Points, Is.EqualTo(5));
        Assert.That(quiz.Questions[0].Options, Has.Count.EqualTo(2));
        Assert.That(quiz.Questions[0].Options.First().Content, Is.EqualTo("2"));
        Assert.That(quiz.Questions[0].Options.First().IsCorrect, Is.True);
    }
}
