using System.Globalization;
using System.Text.RegularExpressions;

namespace FlutterCodeGenerator;

/// <summary>
/// Konwersje nazw. Nazwy pojęć RSL to słowa rozdzielone spacjami („order search”);
/// identyfikatory IR są już w PascalCase („OrderSearch”).
/// </summary>
public static class Naming
{
    private static string[] Words(string s) =>
        s.Trim().Split(new[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);

    /// <summary>„order search” → „OrderSearch”.</summary>
    public static string Pascal(string notion) =>
        string.Concat(Words(notion).Select(w => char.ToUpper(w[0], CultureInfo.InvariantCulture) + w[1..].ToLowerInvariant()));

    /// <summary>„order search” → „orderSearch”.</summary>
    public static string Camel(string notion) => LowerFirst(Pascal(notion));

    /// <summary>„order search” → „order_search”.</summary>
    public static string Snake(string notion) =>
        string.Join("_", Words(notion).Select(w => w.ToLowerInvariant()));

    /// <summary>Identyfikator w PascalCase → snake_case: „OrderSearch” → „order_search”.</summary>
    public static string SnakeFromPascal(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier)) return "";
        var words = Regex.Replace(identifier, "([a-z])([A-Z])", "$1_$2").Split('_', ' ');
        return string.Join("_", words.Select(w => w.ToLowerInvariant()));
    }

    private static readonly HashSet<string> DartReservedWords = new(StringComparer.Ordinal)
    {
        "abstract", "as", "assert", "async", "await", "base", "break", "case", "catch", "class", "const",
        "continue", "covariant", "default", "deferred", "do", "dynamic", "else", "enum", "export", "extends",
        "extension", "external", "factory", "false", "final", "finally", "for", "Function", "get", "hide",
        "if", "implements", "import", "in", "interface", "is", "late", "library", "mixin", "new", "null",
        "of", "on", "operator", "part", "required", "rethrow", "return", "sealed", "set", "show", "static",
        "super", "switch", "sync", "this", "throw", "true", "try", "type", "typedef", "var", "void", "when",
        "while", "with", "yield",
        // Składowe, które ma każdy enum Darta.
        "index", "values", "name", "hashCode", "runtimeType", "toString", "noSuchMethod"
    };

    /// <summary>
    /// Wartość warunku RSL → nazwa elementu enum w camelCase: „OK” → „ok”, „notOK” → „notOk”,
    /// „not valid” → „notValid”. Słowa zastrzeżone Darta dostają prefiks „value”.
    /// </summary>
    public static string EnumMember(string value)
    {
        var words = Regex.Matches(value, "[A-Z]+(?![a-z])|[A-Z]?[a-z]+|[0-9]+")
            .Select(m => m.Value.ToLowerInvariant()).ToList();
        if (words.Count == 0) return "value";
        var name = words[0] + string.Concat(words.Skip(1).Select(UpperFirst));
        if (char.IsDigit(name[0]) || DartReservedWords.Contains(name))
            name = "value" + UpperFirst(name);
        return name;
    }

    public static string LowerFirst(string s) => s.Length == 0 ? s : char.ToLowerInvariant(s[0]) + s[1..];
    public static string UpperFirst(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
}
