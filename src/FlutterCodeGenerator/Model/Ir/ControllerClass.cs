namespace FlutterCodeGenerator.Model.Ir;

public class ControllerClass
{
    public string Label { get; set; } = "";
    public List<COperation> Operations { get; } = new();

    /// <summary>Przypadki użycia, których operacje woła kontroler (kolejność pierwszego użycia).</summary>
    public IEnumerable<UseCaseClass> TargetUseCases =>
        Operations.SelectMany(o => new[] { o.TargetUseCase, o.ReturnUseCase }).OfType<UseCaseClass>().Distinct();
}

public class COperation
{
    public string Label { get; set; } = "";
    public UCOperation? Target { get; set; }
    public UseCaseClass? TargetUseCase { get; set; }
    /// <summary>Dla przycisku z &lt;invoke&gt;: operacja wywołująca, która wznowi działanie po zakończeniu wywołanego przypadku użycia.</summary>
    public UCOperation? ReturnOperation { get; set; }
    public UseCaseClass? ReturnUseCase { get; set; }
}
