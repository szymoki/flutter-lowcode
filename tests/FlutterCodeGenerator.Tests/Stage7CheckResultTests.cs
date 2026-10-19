namespace FlutterCodeGenerator.Tests;

public class Stage7CheckResultTests
{
    [Theory]
    [InlineData("OK", "ok")]
    [InlineData("notOK", "notOk")]
    [InlineData("valid", "valid")]
    [InlineData("not valid", "notValid")]
    [InlineData("default", "valueDefault")]
    [InlineData("index", "valueIndex")]
    [InlineData("2xx", "value2Xx")]
    public void EnumMember_IsCamelCaseDartIdentifier(string value, string expected) =>
        Assert.Equal(expected, Naming.EnumMember(value));

    [Fact]
    public void CheckedNotion_GetsEnumFromConditionValues()
    {
        var ir = IrHelpers.CompileExample();
        var order = ir.ViewModel.Enums.Single(e => e.Label == "OrderCheckResult");
        Assert.Equal(new[] { "OK", "notOK" }, order.Elements.Select(e => e.Label));
        var search = ir.ViewModel.Enums.Single(e => e.Label == "OrderSearchCheckResult");
        Assert.Equal(new[] { "valid", "invalid" }, search.Elements.Select(e => e.Label));

        using var p = GeneratedProject.FromExample();
        Assert.Equal("// Generated from IR\nenum OrderCheckResult { ok, notOk }\n", p.Read("models/order_check_result.dart"));
    }

    [Fact]
    public void ConditionsFromOtherUseCase_AreNotCollected()
    {
        const string spec = """
            Use case A Main scenario
            00: User <select> a
            01: System <show> a form
            02: User <enter> item
            03: User <select> save
            04: System <check> item
            [item ? fine]
            -> end ! OK
            Use case B Main scenario
            00: User <select> b
            01: System <show> b form
            02: User <select> go
            [item ? broken]
            -> end ! OK
            """;
        var ir = IrHelpers.CompileSpec(spec);
        Assert.Equal(new[] { "fine" }, ir.ViewModel.Enums.Single().Elements.Select(e => e.Label));
    }

    [Fact]
    public void CheckWithoutValues_GetsOkAndWarning()
    {
        const string spec = """
            Use case A Main scenario
            00: User <select> a
            01: System <show> a form
            02: User <enter> item
            03: User <select> save
            04: System <check> item
            -> end ! OK
            """;
        var result = Pipeline.Compile(spec);
        Assert.True(result.Success);
        Assert.Contains(result.Warnings, w => w.Contains("ItemCheckResult"));
        Assert.Equal(new[] { "ok" }, result.Ir!.ViewModel.Enums.Single().Elements.Select(e => e.Label));
    }
}
