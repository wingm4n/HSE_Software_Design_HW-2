namespace Pos.ConsoleApp.Menu;

/// <summary>A sub-menu of the main menu. A new screen is a new class and one registration line.</summary>
public interface IMenuScreen
{
    string Title { get; }

    IReadOnlyList<MenuItem> Items { get; }
}
