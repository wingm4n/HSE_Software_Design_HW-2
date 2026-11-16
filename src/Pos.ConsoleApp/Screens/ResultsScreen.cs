using System.Diagnostics.CodeAnalysis;
using Pos.Application.Dtos;
using Pos.Application.Facade;
using Pos.ConsoleApp.Io;
using Pos.ConsoleApp.Menu;

namespace Pos.ConsoleApp.Screens;

public sealed class ResultsScreen(IPosFacade facade, ConsoleIO io) : IMenuScreen
{
    public string Title => "Final grades and export";

    public IReadOnlyList<MenuItem> Items =>
    [
        new("Final grades of all students", ShowAll),
        new("Final grade of one student", ShowOne),
        new("Export final grades", Export),
    ];

    private void ShowAll()
    {
        if (!TryChooseRule(out GradingRuleDto? rule))
        {
            return;
        }

        IReadOnlyList<FinalGradeDto> grades = facade.GetFinalGrades(rule.Name);
        io.WriteLine(grades.Count == 0
            ? "No students yet."
            : TextTable.Render(
                ["Student", "Final, %", "Graded"],
                grades.Select(g => new[] { g.StudentName, TextTable.Number(g.Percent), $"{g.GradedCriteria}/{g.TotalCriteria}" }).ToList()));
    }

    private void ShowOne()
    {
        if (!io.TryChoose("Student?", facade.GetStudents(), s => s.Name, out StudentDto? student)
            || !TryChooseRule(out GradingRuleDto? rule))
        {
            return;
        }

        FinalGradeDto grade = facade.GetFinalGrade(student.Id, rule.Name);
        foreach (EvaluationDto e in facade.GetEvaluations(student.Id))
        {
            io.WriteLine($"  {e.CriterionName}: {TextTable.Number(e.Score)}/{TextTable.Number(e.MaxScore)}");
        }

        io.WriteLine($"{grade.StudentName}: {TextTable.Number(grade.Percent)}% (graded {grade.GradedCriteria} of {grade.TotalCriteria} criteria)");
    }

    private void Export()
    {
        if (!io.TryChoose("Format?", facade.ExportFormats, f => f, out string? format)
            || !TryChooseRule(out GradingRuleDto? rule))
        {
            return;
        }

        string? path = io.PromptOptional("File path (empty to print here)");
        if (path is null)
        {
            facade.ExportResults(format, rule.Name, io.Out);
            return;
        }

        using (StreamWriter writer = new(path))
        {
            facade.ExportResults(format, rule.Name, writer);
        }

        io.WriteLine($"Saved to {Path.GetFullPath(path)}");
    }

    private bool TryChooseRule([NotNullWhen(true)] out GradingRuleDto? rule) =>
        io.TryChoose("Grading rule?", facade.GradingRules, r => $"{r.Name}: {r.Description}", out rule);
}
