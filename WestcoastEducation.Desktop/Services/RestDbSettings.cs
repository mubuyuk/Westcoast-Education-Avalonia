using System;
using System.IO;
using System.Text.Json;

namespace WestcoastEducation.Desktop.Services;

// Läser RestDB-inställningarna från appsettings.json.
// Filen innehåller API-nyckeln och versionshanteras därför inte (se .gitignore).
public class RestDbSettings
{
    // Namnen måste matcha nycklarna i appsettings.json för att deserialiseringen ska fungera.
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;

    public static RestDbSettings Load()
    {
        // appsettings.json kopieras till build-mappen vid build (CopyToOutputDirectory i .csproj).
        string path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        if (!File.Exists(path))
        {
            throw new InvalidOperationException(
                "Filen appsettings.json saknas. Kopiera appsettings.example.json till appsettings.json och fyll i din RestDB API-nyckel.");
        }

        RestDbSettings? settings;
        try
        {
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
            if (!document.RootElement.TryGetProperty("RestDb", out JsonElement restDbSection))
            {
                throw new InvalidOperationException("Avsnittet \"RestDb\" saknas i appsettings.json.");
            }

            // Deserialisering: JSON-avsnittet "RestDb" görs om till ett RestDbSettings-objekt.
            settings = restDbSection.Deserialize<RestDbSettings>();
        }
        catch (JsonException)
        {
            // Byter det tekniska JSON-felet mot ett begripligt meddelande till felfönstret.
            throw new InvalidOperationException("Filen appsettings.json innehåller ogiltig JSON. Jämför med appsettings.example.json.");
        }

        // Kontrolleras vid start, annars märks felet först senare som ett otydligt HTTP-fel.
        if (settings is null || string.IsNullOrWhiteSpace(settings.BaseUrl) || string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new InvalidOperationException("BaseUrl eller ApiKey saknas under \"RestDb\" i appsettings.json.");
        }

        // HttpClient kräver ett avslutande "/" för att "courses" ska läggas efter ".../rest/".
        settings.BaseUrl = settings.BaseUrl.Trim();
        if (!settings.BaseUrl.EndsWith('/'))
        {
            settings.BaseUrl += "/";
        }

        if (!Uri.TryCreate(settings.BaseUrl, UriKind.Absolute, out Uri? uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException($"BaseUrl i appsettings.json är inte en giltig webbadress: \"{settings.BaseUrl}\".");
        }

        return settings;
    }
}
