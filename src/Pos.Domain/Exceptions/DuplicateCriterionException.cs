namespace Pos.Domain.Exceptions;

public sealed class DuplicateCriterionException : DomainException
{
    public DuplicateCriterionException(string criterionName)
        : base($"Criterion '{criterionName}' already exists in the rubric.")
    {
    }
}
