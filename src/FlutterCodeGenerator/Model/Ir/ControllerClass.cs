namespace FlutterCodeGenerator.Model.Ir;

public class ControllerClass
{
    public string Label { get; set; } = "";
    public List<COperation> Operations { get; } = new();

    /// <summary>Przypadki użycia, których operacje woła kontroler (kolejność pierwszego użycia).</summary>
    public IEnumerable<UseCaseClass> TargetUseCases =>
        Operations.Select(o => o.TargetUseCase).OfType<UseCaseClass>().Distinct();
}

public class COperation
{
    public string Label { get; set; } = "";
    public UCOperation? Target { get; set; }
    public UseCaseClass? TargetUseCase { get; set; }
}
