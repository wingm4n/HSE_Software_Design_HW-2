using Pos.Domain.Common;
using Pos.Domain.Evaluations;
using Pos.Domain.Exceptions;
using Pos.Domain.Students;

namespace Pos.Domain.Rubrics;

/// <summary>
/// Aggregate root: the set of criteria a course is graded by.
/// It is the only object that knows which criteria exist, so evaluations are created here.
/// </summary>
public sealed class Rubric : Entity<RubricId>
{
    private readonly List<Criterion> _criteria = [];

    private Rubric(RubricId id, string title)
        : base(id)
    {
        Title = title;
    }

    public string Title { get; }

    public IReadOnlyList<Criterion> Criteria => _criteria;

    // Internal on purpose: rubrics are created through RubricBuilder.
    internal static Rubric Create(string? title)
    {
        string trimmed = title?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            throw new InvalidValueException("Rubric title must not be empty.");
        }

        return new Rubric(RubricId.New(), trimmed);
    }

    public Criterion AddCriterion(string name, MaxScore maxScore, Weight weight)
    {
        string normalized = Criterion.NormalizeName(name);
        EnsureNameIsFree(normalized);

        var criterion = new Criterion(CriterionId.New(), normalized, maxScore, weight);
        _criteria.Add(criterion);
        return criterion;
    }

    public Criterion UpdateCriterion(CriterionId criterionId, string newName, Weight newWeight)
    {
        Criterion criterion = GetCriterion(criterionId);
        string normalized = Criterion.NormalizeName(newName);
        EnsureNameIsFree(normalized, criterionId);

        criterion.Change(normalized, newWeight);
        return criterion;
    }

    // Evaluations of the removed criterion are another aggregate, the application layer deletes them.
    public void RemoveCriterion(CriterionId criterionId)
    {
        _criteria.Remove(GetCriterion(criterionId));
    }

    public Criterion GetCriterion(CriterionId criterionId) =>
        _criteria.Find(c => c.Id == criterionId) ?? throw new CriterionNotInRubricException(criterionId);

    /// <summary>The only way to create an evaluation: the criterion must be in the rubric and accept the score.</summary>
    public Evaluation CreateEvaluation(StudentId studentId, CriterionId criterionId, Score score, DateTimeOffset gradedAt, string? comment = null)
    {
        Criterion criterion = GetCriterion(criterionId);
        criterion.EnsureAccepts(score);
        return new Evaluation(EvaluationId.New(), studentId, criterionId, score, gradedAt, comment);
    }

    private void EnsureNameIsFree(string name, CriterionId? exceptId = null)
    {
        bool taken = _criteria.Exists(c =>
            c.Id != exceptId && string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

        if (taken)
        {
            throw new DuplicateCriterionException(name);
        }
    }
}
