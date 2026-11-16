using Pos.Domain.Evaluations;
using Pos.Domain.Exceptions;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Pos.Tests.TestSupport;

namespace Pos.Tests.Domain;

public class RubricTests
{
    private static Rubric EmptyRubric() => new RubricBuilder().WithTitle("Course").Build();

    [Fact]
    public void Builder_CreatesRubricWithAllCriteria()
    {
        Rubric rubric = TestData.CourseRubric();

        Assert.Equal("Software Design", rubric.Title);
        Assert.Equal(["Homework", "Project", "Exam"], rubric.Criteria.Select(c => c.Name));
    }

    [Fact]
    public void Builder_WithoutTitle_Throws()
    {
        Assert.Throws<InvalidValueException>(() => new RubricBuilder().WithCriterion("Homework", 10, 1).Build());
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(10, 0)]
    public void Builder_InvalidMaxScoreOrWeight_Throws(int maxScore, int weight)
    {
        RubricBuilder builder = new RubricBuilder().WithTitle("Course").WithCriterion("Homework", maxScore, weight);

        Assert.Throws<InvalidValueException>(builder.Build);
    }

    [Fact]
    public void AddCriterion_SameNameIgnoringCase_Throws()
    {
        Rubric rubric = EmptyRubric();
        rubric.AddCriterion("Homework", MaxScore.Of(10), Weight.Of(1));

        Assert.Throws<DuplicateCriterionException>(() => rubric.AddCriterion(" homework ", MaxScore.Of(5), Weight.Of(1)));
        Assert.Single(rubric.Criteria);
    }

    [Fact]
    public void UpdateCriterion_ChangesNameAndWeight_ButNotMaxScore()
    {
        Rubric rubric = EmptyRubric();
        Criterion criterion = rubric.AddCriterion("Homework", MaxScore.Of(10), Weight.Of(1));

        rubric.UpdateCriterion(criterion.Id, "Home assignments", Weight.Of(4));

        Assert.Equal("Home assignments", criterion.Name);
        Assert.Equal(4m, criterion.Weight.Value);
        Assert.Equal(10m, criterion.MaxScore.Value);
    }

    [Fact]
    public void UpdateCriterion_NameOfAnotherCriterion_Throws()
    {
        Rubric rubric = EmptyRubric();
        rubric.AddCriterion("Homework", MaxScore.Of(10), Weight.Of(1));
        Criterion exam = rubric.AddCriterion("Exam", MaxScore.Of(10), Weight.Of(1));

        Assert.Throws<DuplicateCriterionException>(() => rubric.UpdateCriterion(exam.Id, "HOMEWORK", Weight.Of(1)));
    }

    [Fact]
    public void RemoveCriterion_UnknownId_Throws()
    {
        Assert.Throws<CriterionNotInRubricException>(() => EmptyRubric().RemoveCriterion(CriterionId.New()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8.5)]
    [InlineData(10)]
    public void CreateEvaluation_ScoreWithinRange_IsAllowed(double score)
    {
        Rubric rubric = TestData.CourseRubric();

        Evaluation evaluation = rubric.CreateEvaluation(StudentId.New(), rubric.Criteria[0].Id, Score.Of((decimal)score), TestData.Now, " ok ");

        Assert.Equal((decimal)score, evaluation.Score.Value);
        Assert.Equal("ok", evaluation.Comment);
    }

    [Fact]
    public void CreateEvaluation_ScoreAboveMax_Throws()
    {
        Rubric rubric = TestData.CourseRubric();

        Assert.Throws<ScoreOutOfRangeException>(() =>
            rubric.CreateEvaluation(StudentId.New(), rubric.Criteria[0].Id, Score.Of(12), TestData.Now));
    }

    [Fact]
    public void CreateEvaluation_CriterionNotInRubric_Throws()
    {
        Rubric rubric = TestData.CourseRubric();

        Assert.Throws<CriterionNotInRubricException>(() =>
            rubric.CreateEvaluation(StudentId.New(), CriterionId.New(), Score.Of(5), TestData.Now));
    }
}
