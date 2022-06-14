namespace Jaktnat.Compiler.Syntax;

public abstract class AggregateSyntax : SyntaxNode
{
    public IList<SyntaxNode> Children { get; } = new List<SyntaxNode>();
}