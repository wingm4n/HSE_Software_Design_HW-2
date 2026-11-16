namespace Pos.Application.Errors;

/// <summary>The request conflicts with the current state: duplicate student, criterion already graded and so on.</summary>
public sealed class ConflictException : UseCaseException
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
