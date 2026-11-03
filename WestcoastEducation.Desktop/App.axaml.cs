using System;
using System.Net.Http;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using WestcoastEducation.Desktop.Services;
using WestcoastEducation.Desktop.ViewModels;
using WestcoastEducation.Desktop.Views;

namespace WestcoastEducation.Desktop;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            try
            {
                ServiceProvider services = ConfigureServices(RestDbSettings.Load());

                // DI-containern skapar MainViewModel och allt den behöver (ViewModels → services → HttpClient).
                var mainViewModel = services.GetRequiredService<MainViewModel>();

                // DataContext avgör vilket objekt fönstrets {Binding ...} hämtar data från.
                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainViewModel,
                };

                // Körs utan await så att fönstret visas direkt medan datan hämtas.
                _ = mainViewModel.LoadAllAsync();
            }
            catch (InvalidOperationException ex)
            {
                // Visas om appsettings.json saknas eller är ofullständig.
                desktop.MainWindow = new Window
                {
                    Title = "Westcoast Education - Konfigurationsfel",
                    Width = 600,
                    Height = 160,
                    Content = new TextBlock { Text = ex.Message, Margin = new Thickness(20), TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    // Dependency Injection: här registreras alla services och ViewModels på ett ställe.
    // Containern skapar sedan objekten och skickar in deras beroenden via constructorn.
    private static ServiceProvider ConfigureServices(RestDbSettings settings)
    {
        var services = new ServiceCollection();

        // En gemensam HttpClient med RestDB:s adress och API-nyckel.
        // AddSingleton: en enda instans skapas och delas av alla services.
        services.AddSingleton(_ =>
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(settings.BaseUrl) };
            // Skickas automatiskt med i varje anrop, så ingen service behöver hantera nyckeln.
            httpClient.DefaultRequestHeaders.Add("x-apikey", settings.ApiKey);
            return httpClient;
        });

        // Kopplar interfacet till implementationen: den som ber om ICourseService får en CourseService.
        services.AddSingleton<ICourseService, CourseService>();
        services.AddSingleton<IStudentService, StudentService>();
        services.AddSingleton<ITeacherService, TeacherService>();

        // Singleton gör att varje flik behåller sin data så länge appen körs.
        services.AddSingleton<CoursesViewModel>();
        services.AddSingleton<StudentsViewModel>();
        services.AddSingleton<TeachersViewModel>();
        services.AddSingleton<MainViewModel>();

        return services.BuildServiceProvider();
    }
}
