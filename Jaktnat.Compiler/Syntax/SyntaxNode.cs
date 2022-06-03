namespace Jaktnat.Compiler.Syntax;

public abstract class SyntaxNode
{
    public BlockSyntax? ParentBlock { get; set; }
}