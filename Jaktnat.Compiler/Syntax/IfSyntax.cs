namespace Jaktnat.Compiler.Syntax;

public class IfSyntax : BodySyntax
{
    public IfSyntax(ExpressionSyntax condition, BlockSyntax body)
        : base(body)
    {
        Condition = condition;
    }

    public ExpressionSyntax Condition { get; }

    public override string ToString() => $"if {Condition} {Body}";
}