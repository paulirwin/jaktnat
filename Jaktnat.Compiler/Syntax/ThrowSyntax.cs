namespace Jaktnat.Compiler.Syntax;

public class ThrowSyntax : SyntaxNode
{
    public ThrowSyntax(ExpressionSyntax? expression)
    {
        Expression = expression;
    }

    public ExpressionSyntax? Expression { get; }

    public override string ToString() => Expression != null ? $"throw {Expression}" : "throw";
}