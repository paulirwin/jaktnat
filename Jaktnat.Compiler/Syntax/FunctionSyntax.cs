namespace Jaktnat.Compiler.Syntax;

public class FunctionSyntax : BodySyntax
{
    public FunctionSyntax(string name, ParameterListSyntax? parameters, BlockSyntax body)
        : base(body)
    {
        Name = name;
        Parameters = parameters;
    }

    public string Name { get; }

    public ParameterListSyntax? Parameters { get; }

    public Type? ReturnType { get; set; }

    public override string ToString() => $"function {Name}({Parameters})"; // TODO: return type
}