using Pos.Domain.Rubrics;

namespace Pos.Infrastructure.Persistence;

public sealed class InMemoryRubricRepository : IRubricRepository
{
    // The app starts with an empty rubric, criteria are added from the menu.
    private Rubric _current = new RubricBuilder().WithTitle("Course rubric").Build();

    public Rubric GetCurrent() => _current;

    public void Save(Rubric rubric) => _current = rubric;
}
