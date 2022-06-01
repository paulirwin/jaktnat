namespace Jaktnat.Compiler.Syntax;

public class CallArgumentSyntax : SyntaxNode
{
    public CallArgumentSyntax(string? parameterName, SyntaxNode expression)
    {
        ParameterName = parameterName;
        Expression = expression;
    }

    public string? ParameterName { get; }

    public SyntaxNode Expression { get; }
}