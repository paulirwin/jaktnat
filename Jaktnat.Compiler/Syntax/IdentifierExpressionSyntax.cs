namespace Jaktnat.Compiler.Syntax;

public class IdentifierExpressionSyntax : ExpressionSyntax
{
    public IdentifierExpressionSyntax(string name)
    {
        Name = name;
    }

    public string Name { get; }
    
    public ExpressionSyntax? ParentTarget { get; set; }

    public override string ToString() => Name;
}