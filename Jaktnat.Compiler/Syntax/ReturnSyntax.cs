namespace Jaktnat.Compiler.Syntax;

public class ReturnSyntax : SyntaxNode
{
    public ReturnSyntax(ExpressionSyntax? expression)
    {
        Expression = expression;
    }

    public ExpressionSyntax? Expression { get; }

    public override string ToString() => Expression != null ? $"return {Expression}" : "return";
}