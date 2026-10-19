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
            var clone = new Call
            {
                Label = c.Label, OperationRef = c.OperationRef, TargetUseCase = c.TargetUseCase, TargetOperation = c.TargetOperation,
                Presenter = c.Presenter, Service = c.Service, ServiceOperation = c.ServiceOperation
            };
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
                    TypeKind = attr.IsRef && attr.NotionRef != null ? ToPascalCase(attr.NotionRef) : attr.TypeKind,
                    IsList = attr.IsMult
                };
                da.Items.Add(item);
            }
        }

        var ctx = new TranslationContext();
        // Pierwszy przebieg: klasy i operacje wejściowe wszystkich przypadków użycia, żeby <invoke>
        // mógł wskazać przypadek użycia zapisany dalej w pliku.
        foreach (var uc in useCases)
            CreateUseCaseClass(uc, ir, ctx);
        if (ctx.Errors.Count > 0)
        {
            Errors.AddRange(ctx.Errors);
            return ir;
        }

        foreach (var uc in useCases)
        {
            ProcessUseCaseClause(uc, ir, ctx);
            ProcessMainScenario(uc, ir, ctx);
            foreach (var scenario in uc.Scenarios)
                ProcessScenario(scenario, ir, ctx);
            CollectCheckResults(ir, ctx);
        }
        EnsureCheckResultEnums(ir, ctx);

        foreach (var ucc in ir.UseCases)
        {
            MakeOperationLabelsUnique(ucc);
            PruneInputData(ucc);
        }
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

    private void CreateUseCaseClass(UseCase uc, IrModel ir, TranslationContext ctx)
    {
        var ucc = new UseCaseClass
        {
            Label = ToPascalCase(uc.Name),
            IsInitial = string.Equals(uc.Name.Trim(), "Start", StringComparison.OrdinalIgnoreCase)
        };
        if (!ctx.UseCaseByName.TryAdd(uc.Name.Trim(), ucc))
        {
            ctx.Errors.Add($"Przypadek użycia „{uc.Name}” zdefiniowano więcej niż raz.");
            return;
        }
        ir.UseCases.Add(ucc);

        var initOp = new UCOperation { Label = "execute", IsInitial = true };
        ucc.Operations.Add(initOp);
        ucc.EntryOperation = initOp;

        var first = uc.MainScenario.AltSentences.FirstOrDefault();
        if (first is SvoSentence { Step: UserStep { Predicate: SelectPredicate sel } })
        {
            var entry = new UCOperation { Label = "handle" + ToPascalCase(sel.Notion), IsInitial = true, TriggerNotion = sel.Notion };
            ucc.Operations.Add(entry);
            ucc.EntryOperation = entry;
            ucc.EntryTriggerNotion = sel.Notion;
        }
    }

    /// <summary>
    /// Wartości warunków dotyczących pojęcia sprawdzanego w tym samym przypadku użycia
    /// (<c>[order ? OK]</c>, <c>[order ? notOK]</c>) tworzą enum <c>OrderCheckResult</c>.
    /// Pojęcie sprawdzane w kilku przypadkach użycia ma jeden enum z sumą wartości.
    /// </summary>
    private static void CollectCheckResults(IrModel ir, TranslationContext ctx)
    {
        foreach (var notion in ctx.CheckedNotions.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var enumUnion = GetOrCreateCheckResultEnum(notion, ir);
            var values = ctx.ValueConditions
                .Where(c => string.Equals(c.Notion.Trim(), notion.Trim(), StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Value);
            foreach (var value in values)
                if (!enumUnion.Elements.Any(e => Naming.EnumMember(e.Label) == Naming.EnumMember(value)))
                    enumUnion.Elements.Add(new EnumValue { Label = value });
        }
    }

    private static EnumUnion GetOrCreateCheckResultEnum(string notion, IrModel ir)
    {
        var label = ToPascalCase(notion) + "CheckResult";
        var enumUnion = ir.ViewModel.Enums.FirstOrDefault(e => e.Label == label);
        if (enumUnion == null)
        {
            enumUnion = new EnumUnion { Label = label, NotionName = notion.Trim() };
            ir.ViewModel.Enums.Add(enumUnion);
        }
        return enumUnion;
    }

    private static void EnsureCheckResultEnums(IrModel ir, TranslationContext ctx)
    {
        foreach (var si in ir.Services.Where(s => s.Methods.Any(m => m.Type == PredicateType.Check)))
        {
            var enumUnion = GetOrCreateCheckResultEnum(si.NotionName, ir);
            if (enumUnion.Elements.Count > 0) continue;
            enumUnion.Elements.Add(new EnumValue { Label = "ok" });
            ctx.Warnings.Add($"Pojęcie „{si.NotionName}” jest sprawdzane, ale żaden warunek nie podaje wartości wyniku; " +
                             $"{enumUnion.Label} ma jedną wartość „ok”.");
        }
    }

    /// <summary>
    /// Operacja dostaje dane z formularza tylko wtedy, gdy jej instrukcje ich używają – inaczej
    /// np. „cancel” wymagałby poprawnie wypełnionego formularza (D8.2).
    /// </summary>
    private static void PruneInputData(UseCaseClass ucc)
    {
        foreach (var op in ucc.Operations)
        {
            var used = UsedParameterTypes(op.Instructions).ToHashSet();
            op.InputData.RemoveAll(d => !used.Contains(d.Label));
        }
    }

    private static IEnumerable<string> UsedParameterTypes(IEnumerable<Instruction> instructions) =>
        instructions.SelectMany(i => i switch
        {
            Call c => c.Parameters.Select(p => p.Type),
            Decision d => d.Conditions.SelectMany(c => UsedParameterTypes(c.Instructions)),
            _ => Enumerable.Empty<string>()
        });

    private void ProcessUseCaseClause(UseCase uc, IrModel ir, TranslationContext ctx)
    {
        var ucc = ctx.UseCaseByName[uc.Name.Trim()];
        ctx.CurrentUCC = ucc;
        ctx.CurrentUseCaseName = uc.Name;
        ctx.FirstSentence = uc.MainScenario.AltSentences.FirstOrDefault();
        ctx.ContextDA.Clear();
        ctx.LabelSnapshots.Clear();
        ctx.CurrentVF = null;
        ctx.CurrentCondition = null;
        ctx.CurrentDAP.Clear();
        ctx.CurrentDAD.Clear();
        ctx.PendingEditable.Clear();
        ctx.CheckedNotions.Clear();
        ctx.ValueConditions.Clear();
        ctx.CurrentUCO = ucc.Operations[0];

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
                if (ctx.CurrentUCC != null && !ctx.CurrentUCC.ContextData.Contains(da))
                    ctx.CurrentUCC.ContextData.Add(da);
            }
            // Warunek na podmiocie „user” określa rolę aktora, a nie dane – nie tworzy usługi IUser.
            else if (cond is ValueCondition { Notion: "user" })
                continue;
            else if (cond is ValueCondition vc)
            {
                ctx.CheckedNotions.Add(vc.Notion);
                ctx.ValueConditions.Add((vc.Notion, vc.Value));
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
            foreach (var vc in cond.Conditions.OfType<ValueCondition>())
                ctx.ValueConditions.Add((vc.Notion, vc.Value));
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
            {
                if (ReferenceEquals(svo, ctx.FirstSentence))
                    ProcessFirstSelectSentence(ctx);
                else
                    ProcessSelectSentence(user.Actor, sel.Notion, ir, ctx);
            }
            else if (user.Predicate is EnterPredicate ent)
                ProcessEnterSentence(ent.Notion, ir, ctx);
            else if (user.Predicate is InvokePredicate inv)
                ProcessActorInvokeSentence(inv.Names, ctx);
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
            else if (sys.Predicate is SysInvokePredicate si)
                ProcessSystemInvokeSentence(si.Name, ctx);
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
            ir.Views.Add(vw);
            ir.Controllers.Add(ctrl);
            ir.Presenters.Add(pres);
        }
        // Ten sam widok może pokazywać kilka przypadków użycia – każdy potrzebuje jego prezentera.
        if (ctx.CurrentUCC != null && !ctx.CurrentUCC.Presenters.Contains(vw.Presenter!))
            ctx.CurrentUCC.Presenters.Add(vw.Presenter!);
        foreach (var da in ctx.CurrentDAP.Where(da => !vw.Data.Contains(da)))
            vw.Data.Add(da);
        foreach (var da in ctx.PendingEditable.Where(da => !vw.EditableData.Contains(da)))
            vw.EditableData.Add(da);
        ctx.PendingEditable.Clear();
        ctx.CurrentVF = vw;

        var po = new POperation { Label = "show" + label };
        foreach (var da in ctx.CurrentDAP)
            po.Parameters.Add(new Parameter { Label = da.Label, Type = da.Label });
        vw.Presenter!.Methods.Add(po);

        var call = new Call { Label = ctx.CurrentLabel, OperationRef = po.Label, Presenter = vw.Presenter };
        call.Parameters.AddRange(po.Parameters);
        AddInstruction(call, ctx);

        ctx.CurrentDAP.Clear();
        ctx.CurrentDAD.Clear();
        ctx.LabelToVF[ctx.CurrentLabel] = vw;
    }

    private void ProcessEnterSentence(string notion, IrModel ir, TranslationContext ctx)
    {
        var da = GetOrCreateDataAggregate(notion, ir);
        if (!ctx.CurrentDAD.Contains(da))
            ctx.CurrentDAD.Add(da);

        // <enter> następuje po <show> formularza, który aktor wypełnia, więc dane są polami
        // edycji ostatnio wyświetlonego widoku – a nie danymi do prezentacji w następnym (D6.1).
        if (ctx.CurrentVF == null)
        {
            if (!ctx.PendingEditable.Contains(da))
                ctx.PendingEditable.Add(da);
        }
        else if (!ctx.CurrentVF.EditableData.Contains(da))
            ctx.CurrentVF.EditableData.Add(da);
    }

    /// <summary>
    /// Pierwsze zdanie &lt;select&gt; przypadku użycia uruchamia go z zewnątrz, więc nie ma jeszcze
    /// widoku tego przypadku użycia. Przycisk dodaje &lt;invoke&gt; w przypadku wywołującym.
    /// </summary>
    private static void ProcessFirstSelectSentence(TranslationContext ctx)
    {
        ctx.CurrentUCO = ctx.CurrentUCC!.EntryOperation;
        ctx.CurrentCondition = null;
    }

    private void ProcessActorInvokeSentence(List<string> names, TranslationContext ctx)
    {
        foreach (var name in names)
        {
            if (!ctx.UseCaseByName.TryGetValue(name.Trim(), out var invoked))
            {
                ctx.Errors.Add($"Przypadek użycia „{ctx.CurrentUseCaseName}”: <invoke> wskazuje nieznany przypadek użycia „{name}”.");
                continue;
            }
            if (ctx.CurrentUCC != null && !ctx.CurrentUCC.InvokedUseCases.Contains(invoked))
                ctx.CurrentUCC.InvokedUseCases.Add(invoked);
            if (ctx.CurrentVF == null)
            {
                ctx.Warnings.Add($"Przypadek użycia „{ctx.CurrentUseCaseName}”: <invoke> {name} przed pierwszym <show> – brak widoku na przycisk.");
                continue;
            }
            if (invoked.EntryTriggerNotion == null)
                ctx.Warnings.Add($"Przypadek użycia „{name}” jest wywoływany przez <invoke>, ale nie zaczyna się od <select>; " +
                                 "przycisk uruchamia operację execute i ma nazwę przypadku użycia (D5.2).");
            AddTrigger(ctx.CurrentVF, invoked.EntryTriggerNotion ?? name, invoked.EntryOperation!, invoked, ctx);
        }
    }

    private void ProcessSystemInvokeSentence(string name, TranslationContext ctx)
    {
        if (!ctx.UseCaseByName.TryGetValue(name.Trim(), out var invoked))
        {
            ctx.Errors.Add($"Przypadek użycia „{ctx.CurrentUseCaseName}”: <invoke> wskazuje nieznany przypadek użycia „{name}”.");
            return;
        }
        if (ctx.CurrentUCC != null && !ctx.CurrentUCC.InvokedUseCases.Contains(invoked))
            ctx.CurrentUCC.InvokedUseCases.Add(invoked);
        AddInstruction(new Call
        {
            Label = ctx.CurrentLabel,
            OperationRef = invoked.EntryOperation!.Label,
            TargetUseCase = invoked,
            TargetOperation = invoked.EntryOperation
        }, ctx);
    }

    private void ProcessSelectSentence(string actor, string notion, IrModel ir, TranslationContext ctx)
    {
        // Operacja należy do przypadku użycia, w którego scenariuszu stoi zdanie (D3.1),
        // a przycisk – do widoku wyświetlonego jako ostatni.
        var ucOp = new UCOperation
        {
            Label = "handle" + ToPascalCase(notion),
            SourceView = ctx.CurrentVF,
            TriggerNotion = notion
        };
        if (ctx.CurrentVF != null)
            ucOp.InputData.AddRange(ctx.CurrentVF.EditableData);
        ctx.CurrentUCC?.Operations.Add(ucOp);
        ctx.CurrentUCO = ucOp;
        // Nowa operacja zaczyna się poza gałęzią decyzji poprzedniej operacji.
        ctx.CurrentCondition = null;

        if (ctx.CurrentVF != null)
            AddTrigger(ctx.CurrentVF, notion, ucOp, ctx.CurrentUCC, ctx);
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
        var readAggregate = type == PredicateType.Read ? GetOrCreateDataAggregate(notion, ir) : null;
        if (type == PredicateType.Check)
            ctx.CheckedNotions.Add(notion);
        var sopLabel = type.ToString().ToLowerInvariant() + ToPascalCase(notion);
        var parameters = ctx.CurrentDAD.Concat(ctx.ContextDA).Distinct()
            .Select(da => new Parameter { Label = da.Label, Type = da.Label }).ToList();
        // Ta sama operacja w kilku miejscach specyfikacji to jedna metoda interfejsu (D8.5).
        var sop = si.Methods.FirstOrDefault(m => m.Label == sopLabel);
        if (sop == null)
        {
            sop = new SOperation { Label = sopLabel, Type = type };
            sop.Parameters.AddRange(parameters);
            si.Methods.Add(sop);
        }
        else if (!sop.Parameters.Select(p => p.Type).SequenceEqual(parameters.Select(p => p.Type)))
            ctx.Warnings.Add($"Przypadek użycia „{ctx.CurrentUseCaseName}”: operacja {si.Label}.{sopLabel} była już użyta z parametrami " +
                             $"({string.Join(", ", sop.Parameters.Select(p => p.Type))}); pozostają one bez zmian.");

        var call = new Call { Label = ctx.CurrentLabel, OperationRef = sop.Label, Service = si, ServiceOperation = sop };
        call.Parameters.AddRange(sop.Parameters.Select(p => new Parameter { Label = p.Label, Type = p.Type }));
        AddInstruction(call, ctx);

        if (ctx.CurrentUCC != null && !ctx.CurrentUCC.Services.Contains(si))
            ctx.CurrentUCC.Services.Add(si);

        // Odczytany obiekt pokazuje następny widok.
        if (readAggregate != null && !ctx.CurrentDAP.Contains(readAggregate))
            ctx.CurrentDAP.Add(readAggregate);
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
}
