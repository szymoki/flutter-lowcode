using FlutterCodeGenerator.Model.Ir;

namespace FlutterCodeGenerator.Tests;

internal static class IrHelpers
{
    public static IrModel CompileSpec(string text)
    {
        var result = Pipeline.Compile(text);
        Assert.True(result.Success, string.Join("\n", result.Errors));
        return result.Ir!;
    }

    public static IrModel CompileExample() =>
        CompileSpec(File.ReadAllText(Path.Combine(TestPaths.Example, "example.txt")));

    public static ViewWidget View(this IrModel ir, string label) =>
        ir.Views.Single(v => v.Label == label);

    public static UseCaseClass UseCase(this IrModel ir, string label) =>
        ir.UseCases.Single(u => u.Label == label);

    public static string[] TriggerLabels(this ViewWidget view) =>
        view.Triggers.Select(t => t.Label).OrderBy(l => l, StringComparer.Ordinal).ToArray();

    public static string[] Sorted(params string[] items) =>
        items.OrderBy(l => l, StringComparer.Ordinal).ToArray();
}
