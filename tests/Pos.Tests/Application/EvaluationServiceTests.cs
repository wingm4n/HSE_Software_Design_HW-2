using Moq;
using Pos.Application.Errors;
using Pos.Application.Services;
using Pos.Domain.Evaluations;
using Pos.Domain.Exceptions;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Pos.Tests.TestSupport;

namespace Pos.Tests.Application;

public class EvaluationServiceTests
{
    private readonly Mock<IStudentRepository> _students = new();
    private readonly Mock<IRubricRepository> _rubrics = new();
    private readonly Mock<IEvaluationRepository> _evaluations = new();
    private readonly Rubric _rubric = TestData.CourseRubric();
    private readonly Student _student = Student.Register("Alice");

    public EvaluationServiceTests()
    {
        _students.Setup(r => r.FindById(_student.Id)).Returns(_student);
        _rubrics.Setup(r => r.GetCurrent()).Returns(_rubric);
    }

    // Homework, max 10.
    private Guid Homework => _rubric.Criteria[0].Id.Value;

    private EvaluationService CreateService() =>
        new(_students.Object, _rubrics.Object, _evaluations.Object, TimeProvider.System);

    [Fact]
    public void Grade_ValidScore_SavesEvaluation()
    {
        var dto = CreateService().Grade(_student.Id.Value, Homework, 9, "good");

        Assert.Equal(9m, dto.Score);
        Assert.Equal("Homework", dto.CriterionName);
        _evaluations.Verify(r => r.Save(It.Is<Evaluation>(e => e.Score.Value == 9m && e.StudentId == _student.Id)), Times.Once);
    }

    [Fact]
    public void Grade_ScoreAboveMax_ThrowsAndSavesNothing()
    {
        Assert.Throws<ScoreOutOfRangeException>(() => CreateService().Grade(_student.Id.Value, Homework, 12));
        _evaluations.Verify(r => r.Save(It.IsAny<Evaluation>()), Times.Never);
    }

    [Fact]
    public void Grade_CriterionNotInRubric_ThrowsAndSavesNothing()
    {
        Assert.Throws<CriterionNotInRubricException>(() => CreateService().Grade(_student.Id.Value, Guid.NewGuid(), 5));
        _evaluations.Verify(r => r.Save(It.IsAny<Evaluation>()), Times.Never);
    }

    [Fact]
    public void Grade_UnknownStudent_ThrowsNotFound()
    {
        Assert.Throws<NotFoundException>(() => CreateService().Grade(Guid.NewGuid(), Homework, 5));
    }

    [Fact]
    public void Grade_AlreadyGraded_ThrowsConflict()
    {
        Evaluation existing = TestData.Grade(_rubric, _student.Id, 0, 5);
        _evaluations.Setup(r => r.Find(_student.Id, existing.CriterionId)).Returns(existing);

        Assert.Throws<ConflictException>(() => CreateService().Grade(_student.Id.Value, Homework, 7));
        _evaluations.Verify(r => r.Save(It.IsAny<Evaluation>()), Times.Never);
    }

    [Fact]
    public void Correct_ScoreAboveMax_KeepsOldScore()
    {
        Evaluation existing = TestData.Grade(_rubric, _student.Id, 0, 5);
        _evaluations.Setup(r => r.FindById(existing.Id)).Returns(existing);

        Assert.Throws<ScoreOutOfRangeException>(() => CreateService().Correct(existing.Id.Value, 11));
        Assert.Equal(5m, existing.Score.Value);
        _evaluations.Verify(r => r.Save(It.IsAny<Evaluation>()), Times.Never);
    }

    [Fact]
    public void Cancel_UnknownEvaluation_ThrowsNotFound()
    {
        Assert.Throws<NotFoundException>(() => CreateService().Cancel(Guid.NewGuid()));
    }
}
