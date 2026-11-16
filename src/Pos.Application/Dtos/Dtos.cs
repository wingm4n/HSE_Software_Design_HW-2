namespace Pos.Application.Dtos;

// Plain data for the UI. Domain objects do not leave the application layer.

public sealed record StudentDto(Guid Id, string Name);

public sealed record CriterionDto(Guid Id, string Name, decimal MaxScore, decimal Weight);

public sealed record CriterionSpec(string Name, decimal MaxScore, decimal Weight);

public sealed record RubricDto(string Title, IReadOnlyList<CriterionDto> Criteria);

public sealed record EvaluationDto(
    Guid Id,
    Guid StudentId,
    string StudentName,
    Guid CriterionId,
    string CriterionName,
    decimal Score,
    decimal MaxScore,
    DateTimeOffset GradedAt,
    string? Comment);

public sealed record FinalGradeDto(Guid StudentId, string StudentName, decimal Percent, int GradedCriteria, int TotalCriteria);

public sealed record GradingRuleDto(string Name, string Description);
