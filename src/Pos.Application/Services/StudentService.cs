using Pos.Application.Dtos;
using Pos.Application.Errors;
using Pos.Application.Mapping;
using Pos.Domain.Evaluations;
using Pos.Domain.Students;

namespace Pos.Application.Services;

public sealed class StudentService(IStudentRepository students, IEvaluationRepository evaluations)
{
    public StudentDto Add(string fullName)
    {
        Student student = Student.Register(fullName);
        EnsureNameIsFree(student.Name, student.Id);

        students.Save(student);
        return student.ToDto();
    }

    public IReadOnlyList<StudentDto> GetAll() =>
        students.GetAll()
            .OrderBy(s => s.Name.Value, StringComparer.OrdinalIgnoreCase)
            .Select(s => s.ToDto())
            .ToList();

    public StudentDto Rename(Guid studentId, string newFullName)
    {
        Student student = Find(studentId);
        EnsureNameIsFree(PersonName.Of(newFullName), student.Id);

        student.Rename(newFullName);
        students.Save(student);
        return student.ToDto();
    }

    /// <returns>Number of evaluations removed together with the student.</returns>
    public int Remove(Guid studentId)
    {
        Student student = Find(studentId);
        int removed = evaluations.RemoveByStudent(student.Id);
        students.Remove(student.Id);
        return removed;
    }

    private Student Find(Guid studentId) =>
        students.FindById(new StudentId(studentId)) ?? throw new NotFoundException("Student", studentId);

    // Unique names are a rule over all students, so it cannot live inside one Student aggregate.
    private void EnsureNameIsFree(PersonName name, StudentId owner)
    {
        Student? other = students.FindByName(name);
        if (other is not null && other.Id != owner)
        {
            throw new ConflictException($"Student '{name}' already exists.");
        }
    }
}
