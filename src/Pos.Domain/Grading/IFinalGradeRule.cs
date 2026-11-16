namespace Pos.Domain.Grading;

/// <summary>Strategy: a rule that turns the criterion results of one student into a final grade in percent.</summary>
public interface IFinalGradeRule
{
    string Name { get; }

    string Description { get; }

    decimal CalculatePercent(IReadOnlyList<CriterionResult> results);
}
