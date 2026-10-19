using System.Text;
using FlutterCodeGenerator.Model.Ir;

namespace FlutterCodeGenerator.Generator;

/// <summary>
/// Generuje klasę przypadku użycia z instrukcjami IR jako ciałami metod.
/// Bieżące dane przypadku użycia trzymane są w polach <c>_currentX</c>, wyniki sprawdzeń w <c>_xCheckResult</c>.
/// </summary>
internal class UseCaseWriter
{
    private readonly UseCaseClass _uc;
    private readonly IReadOnlyDictionary<string, DataAggregate> _aggregates;
    private readonly List<string> _warnings;

    // Pola tworzone tylko wtedy, gdy są czytane – inaczej analizator zgłasza unused_field.
    private readonly SortedSet<string> _dataFields = new(StringComparer.Ordinal);
    private readonly List<ServiceInterface> _checkFields = new();
    private bool _usesHideViews;
    private bool _usesEnd;

    public UseCaseWriter(UseCaseClass uc, IReadOnlyDictionary<string, DataAggregate> aggregates, List<string> warnings)
    {
        _uc = uc;
        _aggregates = aggregates;
        _warnings = warnings;
        foreach (var op in uc.Operations)
            Analyze(op.Instructions);
    }

    public static string DataField(string aggregateLabel) => "_current" + aggregateLabel;
    public static string CheckField(ServiceInterface si) => "_" + Naming.Camel(si.NotionName) + "CheckResult";
    public static string CheckEnum(ServiceInterface si) => Naming.Pascal(si.NotionName) + "CheckResult";
    public static string ServiceField(ServiceInterface si) => "_" + Naming.Camel(si.NotionName);
    public static string PresenterField(PresenterClass p) => "_" + Naming.LowerFirst(p.Label);

    /// <summary>Typy modeli i enumów, których używa klasa przypadku użycia.</summary>
    public IEnumerable<string> UsedModelTypes =>
        _dataFields
            .Concat(_uc.Operations.SelectMany(o => o.InputData).Select(d => d.Label))
            .Concat(_checkFields.Select(CheckEnum))
            .Distinct().OrderBy(t => t, StringComparer.Ordinal);

    public string Fields()
    {
        var lines = _dataFields.Select(d => $"  {d}? {DataField(d)};")
            .Concat(_checkFields.Select(si => $"  {CheckEnum(si)}? {CheckField(si)};"));
        return string.Join("\n", lines);
    }

    public string Methods()
    {
        var sb = new StringBuilder();
        // Dane z warunku kontekstowego przekazuje przypadek wywołujący (D8.4).
        foreach (var label in _uc.ContextData.Select(d => d.Label).Where(_dataFields.Contains))
            sb.Append($"  void set{label}({label} value) {{\n    {DataField(label)} = value;\n  }}\n\n");

        foreach (var op in _uc.Operations)
            sb.Append(Method(op)).Append('\n');

        if (_usesHideViews)
        {
            sb.Append("  void _hideViews() {\n");
            foreach (var p in _uc.Presenters)
                sb.Append($"    {PresenterField(p)}.hide();\n");
            sb.Append("  }\n\n");
        }
        if (_usesEnd)
            sb.Append("  void _end(String result) {\n    _hideViews();\n  }\n\n");
        return sb.ToString().TrimEnd('\n');
    }

    public static bool IsAsync(IEnumerable<Instruction> instructions) => instructions.Any(i => i switch
    {
        Call c => c.Service != null,
        Decision d => d.Conditions.Any(c => IsAsync(c.Instructions)),
        _ => false
    });

    public static string Signature(UCOperation op) =>
        (IsAsync(op.Instructions) ? "Future<void> " : "void ") + op.Label + "(" +
        string.Join(", ", op.InputData.Select(d => $"{d.Label} {Naming.LowerFirst(d.Label)}")) + ")" +
        (IsAsync(op.Instructions) ? " async" : "");

    private string Method(UCOperation op)
    {
        var body = new StringBuilder();
        foreach (var d in op.InputData.Where(d => _dataFields.Contains(d.Label)))
            body.Append($"    {DataField(d.Label)} = {Naming.LowerFirst(d.Label)};\n");
        EmitInstructions(op.Instructions, body, "    ");
        return body.Length == 0
            ? $"  {Signature(op)} {{}}\n"
            : $"  {Signature(op)} {{\n{body}  }}\n";
    }

    private void Analyze(IEnumerable<Instruction> instructions)
    {
        foreach (var instr in instructions)
        {
            switch (instr)
            {
                case Call c when c.Presenter != null:
                    _usesHideViews = true;
                    foreach (var p in c.Parameters) _dataFields.Add(p.Type);
                    break;
                case Call c when c.Service != null:
                    foreach (var p in c.Parameters) _dataFields.Add(p.Type);
                    break;
                case Decision d:
                    foreach (var cond in d.Conditions)
                    {
                        foreach (var e in cond.Expressions)
                        {
                            var si = CheckedService(e.Notion);
                            if (si != null && !_checkFields.Contains(si)) _checkFields.Add(si);
                            else if (si == null && e.Val == "" && _aggregates.ContainsKey(Naming.Pascal(e.Notion)))
                                _dataFields.Add(Naming.Pascal(e.Notion));
                        }
                        Analyze(cond.Instructions);
                    }
                    break;
                case End:
                    _usesEnd = true;
                    _usesHideViews = true;
                    break;
            }
        }
    }

    /// <summary>Usługa, której operacja &lt;check&gt; sprawdza pojęcie warunku w tym przypadku użycia.</summary>
    private ServiceInterface? CheckedService(string notion) =>
        _uc.Services.FirstOrDefault(s => Naming.Pascal(s.NotionName) == Naming.Pascal(notion) &&
                                         s.Methods.Any(m => m.Type == PredicateType.Check));

    private void EmitInstructions(List<Instruction> instructions, StringBuilder sb, string indent)
    {
        for (var i = 0; i < instructions.Count; i++)
        {
            var isLast = i == instructions.Count - 1;
            switch (instructions[i])
            {
                case Call c when c.Presenter != null:
                    var presenterArgs = string.Join(", ", c.Parameters.Select(p => $"{Naming.LowerFirst(p.Type)}: {DataField(p.Type)}"));
                    sb.Append($"{indent}_hideViews();\n{indent}{PresenterField(c.Presenter)}.{c.OperationRef}({presenterArgs});\n");
                    break;
                case Call c when c.Service != null:
                    sb.Append(indent).Append(ServiceCall(c)).Append('\n');
                    break;
                case Call c when c.TargetUseCase != null:
                    _warnings.Add($"{_uc.Label}: systemowe <invoke> {c.TargetUseCase.Label} nie jest jeszcze generowane.");
                    break;
                case Decision d:
                    EmitDecision(d, sb, indent);
                    break;
                case End e:
                    sb.Append($"{indent}_end('{e.ReturnValue?.Label}');\n");
                    if (!isLast) sb.Append($"{indent}return;\n");
                    break;
            }
        }
    }

    private string ServiceCall(Call c)
    {
        var si = c.Service!;
        var args = string.Join(", ", c.Parameters.Select(p => DataField(p.Type) + "!"));
        var call = $"await {ServiceField(si)}.{c.OperationRef}({args});";
        return c.ServiceOperation!.Type switch
        {
            PredicateType.Check when _checkFields.Contains(si) => $"{CheckField(si)} = {call}",
            PredicateType.Read when _dataFields.Contains(Naming.Pascal(si.NotionName)) => $"{DataField(Naming.Pascal(si.NotionName))} = {call}",
            _ => call
        };
    }

    private void EmitDecision(Decision d, StringBuilder sb, string indent)
    {
        for (var i = 0; i < d.Conditions.Count; i++)
        {
            var cond = d.Conditions[i];
            var test = cond.Expressions.Count == 0 ? "true" : string.Join(" && ", cond.Expressions.Select(Test));
            sb.Append(indent).Append(i == 0 ? "if (" : "} else if (").Append(test).Append(") {\n");
            EmitInstructions(cond.Instructions, sb, indent + "  ");
        }
        if (d.Conditions.Count > 0)
            sb.Append(indent).Append("}\n");
    }

    private string Test(Expression e)
    {
        var si = CheckedService(e.Notion);
        if (si != null)
            return $"{CheckField(si)} {(e.Negated ? "!=" : "==")} {CheckEnum(si)}.{Naming.EnumMember(e.Val)}";
        if (e.Val == "" && _dataFields.Contains(Naming.Pascal(e.Notion)))
            return $"{DataField(Naming.Pascal(e.Notion))} {(e.Negated ? "==" : "!=")} null";
        _warnings.Add($"{_uc.Label}: warunek [{e.Notion} ? {e.Val}] nie dotyczy pojęcia sprawdzanego zdaniem <check>; przyjęto true.");
        return "true";
    }
}
