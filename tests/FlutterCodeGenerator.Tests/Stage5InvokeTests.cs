using System.Text.RegularExpressions;
using FlutterCodeGenerator.Model.Ir;

namespace FlutterCodeGenerator.Tests;

public class Stage5InvokeTests
{
    /// <summary>Tabela z kryteriów akceptacji planu poprawek (20 przypisań).</summary>
    public static readonly Dictionary<string, string[]> ExpectedTriggers = new()
    {
        ["MainMenu"] = new[] { "showOrderList", "findOrder", "addOrder" },
        ["OrderSearchForm"] = new[] { "search", "cancel" },
        ["OrderWindow"] = new[] { "close" },
        ["SearchErrorMessage"] = new[] { "close" },
        ["OrderListWindow"] = new[] { "close", "orderList", "findOrder", "addOrder", "deleteOrder" },
        ["AddOrderForm"] = new[] { "add", "cancel" },
        ["OrderSuccessMessage"] = new[] { "ok" },
        ["OrderErrorMessage"] = new[] { "close", "correct" },
        ["DeleteConfirmationMessage"] = new[] { "delete", "cancel" },
        ["OrderDeletedMessage"] = new[] { "ok" },
    };

    [Fact]
    public void Example_TriggerAssignment_MatchesAcceptanceTable()
    {
        var ir = IrHelpers.CompileExample();
        Assert.Equal(ExpectedTriggers.Keys.OrderBy(k => k, StringComparer.Ordinal), ir.Views.Select(v => v.Label).OrderBy(k => k, StringComparer.Ordinal));
        foreach (var (view, triggers) in ExpectedTriggers)
            Assert.Equal(IrHelpers.Sorted(triggers), ir.View(view).TriggerLabels());
    }

    [Fact]
    public void InvokeTrigger_TargetsEntryOperationOfInvokedUseCase()
    {
        var ir = IrHelpers.CompileExample();
        var onFindOrder = ir.View("MainMenu").Controller!.Operations.Single(o => o.Label == "onFindOrder");
        Assert.Same(ir.UseCase("FindOrder"), onFindOrder.TargetUseCase);
        Assert.Same(ir.UseCase("FindOrder").EntryOperation, onFindOrder.Target);
        Assert.Equal("handleFindOrder", onFindOrder.Target!.Label);
        Assert.Equal(
            new[] { "ShowOrderList", "FindOrder", "AddOrder" },
            ir.UseCase("Start").InvokedUseCases.Select(u => u.Label));
    }

    [Fact]
    public void FirstSelect_IsEntryOperation_WithoutTriggerInForeignView()
    {
        var ir = IrHelpers.CompileExample();
        var start = ir.UseCase("Start");
        Assert.Equal("handleApplication", start.EntryOperation!.Label);
        Assert.True(start.EntryOperation.IsInitial);
        Assert.DoesNotContain(ir.Views, v => v.TriggerLabels().Contains("application"));
        Assert.Single(ir.UseCase("AddOrder").Operations, o => o.IsInitial && o.Label != "execute");
    }

    [Fact]
    public void TriggerSets_DoNotDependOnUseCaseOrder()
    {
        var text = File.ReadAllText(Path.Combine(TestPaths.Example, "example.txt"));
        var dataStart = text.IndexOf("\nData ", StringComparison.Ordinal);
        var useCases = Regex.Split(text[..dataStart], @"(?=^Use case )", RegexOptions.Multiline)
            .Where(b => b.Trim().Length > 0).Select(b => b.TrimEnd() + "\n\n").ToList();
        Assert.Equal(5, useCases.Count);
        useCases.Reverse();
        var reversed = string.Concat(useCases) + text[dataStart..];

        var original = IrHelpers.CompileExample();
        var permuted = IrHelpers.CompileSpec(reversed);
        foreach (var view in original.Views)
            Assert.Equal(view.TriggerLabels(), permuted.View(view.Label).TriggerLabels());
    }

    [Fact]
    public void InvokedUseCaseWithoutSelect_GetsWarningAndExecuteTrigger()
    {
        const string spec = """
            Use case Start Main scenario
            00: User <select> application
            01: System <show> main menu
            02: User <invoke> Refresh
            -> rejoin 01
            Use case Refresh Main scenario
            00: System <read> order list
            01: System <show> order list window
            02: User <select> close
            -> end ! OK
            """;
        var result = Pipeline.Compile(spec);
        Assert.True(result.Success, string.Join("\n", result.Errors));
        Assert.Contains(result.Warnings, w => w.Contains("Refresh"));
        var op = result.Ir!.Views.Single(v => v.Label == "MainMenu").Controller!.Operations.Single();
        Assert.Equal("onRefresh", op.Label);
        Assert.Equal("execute", op.Target!.Label);
    }

    [Fact]
    public void InvokeOfUnknownUseCase_IsError()
    {
        const string spec = """
            Use case Start Main scenario
            00: User <select> application
            01: System <show> main menu
            02: User <invoke> Missing
            -> rejoin 01
            """;
        var result = Pipeline.Compile(spec);
        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("Missing"));
    }

    [Fact]
    public void SystemInvoke_CallsEntryOperation()
    {
        const string spec = """
            Use case Start Main scenario
            00: User <select> application
            01: System <invoke> Show list
            -> end ! OK
            Use case Show list Main scenario
            00: User <select> show list
            01: System <show> list window
            -> end ! OK
            """;
        var ir = IrHelpers.CompileSpec(spec);
        var call = Assert.Single(ir.UseCase("Start").EntryOperation!.Instructions.OfType<Call>());
        Assert.Same(ir.UseCase("ShowList"), call.TargetUseCase);
        Assert.Equal("handleShowList", call.OperationRef);
    }
}
