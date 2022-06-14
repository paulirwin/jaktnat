namespace Jaktnat.Compiler.Syntax;

public class TypeCheckSyntax : UnaryExpressionSyntax
{
    public TypeCheckSyntax(ExpressionSyntax expression, TypeIdentifierSyntax type)
        : base(expression, UnaryOperator.TypeCheck)
    {
        Type = type;
    }

    public TypeIdentifierSyntax Type { get; }

    public override string ToString() => $"{Expression} is {Type}";
}