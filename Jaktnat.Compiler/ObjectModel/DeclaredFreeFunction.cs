using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.ObjectModel;

internal class DeclaredFreeFunction : FreeFunction
{
    public DeclaredFreeFunction(FunctionSyntax functionSyntax)
    {
        FunctionSyntax = functionSyntax;
    }

    public FunctionSyntax FunctionSyntax { get; }

    public override Type ReturnType => FunctionSyntax.ReturnType;
}