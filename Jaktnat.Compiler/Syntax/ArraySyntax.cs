namespace Jaktnat.Compiler.Syntax;

public class ArraySyntax : ExpressionSyntax
{
    public ArraySyntax(ExpressionListSyntax itemsList)
    {
        ItemsList = itemsList;
    }

    public ExpressionListSyntax ItemsList { get; }

    public override string ToString() => $"[{string.Join(", ", ItemsList.Items)}]";
    
    public override bool Mutates => false;
    
    public override bool PreventsMutation => true; // array initializers are immutable
}