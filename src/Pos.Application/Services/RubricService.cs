using Pos.Application.Dtos;
using Pos.Application.Errors;
using Pos.Application.Mapping;
using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;

namespace Pos.Application.Services;

public sealed class RubricService(IRubricRepository rubrics, IEvaluationRepository evaluations)
{
    public RubricDto Get() => rubrics.GetCurrent().ToDto();

    public RubricDto Define(string title, IReadOnlyList<CriterionSpec> criteria)
    {
        // A new rubric has new criteria, so existing evaluations would point to nothing.
        if (evaluations.GetAll().Count > 0)
        {
            throw new ConflictException("The rubric cannot be redefined while evaluations exist.");
        }

        RubricBuilder builder = new RubricBuilder().WithTitle(title);
        foreach (CriterionSpec spec in criteria)
        {
            builder.WithCriterion(spec.Name, spec.MaxScore, spec.Weight);
        }

        Rubric rubric = builder.Build();
        rubrics.Save(rubric);
        return rubric.ToDto();
    }

    public CriterionDto AddCriterion(string name, decimal maxScore, decimal weight)
    {
        Rubric rubric = rubrics.GetCurrent();
        Criterion criterion = rubric.AddCriterion(name, MaxScore.Of(maxScore), Weight.Of(weight));
        rubrics.Save(rubric);
        return criterion.ToDto();
    }

    public CriterionDto UpdateCriterion(Guid criterionId, string newName, decimal newWeight)
    {
        Rubric rubric = rubrics.GetCurrent();
        Criterion criterion = rubric.UpdateCriterion(new CriterionId(criterionId), newName, Weight.Of(newWeight));
        rubrics.Save(rubric);
        return criterion.ToDto();
    }

    /// <returns>Number of evaluations removed together with the criterion.</returns>
    public int RemoveCriterion(Guid criterionId)
    {
        Rubric rubric = rubrics.GetCurrent();
        var id = new CriterionId(criterionId);

        rubric.RemoveCriterion(id);
        rubrics.Save(rubric);
        return evaluations.RemoveByCriterion(id);
    }
}
