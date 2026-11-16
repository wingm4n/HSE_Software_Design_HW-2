using System.Globalization;
using System.Text;

namespace Pos.ConsoleApp.Io;

public static class TextTable
{
    public static string Number(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    public static string Render(IReadOnlyList<string> headers, IReadOnlyList<string[]> rows)
    {
        // Each column is as wide as its longest cell.
        int[] widths = headers
            .Select((h, i) => Math.Max(h.Length, rows.Select(r => r[i].Length).DefaultIfEmpty(0).Max()))
            .ToArray();

        StringBuilder sb = new();
        AppendRow(sb, headers, widths);
        sb.AppendLine(string.Join("-+-", widths.Select(w => new string('-', w))));
        foreach (string[] row in rows)
        {
            AppendRow(sb, row, widths);
        }

        return sb.ToString().TrimEnd();
    }

    private static void AppendRow(StringBuilder sb, IReadOnlyList<string> cells, int[] widths) =>
        sb.AppendLine(string.Join(" | ", cells.Select((c, i) => c.PadRight(widths[i]))).TrimEnd());
}
