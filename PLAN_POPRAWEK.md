# Plan poprawek translatora RslBis → Flutter

Plan dla agenta pracującego w repozytorium `flutter-lowcode` (stan wyjściowy: commit `2fc4a4c`). Etapy są ułożone tak, żeby każdy dało się zamknąć osobnym commitem i sprawdzić testem. Nie przeskakuj etapów 0 i 3: od nich zależą pozostałe.

## Kontekst

Translator (`src/FlutterCodeGenerator`, .NET 8) ma trzy fazy:

1. `Parser/RslBisAstBuilder.cs` buduje model RSL z drzewa ANTLR.
2. `Translation/RslToIrTranslator.cs` tłumaczy model RSL na IR (`Model/Ir`).
3. `Generator/FlutterCodeGenerator.cs` wypełnia szablony z `Templates/*.txt` i zapisuje projekt do `flutter_web/`.

Wejście testowe: `examples/example.txt` (5 przypadków użycia, 12 scenariuszy, 3 definicje `Data`).

Obecny wynik: 48 plików Dart, które się budują, ale:
- metody przypadków użycia są puste,
- nawigacja nie wynika ze scenariuszy,
- z 20 oczekiwanych przypisań wyzwalaczy do widoków poprawnych jest 13.

Szczegóły rozbieżności opisuje `praca/rozdzial-rezultaty.tex` (tabela `tab:rez-wyzwalacze`) i lista ograniczeń w `praca/rozdzial-implementacja.tex` (`sec:kon-ograniczenia`).

## Zasady pracy

- Gramatyki `RslBis.g4` nie zmieniaj. Żadna poprawka z tego planu jej nie wymaga.
- Nie edytuj ręcznie plików w `flutter_web/lib/`. Zmieniaj translator i generuj ponownie.
- Jeden etap to jeden commit. Wiadomość commita konkretna, np. `Przypisuj Scenario.Preconditions w VisitMainscenario`.
- Komentarze w kodzie po polsku, tylko tam, gdzie wyjaśniają *dlaczego*. Nie opisuj tego, co widać z kodu.
- Po każdym etapie uruchom testy i generację, przejrzyj diff w `flutter_web/lib/` i zaktualizuj plik wzorcowy (golden), jeśli zmiana jest zamierzona.
- Jeśli rozstrzygnięcie semantyczne nie wynika z planu ani z `praca/rozdzial-rslbis.tex`, zapisz je w sekcji „Decyzje” na końcu tego pliku zamiast zgadywać po cichu.

## Komendy

```bash
# generacja (z katalogu głównego repozytorium; zawsze z jawnymi argumentami)
dotnet run --project src/FlutterCodeGenerator -- examples flutter_web

# testy (po etapie 0)
dotnet test

# sprawdzenie wygenerowanego projektu
cd flutter_web
flutter create . --platforms web   # jednorazowo, tworzy web/
flutter pub get
flutter analyze
flutter build web --release

# pełny potok w kontenerach
docker compose run --rm generator && docker compose up --build flutter_web   # http://localhost:8787
```

## Etap 0. Testy przed zmianami

Cel: każda kolejna zmiana ma widoczny, porównywalny skutek.

1. Dodaj projekt `tests/FlutterCodeGenerator.Tests` (xUnit) i dołącz go do `src/FlutterCodeGenerator.sln`.
2. Test „golden”:
   - uruchamia potok (parser → translator → generator) dla `examples/example.txt` do katalogu tymczasowego,
   - porównuje każdy plik `lib/**/*.dart` i `pubspec.yaml` z kopią w `tests/golden/example/`.
   - Wzorzec aktualizuje się zmienną środowiskową, np. `UPDATE_GOLDEN=1 dotnet test`.
3. Żeby potok dało się wywołać z testu, wydziel z `Program.cs` metodę, np. `Pipeline.Run(string inputFolder, string outputFolder)`, zwracającą listę błędów.
4. Testy jednostkowe translatora na małych specyfikacjach w `tests/specs/*.txt`. Dla każdej poprawki z etapów 1–8 dodaj co najmniej jeden test sprawdzający IR, a nie tylko wynikowy tekst.

Akceptacja: `dotnet test` przechodzi na niezmienionym kodzie; wzorzec odpowiada obecnej zawartości `flutter_web/lib/`.

## Etap 1. Parser i punkt wejścia

1. Warunki wstępne. `RslBisAstBuilder.VisitMainscenario` (ok. wiersza 66) czyta `conditions`, ale nie przypisuje ich do `Scenario.Preconditions`. Przypisz.
   - Skutek do sprawdzenia: `ProcessPrecondition` zaczyna działać, a `IOrder.deleteOrder` dostaje parametr `Order` (warunek `{user ? cashier; order}`).
   - Zastanów się, co z warunkiem `user ? cashier`. Dziś powstałaby operacja `validateUser` w usłudze `IUser`. Jeśli to niepożądane, pomiń warunki z podmiotem `user` i zapisz decyzję.
2. Negacja. `VisitValuecondition` (ok. wiersza 119) sprawdza `GetText().Contains("'-'")`. Zastąp sprawdzeniem, czy wśród dzieci kontekstu jest węzeł terminalny o tekście `-`. Dodaj test z `[order ? - valid]`.
3. Błędy składniowe w `Program.cs`:
   - podepnij własny `IAntlrErrorListener<IToken>` do parsera i `IAntlrErrorListener<int>` do leksera,
   - zbieraj błędy z nazwą pliku, wierszem i kolumną,
   - gdy są błędy, wypisz je, nie uruchamiaj translacji i zakończ kodem wyjścia 1.
4. Kolejność plików: posortuj wynik `Directory.GetFiles` (ok. wiersza 52) ordynalnie.
5. Ścieżki domyślne: usuń `../../../examples` i `../../../flutter_web`. Bez argumentów wypisz sposób użycia i zakończ kodem 2.
6. Brak plików wejściowych: zamiast generować szkielet z pustego IR, zgłoś błąd i zakończ kodem 1.

Akceptacja: testy dla punktów 1–2; ręczne sprawdzenie, że plik z `<shows>` daje kod wyjścia 1 i komunikat z numerem wiersza.

## Etap 2. Szybkie poprawki generatora

Wszystkie w `Generator/FlutterCodeGenerator.cs` i szablonach.

1. Podwójne `Future` (`WriteServiceInterface`, ok. wierszy 170–187). Zmienna `ret` już zawiera `Future<…>`, a potem jest opakowywana drugi raz. Typ zwracany:
   - `<check>` → `Future<XCheckResult>`,
   - `<read>` → `Future<X>`, gdzie `X` to klasa modelu odczytywanego pojęcia, a nie `dynamic`,
   - pozostałe → `Future<void>`.
2. Nazwy:
   - generator woła `ToPascalCase` na etykietach już zapisanych w PascalCase (np. `ToPascalCase(notionName)` przy `CheckResult`), stąd `OrdersearchCheckResult`,
   - przenieś oryginalną nazwę pojęcia do IR (pole `NotionName` w `DataAggregate` i `ServiceInterface`) i z niej wyprowadzaj nazwy w generatorze.
   - Akceptacja: plik `order_search_check_result.dart`, klasa `OrderSearchCheckResult`.
3. `TrimStart('I')` (m.in. w `Generate`, `WriteServiceInterface`, `WriteUseCase`) usuwa wszystkie wiodące `I`. Po punkcie 2 nie będzie potrzebne, bo nazwa pojęcia jest w IR. Dodaj test z pojęciem `item`.
4. `copyWith` w `ModelObject.txt` / `WriteDataAggregate` ma parametry pozycyjne. Zmień na nazwane: `Order copyWith({int? number, String? title, String? dates})`.
5. Importy modeli zagnieżdżonych. `WriteDataAggregate` nie importuje klas użytych jako typy pól (dziś `order_list.dart` używa `Order` bez importu). Dodaj importy dla pól, których typ jest pojęciem.
6. `pubspec`:
   - do `Templates/Pubspec.txt` dodaj sekcję `flutter:` z `uses-material-design: true`,
   - w `WritePubspec` dopisuj ją do istniejącego pliku, jeśli jej brak, tak jak dziś dopisywane jest `provider`.
7. Klasy pośredniczące: generuj `*_service_proxy.dart` tylko wtedy, gdy plik nie istnieje (jak `WriteCheckResultStub`). Interfejsy generuj zawsze.
8. Pliki nieaktualne: przed generacją usuń zawartość podkatalogów `lib/models`, `lib/views`, `lib/controllers`, `lib/presenters`, `lib/use_cases` oraz pliki `lib/services/i_*.dart`. Nie usuwaj proxy ani plików spoza tych katalogów.

Akceptacja: `flutter analyze` w `flutter_web` bez błędów (ostrzeżenia dopuszczalne, opisz je w raporcie).

## Etap 3. Spójność operacji przypadków użycia w IR i w kodzie

To warunek konieczny etapów 4–9.

Problem:
- `WriteUseCase` (ok. wierszy 257–261) nie generuje metod z `uc.Operations`. Zbiera nazwy z kontrolerów, których `UseCase` wskazuje tę klasę, i dokleja na sztywno `execute` oraz `handleClose`.
- `ProcessSelectSentence` dodaje `UCOperation` do `ctx.CurrentUCC`, a `COperation` do kontrolera bieżącego widoku, który może należeć do innego przypadku użycia.
- Efekt: `handleFindOrder` z instrukcjami jest w IR w `FindOrder`, a pusta metoda o tej nazwie ląduje w `StartUseCase`.

Zmiany:
1. `COperation`: zamiast `string? InvokedOperation` dodaj `UCOperation? Target` i `UseCaseClass? TargetUseCase`.
2. `ControllerClass`: kontroler może wołać operacje wielu przypadków użycia. Zamiast jednego `UseCase` wylicz zbiór `TargetUseCase` z operacji.
3. `WriteController` i `Templates/Controller.txt`: konstruktor przyjmuje wszystkie potrzebne przypadki użycia, a metoda `onX()` woła `_<uc>.<target>()`.
4. `WriteApp`: przekazuje do kontrolerów właściwe instancje przypadków użycia.
5. `WriteUseCase`: metody tylko z `uc.Operations`. Usuń doklejany `handleClose`. `execute` zostaje, bo powstaje w `ProcessUseCaseClause`.
6. Ustal i zapisz w „Decyzjach”, do którego przypadku użycia należy operacja zdania `<select>`. Proponowane: do przypadku użycia, w którego scenariuszu stoi zdanie (`ctx.CurrentUCC`), niezależnie od tego, w którym widoku jest przycisk.

Akceptacja: w IR i w kodzie każda `UCOperation` występuje dokładnie raz, w klasie swojego przypadku użycia. Test: dla `example.txt` klasa `StartUseCase` nie ma metody `handleFindOrder`, a `FindOrderUseCase` ją ma.

## Etap 4. Stan translacji w scenariuszach alternatywnych

Problem:
- `ProcessScenario` ustawia tylko `CurrentLabel`.
- Zdania scenariusza alternatywnego są tłumaczone w stanie pozostawionym przez poprzedni scenariusz: widok, operacja, gałąź decyzji i kontekst danych.
- `LabelToVF` jest uzupełniane tylko w `ProcessShowSentence`.

Zmiany:
1. Dodaj do `TranslationContext` słownik migawek `Dictionary<string, LabelSnapshot> LabelSnapshots`, czyszczony przy każdym nowym przypadku użycia (etykiety są lokalne dla przypadku użycia). `LabelSnapshot` zawiera:
   - `CurrentVF`,
   - `CurrentUCO`,
   - `CurrentCondition`,
   - kopie `CurrentDAP` i `CurrentDAD`.
2. W `ProcessSentence`, po przetworzeniu każdego zdania z etykietą (główną lub alternatywną), zapisz migawkę pod tą etykietą.
3. W `ProcessScenario` przed przetworzeniem zdań odtwórz stan z migawki powtórzonej etykiety. Brak etykiety oznacza błąd semantyczny: zgłoś go z nazwą przypadku użycia.
4. `rejoin`:
   - `FindOperationByLabel` porównuje `op.Label` (np. `handleAdd`) z etykietą (np. `01`) i szuka tylko w gałęziach decyzji,
   - przepisz to tak, żeby korzystało z migawek: operacja i pozycja instrukcji związane z etykietą,
   - skopiuj instrukcje od tej pozycji (zgodnie z regułą z artykułu: kopia wywołań od wskazanego miejsca).
5. Zdanie warunkowe w scenariuszu alternatywnym ma dodawać gałąź do decyzji z operacji odtworzonej z migawki. Dziś dla `[order ? notOK]` powstaje nowa decyzja w `handleCancel`.

Akceptacja (testy na IR dla `example.txt`):
- `cancel` z `Add order` jest wyzwalaczem `AddOrderForm`,
- `cancel` z `Find order` jest wyzwalaczem `OrderSearchForm`,
- `cancel` z `Delete order` jest wyzwalaczem `DeleteConfirmationMessage`,
- decyzja w `handleAdd` ma dwie gałęzie (`OK`, `notOK`),
- `correct` jest w `OrderErrorMessage`.

## Etap 5. `<invoke>` i pierwsze zdanie przypadku użycia

Problem:
- Pierwszy `<select>` przypadku użycia trafia do ostatniego widoku przypadku zapisanego w pliku wcześniej.
- `<invoke>` dopisuje puste akcje do `ScheduledInvocations`.

Zmiany:
1. Pierwszy przebieg po wszystkich przypadkach użycia: dla każdego zapamiętaj pierwsze zdanie scenariusza głównego. Jeśli jest to `<select>`, utwórz od razu jego `UCOperation` (`handleX`, `IsInitial = true`) w klasie tego przypadku użycia.
2. Przy przetwarzaniu pierwszego `<select>` przypadku użycia nie dodawaj wyzwalacza do `ctx.CurrentVF`. Ustaw tylko `CurrentUCO` na operację z punktu 1.
3. `<invoke> A, B` aktora: do `ctx.CurrentVF` dodaj wyzwalacz każdego wywoływanego przypadku użycia i operację kontrolera, której `Target` to początkowa operacja tego przypadku. Dopisz przypadek do `UseCaseClass.InvokedUseCases` bieżącego przypadku użycia.
4. Przypadek wywołany, którego pierwsze zdanie nie jest `<select>`: zgłoś ostrzeżenie i zapisz decyzję.
5. Systemowe `<invoke>` (`SysInvoke`): instrukcja `Call` do operacji początkowej wywołanego przypadku użycia.
6. `ScheduledInvocations` usuń, jeśli po zmianach nie jest używane.
7. Zdanie `00: User <select> application` w `Start`: nie ma widoku, w którym mógłby być przycisk. Operacja `handleApplication` jest punktem startu aplikacji (zob. etap 9).

Akceptacja (testy na IR):
- `MainMenu` ma wyzwalacze `showOrderList`, `findOrder`, `addOrder`,
- `OrderListWindow` ma `close`, `orderList`, `findOrder`, `addOrder`, `deleteOrder`,
- `SearchErrorMessage` ma tylko `close`,
- `OrderErrorMessage` ma `close` i `correct`.

Wynik nie zależy od kolejności przypadków użycia w pliku (test: ta sama specyfikacja z przestawionymi przypadkami użycia daje identyczne IR co do zbiorów wyzwalaczy).

## Etap 6. Dane w widokach

1. `<read>` (`ProcessSystemDataSentence`): po dodaniu operacji usługi dodaj odczytany agregat do `CurrentDAP`, bez oznaczania pól jako edytowalnych.
   - Akceptacja: `OrderWindow` pokazuje dane `Order`, a nie pole `Title` z `OrderSearch`; `OrderListWindow` dostaje dane `OrderList`.
2. Edytowalność:
   - dziś `ProcessEnterSentence` ustawia `EditableStateItem = true` na współdzielonych `DataItem` w `ViewModel`, więc każdy widok pokazujący `order` ma pola edycji,
   - przenieś informację do widoku: `ViewWidget.EditableData` (lista agregatów wprowadzanych w tym widoku) obok `ViewWidget.Data` (agregaty prezentowane),
   - `WriteView` generuje pola edycji tylko dla `EditableData`, a dla `Data` widgety tylko do odczytu (np. `ListTile`/`Text` z wartością z prezentera).
   - Akceptacja: `OrderSuccessMessage` nie ma pól `TextField`.
3. Prezentowanie danych: widok czyta wartości z prezentera (`presenter.order?.title`). Dziś prezenter przechowuje dane, ale widok ich nie wyświetla.
4. Krotność `[pojęcie]`:
   - w `Translate` przepisz `DataAttribute.IsMult` do nowego pola `DataItem.IsList`,
   - w `WriteDataAggregate` usuń warunek `isList` oparty na `TypeKind == agg.Label` i generuj pole `List<Order>` (z `copyWith` i importem),
   - w widoku dla pola listowego generuj `ListView.builder` z `ListTile` na element.
5. Referencja `-> pojęcie`: zostaw obecne odwzorowanie na typ klasy. Zapisz w „Decyzjach”, że różnica między `notion` a `-> notion` nie jest odwzorowywana.

## Etap 7. Wyniki sprawdzenia jako typ wyliczeniowy

1. Dla każdego pojęcia sprawdzanego zdaniem `<check>` zbierz wartości z warunków wartościowych dotyczących tego pojęcia w tym samym przypadku użycia (`[order ? OK]`, `[order ? notOK]`). Zapisz je w `ViewModel.Enums` jako `EnumUnion` `OrderCheckResult` (klasa już istnieje w `Model/Ir/ViewModel.cs`).
2. `WriteCheckResultStub` zastąp generacją `enum OrderCheckResult { ok, notOk }`.
   - Nazwy wartości w camelCase.
   - Wartości kolidujące ze słowami zastrzeżonymi Darta poprzedź prefiksem.
3. Jeśli pojęcie jest sprawdzane, a żaden warunek nie podaje wartości, wygeneruj `enum` z jedną wartością `ok` i ostrzeżeniem.

## Etap 8. Ciała metod przypadków użycia

Odwzorowanie instrukcji IR na Darta w `WriteUseCase`:

| Instrukcja | Kod |
|---|---|
| `Call` operacji prezentera | `_pX.showX(argumenty);` oraz ukrycie poprzednio widocznego widoku przypadku użycia (`hide()`) |
| `Call` operacji usługi `<check>` | `final xResult = await _x.checkX(argumenty);` |
| `Call` operacji usługi `<read>` | `final x = await _x.readX(argumenty);`; wartość trafia jako argument do następnego `showY` |
| `Call` pozostałych operacji usług | `await _x.updateX(argumenty);` |
| `Decision` z gałęziami `Condition` | `if (xResult == XCheckResult.ok) { … } else if (…) { … }` na wyniku ostatniego `check` danego pojęcia |
| `End(v)` | ukrycie widoków przypadku użycia i zwrot wartości `v` do przypadku wywołującego (etap 9) |

Szczegóły:
1. Metody zawierające `await` muszą być `Future<void> handleX() async`. Kontroler woła je bez `await` albo sam jest `async`; wybierz jedno i zapisz.
2. Argumenty:
   - nazwy parametrów pochodzą z `Call.Parameters`,
   - wartości z pól przypadku użycia przechowujących bieżące dane.
   - Dane z formularza: widok przy kliknięciu przekazuje kontrolerowi obiekt modelu zbudowany z `TextEditingController`. Sygnatury `onX(Order order)` i `handleX(Order order)` generuj tylko dla wyzwalaczy widoków z `EditableData`.
   - Konwersje tekstu na `int`, `double`, `bool`, `DateTime`: `int.tryParse` itd. Błąd konwersji traktuj jak wartość nieprawidłową i zapisz decyzję.
3. Dodaj test golden ciała `AddOrderUseCase.handleAdd` po regeneracji.

Akceptacja: `flutter analyze` bez błędów; ręcznie w przeglądarce kliknięcie `add` z poprawnym zamówieniem wywołuje `checkOrder`. Proxy zgłasza `UnimplementedError`; na potrzeby próby uzupełnij lokalnie proxy o zwrot `OrderCheckResult.ok`, ale nie commituj tego jako części generatora.

## Etap 9. Nawigacja i punkt startu

1. `Templates/App.txt`: zamiast listy wszystkich widoków ekran startowy wywołuje `StartUseCase.execute()` / `handleApplication()`, które pokazują `MainMenu`.
2. Wybierz jeden mechanizm wyświetlania widoków i zapisz decyzję:
   - wariant A (prostszy, zgodny z obecnymi prezenterami): jeden ekran z `Stack`/`IndexedStack` widoków, widoczność sterowana `isVisible` prezenterów; `showX` ustawia `isVisible = true`, `End` i przejście do innego widoku ustawiają `hide()`;
   - wariant B: `Navigator` sterowany z przypadków użycia przez wstrzyknięty obiekt nawigacji.
3. Wywołania przypadków użycia:
   - operacja wywołująca zapamiętuje, gdzie wrócić (pola `UCOperation.ReturnTo` i `Parent` już istnieją w IR, nieużywane),
   - `End(v)` w przypadku wywołanym przekazuje `v` do przypadku wywołującego, który kontynuuje od zdania po `<invoke>` (w praktyce `rejoin` w `Start` pokazuje ponownie `MainMenu`).
4. Usuń z widoków `initState` wołające `show()`, jeśli widoczność steruje się z przypadków użycia.
5. Listę wszystkich widoków można zachować jako tryb diagnostyczny (np. flaga w `app.dart`), ale nie jako ekran startowy.

Akceptacja: po uruchomieniu aplikacja startuje od `MainMenu`. Ścieżka `Add order` (kliknięcie `addOrder`, wypełnienie formularza, `add`) prowadzi do `OrderSuccessMessage` przy wyniku `ok` i do `OrderErrorMessage` przy `notOk`. `ok`/`close` wraca do menu.

## Etap 10. Rozszerzenia (opcjonalne, po etapach 0–9)

1. Definicje `View`/`Trigger`: etykiety `{…}` jako teksty tytułów i przycisków zamiast nazw technicznych. Typ widoku (`*form*`, `*window*`) tylko zapisz w IR.
2. Typ `date`: `TextField` z `showDatePicker` i konwersją na `DateTime`.
3. `<close>`: ukrycie wskazanego widoku (`hide()`).
4. Walidacja semantyczna przed translacją, reguły S1–S8 z tabeli `tab:kon-semantyka` w `praca/rozdzial-implementacja.tex`. Błędy przez ten sam mechanizm co w etapie 1, z kodem wyjścia 1.

## Etap 11. Porządki i dokumentacja

1. Usuń nieużywany `src/FlutterCodeGenerator/Generators/FlutterModuleGenerator.cs`.
2. Zostaw jedną kopię gramatyki: `src/FlutterCodeGenerator/Parser/Grammar/RslBis.g4`. Kopia w katalogu głównym różni się tylko końcami wierszy.
3. README:
   - usuń opis domyślnych ścieżek,
   - dodaj `dotnet test` i kody wyjścia (0 sukces, 1 błędy specyfikacji, 2 złe użycie).
4. Wygeneruj ponownie `flutter_web/` i zcommituj wynik.
5. Zrób nowe zrzuty ekranu do `screenshots/`, z tymi samymi nazwami plików plus zrzuty ścieżki `Add order` (sukces i błąd).

## Kryteria akceptacji całości

- `dotnet test` przechodzi.
- `flutter analyze` w `flutter_web` bez błędów.
- `docker compose run --rm generator && docker compose up --build flutter_web` buduje i serwuje aplikację.
- Przypisanie wyzwalaczy dla `example.txt` jest zgodne z tabelą poniżej (20 z 20):

| Widok | Wyzwalacze |
|---|---|
| `MainMenu` | `showOrderList`, `findOrder`, `addOrder` |
| `OrderSearchForm` | `search`, `cancel` |
| `OrderWindow` | `close` |
| `SearchErrorMessage` | `close` |
| `OrderListWindow` | `close`, `orderList`, `findOrder`, `addOrder`, `deleteOrder` |
| `AddOrderForm` | `add`, `cancel` |
| `OrderSuccessMessage` | `ok` |
| `OrderErrorMessage` | `close`, `correct` |
| `DeleteConfirmationMessage` | `delete`, `cancel` |
| `OrderDeletedMessage` | `ok` |

Przyjęte założenie: wyzwalacz scenariusza alternatywnego, który powtarza etykietę zdania `<enter>`, trafia do widoku, w którym to zdanie jest wykonywane.

- Translator kończy się kodem 1 dla specyfikacji z błędem składniowym.

## Poza zakresem

- Zmiany gramatyki RslBis.
- Generowanie logiki dziedziny w klasach pośredniczących i backendu.
- Inne platformy niż Flutter Web, poza tym, co daje `flutter create`.

## Raport końcowy

Po zakończeniu dopisz na końcu tego pliku krótki raport. Posłuży do aktualizacji rozdziałów pracy.

1. Lista zrealizowanych etapów z hashami commitów.
2. Które punkty listy ograniczeń z `praca/rozdzial-implementacja.tex` (sekcja `sec:kon-ograniczenia`) i tabeli `tab:kon-decyzje` przestały obowiązywać.
3. Nowe liczby dla `example.txt`:
   - liczba plików i wierszy Dart,
   - zgodność wyzwalaczy,
   - wynik `flutter analyze`,
   - rzeczywisty log z uruchomienia translatora i kod wyjścia.
4. Wszystkie wpisy z sekcji „Decyzje”.

## Decyzje

(uzupełnia agent w trakcie pracy: rozstrzygnięcie, uzasadnienie, miejsce w kodzie)

- **D0.1 – `dotnet test src` zamiast samego `dotnet test`.** Projekt testów dołączono do `src/FlutterCodeGenerator.sln`, zgodnie z etapem 0. Katalog główny repozytorium nie ma pliku solucji, więc `dotnet test` trzeba wywołać z argumentem `src` (jedyna solucja w tym katalogu). Dodanie drugiej solucji w katalogu głównym tylko po to, by skrócić komendę, uznano za zbędne. Miejsce: `src/FlutterCodeGenerator.sln`, README.
- **D0.2 – Dockerfile buduje sam projekt generatora.** Solucja odwołuje się do `../tests`, których nie kopiuje się do obrazu, więc `dotnet restore` na solucji przestałby działać. `docker/Dockerfile.dotnet` publikuje bezpośrednio `FlutterCodeGenerator.csproj`.
- **D1.1 – Warunki wstępne z podmiotem `user` są pomijane w translacji.** `{user ? cashier}` określa rolę aktora, a nie stan danych systemu. Tworzenie z niego usługi `IUser` z operacją `validateUser` dawałoby interfejs, którego żaden przypadek użycia nie wywołuje. Miejsce: `RslToIrTranslator.ProcessPrecondition`.
- **D1.2 – Agregat z warunku kontekstowego jest parametrem wszystkich operacji usług danego przypadku użycia.** Warunek `{…; order}` oznacza, że przypadek użycia działa na wskazanym obiekcie przez cały czas trwania. Dlatego agregat trafia do osobnej listy `TranslationContext.ContextDA`, której nie czyści zdanie `<show>` (w przeciwieństwie do `CurrentDAD`). Dla `example.txt` skutkiem jest `deleteOrder(Order order)`. Miejsce: `ProcessPrecondition`, `ProcessSystemDataSentence`.
- **D1.3 – Po błędzie składniowym model RSL nie jest budowany.** Drzewo naprawione przez strategię ANTLR może nie mieć wymaganych węzłów, a `RslBisAstBuilder` zakłada ich obecność. Parser zgłasza wszystkie błędy ze wszystkich plików, potem potok kończy się bez translacji i bez zapisu plików. Kolumna w komunikacie liczona od 1. Miejsce: `Pipeline.Parse`, `Parser/SyntaxErrorCollector.cs`.
- **D2.1 – Zaślepka proxy jest nadpisywana, dopóki zawiera znacznik w pierwszej linii.** Plan zakładał generację proxy tylko wtedy, gdy plik nie istnieje. Przy takiej regule każda zmiana sygnatury interfejsu (a etapy 3–8 je zmieniają) zostawiałaby nieaktualną, niekompilującą się zaślepkę, której nikt nie edytował. Dlatego generator pisze w pierwszej linii proxy znacznik `// WYGENEROWANO: …`. Plik ze znacznikiem jest nadpisywany. Plik bez niego (programista usunął znacznik, uzupełniając implementację) nie jest ruszany. Miejsce: `FlutterCodeGenerator.WriteServiceInterface`, `ProxyStubMarker`.
- **D2.2 – Jednorazowe usunięcie starych zaślepek w `flutter_web/lib/services`.** Zaślepki z commita `2fc4a4c` nie miały znacznika z D2.1, a nie były edytowane ręcznie (zgodne z wynikiem generatora). Usunięto je przed regeneracją w etapie 2, żeby powstały na nowo ze znacznikiem.
- **D2.3 – `<read>` tworzy klasę modelu czytanego pojęcia.** Typ zwracany `readX` to `Future<X>`, więc klasa `X` musi istnieć także wtedy, gdy specyfikacja nie definiuje `Data X`. Powstaje wtedy klasa bez pól. Miejsce: `RslToIrTranslator.ProcessSystemDataSentence`.
- **D3.1 – Operacja zdania `<select>` należy do przypadku użycia, w którego scenariuszu stoi zdanie** (`ctx.CurrentUCC`), niezależnie od tego, w którym widoku jest przycisk. Kontroler może więc wołać operacje kilku przypadków użycia i dostaje w konstruktorze wszystkie, których potrzebuje (`ControllerClass.TargetUseCases`). Miejsce: `RslToIrTranslator.ProcessSelectSentence`, `FlutterCodeGenerator.WriteController`.
- **D3.2 – Kolizja nazw operacji w jednym przypadku użycia: sufiks z nazwą widoku źródłowego.** Ten sam wyzwalacz w różnych widokach jednego przypadku użycia (w `Find order`: `close` w `OrderWindow` i w `SearchErrorMessage`) daje dwie operacje o różnym przebiegu. Dart nie ma przeciążania, więc po translacji wszystkie kolidujące operacje dostają sufiks z nazwą widoku: `handleCloseOrderWindow`, `handleCloseSearchErrorMessage`. Operacje bez kolizji zachowują krótką nazwę. Miejsce: `RslToIrTranslator.MakeOperationLabelsUnique`.
- **D3.3 – Powtórzony wyzwalacz w tym samym widoku nie jest dublowany.** Gdy w widoku jest już przycisk o tej nazwie, wskazujący inną operację, zostaje pierwsze przypisanie, a translator zgłasza ostrzeżenie (jeden przycisk nie może uruchamiać dwóch operacji). Miejsce: `RslToIrTranslator.AddTrigger`.
- **D4.1 – Scenariusz alternatywny kontynuuje ze stanu *po* powtórzonym zdaniu.** `NN: -"-` oznacza, że zdanie NN zostało wykonane, a dalej następują zdania alternatywne. Tylko taka interpretacja pasuje do wszystkich scenariuszy z `example.txt`: w `04: -"- [order ? notOK]` warunek wymaga wyniku `<check>` z kroku 04. Migawka zapisywana jest po każdym zdaniu z etykietą (główną lub alternatywną), a słownik czyszczony na początku każdego przypadku użycia. Miejsce: `RslToIrTranslator.ProcessSentence`, `ProcessScenario`, `Translation/LabelSnapshot.cs`.
- **D4.2 – `rejoin` kopiuje instrukcje od pozycji, w której zdanie docelowe zaczęło je dopisywać, z dwoma wyjątkami.** (a) Gdy cel leży wcześniej w tej samej liście instrukcji co miejsce `rejoin` (np. `Start`: `-> rejoin 01` w `handleApplication`), kopia zdublowałaby wywołania. To pętla powrotu do widoku i nic się nie kopiuje. (b) Zdanie aktora (`<select>`, `<enter>`) nie tworzy instrukcji, więc nie ma czego kopiować. W obu przypadkach, jak i przy zwykłej kopii, etykieta celu trafia do `UCOperation.ReturnTo` (do użycia w etapie 9). Kopia trafia do gałęzi decyzji, w której stoi zdanie końcowe, a nie poza nią. Miejsce: `ProcessRejoinSentence`, `ProcessEndSentence`.
- **D4.3 – Każdy przypadek użycia zaczyna translację bez widoku, gałęzi i danych poprzedniego.** Dotąd pierwszy `<select>` przypadku użycia trafiał do ostatniego widoku przypadku zapisanego wcześniej w pliku. Po zmianie przycisk taki nie powstaje wcale, dopóki etap 5 nie doda go przez `<invoke>`. Nowa operacja z `<select>` zaczyna się też poza gałęzią decyzji poprzedniej operacji (wcześniej `End(OK)` z `07: <select> ok` trafiał do gałęzi `OK` w `handleAdd`). Miejsce: `ProcessUseCaseClause`, `ProcessSelectSentence`.
- **D4.4 – Etykiety zdań scenariuszy alternatywnych.** `SvoSentence.Label` ma wartość domyślną `""`, a nie `null`, więc `svo.Label ?? svo.AltLabel` zawsze dawało pusty napis dla zdań `A1`, `B1` itd. Poprawiono na sprawdzenie pustego napisu, bez tego migawka `B1` (scenariusz z `C1: correct`) by nie powstała.
- **D5.1 – Operacja wejściowa przypadku użycia.** Pierwszy przebieg translacji tworzy klasy wszystkich przypadków użycia oraz, gdy pierwsze zdanie scenariusza głównego to `<select> X`, operację `handleX` z `IsInitial = true` (`UseCaseClass.EntryOperation`, `EntryTriggerNotion`). Zdanie to nie dodaje przycisku do żadnego widoku. Przycisk powstaje w widoku przypadku wywołującego, gdy ten ma `<invoke>`, i ma nazwę pojęcia z tego zdania (`find order` → `findOrder`). Dla `Start` (`00: User <select> application`) `handleApplication` nie ma przycisku: to punkt startu aplikacji. Poprawiono też przy okazji błąd, przez który `IsInitial` przechodziło łańcuchowo na każdą kolejną operację (`isInitial` brane z poprzedniej operacji). Miejsce: `RslToIrTranslator.CreateUseCaseClass`, `ProcessFirstSelectSentence`.
- **D5.2 – Przypadek wywoływany, który nie zaczyna się od `<select>`:** ostrzeżenie, a przycisk w widoku wywołującym ma nazwę przypadku użycia (`<invoke> Refresh` → `refresh`) i uruchamia operację `execute`. Miejsce: `ProcessActorInvokeSentence`.
- **D5.3 – `<invoke>` nieznanego przypadku użycia i dwukrotna definicja przypadku użycia to błędy** (kod wyjścia 1), tak jak nieznana etykieta w etapie 4. Nie da się wygenerować wywołania klasy, która nie istnieje. `<invoke>` aktora przed pierwszym `<show>` daje tylko ostrzeżenie (brak widoku na przycisk), a zależność `InvokedUseCases` i tak jest zapisywana.
