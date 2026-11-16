using Pos.Application.Errors;
using Pos.ConsoleApp.Io;
using Pos.ConsoleApp.Menu;
using Pos.Domain.Exceptions;

namespace Pos.ConsoleApp;

/// <summary>Main menu loop and the single place where errors are turned into messages.</summary>
public sealed class PosConsoleApp(ConsoleIO io, IEnumerable<IMenuScreen> screens, DemoScenario demo)
{
    public void Run()
    {
        io.WriteLine("=== POS: Teacher's Evaluation of Students ===");
        io.WriteLine($"Type '{ConsoleIO.CancelKeyword}' at any prompt to abort the current action.");

        bool exit = false;
        List<MenuItem> items = screens
            .Select(s => new MenuItem(s.Title, () => RunScreen(s)))
            .Append(new MenuItem("Run the end-to-end demo", demo.Run))
            .Append(new MenuItem("Exit", () => exit = true))
            .ToList();

        try
        {
            while (!exit)
            {
                ShowAndExecute("Main menu", items);
            }
        }
        catch (EndOfStreamException)
        {
            io.WriteLine("Input closed.");
        }
    }

    public void RunDemo() => Safely(demo.Run);

    private void RunScreen(IMenuScreen screen)
    {
        bool back = false;
        List<MenuItem> items = screen.Items.Append(new MenuItem("Back", () => back = true)).ToList();

        while (!back)
        {
            ShowAndExecute(screen.Title, items);
        }
    }

    private void ShowAndExecute(string title, IReadOnlyList<MenuItem> items)
    {
        io.WriteLine();
        Safely(() =>
        {
            if (io.TryChoose(title, items, i => i.Title, out MenuItem? chosen))
            {
                chosen.Execute();
            }
        });
    }

    // Expected errors keep the app running; closed input is passed on so the main loop can stop.
    private void Safely(Action action)
    {
        try
        {
            action();
        }
        catch (DomainException ex)
        {
            io.WriteError($"Rule violated: {ex.Message}");
        }
        catch (UseCaseException ex)
        {
            io.WriteError(ex.Message);
        }
        catch (OperationCanceledException)
        {
            io.WriteLine("Cancelled, nothing was changed.");
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or (IOException and not EndOfStreamException))
        {
            io.WriteError($"File problem: {ex.Message}");
        }
    }
}
