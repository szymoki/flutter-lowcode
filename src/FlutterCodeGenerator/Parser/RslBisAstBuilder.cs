using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;
using FlutterCodeGenerator.Model.Rsl;

namespace FlutterCodeGenerator.Parser;

/// <summary>
/// Visitor budujący obiekty modelu RSL z AST ANTLR.
/// </summary>
public class RslBisAstBuilder : RslBisBaseVisitor<object?>
{
    private readonly List<UseCase> _useCases = new();
    private readonly List<NotionDef> _notions = new();

    public IReadOnlyList<UseCase> UseCases => _useCases;
    public IReadOnlyList<NotionDef> Notions => _notions;

    private static string GetMultiWordText(ParserRuleContext ctx)
    {
        if (ctx == null) return "";
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < ctx.ChildCount; i++)
        {
            var c = ctx.GetChild(i);
            if (c is ITerminalNode tn)
                sb.Append(tn.GetText()).Append(" ");
        }
        return sb.ToString().Trim();
    }

    public override object? VisitStart(RslBisParser.StartContext context)
    {
        if (context?.usecase() != null)
        {
            foreach (var uc in context.usecase())
                Visit(uc);
        }
        if (context?.notiondef() != null)
        {
            foreach (var nd in context.notiondef())
                Visit(nd);
        }
        return null;
    }

    public override object? VisitUsecase(RslBisParser.UsecaseContext context)
    {
        var nameCtx = context.name();
        var name = GetMultiWordText(nameCtx);
        var mainScenario = (Scenario)Visit(context.mainscenario())!;
        var scenarios = context.scenarios() != null
            ? (List<Scenario>)Visit(context.scenarios())!
            : new List<Scenario>();

        var uc = new UseCase
        {
            Name = name,
            MainScenario = mainScenario,
            Scenarios = scenarios
        };
        _useCases.Add(uc);
        return uc;
    }

    public override object? VisitMainscenario(RslBisParser.MainscenarioContext context)
    {
        List<UcCondition>? conditions = null;
        if (context.ucconditions() != null)
            conditions = (List<UcCondition>)Visit(context.ucconditions())!;
        var sentences = (List<Sentence>)Visit(context.sentences())!;
        var endSentence = (EndSentence)Visit(context.endsentence())!;

        return new Scenario
        {
            AltSentences = sentences,
            EndSentence = endSentence
        };
    }

    public override object? VisitUcconditions(RslBisParser.UcconditionsContext context)
    {
        return Visit(context.conditions());
    }

    public override object? VisitConditions(RslBisParser.ConditionsContext context)
    {
        var list = new List<UcCondition>();
        var c = context;
        while (c != null)
        {
            var cond = (UcCondition)Visit(c.condition())!;
            list.Add(cond);
            if (c.conditions()?.condition() != null)
                c = c.conditions();
            else
                break;
        }
        return list;
    }

    public override object? VisitCondition(RslBisParser.ConditionContext context)
    {
        if (context.contextcondition() != null)
            return Visit(context.contextcondition());
        return Visit(context.valuecondition());
    }

    public override object? VisitContextcondition(RslBisParser.ContextconditionContext context)
    {
        var notion = GetMultiWordText(context.notion());
        return new ContextCondition { Notion = notion };
    }

    public override object? VisitValuecondition(RslBisParser.ValueconditionContext context)
    {
        var notion = context.notion() != null ? GetMultiWordText(context.notion()) : "user";
        var value = GetMultiWordText(context.value());
        var negated = context.GetText().Contains("'-'");
        return new ValueCondition { Notion = notion, Value = value, Negated = negated };
    }

    public override object? VisitScenarios(RslBisParser.ScenariosContext context)
    {
        var list = new List<Scenario>();
        var c = context;
        while (c != null)
        {
            list.Add((Scenario)Visit(c.scenario())!);
            if (c.scenarios()?.scenario() != null)
                c = c.scenarios();
            else
                break;
        }
        return list;
    }

    public override object? VisitScenario(RslBisParser.ScenarioContext context)
    {
        var rep = context.repsentence();
        string? repLabel = null;
        string? repAltLabel = null;
        if (rep?.label() != null)
            repLabel = rep.label()?.NUMBER()?.GetText();
        if (rep?.altlabel() != null)
            repAltLabel = rep.altlabel()?.CHAR()?.GetText() + rep.altlabel()?.NUMBER()?.GetText();

        var altSentences = (List<Sentence>)Visit(context.altsentences())!;
        var endSentence = (EndSentence)Visit(context.endsentence())!;

        return new Scenario
        {
            RepLabel = repLabel,
            RepAltLabel = repAltLabel,
            AltSentences = altSentences,
            EndSentence = endSentence
        };
    }

    public override object? VisitSentences(RslBisParser.SentencesContext context)
    {
        var list = new List<Sentence>();
        var c = context;
        while (c != null)
        {
            list.Add((Sentence)Visit(c.sentence())!);
            if (c.sentences()?.sentence() != null)
                c = c.sentences();
            else
                break;
        }
        return list;
    }

    public override object? VisitAltsentences(RslBisParser.AltsentencesContext context)
    {
        var list = new List<Sentence>();
        var c = context;
        while (c != null)
        {
            list.Add((Sentence)Visit(c.altsentence())!);
            if (c.altsentences()?.altsentence() != null)
                c = c.altsentences();
            else
                break;
        }
        return list;
    }

    public override object? VisitSentence(RslBisParser.SentenceContext context)
    {
        if (context.svosentence() != null)
            return Visit(context.svosentence());
        return Visit(context.condsentence());
    }

    public override object? VisitAltsentence(RslBisParser.AltsentenceContext context)
    {
        if (context.altsvosentence() != null)
            return Visit(context.altsvosentence());
        return Visit(context.condsentence());
    }

    public override object? VisitSvosentence(RslBisParser.SvosentenceContext context)
    {
        var label = context.label()?.NUMBER()?.GetText() ?? "";
        var step = (Step)Visit(context.step())!;
        return new SvoSentence { Label = label, Step = step };
    }

    public override object? VisitAltsvosentence(RslBisParser.AltsvosentenceContext context)
    {
        var altLabel = context.altlabel() != null
            ? context.altlabel()!.CHAR()?.GetText() + context.altlabel()!.NUMBER()?.GetText()
            : null;
        var step = (Step)Visit(context.step())!;
        return new SvoSentence { AltLabel = altLabel, Step = step };
    }

    public override object? VisitCondsentence(RslBisParser.CondsentenceContext context)
    {
        var conditions = (List<UcCondition>)Visit(context.conditions())!;
        return new CondSentence { Conditions = conditions };
    }

    public override object? VisitStep(RslBisParser.StepContext context)
    {
        if (context.userstep() != null)
            return Visit(context.userstep());
        return Visit(context.systemstep());
    }

    public override object? VisitUserstep(RslBisParser.UserstepContext context)
    {
        var actor = context.actor()?.GetText() ?? "";
        UserPredicate predicate;
        if (context.selectpredicate() != null)
            predicate = (SelectPredicate)Visit(context.selectpredicate())!;
        else if (context.enterpredicate() != null)
            predicate = (EnterPredicate)Visit(context.enterpredicate())!;
        else
            predicate = (InvokePredicate)Visit(context.invoke())!;
        return new UserStep { Actor = actor, Predicate = predicate };
    }

    public override object? VisitSystemstep(RslBisParser.SystemstepContext context)
    {
        SystemPredicate pred;
        if (context.tosystempredicate() != null)
            pred = (SystemPredicate)Visit(context.tosystempredicate())!;
        else if (context.toactorpredicate() != null)
            pred = (SystemPredicate)Visit(context.toactorpredicate())!;
        else
            pred = (SysInvokePredicate)Visit(context.sysinvoke())!;
        return new SystemStep { Predicate = pred };
    }

    public override object? VisitSelectpredicate(RslBisParser.SelectpredicateContext context)
    {
        var notion = GetMultiWordText(context.notion());
        return new SelectPredicate { Notion = notion };
    }

    public override object? VisitEnterpredicate(RslBisParser.EnterpredicateContext context)
    {
        var notion = GetMultiWordText(context.notion());
        return new EnterPredicate { Notion = notion };
    }

    public override object? VisitInvoke(RslBisParser.InvokeContext context)
    {
        var names = (List<string>)Visit(context.names())!;
        return new InvokePredicate { Names = names };
    }

    public override object? VisitShowpredicate(RslBisParser.ShowpredicateContext context)
    {
        var notion = GetMultiWordText(context.notion());
        return new ShowPredicate { Notion = notion };
    }

    public override object? VisitClosepredicate(RslBisParser.ClosepredicateContext context)
    {
        var notion = GetMultiWordText(context.notion());
        return new ClosePredicate { Notion = notion };
    }

    public override object? VisitReadpredicate(RslBisParser.ReadpredicateContext context)
    {
        var notion = GetMultiWordText(context.notion());
        return new ReadPredicate { Notion = notion };
    }

    public override object? VisitUpdatepredicate(RslBisParser.UpdatepredicateContext context)
    {
        var notion = GetMultiWordText(context.notion());
        return new UpdatePredicate { Notion = notion };
    }

    public override object? VisitDeletepredicate(RslBisParser.DeletepredicateContext context)
    {
        var notion = GetMultiWordText(context.notion());
        return new DeletePredicate { Notion = notion };
    }

    public override object? VisitCheckpredicate(RslBisParser.CheckpredicateContext context)
    {
        var notion = GetMultiWordText(context.notion());
        return new CheckPredicate { Notion = notion };
    }

    public override object? VisitExecutepredicate(RslBisParser.ExecutepredicateContext context)
    {
        var notion = GetMultiWordText(context.notion());
        return new ExecutePredicate { Notion = notion };
    }

    public override object? VisitSysinvoke(RslBisParser.SysinvokeContext context)
    {
        var name = GetMultiWordText(context.name());
        return new SysInvokePredicate { Name = name };
    }

    public override object? VisitNames(RslBisParser.NamesContext context)
    {
        var list = new List<string>();
        var c = context;
        while (c != null)
        {
            list.Add(GetMultiWordText(c.name()));
            if (c.names()?.name() != null)
                c = c.names();
            else
                break;
        }
        return list;
    }

    public override object? VisitEndsentence(RslBisParser.EndsentenceContext context)
    {
        if (context.resultsentence() != null)
        {
            var value = GetMultiWordText(context.resultsentence().value());
            return new EndSentence { IsEnd = true, ReturnValue = value };
        }
        var rejoinLabel = context.rejoinsentence()?.labelref()?.GetText() ?? "";
        return new EndSentence { IsRejoin = true, RejoinLabel = rejoinLabel };
    }

    public override object? VisitNotiondef(RslBisParser.NotiondefContext context)
    {
        NotionDef? nd;
        if (context.viewnotion() != null)
            nd = (ViewNotion)Visit(context.viewnotion())!;
        else if (context.triggernotion() != null)
            nd = (TriggerNotion)Visit(context.triggernotion())!;
        else
            nd = (DataNotion)Visit(context.datanotion())!;
        _notions.Add(nd);
        return nd;
    }

    public override object? VisitViewnotion(RslBisParser.ViewnotionContext context)
    {
        var viewType = context.viewtype()?.GetText() ?? "";
        var namesAndLabels = context.namesandlabels() != null
            ? (List<NameAndLabel>)Visit(context.namesandlabels())!
            : new List<NameAndLabel>();
        return new ViewNotion { ViewType = viewType, NamesAndLabels = namesAndLabels };
    }

    public override object? VisitTriggernotion(RslBisParser.TriggernotionContext context)
    {
        var triggerType = context.triggertype()?.GetText();
        var namesAndLabels = context.namesandlabels() != null
            ? (List<NameAndLabel>)Visit(context.namesandlabels())!
            : new List<NameAndLabel>();
        return new TriggerNotion { TriggerType = triggerType, NamesAndLabels = namesAndLabels };
    }

    public override object? VisitDatanotion(RslBisParser.DatanotionContext context)
    {
        var name = GetMultiWordText(context.name());
        var attributes = (List<DataAttribute>)Visit(context.attributes())!;
        return new DataNotion { Name = name, Attributes = attributes };
    }

    public override object? VisitNamesandlabels(RslBisParser.NamesandlabelsContext context)
    {
        var list = new List<NameAndLabel>();
        var c = context;
        while (c != null)
        {
            var n = GetMultiWordText(c.name());
            var label = c.uilabel() != null ? GetMultiWordText(c.uilabel()) : null;
            list.Add(new NameAndLabel { Name = n, UiLabel = label });
            if (c.namesandlabels()?.name() != null)
                c = c.namesandlabels();
            else
                break;
        }
        return list;
    }

    public override object? VisitAttributes(RslBisParser.AttributesContext context)
    {
        var list = new List<DataAttribute>();
        var c = context;
        while (c != null)
        {
            list.Add((DataAttribute)Visit(c.attribute())!);
            if (c.attributes()?.attribute() != null)
                c = c.attributes();
            else
                break;
        }
        return list;
    }

    public override object? VisitAttribute(RslBisParser.AttributeContext context)
    {
        var name = GetMultiWordText(context.name());
        string typeKind;
        bool isMult = false, isRef = false;
        string? notionRef = null;
        if (context.datatype() != null)
            typeKind = context.datatype().GetText();
        else if (context.refnotion() != null)
        {
            typeKind = "ref";
            isRef = true;
            notionRef = GetMultiWordText(context.refnotion().notion());
        }
        else if (context.multnotion() != null)
        {
            typeKind = GetMultiWordText(context.multnotion().notion());
            isMult = true;
        }
        else
            typeKind = GetMultiWordText(context.notion());
        return new DataAttribute
        {
            Name = name,
            TypeKind = typeKind,
            IsMult = isMult,
            IsRef = isRef,
            NotionRef = notionRef
        };
    }
}
