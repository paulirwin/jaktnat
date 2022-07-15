namespace Jaktnat.Compiler.Syntax;

public class UnaryExpressionSyntax : ExpressionSyntax
{
    public UnaryExpressionSyntax(ExpressionSyntax expression, UnaryOperator op)
    {
        Expression = expression;
        Operator = op;
    }

    public ExpressionSyntax Expression { get; }

    public UnaryOperator Operator { get; }

    public override string ToString() => Operator switch
    {
        UnaryOperator.PreIncrement => $"++{Expression}",
        UnaryOperator.PostIncrement => $"{Expression}++",
        UnaryOperator.PreDecrement => $"--{Expression}",
        UnaryOperator.PostDecrement => $"{Expression}--",
        UnaryOperator.Negate => $"-{Expression}",
        UnaryOperator.Dereference => $"*{Expression}",
        UnaryOperator.RawAddress => $"&{Expression}",
        UnaryOperator.LogicalNot => $"not {Expression}",
        UnaryOperator.BitwiseNot => $"~{Expression}",
        _ => throw new ArgumentOutOfRangeException()
    };

    public override bool Mutates => Operator
        is UnaryOperator.PreIncrement
        or UnaryOperator.PreDecrement
        or UnaryOperator.PostIncrement
        or UnaryOperator.PostDecrement;

    public override bool PreventsMutation => Expression.PreventsMutation;
}