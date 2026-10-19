using FlutterCodeGenerator.Model.Ir;

namespace FlutterCodeGenerator.Translation;

/// <summary>
/// Stan translacji związany z etykietą zdania. Scenariusz alternatywny zaczyna się
/// od powtórzenia etykiety i kontynuuje ze stanu po tym zdaniu; <c>rejoin</c> kopiuje
/// instrukcje od miejsca, w którym zdanie zaczęło dopisywać instrukcje.
/// </summary>
public class LabelSnapshot
{
    // Stan po przetworzeniu zdania.
    public ViewWidget? CurrentVF { get; init; }
    public UCOperation? CurrentUCO { get; init; }
    public Condition? CurrentCondition { get; init; }
    public List<DataAggregate> CurrentDAP { get; init; } = new();
    public List<DataAggregate> CurrentDAD { get; init; } = new();

    // Pozycja przed przetworzeniem zdania; null dla zdań aktora, które nie tworzą instrukcji.
    public List<Instruction>? InstructionList { get; init; }
    public int InstructionIndex { get; init; }
}
