namespace FlutterCodeGenerator.Model.Ir;

public class ViewWidget
{
    public string Label { get; set; } = "";
    public List<Trigger> Triggers { get; } = new();
    /// <summary>Agregaty prezentowane w widoku (tylko do odczytu).</summary>
    public List<DataAggregate> Data { get; } = new();
    /// <summary>Agregaty wprowadzane w tym widoku zdaniem &lt;enter&gt; (pola edycji).</summary>
    public List<DataAggregate> EditableData { get; } = new();
    public ControllerClass? Controller { get; set; }
    public PresenterClass? Presenter { get; set; }
}
