using Microsoft.Extensions.DependencyInjection;
using Pos.ConsoleApp;
using Pos.Tests.TestSupport;

namespace Pos.Tests.EndToEnd;

// Main menu: 1 Students, 2 Criteria, 3 Evaluations, 4 Final grades and export, 5 Demo, 6 Exit.
public class ConsoleAppTests
{
    private static string Run(params string[] lines)
    {
        var output = new StringWriter();
        using ServiceProvider provider = TestHost.Create(string.Join('\n', lines) + '\n', output);
        provider.GetRequiredService<PosConsoleApp>().Run();
        return output.ToString();
    }

    [Fact]
    public void Menu_FromCriterionToFinalGrade()
    {
        string output = Run(
            "2", "2", "Homework", "10", "1", "6",
            "1", "1", "Alice", "5",
            "3", "1", "1", "1", "12", "",
            "1", "1", "1", "9", "", "5",
            "4", "1", "1", "4",
            "6");

        Assert.Contains("Rule violated: Score 12 is outside the range [0, 10]", output);
        Assert.Contains("Saved: Alice - Homework: 9/10", output);
        Assert.Contains("Alice   | 90", output);
    }

    [Fact]
    public void Menu_EveryScreenAction()
    {
        string file = Path.Combine(Path.GetTempPath(), $"pos-{Guid.NewGuid()}.json");

        string output = Run(
            "9",
            "2", "5", "Course", "Homework", "10", "3", "y", "Exam", "20", "1", "n",
            "1", "3", "2", "Final exam", "2", "6",
            "1", "1", "Alice", "1", "Bob", "2", "3", "2", "Boris", "5",
            "3", "1", "1", "1", "abc", "8,5", "good", "1", "2", "2", "15", "",
            "2", "3", "1", "9", "", "4", "2", "y", "5",
            "4", "2", "1", "2", "3", "1", "1", "", "3", "2", "1", file, "4",
            "1", "4", "2", "y", "5",
            "2", "4", "1", "y", "6",
            "6");

        Assert.Contains("Enter a number from 1 to 6.", output);
        Assert.Contains("Updated: Final exam (max 20, weight 2)", output);
        Assert.Contains("Renamed to: Boris", output);
        Assert.Contains("Enter a number, for example 8.5", output);
        Assert.Contains("Corrected: Alice - Homework: 9/10", output);
        Assert.Contains("Cancelled.", output);
        Assert.Contains("Alice: 90% (graded 1 of 2 criteria)", output);
        Assert.Contains("Alice,54,1,2", output);
        Assert.Contains("Saved to", output);
        Assert.Contains("\"studentName\": \"Alice\"", File.ReadAllText(file));
        Assert.Contains("Removed. Evaluations deleted: 1", output);
        File.Delete(file);
    }

    [Fact]
    public void Cancel_AbortsTheCurrentAction()
    {
        string output = Run("1", "1", "cancel", "5", "6");

        Assert.Contains("Cancelled, nothing was changed.", output);
    }

    [Fact]
    public void ClosedInput_StopsTheApp()
    {
        Assert.Contains("Input closed.", Run("1"));
    }

    [Fact]
    public void Demo_RunsWithoutUnexpectedResults_AndOnlyOnEmptyData()
    {
        string output = Run("5", "5", "6");

        Assert.Contains("Demo finished.", output);
        Assert.DoesNotContain("should not be", output);
        Assert.Contains("The demo needs empty data", output);
    }
}
