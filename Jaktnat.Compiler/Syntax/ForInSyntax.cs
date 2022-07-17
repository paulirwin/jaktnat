namespace Jaktnat.Compiler.Syntax;

public class ForInSyntax : SyntaxNode
{
    public ForInSyntax(BlockScopedIdentifierSyntax identifier, ExpressionSyntax expression, BlockSyntax block)
    {
        Identifier = identifier;
        Expression = expression;
        Block = block;
    }

    public BlockScopedIdentifierSyntax Identifier { get; }

    public ExpressionSyntax Expression { get; }

    public BlockSyntax Block { get; }

    public override string ToString() => $"for {Identifier} in {Expression} {Block}";
}