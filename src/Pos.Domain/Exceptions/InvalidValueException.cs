namespace Pos.Domain.Exceptions;

/// <summary>A value that the model does not allow: blank name, negative score, zero weight and so on.</summary>
public sealed class InvalidValueException : DomainException
{
    public InvalidValueException(string message)
        : base(message)
    {
    }
}
