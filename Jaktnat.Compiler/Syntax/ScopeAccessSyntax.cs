namespace Jaktnat.Compiler.Syntax;

public class ScopeAccessSyntax : ExpressionSyntax
{
    public ScopeAccessSyntax(IdentifierExpressionSyntax scope, IdentifierExpressionSyntax member)
    {
        Scope = scope;
        Member = member;
    }

    public IdentifierExpressionSyntax Scope { get; }

    public IdentifierExpressionSyntax Member { get; }

    public override string ToString() => $"{Scope}::{Member}";
}