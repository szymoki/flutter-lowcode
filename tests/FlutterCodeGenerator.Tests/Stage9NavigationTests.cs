namespace FlutterCodeGenerator.Tests;

public class Stage9NavigationTests
{
    [Fact]
    public void App_StartsFromStartUseCase_NotFromViewList()
    {
        using var p = GeneratedProject.FromExample();
        var app = p.Read("app.dart");
        Assert.Contains("addPostFrameCallback((_) => _startUseCase.handleApplication());", app);
        Assert.Contains("IndexedStack(", app);
        Assert.Contains("this.viewListMode = false", app);
        Assert.Contains("const App()", p.Read("main.dart"));
    }

    [Fact]
    public void Views_DoNotShowThemselves()
    {
        using var p = GeneratedProject.FromExample();
        foreach (var file in Directory.GetFiles(p.Lib("views")))
            Assert.DoesNotContain("initState", File.ReadAllText(file));
    }

    [Fact]
    public void InvokeButton_RegistersResumeOfCaller()
    {
        using var p = GeneratedProject.FromExample();
        Assert.Contains(
            "void onAddOrder() { _addOrderUseCase.setOnEnd(_startUseCase.resumeHandleApplication); _addOrderUseCase.handleAddOrder(); }",
            p.Read("controllers/main_menu_controller.dart"));
        Assert.Contains(
            "void onDeleteOrder() { _deleteOrderUseCase.setOnEnd(_showOrderListUseCase.resumeHandleOrderList); _deleteOrderUseCase.handleDeleteOrder(); }",
            p.Read("controllers/order_list_window_controller.dart"));
    }

    [Fact]
    public void InstructionsAfterInvoke_GoToResume()
    {
        var ir = IrHelpers.CompileExample();
        var start = ir.UseCase("Start").EntryOperation!;
        Assert.True(start.IsInvoking);
        Assert.Equal(new[] { "showMainMenu" }, start.ResumeInstructions.OfType<Model.Ir.Call>().Select(c => c.OperationRef));

        var orderList = ir.UseCase("ShowOrderList").Operations.Single(o => o.Label == "handleOrderList");
        Assert.Empty(orderList.Instructions);
        Assert.Equal(new[] { "readOrderList", "showOrderListWindow" }, orderList.ResumeInstructions.OfType<Model.Ir.Call>().Select(c => c.OperationRef));
    }

    [Fact]
    public void End_NotifiesCaller()
    {
        using var p = GeneratedProject.FromExample();
        var code = p.Read("use_cases/add_order_use_case.dart");
        Assert.Contains("void setOnEnd(void Function(String result) onEnd) {", code);
        Assert.Contains("final onEnd = _onEnd;\n    _onEnd = null;\n    onEnd?.call(result);", code);
        Assert.DoesNotContain("setOnEnd", p.Read("use_cases/start_use_case.dart"));
    }

    [Fact]
    public void SystemInvoke_IsConnectedAndResumes()
    {
        using var p = GeneratedProject.FromSpec(TestPaths.ReadSpec("system_invoke.txt"));
        var start = p.Read("use_cases/start_use_case.dart");
        Assert.Contains("late final ShowListUseCase _showListUseCase;", start);
        Assert.Contains("void connectUseCases({required ShowListUseCase showListUseCase}) {", start);
        Assert.Contains("_showListUseCase.setOnEnd(resumeHandleRefresh);\n    _showListUseCase.handleShowList();", start);
        Assert.Contains("void resumeHandleRefresh(String result) {\n    _hideViews();\n    _pMainMenu.showMainMenu();", start);
        Assert.Contains("_startUseCase.connectUseCases(showListUseCase: _showListUseCase);", p.Read("app.dart"));
    }
}
