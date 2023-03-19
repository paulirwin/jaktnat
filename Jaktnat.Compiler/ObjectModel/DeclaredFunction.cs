using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.ObjectModel;

internal class DeclaredFunction : Function
{
    public DeclaredFunction(TypeDeclarationSyntax? declaringType, FunctionLikeSyntax function)
    {
        DeclaringType = declaringType;
        Function = function;
    }

    public TypeDeclarationSyntax? DeclaringType { get; }

    public FunctionLikeSyntax Function { get; }

    public override TypeReference ReturnType => Function switch
    {
        FunctionSyntax func => func.ReturnType ??
                               throw new InvalidOperationException("Function type resolution has not yet happened"),
        ConstructorSyntax ctor => ctor.DeclaringType,
        _ => throw new InvalidOperationException("Unexpected function type")
    };

    public string Name => Function switch
    {
        FunctionSyntax func => func.Name,
        ConstructorSyntax ctor => ".ctor",
        _ => throw new InvalidOperationException("Unexpected function type")
    };

    public override bool Mutates => DeclaringType != null
                                    && Function.Parameters.Parameters.Count > 0
                                    && Function.Parameters.Parameters[0] is ThisParameterSyntax { Mutable: true };

    public override string ToString() => Function.ToString();
}