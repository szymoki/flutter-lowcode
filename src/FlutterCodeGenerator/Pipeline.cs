using System.Text;
using Antlr4.Runtime;
using FlutterCodeGenerator.Model.Ir;
using FlutterCodeGenerator.Model.Rsl;
using FlutterCodeGenerator.Parser;
using FlutterCodeGenerator.Translation;

namespace FlutterCodeGenerator;

public class PipelineResult
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
    public IrModel? Ir { get; set; }
    public bool Success => Errors.Count == 0;
}

/// <summary>
/// Potok kompilacji: parsowanie plików RSL-bis → translacja do IR → generacja kodu Flutter.
/// </summary>
public static class Pipeline
{
    public static PipelineResult Run(string inputFolder, string outputFolder, TextWriter? log = null)
    {
        log ??= TextWriter.Null;
        var result = new PipelineResult();

        var rslFiles = Directory.Exists(inputFolder)
            ? Directory.GetFiles(inputFolder, "*.txt", SearchOption.AllDirectories)
            : Array.Empty<string>();
        // Kolejność plików wpływa na kolejność przypadków użycia w IR, więc nie może zależeć od systemu plików.
        Array.Sort(rslFiles, StringComparer.Ordinal);

        if (rslFiles.Length == 0)
        {
            result.Errors.Add("Brak plików .txt w folderze: " + Path.GetFullPath(inputFolder));
            return result;
        }

        var useCases = new List<UseCase>();
        var notions = new List<NotionDef>();
        foreach (var file in rslFiles)
        {
            var (fileUseCases, fileNotions) = Parse(File.ReadAllText(file), Path.GetRelativePath(inputFolder, file), result.Errors, log);
            useCases.AddRange(fileUseCases);
            notions.AddRange(fileNotions);
        }
        if (!result.Success)
            return result;

        var translator = new RslToIrTranslator();
        var irModel = translator.Translate(useCases, notions);
        result.Warnings.AddRange(translator.Warnings);
        result.Errors.AddRange(translator.Errors);
        if (!result.Success)
            return result;
        result.Ir = irModel;

        Directory.CreateDirectory(outputFolder);
        var generator = new Generator.FlutterCodeGenerator();
        generator.Generate(irModel, outputFolder);
        result.Warnings.AddRange(generator.Warnings);

        log.WriteLine($"Wygenerowano aplikację Flutter w: {Path.GetFullPath(outputFolder)}");
        return result;
    }

    /// <summary>Parsuje jeden plik RSL-bis i buduje model RSL.</summary>
    public static (IReadOnlyList<UseCase> UseCases, IReadOnlyList<NotionDef> Notions) Parse(
        string text, string fileName, List<string> errors, TextWriter? log = null)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        var input = new AntlrInputStream(new MemoryStream(bytes));
        var collector = new SyntaxErrorCollector(fileName, errors);
        var lexer = new RslBisLexer(input);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(collector);
        var tokens = new CommonTokenStream(lexer);
        var parser = new RslBisParser(tokens);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(collector);
        var tree = parser.start();
        // Drzewo po błędach jest naprawione heurystycznie przez ANTLR i może nie mieć
        // wymaganych węzłów, więc nie budujemy z niego modelu.
        if (collector.Count > 0)
            return (Array.Empty<UseCase>(), Array.Empty<NotionDef>());

        var builder = new RslBisAstBuilder();
        builder.Visit(tree);
        if (log != null)
            DumpParserTxtStructure(log, fileName, tree, builder.UseCases, builder.Notions);
        return (builder.UseCases, builder.Notions);
    }

    /// <summary>Parsuje i tłumaczy tekst specyfikacji do IR (bez generacji kodu).</summary>
    public static PipelineResult Compile(string text, string fileName = "spec.txt")
    {
        var result = new PipelineResult();
        var (useCases, notions) = Parse(text, fileName, result.Errors);
        if (result.Success)
        {
            var translator = new RslToIrTranslator();
            result.Ir = translator.Translate(useCases.ToList(), notions.ToList());
            result.Warnings.AddRange(translator.Warnings);
            result.Errors.AddRange(translator.Errors);
        }
        return result;
    }

    private static void DumpParserTxtStructure(TextWriter log, string filePath, Antlr4.Runtime.Tree.IParseTree tree, IReadOnlyList<UseCase> useCases, IReadOnlyList<NotionDef> notions)
    {
        log.WriteLine();
        log.WriteLine("[Parser .txt structure] File: " + filePath);
        var inputText = tree.GetText();
        log.WriteLine("  Parsed text length: " + inputText.Length + (inputText.Length > 120 ? " | preview: " + inputText.AsSpan(0, 120).ToString() + "..." : " | " + inputText));
        log.WriteLine("  AST: UseCases=" + useCases.Count + ", Notions=" + notions.Count);
        foreach (var uc in useCases)
        {
            log.WriteLine("    UseCase: " + uc.Name + " | MainScenario sentences=" + (uc.MainScenario?.AltSentences?.Count ?? 0) + ", scenarios=" + uc.Scenarios.Count);
            if (uc.MainScenario != null)
                foreach (var s in uc.MainScenario.AltSentences.Take(5))
                    log.WriteLine("      - " + SentenceSummary(s));
        }
        foreach (var n in notions)
        {
            if (n is ViewNotion v)
                log.WriteLine("    Notion View: " + v.ViewType + " | " + string.Join(", ", v.NamesAndLabels.Select(x => x.Name + (x.UiLabel != null ? "(\"" + x.UiLabel + "\")" : ""))));
            else if (n is TriggerNotion t)
                log.WriteLine("    Notion Trigger: " + (t.TriggerType ?? "?") + " | " + string.Join(", ", t.NamesAndLabels.Select(x => x.Name)));
            else if (n is DataNotion d)
                log.WriteLine("    Notion Data: " + d.Name + " | attrs=" + string.Join(", ", d.Attributes.Select(a => a.Name + ":" + a.TypeKind)));
        }
        log.WriteLine();
    }

    private static string SentenceSummary(Sentence s)
    {
        if (s is SvoSentence sv)
            return "SVO " + sv.Label + " " + (sv.Step is UserStep u ? "User " + (u.Predicate is SelectPredicate sel ? "Select " + sel.Notion : u.Predicate is EnterPredicate e ? "Enter " + e.Notion : u.Predicate.GetType().Name) : "System " + (sv.Step is SystemStep sys ? sys.Predicate switch { ShowPredicate sh => "Show " + sh.Notion, ClosePredicate cl => "Close " + cl.Notion, _ => sys.Predicate.GetType().Name } : "?"));
        if (s is CondSentence c) return "Cond(" + c.Conditions.Count + ")";
        return s.GetType().Name;
    }
}
