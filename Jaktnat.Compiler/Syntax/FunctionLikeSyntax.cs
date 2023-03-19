namespace Jaktnat.Compiler.Syntax;

public abstract class FunctionLikeSyntax : SyntaxNode
{
    protected FunctionLikeSyntax(ParameterListSyntax parameters)
    {
        Parameters = parameters;
    }

    public ParameterListSyntax Parameters { get; }

    public abstract override string ToString();
}