using FlutterCodeGenerator.Model.Ir;

namespace FlutterCodeGenerator.Translation;

public class TranslationContext
{
    public ViewWidget? CurrentVF { get; set; }
    public UseCaseClass? CurrentUCC { get; set; }
    public List<DataAggregate> CurrentDAP { get; } = new();
    public List<DataAggregate> CurrentDAD { get; } = new();
    public string CurrentLabel { get; set; } = "";
    public UCOperation? CurrentUCO { get; set; }
    public Condition? CurrentCondition { get; set; }
    public Dictionary<string, ViewWidget> LabelToVF { get; } = new();
    public List<(string UcName, Action Attach)> ScheduledInvocations { get; } = new();
}
