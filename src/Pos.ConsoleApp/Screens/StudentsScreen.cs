using Pos.Application.Dtos;
using Pos.Application.Facade;
using Pos.ConsoleApp.Io;
using Pos.ConsoleApp.Menu;

namespace Pos.ConsoleApp.Screens;

public sealed class StudentsScreen(IPosFacade facade, ConsoleIO io) : IMenuScreen
{
    public string Title => "Students";

    public IReadOnlyList<MenuItem> Items =>
    [
        new("Add a student", Add),
        new("List students", List),
        new("Rename a student", Rename),
        new("Remove a student", Remove),
    ];

    private void Add()
    {
        StudentDto student = facade.AddStudent(io.Prompt("Full name"));
        io.WriteLine($"Added: {student.Name}");
    }

    private void List()
    {
        IReadOnlyList<StudentDto> students = facade.GetStudents();
        io.WriteLine(students.Count == 0
            ? "No students yet."
            : TextTable.Render(["#", "Student"], students.Select((s, i) => new[] { $"{i + 1}", s.Name }).ToList()));
    }

    private void Rename()
    {
        if (io.TryChoose("Whom to rename?", facade.GetStudents(), s => s.Name, out StudentDto? student))
        {
            StudentDto renamed = facade.RenameStudent(student.Id, io.Prompt("New full name"));
            io.WriteLine($"Renamed to: {renamed.Name}");
        }
    }

    private void Remove()
    {
        if (io.TryChoose("Whom to remove?", facade.GetStudents(), s => s.Name, out StudentDto? student)
            && io.Confirm($"Remove {student.Name} together with all their evaluations?"))
        {
            int removed = facade.RemoveStudent(student.Id);
            io.WriteLine($"Removed. Evaluations deleted: {removed}");
        }
    }
}
