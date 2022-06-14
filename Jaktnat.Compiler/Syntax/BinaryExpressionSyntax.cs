namespace Jaktnat.Compiler.Syntax;

public class BinaryExpressionSyntax : ExpressionSyntax
{
    public BinaryExpressionSyntax(ExpressionSyntax left, BinaryOperator op, ExpressionSyntax right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public ExpressionSyntax Left { get; }

    public BinaryOperator Operator { get; }

    public ExpressionSyntax Right { get; }

    public override string ToString() => $"{Left} {OperatorDisplay} {Right}";

    public string OperatorDisplay => Operator switch
    {
        BinaryOperator.Add => "+",
        BinaryOperator.Subtract => "-",
        BinaryOperator.Multiply => "*",
        BinaryOperator.Divide => "/",
        BinaryOperator.Modulo => "%",
        BinaryOperator.Equal => "==",
        BinaryOperator.NotEqual => "!=",
        BinaryOperator.LessThan => "<",
        BinaryOperator.GreaterThan => ">",
        BinaryOperator.LessThanOrEqual => "<=",
        BinaryOperator.GreaterThanOrEqual => ">=",
        BinaryOperator.LogicalAnd => "and",
        BinaryOperator.LogicalOr => "or",
        BinaryOperator.BitwiseAnd => "&",
        BinaryOperator.BitwiseOr => "|",
        BinaryOperator.BitwiseXor => "^",
        BinaryOperator.BitwiseLeftShift => "<<",
        BinaryOperator.BitwiseRightShift => ">>",
        BinaryOperator.ArithmeticLeftShift => "<<<",
        BinaryOperator.ArithmeticRightShift => ">>>",
        BinaryOperator.Assign => "=",
        BinaryOperator.AddAssign => "+=",
        BinaryOperator.SubtractAssign => "-=",
        BinaryOperator.MultiplyAssign => "*=",
        BinaryOperator.DivideAssign => "/=",
        BinaryOperator.ModuloAssign => "%=",
        BinaryOperator.BitwiseAndAssign => "&=",
        BinaryOperator.BitwiseOrAssign => "|=",
        BinaryOperator.BitwiseXorAssign => "^=",
        BinaryOperator.BitwiseLeftShiftAssign => "<<=",
        BinaryOperator.BitwiseRightShiftAssign => ">>=",
        BinaryOperator.NoneCoalescing => "??",
        BinaryOperator.NoneCoalescingAssign => "??=",
        _ => throw new ArgumentOutOfRangeException()
    };
}