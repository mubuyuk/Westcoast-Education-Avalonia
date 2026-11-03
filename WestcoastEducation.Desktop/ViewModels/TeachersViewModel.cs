using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WestcoastEducation.Desktop.Models;
using WestcoastEducation.Desktop.Services;

namespace WestcoastEducation.Desktop.ViewModels;

// Uppdatera används både för lärarens uppgifter och för att hantera kompetensområdet.
public partial class TeachersViewModel : ViewModelBase
{
    private readonly ITeacherService _teacherService;

    public TeachersViewModel(ITeacherService teacherService)
    {
        _teacherService = teacherService;
    }

    public ObservableCollection<Teacher> Teachers { get; } = new();

    // När en lärare väljs i listan fylls formuläret och knappen Uppdatera aktiveras.
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UpdateTeacherCommand))]
    public partial Teacher? SelectedTeacher { get; set; }

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
    public partial string KnowledgeArea { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime? EmploymentDate { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    public async Task LoadTeachersAsync()
    {
        try
        {
            Teachers.Clear();
            foreach (Teacher teacher in await _teacherService.GetTeachersAsync())
            {
                Teachers.Add(teacher);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunde inte hämta lärare: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddTeacherAsync()
    {
        // null: en ny lärare ska jämföras mot alla befintliga lärare, även den som är markerad.
        Teacher? teacher = CreateTeacherFromForm(null);
        if (teacher is null)
        {
            return;
        }

        try
        {
            Teacher savedTeacher = await _teacherService.AddTeacherAsync(teacher);
            Teachers.Add(savedTeacher);
            ClearForm();
            StatusMessage = $"Läraren {savedTeacher.FullName} har lagts till.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunde inte lägga till läraren: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(IsTeacherSelected))]
    private async Task UpdateTeacherAsync()
    {
        // Spara den valda läraren innan anropet, ifall användaren hinner välja en annan lärare under tiden.
        Teacher? originalTeacher = SelectedTeacher;
        if (originalTeacher is null)
        {
            return;
        }

        Teacher? teacher = CreateTeacherFromForm(originalTeacher);
        if (teacher is null)
        {
            return;
        }

        teacher.Id = originalTeacher.Id;

        try
        {
            await _teacherService.UpdateTeacherAsync(teacher);

            // Byt ut läraren i listan så att den nya informationen visas.
            int index = Teachers.IndexOf(originalTeacher);
            if (index >= 0)
            {
                Teachers[index] = teacher;
            }
            // Markera den uppdaterade läraren, om inte användaren redan har valt en annan lärare.
            if (SelectedTeacher is null || SelectedTeacher == originalTeacher)
            {
                SelectedTeacher = teacher;
            }
            StatusMessage = $"Läraren {teacher.FullName} har uppdaterats.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kunde inte uppdatera läraren: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedTeacher = null;
        FirstName = string.Empty;
        LastName = string.Empty;
        PersonalNumber = string.Empty;
        PhoneNumber = string.Empty;
        Address = string.Empty;
        PostalCode = string.Empty;
        City = string.Empty;
        KnowledgeArea = string.Empty;
        EmploymentDate = null;
    }

    private bool IsTeacherSelected() => SelectedTeacher is not null;

    partial void OnSelectedTeacherChanged(Teacher? value)
    {
        if (value is null)
        {
            return;
        }

        FirstName = value.FirstName;
        LastName = value.LastName;
        PersonalNumber = value.PersonalNumber;
        PhoneNumber = value.PhoneNumber;
        Address = value.Address;
        PostalCode = value.PostalCode;
        City = value.City;
        KnowledgeArea = value.KnowledgeArea;
        EmploymentDate = value.EmploymentDate;
    }

    // Kontrollerar formuläret och skapar en lärare. Returnerar null och visar ett meddelande om något är fel.
    // teacherBeingUpdated är läraren som uppdateras (null vid Lägg till) och undantas från dubblettkontrollen.
    private Teacher? CreateTeacherFromForm(Teacher? teacherBeingUpdated)
    {
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(PersonalNumber))
        {
            StatusMessage = "Förnamn, efternamn och personnummer måste fyllas i.";
            return null;
        }

        if (string.IsNullOrWhiteSpace(KnowledgeArea))
        {
            StatusMessage = "Ange lärarens kompetensområde.";
            return null;
        }

        if (EmploymentDate is null)
        {
            StatusMessage = "Ange anställningsdatum.";
            return null;
        }

        if (Teachers.Any(t => t.PersonalNumber == PersonalNumber.Trim() && t != teacherBeingUpdated))
        {
            StatusMessage = $"Lärare med personnummer {PersonalNumber} finns redan.";
            return null;
        }

        return new Teacher
        {
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            PersonalNumber = PersonalNumber.Trim(),
            PhoneNumber = PhoneNumber.Trim(),
            Address = Address.Trim(),
            PostalCode = PostalCode.Trim(),
            City = City.Trim(),
            KnowledgeArea = KnowledgeArea.Trim(),
            EmploymentDate = EmploymentDate.Value.Date
        };
    }
}
