namespace Jaktnat.Compiler.Syntax;

public class CatchSyntax : SyntaxNode
{
    public CatchSyntax(CatchIdentifierSyntax catchIdentifier, BlockSyntax catchBlock)
    {
        CatchIdentifier = catchIdentifier;
        CatchBlock = catchBlock;
    }

    public CatchIdentifierSyntax CatchIdentifier { get; }

    public BlockSyntax CatchBlock { get; }

    public override string ToString() => $"catch {CatchIdentifier} {CatchBlock}";
}