namespace FlutterCodeGenerator.Model.Ir;

public class IrModel
{
    public List<ViewWidget> Views { get; } = new();
    public List<ControllerClass> Controllers { get; } = new();
    public List<UseCaseClass> UseCases { get; } = new();
    public List<PresenterClass> Presenters { get; } = new();
    public List<ServiceInterface> Services { get; } = new();
    public ViewModel ViewModel { get; } = new();
}
