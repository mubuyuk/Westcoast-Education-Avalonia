# Westcoast Education – Administration (Avalonia)

En desktop-applikation byggd med Avalonia UI där Westcoast Educations utbildningsledare och administratörer kan administrera kurser, studenter och lärare. Applikationen bygger vidare på konsolprogrammet från inlämningsuppgift 1 och sparar all data i RestDB.io.

## Funktioner

| Område | Funktioner |
|---|---|
| Kurser | Lista, lägga till, uppdatera och radera kurser |
| Studenter | Lista och lägga till studenter |
| Lärare | Lista, lägga till och uppdatera lärare, inklusive lärarens kompetensområde |

## Teknik

- **.NET 10** och **Avalonia 12**
- **MVVM** med CommunityToolkit.Mvvm (`[ObservableProperty]` och `[RelayCommand]`)
- **Dependency Injection** med Microsoft.Extensions.DependencyInjection
- **RestDB.io** som databas, via `HttpClient`

## Kom igång

1. Kopiera `WestcoastEducation.Desktop/appsettings.example.json` till `WestcoastEducation.Desktop/appsettings.json`.
2. Ersätt `DIN-API-NYCKEL-HAR` med din RestDB API-nyckel.
3. Starta applikationen:

```
dotnet run --project WestcoastEducation.Desktop
```

`appsettings.json` finns med i `.gitignore` och versionshanteras aldrig, så API-nyckeln hamnar inte på GitHub. Om filen saknas visar applikationen ett meddelande om det i stället för att starta.

RestDB-databasen behöver tre collections: `courses`, `students` och `teachers`. Fälten behöver inte definieras i förväg, RestDB sparar den JSON som skickas.

## Projektstruktur

| Mapp / fil | Ansvar |
|---|---|
| `Models` | Klasserna `Person`, `Student`, `Teacher`, `Course` och enumen `CourseType`. Återanvända från konsolprogrammet, men förenklade (inga konstruktorer med parametrar) och med ett `Id` som motsvarar RestDB:s `_id`. |
| `Services` | All HTTP-kommunikation med RestDB. Varje område har ett interface (`ICourseService`, `IStudentService`, `ITeacherService`) och en implementation (`CourseService`, `StudentService`, `TeacherService`). `RestDbSettings` läser `appsettings.json`. |
| `ViewModels` | En ViewModel per flik (`CoursesViewModel`, `StudentsViewModel`, `TeachersViewModel`) samt `MainViewModel` som håller ihop dem. Här finns formulärdata, kommandon och enkel validering. |
| `Views` | XAML-filer (`.axaml`). `MainWindow` har en flik per område och varje flik är en egen `UserControl`. Views innehåller bara bindningar, ingen logik i code-behind. |
| `Styles/AppResources.axaml` | Applikationens färger. De används via `{DynamicResource ...}` i views och styles. |
| `App.axaml` | Gemensamma styles samt inkludering av färgresurserna. |
| `App.axaml.cs` | Dependency Injection: här registreras `HttpClient`, services och ViewModels. |

## Hur delarna hänger ihop

```
View (XAML)  --binding-->  ViewModel  --interface-->  Service  --HttpClient-->  RestDB.io
```

1. Användaren fyller i formuläret i en **View**. Fälten är bundna till egenskaper i **ViewModel**.
2. Knappen är bunden till ett **command** i ViewModel, till exempel `AddCourseCommand`.
3. ViewModel kontrollerar formuläret och anropar sin service via **interfacet**, till exempel `ICourseService`.
4. **Servicen** skickar ett HTTP-anrop (GET, POST, PUT eller DELETE) till RestDB.
5. ViewModel uppdaterar sin `ObservableCollection`, och listan i View uppdateras automatiskt.

ViewModels känner bara till interfacen. Vilken implementation de får bestäms på ett ställe, i `App.axaml.cs`, med Dependency Injection.

## Dynamiska resurser

Alla färger definieras en gång i `Styles/AppResources.axaml`, till exempel:

```xml
<SolidColorBrush x:Key="PrimaryBrush" Color="#1F4E79" />
```

och används med `DynamicResource`:

```xml
<Setter Property="Background" Value="{DynamicResource PrimaryBrush}" />
```

Om en färg ändras i resursfilen ändras den i hela applikationen.
