namespace Jaktnat.Compiler.Syntax;

// TODO: Get rid of this class, it's only used for array initializers and is not an expression
public class ExpressionListSyntax : ExpressionSyntax
{
    public IList<ExpressionSyntax> Items { get; set; } = new List<ExpressionSyntax>();

    public override string ToString() => string.Join(", ", Items);

    public override bool Mutates => false;

    public override bool PreventsMutation => false;
}