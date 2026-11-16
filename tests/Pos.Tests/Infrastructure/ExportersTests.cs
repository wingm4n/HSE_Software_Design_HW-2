using System.Text.Json;
using Pos.Application.Dtos;
using Pos.Infrastructure.Export;

namespace Pos.Tests.Infrastructure;

public class ExportersTests
{
    private static readonly IReadOnlyList<FinalGradeDto> Results =
    [
        new(Guid.NewGuid(), "Alice Ivanova", 88m, 3, 3),
        new(Guid.NewGuid(), "Petrov, Boris", 45.5m, 2, 3),
    ];

    [Fact]
    public void Csv_WritesHeaderAndOneLinePerStudent_EscapingCommas()
    {
        var writer = new StringWriter();

        new CsvResultExporter().Export(Results, writer);

        string[] lines = writer.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Equal("student,percent,graded_criteria,total_criteria", lines[0]);
        Assert.Equal("Alice Ivanova,88,3,3", lines[1]);
        Assert.Equal("\"Petrov, Boris\",45.5,2,3", lines[2]);
    }

    [Fact]
    public void Json_WritesArrayWithAllStudents()
    {
        var writer = new StringWriter();

        new JsonResultExporter().Export(Results, writer);

        using JsonDocument document = JsonDocument.Parse(writer.ToString());
        Assert.Equal(2, document.RootElement.GetArrayLength());
        Assert.Equal("Alice Ivanova", document.RootElement[0].GetProperty("studentName").GetString());
    }
}
