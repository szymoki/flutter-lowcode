namespace FlutterCodeGenerator.Model.Rsl;

public class UseCase
{
    public string Name { get; set; } = "";
    public Scenario MainScenario { get; set; } = null!;
    public List<Scenario> Scenarios { get; set; } = new();
}
