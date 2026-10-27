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

                var mainViewModel = services.GetRequiredService<MainViewModel>();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainViewModel,
                };

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
    private static ServiceProvider ConfigureServices(RestDbSettings settings)
    {
        var services = new ServiceCollection();

        // En gemensam HttpClient med RestDB:s adress och API-nyckel.
        services.AddSingleton(_ =>
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(settings.BaseUrl) };
            httpClient.DefaultRequestHeaders.Add("x-apikey", settings.ApiKey);
            return httpClient;
        });

        services.AddSingleton<ICourseService, CourseService>();
        services.AddSingleton<IStudentService, StudentService>();
        services.AddSingleton<ITeacherService, TeacherService>();

        services.AddSingleton<CoursesViewModel>();
        services.AddSingleton<StudentsViewModel>();
        services.AddSingleton<TeachersViewModel>();
        services.AddSingleton<MainViewModel>();

        return services.BuildServiceProvider();
    }
}
