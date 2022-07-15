namespace Jaktnat.Compiler.Syntax;

public class DeferSyntax : SyntaxNode
{
    public DeferSyntax(SyntaxNode body)
    {
        Body = body;
    }

    public SyntaxNode Body { get; }

    public override string ToString() => $"defer {Body}";
}