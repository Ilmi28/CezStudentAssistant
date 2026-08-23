using CezStudentAssistant.Application.Helpers;
using CezStudentAssistant.Domain.Entities;
using CezStudentAssistant.Domain.Enums;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CezStudentAssistant.UnitTests.Application.Helpers;

[TestFixture]
public class QuizMasteryCalculationHelperTests
{
    [Test]
    public void CalculateMastery_ShouldReturnZero_WhenNoQuestionsOrAttempts()
    {
        var result = QuizMasteryCalculationHelper.CalculateMastery(new List<Question>(), new List<QuizAttempt>());

        result.Should().NotBeNull();
        result.MasteredCount.Should().Be(0);
        result.TotalPoolCount.Should().Be(0);
        result.ProgressPercentage.Should().Be(0);
    }

    [Test]
    public void CalculateMastery_ShouldCalculateCorrectPercentage_ForSingleChoiceQuestions()
    {
        var q1Id = Guid.NewGuid();
        var q1Opt1Correct = Guid.NewGuid();
        var q1Opt2 = Guid.NewGuid();

        var q2Id = Guid.NewGuid();
        var q2Opt1Correct = Guid.NewGuid();
        var q2Opt2 = Guid.NewGuid();

        var questions = new List<Question>
        {
            new Question
            {
                Id = q1Id,
                Content = "Single Choice Q1",
                Type = QuestionType.SingleChoice,
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Id = q1Opt1Correct, IsCorrect = true, Content = "Correct" },
                    new QuestionOption { Id = q1Opt2, IsCorrect = false, Content = "Wrong" }
                }
            },
            new Question
            {
                Id = q2Id,
                Content = "Single Choice Q2",
                Type = QuestionType.SingleChoice,
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Id = q2Opt1Correct, IsCorrect = true, Content = "Correct" },
                    new QuestionOption { Id = q2Opt2, IsCorrect = false, Content = "Wrong" }
                }
            }
        };

        var attempts = new List<QuizAttempt>
        {
            new QuizAttempt
            {
                Id = Guid.NewGuid(),
                Status = QuizAttemptStatus.Completed,
                Answers = new List<QuestionAnswer>
                {
                    new QuestionAnswer
                    {
                        QuestionId = q1Id,
                        SelectedOptions = new List<SelectedQuizOption>
                        {
                            new SelectedQuizOption { QuestionOptionId = q1Opt1Correct }
                        }
                    },
                    new QuestionAnswer
                    {
                        QuestionId = q2Id,
                        SelectedOptions = new List<SelectedQuizOption>
                        {
                            new SelectedQuizOption { QuestionOptionId = q2Opt2 } // Incorrect
                        }
                    }
                }
            }
        };

        var result = QuizMasteryCalculationHelper.CalculateMastery(questions, attempts);

        result.MasteredCount.Should().Be(1);
        result.TotalPoolCount.Should().Be(2);
        result.ProgressPercentage.Should().Be(50);
    }

    [Test]
    public void CalculateMastery_ShouldRequireExactSetMatch_ForMultipleChoiceQuestions()
    {
        var qId = Guid.NewGuid();
        var opt1Correct = Guid.NewGuid();
        var opt2Correct = Guid.NewGuid();
        var opt3Wrong = Guid.NewGuid();

        var questions = new List<Question>
        {
            new Question
            {
                Id = qId,
                Content = "Multiple Choice Q",
                Type = QuestionType.MultipleChoice,
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Id = opt1Correct, IsCorrect = true, Content = "Correct 1" },
                    new QuestionOption { Id = opt2Correct, IsCorrect = true, Content = "Correct 2" },
                    new QuestionOption { Id = opt3Wrong, IsCorrect = false, Content = "Wrong" }
                }
            }
        };

        // Partial selection should NOT count as mastered
        var partialAttempt = new List<QuizAttempt>
        {
            new QuizAttempt
            {
                Id = Guid.NewGuid(),
                Status = QuizAttemptStatus.Completed,
                Answers = new List<QuestionAnswer>
                {
                    new QuestionAnswer
                    {
                        QuestionId = qId,
                        SelectedOptions = new List<SelectedQuizOption>
                        {
                            new SelectedQuizOption { QuestionOptionId = opt1Correct } // Missing opt2Correct
                        }
                    }
                }
            }
        };

        var partialResult = QuizMasteryCalculationHelper.CalculateMastery(questions, partialAttempt);
        partialResult.MasteredCount.Should().Be(0);
        partialResult.ProgressPercentage.Should().Be(0);

        // Full exact selection counts as mastered
        var fullAttempt = new List<QuizAttempt>
        {
            new QuizAttempt
            {
                Id = Guid.NewGuid(),
                Status = QuizAttemptStatus.Completed,
                Answers = new List<QuestionAnswer>
                {
                    new QuestionAnswer
                    {
                        QuestionId = qId,
                        SelectedOptions = new List<SelectedQuizOption>
                        {
                            new SelectedQuizOption { QuestionOptionId = opt1Correct },
                            new SelectedQuizOption { QuestionOptionId = opt2Correct }
                        }
                    }
                }
            }
        };

        var fullResult = QuizMasteryCalculationHelper.CalculateMastery(questions, fullAttempt);
        fullResult.MasteredCount.Should().Be(1);
        fullResult.ProgressPercentage.Should().Be(100);
    }
}
