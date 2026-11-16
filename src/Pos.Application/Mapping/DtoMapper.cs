using Pos.Application.Dtos;
using Pos.Domain.Evaluations;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Application.Mapping;

internal static class DtoMapper
{
    public static StudentDto ToDto(this Student student) =>
        new(student.Id.Value, student.Name.Value);

    public static CriterionDto ToDto(this Criterion criterion) =>
        new(criterion.Id.Value, criterion.Name, criterion.MaxScore.Value, criterion.Weight.Value);

    public static RubricDto ToDto(this Rubric rubric) =>
        new(rubric.Title, rubric.Criteria.Select(c => c.ToDto()).ToList());

    public static EvaluationDto ToDto(this Evaluation evaluation, Student student, Criterion criterion) =>
        new(
            evaluation.Id.Value,
            student.Id.Value,
            student.Name.Value,
            criterion.Id.Value,
            criterion.Name,
            evaluation.Score.Value,
            criterion.MaxScore.Value,
            evaluation.GradedAt,
            evaluation.Comment);

    public static FinalGradeDto ToDto(this FinalGrade grade, Student student) =>
        new(student.Id.Value, student.Name.Value, grade.Percent, grade.GradedCriteria, grade.TotalCriteria);

    public static GradingRuleDto ToDto(this IFinalGradeRule rule) =>
        new(rule.Name, rule.Description);
}
