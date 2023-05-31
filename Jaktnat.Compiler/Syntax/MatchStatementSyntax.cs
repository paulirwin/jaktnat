namespace Jaktnat.Compiler.Syntax;

public class MatchStatementSyntax : SyntaxNode
{
    public MatchStatementSyntax(MatchExpressionSyntax matchExpression)
    {
        MatchExpression = matchExpression;
    }
    
    public MatchExpressionSyntax MatchExpression { get; }

    public override string ToString() => MatchExpression.ToString();
}