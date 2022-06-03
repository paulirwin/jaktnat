namespace Jaktnat.Compiler.Syntax;

public class FunctionSyntax : BodySyntax
{
    public FunctionSyntax(string name, BlockSyntax body)
        : base(body)
    {
        Name = name;
    }

    public string Name { get; }
}