namespace Pos.Domain.Rubrics;

public readonly record struct CriterionId(Guid Value)
{
    public static CriterionId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
