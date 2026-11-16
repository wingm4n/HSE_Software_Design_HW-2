using Pos.Domain.Exceptions;

namespace Pos.Domain.Students;

/// <summary>Value object: full name of a student. Trimmed, not blank, limited in length.</summary>
public sealed record PersonName
{
    public const int MaxLength = 100;

    private PersonName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PersonName Of(string? value)
    {
        string trimmed = value?.Trim() ?? string.Empty;

        if (trimmed.Length == 0)
        {
            throw new InvalidValueException("Student name must not be empty.");
        }

        if (trimmed.Length > MaxLength)
        {
            throw new InvalidValueException($"Student name must not be longer than {MaxLength} characters.");
        }

        return new PersonName(trimmed);
    }

    public override string ToString() => Value;
}
