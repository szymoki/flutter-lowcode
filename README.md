# kompilator RSL-bis → Flutter (.NET) + aplikacja web

Kompilator w .NET: parsuje pliki **RSL-bis** (język niskokodowy), buduje reprezentację pośrednią (IR) i generuje pełny kod źródłowy aplikacji we **Flutterze** (Dart). Docker Compose pozwala uruchomić generator i zbudowaną aplikację Flutter Web na Linuxie.

## Struktura projektu

```
magistrka/
├── src/FlutterCodeGenerator/         # Kompilator .NET
│   ├── Parser/
│   │   ├── Grammar/RslBis.g4         # Gramatyka ANTLR4
│   │   ├── Generated/               # Wygenerowany parser (RslBisLexer, RslBisParser, …)
│   │   └── RslBisAstBuilder.cs      # Visitor budujący model RSL z AST
│   ├── Model/
│   │   ├── Ir/                      # Reprezentacja pośrednia (IR)
│   │   └── Rsl/                     # Model domenowy RSL-bis
│   ├── Translation/
│   │   └── RslToIrTranslator.cs     # Translacja RSL-bis → IR (12 reguł)
│   ├── Generator/
│   │   └── FlutterCodeGenerator.cs  # Generacja kodu Dart z IR
│   ├── Program.cs                    # parsuj → tłumacz → generuj
│   └── FlutterCodeGenerator.csproj
├── flutter_web/                      # Wygenerowana aplikacja Flutter
├── examples/                         # Przykładowe pliki .txt (RSL-bis) – opcjonalnie
├── docker/
├── docker-compose.yml
├── rslbis-flutter-compiler.md       # Szczegółowe instrukcje kompilatora
└── README.md
```

## Regeneracja parsera ANTLR4 (opcjonalnie)

Jeśli zmieniasz gramatykę `Parser/Grammar/RslBis.g4`:

```bash
cd src/FlutterCodeGenerator/Parser/Grammar
# Pobierz antlr-4.13.1-complete.jar (jeśli brak)
# java -jar antlr-4.13.1-complete.jar -Dlanguage=CSharp -visitor -no-listener RslBis.g4 -o ../Generated/
```

## Uruchomienie lokalne (bez Docker)

### Wymagania

- .NET 8 SDK
- Flutter SDK (dla budowania aplikacji web)
- (Opcjonalnie) Java – tylko do regeneracji parsera z pliku .g4

### Kompilator .NET

```bash
cd src
dotnet run --project FlutterCodeGenerator
# Domyślnie: szuka plików .txt w examples/, zapisuje wynik do flutter_web/

# Jawne katalogi:
dotnet run --project FlutterCodeGenerator -- <folder_z_plikami_.txt> <folder_wyjściowy>

# Przykład:
dotnet run --project FlutterCodeGenerator -- ./examples ./flutter_web
```

Jeśli w folderze wejściowym nie ma plików `.txt`, generator tworzy minimalny szkielet aplikacji Flutter w folderze wyjściowym.

### Aplikacja Flutter Web

```bash
cd flutter_web
flutter pub get
flutter run -d chrome   # lub: flutter run -d web-server --web-port=8080
```

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

## Rozszerzanie generatora

Logika generowania kodu Fluttera jest w `src/FlutterCodeGenerator/Generator/FlutterCodeGenerator.cs`. Można dodać nowe metody generujące kolejne typy plików i wywołać je z `Generate()`.
