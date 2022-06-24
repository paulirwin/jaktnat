namespace Jaktnat.Compiler.Syntax;

public class ElseSyntax : SyntaxNode
{
    public ElseSyntax(SyntaxNode child)
    {
        Child = child;
    }

    public SyntaxNode Child { get; }

    public override string ToString() => $"else {Child}";
}