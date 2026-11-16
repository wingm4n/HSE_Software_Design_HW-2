namespace Pos.Domain.Exceptions;

public sealed class EmptyRubricException : DomainException
{
    public EmptyRubricException()
        : base("The rubric has no criteria, so a final grade cannot be calculated.")
    {
    }
}
