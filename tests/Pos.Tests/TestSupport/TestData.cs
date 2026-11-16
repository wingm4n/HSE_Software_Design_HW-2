using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Tests.TestSupport;

public static class TestData
{
    public static readonly DateTimeOffset Now = new(2025, 3, 1, 12, 0, 0, TimeSpan.Zero);

    // Homework (max 10, weight 3), Project (max 20, weight 5), Exam (max 10, weight 2).
    public static Rubric CourseRubric() =>
        new RubricBuilder()
            .WithTitle("Software Design")
            .WithCriterion("Homework", maxScore: 10, weight: 3)
            .WithCriterion("Project", maxScore: 20, weight: 5)
            .WithCriterion("Exam", maxScore: 10, weight: 2)
            .Build();

    public static Evaluation Grade(Rubric rubric, StudentId student, int criterionIndex, decimal score) =>
        rubric.CreateEvaluation(student, rubric.Criteria[criterionIndex].Id, Score.Of(score), Now);
}
