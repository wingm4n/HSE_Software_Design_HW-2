using Pos.Application.Dtos;
using Pos.Application.Errors;
using Pos.Application.Mapping;
using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Application.Services;

public sealed class EvaluationService(
    IStudentRepository students,
    IRubricRepository rubrics,
    IEvaluationRepository evaluations,
    TimeProvider clock)
{
    public EvaluationDto Grade(Guid studentId, Guid criterionId, decimal score, string? comment = null)
    {
        Student student = FindStudent(new StudentId(studentId));
        Rubric rubric = rubrics.GetCurrent();
        var criterion = new CriterionId(criterionId);

        // One evaluation per student and criterion: a wrong score is corrected, not added twice.
        if (evaluations.Find(student.Id, criterion) is not null)
        {
            throw new ConflictException("The student already has an evaluation for this criterion, correct it instead.");
        }

        Evaluation evaluation = rubric.CreateEvaluation(student.Id, criterion, Score.Of(score), clock.GetUtcNow(), comment);
        evaluations.Save(evaluation);
        return evaluation.ToDto(student, rubric.GetCriterion(criterion));
    }

    public IReadOnlyList<EvaluationDto> GetAll(Guid? studentId = null)
    {
        IReadOnlyList<Evaluation> items = studentId is null
            ? evaluations.GetAll()
            : evaluations.GetByStudent(new StudentId(studentId.Value));

        Rubric rubric = rubrics.GetCurrent();
        Dictionary<StudentId, Student> studentsById = students.GetAll().ToDictionary(s => s.Id);

        return items
            .Select(e => e.ToDto(studentsById[e.StudentId], rubric.GetCriterion(e.CriterionId)))
            .OrderBy(e => e.StudentName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(e => e.CriterionName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public EvaluationDto Correct(Guid evaluationId, decimal newScore, string? comment = null)
    {
        Evaluation evaluation = FindEvaluation(evaluationId);
        Criterion criterion = rubrics.GetCurrent().GetCriterion(evaluation.CriterionId);
        Student student = FindStudent(evaluation.StudentId);

        evaluation.Revise(criterion, Score.Of(newScore), clock.GetUtcNow(), comment);
        evaluations.Save(evaluation);
        return evaluation.ToDto(student, criterion);
    }

    public void Cancel(Guid evaluationId)
    {
        evaluations.Remove(FindEvaluation(evaluationId).Id);
    }

    private Student FindStudent(StudentId id) =>
        students.FindById(id) ?? throw new NotFoundException("Student", id);

    private Evaluation FindEvaluation(Guid id) =>
        evaluations.FindById(new EvaluationId(id)) ?? throw new NotFoundException("Evaluation", id);
}
