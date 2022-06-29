using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.ObjectModel;

internal class DeclaredFunction : Function
{
    public DeclaredFunction(FunctionSyntax functionSyntax)
    {
        FunctionSyntax = functionSyntax;
    }

    public FunctionSyntax FunctionSyntax { get; }

    public override TypeReference ReturnType => FunctionSyntax.ReturnType ?? throw new InvalidOperationException("Function type resolution has not yet happened");

    public override string ToString() => FunctionSyntax.ToString();
}