using System.Globalization;
using Pos.Application.Abstractions;
using Pos.Application.Dtos;

namespace Pos.Infrastructure.Export;

public sealed class CsvResultExporter : IResultExporter
{
    public string Format => "csv";

    public void Export(IReadOnlyList<FinalGradeDto> results, TextWriter output)
    {
        output.WriteLine("student,percent,graded_criteria,total_criteria");
        foreach (FinalGradeDto result in results)
        {
            output.WriteLine(string.Join(
                ',',
                Escape(result.StudentName),
                result.Percent.ToString("0.##", CultureInfo.InvariantCulture),
                result.GradedCriteria,
                result.TotalCriteria));
        }
    }

    // A value with a comma or a quote is wrapped in quotes, inner quotes are doubled.
    private static string Escape(string value) =>
        value.Contains(',') || value.Contains('"') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
}
