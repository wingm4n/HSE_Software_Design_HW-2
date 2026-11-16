namespace Pos.Domain.Rubrics;

public readonly record struct RubricId(Guid Value)
{
    public static RubricId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
