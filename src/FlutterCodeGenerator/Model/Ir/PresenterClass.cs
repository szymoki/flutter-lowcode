namespace FlutterCodeGenerator.Model.Ir;

public class PresenterClass
{
    public string Label { get; set; } = "";
    public List<POperation> Methods { get; } = new();
}

public class POperation
{
    public string Label { get; set; } = "";
    public List<Parameter> Parameters { get; } = new();
}
