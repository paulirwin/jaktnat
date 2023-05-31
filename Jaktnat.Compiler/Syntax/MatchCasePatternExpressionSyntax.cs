namespace Jaktnat.Compiler.Syntax;

public class MatchCasePatternExpressionSyntax : MatchCasePatternSyntax
{
    public MatchCasePatternExpressionSyntax(ExpressionSyntax expression)
    {
        Expression = expression;
    }

    public ExpressionSyntax Expression { get; }

    public override bool Mutates => Expression.Mutates;
    
    public override bool PreventsMutation => Expression.PreventsMutation;

    public override string ToString() => $"({Expression})";
}