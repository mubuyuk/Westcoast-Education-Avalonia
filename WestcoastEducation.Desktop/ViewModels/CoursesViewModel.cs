using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WestcoastEducation.Desktop.Models;
using WestcoastEducation.Desktop.Services;

namespace WestcoastEducation.Desktop.ViewModels;

public partial class CoursesViewModel : ViewModelBase
{
    private readonly ICourseService _courseService;

    public CoursesViewModel(ICourseService courseService)
    {
        _courseService = courseService;
    }

    public ObservableCollection<Course> Courses { get; } = new();

    public CourseType[] CourseTypes { get; } = Enum.GetValues<CourseType>();

    // När en kurs väljs i listan fylls formuläret och knapparna Uppdatera/Radera aktiveras.
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UpdateCourseCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCourseCommand))]
    public partial Course? SelectedCourse { get; set; }

    // Formulärets fält. Siffror är text i formuläret och kontrolleras när kursen sparas.
    [ObservableProperty]
    public partial string CourseNumber { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Duration { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime? StartDate { get; set; }

    [ObservableProperty]
    public partial DateTime? EndDate { get; set; }

    [ObservableProperty]
    public partial CourseType SelectedCourseType { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    public async Task LoadCoursesAsync()
    {
        try
        {
            Courses.Clear();
            foreach (Course course in await _courseService.GetCoursesAsync())
            {
                Courses.Add(course);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunde inte hämta kurser: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddCourseAsync()
    {
        Course? course = CreateCourseFromForm();
        if (course is null)
        {
            return;
        }

        try
        {
            Course savedCourse = await _courseService.AddCourseAsync(course);
            Courses.Add(savedCourse);
            ClearForm();
            StatusMessage = $"Kursen \"{savedCourse.Title}\" har lagts till.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunde inte lägga till kursen: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(IsCourseSelected))]
    private async Task UpdateCourseAsync()
    {
        Course? course = CreateCourseFromForm();
        if (course is null || SelectedCourse is null)
        {
            return;
        }

        course.Id = SelectedCourse.Id;

        try
        {
            await _courseService.UpdateCourseAsync(course);

            // Byt ut kursen i listan så att den nya informationen visas.
            int index = Courses.IndexOf(SelectedCourse);
            Courses[index] = course;
            SelectedCourse = course;
            StatusMessage = $"Kursen \"{course.Title}\" har uppdaterats.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunde inte uppdatera kursen: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(IsCourseSelected))]
    private async Task DeleteCourseAsync()
    {
        if (SelectedCourse?.Id is null)
        {
            return;
        }

        Course course = SelectedCourse;

        try
        {
            await _courseService.DeleteCourseAsync(course.Id!);
            Courses.Remove(course);
            ClearForm();
            StatusMessage = $"Kursen \"{course.Title}\" har raderats.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunde inte radera kursen: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedCourse = null;
        CourseNumber = string.Empty;
        Title = string.Empty;
        Duration = string.Empty;
        StartDate = null;
        EndDate = null;
        SelectedCourseType = CourseType.Classroom;
    }

    private bool IsCourseSelected() => SelectedCourse is not null;

    partial void OnSelectedCourseChanged(Course? value)
    {
        if (value is null)
        {
            return;
        }

        CourseNumber = value.CourseNumber.ToString();
        Title = value.Title;
        Duration = value.Duration.ToString();
        StartDate = value.StartDate;
        EndDate = value.EndDate;
        SelectedCourseType = value.TypeOfCourse;
    }

    // Kontrollerar formuläret och skapar en kurs. Returnerar null och visar ett meddelande om något är fel.
    private Course? CreateCourseFromForm()
    {
        if (!int.TryParse(CourseNumber, out int courseNumber) || courseNumber <= 0)
        {
            StatusMessage = "Kursnummer måste vara ett positivt heltal.";
            return null;
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            StatusMessage = "Ange en titel.";
            return null;
        }

        if (!int.TryParse(Duration, out int duration) || duration <= 0)
        {
            StatusMessage = "Längd (veckor) måste vara ett positivt heltal.";
            return null;
        }

        if (StartDate is null || EndDate is null)
        {
            StatusMessage = "Ange både startdatum och slutdatum.";
            return null;
        }

        if (EndDate < StartDate)
        {
            StatusMessage = "Slutdatum kan inte vara före startdatum.";
            return null;
        }

        // Samma kontroll som i konsolapplikationen: kursnumret måste vara unikt.
        if (Courses.Any(c => c.CourseNumber == courseNumber && c != SelectedCourse))
        {
            StatusMessage = $"Kurs med kursnummer {courseNumber} finns redan.";
            return null;
        }

        return new Course
        {
            CourseNumber = courseNumber,
            Title = Title.Trim(),
            Duration = duration,
            StartDate = StartDate.Value.Date,
            EndDate = EndDate.Value.Date,
            TypeOfCourse = SelectedCourseType
        };
    }
}
