using Pos.Domain.Common;
using Pos.Domain.Evaluations;
using Pos.Domain.Exceptions;

namespace Pos.Domain.Rubrics;

/// <summary>
/// Entity inside the <see cref="Rubric"/> aggregate. Only the rubric can create or change it.
/// MaxScore cannot be changed: existing evaluations could silently become invalid.
/// </summary>
public sealed class Criterion : Entity<CriterionId>
{
    public const int MaxNameLength = 100;

    internal Criterion(CriterionId id, string name, MaxScore maxScore, Weight weight)
        : base(id)
    {
        Name = NormalizeName(name);
        MaxScore = maxScore;
        Weight = weight;
    }

    public string Name { get; private set; }

    public MaxScore MaxScore { get; }

    public Weight Weight { get; private set; }

    public void EnsureAccepts(Score score)
    {
        // The lower bound is guaranteed by Score itself.
        if (score.Value > MaxScore.Value)
        {
            throw new ScoreOutOfRangeException(score, MaxScore, Name);
        }
    }

    internal void Change(string newName, Weight newWeight)
    {
        Name = NormalizeName(newName);
        Weight = newWeight;
    }

    internal static string NormalizeName(string? name)
    {
        string trimmed = name?.Trim() ?? string.Empty;

        if (trimmed.Length == 0)
        {
            throw new InvalidValueException("Criterion name must not be empty.");
        }

        if (trimmed.Length > MaxNameLength)
        {
            throw new InvalidValueException($"Criterion name must not be longer than {MaxNameLength} characters.");
        }

        return trimmed;
    }
}
