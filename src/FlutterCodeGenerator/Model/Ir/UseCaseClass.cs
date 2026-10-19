namespace FlutterCodeGenerator.Model.Ir;

public class UseCaseClass
{
    public string Label { get; set; } = "";
    public bool IsInitial { get; set; }
    public List<UCOperation> Operations { get; } = new();
    public List<PresenterClass> Presenters { get; } = new();
    public List<ServiceInterface> Services { get; } = new();
    public List<UseCaseClass> InvokedUseCases { get; } = new();

    /// <summary>
    /// Operacja uruchamiająca przypadek użycia z zewnątrz: operacja pierwszego zdania <c>&lt;select&gt;</c>,
    /// a gdy przypadek użycia nie zaczyna się od <c>&lt;select&gt;</c> – <c>execute</c>.
    /// </summary>
    public UCOperation? EntryOperation { get; set; }
    /// <summary>Pojęcie pierwszego zdania <c>&lt;select&gt;</c> (nazwa przycisku uruchamiającego), jeśli istnieje.</summary>
    public string? EntryTriggerNotion { get; set; }
}

public class UCOperation
{
    public string Label { get; set; } = "";
    public bool IsInitial { get; set; }
    public bool IsInvoking { get; set; }
    public string? ReturnTo { get; set; }
    public UCOperation? Parent { get; set; }
    /// <summary>Widok, którego wyzwalacz uruchamia operację (null dla operacji bez widoku, np. execute).</summary>
    public ViewWidget? SourceView { get; set; }
    /// <summary>Nazwa wyzwalacza RSL, z której powstała etykieta operacji („close”).</summary>
    public string? TriggerNotion { get; set; }
    public List<Instruction> Instructions { get; } = new();
}
