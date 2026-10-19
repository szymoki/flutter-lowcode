using FlutterCodeGenerator.Model.Ir;
using FlutterCodeGenerator.Model.Rsl;

namespace FlutterCodeGenerator.Translation;

public class RslToIrTranslator
{
    public List<string> Warnings { get; } = new();
    public List<string> Errors { get; } = new();

    private static string ToPascalCase(string s) => Naming.Pascal(s);

    private static string ToCamelCase(string s) => Naming.Camel(s);

    private static DataAggregate GetOrCreateDataAggregate(string notion, IrModel ir)
    {
        var label = ToPascalCase(notion);
        var da = ir.ViewModel.Aggregates.FirstOrDefault(a => a.Label == label);
        if (da == null)
        {
            da = new DataAggregate { Label = label, NotionName = notion.Trim() };
            ir.ViewModel.Aggregates.Add(da);
        }
        return da;
    }

    private static ServiceInterface GetOrCreateServiceInterface(string notion, IrModel ir)
    {
        var label = "I" + ToPascalCase(notion);
        var si = ir.Services.FirstOrDefault(s => s.Label == label);
        if (si == null)
        {
            si = new ServiceInterface { Label = label, NotionName = notion.Trim() };
            ir.Services.Add(si);
        }
        return si;
    }

    private static void AddInstruction(Instruction instr, TranslationContext ctx)
    {
        if (ctx.CurrentCondition != null)
            ctx.CurrentCondition.Instructions.Add(instr);
        else if (ctx.CurrentUCO?.Instructions != null)
            ctx.CurrentUCO.Instructions.Add(instr);
    }

    private static Instruction CloneInstruction(Instruction instr)
    {
        if (instr is Call c)
        {
            var clone = new Call { Label = c.Label, OperationRef = c.OperationRef };
            clone.Parameters.AddRange(c.Parameters.Select(p => new Parameter { Label = p.Label, IsAttribute = p.IsAttribute, Type = p.Type }));
            return clone;
        }
        if (instr is End e)
            return new End { Label = e.Label, ReturnValue = e.ReturnValue != null ? new Value { Label = e.ReturnValue.Label } : null };
        if (instr is Decision d)
        {
            var clone = new Decision { Label = d.Label };
            foreach (var cond in d.Conditions)
            {
                var cc = new Condition { Label = cond.Label, Value = cond.Value != null ? new Value { Label = cond.Value.Label } : null };
                cc.Expressions.AddRange(cond.Expressions.Select(x => new Expression { Notion = x.Notion, Val = x.Val, Negated = x.Negated }));
                foreach (var i in cond.Instructions)
                    cc.Instructions.Add(CloneInstruction(i));
                clone.Conditions.Add(cc);
            }
            return clone;
        }
        return new Call { Label = instr.Label };
    }

    public IrModel Translate(List<UseCase> useCases, List<NotionDef> notions)
    {
        var ir = new IrModel();
        foreach (var nd in notions.OfType<DataNotion>())
        {
            var da = GetOrCreateDataAggregate(nd.Name, ir);
            foreach (var attr in nd.Attributes)
            {
                var item = new DataItem
                {
                    Label = ToPascalCase(attr.Name),
                    TypeKind = attr.IsRef && attr.NotionRef != null ? ToPascalCase(attr.NotionRef) : attr.TypeKind
                };
                da.Items.Add(item);
            }
        }

        var ctx = new TranslationContext();
        foreach (var uc in useCases)
        {
            ProcessUseCaseClause(uc, ir, ctx);
            ProcessMainScenario(uc, ir, ctx);
            foreach (var scenario in uc.Scenarios)
                ProcessScenario(scenario, ir, ctx);
        }

        foreach (var ucc in ir.UseCases)
            MakeOperationLabelsUnique(ucc);
        Warnings.AddRange(ctx.Warnings);
        Errors.AddRange(ctx.Errors);
        return ir;
    }

    /// <summary>
    /// Ten sam wyzwalacz w różnych widokach jednego przypadku użycia (np. „close” w OrderWindow
    /// i w SearchErrorMessage) daje różne operacje o tej samej nazwie. Dart nie ma przeciążania,
    /// więc kolidujące operacje dostają w nazwie widok źródłowy (D3.2).
    /// </summary>
    private static void MakeOperationLabelsUnique(UseCaseClass ucc)
    {
        foreach (var group in ucc.Operations.GroupBy(o => o.Label).Where(g => g.Count() > 1).ToList())
        {
            var index = 1;
            foreach (var op in group)
            {
                var suffix = op.SourceView?.Label ?? index.ToString();
                op.Label += suffix;
                index++;
            }
        }
    }

    private void ProcessUseCaseClause(UseCase uc, IrModel ir, TranslationContext ctx)
    {
        var ucc = new UseCaseClass
        {
            Label = ToPascalCase(uc.Name),
            IsInitial = string.Equals(uc.Name.Trim(), "Start", StringComparison.OrdinalIgnoreCase)
        };
        ir.UseCases.Add(ucc);
        ctx.CurrentUCC = ucc;
        ctx.CurrentUseCaseName = uc.Name;
        ctx.ContextDA.Clear();
        ctx.LabelSnapshots.Clear();
        ctx.CurrentVF = null;
        ctx.CurrentCondition = null;
        ctx.CurrentDAP.Clear();
        ctx.CurrentDAD.Clear();

        var initOp = new UCOperation { Label = "execute", IsInitial = true };
        ucc.Operations.Add(initOp);
        ctx.CurrentUCO = initOp;

        if (uc.MainScenario.Preconditions.Count > 0)
            ProcessPrecondition(uc.MainScenario.Preconditions, ir, ctx);
    }

    private void ProcessPrecondition(List<UcCondition> conds, IrModel ir, TranslationContext ctx)
    {
        foreach (var cond in conds)
        {
            if (cond is ContextCondition cc)
            {
                var da = GetOrCreateDataAggregate(cc.Notion, ir);
                if (!ctx.ContextDA.Contains(da))
                    ctx.ContextDA.Add(da);
            }
            // Warunek na podmiocie „user” określa rolę aktora, a nie dane – nie tworzy usługi IUser.
            else if (cond is ValueCondition { Notion: "user" })
                continue;
            else if (cond is ValueCondition vc)
            {
                var si = GetOrCreateServiceInterface(vc.Notion, ir);
                var sop = new SOperation { Label = "validate" + ToPascalCase(vc.Notion), Type = PredicateType.Check };
                si.Methods.Add(sop);
            }
        }
    }

    private void ProcessMainScenario(UseCase uc, IrModel ir, TranslationContext ctx)
    {
        foreach (var sent in uc.MainScenario.AltSentences)
            ProcessSentence(sent, ir, ctx);
        ProcessEndSentence(uc.MainScenario.EndSentence, ir, ctx);
    }

    private void ProcessScenario(Scenario scenario, IrModel ir, TranslationContext ctx)
    {
        var repLabel = scenario.RepLabel ?? scenario.RepAltLabel ?? "";
        if (!ctx.LabelSnapshots.TryGetValue(repLabel, out var snapshot))
        {
            ctx.Errors.Add($"Przypadek użycia „{ctx.CurrentUseCaseName}”: scenariusz alternatywny powtarza etykietę {repLabel}, " +
                           "której nie ma wcześniej w tym przypadku użycia.");
            return;
        }
        ctx.CurrentVF = snapshot.CurrentVF;
        ctx.CurrentUCO = snapshot.CurrentUCO;
        ctx.CurrentCondition = snapshot.CurrentCondition;
        ctx.CurrentDAP.Clear();
        ctx.CurrentDAP.AddRange(snapshot.CurrentDAP);
        ctx.CurrentDAD.Clear();
        ctx.CurrentDAD.AddRange(snapshot.CurrentDAD);
        ctx.CurrentLabel = repLabel;

        foreach (var sent in scenario.AltSentences)
            ProcessSentence(sent, ir, ctx);
        ProcessEndSentence(scenario.EndSentence, ir, ctx);
    }

    private void ProcessSentence(Sentence sent, IrModel ir, TranslationContext ctx)
    {
        if (sent is SvoSentence svo)
        {
            ctx.CurrentLabel = !string.IsNullOrEmpty(svo.Label) ? svo.Label : svo.AltLabel ?? "";
            var positionList = svo.Step is SystemStep ? CurrentInstructionList(ctx) : null;
            var positionIndex = positionList?.Count ?? 0;
            ProcessSvoSentence(svo, ir, ctx);
            if (ctx.CurrentLabel != "")
                ctx.LabelSnapshots[ctx.CurrentLabel] = new LabelSnapshot
                {
                    CurrentVF = ctx.CurrentVF,
                    CurrentUCO = ctx.CurrentUCO,
                    CurrentCondition = ctx.CurrentCondition,
                    CurrentDAP = ctx.CurrentDAP.ToList(),
                    CurrentDAD = ctx.CurrentDAD.ToList(),
                    InstructionList = positionList,
                    InstructionIndex = positionIndex
                };
        }
        else if (sent is CondSentence cond)
        {
            var exprs = cond.Conditions.Select(c =>
            {
                if (c is ValueCondition vc)
                    return new Expression { Notion = vc.Notion, Val = vc.Value, Negated = vc.Negated };
                return new Expression { Notion = (c as ContextCondition)?.Notion ?? "", Val = "" };
            }).ToList();
            ProcessConditionSentence(exprs, ir, ctx);
        }
    }

    private static List<Instruction>? CurrentInstructionList(TranslationContext ctx) =>
        ctx.CurrentCondition?.Instructions ?? ctx.CurrentUCO?.Instructions;

    private void ProcessSvoSentence(SvoSentence svo, IrModel ir, TranslationContext ctx)
    {
        if (svo.Step is UserStep user)
        {
            if (user.Predicate is SelectPredicate sel)
                ProcessSelectSentence(user.Actor, sel.Notion, ctx.CurrentUCO?.IsInitial ?? false, ir, ctx);
            else if (user.Predicate is EnterPredicate ent)
                ProcessEnterSentence(ent.Notion, ir, ctx);
            else if (user.Predicate is InvokePredicate inv)
            {
                foreach (var name in inv.Names)
                    ctx.ScheduledInvocations.Add((ToPascalCase(name), () => { }));
            }
        }
        else if (svo.Step is SystemStep sys)
        {
            if (sys.Predicate is ShowPredicate sh)
                ProcessShowSentence(sh.Notion, ir, ctx);
            else if (sys.Predicate is ClosePredicate)
                { /* nawigacja wstecz – można dodać End lub nic */ }
            else if (sys.Predicate is ReadPredicate r)
                ProcessSystemDataSentence(PredicateType.Read, r.Notion, ir, ctx);
            else if (sys.Predicate is UpdatePredicate u)
                ProcessSystemDataSentence(PredicateType.Update, u.Notion, ir, ctx);
            else if (sys.Predicate is DeletePredicate d)
                ProcessSystemDataSentence(PredicateType.Delete, d.Notion, ir, ctx);
            else if (sys.Predicate is CheckPredicate ch)
                ProcessSystemDataSentence(PredicateType.Check, ch.Notion, ir, ctx);
            else if (sys.Predicate is ExecutePredicate e)
                ProcessSystemDataSentence(PredicateType.Execute, e.Notion, ir, ctx);
        }
    }

    private void ProcessShowSentence(string notion, IrModel ir, TranslationContext ctx)
    {
        var label = ToPascalCase(notion);
        var vw = ir.Views.FirstOrDefault(v => v.Label == label);
        if (vw == null)
        {
            vw = new ViewWidget { Label = label };
            var ctrl = new ControllerClass { Label = "C" + label };
            var pres = new PresenterClass { Label = "P" + label };
            vw.Controller = ctrl;
            vw.Presenter = pres;
            foreach (var da in ctx.CurrentDAP)
                vw.Data.Add(da);
            ir.Views.Add(vw);
            ir.Controllers.Add(ctrl);
            ir.Presenters.Add(pres);
            ctx.CurrentUCC?.Presenters.Add(pres);
        }
        ctx.CurrentVF = vw;

        var po = new POperation { Label = "show" + label };
        foreach (var da in ctx.CurrentDAP)
            po.Parameters.Add(new Parameter { Label = da.Label, Type = da.Label });
        vw.Presenter!.Methods.Add(po);

        var call = new Call { Label = ctx.CurrentLabel, OperationRef = po.Label };
        call.Parameters.AddRange(po.Parameters);
        AddInstruction(call, ctx);

        ctx.CurrentDAP.Clear();
        ctx.CurrentDAD.Clear();
        ctx.LabelToVF[ctx.CurrentLabel] = vw;
    }

    private void ProcessEnterSentence(string notion, IrModel ir, TranslationContext ctx)
    {
        var da = GetOrCreateDataAggregate(notion, ir);
        foreach (var i in da.Items)
            i.EditableStateItem = true;
        ctx.CurrentDAP.Add(da);
        ctx.CurrentDAD.Add(da);

        // <enter> zawsze następuje po <show> formularza, do którego dane są wprowadzane
        // (system najpierw pokazuje formularz, dopiero potem aktor go wypełnia). Widok
        // został już utworzony przez ProcessShowSentence z pustym Data, więc encję
        // trzeba dopiąć bezpośrednio do aktualnie wyświetlanego widoku, inaczej formularz
        // nigdy nie dostanie pól wejściowych.
        if (ctx.CurrentVF != null && !ctx.CurrentVF.Data.Contains(da))
            ctx.CurrentVF.Data.Add(da);
    }

    private void ProcessSelectSentence(string actor, string notion, bool isInitial, IrModel ir, TranslationContext ctx)
    {
        // Operacja należy do przypadku użycia, w którego scenariuszu stoi zdanie (D3.1),
        // a przycisk – do widoku wyświetlonego jako ostatni.
        var ucOp = new UCOperation
        {
            Label = "handle" + ToPascalCase(notion),
            IsInitial = isInitial,
            SourceView = ctx.CurrentVF,
            TriggerNotion = notion
        };
        ctx.CurrentUCC?.Operations.Add(ucOp);
        ctx.CurrentUCO = ucOp;
        // Nowa operacja zaczyna się poza gałęzią decyzji poprzedniej operacji.
        ctx.CurrentCondition = null;

        if (ctx.CurrentVF != null)
            AddTrigger(ctx.CurrentVF, notion, ucOp, ctx.CurrentUCC, ctx);

        if (isInitial)
            ProcessScheduledInvocations(ir, ctx);
    }

    /// <summary>Dodaje do widoku wyzwalacz i operację kontrolera wskazującą operację przypadku użycia.</summary>
    private static void AddTrigger(ViewWidget view, string notion, UCOperation target, UseCaseClass? targetUseCase, TranslationContext ctx)
    {
        var triggerLabel = ToCamelCase(notion);
        if (!view.Triggers.Any(t => t.Label == triggerLabel))
            view.Triggers.Add(new Trigger { Label = triggerLabel });

        var ctrl = view.Controller!;
        var copLabel = "on" + ToPascalCase(notion);
        var existing = ctrl.Operations.FirstOrDefault(o => o.Label == copLabel);
        if (existing == null)
            ctrl.Operations.Add(new COperation { Label = copLabel, Target = target, TargetUseCase = targetUseCase });
        else if (existing.Target != target)
            ctx.Warnings.Add($"Wyzwalacz „{notion}” w widoku {view.Label} wskazuje już operację {existing.TargetUseCase?.Label}.{existing.Target?.Label}; " +
                             $"pominięto drugie przypisanie do {targetUseCase?.Label}.{target.Label}.");
    }

    private void ProcessSystemDataSentence(PredicateType type, string notion, IrModel ir, TranslationContext ctx)
    {
        var si = GetOrCreateServiceInterface(notion, ir);
        // <read> zwraca obiekt pojęcia, więc jego klasa modelu musi istnieć, nawet bez definicji Data.
        if (type == PredicateType.Read)
            GetOrCreateDataAggregate(notion, ir);
        var sop = new SOperation
        {
            Label = type.ToString().ToLowerInvariant() + ToPascalCase(notion),
            Type = type
        };
        foreach (var da in ctx.CurrentDAD.Concat(ctx.ContextDA).Distinct())
            sop.Parameters.Add(new Parameter { Label = da.Label, Type = da.Label });
        si.Methods.Add(sop);

        var call = new Call { Label = ctx.CurrentLabel, OperationRef = sop.Label };
        AddInstruction(call, ctx);

        if (ctx.CurrentUCC != null && !ctx.CurrentUCC.Services.Contains(si))
            ctx.CurrentUCC.Services.Add(si);
    }

    private void ProcessConditionSentence(List<Expression> exprs, IrModel ir, TranslationContext ctx)
    {
        var decision = ctx.CurrentUCO?.Instructions.OfType<Decision>().LastOrDefault();
        if (decision == null)
        {
            decision = new Decision { Label = ctx.CurrentLabel };
            AddInstruction(decision, ctx);
        }
        var cond = new Condition();
        cond.Expressions.AddRange(exprs);
        decision.Conditions.Add(cond);
        ctx.CurrentCondition = cond;
    }

    private void ProcessEndSentence(EndSentence end, IrModel ir, TranslationContext ctx)
    {
        if (end.IsEnd && end.ReturnValue != null)
        {
            var endInstr = new End { ReturnValue = new Value { Label = end.ReturnValue } };
            AddInstruction(endInstr, ctx);
        }
        // Kopia instrukcji z rejoin trafia do gałęzi, w której stoi zdanie końcowe.
        if (end.IsRejoin && !string.IsNullOrEmpty(end.RejoinLabel))
            ProcessRejoinSentence(end.RejoinLabel, ctx);
        ctx.CurrentCondition = null;
    }

    private void ProcessRejoinSentence(string label, TranslationContext ctx)
    {
        if (!ctx.LabelSnapshots.TryGetValue(label, out var target))
        {
            ctx.Errors.Add($"Przypadek użycia „{ctx.CurrentUseCaseName}”: rejoin {label} wskazuje etykietę, której nie ma w tym przypadku użycia.");
            return;
        }
        if (ctx.CurrentUCO != null)
            ctx.CurrentUCO.ReturnTo = label;

        var current = CurrentInstructionList(ctx);
        // Powrót do wcześniejszego miejsca tej samej listy instrukcji to pętla – kopia
        // zdublowałaby wywołania (D4.2). Zdanie aktora nie ma instrukcji do skopiowania.
        if (target.InstructionList == null || ReferenceEquals(target.InstructionList, current))
            return;

        var toCopy = target.InstructionList.Skip(target.InstructionIndex).ToList();
        foreach (var instr in toCopy)
            AddInstruction(CloneInstruction(instr), ctx);
    }

    private void ProcessScheduledInvocations(IrModel ir, TranslationContext ctx)
    {
        foreach (var (ucName, attach) in ctx.ScheduledInvocations)
            attach();
        ctx.ScheduledInvocations.Clear();
    }
}
