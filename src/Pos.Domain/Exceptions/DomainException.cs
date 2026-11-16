namespace Pos.Domain.Exceptions;

/// <summary>Base type for every violation of a domain rule.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message)
    {
    }
}
