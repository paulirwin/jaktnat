namespace Jaktnat.Compiler.Syntax;

public class IfSyntax : SyntaxNode
{
    public IfSyntax(ExpressionSyntax condition, BlockSyntax block)
    {
        Condition = condition;
        Block = block;
    }

    public ExpressionSyntax Condition { get; }
    
    public BlockSyntax Block { get; }
}