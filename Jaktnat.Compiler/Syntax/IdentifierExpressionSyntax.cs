namespace Jaktnat.Compiler.Syntax;

public class IdentifierExpressionSyntax : ExpressionSyntax
{
    public IdentifierExpressionSyntax(string name)
    {
        Name = name;
    }

    public string Name { get; }
}