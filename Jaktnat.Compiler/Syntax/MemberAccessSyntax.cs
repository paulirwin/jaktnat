namespace Jaktnat.Compiler.Syntax;

public class MemberAccessSyntax : ExpressionSyntax
{
    public MemberAccessSyntax(ExpressionSyntax target, IdentifierExpressionSyntax member)
    {
        Target = target;
        Member = member;
    }

    public ExpressionSyntax Target { get; }

    public IdentifierExpressionSyntax Member { get; }

    public override string ToString() => $"{Target}.{Member}";
}