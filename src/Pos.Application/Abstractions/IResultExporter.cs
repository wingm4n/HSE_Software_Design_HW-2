using Pos.Application.Dtos;

namespace Pos.Application.Abstractions;

/// <summary>Output port: writes final grades in some format. Implemented in Infrastructure.</summary>
public interface IResultExporter
{
    string Format { get; }

    void Export(IReadOnlyList<FinalGradeDto> results, TextWriter output);
}
