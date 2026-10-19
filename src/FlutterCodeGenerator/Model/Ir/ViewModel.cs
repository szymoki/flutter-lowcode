namespace FlutterCodeGenerator.Model.Ir;

public class ViewModel
{
    public List<DataAggregate> Aggregates { get; } = new();
    public List<EnumUnion> Enums { get; } = new();
}

public class DataAggregate
{
    public string Label { get; set; } = "";
    /// <summary>Oryginalna nazwa pojęcia RSL („order search”), z której generator wyprowadza nazwy.</summary>
    public string NotionName { get; set; } = "";
    public DataAggregate? BaseType { get; set; }
    public List<DataItem> Items { get; } = new();
}

public class DataItem
{
    public string Label { get; set; } = "";
    public string TypeKind { get; set; } = "";
    /// <summary>Atrybut-kolekcja <c>[pojęcie]</c>.</summary>
    public bool IsList { get; set; }
    public DataItem? BaseDataItem { get; set; }
}

public class Parameter
{
    public string Label { get; set; } = "";
    public bool IsAttribute { get; set; }
    public string Type { get; set; } = "";
}

public class Trigger
{
    public string Label { get; set; } = "";
    /// <summary>Typ z definicji <c>Trigger *typ*</c>.</summary>
    public string Type { get; set; } = "";
    /// <summary>Etykieta z definicji <c>Trigger</c> (<c>{…}</c>), wyświetlana na przycisku.</summary>
    public string? UiLabel { get; set; }
    public string Text => UiLabel ?? Label;
}

public class EnumUnion
{
    public string Label { get; set; } = "";
    /// <summary>Pojęcie RSL, którego wynik sprawdzenia opisuje enum.</summary>
    public string NotionName { get; set; } = "";
    public List<EnumValue> Elements { get; } = new();
}

public class EnumValue
{
    public string Label { get; set; } = "";
}

public class Value
{
    public string Label { get; set; } = "";
}

public class Expression
{
    public string Notion { get; set; } = "";
    public string Val { get; set; } = "";
    public bool Negated { get; set; }
}
