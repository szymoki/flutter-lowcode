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

    public static string LowerFirst(string s) => s.Length == 0 ? s : char.ToLowerInvariant(s[0]) + s[1..];
    public static string UpperFirst(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
}
