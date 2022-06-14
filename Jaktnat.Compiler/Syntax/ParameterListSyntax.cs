namespace Jaktnat.Compiler.Syntax;

public class ParameterListSyntax : SyntaxNode
{
    public IList<ParameterSyntax> Parameters { get; } = new List<ParameterSyntax>();

    public override string ToString() => string.Join(", ", Parameters);
}