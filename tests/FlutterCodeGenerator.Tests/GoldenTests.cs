namespace FlutterCodeGenerator.Tests;

/// <summary>
/// Porównuje wynik generacji dla examples/ z wzorcem w tests/golden/example.
/// Aktualizacja wzorca: UPDATE_GOLDEN=1 dotnet test src
/// </summary>
public class GoldenTests
{
    [Fact]
    public void Example_MatchesGolden()
    {
        var output = TestPaths.NewTempDir();
        try
        {
            var result = Pipeline.Run(TestPaths.Example, output);
            Assert.True(result.Success, string.Join("\n", result.Errors));

            var actual = CollectFiles(output);
            if (Environment.GetEnvironmentVariable("UPDATE_GOLDEN") == "1")
            {
                if (Directory.Exists(TestPaths.GoldenExample))
                    Directory.Delete(TestPaths.GoldenExample, recursive: true);
                foreach (var (rel, content) in actual)
                {
                    var path = Path.Combine(TestPaths.GoldenExample, rel);
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    File.WriteAllText(path, content);
                }
                return;
            }

            var expected = CollectFiles(TestPaths.GoldenExample);
            Assert.Equal(expected.Keys.OrderBy(k => k, StringComparer.Ordinal), actual.Keys.OrderBy(k => k, StringComparer.Ordinal));
            foreach (var (rel, content) in expected)
                Assert.True(content == actual[rel], "Plik różni się od wzorca: " + rel);
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    private static Dictionary<string, string> CollectFiles(string root)
    {
        var files = Directory.GetFiles(Path.Combine(root, "lib"), "*.dart", SearchOption.AllDirectories)
            .Append(Path.Combine(root, "pubspec.yaml"));
        return files.ToDictionary(
            f => Path.GetRelativePath(root, f).Replace('\\', '/'),
            f => File.ReadAllText(f).Replace("\r\n", "\n"));
    }
}
