namespace Jaktnat.Compiler.Syntax;

public class MemberAccessSyntax : ExpressionSyntax
{
    public MemberAccessSyntax(IdentifierExpressionSyntax member)
    {
        Target = new ThisExpressionSyntax(isImplicit: true);
        Member = member;
    }
    
    public MemberAccessSyntax(ExpressionSyntax target, IdentifierExpressionSyntax member)
    {
        Target = target;
        Member = member;
    }

    public ExpressionSyntax Target { get; }

    public IdentifierExpressionSyntax Member { get; }

    public override string ToString() => $"{Target}.{Member}";

    // NOTE: technically a property getter could mutate,
    // but for now we'll assume that they don't
    public override bool Mutates => false;

    public override bool PreventsMutation => Target.PreventsMutation || Member.PreventsMutation;

    public MemberAccessSyntax WithTarget(ExpressionSyntax target) => new(target, Member);
}