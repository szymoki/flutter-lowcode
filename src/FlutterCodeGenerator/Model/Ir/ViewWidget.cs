namespace FlutterCodeGenerator.Model.Ir;

public class ViewWidget
{
    public string Label { get; set; } = "";
    /// <summary>Etykieta z definicji <c>View</c> (<c>{…}</c>), wyświetlana jako tytuł.</summary>
    public string? UiLabel { get; set; }
    /// <summary>Typ z definicji <c>View *typ*</c> (np. form, window); zapisany, ale nie wpływa na kod.</summary>
    public string? ViewType { get; set; }
    public string Title => UiLabel ?? Label;
    public List<Trigger> Triggers { get; } = new();
    /// <summary>Agregaty prezentowane w widoku (tylko do odczytu).</summary>
    public List<DataAggregate> Data { get; } = new();
    /// <summary>Agregaty wprowadzane w tym widoku zdaniem &lt;enter&gt; (pola edycji).</summary>
    public List<DataAggregate> EditableData { get; } = new();
    public ControllerClass? Controller { get; set; }
    public PresenterClass? Presenter { get; set; }
}
