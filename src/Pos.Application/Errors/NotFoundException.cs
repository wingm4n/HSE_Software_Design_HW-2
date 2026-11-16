namespace Pos.Application.Errors;

public sealed class NotFoundException : UseCaseException
{
    public NotFoundException(string what, object key)
        : base($"{what} '{key}' was not found.")
    {
    }
}
