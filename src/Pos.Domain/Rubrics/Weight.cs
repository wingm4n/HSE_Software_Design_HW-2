using System.Globalization;
using Pos.Domain.Exceptions;

namespace Pos.Domain.Rubrics;

/// <summary>
/// Value object: contribution of a criterion to the final grade. Strictly positive.
/// Weights are relative, so they do not have to add up to 100.
/// </summary>
public sealed record Weight
{
    private Weight(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static Weight Of(decimal value)
    {
        if (value <= 0)
        {
            throw new InvalidValueException("Weight must be greater than zero.");
        }

        return new Weight(value);
    }

    public override string ToString() => Value.ToString("0.##", CultureInfo.InvariantCulture);
}
