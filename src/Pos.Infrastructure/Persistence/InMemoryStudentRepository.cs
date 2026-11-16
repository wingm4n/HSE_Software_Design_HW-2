using Pos.Domain.Students;

namespace Pos.Infrastructure.Persistence;

public sealed class InMemoryStudentRepository : IStudentRepository
{
    private readonly Dictionary<StudentId, Student> _items = [];

    public Student? FindById(StudentId id) => _items.GetValueOrDefault(id);

    public Student? FindByName(PersonName name) =>
        _items.Values.FirstOrDefault(s => string.Equals(s.Name.Value, name.Value, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<Student> GetAll() => _items.Values.ToList();

    public void Save(Student student) => _items[student.Id] = student;

    public void Remove(StudentId id) => _items.Remove(id);
}
