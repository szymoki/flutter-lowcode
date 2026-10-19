namespace FlutterCodeGenerator.Tests;

public class Stage10ExtensionTests
{
    [Fact]
    public void ViewAndTriggerDefinitions_ProvideLabels()
    {
        var ir = IrHelpers.CompileSpec(TestPaths.ReadSpec("labels_date_close.txt"));
        var form = ir.View("VisitForm");
        Assert.Equal("Nowa wizyta", form.UiLabel);
        Assert.Equal("form", form.ViewType);
        Assert.Equal("Zapisz", form.Triggers.Single(t => t.Label == "save").UiLabel);
        Assert.Equal("Umow wizyte", ir.View("MainMenu").Triggers.Single(t => t.Label == "planVisit").UiLabel);

        using var p = GeneratedProject.FromSpec(TestPaths.ReadSpec("labels_date_close.txt"));
        var view = p.Read("views/visit_form_view.dart");
        Assert.Contains("AppBar(title: const Text('Nowa wizyta'))", view);
        Assert.Contains("child: const Text('Zapisz')", view);
        Assert.Contains("child: const Text('ok')", p.Read("views/visit_saved_message_view.dart"));
    }

    [Fact]
    public void DateField_UsesDatePicker()
    {
        using var p = GeneratedProject.FromSpec(TestPaths.ReadSpec("labels_date_close.txt"));
        var view = p.Read("views/visit_form_view.dart");
        Assert.Contains("showDatePicker(", view);
        Assert.Contains("final day = DateTime.tryParse(_visitDayController.text);", view);
    }

    [Fact]
    public void Close_HidesView()
    {
        using var p = GeneratedProject.FromSpec(TestPaths.ReadSpec("labels_date_close.txt"));
        Assert.Contains("_pVisitForm.hide();\n    _hideViews();\n    _pVisitSavedMessage.showVisitSavedMessage();",
            p.Read("use_cases/plan_visit_use_case.dart"));
    }

    [Fact]
    public void UnknownViewDefinition_IsWarning()
    {
        var spec = TestPaths.ReadSpec("labels_date_close.txt").Replace("visit saved message {Zapisano}", "missing view {Brak}");
        var result = Pipeline.Compile(spec);
        Assert.True(result.Success);
        Assert.Contains(result.Warnings, w => w.Contains("missing view"));
    }
}
