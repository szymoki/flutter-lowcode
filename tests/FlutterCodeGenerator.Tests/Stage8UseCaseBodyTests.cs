namespace FlutterCodeGenerator.Tests;

public class Stage8UseCaseBodyTests
{
    private static string MethodText(string code, string signatureStart)
    {
        var start = code.IndexOf(signatureStart, StringComparison.Ordinal);
        Assert.True(start >= 0, "Brak metody: " + signatureStart);
        var end = code.IndexOf("\n  }\n", start, StringComparison.Ordinal);
        return code[start..(end + 4)];
    }

    [Fact]
    public void HandleAdd_Body_MatchesGolden()
    {
        using var p = GeneratedProject.FromExample();
        var expected = """
            Future<void> handleAdd(Order order) async {
                _currentOrder = order;
                _orderCheckResult = await _order.checkOrder(_currentOrder!);
                if (_orderCheckResult == OrderCheckResult.ok) {
                  await _order.updateOrder(_currentOrder!);
                  _hideViews();
                  _pOrderSuccessMessage.showOrderSuccessMessage();
                } else if (_orderCheckResult == OrderCheckResult.notOk) {
                  _hideViews();
                  _pOrderErrorMessage.showOrderErrorMessage();
                }
              }
            """;
        Assert.Equal(expected, MethodText(p.Read("use_cases/add_order_use_case.dart"), "Future<void> handleAdd("));
    }

    [Fact]
    public void ReadResult_IsPassedToNextView()
    {
        using var p = GeneratedProject.FromExample();
        var body = MethodText(p.Read("use_cases/find_order_use_case.dart"), "Future<void> handleSearch(");
        Assert.Contains("_currentOrder = await _order.readOrder(_currentOrderSearch!);", body);
        Assert.Contains("_pOrderWindow.showOrderWindow(order: _currentOrder);", body);
    }

    [Fact]
    public void End_HidesViewsOfUseCase()
    {
        using var p = GeneratedProject.FromExample();
        var code = p.Read("use_cases/add_order_use_case.dart");
        Assert.Contains("void handleCancel() {\n    _end('CANCEL');\n  }", code);
        Assert.Contains("void _end(String result) {\n    _hideViews();", code);
    }

    [Fact]
    public void FormData_IsPassedOnlyToOperationsThatUseIt()
    {
        var ir = IrHelpers.CompileExample();
        var addOrder = ir.UseCase("AddOrder");
        Assert.Equal(new[] { "Order" }, addOrder.Operations.Single(o => o.Label == "handleAdd").InputData.Select(d => d.Label));
        Assert.Empty(addOrder.Operations.Single(o => o.Label == "handleCancel").InputData);

        using var p = GeneratedProject.FromExample();
        var controller = p.Read("controllers/add_order_form_controller.dart");
        Assert.Contains("void onAdd(Order order) { _addOrderUseCase.handleAdd(order); }", controller);
        Assert.Contains("void onCancel() { _addOrderUseCase.handleCancel(); }", controller);
        var view = p.Read("views/add_order_form_view.dart");
        Assert.Contains("final number = int.tryParse(_orderNumberController.text);", view);
        Assert.Contains("widget.controller.onAdd(order);", view);
    }

    [Fact]
    public void ContextData_HasSetter()
    {
        using var p = GeneratedProject.FromExample();
        var code = p.Read("use_cases/delete_order_use_case.dart");
        Assert.Contains("void setOrder(Order value) {", code);
        Assert.Contains("await _order.deleteOrder(_currentOrder!);", code);
    }

    [Fact]
    public void RepeatedServiceOperation_IsOneInterfaceMethod()
    {
        const string spec = """
            Use case A Main scenario
            00: User <select> a
            01: System <read> item
            02: System <show> item window
            03: User <select> refresh
            04: System <read> item
            05: System <show> item window
            -> end ! OK
            """;
        var ir = IrHelpers.CompileSpec(spec);
        Assert.Single(ir.Services.Single().Methods);
    }
}
