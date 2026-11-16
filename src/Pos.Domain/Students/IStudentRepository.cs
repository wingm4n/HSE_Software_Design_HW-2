namespace Pos.Domain.Students;

public interface IStudentRepository
{
    Student? FindById(StudentId id);

    Student? FindByName(PersonName name);

    IReadOnlyList<Student> GetAll();

    void Save(Student student);

    void Remove(StudentId id);
}
