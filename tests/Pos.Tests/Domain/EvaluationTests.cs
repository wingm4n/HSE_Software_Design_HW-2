using Pos.Domain.Evaluations;
using Pos.Domain.Exceptions;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Pos.Tests.TestSupport;

namespace Pos.Tests.Domain;

public class EvaluationTests
{
    private readonly Rubric _rubric = TestData.CourseRubric();

    [Fact]
    public void Revise_ValidScore_UpdatesScoreDateAndComment()
    {
        Evaluation evaluation = TestData.Grade(_rubric, StudentId.New(), 0, 5);
        DateTimeOffset later = TestData.Now.AddDays(1);

        evaluation.Revise(_rubric.Criteria[0], Score.Of(9), later, "re-checked");

        Assert.Equal(9m, evaluation.Score.Value);
        Assert.Equal(later, evaluation.GradedAt);
        Assert.Equal("re-checked", evaluation.Comment);
    }

    [Fact]
    public void Revise_ScoreAboveMax_Throws_AndKeepsOldScore()
    {
        Evaluation evaluation = TestData.Grade(_rubric, StudentId.New(), 0, 5);

        Assert.Throws<ScoreOutOfRangeException>(() => evaluation.Revise(_rubric.Criteria[0], Score.Of(11), TestData.Now));
        Assert.Equal(5m, evaluation.Score.Value);
    }

    [Fact]
    public void Revise_WithCriterionOfAnotherEvaluation_Throws()
    {
        Evaluation evaluation = TestData.Grade(_rubric, StudentId.New(), 0, 5);

        Assert.Throws<ArgumentException>(() => evaluation.Revise(_rubric.Criteria[1], Score.Of(5), TestData.Now));
    }
}
