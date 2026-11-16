using Pos.Application.Dtos;
using Pos.Application.Facade;
using Pos.ConsoleApp.Io;
using Pos.ConsoleApp.Menu;

namespace Pos.ConsoleApp.Screens;

public sealed class EvaluationsScreen(IPosFacade facade, ConsoleIO io) : IMenuScreen
{
    public string Title => "Evaluations";

    public IReadOnlyList<MenuItem> Items =>
    [
        new("Grade a student", Grade),
        new("List evaluations", List),
        new("Correct an evaluation", Correct),
        new("Cancel an evaluation", Cancel),
    ];

    private void Grade()
    {
        if (!io.TryChoose("Student?", facade.GetStudents(), s => s.Name, out StudentDto? student)
            || !io.TryChoose("Criterion?", facade.GetRubric().Criteria, c => $"{c.Name} (max {TextTable.Number(c.MaxScore)})", out CriterionDto? criterion))
        {
            return;
        }

        EvaluationDto evaluation = facade.GradeStudent(student.Id, criterion.Id, io.PromptDecimal("Score"), io.PromptOptional("Comment (optional)"));
        io.WriteLine($"Saved: {Describe(evaluation)}");
    }

    private void List()
    {
        IReadOnlyList<EvaluationDto> evaluations = facade.GetEvaluations();
        io.WriteLine(evaluations.Count == 0
            ? "No evaluations yet."
            : TextTable.Render(["Student", "Criterion", "Score", "Date", "Comment"], evaluations.Select(Row).ToList()));
    }

    private void Correct()
    {
        if (io.TryChoose("Which evaluation?", facade.GetEvaluations(), Describe, out EvaluationDto? evaluation))
        {
            EvaluationDto corrected = facade.CorrectEvaluation(evaluation.Id, io.PromptDecimal("New score"), io.PromptOptional("Comment (optional)"));
            io.WriteLine($"Corrected: {Describe(corrected)}");
        }
    }

    private void Cancel()
    {
        if (io.TryChoose("Which evaluation?", facade.GetEvaluations(), Describe, out EvaluationDto? evaluation)
            && io.Confirm($"Cancel '{Describe(evaluation)}'?"))
        {
            facade.CancelEvaluation(evaluation.Id);
            io.WriteLine("Cancelled.");
        }
    }

    private static string Describe(EvaluationDto e) =>
        $"{e.StudentName} - {e.CriterionName}: {TextTable.Number(e.Score)}/{TextTable.Number(e.MaxScore)}";

    private static string[] Row(EvaluationDto e) =>
    [
        e.StudentName,
        e.CriterionName,
        $"{TextTable.Number(e.Score)}/{TextTable.Number(e.MaxScore)}",
        e.GradedAt.ToString("yyyy-MM-dd"),
        e.Comment ?? string.Empty,
    ];
}
