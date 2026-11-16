namespace Pos.Domain.Rubrics;

/// <summary>
/// Builder: collects the title and criteria step by step and creates the rubric in one go.
/// All checks happen in <see cref="Build"/>, so a half-filled rubric never exists.
/// </summary>
public sealed class RubricBuilder
{
    private readonly List<(string Name, decimal MaxScore, decimal Weight)> _criteria = [];
    private string? _title;

    public RubricBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public RubricBuilder WithCriterion(string name, decimal maxScore, decimal weight)
    {
        _criteria.Add((name, maxScore, weight));
        return this;
    }

    public Rubric Build()
    {
        Rubric rubric = Rubric.Create(_title);
        foreach ((string name, decimal maxScore, decimal weight) in _criteria)
        {
            rubric.AddCriterion(name, MaxScore.Of(maxScore), Weight.Of(weight));
        }

        return rubric;
    }
}
