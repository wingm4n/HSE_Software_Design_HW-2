using Pos.Application.Dtos;

namespace Pos.Application.Facade;

/// <summary>Facade: the single entry point to all scenarios of the module. The UI depends only on this interface.</summary>
public interface IPosFacade
{
    StudentDto AddStudent(string fullName);

    IReadOnlyList<StudentDto> GetStudents();

    StudentDto RenameStudent(Guid studentId, string newFullName);

    int RemoveStudent(Guid studentId);

    RubricDto DefineRubric(string title, IReadOnlyList<CriterionSpec> criteria);

    RubricDto GetRubric();

    CriterionDto AddCriterion(string name, decimal maxScore, decimal weight);

    CriterionDto UpdateCriterion(Guid criterionId, string newName, decimal newWeight);

    int RemoveCriterion(Guid criterionId);

    EvaluationDto GradeStudent(Guid studentId, Guid criterionId, decimal score, string? comment = null);

    IReadOnlyList<EvaluationDto> GetEvaluations(Guid? studentId = null);

    EvaluationDto CorrectEvaluation(Guid evaluationId, decimal newScore, string? comment = null);

    void CancelEvaluation(Guid evaluationId);

    IReadOnlyList<GradingRuleDto> GradingRules { get; }

    FinalGradeDto GetFinalGrade(Guid studentId, string rule);

    IReadOnlyList<FinalGradeDto> GetFinalGrades(string rule);

    IReadOnlyList<string> ExportFormats { get; }

    void ExportResults(string format, string rule, TextWriter output);
}
