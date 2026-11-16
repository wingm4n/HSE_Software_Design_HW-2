using System.Text.Json;
using Pos.Application.Abstractions;
using Pos.Application.Dtos;

namespace Pos.Infrastructure.Export;

public sealed class JsonResultExporter : IResultExporter
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public string Format => "json";

    public void Export(IReadOnlyList<FinalGradeDto> results, TextWriter output)
    {
        output.WriteLine(JsonSerializer.Serialize(results, Options));
    }
}
