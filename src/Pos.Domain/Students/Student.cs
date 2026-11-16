using Pos.Domain.Common;

namespace Pos.Domain.Students;

/// <summary>Aggregate root: the person who is evaluated.</summary>
public sealed class Student : Entity<StudentId>
{
    private Student(StudentId id, PersonName name)
        : base(id)
    {
        Name = name;
    }

    public PersonName Name { get; private set; }

    public static Student Register(string fullName) => new(StudentId.New(), PersonName.Of(fullName));

    public void Rename(string newFullName)
    {
        Name = PersonName.Of(newFullName);
    }
}
