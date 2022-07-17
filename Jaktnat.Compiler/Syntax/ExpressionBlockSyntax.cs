namespace Jaktnat.Compiler.Syntax;

// HACK.PI: Currently you can only declare variables within blocks.
// This class exists mainly to support expression-bodied functions.
public class ExpressionBlockSyntax : BlockSyntax
{
    public ExpressionBlockSyntax(ExpressionSyntax expression)
    {
        Children.Add(expression);
    }

    public override string ToString() => string.Join(Environment.NewLine, Children);
}