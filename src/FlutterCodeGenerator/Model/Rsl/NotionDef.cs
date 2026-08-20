namespace FlutterCodeGenerator.Model.Rsl;

public abstract class NotionDef;

public class ViewNotion : NotionDef
{
    public string ViewType { get; set; } = "";
    public List<NameAndLabel> NamesAndLabels { get; set; } = new();
}

public class TriggerNotion : NotionDef
{
    public string? TriggerType { get; set; }
    public List<NameAndLabel> NamesAndLabels { get; set; } = new();
}

public class DataNotion : NotionDef
{
    public string Name { get; set; } = "";
    public List<DataAttribute> Attributes { get; set; } = new();
}

public class NameAndLabel
{
    public string Name { get; set; } = "";
    public string? UiLabel { get; set; }
}

public class DataAttribute
{
    public string Name { get; set; } = "";
    public string TypeKind { get; set; } = ""; // datatype, notion ref, multnotion
    public bool IsMult { get; set; }
    public bool IsRef { get; set; }
    public string? NotionRef { get; set; }
}
