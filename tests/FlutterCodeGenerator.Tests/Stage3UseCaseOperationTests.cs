using System.Text.RegularExpressions;

namespace FlutterCodeGenerator.Tests;

public class Stage3UseCaseOperationTests
{
    [Fact]
    public void SelectOperation_BelongsToUseCaseOfItsScenario()
    {
        var ir = IrHelpers.CompileExample();
        Assert.DoesNotContain(ir.UseCase("Start").Operations, o => o.Label == "handleFindOrder");
        Assert.Contains(ir.UseCase("FindOrder").Operations, o => o.Label == "handleFindOrder");
    }

    [Fact]
    public void EveryOperation_IsUniqueWithinUseCase_AndTargetsOwnUseCase()
    {
        var ir = IrHelpers.CompileExample();
        foreach (var uc in ir.UseCases)
            Assert.Equal(uc.Operations.Count, uc.Operations.Select(o => o.Label).Distinct().Count());
        foreach (var cop in ir.Controllers.SelectMany(c => c.Operations))
            Assert.Contains(cop.Target!, cop.TargetUseCase!.Operations);
    }

    [Fact]
    public void GeneratedUseCaseMethods_MatchIrOperationsExactly()
    {
        using var p = GeneratedProject.FromExample();
        var ir = p.Result.Ir!;
        foreach (var uc in ir.UseCases)
        {
            var code = p.Read("use_cases/" + Naming.SnakeFromPascal(uc.Label) + "_use_case.dart");
            var methods = Regex.Matches(code, @"^  (?:void|Future<void>) (\w+)\(", RegexOptions.Multiline).Select(m => m.Groups[1].Value).ToList();
            Assert.Equal(uc.Operations.Select(o => o.Label).OrderBy(x => x, StringComparer.Ordinal), methods.OrderBy(x => x, StringComparer.Ordinal));
        }
        Assert.DoesNotContain("handleFindOrder", p.Read("use_cases/start_use_case.dart"));
        Assert.Contains("handleFindOrder", p.Read("use_cases/find_order_use_case.dart"));
    }

    [Fact]
    public void Controller_CallsTargetUseCase()
    {
        using var p = GeneratedProject.FromExample();
        var orderWindow = p.Read("controllers/order_window_controller.dart");
        Assert.Contains("void onClose() { _findOrderUseCase.handleCloseOrderWindow(); }", orderWindow);
        Assert.Contains("required FindOrderUseCase findOrderUseCase", orderWindow);
    }

    [Fact]
    public void ViewNameStartingWithC_KeepsControllerFileName()
    {
        const string spec = """
            Use case Confirm Main scenario
            00: User <select> confirm
            01: System <show> confirmation
            02: User <select> close
            -> end ! OK
            """;
        using var p = GeneratedProject.FromSpec(spec);
        Assert.True(File.Exists(p.Lib("controllers/confirmation_controller.dart")));
        Assert.Contains("import '../controllers/confirmation_controller.dart';", p.Read("views/confirmation_view.dart"));
    }
}
