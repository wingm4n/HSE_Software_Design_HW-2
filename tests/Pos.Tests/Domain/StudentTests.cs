using Pos.Domain.Exceptions;
using Pos.Domain.Students;

namespace Pos.Tests.Domain;

public class StudentTests
{
    [Fact]
    public void Register_TrimsNameAndAssignsIdentity()
    {
        Student student = Student.Register("  Alice Ivanova ");

        Assert.Equal("Alice Ivanova", student.Name.Value);
        Assert.NotEqual(default, student.Id);
    }

    [Fact]
    public void Rename_KeepsIdentity()
    {
        Student student = Student.Register("Alice");
        StudentId id = student.Id;

        student.Rename("Alice Smith");

        Assert.Equal("Alice Smith", student.Name.Value);
        Assert.Equal(id, student.Id);
    }

    [Fact]
    public void Rename_BlankName_Throws_AndKeepsOldName()
    {
        Student student = Student.Register("Alice");

        Assert.Throws<InvalidValueException>(() => student.Rename(" "));
        Assert.Equal("Alice", student.Name.Value);
    }

    [Fact]
    public void Students_AreEqualByIdentity_NotByName()
    {
        Student first = Student.Register("Alice");
        Student second = Student.Register("Alice");

        Assert.NotEqual(first, second);
        Assert.Equal(first, first);
    }
}
