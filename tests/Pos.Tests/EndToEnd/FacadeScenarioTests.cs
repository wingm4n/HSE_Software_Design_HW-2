using Pos.Application.Dtos;
using Pos.Application.Errors;
using Pos.Application.Facade;
using Pos.Domain.Exceptions;
using Pos.Tests.TestSupport;

namespace Pos.Tests.EndToEnd;

// The required scenario on the real object graph: students and criteria, evaluations, final grade, export.
public class FacadeScenarioTests
{
    [Fact]
    public void FullScenario_FinalGradeFollowsEvaluations()
    {
        IPosFacade facade = TestHost.CreateFacade();
        RubricDto rubric = facade.DefineRubric("Software Design", [new("Homework", 10, 3), new("Project", 20, 5), new("Exam", 10, 2)]);
        StudentDto alice = facade.AddStudent("Alice Ivanova");
        StudentDto boris = facade.AddStudent("Boris Petrov");

        facade.GradeStudent(alice.Id, rubric.Criteria[0].Id, 9);
        facade.GradeStudent(alice.Id, rubric.Criteria[1].Id, 18);
        EvaluationDto exam = facade.GradeStudent(alice.Id, rubric.Criteria[2].Id, 4);
        Assert.Equal(80m, facade.GetFinalGrade(alice.Id, "all-criteria").Percent);

        facade.CorrectEvaluation(exam.Id, 8);
        Assert.Equal(88m, facade.GetFinalGrade(alice.Id, "all-criteria").Percent);

        Assert.Throws<ScoreOutOfRangeException>(() => facade.GradeStudent(boris.Id, rubric.Criteria[2].Id, 12));
        Assert.Equal(0m, facade.GetFinalGrade(boris.Id, "all-criteria").Percent);

        var writer = new StringWriter();
        facade.ExportResults("csv", "all-criteria", writer);
        Assert.Contains("Alice Ivanova,88,3,3", writer.ToString());
    }

    [Fact]
    public void Rules_GiveDifferentResults_ForIncompleteGrades()
    {
        IPosFacade facade = TestHost.CreateFacade();
        RubricDto rubric = facade.DefineRubric("Course", [new("Homework", 10, 1), new("Exam", 10, 1)]);
        StudentDto alice = facade.AddStudent("Alice");
        facade.GradeStudent(alice.Id, rubric.Criteria[0].Id, 10);

        Assert.Equal(50m, facade.GetFinalGrade(alice.Id, "all-criteria").Percent);
        Assert.Equal(100m, facade.GetFinalGrade(alice.Id, "graded-only").Percent);
    }

    [Fact]
    public void RemovingCriterion_RemovesItsEvaluations()
    {
        IPosFacade facade = TestHost.CreateFacade();
        CriterionDto homework = facade.AddCriterion("Homework", 10, 1);
        StudentDto alice = facade.AddStudent("Alice");
        facade.GradeStudent(alice.Id, homework.Id, 5);

        Assert.Equal(1, facade.RemoveCriterion(homework.Id));
        Assert.Empty(facade.GetEvaluations());
    }

    [Fact]
    public void UnknownRuleOrFormat_ThrowsNotFound()
    {
        IPosFacade facade = TestHost.CreateFacade();

        Assert.Throws<NotFoundException>(() => facade.GetFinalGrades("best-of-three"));
        Assert.Throws<NotFoundException>(() => facade.ExportResults("xml", "all-criteria", new StringWriter()));
    }
}
