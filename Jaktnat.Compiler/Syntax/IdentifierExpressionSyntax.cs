namespace Jaktnat.Compiler.Syntax;

public class IdentifierExpressionSyntax : ExpressionSyntax, IHasTarget
{
    public IdentifierExpressionSyntax(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public object? Target { get; set; }
}