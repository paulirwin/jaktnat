namespace Jaktnat.Compiler.Syntax;

public class LoopSyntax : SyntaxNode
{
    public LoopSyntax(BlockSyntax body)
    {
        Body = body;
    }

    public BlockSyntax Body { get; }

    public override string ToString() => $"loop {Body}";
}