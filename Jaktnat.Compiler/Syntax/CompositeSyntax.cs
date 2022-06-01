namespace Jaktnat.Compiler.Syntax;

public abstract class CompositeSyntax : SyntaxNode
{
    public IList<SyntaxNode> Children { get; } = new List<SyntaxNode>();
}