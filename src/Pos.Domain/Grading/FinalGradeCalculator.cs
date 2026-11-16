using Pos.Domain.Evaluations;
using Pos.Domain.Exceptions;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Domain.Grading;

/// <summary>
/// Domain service: the final grade needs both the rubric and the evaluations, so it belongs to neither of them.
/// The formula itself is a replaceable <see cref="IFinalGradeRule"/>.
/// </summary>
public sealed class FinalGradeCalculator
{
    public FinalGrade Calculate(StudentId studentId, Rubric rubric, IEnumerable<Evaluation> evaluations, IFinalGradeRule rule)
    {
        if (rubric.Criteria.Count == 0)
        {
            throw new EmptyRubricException();
        }

        Dictionary<CriterionId, Score> scores = [];
        foreach (Evaluation evaluation in evaluations.Where(e => e.StudentId == studentId))
        {
            // Throws if the evaluation refers to a criterion that is not in the rubric.
            rubric.GetCriterion(evaluation.CriterionId);

            if (!scores.TryAdd(evaluation.CriterionId, evaluation.Score))
            {
                throw new InvalidValueException("A student cannot have two evaluations for one criterion.");
            }
        }

        List<CriterionResult> results = rubric.Criteria
            .Select(c => new CriterionResult(c.MaxScore, c.Weight, scores.GetValueOrDefault(c.Id)))
            .ToList();

        decimal percent = Math.Round(rule.CalculatePercent(results), 2, MidpointRounding.AwayFromZero);
        return new FinalGrade(studentId, percent, scores.Count, results.Count);
    }
}
