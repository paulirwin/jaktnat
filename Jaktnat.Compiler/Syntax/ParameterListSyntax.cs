namespace Jaktnat.Compiler.Syntax;

public class ParameterListSyntax : SyntaxNode
{
    public ParameterListSyntax()
    {
        Parameters = new List<ParameterSyntax>();
    }

    public ParameterListSyntax(IList<ParameterSyntax> parameters)
    {
        Parameters = parameters;
    }

    public IList<ParameterSyntax> Parameters { get; }

    public IEnumerable<ParameterSyntax> GetNonThisParameters() => Parameters.Where(i => i is not ThisParameterSyntax);

    public override string ToString() => string.Join(", ", Parameters);
}