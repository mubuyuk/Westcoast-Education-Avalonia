# Arkitektur – MVVM, Interfaces och Dependency Injection

Det här dokumentet förklarar hur **Westcoast Education** är uppbyggt. Vi använder funktionen **Lägg till kurs** som ett genomgående exempel. Kodexemplen är hämtade ur projektets visade kod eller förkortade för att förklara principerna.

[Tillbaka till README](../README.md)

## 1. MVVM – olika delar har olika ansvar

**MVVM** står för *Model–View–ViewModel*.

| Del | Vad gör den? | Exempel |
|---|---|---|
| **Model** | Representerar information | `Course` |
| **View** | Visar gränssnittet | `CoursesView.axaml` |
| **ViewModel** | Hanterar formulär, validering och commands | `CoursesViewModel.cs` |
| **Service** | Sköter kommunikation med databasen | `CourseService.cs` |

En **Model** innehåller exempelvis en kurs titel, kursnummer, längd och datum. En **View** visar textfält och knappar. En **ViewModel** tar hand om användarens handlingar. En **Service** skickar data till RestDB.

### Data Binding – kopplingen mellan View och ViewModel

I `CoursesView.axaml` finns bland annat:

```xml
<TextBox Text="{Binding Title}" />
<Button Content="Lägg till" Command="{Binding AddCourseCommand}" />
```

`{Binding Title}` kopplar textfältet till `Title` i ViewModel. Knappen använder `AddCourseCommand` för att anropa logik i ViewModel utan att lägga den i XAML-filen.

I `CoursesViewModel.cs` finns:

```csharp
[ObservableProperty]
public partial string Title { get; set; } = string.Empty;

[RelayCommand]
private async Task AddCourseAsync()
{
    // Validera och spara kursen.
}
```

`[ObservableProperty]` hjälper UI att reagera på ändrade värden. `[RelayCommand]` genererar ett command som kan bindas till knappen. Båda kommer från `CommunityToolkit.Mvvm`.

## 2. Service – vem sparar informationen?

`CoursesViewModel` ansvarar för att kontrollera formuläret. Den skickar sedan kursen vidare:

```csharp
Course savedCourse = await _courseService.AddCourseAsync(course);
Courses.Add(savedCourse);
```

`CourseService` innehåller HTTP-kommunikationen:

```csharp
public async Task<Course> AddCourseAsync(Course course)
{
    HttpResponseMessage response =
        await _httpClient.PostAsJsonAsync("courses", course);

    response.EnsureSuccessStatusCode();

    Course? savedCourse =
        await response.Content.ReadFromJsonAsync<Course>();

    return savedCourse
        ?? throw new InvalidOperationException("RestDB returnerade ingen kurs.");
}
```

Detta händer:

1. `PostAsJsonAsync` skickar kursen som JSON i ett HTTP **POST**-anrop.
2. `EnsureSuccessStatusCode` kastar ett undantag om HTTP-svaret inte visar framgång.
3. `ReadFromJsonAsync<Course>` läser svaret som ett `Course`-objekt.
4. Servicen returnerar den sparade kursen, inklusive ID från RestDB.

ViewModel behöver alltså inte känna till HTTP-anropens detaljer.

## 3. Interface – ett gemensamt kontrakt

Ett **interface** beskriver **VAD** en klass måste erbjuda, inte **HUR** det görs.

Ett exempel med samma metoder som vår kursservice:

```csharp
public interface ICourseService
{
    Task<List<Course>> GetCoursesAsync();
    Task<Course> AddCourseAsync(Course course);
    Task UpdateCourseAsync(Course course);
    Task DeleteCourseAsync(string id);
}
```

Klassen deklarerar att den följer kontraktet:

```csharp
public class CourseService : ICourseService
{
    // Implementerar metoderna med RestDB och HttpClient.
}
```

Om en vanlig, konkret klass inte implementerar alla obligatoriska interfacemedlemmar får vi ett **kompileringsfel**. Interfacet garanterar att rätt metoder finns, men inte att de gör rätt sak – det kontrollerar vi med tester.

**Minnesregel:** Interface = **VAD**. Implementation = **HUR**.

## 4. Dependency Injection – hur får ViewModel sin Service?

I `CoursesViewModel` ser konstruktorn ut så här:

```csharp
private readonly ICourseService _courseService;

public CoursesViewModel(ICourseService courseService)
{
    _courseService = courseService;
}
```

ViewModel ber om ett **`ICourseService`**, inte en specifik `CourseService`. Den skapar alltså inte själv en service med `new`.

I `App.axaml.cs` registrerar vi kopplingen:

```csharp
var services = new ServiceCollection();

services.AddSingleton<ICourseService, CourseService>();
services.AddSingleton<IStudentService, StudentService>();
services.AddSingleton<ITeacherService, TeacherService>();

services.AddSingleton<CoursesViewModel>();
services.AddSingleton<StudentsViewModel>();
services.AddSingleton<TeachersViewModel>();
services.AddSingleton<MainViewModel>();

return services.BuildServiceProvider();
```

Den första registreringen betyder: **När någon efterfrågar `ICourseService`, tillhandahåll `CourseService`.**

`BuildServiceProvider()` bygger en **DI-container** som vet hur de registrerade objekten ska tillhandahållas. När containern ska skapa `CoursesViewModel` ser den att konstruktorn kräver `ICourseService`, hittar registreringen och skickar in en `CourseService`.

Det kallas **Constructor Injection** – beroendet kommer in via konstruktorn.

### Vad betyder Singleton?

`AddSingleton` betyder att samma instans återanvänds inom DI-containern. Ett annat alternativ är `AddTransient`, där en ny instans normalt skapas varje gång tjänsten begärs.

## 5. DI kan lösa flera beroenden i följd

Även `CourseService` behöver något: en `HttpClient`.

```csharp
private readonly HttpClient _httpClient;

public CourseService(HttpClient httpClient)
{
    _httpClient = httpClient;
}
```

I `App.axaml.cs` registreras en gemensam klient:

```csharp
services.AddSingleton(_ =>
{
    var httpClient = new HttpClient
    {
        BaseAddress = new Uri(settings.BaseUrl)
    };

    httpClient.DefaultRequestHeaders.Add("x-apikey", settings.ApiKey);
    return httpClient;
});
```

När containern skapar `CourseService` kan den därför även ge den en `HttpClient`.

```text
CoursesViewModel
    │ behöver ICourseService
    ▼
CourseService
    │ behöver HttpClient
    ▼
HttpClient
    │ skickar HTTP-anrop
    ▼
RestDB.io
```

Det här är ett exempel på **en kedja av beroenden**. Klasserna behöver inte själva skapa allt de använder.

## 6. Loose Coupling – varför använda interface?

**Loose Coupling** betyder *lös koppling*: en klass är inte onödigt beroende av detaljerna i en annan klass.

Jämför konstruktorerna:

```csharp
// Beroende av en konkret implementation:
public CoursesViewModel(CourseService courseService)

// Beroende av ett kontrakt:
public CoursesViewModel(ICourseService courseService)
```

I det andra fallet behöver ViewModel bara veta vilka metoder `ICourseService` erbjuder. Det gör implementationen lättare att byta ut.

### Tänk om vi byter från RestDB till MongoDB?

Vi skulle kunna skapa en ny klass `MongoDbCourseService` som implementerar **samma** `ICourseService` men använder MongoDB:s .NET-driver.

Sedan ändrar vi registreringen:

```csharp
// Nuvarande:
services.AddSingleton<ICourseService, CourseService>();

// Hypotetiskt byte till MongoDB:
services.AddSingleton<ICourseService, MongoDbCourseService>();
```

`CoursesViewModel` behöver då inte ändra sin konstruktor – den efterfrågar fortfarande `ICourseService`.

**Viktigt:** MongoDB-klassen finns inte i det visade projektet. För ett verkligt byte behöver vi implementera alla metoder, konfigurera anslutningen, eventuellt migrera data och byta motsvarande services för studenter och lärare. DI gör inte databasmigreringen åt oss.

```text
FÖRE                       EFTER
CoursesViewModel           CoursesViewModel
       │                          │
       ▼                          ▼
ICourseService             ICourseService
       │                          │
       ▼                          ▼
CourseService              MongoDbCourseService
       │                          │
       ▼                          ▼
    RestDB                      MongoDB
```

Det är ett praktiskt exempel på **Loose Coupling**.

## 7. Hela flödet – Lägg till kurs

```text
1. Användaren klickar på "Lägg till"
                  │
                  ▼
2. CoursesView.axaml
   Command="{Binding AddCourseCommand}"
                  │
                  ▼
3. CoursesViewModel
   Kontrollerar formuläret och skapar Course
                  │
                  ▼
4. Anropar _courseService.AddCourseAsync(course)
   via kontraktet ICourseService
                  │
                  ▼
5. CourseService
   Skickar HTTP POST med HttpClient
                  │
                  ▼
6. RestDB sparar kursen och skickar svar
                  │
                  ▼
7. CoursesViewModel
   Courses.Add(savedCourse)
                  │
                  ▼
8. CoursesView.axaml
   Listan visar den nya kursen
```

`ICourseService` är ett kontrakt, inte ett separat nätverkssteg. DI har redan kopplat ViewModel till rätt implementation.

Varför uppdateras listan? I ViewModel finns:

```csharp
public ObservableCollection<Course> Courses { get; } = new();
```

Och i XAML:

```xml
<ListBox ItemsSource="{Binding Courses}"
         SelectedItem="{Binding SelectedCourse}" />
```

`ObservableCollection` meddelar gränssnittet när objekt läggs till eller tas bort. Därför visas den nya kursen utan att vi manuellt behöver bygga om listan.

## 8. Var börjar applikationen?

`Program.cs` startar Avalonia. Därefter konfigurerar `App.axaml.cs` applikationen. Här är den centrala uppstartskoden:

```csharp
ServiceProvider services = ConfigureServices(RestDbSettings.Load());

var mainViewModel = services.GetRequiredService<MainViewModel>();
desktop.MainWindow = new MainWindow
{
    DataContext = mainViewModel,
};

_ = mainViewModel.LoadAllAsync();
```

`GetRequiredService<MainViewModel>()` hämtar ViewModel via DI-containern. `DataContext` gör att XAML kan använda ViewModels properties och commands via Data Binding. `LoadAllAsync()` påbörjar inläsning av data.

## 9. Begrepp att komma ihåg

| Begrepp | Enkel förklaring |
|---|---|
| **Model** | Beskriver data, till exempel en kurs |
| **View** | Det användaren ser |
| **ViewModel** | Logik bakom gränssnittet |
| **Data Binding** | Kopplar UI till properties och commands |
| **Service** | Utför exempelvis databaskommunikation |
| **Interface** | Kontrakt för vilka metoder som måste finnas |
| **Implementation** | Koden som utför arbetet |
| **Dependency** | Ett objekt som en klass behöver |
| **Dependency Injection** | Ger klasser deras beroenden utifrån |
| **DI-container** | Tillhandahåller registrerade objekt och deras beroenden |
| **Constructor Injection** | Beroendet skickas in via konstruktorn |
| **Loose Coupling** | Minskar beroendet av specifika implementationer |
| **Singleton** | Samma instans återanvänds inom containern |

**Kort sagt:** Interface bestämmer **VAD**, implementationen bestämmer **HUR**, DI tillhandahåller **VILKEN implementation** och Loose Coupling gör den lättare att byta.

[Tillbaka till README](../README.md)
