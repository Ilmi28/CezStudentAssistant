using CezStudentAssistant.Application.Helpers;
using CezStudentAssistant.Domain.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace CezStudentAssistant.UnitTests.Application.Helpers;

[TestFixture]
public class QuizPointsCalculationHelperTests
{
    [TestCase(QuestionDifficulty.Easy, 1)]
    [TestCase(QuestionDifficulty.Medium, 2)]
    [TestCase(QuestionDifficulty.Hard, 3)]
    public void CalculatePoints_ShouldReturnExpectedPoints_ForSingleCorrectQuestionDifficulty(QuestionDifficulty difficulty, decimal expectedPoints)
    {
        // Arrange
        var qId = System.Guid.NewGuid();
        var optCorrect = System.Guid.NewGuid();

        var question = new CezStudentAssistant.Domain.Entities.Question
        {
            Id = qId,
            Content = "Test Q",
            Difficulty = difficulty,
            Options = new System.Collections.Generic.List<CezStudentAssistant.Domain.Entities.QuestionOption>
            {
                new() { Id = optCorrect, Content = "Opt1", IsCorrect = true }
            }
        };

        var quiz = new CezStudentAssistant.Domain.Entities.Quiz
        {
            Name = "Quiz",
            DisplayName = "Quiz",
            Questions = new System.Collections.Generic.List<CezStudentAssistant.Domain.Entities.Question> { question }
        };

        var answer = new CezStudentAssistant.Domain.Entities.QuestionAnswer
        {
            QuestionId = qId,
            SelectedOptions = new System.Collections.Generic.List<CezStudentAssistant.Domain.Entities.SelectedQuizOption>
            {
                new() { QuestionOptionId = optCorrect }
            }
        };

        var attempt = new CezStudentAssistant.Domain.Entities.QuizAttempt
        {
            Quiz = quiz,
            Answers = new System.Collections.Generic.List<CezStudentAssistant.Domain.Entities.QuestionAnswer> { answer }
        };

        // Act
        var result = QuizPointsCalculationHelper.CalculatePoints(attempt);

        // Assert
        result.Should().Be(expectedPoints);
    }

    [Test]
    public void CalculatePoints_ShouldReturnZero_WhenAttemptOrAnswersAreNull()
    {
        // Act
        var result = QuizPointsCalculationHelper.CalculatePoints(new CezStudentAssistant.Domain.Entities.QuizAttempt());

        // Assert
        result.Should().Be(0m);
    }

    [Test]
    public void CalculatePoints_ShouldCalculateSumOfPointsForCorrectAnswers_BasedOnQuestionDifficulty()
    {
        // Arrange
        var quizId = System.Guid.NewGuid();
        var q1Id = System.Guid.NewGuid();
        var q2Id = System.Guid.NewGuid();
        var q1OptCorrect = System.Guid.NewGuid();
        var q2OptCorrect = System.Guid.NewGuid();

        var question1 = new CezStudentAssistant.Domain.Entities.Question
        {
            Id = q1Id,
            Content = "Q1",
            Difficulty = QuestionDifficulty.Easy,
            Options = new System.Collections.Generic.List<CezStudentAssistant.Domain.Entities.QuestionOption>
            {
                new() { Id = q1OptCorrect, Content = "Opt1", IsCorrect = true }
            }
        };

        var question2 = new CezStudentAssistant.Domain.Entities.Question
        {
            Id = q2Id,
            Content = "Q2",
            Difficulty = QuestionDifficulty.Hard,
            Options = new System.Collections.Generic.List<CezStudentAssistant.Domain.Entities.QuestionOption>
            {
                new() { Id = q2OptCorrect, Content = "Opt2", IsCorrect = true }
            }
        };

        var quiz = new CezStudentAssistant.Domain.Entities.Quiz
        {
            Id = quizId,
            Name = "Sample Quiz",
            DisplayName = "Sample Quiz",
            Questions = new System.Collections.Generic.List<CezStudentAssistant.Domain.Entities.Question> { question1, question2 }
        };

        var answer1 = new CezStudentAssistant.Domain.Entities.QuestionAnswer
        {
            QuestionId = q1Id,
            SelectedOptions = new System.Collections.Generic.List<CezStudentAssistant.Domain.Entities.SelectedQuizOption>
            {
                new() { QuestionOptionId = q1OptCorrect }
            }
        };

        var answer2 = new CezStudentAssistant.Domain.Entities.QuestionAnswer
        {
            QuestionId = q2Id,
            SelectedOptions = new System.Collections.Generic.List<CezStudentAssistant.Domain.Entities.SelectedQuizOption>
            {
                new() { QuestionOptionId = System.Guid.NewGuid() } // Incorrect
            }
        };

        var attempt = new CezStudentAssistant.Domain.Entities.QuizAttempt
        {
            Quiz = quiz,
            Answers = new System.Collections.Generic.List<CezStudentAssistant.Domain.Entities.QuestionAnswer> { answer1, answer2 }
        };

        // Act
        var totalPoints = QuizPointsCalculationHelper.CalculatePoints(attempt);

        // Assert: Q1 is Easy (1m, correct), Q2 is Hard (3m, incorrect) => Total = 1m
        totalPoints.Should().Be(1m);
    }

    [Test]
    public void CalculatePoints_ShouldCalculatePartialCredit_ForMultipleChoiceQuestion()
    {
        // Arrange: Medium difficulty question (2m max), 3 correct options: Opt1, Opt2, Opt3.
        // User selects: Opt1 (correct), Opt4 (incorrect).
        // net = 1 - 1 = 0 => 0m
        // If User selects: Opt1 (correct), Opt2 (correct), Opt4 (incorrect):
        // net = 2 - 1 = 1 => 1/3 * 2m = 0.67m
        var quizId = System.Guid.NewGuid();
        var qId = System.Guid.NewGuid();
        var opt1Correct = System.Guid.NewGuid();
        var opt2Correct = System.Guid.NewGuid();
        var opt3Correct = System.Guid.NewGuid();
        var opt4Wrong = System.Guid.NewGuid();

        var question = new CezStudentAssistant.Domain.Entities.Question
        {
            Id = qId,
            Content = "Which tables are used?",
            Type = QuestionType.MultipleChoice,
            Difficulty = QuestionDifficulty.Medium,
            Options = new System.Collections.Generic.List<CezStudentAssistant.Domain.Entities.QuestionOption>
            {
                new() { Id = opt1Correct, Content = "CARS", IsCorrect = true },
                new() { Id = opt2Correct, Content = "DEPARTMENTS", IsCorrect = true },
                new() { Id = opt3Correct, Content = "EMPLOYEES", IsCorrect = true },
                new() { Id = opt4Wrong, Content = "CUSTOMERS", IsCorrect = false }
            }
        };

        var quiz = new CezStudentAssistant.Domain.Entities.Quiz
        {
            Name = "DB Quiz",
            DisplayName = "DB Quiz",
            Questions = new System.Collections.Generic.List<CezStudentAssistant.Domain.Entities.Question> { question }
        };

        // User chose 1 correct (CARS) and 0 incorrect: fraction = (1 - 0) / 3 = 1/3 * 2m = 0.67m
        var answer = new CezStudentAssistant.Domain.Entities.QuestionAnswer
        {
            QuestionId = qId,
            SelectedOptions = new System.Collections.Generic.List<CezStudentAssistant.Domain.Entities.SelectedQuizOption>
            {
                new() { QuestionOptionId = opt1Correct }
            }
        };

        var attempt = new CezStudentAssistant.Domain.Entities.QuizAttempt
        {
            Quiz = quiz,
            Answers = new System.Collections.Generic.List<CezStudentAssistant.Domain.Entities.QuestionAnswer> { answer }
        };

        // Act
        var totalPoints = QuizPointsCalculationHelper.CalculatePoints(attempt);

        // Assert
        totalPoints.Should().Be(0.67m);
    }
}
