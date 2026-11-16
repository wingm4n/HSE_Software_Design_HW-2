using Moq;
using Pos.Application.Dtos;
using Pos.Application.Errors;
using Pos.Application.Services;
using Pos.Domain.Evaluations;
using Pos.Domain.Exceptions;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Pos.Tests.TestSupport;

namespace Pos.Tests.Application;

public class RubricServiceTests
{
    private readonly Mock<IRubricRepository> _rubrics = new();
    private readonly Mock<IEvaluationRepository> _evaluations = new();
    private readonly Rubric _rubric = TestData.CourseRubric();

    public RubricServiceTests()
    {
        _rubrics.Setup(r => r.GetCurrent()).Returns(_rubric);
        _evaluations.Setup(r => r.GetAll()).Returns([]);
    }

    private RubricService CreateService() => new(_rubrics.Object, _evaluations.Object);

    [Fact]
    public void Define_ValidCriteria_SavesNewRubric()
    {
        RubricDto dto = CreateService().Define("Course", [new CriterionSpec("Homework", 10, 3), new CriterionSpec("Exam", 20, 5)]);

        Assert.Equal(2, dto.Criteria.Count);
        _rubrics.Verify(r => r.Save(It.Is<Rubric>(x => x.Criteria.Count == 2)), Times.Once);
    }

    [Fact]
    public void Define_DuplicateNames_ThrowsAndSavesNothing()
    {
        Assert.Throws<DuplicateCriterionException>(() =>
            CreateService().Define("Course", [new CriterionSpec("Exam", 10, 1), new CriterionSpec("exam", 10, 1)]));
        _rubrics.Verify(r => r.Save(It.IsAny<Rubric>()), Times.Never);
    }

    [Fact]
    public void Define_WhenEvaluationsExist_ThrowsConflict()
    {
        _evaluations.Setup(r => r.GetAll()).Returns([TestData.Grade(_rubric, StudentId.New(), 0, 5)]);

        Assert.Throws<ConflictException>(() => CreateService().Define("Course", [new CriterionSpec("Exam", 10, 1)]));
        _rubrics.Verify(r => r.Save(It.IsAny<Rubric>()), Times.Never);
    }

    [Fact]
    public void RemoveCriterion_AlsoRemovesItsEvaluations()
    {
        Criterion exam = _rubric.Criteria[2];
        _evaluations.Setup(r => r.RemoveByCriterion(exam.Id)).Returns(2);

        int removed = CreateService().RemoveCriterion(exam.Id.Value);

        Assert.Equal(2, removed);
        Assert.DoesNotContain(exam, _rubric.Criteria);
        _rubrics.Verify(r => r.Save(_rubric), Times.Once);
    }
}
