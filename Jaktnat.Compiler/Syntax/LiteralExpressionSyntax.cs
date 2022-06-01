namespace Jaktnat.Compiler.Syntax;

public class LiteralExpressionSyntax : ExpressionSyntax
{
    public LiteralExpressionSyntax(object value)
    {
        Value = value;
    }

    public object Value { get; }
}