namespace Jaktnat.Compiler.Syntax;

public class GuardSyntax : SyntaxNode
{
    public GuardSyntax(ExpressionSyntax condition, ElseSyntax elseNode)
    {
        Condition = condition;
        ElseNode = elseNode;
    }

    public ExpressionSyntax Condition { get; }

    public ElseSyntax ElseNode { get; }

    public override string ToString() => $"guard {Condition} {ElseNode}";
}