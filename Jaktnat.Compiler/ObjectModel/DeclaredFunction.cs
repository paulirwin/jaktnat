using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.ObjectModel;

internal class DeclaredFunction : Function
{
    public DeclaredFunction(TypeDeclarationSyntax? declaringType, FunctionSyntax functionSyntax)
    {
        DeclaringType = declaringType;
        FunctionSyntax = functionSyntax;
    }

    public TypeDeclarationSyntax? DeclaringType { get; }

    public FunctionSyntax FunctionSyntax { get; }

    public override TypeReference ReturnType => FunctionSyntax.ReturnType ?? throw new InvalidOperationException("Function type resolution has not yet happened");

    public override bool Mutates => DeclaringType != null
                                    && FunctionSyntax.Parameters != null
                                    && FunctionSyntax.Parameters.Parameters.Count > 0
                                    && FunctionSyntax.Parameters.Parameters[0] is ThisParameterSyntax { Mutable: true };

    public override string ToString() => FunctionSyntax.ToString();
}