namespace Jaktnat.Compiler.Syntax;

public class UnsafeBlockSyntax : SyntaxNode
{
    public UnsafeBlockSyntax(BlockSyntax block)
    {
        Block = block;
    }

    public BlockSyntax Block { get; }

    public override string ToString() => $"unsafe {Block}";
}