namespace FlutterCodeGenerator.Model.Ir;

public class ViewWidget
{
    public string Label { get; set; } = "";
    public List<Trigger> Triggers { get; } = new();
    public List<DataAggregate> Data { get; } = new();
    public ControllerClass? Controller { get; set; }
    public PresenterClass? Presenter { get; set; }
}
