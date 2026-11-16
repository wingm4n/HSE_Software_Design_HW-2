namespace Pos.Domain.Evaluations;

public readonly record struct EvaluationId(Guid Value)
{
    public static EvaluationId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
