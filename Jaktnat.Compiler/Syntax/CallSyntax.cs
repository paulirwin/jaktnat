using System.Reflection;

namespace Jaktnat.Compiler.Syntax;

public class CallSyntax : ExpressionSyntax
{
    public CallSyntax(string name, IList<CallArgumentSyntax> arguments)
    {
        Name = name;
        Arguments = arguments;
    }

    public string Name { get; }

    public IList<CallArgumentSyntax> Arguments { get; }

    public IList<MethodInfo>? PossibleMatchedMethods { get; set; }

    public MethodInfo? MatchedMethod { get; set; }
}