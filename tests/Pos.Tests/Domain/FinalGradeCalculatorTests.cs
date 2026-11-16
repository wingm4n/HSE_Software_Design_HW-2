using Pos.Domain.Evaluations;
using Pos.Domain.Exceptions;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Pos.Tests.TestSupport;

namespace Pos.Tests.Domain;

// Rubric: Homework (max 10, weight 3), Project (max 20, weight 5), Exam (max 10, weight 2).
public class FinalGradeCalculatorTests
{
    private readonly FinalGradeCalculator _calculator = new();
    private readonly AllCriteriaRule _allCriteria = new();
    private readonly GradedOnlyRule _gradedOnly = new();
    private readonly Rubric _rubric = TestData.CourseRubric();
    private readonly StudentId _student = StudentId.New();

    private Evaluation Grade(int criterionIndex, decimal score) => TestData.Grade(_rubric, _student, criterionIndex, score);

    [Fact]
    public void AllCriteria_EverythingGraded_IsWeightedAverage()
    {
        // (3 * 0.9 + 5 * 0.9 + 2 * 0.8) / 10 = 0.88
        Evaluation[] evaluations = [Grade(0, 9), Grade(1, 18), Grade(2, 8)];

        FinalGrade grade = _calculator.Calculate(_student, _rubric, evaluations, _allCriteria);

        Assert.Equal(88m, grade.Percent);
        Assert.Equal(3, grade.GradedCriteria);
    }

    [Fact]
    public void AllCriteria_MissingEvaluationCountsAsZero()
    {
        // Only Homework 10/10: 3 / 10 = 30 %.
        FinalGrade grade = _calculator.Calculate(_student, _rubric, [Grade(0, 10)], _allCriteria);

        Assert.Equal(30m, grade.Percent);
        Assert.Equal(1, grade.GradedCriteria);
        Assert.Equal(3, grade.TotalCriteria);
    }

    [Fact]
    public void GradedOnly_IgnoresCriteriaWithoutEvaluation()
    {
        // Homework 10/10 and Exam 5/10: (3 * 1 + 2 * 0.5) / 5 = 80 %.
        FinalGrade grade = _calculator.Calculate(_student, _rubric, [Grade(0, 10), Grade(2, 5)], _gradedOnly);

        Assert.Equal(80m, grade.Percent);
    }

    [Fact]
    public void BothRules_NoEvaluations_GiveZero()
    {
        Assert.Equal(0m, _calculator.Calculate(_student, _rubric, [], _allCriteria).Percent);
        Assert.Equal(0m, _calculator.Calculate(_student, _rubric, [], _gradedOnly).Percent);
    }

    [Fact]
    public void EvaluationsOfOtherStudents_AreIgnored()
    {
        Evaluation other = TestData.Grade(_rubric, StudentId.New(), 0, 10);

        FinalGrade grade = _calculator.Calculate(_student, _rubric, [other, Grade(0, 5)], _allCriteria);

        Assert.Equal(15m, grade.Percent);
    }

    [Fact]
    public void ResultIsRoundedAndWithinZeroAndHundred()
    {
        Evaluation[] evaluations = [Grade(0, 3.333m), Grade(1, 19.999m), Grade(2, 9.999m)];

        decimal percent = _calculator.Calculate(_student, _rubric, evaluations, _allCriteria).Percent;

        Assert.InRange(percent, 0m, 100m);
        Assert.Equal(percent, Math.Round(percent, 2));
    }

    [Fact]
    public void EmptyRubric_Throws()
    {
        Rubric empty = new RubricBuilder().WithTitle("Empty").Build();

        Assert.Throws<EmptyRubricException>(() => _calculator.Calculate(_student, empty, [], _allCriteria));
    }

    [Fact]
    public void EvaluationForCriterionOutsideRubric_Throws()
    {
        Evaluation foreign = TestData.Grade(TestData.CourseRubric(), _student, 0, 5);

        Assert.Throws<CriterionNotInRubricException>(() => _calculator.Calculate(_student, _rubric, [foreign], _allCriteria));
    }

    [Fact]
    public void TwoEvaluationsForOneCriterion_Throw()
    {
        Assert.Throws<InvalidValueException>(() => _calculator.Calculate(_student, _rubric, [Grade(0, 5), Grade(0, 6)], _allCriteria));
    }
}
