namespace Jaktnat.Compiler.Syntax;

public class ParenthesizedExpressionSyntax : ExpressionSyntax
{
    public ParenthesizedExpressionSyntax(ExpressionSyntax expression)
    {
        Expression = expression;
    }

    public ExpressionSyntax Expression { get; }

    public override string ToString() => $"({Expression})";

    public override bool Mutates => false;

    public override bool PreventsMutation => Expression.PreventsMutation;
}