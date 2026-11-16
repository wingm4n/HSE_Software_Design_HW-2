namespace Pos.Domain.Grading;

/// <summary>Weighted average over the whole rubric. A criterion without an evaluation counts as zero.</summary>
public sealed class AllCriteriaRule : IFinalGradeRule
{
    public string Name => "all-criteria";

    public string Description => "weighted average over all criteria, a missing evaluation counts as 0";

    public decimal CalculatePercent(IReadOnlyList<CriterionResult> results)
    {
        decimal totalWeight = results.Sum(r => r.Weight.Value);
        return results.Sum(r => r.Weight.Value * r.Ratio) / totalWeight * 100m;
    }
}
