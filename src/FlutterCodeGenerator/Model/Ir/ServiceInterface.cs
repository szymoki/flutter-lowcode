namespace FlutterCodeGenerator.Model.Ir;

public class ServiceInterface
{
    public string Label { get; set; } = "";
    public List<SOperation> Methods { get; } = new();
}

public class SOperation
{
    public string Label { get; set; } = "";
    public PredicateType Type { get; set; }
    public List<Parameter> Parameters { get; } = new();
}

public enum PredicateType { Read, Update, Delete, Check, Execute }
