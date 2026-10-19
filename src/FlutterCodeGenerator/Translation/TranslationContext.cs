using FlutterCodeGenerator.Model.Ir;

namespace FlutterCodeGenerator.Translation;

public class TranslationContext
{
    public ViewWidget? CurrentVF { get; set; }
    public UseCaseClass? CurrentUCC { get; set; }
    public List<DataAggregate> CurrentDAP { get; } = new();
    public List<DataAggregate> CurrentDAD { get; } = new();
    /// <summary>Agregaty z warunków kontekstowych przypadku użycia; dostępne w całym przypadku użycia.</summary>
    public List<DataAggregate> ContextDA { get; } = new();
    public string CurrentLabel { get; set; } = "";
    public UCOperation? CurrentUCO { get; set; }
    public Condition? CurrentCondition { get; set; }
    public Dictionary<string, ViewWidget> LabelToVF { get; } = new();
    /// <summary>Migawki stanu po zdaniach z etykietą; etykiety są lokalne dla przypadku użycia.</summary>
    public Dictionary<string, LabelSnapshot> LabelSnapshots { get; } = new();
    public string CurrentUseCaseName { get; set; } = "";
    public List<(string UcName, Action Attach)> ScheduledInvocations { get; } = new();
    public List<string> Warnings { get; } = new();
    public List<string> Errors { get; } = new();
}
