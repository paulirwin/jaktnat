namespace Jaktnat.Compiler.Syntax;

public abstract class ExpressionSyntax : SyntaxNode
{
    protected ExpressionSyntax()
    {
    }

    protected ExpressionSyntax(Type? expressionType)
    {
        ExpressionType = expressionType;
    }

    public Type? ExpressionType { get; set; }
}