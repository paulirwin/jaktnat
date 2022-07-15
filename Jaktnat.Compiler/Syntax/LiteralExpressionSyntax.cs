namespace Jaktnat.Compiler.Syntax;

public class LiteralExpressionSyntax : ExpressionSyntax
{
    public LiteralExpressionSyntax(object value)
        : base(value.GetType())
    {
        Value = value;
    }

    public object Value { get; }

    public override string ToString() => Value switch
    {
        string s => $"\"{s}\"",
        _ => Value.ToString() ?? "",
    };

    public override bool Mutates => false;

    public override bool PreventsMutation => true; // literals are immutable
}