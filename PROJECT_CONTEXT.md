# Projektkontext – Westcoast Education steg 2

Sammanställning för att kunna återuppta arbetet i nya sessioner. Senast uppdaterad: 2026-11-03.
Instruktioner och arbetsprinciper finns i `Claude.md`; hur man startar appen och en arkitekturöversikt finns i `README.md`.

## 1. Syfte

Inlämningsuppgift 2 i C#/.NET: en Avalonia desktop-applikation för Westcoast Educations utbildningsledare och administratörer. Den bygger vidare på konsolprogrammet från uppgift 1 (`C:\Users\murat\source\repos\WestcoastEducation\WestcoastEducation`, **strikt read-only**).

Mål: uppfylla **alla G- och VG-krav** med en enkel, nybörjarvänlig lösning utan overengineering.

Krav i korthet:
- **G:** lista/lägga till kurser, studenter och lärare; hantera en lärares kompetensområde; radera kurs; MVVM; RestDB.io som datahanterare; design och färger som dynamiska resurser.
- **VG:** Dependency Injection; all HTTP mot RestDB uppdelad i interface + implementation; uppdatera kurs; uppdatera lärare.

## 2. Arkitektur

```
View (.axaml)  --binding/commands-->  ViewModel  --interface-->  Service  --HttpClient-->  RestDB.io
```

- **Avalonia 12.1.4 + .NET 10**, ett enda projekt.
- **MVVM** med CommunityToolkit.Mvvm (`[ObservableProperty]` som partial properties, `[RelayCommand]`). Views har ingen logik i code-behind.
- **Dependency Injection** (Microsoft.Extensions.DependencyInjection) konfigureras i `App.axaml.cs` → `ConfigureServices`. Allt registreras som singleton: en gemensam `HttpClient` (BaseAddress + header `x-apikey`), tre services och fyra ViewModels.
- **Services** är det enda stället som använder `HttpClient`. ViewModels känner bara till interfacen.
- **RestDB.io:** `https://westcoasteducation-a467.restdb.io/rest/`, collections `courses`, `students`, `teachers`. JSON i camelCase (System.Text.Json web defaults). `_id` mappas till `Id` och skickas inte vid POST (`JsonIgnore WhenWritingNull`). `CourseType` sparas som text.
- Vid start: `App` läser `RestDbSettings`, bygger DI-containern, skapar `MainWindow` och anropar `MainViewModel.LoadAllAsync()`. Saknas `appsettings.json`, eller innehåller den ogiltig JSON, saknar avsnittet `RestDb`, har tom `BaseUrl`/`ApiKey` eller en ogiltig `BaseUrl`, visas ett enkelt felfönster med ett svenskt meddelande. Saknas avslutande `/` i `BaseUrl` läggs det till automatiskt.

## 3. Projektstruktur

```
WestcoastEducation-Avalonia/
├── WestcoastEducation.Desktop.slnx
├── Claude.md, README.md, PROJECT_CONTEXT.md, .gitignore
└── WestcoastEducation.Desktop/
    ├── App.axaml              FluentTheme (Light), gemensamma styles, inkluderar färgresurserna
    ├── App.axaml.cs           DI-konfiguration och start
    ├── appsettings.example.json   mall utan nyckel (versionshanteras)
    ├── appsettings.json           riktig nyckel (lokal, gitignorerad)
    ├── Models/        Person (bas), Student, Teacher, Course, CourseType
    ├── Services/      ICourseService/CourseService (GET, POST, PUT, DELETE)
    │                  IStudentService/StudentService (GET, POST)
    │                  ITeacherService/TeacherService (GET, POST, PUT)
    │                  RestDbSettings (läser appsettings.json)
    ├── ViewModels/    ViewModelBase, MainViewModel, CoursesViewModel, StudentsViewModel, TeachersViewModel
    ├── Views/         MainWindow (TabControl), CoursesView, StudentsView, TeachersView
    └── Styles/AppResources.axaml   alla färger som SolidColorBrush, används via {DynamicResource}
```

Varje flik: lista (ListBox) till vänster, formulär till höger. Formulärfälten är egna egenskaper i ViewModel; att välja en post i listan fyller formuläret (`OnSelectedXxxChanged`). Validering (obligatoriska fält, heltal, datumordning, unikt kursnummer/personnummer) görs i ViewModel och visas i `StatusMessage`. `CreateCourseFromForm`/`CreateTeacherFromForm` tar emot den post som uppdateras (eller `null` vid Lägg till), så att bara Uppdatera undantar den egna posten från dubblettkontrollen. Update-kommandona sparar den valda posten i en lokal variabel före `await`.

## 4. Viktiga beslut

| Beslut | Motivering |
|---|---|
| Ett Avalonia-projekt, inget testprojekt | Minsta möjliga struktur; testprojekt ingår inte i kraven. |
| Mall `avalonia.mvvm --remove-view-locator` | Flikarna sätter `DataContext` direkt; ingen ViewLocator/navigation behövs. |
| Projektnamn `.Desktop` (inte `.Avalonia`) | Undviker namespace-krock med Avalonias egna namespaces. |
| Ett interface + en implementation per entitet | Uppfyller VG-kravet tydligt; inget generiskt repository. |
| Ett kompetensområde per lärare (textfält `KnowledgeArea`) | Användarens beslut. Anges vid skapande och ändras via Uppdatera lärare. |
| Behåll Persons sju fält + `EmploymentDate` för lärare | Kontinuitet med uppgift 1 (användarens beslut). |
| Modeller förenklade | Inga konstruktorer med parametrar (enklare för formulär och JSON); `Employee`, `Administrator`, `EducationManager`, managers och JSON-fillagring återanvänds inte. |
| RestDB utan manuellt definierade fält | Testat: RestDB lagrar och returnerar alla fält utan schema, datum utan tidszonsförskjutning. Användaren bekräftade att posterna syns i RestDB:s JSON-vy. |
| Ingen bekräftelsedialog vid radering, ingen temaväxling, ingen refresh-knapp | Inte krav; undviker extra kod. |
| Kurstyp visas som `Classroom`/`Distance` | Samma enum-namn som i uppgift 1. |

## 5. Genomförda funktioner

- **Kurser:** lista, lägga till, uppdatera (VG), radera (G).
- **Studenter:** lista, lägga till.
- **Lärare:** lista, lägga till, uppdatera (VG) inklusive kompetensområde.
- Dynamiska resurser för färger, MVVM, DI, services med interface/implementation, konfigurationsfil för API-nyckeln, README.

## 6. Automatiska tester (utförda av Claude, 2026-10-27)

Utfördes med ett tillfälligt konsolprogram i Claudes scratchpad (inte i repot) som refererade appens projekt och körde mot RestDB. Endast tillfälliga testposter användes; alla raderades och samtliga collections hade 0 poster efteråt.

- **Rena HTTP-anrop mot `courses`:** GET (tom) → POST (201) → GET (200, alla sex fält) → DELETE (200) → GET (404). Godkänt.
- **Services, 27/27 godkända:** kurs POST → GET → PUT → GET → DELETE med kontroll av alla egenskaper; student POST → GET (alla sju fält inkl. å/ä/ö); lärare POST → GET → PUT → GET (kompetensområde, datum och övriga fält). Studenter och lärare städades via direkt HTTP DELETE eftersom appen inte har radering för dem.
- **ViewModels utan GUI, 21/21 godkända:** validering (ogiltigt kursnummer, slutdatum före start, tomma obligatoriska fält, kompetensområde krävs, dubbletter av kursnummer/personnummer), lägg till/uppdatera/radera via commands, formuläret fylls vid val, `CanExecute` för Uppdatera/Radera, ändringar sparade i RestDB (kontrollerat med ny ViewModel-instans).
- **Bygg:** `dotnet build` 0 fel, 0 varningar.
- **Start:** appen startar med huvudfönstret "Westcoast Education - Administration" när `appsettings.json` finns, och med fönstret "Konfigurationsfel" när den saknas (kontrollerat via fönstertitel, inte visuellt).

## 6b. Rättningar efter kodgranskning (2026-11-03)

Ändrade filer: `CoursesViewModel.cs`, `TeachersViewModel.cs`, `RestDbSettings.cs`.

1. **Dubblettkontroll:** "Lägg till" kunde tidigare skapa en dubblett av kursnummer/personnummer om en befintlig post var markerad (den markerade posten undantogs alltid). Nu undantas en post bara vid Uppdatera.
2. **Felhantering i `RestDbSettings`:** ogiltig JSON, saknat `RestDb`-avsnitt och ogiltig `BaseUrl` (även andra scheman än http/https) ger nu `InvalidOperationException` med svenskt meddelande, som visas i felfönstret i stället för att appen kraschar.
3. **Race condition vid Uppdatera:** den valda kursen/läraren sparas innan PUT-anropet. Om användaren byter markering under anropet uppdateras ändå rätt post i listan, och den nya markeringen behålls.
4. **`BaseUrl` utan avslutande `/`** kompletteras automatiskt, så att `.../rest` + `courses` blir `.../rest/courses`.

Tester utförda av Claude med ett tillfälligt konsolprogram i scratchpad (raderat efteråt), **33/33 godkända**:
- **ViewModels med fejkade services (18):** Lägg till med markerad kurs/lärare och samma nummer nekas; Lägg till med annans nummer nekas; Lägg till med nytt nummer fungerar; Uppdatera med oförändrat nummer fungerar (kurs och lärarens kompetensområde); Uppdatera till en annan posts nummer nekas; race-scenario (markeringen byts medan PUT pågår) uppdaterar rätt post och lämnar den andra orörd; Radera kurs fungerar.
- **Live mot RestDB (6):** kurs Lägg till → dubblettförsök med markerad kurs nekas → Uppdatera → kontroll i RestDB (exakt en post, ny titel) → Radera; lärare Lägg till → dubblettförsök nekas → Uppdatera kompetensområde → kontroll i RestDB → städning via HTTP DELETE. Testposterna är borttagna (kontrollerat).
- **`RestDbSettings` (9):** URL utan `/` kompletteras, URL med `/` oförändrad, ogiltig JSON, saknat `RestDb`, ogiltig URL, ftp-URL, tom ApiKey, saknad fil, samt att relativ adress blir `.../rest/courses`. Testfilerna skrevs i testprogrammets egen bin-mapp, inte i appens.
- **Bygg:** `dotnet restore` + `dotnet build` 0 fel, 0 varningar. **Start:** appen startar med huvudfönstret (kontrollerat via fönstertitel).
- Inte testat: felfönstret visuellt med de nya felmeddelandena, och race-scenariot i det riktiga GUI:t (bara på ViewModel-nivå).

## 6c. Pedagogiska kommentarer (2026-11-03)

Enbart kommentarer tillagda (ingen kodändring) som korta minnesanteckningar. Användaren tyckte att första versionen var för detaljerad, så de förenklades: princip "kommentera varför, inte vad", oftast en mening, varje begrepp förklaras bara en gång.
- `App.axaml.cs`: DI, `AddSingleton`, interface → implementation, gemensam `HttpClient`, `DataContext`.
- `RestDbSettings.cs`: deserialisering, varför konfigurationen valideras vid start.
- `ICourseService.cs` / `CourseService.cs`: interface vs implementation, injicerad `HttpClient`, `async`/`await`, JSON-serialisering, `EnsureSuccessStatusCode`.
- `CoursesViewModel.cs`: MVVM, `partial`, `ObservableCollection`, `[ObservableProperty]`, `[RelayCommand]`, `On...Changed`.
- Enstaka rader i `ViewModelBase`, `MainViewModel`, `TeachersViewModel`, `Person`, `CoursesView.axaml(.cs)`.
Verifierat: `git diff` innehåller bara kommentarsrader; `dotnet build` 0 fel, 0 varningar.

## 7. Manuella GUI-tester (rapporterade av användaren)

Användaren har rapporterat att följande har testats i GUI:t och fungerar:
- Skapa kurs, student och lärare.
- Uppdatera kurs.
- Uppdatera lärares kompetensområde.
- Radera kurs.
- Uppdateringar och radering verifierade efter omstart av appen.

Inte rapporterat som manuellt testat: byte av färg i `AppResources.axaml` (dynamiska resurser), dubblettmeddelande för personnummer i GUI:t.

## 8. Aktuell status

**Klart:** samtliga G- och VG-krav implementerade, automatiskt verifierade och manuellt testade enligt ovan.

**Återstår:** inget obligatoriskt. Eventuellt: manuell kontroll av dynamiska resurser (byt `PrimaryBrush` och starta om), push till GitHub när användaren godkänner.

**Kända begränsningar:**
- "Hantera kompetensområde" (G) och "uppdatera lärare" (VG) använder samma Uppdatera-knapp; bra att kunna förklara vid redovisning.
- Nätverksfel från RestDB visar .NET:s engelska feltext efter en svensk inledning.
- Ingen bekräftelse vid radering av kurs.
- Server API Key ligger i klartext i den lokala konfigurationsfilen (acceptabelt för skolprojekt).

## 9. Git

- Branch: `main`, spårar `origin/main`. Remote `origin`: `https://github.com/mubuyuk/Westcoast-Education-Avalonia.git`. Push kräver användarens uttryckliga godkännande.
- Commits (äldst först):
  - `6b6db55` Skapa Avalonia MVVM-projekt (net10.0, Avalonia 12.1.4)
  - `8b7f1c9` Lägg till modeller, RestDB-services, ViewModels, views och dynamiska resurser
  - `b063bff` Lägg till README med instruktioner och arkitekturbeskrivning
  - `8503ec6` Lägg till PROJECT_CONTEXT.md och hänvisning i Claude.md
  - följt av commiten med rättningarna efter kodgranskningen (se `git log`).

## 10. Säkerhet

- API-nyckeln läses enbart från `WestcoastEducation.Desktop/appsettings.json` (kopieras till build-mappen via `CopyToOutputDirectory`). Den hårdkodas aldrig och skrivs aldrig ut.
- `appsettings.json` står i `.gitignore` och får **aldrig** versionshanteras; `appsettings.example.json` innehåller bara en platshållare.
- Även `bin/`, `obj/`, `.vs/`, `.vscode/`, `.idea/` och `*.user` ignoreras.
- Kontrollerat 2026-10-27: `git check-ignore` bekräftar att filen ignoreras, och en sökning efter nyckelns värde i hela Git-historiken och staging gav 0 träffar.
- Vid tester: läs nyckeln från filen inne i skript/program, skriv aldrig ut den, och kontrollera `git status` innan commit.
