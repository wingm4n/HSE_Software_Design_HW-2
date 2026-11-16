using System.Globalization;
using Pos.Domain.Exceptions;

namespace Pos.Domain.Evaluations;

/// <summary>
/// Value object: points for one criterion. Never negative.
/// The upper bound depends on the criterion, see <c>Criterion.EnsureAccepts</c>.
/// </summary>
public sealed record Score
{
    private Score(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static Score Of(decimal value)
    {
        if (value < 0)
        {
            throw new InvalidValueException("Score must not be negative.");
        }

        return new Score(value);
    }

    public override string ToString() => Value.ToString("0.##", CultureInfo.InvariantCulture);
}
