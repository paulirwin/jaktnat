namespace Jaktnat.Compiler.Syntax;

public class WhileSyntax : SyntaxNode
{
    public WhileSyntax(ExpressionSyntax condition, BlockSyntax body)
    {
        Condition = condition;
        Body = body;
    }

    public ExpressionSyntax Condition { get; }

    public BlockSyntax Body { get; }

    public override string ToString() => $"while {Condition} {Body}";
}