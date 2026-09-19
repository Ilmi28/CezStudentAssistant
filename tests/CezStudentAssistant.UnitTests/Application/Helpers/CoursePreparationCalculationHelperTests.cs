using CezStudentAssistant.Application.Helpers;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CezStudentAssistant.UnitTests.Application.Helpers;

[TestFixture]
public class CoursePreparationCalculationHelperTests
{
    [Test]
    public void CalculatePreparation_ShouldReturnNulls_WhenNoQuizzesOrDecksExist()
    {
        var result = CoursePreparationCalculationHelper.CalculatePreparation(new List<Quiz>(), new List<FlashcardDeck>());

        result.PreparationPercentage.Should().BeNull();
        result.QuizProgressPercentage.Should().BeNull();
        result.FlashcardProgressPercentage.Should().BeNull();
    }

    [Test]
    public void CalculatePreparation_ShouldCalculateWeightedFlashcardProgress_BasedOnCardDifficulty()
    {
        var deckId = Guid.NewGuid();
        var easyCard = new Flashcard
        {
            Id = Guid.NewGuid(),
            Front = "Q1",
            Back = "A1",
            Difficulty = QuestionDifficulty.Easy, // 1 pt
            State = FlashcardStateEnum.Mastered,
            DeckId = deckId
        };
        var hardCard = new Flashcard
        {
            Id = Guid.NewGuid(),
            Front = "Q2",
            Back = "A2",
            Difficulty = QuestionDifficulty.Hard, // 3 pts
            State = FlashcardStateEnum.New,
            DeckId = deckId
        };

        var deck = new FlashcardDeck
        {
            Id = deckId,
            Name = "Deck 1",
            Cards = new List<Flashcard> { easyCard, hardCard }
        };

        // Total weight = 1 + 3 = 4 pts. Mastered weight = 1 pt. 1/4 = 25%.
        var result = CoursePreparationCalculationHelper.CalculatePreparation(new List<Quiz>(), new List<FlashcardDeck> { deck });

        result.FlashcardProgressPercentage.Should().Be(25);
        result.QuizProgressPercentage.Should().BeNull();
        result.PreparationPercentage.Should().Be(25);
    }

    [Test]
    public void CalculatePreparation_ShouldIncludeUnattemptedQuizzesInAverage()
    {
        var quiz1Option = new QuestionOption { Id = Guid.NewGuid(), IsCorrect = true, Content = "Opt1" };
        var quiz1Question = new Question
        {
            Id = Guid.NewGuid(),
            Content = "Q1 Content",
            Difficulty = QuestionDifficulty.Easy,
            Type = QuestionType.SingleChoice,
            Options = new List<QuestionOption> { quiz1Option }
        };

        var quiz1 = new Quiz
        {
            Id = Guid.NewGuid(),
            Name = "Quiz 1",
            Questions = new List<Question> { quiz1Question },
            Attempts = new List<QuizAttempt>
            {
                new QuizAttempt
                {
                    Status = QuizAttemptStatus.Completed,
                    Answers = new List<QuestionAnswer>
                    {
                        new QuestionAnswer
                        {
                            QuestionId = quiz1Question.Id,
                            SelectedOptions = new List<SelectedQuizOption>
                            {
                                new SelectedQuizOption { QuestionOptionId = quiz1Option.Id }
                            }
                        }
                    }
                }
            }
        };

        var quiz2Question = new Question
        {
            Id = Guid.NewGuid(),
            Content = "Q2 Content",
            Difficulty = QuestionDifficulty.Easy,
            Type = QuestionType.SingleChoice,
            Options = new List<QuestionOption> { new QuestionOption { Id = Guid.NewGuid(), IsCorrect = true, Content = "Opt2" } }
        };

        var unattemptedQuiz = new Quiz
        {
            Id = Guid.NewGuid(),
            Name = "Unattempted Quiz",
            Questions = new List<Question> { quiz2Question },
            Attempts = new List<QuizAttempt>()
        };

        var result = CoursePreparationCalculationHelper.CalculatePreparation(
            new List<Quiz> { quiz1, unattemptedQuiz },
            new List<FlashcardDeck>()
        );

        result.QuizProgressPercentage.Should().Be(50);
        result.PreparationPercentage.Should().Be(50);
    }
}
