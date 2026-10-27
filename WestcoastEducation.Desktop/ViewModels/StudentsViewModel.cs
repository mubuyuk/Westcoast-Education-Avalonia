using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WestcoastEducation.Desktop.Models;
using WestcoastEducation.Desktop.Services;

namespace WestcoastEducation.Desktop.ViewModels;

public partial class StudentsViewModel : ViewModelBase
{
    private readonly IStudentService _studentService;

    public StudentsViewModel(IStudentService studentService)
    {
        _studentService = studentService;
    }

    public ObservableCollection<Student> Students { get; } = new();

    // Formulärets fält.
    [ObservableProperty]
    public partial string FirstName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LastName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PersonalNumber { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PhoneNumber { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Address { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PostalCode { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string City { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    public async Task LoadStudentsAsync()
    {
        try
        {
            Students.Clear();
            foreach (Student student in await _studentService.GetStudentsAsync())
            {
                Students.Add(student);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunde inte hämta studenter: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddStudentAsync()
    {
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(PersonalNumber))
        {
            StatusMessage = "Förnamn, efternamn och personnummer måste fyllas i.";
            return;
        }

        // Samma kontroll som i konsolapplikationen: personnumret måste vara unikt.
        if (Students.Any(s => s.PersonalNumber == PersonalNumber.Trim()))
        {
            StatusMessage = $"Student med personnummer {PersonalNumber} finns redan.";
            return;
        }

        var student = new Student
        {
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            PersonalNumber = PersonalNumber.Trim(),
            PhoneNumber = PhoneNumber.Trim(),
            Address = Address.Trim(),
            PostalCode = PostalCode.Trim(),
            City = City.Trim()
        };

        try
        {
            Student savedStudent = await _studentService.AddStudentAsync(student);
            Students.Add(savedStudent);
            ClearForm();
            StatusMessage = $"Studenten {savedStudent.FullName} har lagts till.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunde inte lägga till studenten: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        PersonalNumber = string.Empty;
        PhoneNumber = string.Empty;
        Address = string.Empty;
        PostalCode = string.Empty;
        City = string.Empty;
    }
}
