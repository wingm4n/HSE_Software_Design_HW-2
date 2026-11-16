using Pos.Application.Abstractions;
using Pos.Application.Dtos;
using Pos.Application.Errors;
using Pos.Application.Mapping;
using Pos.Domain.Evaluations;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Application.Services;

public sealed class ResultService(
    IStudentRepository students,
    IRubricRepository rubrics,
    IEvaluationRepository evaluations,
    FinalGradeCalculator calculator,
    IEnumerable<IFinalGradeRule> rules,
    IEnumerable<IResultExporter> exporters)
{
    public IReadOnlyList<GradingRuleDto> Rules => rules.Select(r => r.ToDto()).ToList();

    public IReadOnlyList<string> ExportFormats => exporters.Select(e => e.Format).ToList();

    public FinalGradeDto Calculate(Guid studentId, string ruleName)
    {
        Student student = students.FindById(new StudentId(studentId)) ?? throw new NotFoundException("Student", studentId);
        return Calculate(student, rubrics.GetCurrent(), FindRule(ruleName));
    }

    public IReadOnlyList<FinalGradeDto> CalculateAll(string ruleName)
    {
        IFinalGradeRule rule = FindRule(ruleName);
        Rubric rubric = rubrics.GetCurrent();

        return students.GetAll()
            .OrderBy(s => s.Name.Value, StringComparer.OrdinalIgnoreCase)
            .Select(s => Calculate(s, rubric, rule))
            .ToList();
    }

    public void Export(string format, string ruleName, TextWriter output)
    {
        IResultExporter exporter = exporters.FirstOrDefault(e => e.Format.Equals(format, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException("Export format", format);

        exporter.Export(CalculateAll(ruleName), output);
    }

    private FinalGradeDto Calculate(Student student, Rubric rubric, IFinalGradeRule rule) =>
        calculator.Calculate(student.Id, rubric, evaluations.GetByStudent(student.Id), rule).ToDto(student);

    private IFinalGradeRule FindRule(string name) =>
        rules.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException("Grading rule", name);
}
