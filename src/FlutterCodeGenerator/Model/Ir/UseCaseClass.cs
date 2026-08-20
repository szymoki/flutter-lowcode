namespace FlutterCodeGenerator.Model.Ir;

public class UseCaseClass
{
    public string Label { get; set; } = "";
    public bool IsInitial { get; set; }
    public List<UCOperation> Operations { get; } = new();
    public List<PresenterClass> Presenters { get; } = new();
    public List<ServiceInterface> Services { get; } = new();
    public List<UseCaseClass> InvokedUseCases { get; } = new();
}

public class UCOperation
{
    public string Label { get; set; } = "";
    public bool IsInitial { get; set; }
    public bool IsInvoking { get; set; }
    public string? ReturnTo { get; set; }
    public UCOperation? Parent { get; set; }
    public List<Instruction> Instructions { get; } = new();
}
