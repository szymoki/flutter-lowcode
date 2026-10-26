namespace FlutterCodeGenerator.Tests;

/// <summary>Uruchamia cały potok na podanej specyfikacji w katalogu tymczasowym.</summary>
internal sealed class GeneratedProject : IDisposable
{
    private readonly string _input;
    public string Output { get; }
    public PipelineResult Result { get; private set; } = null!;

    private GeneratedProject()
    {
        _input = TestPaths.NewTempDir();
        Output = TestPaths.NewTempDir();
    }

    public static GeneratedProject FromSpec(string specText)
    {
        var p = new GeneratedProject();
        File.WriteAllText(Path.Combine(p._input, "spec.txt"), specText);
        p.Regenerate();
        return p;
    }

    public static GeneratedProject FromExample() =>
        FromSpec(File.ReadAllText(Path.Combine(TestPaths.Example, "example.txt")));

    public void Regenerate()
    {
        Result = Pipeline.Run(_input, Output);
        Assert.True(Result.Success, string.Join("\n", Result.Errors));
    }

    public string Lib(string relative) => Path.Combine(Output, "lib", relative);
    public string Read(string relative) => File.ReadAllText(Lib(relative));

    public void Dispose()
    {
        Directory.Delete(_input, true);
        Directory.Delete(Output, true);
    }
}
