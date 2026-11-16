using Pos.Application.Dtos;
using Pos.Application.Errors;
using Pos.Application.Facade;
using Pos.ConsoleApp.Io;
using Pos.Domain.Exceptions;

namespace Pos.ConsoleApp;

/// <summary>
/// End-to-end scenario: rubric and students, evaluations, rule violations, final grades, export.
/// Works through the same facade as the menu and leaves its data loaded.
/// </summary>
public sealed class DemoScenario(IPosFacade facade, ConsoleIO io)
{
    public void Run()
    {
        if (facade.GetStudents().Count > 0 || facade.GetRubric().Criteria.Count > 0)
        {
            io.WriteError("The demo needs empty data. Restart the application to run it.");
            return;
        }

        Step("1. Rubric (assembled by RubricBuilder)");
        RubricDto rubric = facade.DefineRubric("Software Design",
        [
            new CriterionSpec("Homework", MaxScore: 10, Weight: 3),
            new CriterionSpec("Project", MaxScore: 20, Weight: 5),
            new CriterionSpec("Exam", MaxScore: 10, Weight: 2),
        ]);
        CriterionDto homework = rubric.Criteria[0];
        CriterionDto project = rubric.Criteria[1];
        CriterionDto exam = rubric.Criteria[2];
        foreach (CriterionDto c in rubric.Criteria)
        {
            io.WriteLine($"  {c.Name}: max {TextTable.Number(c.MaxScore)}, weight {TextTable.Number(c.Weight)}");
        }

        Step("2. Students");
        StudentDto alice = facade.AddStudent("Alice Ivanova");
        StudentDto boris = facade.AddStudent("Boris Petrov");
        StudentDto chen = facade.AddStudent("Chen Li");
        io.WriteLine($"  {string.Join(", ", facade.GetStudents().Select(s => s.Name))}");

        Step("3. Evaluations");
        facade.GradeStudent(alice.Id, homework.Id, 9, "Great work");
        facade.GradeStudent(alice.Id, project.Id, 18);
        facade.GradeStudent(alice.Id, exam.Id, 8);
        EvaluationDto borisHomework = facade.GradeStudent(boris.Id, homework.Id, 6);
        EvaluationDto borisProject = facade.GradeStudent(boris.Id, project.Id, 12);
        facade.GradeStudent(chen.Id, homework.Id, 10);
        io.WriteLine($"  {facade.GetEvaluations().Count} evaluations saved.");

        Step("4. Invalid evaluations are rejected");
        ExpectRejection("12 points out of 10", () => facade.GradeStudent(boris.Id, exam.Id, 12));
        ExpectRejection("a criterion that is not in the rubric", () => facade.GradeStudent(boris.Id, Guid.NewGuid(), 5));
        ExpectRejection("a negative score", () => facade.GradeStudent(boris.Id, exam.Id, -1));
        ExpectRejection("a second evaluation for the same criterion", () => facade.GradeStudent(alice.Id, exam.Id, 5));

        Step("5. Correct and cancel");
        facade.CorrectEvaluation(borisHomework.Id, 7, "Re-checked");
        facade.CancelEvaluation(borisProject.Id);
        io.WriteLine("  Boris: homework corrected from 6 to 7, project evaluation cancelled.");

        Step("6. Final grades by both rules");
        PrintFinalGrades();

        string rule = facade.GradingRules[0].Name;
        Step($"7. Export of final grades (rule {rule})");
        foreach (string format in facade.ExportFormats)
        {
            io.WriteLine($"[{format}]");
            facade.ExportResults(format, rule, io.Out);
        }

        io.WriteLine();
        io.WriteLine("Demo finished.");
    }

    private void PrintFinalGrades()
    {
        IReadOnlyList<GradingRuleDto> rules = facade.GradingRules;
        string[] headers = ["Student", .. rules.Select(r => $"{r.Name}, %"), "Graded"];

        List<string[]> rows = [];
        foreach (StudentDto student in facade.GetStudents())
        {
            List<FinalGradeDto> grades = rules.Select(r => facade.GetFinalGrade(student.Id, r.Name)).ToList();
            rows.Add([
                student.Name,
                .. grades.Select(g => TextTable.Number(g.Percent)),
                $"{grades[0].GradedCriteria}/{grades[0].TotalCriteria}",
            ]);
        }

        io.WriteLine(TextTable.Render(headers, rows));
    }

    private void Step(string title)
    {
        io.WriteLine();
        io.WriteLine($"--- {title} ---");
    }

    private void ExpectRejection(string what, Action action)
    {
        try
        {
            action();
            io.WriteError($"{what}: accepted, but should not be");
        }
        catch (Exception ex) when (ex is DomainException or UseCaseException)
        {
            io.WriteLine($"  {what}: rejected. {ex.Message}");
        }
    }
}
