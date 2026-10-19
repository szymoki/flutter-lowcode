namespace FlutterCodeGenerator.Tests;

public class Stage2GeneratorTests
{
    [Fact]
    public void ServiceReturnTypes_AreNotDoubleWrapped()
    {
        using var p = GeneratedProject.FromExample();
        var iOrder = p.Read("services/i_order.dart");
        Assert.DoesNotContain("Future<Future", iOrder);
        Assert.Contains("Future<Order> readOrder(", iOrder);
        Assert.Contains("Future<OrderCheckResult> checkOrder(Order order);", iOrder);
        Assert.Contains("Future<void> updateOrder(Order order);", iOrder);
        Assert.Contains("Future<OrderList> readOrderList(", p.Read("services/i_order_list.dart"));
    }

    [Fact]
    public void MultiWordNotion_KeepsWordBoundaries()
    {
        using var p = GeneratedProject.FromExample();
        Assert.True(File.Exists(p.Lib("models/order_search_check_result.dart")));
        Assert.False(File.Exists(p.Lib("models/ordersearch_check_result.dart")));
        Assert.Contains("enum OrderSearchCheckResult", p.Read("models/order_search_check_result.dart"));
        Assert.Contains("Future<OrderSearchCheckResult> checkOrderSearch(", p.Read("services/i_order_search.dart"));
    }

    [Fact]
    public void NotionStartingWithI_IsNotTruncated()
    {
        using var p = GeneratedProject.FromSpec(TestPaths.ReadSpec("negation.txt"));
        Assert.True(File.Exists(p.Lib("services/i_item.dart")));
        Assert.Contains("class ItemServiceProxy implements IItem", p.Read("services/item_service_proxy.dart"));
        Assert.True(File.Exists(p.Lib("models/item_check_result.dart")));
        Assert.Contains("required IItem item", p.Read("use_cases/find_item_use_case.dart"));
    }

    [Fact]
    public void CopyWith_UsesNamedParameters()
    {
        using var p = GeneratedProject.FromExample();
        Assert.Contains("Order copyWith({int? number, String? title, String? dates})", p.Read("models/order.dart"));
    }

    [Fact]
    public void ModelWithNestedNotion_ImportsIt()
    {
        using var p = GeneratedProject.FromExample();
        Assert.Contains("import 'order.dart';", p.Read("models/order_list.dart"));
    }

    [Fact]
    public void Pubspec_HasMaterialDesign_AlsoWhenMerged()
    {
        using var p = GeneratedProject.FromExample();
        var pubspec = Path.Combine(p.Output, "pubspec.yaml");
        Assert.Contains("uses-material-design: true", File.ReadAllText(pubspec));

        File.WriteAllText(pubspec, "name: x\ndependencies:\n  flutter:\n    sdk: flutter\n");
        p.Regenerate();
        var merged = File.ReadAllText(pubspec);
        Assert.Contains("  provider: ^6.0.0", merged);
        Assert.Contains("\nflutter:\n  uses-material-design: true", merged);
    }

    [Fact]
    public void EditedProxy_IsPreserved_StubIsRefreshed()
    {
        using var p = GeneratedProject.FromExample();
        var edited = p.Lib("services/order_service_proxy.dart");
        File.WriteAllText(edited, "// moja implementacja\n");
        var stub = p.Lib("services/order_list_service_proxy.dart");
        File.AppendAllText(stub, "// stara zaślepka\n");

        p.Regenerate();
        Assert.Equal("// moja implementacja\n", File.ReadAllText(edited));
        Assert.DoesNotContain("stara zaślepka", File.ReadAllText(stub));
    }

    [Fact]
    public void StaleGeneratedFiles_AreRemoved()
    {
        using var p = GeneratedProject.FromExample();
        File.WriteAllText(p.Lib("models/stale.dart"), "");
        File.WriteAllText(p.Lib("views/stale_view.dart"), "");
        File.WriteAllText(p.Lib("services/i_stale.dart"), "");
        File.WriteAllText(p.Lib("services/custom.dart"), "");
        File.WriteAllText(p.Lib("my_code.dart"), "");

        p.Regenerate();
        Assert.False(File.Exists(p.Lib("models/stale.dart")));
        Assert.False(File.Exists(p.Lib("views/stale_view.dart")));
        Assert.False(File.Exists(p.Lib("services/i_stale.dart")));
        Assert.True(File.Exists(p.Lib("services/custom.dart")));
        Assert.True(File.Exists(p.Lib("my_code.dart")));
    }
}
