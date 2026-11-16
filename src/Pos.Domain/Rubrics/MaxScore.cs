using System.Globalization;
using Pos.Domain.Exceptions;

namespace Pos.Domain.Rubrics;

/// <summary>Value object: the maximum score of a criterion. Strictly positive.</summary>
public sealed record MaxScore
{
    private MaxScore(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static MaxScore Of(decimal value)
    {
        if (value <= 0)
        {
            throw new InvalidValueException("Maximum score must be greater than zero.");
        }

        return new MaxScore(value);
    }

    public override string ToString() => Value.ToString("0.##", CultureInfo.InvariantCulture);
}
