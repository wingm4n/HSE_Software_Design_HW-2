using Moq;
using Pos.Application.Errors;
using Pos.Application.Services;
using Pos.Domain.Evaluations;
using Pos.Domain.Exceptions;
using Pos.Domain.Students;

namespace Pos.Tests.Application;

public class StudentServiceTests
{
    private readonly Mock<IStudentRepository> _students = new();
    private readonly Mock<IEvaluationRepository> _evaluations = new();

    private StudentService CreateService() => new(_students.Object, _evaluations.Object);

    [Fact]
    public void Add_ValidName_SavesStudent()
    {
        var dto = CreateService().Add("  Alice Ivanova ");

        Assert.Equal("Alice Ivanova", dto.Name);
        _students.Verify(r => r.Save(It.Is<Student>(s => s.Name.Value == "Alice Ivanova")), Times.Once);
    }

    [Fact]
    public void Add_ExistingName_ThrowsConflict_AndSavesNothing()
    {
        _students.Setup(r => r.FindByName(It.IsAny<PersonName>())).Returns(Student.Register("Alice Ivanova"));

        Assert.Throws<ConflictException>(() => CreateService().Add("Alice Ivanova"));
        _students.Verify(r => r.Save(It.IsAny<Student>()), Times.Never);
    }

    [Fact]
    public void Add_BlankName_ThrowsDomainException()
    {
        Assert.Throws<InvalidValueException>(() => CreateService().Add("   "));
        _students.Verify(r => r.Save(It.IsAny<Student>()), Times.Never);
    }

    [Fact]
    public void Remove_AlsoRemovesTheirEvaluations()
    {
        Student student = Student.Register("Alice");
        _students.Setup(r => r.FindById(student.Id)).Returns(student);
        _evaluations.Setup(r => r.RemoveByStudent(student.Id)).Returns(3);

        int removed = CreateService().Remove(student.Id.Value);

        Assert.Equal(3, removed);
        _students.Verify(r => r.Remove(student.Id), Times.Once);
    }

    [Fact]
    public void Remove_UnknownStudent_ThrowsNotFound()
    {
        Assert.Throws<NotFoundException>(() => CreateService().Remove(Guid.NewGuid()));
        _evaluations.Verify(r => r.RemoveByStudent(It.IsAny<StudentId>()), Times.Never);
    }
}
