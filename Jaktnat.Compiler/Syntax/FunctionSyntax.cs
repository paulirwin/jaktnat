namespace Jaktnat.Compiler.Syntax;

public class FunctionSyntax : SyntaxNode
{
    public FunctionSyntax(string name, SyntaxNode body)
    {
        Name = name;
        Body = body;
    }

    public string Name { get; }

    public SyntaxNode Body { get; }
}