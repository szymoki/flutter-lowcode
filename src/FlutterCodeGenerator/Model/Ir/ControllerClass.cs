namespace FlutterCodeGenerator.Model.Ir;

public class ControllerClass
{
    public string Label { get; set; } = "";
    public List<COperation> Operations { get; } = new();
    public UseCaseClass? UseCase { get; set; }
}

public class COperation
{
    public string Label { get; set; } = "";
    public string? InvokedOperation { get; set; }
}
