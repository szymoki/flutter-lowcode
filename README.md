# kompilator RSL-bis → Flutter (.NET) + aplikacja web

Kompilator w .NET: parsuje pliki **RSL-bis** (język niskokodowy), buduje reprezentację pośrednią (IR) i generuje pełny kod źródłowy aplikacji we **Flutterze** (Dart). Docker Compose pozwala uruchomić generator i zbudowaną aplikację Flutter Web na Linuxie.

## Struktura projektu

```
flutter-lowcode/
├── src/
│   ├── FlutterCodeGenerator.sln      # solucja: generator + testy
│   └── FlutterCodeGenerator/         # Kompilator .NET
│       ├── Parser/
│       │   ├── Grammar/RslBis.g4     # Gramatyka ANTLR4 (jedyna kopia)
│       │   ├── Generated/            # Wygenerowany parser (RslBisLexer, RslBisParser, …)
│       │   ├── RslBisAstBuilder.cs   # Visitor budujący model RSL z AST
│       │   └── SyntaxErrorCollector.cs
│       ├── Model/
│       │   ├── Rsl/                  # Model domenowy RSL-bis
│       │   └── Ir/                   # Reprezentacja pośrednia (IR)
│       ├── Translation/              # Translacja RSL-bis → IR
│       ├── Generator/                # Generacja kodu Dart z IR
│       ├── Templates/                # Szablony plików Dart (zasoby osadzone)
│       ├── Pipeline.cs               # parsuj → tłumacz → generuj
│       └── Program.cs                # punkt wejścia, kody wyjścia
├── tests/
│   ├── FlutterCodeGenerator.Tests/   # testy xUnit (IR i wygenerowany kod)
│   ├── golden/example/               # wzorzec wyniku dla examples/
│   └── specs/                        # małe specyfikacje testowe
├── examples/                         # wejście: pliki .txt (RSL-bis)
├── flutter_web/                      # wygenerowana aplikacja Flutter
├── screenshots/                      # zrzuty ekranu wygenerowanej aplikacji
├── docker/
└── docker-compose.yml
```

## Uruchomienie lokalne (bez Dockera)

### Wymagania

- .NET 8 SDK
- Flutter SDK (do budowania aplikacji web)
- (Opcjonalnie) Java – tylko do regeneracji parsera z pliku .g4

### Kompilator .NET

Z katalogu głównego repozytorium:

```bash
dotnet run --project src/FlutterCodeGenerator -- <folder_z_plikami_.txt> <folder_wyjściowy>

# Przykład:
dotnet run --project src/FlutterCodeGenerator -- examples flutter_web
```

Oba argumenty są wymagane. Generator wczytuje wszystkie pliki `.txt` z folderu wejściowego (rekurencyjnie, w kolejności nazw) i zapisuje projekt Flutter do folderu wyjściowego.

Kody wyjścia:

| Kod | Znaczenie |
|---|---|
| 0 | sukces (ostrzeżenia wypisywane są na stderr) |
| 1 | błędy specyfikacji: składniowe (z plikiem, wierszem i kolumną), semantyczne albo brak plików `.txt`; nic nie jest zapisywane |
| 2 | złe użycie (brak argumentów) |

Przy ponownej generacji generator usuwa poprzednią zawartość `lib/models`, `lib/views`, `lib/controllers`, `lib/presenters`, `lib/use_cases` i interfejsy `lib/services/i_*.dart`. Zaślepki usług (`lib/services/*_service_proxy.dart`) są nadpisywane tylko dopóki zaczynają się od linii `// WYGENEROWANO: …`. Po uzupełnieniu implementacji usuń tę linię, a generator przestanie nadpisywać plik.

### Testy

```bash
dotnet test src
```

Test „golden” porównuje wynik generacji dla `examples/` z `tests/golden/example/`. Po zamierzonej zmianie generatora zaktualizuj wzorzec:

```bash
UPDATE_GOLDEN=1 dotnet test src
```

### Aplikacja Flutter Web

```bash
cd flutter_web
flutter create . --platforms web   # jednorazowo, tworzy web/
flutter pub get
flutter run -d chrome              # lub: flutter run -d web-server --web-port=8080
```

Aplikacja startuje od przypadku użycia `Start`. Lista wszystkich widoków jest dostępna jako tryb diagnostyczny: `App(viewListMode: true)` w `lib/main.dart`.

## Uruchomienie w Dockerze (Linux)

Generator czyta pliki RSL-bis (`.txt`) z `./examples` i zapisuje wygenerowaną aplikację do `./flutter_web`.

1. **Wygeneruj aplikację Flutter** z plików w `examples/`:

   ```bash
   docker compose run --rm generator
   ```

2. **Zbuduj i uruchom aplikację Flutter Web** (nginx na porcie 8787):

   ```bash
   docker compose up --build flutter_web
   ```

   Aplikacja: **http://localhost:8787**

Jedną komendą (generator + build + uruchomienie):

```bash
docker compose run --rm generator && docker compose up --build flutter_web
```

## Regeneracja parsera ANTLR4 (opcjonalnie)

Jeśli zmieniasz gramatykę `src/FlutterCodeGenerator/Parser/Grammar/RslBis.g4`:

```bash
cd src/FlutterCodeGenerator/Parser/Grammar
java -jar antlr-4.13.1-complete.jar -Dlanguage=CSharp -visitor -no-listener RslBis.g4 -o ../Generated/
```

## Rozszerzanie generatora

- Translacja RSL-bis → IR: `src/FlutterCodeGenerator/Translation/RslToIrTranslator.cs`.
- Generacja kodu Dart: `src/FlutterCodeGenerator/Generator/FlutterCodeGenerator.cs` (pliki) i `Generator/UseCaseWriter.cs` (ciała metod przypadków użycia).
- Wygląd generowanego kodu: szablony w `src/FlutterCodeGenerator/Templates/` (znaczniki `{{…}}`).

Decyzje projektowe podjęte przy poprawkach translatora opisuje sekcja „Decyzje” w `PLAN_POPRAWEK.md`.
