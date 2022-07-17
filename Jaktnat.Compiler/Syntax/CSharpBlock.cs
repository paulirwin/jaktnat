namespace Jaktnat.Compiler.Syntax;

public class CSharpBlockSyntax : SyntaxNode
{
    public CSharpBlockSyntax(BlockSyntax block)
    {
        Block = block;
    }

    public BlockSyntax Block { get; }
    
    public UnsafeBlockSyntax? ParentUnsafeBlock { get; set; }

    public override string ToString() => $"csharp {Block}";
}