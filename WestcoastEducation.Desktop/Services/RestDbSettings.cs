using System;
using System.IO;
using System.Text.Json;

namespace WestcoastEducation.Desktop.Services;

// Läser RestDB-inställningarna från appsettings.json.
// Filen innehåller API-nyckeln och versionshanteras därför inte (se .gitignore).
public class RestDbSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;

    public static RestDbSettings Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        if (!File.Exists(path))
        {
            throw new InvalidOperationException(
                "Filen appsettings.json saknas. Kopiera appsettings.example.json till appsettings.json och fyll i din RestDB API-nyckel.");
        }

        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
        RestDbSettings? settings = document.RootElement.GetProperty("RestDb").Deserialize<RestDbSettings>();

        if (settings is null || string.IsNullOrWhiteSpace(settings.BaseUrl) || string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new InvalidOperationException("BaseUrl eller ApiKey saknas under \"RestDb\" i appsettings.json.");
        }

        return settings;
    }
}
