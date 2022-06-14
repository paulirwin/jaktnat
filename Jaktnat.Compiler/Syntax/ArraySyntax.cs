namespace Jaktnat.Compiler.Syntax;

public class ArraySyntax : ExpressionSyntax
{
    public ArraySyntax(ExpressionListSyntax itemsList)
    {
        ItemsList = itemsList;
    }

    public ExpressionListSyntax ItemsList { get; }

    public override string ToString() => $"[{string.Join(", ", ItemsList.Items)}]";
}