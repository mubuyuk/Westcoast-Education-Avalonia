using System.Threading.Tasks;

namespace WestcoastEducation.Desktop.ViewModels;

// Huvudfönstrets ViewModel. Den håller en ViewModel per flik, som skickas in via Dependency Injection.
public class MainViewModel : ViewModelBase
{
    public MainViewModel(CoursesViewModel courses, StudentsViewModel students, TeachersViewModel teachers)
    {
        Courses = courses;
        Students = students;
        Teachers = teachers;
    }

    public CoursesViewModel Courses { get; }
    public StudentsViewModel Students { get; }
    public TeachersViewModel Teachers { get; }

    // Hämtar all data från RestDB när applikationen startar.
    // Task.WhenAll kör hämtningarna samtidigt i stället för en i taget.
    public async Task LoadAllAsync()
    {
        await Task.WhenAll(
            Courses.LoadCoursesAsync(),
            Students.LoadStudentsAsync(),
            Teachers.LoadTeachersAsync());
    }
}
