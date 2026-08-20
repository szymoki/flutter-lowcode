namespace FlutterCodeGenerator.Model.Rsl;

public abstract class Sentence;

public class SvoSentence : Sentence
{
    public string Label { get; set; } = "";
    public string? AltLabel { get; set; }
    public Step Step { get; set; } = null!;
}

public class CondSentence : Sentence
{
    public List<UcCondition> Conditions { get; set; } = new();
}

public abstract class Step;

public class UserStep : Step
{
    public string Actor { get; set; } = "";
    public UserPredicate Predicate { get; set; } = null!;
}

public class SystemStep : Step
{
    public SystemPredicate Predicate { get; set; } = null!;
}

public abstract class UserPredicate;

public class SelectPredicate : UserPredicate
{
    public string Notion { get; set; } = "";
}

public class EnterPredicate : UserPredicate
{
    public string Notion { get; set; } = "";
}

public class InvokePredicate : UserPredicate
{
    public List<string> Names { get; set; } = new();
}

public abstract class SystemPredicate;

public class ShowPredicate : SystemPredicate
{
    public string Notion { get; set; } = "";
}

public class ClosePredicate : SystemPredicate
{
    public string Notion { get; set; } = "";
}

public class ReadPredicate : SystemPredicate
{
    public string Notion { get; set; } = "";
}

public class UpdatePredicate : SystemPredicate
{
    public string Notion { get; set; } = "";
}

public class DeletePredicate : SystemPredicate
{
    public string Notion { get; set; } = "";
}

public class CheckPredicate : SystemPredicate
{
    public string Notion { get; set; } = "";
}

public class ExecutePredicate : SystemPredicate
{
    public string Notion { get; set; } = "";
}

public class SysInvokePredicate : SystemPredicate
{
    public string Name { get; set; } = "";
}

public abstract class UcCondition;

public class ContextCondition : UcCondition
{
    public string Notion { get; set; } = "";
}

public class ValueCondition : UcCondition
{
    public string Notion { get; set; } = "";
    public string Value { get; set; } = "";
    public bool Negated { get; set; }
}

public class EndSentence
{
    public bool IsEnd { get; set; }
    public string? ReturnValue { get; set; }
    public bool IsRejoin { get; set; }
    public string? RejoinLabel { get; set; }
}
