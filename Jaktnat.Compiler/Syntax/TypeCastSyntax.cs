namespace Jaktnat.Compiler.Syntax;

public class TypeCastSyntax : UnaryExpressionSyntax
{
    public TypeCastSyntax(ExpressionSyntax expression, bool fallible, TypeIdentifierSyntax type)
        : base(expression, UnaryOperator.TypeCast)
    {
        Fallible = fallible;
        Type = type;
    }

    public bool Fallible { get; }

    public TypeIdentifierSyntax Type { get; }

    public override string ToString() => $"{Expression} {(Fallible ? "as?" : "as!")} {Type}";
}