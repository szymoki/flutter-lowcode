using FlutterCodeGenerator.Model.Ir;
using FlutterCodeGenerator.Model.Rsl;

namespace FlutterCodeGenerator.Translation;

public class RslToIrTranslator
{
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

    private static UCOperation? FindOperationByLabel(string label, TranslationContext ctx)
    {
        if (ctx.CurrentUCC == null) return null;
        foreach (var op in ctx.CurrentUCC.Operations)
        {
            if (op.Label == label) return op;
            foreach (var instr in op.Instructions.OfType<Decision>())
                foreach (var cond in instr.Conditions)
                    foreach (var i in cond.Instructions)
                        if (i is Call call && call.Label == label)
                            return op;
        }
        return null;
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

        return ir;
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
        ctx.ContextDA.Clear();

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
        ctx.CurrentLabel = scenario.RepLabel ?? scenario.RepAltLabel ?? "";
        foreach (var sent in scenario.AltSentences)
            ProcessSentence(sent, ir, ctx);
        ProcessEndSentence(scenario.EndSentence, ir, ctx);
    }

    private void ProcessSentence(Sentence sent, IrModel ir, TranslationContext ctx)
    {
        if (sent is SvoSentence svo)
        {
            ctx.CurrentLabel = svo.Label ?? svo.AltLabel ?? "";
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

    private void ProcessShowSentence(string notion, IrModel ir, TranslationContext ctx)
    {
        var label = ToPascalCase(notion);
        var vw = ir.Views.FirstOrDefault(v => v.Label == label);
        if (vw == null)
        {
            vw = new ViewWidget { Label = label };
            var ctrl = new ControllerClass { Label = "C" + label, UseCase = ctx.CurrentUCC };
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
        var trigger = new Trigger { Label = ToCamelCase(notion) };
        ctx.CurrentVF?.Triggers.Add(trigger);

        var cop = new COperation { Label = "on" + ToPascalCase(notion) };
        ctx.CurrentVF?.Controller?.Operations.Add(cop);

        var ucOp = new UCOperation { Label = "handle" + ToPascalCase(notion), IsInitial = isInitial };
        ctx.CurrentUCC?.Operations.Add(ucOp);
        cop.InvokedOperation = ucOp.Label;
        ctx.CurrentUCO = ucOp;

        if (isInitial)
            ProcessScheduledInvocations(ir, ctx);
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
        ctx.CurrentCondition = null;

        if (end.IsRejoin && !string.IsNullOrEmpty(end.RejoinLabel))
            ProcessRejoinSentence(end.RejoinLabel, ctx);
    }

    private void ProcessRejoinSentence(string label, TranslationContext ctx)
    {
        var targetOp = FindOperationByLabel(label, ctx);
        if (targetOp != null)
        {
            foreach (var instr in targetOp.Instructions)
                AddInstruction(CloneInstruction(instr), ctx);
        }
    }

    private void ProcessScheduledInvocations(IrModel ir, TranslationContext ctx)
    {
        foreach (var (ucName, attach) in ctx.ScheduledInvocations)
            attach();
        ctx.ScheduledInvocations.Clear();
    }
}
