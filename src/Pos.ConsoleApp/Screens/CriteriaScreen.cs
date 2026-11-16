using Pos.Application.Dtos;
using Pos.Application.Facade;
using Pos.ConsoleApp.Io;
using Pos.ConsoleApp.Menu;

namespace Pos.ConsoleApp.Screens;

public sealed class CriteriaScreen(IPosFacade facade, ConsoleIO io) : IMenuScreen
{
    public string Title => "Criteria (rubric)";

    public IReadOnlyList<MenuItem> Items =>
    [
        new("Show the rubric", Show),
        new("Add a criterion", Add),
        new("Change a criterion (name, weight)", Update),
        new("Remove a criterion", Remove),
        new("Define a new rubric at once", Define),
    ];

    private void Show()
    {
        RubricDto rubric = facade.GetRubric();
        io.WriteLine($"Rubric: {rubric.Title}");
        io.WriteLine(rubric.Criteria.Count == 0
            ? "No criteria yet."
            : TextTable.Render(
                ["#", "Criterion", "Max score", "Weight"],
                rubric.Criteria
                    .Select((c, i) => new[] { $"{i + 1}", c.Name, TextTable.Number(c.MaxScore), TextTable.Number(c.Weight) })
                    .ToList()));
    }

    private void Add()
    {
        CriterionDto criterion = facade.AddCriterion(io.Prompt("Criterion name"), io.PromptDecimal("Max score"), io.PromptDecimal("Weight"));
        io.WriteLine($"Added: {Describe(criterion)}");
    }

    private void Update()
    {
        if (io.TryChoose("Which criterion?", facade.GetRubric().Criteria, Describe, out CriterionDto? criterion))
        {
            CriterionDto updated = facade.UpdateCriterion(criterion.Id, io.Prompt("New name"), io.PromptDecimal("New weight"));
            io.WriteLine($"Updated: {Describe(updated)}");
        }
    }

    private void Remove()
    {
        if (io.TryChoose("Which criterion?", facade.GetRubric().Criteria, Describe, out CriterionDto? criterion)
            && io.Confirm($"Remove '{criterion.Name}' together with its evaluations?"))
        {
            io.WriteLine($"Removed. Evaluations deleted: {facade.RemoveCriterion(criterion.Id)}");
        }
    }

    private void Define()
    {
        io.WriteLine("The current rubric will be replaced (only possible while there are no evaluations).");
        string title = io.Prompt("Rubric title");

        List<CriterionSpec> criteria = [];
        do
        {
            criteria.Add(new CriterionSpec(io.Prompt("Criterion name"), io.PromptDecimal("Max score"), io.PromptDecimal("Weight")));
        }
        while (io.Confirm("Add another criterion?"));

        RubricDto rubric = facade.DefineRubric(title, criteria);
        io.WriteLine($"Rubric '{rubric.Title}' defined with {rubric.Criteria.Count} criteria.");
    }

    private static string Describe(CriterionDto c) =>
        $"{c.Name} (max {TextTable.Number(c.MaxScore)}, weight {TextTable.Number(c.Weight)})";
}
