namespace Jaktnat.Compiler.Syntax;

public abstract class ExpressionSyntax : SyntaxNode
{
    protected ExpressionSyntax()
    {
    }

    protected ExpressionSyntax(TypeReference expressionType)
    {
        ExpressionType = expressionType;
    }

    public TypeReference? ExpressionType { get; set; }

    public object? CompileTimeTarget { get; set; }

    public abstract bool Mutates { get; }
    
    public abstract bool PreventsMutation { get; }
}