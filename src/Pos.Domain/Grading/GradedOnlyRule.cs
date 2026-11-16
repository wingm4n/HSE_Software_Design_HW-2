namespace Pos.Domain.Grading;

/// <summary>Current progress: weighted average over the graded criteria only.</summary>
public sealed class GradedOnlyRule : IFinalGradeRule
{
    public string Name => "graded-only";

    public string Description => "weighted average over graded criteria only (current progress)";

    public decimal CalculatePercent(IReadOnlyList<CriterionResult> results)
    {
        List<CriterionResult> graded = results.Where(r => r.IsGraded).ToList();
        if (graded.Count == 0)
        {
            return 0m;
        }

        decimal gradedWeight = graded.Sum(r => r.Weight.Value);
        return graded.Sum(r => r.Weight.Value * r.Ratio) / gradedWeight * 100m;
    }
}
