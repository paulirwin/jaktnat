namespace Jaktnat.Compiler.Syntax;

public class CallArgumentSyntax : SyntaxNode
{
    public CallArgumentSyntax(string? parameterName, ExpressionSyntax expression)
    {
        ParameterName = parameterName;
        Expression = expression;
    }

    public string? ParameterName { get; }

    public ExpressionSyntax Expression { get; }

    public Type? ArgumentType { get; set; }

    public override string ToString() => ParameterName != null ? $"{ParameterName}: {Expression}" : Expression.ToString();
}