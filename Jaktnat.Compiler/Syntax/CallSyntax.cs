using Jaktnat.Compiler.ObjectModel;

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

    internal IList<Function>? PossibleMatchedMethods { get; set; }

    internal Function? MatchedMethod { get; set; }
    
    internal IList<ConstructorReferenceSyntax>? PossibleMatchedConstructors { get; set; }

    internal ConstructorReferenceSyntax? MatchedConstructor { get; set; }

    public override string ToString() => $"{Target}({string.Join(", ", Arguments)})";
}