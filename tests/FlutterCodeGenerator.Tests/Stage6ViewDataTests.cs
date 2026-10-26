namespace FlutterCodeGenerator.Tests;

public class Stage6ViewDataTests
{
    [Fact]
    public void ReadData_IsPresentedInNextView()
    {
        var ir = IrHelpers.CompileExample();
        Assert.Equal(new[] { "Order" }, ir.View("OrderWindow").Data.Select(d => d.Label));
        Assert.Empty(ir.View("OrderWindow").EditableData);
        Assert.Equal(new[] { "OrderList" }, ir.View("OrderListWindow").Data.Select(d => d.Label));
    }

    [Fact]
    public void EnteredData_IsEditableOnlyInItsForm()
    {
        var ir = IrHelpers.CompileExample();
        Assert.Equal(new[] { "Order" }, ir.View("AddOrderForm").EditableData.Select(d => d.Label));
        Assert.Equal(new[] { "OrderSearch" }, ir.View("OrderSearchForm").EditableData.Select(d => d.Label));
        Assert.All(ir.Views.Where(v => v.Label is not ("AddOrderForm" or "OrderSearchForm")), v => Assert.Empty(v.EditableData));
    }

    [Fact]
    public void Views_HaveTextFieldsOnlyForEditableData()
    {
        using var p = GeneratedProject.FromExample();
        Assert.DoesNotContain("TextField", p.Read("views/order_success_message_view.dart"));
        Assert.DoesNotContain("TextField", p.Read("views/order_window_view.dart"));
        Assert.Contains("TextField(controller: _orderNumberController", p.Read("views/add_order_form_view.dart"));
        var orderWindow = p.Read("views/order_window_view.dart");
        Assert.Contains("presenter.order?.title", orderWindow);
        Assert.DoesNotContain("orderSearch", orderWindow);
    }

    [Fact]
    public void ListAttribute_IsDartList_AndRenderedWithListView()
    {
        var ir = IrHelpers.CompileExample();
        var orders = ir.ViewModel.Aggregates.Single(a => a.Label == "OrderList").Items.Single();
        Assert.True(orders.IsList);

        using var p = GeneratedProject.FromExample();
        Assert.Contains("final List<Order> orders;", p.Read("models/order_list.dart"));
        Assert.Contains("OrderList copyWith({List<Order>? orders})", p.Read("models/order_list.dart"));
        Assert.Contains("ListView.builder(", p.Read("views/order_list_window_view.dart"));
        Assert.Contains("presenter.orderList?.orders.length", p.Read("views/order_list_window_view.dart"));
    }

    [Fact]
    public void EnterBeforeShow_GoesToNextView()
    {
        const string spec = """
            Use case Add item Main scenario
            00: Clerk <select> add item
            01: Clerk <enter> item
            02: System <show> item form
            03: Clerk <select> save
            -> end ! OK
            Data item { name : text }
            """;
        var ir = IrHelpers.CompileSpec(spec);
        Assert.Equal(new[] { "Item" }, ir.View("ItemForm").EditableData.Select(d => d.Label));
    }
}
