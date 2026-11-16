using Pos.Domain.Rubrics;

namespace Pos.Domain.Exceptions;

public sealed class CriterionNotInRubricException : DomainException
{
    public CriterionNotInRubricException(CriterionId criterionId)
        : base($"Criterion '{criterionId}' does not exist in the rubric.")
    {
    }
}
