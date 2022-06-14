namespace Jaktnat.Compiler.Syntax;

public class ExpressionListSyntax : ExpressionSyntax
{
    public IList<ExpressionSyntax> Items { get; set; } = new List<ExpressionSyntax>();

    public override string ToString() => string.Join(", ", Items);
}