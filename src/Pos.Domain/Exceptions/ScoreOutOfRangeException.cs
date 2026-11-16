using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Exceptions;

public sealed class ScoreOutOfRangeException : DomainException
{
    public ScoreOutOfRangeException(Score score, MaxScore maxScore, string criterionName)
        : base($"Score {score} is outside the range [0, {maxScore}] of criterion '{criterionName}'.")
    {
    }
}
