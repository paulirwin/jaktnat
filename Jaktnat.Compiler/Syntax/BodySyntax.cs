namespace Jaktnat.Compiler.Syntax;

public abstract class BodySyntax : SyntaxNode
{
    protected BodySyntax(BlockSyntax body)
    {
        Body = body;
    }

    public BlockSyntax Body { get; set; }
}