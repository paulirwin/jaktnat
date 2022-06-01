namespace Jaktnat.Compiler.Syntax;

public class CallSyntax : SyntaxNode
{
    public CallSyntax(string name, IList<CallArgumentSyntax> arguments)
    {
        Name = name;
        Arguments = arguments;
    }

    public string Name { get; }

    public IList<CallArgumentSyntax> Arguments { get; }
}