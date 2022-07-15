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

    // NOTE: technically a static getter could mutate,
    // but we'll assume they don't for now
    public override bool Mutates => false;

    public override bool PreventsMutation => Scope.PreventsMutation || Member.PreventsMutation;
}