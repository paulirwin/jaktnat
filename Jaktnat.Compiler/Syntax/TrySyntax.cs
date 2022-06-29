namespace Jaktnat.Compiler.Syntax;

public class TrySyntax : SyntaxNode
{
    public TrySyntax(SyntaxNode tryable, CatchSyntax catchSyntax)
    {
        Tryable = tryable;
        Catch = catchSyntax;
    }

    public SyntaxNode Tryable { get; }

    public CatchSyntax Catch { get; set; }

    public override string ToString() => $"try {Tryable} {Catch}";
}