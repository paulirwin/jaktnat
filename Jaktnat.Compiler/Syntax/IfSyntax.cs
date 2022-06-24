namespace Jaktnat.Compiler.Syntax;

public class IfSyntax : BodySyntax
{
    public IfSyntax(ExpressionSyntax condition, BlockSyntax body, ElseSyntax? elseNode)
        : base(body)
    {
        Condition = condition;
        ElseNode = elseNode;
    }

    public ExpressionSyntax Condition { get; }

    public ElseSyntax? ElseNode { get; }

    public override string ToString() => $"if {Condition} {Body} {ElseNode}";
}