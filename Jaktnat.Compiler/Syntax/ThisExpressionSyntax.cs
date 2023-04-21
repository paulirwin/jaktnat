namespace Jaktnat.Compiler.Syntax;

public class ThisExpressionSyntax : ExpressionSyntax
{
    public ThisExpressionSyntax()
    {
        IsImplicit = false;
    }

    public ThisExpressionSyntax(bool isImplicit)
    {
        IsImplicit = isImplicit;
    }
    
    public bool IsImplicit { get; }
    
    public override string ToString() => IsImplicit ? "" : "this";

    public override bool Mutates => false;

    public override bool PreventsMutation => false;
}