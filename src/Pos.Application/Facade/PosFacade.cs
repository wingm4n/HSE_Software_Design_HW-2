using Pos.Application.Dtos;
using Pos.Application.Services;

namespace Pos.Application.Facade;

public sealed class PosFacade(
    StudentService students,
    RubricService rubric,
    EvaluationService evaluations,
    ResultService results) : IPosFacade
{
    public StudentDto AddStudent(string fullName) => students.Add(fullName);

    public IReadOnlyList<StudentDto> GetStudents() => students.GetAll();

    public StudentDto RenameStudent(Guid studentId, string newFullName) => students.Rename(studentId, newFullName);

    public int RemoveStudent(Guid studentId) => students.Remove(studentId);

    public RubricDto DefineRubric(string title, IReadOnlyList<CriterionSpec> criteria) => rubric.Define(title, criteria);

    public RubricDto GetRubric() => rubric.Get();

    public CriterionDto AddCriterion(string name, decimal maxScore, decimal weight) =>
        rubric.AddCriterion(name, maxScore, weight);

    public CriterionDto UpdateCriterion(Guid criterionId, string newName, decimal newWeight) =>
        rubric.UpdateCriterion(criterionId, newName, newWeight);

    public int RemoveCriterion(Guid criterionId) => rubric.RemoveCriterion(criterionId);

    public EvaluationDto GradeStudent(Guid studentId, Guid criterionId, decimal score, string? comment = null) =>
        evaluations.Grade(studentId, criterionId, score, comment);

    public IReadOnlyList<EvaluationDto> GetEvaluations(Guid? studentId = null) => evaluations.GetAll(studentId);

    public EvaluationDto CorrectEvaluation(Guid evaluationId, decimal newScore, string? comment = null) =>
        evaluations.Correct(evaluationId, newScore, comment);

    public void CancelEvaluation(Guid evaluationId) => evaluations.Cancel(evaluationId);

    public IReadOnlyList<GradingRuleDto> GradingRules => results.Rules;

    public FinalGradeDto GetFinalGrade(Guid studentId, string rule) => results.Calculate(studentId, rule);

    public IReadOnlyList<FinalGradeDto> GetFinalGrades(string rule) => results.CalculateAll(rule);

    public IReadOnlyList<string> ExportFormats => results.ExportFormats;

    public void ExportResults(string format, string rule, TextWriter output) => results.Export(format, rule, output);
}
