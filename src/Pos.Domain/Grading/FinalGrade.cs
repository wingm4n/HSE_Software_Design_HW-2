using Pos.Domain.Exceptions;
using Pos.Domain.Students;

namespace Pos.Domain.Grading;

/// <summary>
/// Value object: the final grade of one student in percent.
/// It is never stored, only calculated by <see cref="FinalGradeCalculator"/> from the current evaluations.
/// </summary>
public sealed record FinalGrade
{
    public FinalGrade(StudentId studentId, decimal percent, int gradedCriteria, int totalCriteria)
    {
        if (percent is < 0 or > 100)
        {
            throw new InvalidValueException("Final grade must be between 0 and 100 percent.");
        }

        StudentId = studentId;
        Percent = percent;
        GradedCriteria = gradedCriteria;
        TotalCriteria = totalCriteria;
    }

    public StudentId StudentId { get; }

    public decimal Percent { get; }

    public int GradedCriteria { get; }

    public int TotalCriteria { get; }
}
