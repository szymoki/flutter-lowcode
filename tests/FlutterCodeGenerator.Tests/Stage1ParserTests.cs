using FlutterCodeGenerator.Model.Ir;
using FlutterCodeGenerator.Model.Rsl;

namespace FlutterCodeGenerator.Tests;

public class Stage1ParserTests
{
    [Fact]
    public void Preconditions_AreAssignedToMainScenario()
    {
        var errors = new List<string>();
        var (useCases, _) = Pipeline.Parse(File.ReadAllText(Path.Combine(TestPaths.Example, "example.txt")), "example.txt", errors);
        Assert.Empty(errors);

        var delete = useCases.Single(u => u.Name == "Delete order");
        Assert.Collection(delete.MainScenario.Preconditions,
            c => Assert.Equal("user", Assert.IsType<ValueCondition>(c).Notion),
            c => Assert.Equal("order", Assert.IsType<ContextCondition>(c).Notion));
    }

    [Fact]
    public void ContextCondition_BecomesParameterOfServiceOperation()
    {
        var ir = IrHelpers.CompileExample();
        var deleteOrder = ir.Services.Single(s => s.Label == "IOrder").Methods.Single(m => m.Label == "deleteOrder");
        Assert.Equal(new[] { "Order" }, deleteOrder.Parameters.Select(p => p.Type));
    }

    [Fact]
    public void UserCondition_DoesNotCreateService()
    {
        var ir = IrHelpers.CompileExample();
        Assert.DoesNotContain(ir.Services, s => s.Label == "IUser");
    }

    [Fact]
    public void NegatedValueCondition_IsDetected()
    {
        var spec = TestPaths.ReadSpec("negation.txt");
        var errors = new List<string>();
        var (useCases, _) = Pipeline.Parse(spec, "negation.txt", errors);
        Assert.Empty(errors);
        var cond = useCases[0].MainScenario.AltSentences.OfType<CondSentence>().Single().Conditions.Single();
        var vc = Assert.IsType<ValueCondition>(cond);
        Assert.True(vc.Negated);
        Assert.Equal("valid", vc.Value);

        var ir = IrHelpers.CompileSpec(spec);
        var expr = ir.UseCases.Single().Operations
            .SelectMany(o => o.Instructions.OfType<Decision>())
            .SelectMany(d => d.Conditions).SelectMany(c => c.Expressions).Single();
        Assert.True(expr.Negated);
    }

    [Fact]
    public void NonNegatedValueCondition_IsNotNegated()
    {
        var errors = new List<string>();
        var (useCases, _) = Pipeline.Parse(File.ReadAllText(Path.Combine(TestPaths.Example, "example.txt")), "example.txt", errors);
        var conds = useCases.SelectMany(u => u.MainScenario.AltSentences.Concat(u.Scenarios.SelectMany(s => s.AltSentences)))
            .OfType<CondSentence>().SelectMany(c => c.Conditions).OfType<ValueCondition>().ToList();
        Assert.NotEmpty(conds);
        Assert.All(conds, c => Assert.False(c.Negated));
    }

    [Fact]
    public void SyntaxError_StopsPipelineWithLocation()
    {
        var input = TestPaths.NewTempDir();
        var output = TestPaths.NewTempDir();
        try
        {
            File.Copy(Path.Combine(TestPaths.Specs, "syntax_error.txt"), Path.Combine(input, "bad.txt"));
            var result = Pipeline.Run(input, output);
            Assert.False(result.Success);
            Assert.Contains(result.Errors, e => e.StartsWith("bad.txt:3:"));
            Assert.Null(result.Ir);
            Assert.False(Directory.Exists(Path.Combine(output, "lib")));
        }
        finally
        {
            Directory.Delete(input, true);
            Directory.Delete(output, true);
        }
    }

    [Fact]
    public void NoInputFiles_IsError()
    {
        var input = TestPaths.NewTempDir();
        var output = TestPaths.NewTempDir();
        try
        {
            var result = Pipeline.Run(input, output);
            Assert.False(result.Success);
            Assert.False(Directory.Exists(Path.Combine(output, "lib")));
        }
        finally
        {
            Directory.Delete(input, true);
            Directory.Delete(output, true);
        }
    }
}
