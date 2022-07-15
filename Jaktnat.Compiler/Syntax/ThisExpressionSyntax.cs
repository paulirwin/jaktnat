namespace Jaktnat.Compiler.Syntax;

public class ThisExpressionSyntax : ExpressionSyntax
{
    public override string ToString() => "this";

    public override bool Mutates => false;

    public override bool PreventsMutation => false;
}