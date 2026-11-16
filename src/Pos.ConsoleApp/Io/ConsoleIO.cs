using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Pos.ConsoleApp.Io;

/// <summary>Prompts, input checks and retry loops over injected streams, so the UI can be tested without a real console.</summary>
public sealed class ConsoleIO(TextReader input, TextWriter output)
{
    public const string CancelKeyword = "cancel";

    public TextWriter Out => output;

    public void WriteLine(string text = "") => output.WriteLine(text);

    public void WriteError(string text) => output.WriteLine($"  [!] {text}");

    public string Prompt(string label)
    {
        while (true)
        {
            string? answer = PromptOptional(label);
            if (answer is not null)
            {
                return answer;
            }

            WriteError("Value is required.");
        }
    }

    /// <summary>Reads a trimmed line; an empty answer gives null.</summary>
    public string? PromptOptional(string label)
    {
        output.Write($"{label}: ");
        string line = input.ReadLine()?.Trim() ?? throw new EndOfStreamException("Input stream closed.");

        if (line.Equals(CancelKeyword, StringComparison.OrdinalIgnoreCase))
        {
            throw new OperationCanceledException();
        }

        return line.Length == 0 ? null : line;
    }

    // Accepts both '.' and ',' as the decimal separator.
    public decimal PromptDecimal(string label)
    {
        while (true)
        {
            string text = Prompt(label).Replace(',', '.');
            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
            {
                return value;
            }

            WriteError("Enter a number, for example 8.5");
        }
    }

    public bool Confirm(string question) =>
        Prompt($"{question} (y/n)").StartsWith("y", StringComparison.OrdinalIgnoreCase);

    /// <summary>Prints a numbered list and asks for a number. Returns false when the list is empty.</summary>
    public bool TryChoose<T>(string title, IReadOnlyList<T> items, Func<T, string> display, [NotNullWhen(true)] out T? chosen)
        where T : class
    {
        chosen = null;
        if (items.Count == 0)
        {
            WriteLine("Nothing to choose from yet.");
            return false;
        }

        WriteLine(title);
        for (int i = 0; i < items.Count; i++)
        {
            WriteLine($"  {i + 1}) {display(items[i])}");
        }

        while (true)
        {
            if (int.TryParse(Prompt("Number"), out int number) && number >= 1 && number <= items.Count)
            {
                chosen = items[number - 1];
                return true;
            }

            WriteError($"Enter a number from 1 to {items.Count}.");
        }
    }
}
