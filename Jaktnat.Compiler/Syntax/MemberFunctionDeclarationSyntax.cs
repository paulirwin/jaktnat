namespace Jaktnat.Compiler.Syntax;

public class MemberFunctionDeclarationSyntax : MemberDeclarationSyntax
{
    public MemberFunctionDeclarationSyntax(string name, FunctionSyntax function) 
        : base(name)
    {
        Function = function;
    }

    public FunctionSyntax Function { get; }

    public override string ToString() => Function.ToString();
}