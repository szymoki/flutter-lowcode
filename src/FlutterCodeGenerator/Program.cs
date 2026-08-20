using System.IO;
using System.Text;
using Antlr4.Runtime;
using FlutterCodeGenerator.Generator;
using FlutterCodeGenerator.Model.Ir;
using FlutterCodeGenerator.Model.Rsl;
using FlutterCodeGenerator.Parser;
using FlutterCodeGenerator.Translation;

void DumpParserTxtStructure(string filePath, Antlr4.Runtime.Tree.IParseTree tree, IReadOnlyList<UseCase> useCases, IReadOnlyList<NotionDef> notions)
{
    Console.WriteLine();
    Console.WriteLine("[Parser .txt structure] File: " + filePath);
    var inputText = tree.GetText();
    Console.WriteLine("  Parsed text length: " + inputText.Length + (inputText.Length > 120 ? " | preview: " + inputText.AsSpan(0, 120).ToString() + "..." : " | " + inputText));
    Console.WriteLine("  AST: UseCases=" + useCases.Count + ", Notions=" + notions.Count);
    foreach (var uc in useCases)
    {
        Console.WriteLine("    UseCase: " + uc.Name + " | MainScenario sentences=" + (uc.MainScenario?.AltSentences?.Count ?? 0) + ", scenarios=" + uc.Scenarios.Count);
        if (uc.MainScenario != null)
            foreach (var s in uc.MainScenario.AltSentences.Take(5))
                Console.WriteLine("      - " + SentenceSummary(s));
    }
    foreach (var n in notions)
    {
        if (n is ViewNotion v)
            Console.WriteLine("    Notion View: " + v.ViewType + " | " + string.Join(", ", v.NamesAndLabels.Select(x => x.Name + (x.UiLabel != null ? "(\"" + x.UiLabel + "\")" : ""))));
        else if (n is TriggerNotion t)
            Console.WriteLine("    Notion Trigger: " + (t.TriggerType ?? "?") + " | " + string.Join(", ", t.NamesAndLabels.Select(x => x.Name)));
        else if (n is DataNotion d)
            Console.WriteLine("    Notion Data: " + d.Name + " | attrs=" + string.Join(", ", d.Attributes.Select(a => a.Name + ":" + a.TypeKind)));
    }
    Console.WriteLine();
}

string SentenceSummary(Sentence s)
{
    if (s is SvoSentence sv)
        return "SVO " + sv.Label + " " + (sv.Step is UserStep u ? "User " + (u.Predicate is SelectPredicate sel ? "Select " + sel.Notion : u.Predicate is EnterPredicate e ? "Enter " + e.Notion : u.Predicate.GetType().Name) : "System " + (sv.Step is SystemStep sys ? sys.Predicate switch { ShowPredicate sh => "Show " + sh.Notion, ClosePredicate cl => "Close " + cl.Notion, _ => sys.Predicate.GetType().Name } : "?"));
    if (s is CondSentence c) return "Cond(" + c.Conditions.Count + ")";
    return s.GetType().Name;
}

// args[0] = folder z plikami .txt (RSL-bis), args[1] = folder wyjściowy (aplikacja Flutter)
var inputFolder = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "examples");
var outputFolder = args.Length > 1 ? args[1] : Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "flutter_web");

var useCases = new List<UseCase>();
var notions = new List<NotionDef>();

var rslFiles = Directory.Exists(inputFolder)
    ? Directory.GetFiles(inputFolder, "*.txt", SearchOption.AllDirectories)
    : Array.Empty<string>();

if (rslFiles.Length == 0)
{
    Console.WriteLine("Brak plików .txt w folderze: " + Path.GetFullPath(inputFolder));
    Console.WriteLine("Użycie: FlutterCodeGenerator <input_folder> <output_folder>");
    // Generuj minimalną aplikację bez wejścia
    outputFolder = Path.GetFullPath(outputFolder);
    Directory.CreateDirectory(outputFolder);
    var ir = new IrModel();
    var generator = new FlutterCodeGenerator.Generator.FlutterCodeGenerator();
    generator.Generate(ir, outputFolder);
    Console.WriteLine("Wygenerowano szkielet aplikacji Flutter w: " + outputFolder);
    return;
}

foreach (var file in rslFiles)
{
    var text = File.ReadAllText(file);
    var bytes = Encoding.UTF8.GetBytes(text);
    var input = new AntlrInputStream(new MemoryStream(bytes));
    var lexer = new RslBisLexer(input);
    var tokens = new CommonTokenStream(lexer);
    var parser = new RslBisParser(tokens);
    var tree = parser.start();

    var builder = new RslBisAstBuilder();
    builder.Visit(tree);
    DumpParserTxtStructure(file, tree, builder.UseCases, builder.Notions);
    useCases.AddRange(builder.UseCases);
    notions.AddRange(builder.Notions);
}

var translator = new RslToIrTranslator();
var irModel = translator.Translate(useCases, notions);

Directory.CreateDirectory(outputFolder);
var codeGenerator = new FlutterCodeGenerator.Generator.FlutterCodeGenerator();
codeGenerator.Generate(irModel, outputFolder);

Console.WriteLine($"Wygenerowano aplikację Flutter w: {Path.GetFullPath(outputFolder)}");
