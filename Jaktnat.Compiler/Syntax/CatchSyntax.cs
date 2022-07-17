namespace Jaktnat.Compiler.Syntax;

public class CatchSyntax : SyntaxNode
{
    public CatchSyntax(BlockScopedIdentifierSyntax identifier, BlockSyntax catchBlock)
    {
        Identifier = identifier;
        CatchBlock = catchBlock;
    }

    public BlockScopedIdentifierSyntax Identifier { get; }

    public BlockSyntax CatchBlock { get; }

    public override string ToString() => $"catch {Identifier} {CatchBlock}";
}