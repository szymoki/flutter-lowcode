namespace FlutterCodeGenerator.Tests;

internal static class TestPaths
{
    public static string RepoRoot { get; } = FindRepoRoot();

    public static string Example => Path.Combine(RepoRoot, "examples");
    public static string GoldenExample => Path.Combine(RepoRoot, "tests", "golden", "example");
    public static string Specs => Path.Combine(RepoRoot, "tests", "specs");

    public static string ReadSpec(string name) => File.ReadAllText(Path.Combine(Specs, name));

    public static string NewTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "fcg-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "examples", "example.txt")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Nie znaleziono katalogu repozytorium.");
    }
}
