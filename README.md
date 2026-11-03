# Westcoast Education – Administration (Avalonia)

En desktop-applikation i **C#/.NET 10** och **Avalonia UI** för att administrera kurser, studenter och lärare. Projektet bygger vidare på konsolapplikationen från inlämningsuppgift 1 och lagrar data i **RestDB.io**.

## Funktioner

| Område | Funktioner |
|---|---|
| **Kurser** | Visa, lägga till, uppdatera och radera kurser |
| **Studenter** | Visa och lägga till studenter |
| **Lärare** | Visa, lägga till och uppdatera lärare och deras kompetensområden |

Kursformuläret kontrollerar bland annat obligatoriska fält, giltiga datum och unika kursnummer.

## Teknik

- **.NET 10** och **Avalonia 12** – desktop-applikation
- **MVVM** med `CommunityToolkit.Mvvm` – View, ViewModel och data binding
- **Interfaces och Dependency Injection** – löst kopplade komponenter
- **HttpClient** – kommunikation med RestDB:s HTTP-API
- **RestDB.io** – datalagring

## Kom igång

1. Kopiera `WestcoastEducation.Desktop/appsettings.example.json` till `WestcoastEducation.Desktop/appsettings.json`.
2. Ange din RestDB API-nyckel i `appsettings.json`.
3. Se till att databasen har collections `courses`, `students` och `teachers`.
4. Starta från projektets rotmapp:

   ```bash
   dotnet run --project WestcoastEducation.Desktop
   ```

`appsettings.json` är undantagen från versionshantering via `.gitignore`. Lägg aldrig in riktiga API-nycklar i Git.

## Projektstruktur

| Mapp / fil | Ansvar |
|---|---|
| `Models/` | Datamodeller för kurser, studenter och lärare |
| `Views/` | Användargränssnitt i Avalonia XAML (`.axaml`) |
| `ViewModels/` | Formulärdata, validering och commands |
| `Services/` | Interfaces, HTTP-kommunikation och RestDB-konfiguration |
| `Styles/AppResources.axaml` | Gemensamma färgresurser |
| `App.axaml.cs` | Registrering av services och ViewModels med DI |
| `Program.cs` | Startar Avalonia |

## Så fungerar applikationen

När användaren klickar på **Lägg till** i kursvyn går informationen genom följande delar:

```text
CoursesView.axaml
       │  Data Binding / AddCourseCommand
       ▼
CoursesViewModel
       │  ICourseService
       ▼
CourseService
       │  HttpClient / HTTP POST
       ▼
RestDB.io
```

ViewModel arbetar mot interfacet `ICourseService` i stället för att vara direkt beroende av `CourseService`. Med **Dependency Injection** bestämmer vi vilken implementation som ska användas.

**[Läs den pedagogiska genomgången av MVVM, Interfaces, Dependency Injection och Loose Coupling →](docs/architecture.md)**

## Utseende och resurser

Färger definieras centralt i `Styles/AppResources.axaml` och används med `DynamicResource`. Det gör att samma färg kan ändras på ett ställe.
