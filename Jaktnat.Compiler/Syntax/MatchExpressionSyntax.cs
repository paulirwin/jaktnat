namespace Jaktnat.Compiler.Syntax;

public class MatchExpressionSyntax : ExpressionSyntax
{
    public MatchExpressionSyntax(ExpressionSyntax expression, IReadOnlyList<MatchCaseSyntax> cases)
    {
        Expression = expression;
        Cases = cases;
    }
    
    public ExpressionSyntax Expression { get; }
    
    public IReadOnlyList<MatchCaseSyntax> Cases { get; }

    public override string ToString() => $"match {Expression} {{{Environment.NewLine}{string.Join("", Cases.Select(i => $"    {i}{Environment.NewLine}"))}}}";
    
    public override bool Mutates => Expression.Mutates || Cases.Any(i => i.Mutates);
    
    // TODO: is this needed, or just return false here?
    public override bool PreventsMutation => Expression.PreventsMutation || Cases.Any(i => i.PreventsMutation);
}