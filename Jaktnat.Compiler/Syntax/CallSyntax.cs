using System.Reflection;

namespace Jaktnat.Compiler.Syntax;

public class CallSyntax : ExpressionSyntax
{
    public CallSyntax(ExpressionSyntax target, IList<CallArgumentSyntax> arguments)
    {
        Target = target;
        Arguments = arguments;
    }

    public ExpressionSyntax Target { get; }

    public IList<CallArgumentSyntax> Arguments { get; }

    public IList<MethodInfo>? PossibleMatchedMethods { get; set; }

    public MethodInfo? MatchedMethod { get; set; }
}