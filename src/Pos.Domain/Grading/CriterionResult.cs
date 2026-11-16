using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Grading;

/// <summary>A criterion of the rubric together with the student's score for it (null if not graded yet).</summary>
public sealed record CriterionResult(MaxScore MaxScore, Weight Weight, Score? Score)
{
    public bool IsGraded => Score is not null;

    // Share of the maximum that was earned, from 0 to 1.
    public decimal Ratio => Score is null ? 0m : Score.Value / MaxScore.Value;
}
