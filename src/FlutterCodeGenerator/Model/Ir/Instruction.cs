namespace FlutterCodeGenerator.Model.Ir;

public abstract class Instruction
{
    public string Label { get; set; } = "";
}

public class Call : Instruction
{
    public string OperationRef { get; set; } = "";
    /// <summary>Wywołanie innego przypadku użycia (systemowe &lt;invoke&gt;); wtedy OperationRef to nazwa jego operacji.</summary>
    public UseCaseClass? TargetUseCase { get; set; }
    public UCOperation? TargetOperation { get; set; }
    public List<Parameter> Parameters { get; } = new();
}

public class Decision : Instruction
{
    public List<Condition> Conditions { get; } = new();
}

public class Condition : Instruction
{
    public List<Expression> Expressions { get; } = new();
    public List<Instruction> Instructions { get; } = new();
    public Value? Value { get; set; }
}

public class End : Instruction
{
    public Value? ReturnValue { get; set; }
}
