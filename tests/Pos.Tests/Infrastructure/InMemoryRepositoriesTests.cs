using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Pos.Infrastructure.Persistence;
using Pos.Tests.TestSupport;

namespace Pos.Tests.Infrastructure;

public class InMemoryRepositoriesTests
{
    [Fact]
    public void StudentRepository_FindByName_IgnoresCase()
    {
        var repository = new InMemoryStudentRepository();
        Student alice = Student.Register("Alice Ivanova");
        repository.Save(alice);

        Assert.Same(alice, repository.FindByName(PersonName.Of("ALICE ivanova")));
        Assert.Null(repository.FindByName(PersonName.Of("Bob")));
    }

    [Fact]
    public void EvaluationRepository_RemoveByCriterion_RemovesOnlyThatCriterion()
    {
        var repository = new InMemoryEvaluationRepository();
        Rubric rubric = TestData.CourseRubric();
        StudentId alice = StudentId.New();
        repository.Save(TestData.Grade(rubric, alice, 0, 5));
        repository.Save(TestData.Grade(rubric, alice, 1, 5));
        repository.Save(TestData.Grade(rubric, StudentId.New(), 0, 5));

        int removed = repository.RemoveByCriterion(rubric.Criteria[0].Id);

        Assert.Equal(2, removed);
        Assert.Single(repository.GetAll());
        Assert.Single(repository.GetByStudent(alice));
    }

    [Fact]
    public void RubricRepository_StartsWithEmptyRubric()
    {
        Assert.Empty(new InMemoryRubricRepository().GetCurrent().Criteria);
    }
}
