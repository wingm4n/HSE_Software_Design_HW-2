namespace Pos.Domain.Rubrics;

// The module works with one course, so there is one current rubric.
public interface IRubricRepository
{
    Rubric GetCurrent();

    void Save(Rubric rubric);
}
