using Antlr4.Runtime;

namespace FlutterCodeGenerator.Parser;

/// <summary>
/// Zbiera błędy leksera i parsera zamiast wypisywać je na konsolę i kontynuować,
/// jak robi domyślny listener ANTLR.
/// </summary>
public class SyntaxErrorCollector : IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
{
    private readonly string _fileName;
    private readonly List<string> _errors;

    public SyntaxErrorCollector(string fileName, List<string> errors)
    {
        _fileName = fileName;
        _errors = errors;
    }

    public int Count { get; private set; }

    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        => Add(line, charPositionInLine, msg);

    public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        => Add(line, charPositionInLine, msg);

    private void Add(int line, int column, string msg)
    {
        Count++;
        // Kolumna w ANTLR liczona jest od 0; w komunikacie podajemy od 1, jak edytory.
        _errors.Add($"{_fileName}:{line}:{column + 1}: {msg}");
    }
}
