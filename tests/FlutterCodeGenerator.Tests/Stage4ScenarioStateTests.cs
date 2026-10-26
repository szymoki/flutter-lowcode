using FlutterCodeGenerator.Model.Ir;

namespace FlutterCodeGenerator.Tests;

public class Stage4ScenarioStateTests
{
    [Theory]
    [InlineData("AddOrderForm")]
    [InlineData("OrderSearchForm")]
    [InlineData("DeleteConfirmationMessage")]
    public void Cancel_IsTriggerOfViewFromRepeatedLabel(string view)
    {
        var ir = IrHelpers.CompileExample();
        Assert.Contains("cancel", ir.View(view).TriggerLabels());
    }

    [Fact]
    public void Cancel_IsNotInOtherViews()
    {
        var ir = IrHelpers.CompileExample();
        var withCancel = ir.Views.Where(v => v.TriggerLabels().Contains("cancel")).Select(v => v.Label).OrderBy(x => x, StringComparer.Ordinal);
        Assert.Equal(IrHelpers.Sorted("AddOrderForm", "OrderSearchForm", "DeleteConfirmationMessage"), withCancel);
    }

    [Fact]
    public void HandleAdd_HasOneDecisionWithTwoBranches()
    {
        var ir = IrHelpers.CompileExample();
        var handleAdd = ir.UseCase("AddOrder").Operations.Single(o => o.Label == "handleAdd");
        var decision = Assert.Single(handleAdd.Instructions.OfType<Decision>());
        Assert.Equal(new[] { "OK", "notOK" }, decision.Conditions.Select(c => c.Expressions.Single().Val));
        Assert.DoesNotContain(ir.UseCase("AddOrder").Operations.Where(o => o != handleAdd), o => o.Instructions.OfType<Decision>().Any());
    }

    [Fact]
    public void Correct_IsTriggerOfOrderErrorMessage()
    {
        var ir = IrHelpers.CompileExample();
        Assert.Contains("correct", ir.View("OrderErrorMessage").TriggerLabels());
        Assert.DoesNotContain("correct", ir.View("OrderSuccessMessage").TriggerLabels());
    }

    [Fact]
    public void Rejoin_CopiesInstructionsFromTargetLabel()
    {
        var ir = IrHelpers.CompileExample();
        var handleCorrect = ir.UseCase("AddOrder").Operations.Single(o => o.Label == "handleCorrect");
        var call = Assert.IsType<Call>(Assert.Single(handleCorrect.Instructions));
        Assert.Equal("showAddOrderForm", call.OperationRef);
        Assert.Equal("01", handleCorrect.ReturnTo);
    }

    [Fact]
    public void Rejoin_ToEarlierPositionOfSameOperation_DoesNotDuplicate()
    {
        var ir = IrHelpers.CompileExample();
        var start = ir.UseCase("Start").Operations.Single(o => o.Label == "handleApplication");
        Assert.Single(start.Instructions.OfType<Call>(), c => c.OperationRef == "showMainMenu");
    }

    [Fact]
    public void UnknownRepeatedLabel_IsSemanticError()
    {
        const string spec = """
            Use case Add order Main scenario
            00: Cashier <select> add order
            01: System <show> add order form
            -> end ! OK
            Scenario
            07: -"-
            A1: Cashier <select> cancel
            -> end ! CANCEL
            """;
        var result = Pipeline.Compile(spec);
        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("Add order") && e.Contains("07"));
    }
}
