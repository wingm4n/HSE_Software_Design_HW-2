namespace Pos.Application.Errors;

/// <summary>Base type for scenario errors, as opposed to domain rule violations.</summary>
public abstract class UseCaseException : Exception
{
    protected UseCaseException(string message)
        : base(message)
    {
    }
}
