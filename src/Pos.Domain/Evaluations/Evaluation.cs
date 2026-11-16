using Pos.Domain.Common;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Domain.Evaluations;

/// <summary>
/// Aggregate root: "this student got this score for this criterion".
/// Refers to the student and the criterion by id. Created only by <see cref="Rubric.CreateEvaluation"/>.
/// </summary>
public sealed class Evaluation : Entity<EvaluationId>
{
    internal Evaluation(EvaluationId id, StudentId studentId, CriterionId criterionId, Score score, DateTimeOffset gradedAt, string? comment)
        : base(id)
    {
        StudentId = studentId;
        CriterionId = criterionId;
        Score = score;
        GradedAt = gradedAt;
        Comment = NormalizeComment(comment);
    }

    public StudentId StudentId { get; }

    public CriterionId CriterionId { get; }

    public Score Score { get; private set; }

    public DateTimeOffset GradedAt { get; private set; }

    public string? Comment { get; private set; }

    // The range depends on the criterion, so the caller passes it in and the evaluation checks it is its own.
    public void Revise(Criterion criterion, Score newScore, DateTimeOffset revisedAt, string? comment = null)
    {
        if (criterion.Id != CriterionId)
        {
            throw new ArgumentException("The criterion does not belong to this evaluation.", nameof(criterion));
        }

        criterion.EnsureAccepts(newScore);
        Score = newScore;
        GradedAt = revisedAt;
        Comment = NormalizeComment(comment);
    }

    private static string? NormalizeComment(string? comment) =>
        string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
}
