using System.Reflection;
using System.Text;
using FlutterCodeGenerator.Model.Ir;

namespace FlutterCodeGenerator.Generator;

public class FlutterCodeGenerator
{
    private static string LoadTemplate(string name)
    {
        var asm = Assembly.GetExecutingAssembly();
        var resourceName = "FlutterCodeGenerator.Templates." + name;
        using var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException("Template not found: " + name);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static string FillTemplate(string template, params (string key, string value)[] replacements)
    {
        var result = template;
        foreach (var (key, value) in replacements)
            result = result.Replace("{{" + key + "}}", value ?? "");
        return result;
    }

    private static string ToSnakeCase(string s) => Naming.SnakeFromPascal(s);

    private static string ToPascalCase(string s) => Naming.Pascal(s);

    /// <summary>Pierwsza linia zaślepki proxy. Dopóki istnieje, generator może nadpisać plik.</summary>
    private const string ProxyStubMarker = "// WYGENEROWANO: plik jest nadpisywany przy każdej generacji, dopóki zawiera tę linię. Usuń ją po uzupełnieniu implementacji.";

    private static string ServiceVar(ServiceInterface s) => Naming.Camel(s.NotionName);
    private static string ServiceProxyClass(ServiceInterface s) => Naming.Pascal(s.NotionName) + "ServiceProxy";
    private static string ServiceInterfaceFile(ServiceInterface s) => "i_" + Naming.Snake(s.NotionName) + ".dart";
    private static string ServiceProxyFile(ServiceInterface s) => Naming.Snake(s.NotionName) + "_service_proxy.dart";
    private static string CheckResultClass(ServiceInterface s) => Naming.Pascal(s.NotionName) + "CheckResult";
    private static string ModelImport(string typeLabel, string prefix = "../models/") => "import '" + prefix + ToSnakeCase(typeLabel) + ".dart';";

    /// <summary>Typ zwracany operacji usługi (bez opakowania w Future).</summary>
    private static string ServiceResultType(ServiceInterface si, SOperation m) => m.Type switch
    {
        PredicateType.Check => CheckResultClass(si),
        PredicateType.Read => Naming.Pascal(si.NotionName),
        _ => "void"
    };

    private static string DartType(string typeKind)
    {
        return typeKind.ToLowerInvariant() switch
        {
            "text" => "String",
            "integer" => "int",
            "float" => "double",
            "boolean" => "bool",
            "time" => "String",
            "date" => "DateTime",
            "location" => "String",
            "file" => "String",
            "image" => "String",
            _ => ToPascalCase(typeKind)
        };
    }

    private HashSet<string> AggregateLabels = new();

    public void Generate(IrModel ir, string outputFolder)
    {
        AggregateLabels = ir.ViewModel.Aggregates.Select(a => a.Label).ToHashSet();
        var libPath = Path.Combine(outputFolder, "lib");
        Directory.CreateDirectory(libPath);

        var modelsPath = Path.Combine(libPath, "models");
        var servicesPath = Path.Combine(libPath, "services");
        var presentersPath = Path.Combine(libPath, "presenters");
        var useCasesPath = Path.Combine(libPath, "use_cases");
        var controllersPath = Path.Combine(libPath, "controllers");
        var viewsPath = Path.Combine(libPath, "views");

        RemoveStaleFiles(libPath);

        Directory.CreateDirectory(modelsPath);
        Directory.CreateDirectory(servicesPath);
        Directory.CreateDirectory(presentersPath);
        Directory.CreateDirectory(useCasesPath);
        Directory.CreateDirectory(controllersPath);
        Directory.CreateDirectory(viewsPath);

        foreach (var agg in ir.ViewModel.Aggregates)
            WriteDataAggregate(agg, modelsPath);

        foreach (var si in ir.Services)
        {
            if (si.Methods.Any(m => m.Type == PredicateType.Check))
                WriteCheckResultStub(CheckResultClass(si), modelsPath);
            WriteServiceInterface(si, servicesPath, ir);
        }

        foreach (var pres in ir.Presenters)
            WritePresenter(pres, presentersPath, ir);

        foreach (var uc in ir.UseCases)
            WriteUseCase(uc, useCasesPath, ir);

        foreach (var ctrl in ir.Controllers)
            WriteController(ctrl, controllersPath);

        foreach (var vw in ir.Views)
            WriteView(vw, viewsPath);

        WriteMain(ir, libPath);
        WriteApp(ir, libPath);
        WritePubspec(outputFolder);
    }

    /// <summary>
    /// Usuwa pliki z poprzedniej generacji, żeby nie zostawały klasy pojęć usuniętych ze specyfikacji.
    /// Nie rusza zaślepek proxy ani plików spoza katalogów generatora.
    /// </summary>
    private static void RemoveStaleFiles(string libPath)
    {
        foreach (var dir in new[] { "models", "views", "controllers", "presenters", "use_cases" })
        {
            var path = Path.Combine(libPath, dir);
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        var servicesPath = Path.Combine(libPath, "services");
        if (Directory.Exists(servicesPath))
            foreach (var file in Directory.GetFiles(servicesPath, "i_*.dart"))
                File.Delete(file);
    }

    private void WriteDataAggregate(DataAggregate agg, string modelsPath)
    {
        var isList = agg.Items.Count == 1 && agg.Items[0].TypeKind == agg.Label;
        if (isList)
        {
            var itemType = agg.Items[0].TypeKind;
            var content = FillTemplate(LoadTemplate("ModelList.txt"),
                ("ClassName", agg.Label),
                ("ItemType", DartType(itemType)));
            File.WriteAllText(Path.Combine(modelsPath, ToSnakeCase(agg.Label) + ".dart"), content);
            return;
        }

        var fields = agg.Items.Select(i => $"  final {DartType(i.TypeKind)} {char.ToLowerInvariant(i.Label[0]) + i.Label[1..]};").ToList();
        var ctorParams = agg.Items.Select(i => "required this." + char.ToLowerInvariant(i.Label[0]) + i.Label[1..]).ToList();
        var copyParams = agg.Items.Select(i =>
        {
            var name = char.ToLowerInvariant(i.Label[0]) + i.Label[1..];
            return $"{DartType(i.TypeKind)}? {name}";
        }).ToList();
        var nestedTypes = agg.Items.Select(i => DartType(i.TypeKind))
            .Where(t => t != agg.Label && AggregateLabels.Contains(t))
            .Distinct().OrderBy(t => t, StringComparer.Ordinal).ToList();
        var modelImports = nestedTypes.Count > 0
            ? string.Join("\n", nestedTypes.Select(t => "import '" + ToSnakeCase(t) + ".dart';")) + "\n\n"
            : "";
        var copyAssign = agg.Items.Select(i =>
        {
            var name = char.ToLowerInvariant(i.Label[0]) + i.Label[1..];
            return $"{name}: {name} ?? this.{name}";
        }).ToList();

        var constructor = ctorParams.Count == 0
            ? $"  const {agg.Label}();"
            : "  const " + agg.Label + "({\n    " + string.Join(",\n    ", ctorParams) + "\n  });";
        var copyAssignInline = copyAssign.Count == 0 ? "" : "\n      " + string.Join(",\n      ", copyAssign) + "\n    ";

        var objectContent = FillTemplate(LoadTemplate("ModelObject.txt"),
            ("ClassName", agg.Label),
            ("ModelImports", modelImports),
            ("Fields", string.Join("\n", fields)),
            ("Constructor", constructor),
            ("CopyParams", copyParams.Count > 0 ? "{" + string.Join(", ", copyParams) + "}" : ""),
            ("CopyAssignInline", copyAssignInline));
        File.WriteAllText(Path.Combine(modelsPath, ToSnakeCase(agg.Label) + ".dart"), objectContent);
    }

    private void WriteCheckResultStub(string className, string modelsPath)
    {
        var path = Path.Combine(modelsPath, ToSnakeCase(className) + ".dart");
        if (File.Exists(path)) return;
        File.WriteAllText(path, "// Generated from IR - stub for check result\nclass " + className + " {}\n");
    }

    private void WriteServiceInterface(ServiceInterface si, string servicesPath, IrModel ir)
    {
        var ifaceName = si.Label;
        var ifaceFile = ServiceInterfaceFile(si);
        var usedTypes = new HashSet<string>();
        foreach (var m in si.Methods)
        {
            var resultType = ServiceResultType(si, m);
            if (resultType != "void") usedTypes.Add(resultType);
            foreach (var p in m.Parameters)
                if (AggregateLabels.Contains(p.Type)) usedTypes.Add(p.Type);
        }
        var modelImportsStr = usedTypes.Count > 0
            ? string.Join("\n", usedTypes.OrderBy(t => t, StringComparer.Ordinal).Select(t => ModelImport(t))) + "\n"
            : "";

        string Signature(SOperation m)
        {
            var pars = string.Join(", ", m.Parameters.Select(p => $"{p.Type} {char.ToLowerInvariant(p.Label[0]) + p.Label[1..]}"));
            return $"Future<{ServiceResultType(si, m)}> {char.ToLowerInvariant(m.Label[0]) + m.Label[1..]}({pars})";
        }

        var methods = si.Methods.Select(m => "  " + Signature(m) + ";").ToList();
        var content = FillTemplate(LoadTemplate("ServiceInterface.txt"),
            ("InterfaceName", ifaceName),
            ("ModelImports", modelImportsStr),
            ("Methods", string.Join("\n", methods)));
        File.WriteAllText(Path.Combine(servicesPath, ifaceFile), content);

        var proxyPath = Path.Combine(servicesPath, ServiceProxyFile(si));
        if (File.Exists(proxyPath) && !File.ReadAllText(proxyPath).StartsWith(ProxyStubMarker, StringComparison.Ordinal))
            return;
        var proxyMethods = si.Methods.Select(m =>
            "  @override\n  " + Signature(m) + " async {\n    throw UnimplementedError();\n  }\n").ToList();
        var proxyContent = FillTemplate(LoadTemplate("ServiceProxy.txt"),
            ("Marker", ProxyStubMarker),
            ("InterfaceFile", ifaceFile),
            ("ProxyClass", ServiceProxyClass(si)),
            ("InterfaceName", ifaceName),
            ("ModelImports", modelImportsStr),
            ("ProxyMethods", string.Join("\n", proxyMethods)));
        File.WriteAllText(proxyPath, proxyContent);
    }

    private void WritePresenter(PresenterClass pres, string presentersPath, IrModel ir)
    {
        var modelNames = ir.ViewModel.Aggregates.Select(a => a.Label).ToHashSet();
        var usedTypes = pres.Methods.SelectMany(m => m.Parameters).Where(p => modelNames.Contains(p.Type)).Select(p => p.Type).Distinct().ToList();
        var modelImports = usedTypes.Select(t => "import '../models/" + ToSnakeCase(t) + ".dart';").ToList();
        var fields = pres.Methods.SelectMany(m => m.Parameters).DistinctBy(p => p.Type).Select(p =>
            $"  {p.Type}? _{char.ToLowerInvariant(p.Label[0]) + p.Label[1..]};\n  {p.Type}? get {char.ToLowerInvariant(p.Label[0]) + p.Label[1..]} => _{char.ToLowerInvariant(p.Label[0]) + p.Label[1..]};").ToList();
        // Bypass: Dart has no overloading. Group by method name and emit one method per name with optional params.
        var methodsByLabel = pres.Methods.GroupBy(m => m.Label).ToList();
        var methods = methodsByLabel.Select(g =>
        {
            var allParams = g.SelectMany(m => m.Parameters).DistinctBy(p => p.Label).ToList();
            var pars = allParams.Count > 0
                ? string.Join(", ", allParams.Select(p => $"{p.Type}? {char.ToLowerInvariant(p.Label[0]) + p.Label[1..]}"))
                : "";
            var assigns = allParams.Select(p =>
            {
                var name = char.ToLowerInvariant(p.Label[0]) + p.Label[1..];
                return $"    if ({name} != null) _{name} = {name};";
            }).ToList();
            var body = assigns.Count > 0 ? string.Join("\n", assigns) + "\n    " : "";
            return "  void " + g.Key + "(" + pars + ") {\n    " + body + "notifyListeners();\n  }\n";
        }).ToList();
        var content = FillTemplate(LoadTemplate("Presenter.txt"),
            ("ClassName", pres.Label),
            ("ModelImports", modelImports.Count > 0 ? string.Join("\n", modelImports) + "\n" : ""),
            ("Fields", string.Join("\n", fields)),
            ("Methods", string.Join("\n", methods)));
        File.WriteAllText(Path.Combine(presentersPath, ToSnakeCase(pres.Label) + "_presenter.dart"), content);
    }

    private void WriteUseCase(UseCaseClass uc, string useCasesPath, IrModel ir)
    {
        var ctorParams = new List<string>();
        foreach (var p in uc.Presenters)
            ctorParams.Add($"required {p.Label} {char.ToLowerInvariant(p.Label[0]) + p.Label[1..]}");
        foreach (var s in uc.Services)
            ctorParams.Add($"required {s.Label} {ServiceVar(s)}");
        var constructorParams = "{ " + string.Join(", ", ctorParams) + " }";

        var initParts = new List<string>();
        foreach (var p in uc.Presenters)
        {
            var varName = char.ToLowerInvariant(p.Label[0]) + p.Label[1..];
            initParts.Add($"_{varName} = {varName}");
        }
        foreach (var s in uc.Services)
            initParts.Add($"_{ServiceVar(s)} = {ServiceVar(s)}");
        var initializerList = initParts.Count > 0 ? ": " + string.Join(",\n      ", initParts) + ";" : ";";

        var operationMethodsCode = string.Join("\n", uc.Operations.Select(op => $"  void {op.Label}() {{}}"));

        var presenterImports = uc.Presenters.Select(p => "import '../presenters/" + ToSnakeCase(p.Label) + "_presenter.dart';").ToList();
        var serviceImports = uc.Services.Select(s => "import '../services/" + ServiceInterfaceFile(s) + "';").ToList();
        var presenterFields = string.Join("\n", uc.Presenters.Select(p => $"  final {p.Label} _{char.ToLowerInvariant(p.Label[0]) + p.Label[1..]};"));
        var serviceFields = string.Join("\n", uc.Services.Select(s => $"  final {s.Label} _{ServiceVar(s)};"));

        var content = FillTemplate(LoadTemplate("UseCase.txt"),
            ("ClassName", uc.Label),
            ("PresenterImports", string.Join("\n", presenterImports)),
            ("ServiceImports", string.Join("\n", serviceImports)),
            ("PresenterFields", presenterFields),
            ("ServiceFields", serviceFields),
            ("ConstructorParams", constructorParams),
            ("InitializerList", initializerList),
            ("OperationMethods", operationMethodsCode));
        File.WriteAllText(Path.Combine(useCasesPath, ToSnakeCase(uc.Label) + "_use_case.dart"), content);
    }

    private static string UseCaseVar(UseCaseClass uc) => Naming.LowerFirst(uc.Label) + "UseCase";
    private static string ControllerFile(ControllerClass c) => ToSnakeCase(c.Label[1..]) + "_controller.dart";

    private void WriteController(ControllerClass ctrl, string controllersPath)
    {
        var useCases = ctrl.TargetUseCases.ToList();
        var imports = useCases.Select(u => "import '../use_cases/" + ToSnakeCase(u.Label) + "_use_case.dart';");
        var fields = useCases.Select(u => $"  final {u.Label}UseCase _{UseCaseVar(u)};");
        var ctorParams = useCases.Count > 0
            ? "{" + string.Join(", ", useCases.Select(u => $"required {u.Label}UseCase {UseCaseVar(u)}")) + "}"
            : "";
        var initializer = useCases.Count > 0
            ? " : " + string.Join(", ", useCases.Select(u => $"_{UseCaseVar(u)} = {UseCaseVar(u)}"))
            : "";
        var methods = ctrl.Operations.Select(op =>
            $"  void {op.Label}() {{ _{UseCaseVar(op.TargetUseCase!)}.{op.Target!.Label}(); }}");
        var content = FillTemplate(LoadTemplate("Controller.txt"),
            ("UseCaseImports", string.Join("\n", imports)),
            ("ControllerName", ctrl.Label),
            ("Fields", string.Join("\n", fields)),
            ("ConstructorParams", ctorParams),
            ("InitializerList", initializer),
            ("Methods", string.Join("\n", methods)));
        File.WriteAllText(Path.Combine(controllersPath, ControllerFile(ctrl)), content);
    }

    private void WriteView(ViewWidget vw, string viewsPath)
    {
        var viewName = vw.Label;
        var controllerName = vw.Controller?.Label ?? "C" + viewName;
        var presenterName = vw.Presenter?.Label ?? "P" + viewName;
        var editableItems = vw.Data.SelectMany(d => d.Items.Where(i => i.EditableStateItem)).ToList();
        var fields = editableItems.Select(i =>
            $"  final _{(char.ToLowerInvariant(i.Label[0]) + i.Label[1..])}Controller = TextEditingController();").ToList();
        var formFields = editableItems.Select(i => FormFieldWidget(i)).ToList();
        var triggers = vw.Triggers.Select(t =>
        {
            var methodName = "on" + char.ToUpperInvariant(t.Label[0]) + t.Label[1..];
            return $"                ElevatedButton(onPressed: () => widget.controller.{methodName}(), child: const Text('{t.Label}')),";
        }).ToList();
        var bodyChildren = string.Join("\n", formFields.Concat(triggers));
        var disposeCalls = string.Join("\n", editableItems.Select(i => "    _" + (char.ToLowerInvariant(i.Label[0]) + i.Label[1..]) + "Controller.dispose();"));
        var content = FillTemplate(LoadTemplate("View.txt"),
            ("ViewName", viewName),
            ("ControllerName", controllerName),
            ("ControllerSnake", ToSnakeCase(controllerName[1..])),
            ("PresenterName", presenterName),
            ("PresenterSnake", ToSnakeCase(presenterName)),
            ("Fields", string.Join("\n", fields)),
            ("DisposeCalls", disposeCalls),
            ("BodyChildren", bodyChildren));
        File.WriteAllText(Path.Combine(viewsPath, ToSnakeCase(viewName) + "_view.dart"), content);
    }

    private static string FormFieldWidget(DataItem item)
    {
        var varName = char.ToLowerInvariant(item.Label[0]) + item.Label[1..];
        var label = item.Label;
        var tk = (item.TypeKind ?? "").ToLowerInvariant();
        if (tk == "boolean")
            return "                Padding(padding: const EdgeInsets.only(bottom: 8.0), child: StatefulBuilder(builder: (context, setInnerState) => SwitchListTile(title: const Text('" + label + "'), value: _" + varName + "Controller.text == 'true', onChanged: (v) { setInnerState(() => _" + varName + "Controller.text = v.toString()); }))),";
        var keyboard = tk switch { "integer" => " keyboardType: TextInputType.number,", "float" => " keyboardType: const TextInputType.numberWithOptions(decimal: true),", _ => "" };
        return "                Padding(padding: const EdgeInsets.only(bottom: 8.0), child: TextField(controller: _" + varName + "Controller, decoration: const InputDecoration(labelText: '" + label + "', border: OutlineInputBorder())," + keyboard + ")),";
    }

    private void WriteMain(IrModel ir, string libPath)
    {
        var presenterImports = ir.Presenters.Select(p => "import 'presenters/" + ToSnakeCase(p.Label) + "_presenter.dart';").ToList();
        var providers = ir.Presenters.Select(p => $"        ChangeNotifierProvider(create: (_) => {p.Label}()),").ToList();
        var content = FillTemplate(LoadTemplate("Main.txt"),
            ("PresenterImports", string.Join("\n", presenterImports)),
            ("Providers", string.Join("\n", providers)));
        File.WriteAllText(Path.Combine(libPath, "main.dart"), content);
    }

    private void WriteApp(IrModel ir, string libPath)
    {
        if (ir.UseCases.Count == 0 || ir.Views.Count == 0)
        {
            var simpleApp = "// Generated from IR\nimport 'package:flutter/material.dart';\nimport 'package:provider/provider.dart';\n\nclass App extends StatelessWidget {\n  const App({super.key});\n  @override\n  Widget build(BuildContext context) {\n    return MaterialApp(\n      title: 'Generated App',\n      home: const Scaffold(body: Center(child: Text('Generated Flutter App'))),\n    );\n  }\n}\n";
            File.WriteAllText(Path.Combine(libPath, "app.dart"), simpleApp);
            return;
        }

        string UcVar(UseCaseClass u) => "_" + char.ToLowerInvariant(u.Label[0]) + u.Label[1..] + "UseCase";

        var presenterImports = ir.Presenters.Select(p => "import 'presenters/" + ToSnakeCase(p.Label) + "_presenter.dart';").ToList();
        var useCaseImports = ir.UseCases.Select(uc => "import 'use_cases/" + ToSnakeCase(uc.Label) + "_use_case.dart';").ToList();
        var controllerImports = ir.Views.Where(v => v.Controller != null).Select(v => v.Controller!)
            .Distinct().Select(c => "import 'controllers/" + ControllerFile(c) + "';").ToList();
        var viewImports = ir.Views.Select(v => "import 'views/" + ToSnakeCase(v.Label) + "_view.dart';").ToList();
        var allServices = ir.UseCases.SelectMany(u => u.Services).DistinctBy(s => s.Label).ToList();
        var serviceProxyImports = allServices.Select(s => "import 'services/" + ServiceProxyFile(s) + "';").ToList();

        var useCaseFields = ir.UseCases.Select(uc => "  " + uc.Label + "UseCase? " + UcVar(uc) + ";").ToList();
        var controllerFields = ir.Views.Where(v => v.Controller != null).Select(v =>
        {
            var c = v.Controller!;
            var ctrlVar = char.ToLowerInvariant(c.Label[0]) + c.Label[1..];
            return "  late " + c.Label + " _" + ctrlVar + ";";
        }).ToList();

        var allPresenters = ir.UseCases.SelectMany(u => u.Presenters).DistinctBy(p => p.Label).ToList();
        var presenterReads = allPresenters.Select(p =>
        {
            var varName = char.ToLowerInvariant(p.Label[0]) + p.Label[1..];
            return "      final " + varName + " = context.read<" + p.Label + ">();";
        }).ToList();
        var serviceCreations = allServices.Select(s => "      final " + ServiceVar(s) + " = " + ServiceProxyClass(s) + "();").ToList();

        var useCaseCreations = ir.UseCases.Select(uc =>
        {
            var ucVar = UcVar(uc);
            var useCaseType = uc.Label + "UseCase";
            var useCaseParams = new List<string>();
            foreach (var p in uc.Presenters)
                useCaseParams.Add(char.ToLowerInvariant(p.Label[0]) + p.Label[1..] + ": " + (char.ToLowerInvariant(p.Label[0]) + p.Label[1..]));
            foreach (var s in uc.Services)
                useCaseParams.Add(ServiceVar(s) + ": " + ServiceVar(s));
            return "      " + ucVar + " = " + useCaseType + "(" + string.Join(", ", useCaseParams) + ");";
        }).ToList();
        var firstUcVar = UcVar(ir.UseCases.First());
        var controllerCreations = ir.Views.Where(v => v.Controller != null).Select(v =>
        {
            var c = v.Controller!;
            var ctrlVar = char.ToLowerInvariant(c.Label[0]) + c.Label[1..];
            var args = c.TargetUseCases.Select(u => UseCaseVar(u) + ": " + UcVar(u) + "!");
            return "      _" + ctrlVar + " = " + c.Label + "(" + string.Join(", ", args) + ");";
        }).ToList();

        var viewTiles = ir.Presenters
            .Where(p => ir.Views.Any(v => v.Presenter?.Label == p.Label && v.Controller != null))
            .Select(p =>
            {
                var v = ir.Views.First(v => v.Presenter?.Label == p.Label && v.Controller != null);
                var viewName = v.Label + "View";
                var ctrlVar = char.ToLowerInvariant(v.Controller!.Label[0]) + v.Controller!.Label[1..];
                var title = p.Label.StartsWith("P") && p.Label.Length > 1 ? p.Label[1..] : p.Label;
                return "            ListTile(title: const Text('" + title + "'), onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => " + viewName + "(controller: _" + ctrlVar + ")))),";
            }).ToList();

        var content = FillTemplate(LoadTemplate("App.txt"),
            ("PresenterImports", string.Join("\n", presenterImports)),
            ("UseCaseImports", string.Join("\n", useCaseImports)),
            ("ControllerImports", string.Join("\n", controllerImports)),
            ("ViewImports", string.Join("\n", viewImports)),
            ("ServiceProxyImports", string.Join("\n", serviceProxyImports)),
            ("UseCaseFields", string.Join("\n", useCaseFields)),
            ("UseCaseNullCheck", firstUcVar + " == null"),
            ("ControllerFields", string.Join("\n", controllerFields)),
            ("PresenterReads", string.Join("\n", presenterReads)),
            ("ServiceCreations", string.Join("\n", serviceCreations)),
            ("UseCaseCreations", string.Join("\n", useCaseCreations)),
            ("ControllerCreations", string.Join("\n", controllerCreations)),
            ("ViewTiles", string.Join("\n", viewTiles)));
        File.WriteAllText(Path.Combine(libPath, "app.dart"), content);
    }

    private void WritePubspec(string outputFolder)
    {
        var path = Path.Combine(outputFolder, "pubspec.yaml");
        if (!File.Exists(path))
        {
            File.WriteAllText(path, LoadTemplate("Pubspec.txt"));
            return;
        }

        var existing = File.ReadAllText(path);
        if (!existing.Contains("provider:"))
        {
            // Insert under dependencies (after first "sdk: flutter" line), not at end of file
            var idx = existing.IndexOf("sdk: flutter", StringComparison.Ordinal);
            if (idx >= 0)
            {
                var endOfLine = existing.IndexOf("\n", idx, StringComparison.Ordinal);
                if (endOfLine < 0) endOfLine = existing.Length;
                existing = existing[..(endOfLine + 1)] + "  provider: ^6.0.0\n" + existing[(endOfLine + 1)..];
            }
            else
                existing = existing.TrimEnd() + "\n  provider: ^6.0.0\n";
        }
        if (!existing.Contains("uses-material-design:"))
        {
            // Sekcja najwyższego poziomu „flutter:” (bez wcięcia), a nie zależność „flutter:” z dependencies.
            var lines = existing.Split('\n').ToList();
            var sectionIdx = lines.FindIndex(l => l.TrimEnd() == "flutter:");
            if (sectionIdx >= 0)
                lines.Insert(sectionIdx + 1, "  uses-material-design: true");
            else
            {
                while (lines.Count > 0 && lines[^1].Trim() == "") lines.RemoveAt(lines.Count - 1);
                lines.AddRange(new[] { "", "flutter:", "  uses-material-design: true", "" });
            }
            existing = string.Join("\n", lines);
        }
        File.WriteAllText(path, existing);
    }
}
