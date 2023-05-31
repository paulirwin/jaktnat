namespace Jaktnat.Compiler.Syntax;

public class MatchCaseExpressionBodySyntax : MatchCaseBodySyntax
{
    public MatchCaseExpressionBodySyntax(ExpressionSyntax expression)
    {
        Expression = expression;
    }
    
    public ExpressionSyntax Expression { get; }

    public override string ToString() => Expression.ToString();
    
    public override bool Mutates => Expression.Mutates;
    
    public override bool PreventsMutation => Expression.PreventsMutation;
}