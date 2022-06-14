namespace Jaktnat.Compiler.Syntax;

public class LiteralExpressionSyntax : ExpressionSyntax
{
    public LiteralExpressionSyntax(object value)
        : base(value.GetType())
    {
        Value = value;
    }

    public object Value { get; }

    public override string ToString() => Value.ToString() ?? ""; // TODO: format literals properly
}