namespace FlutterCodeGenerator.Model.Rsl;

public class Scenario
{
    public string? RepLabel { get; set; }
    public string? RepAltLabel { get; set; }
    /// <summary>Warunki wstępne (tylko w Main scenario).</summary>
    public List<UcCondition> Preconditions { get; set; } = new();
    public List<Sentence> AltSentences { get; set; } = new();
    public EndSentence EndSentence { get; set; } = null!;
}
