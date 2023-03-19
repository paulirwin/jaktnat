using Jaktnat.Compiler.ObjectModel;

namespace Jaktnat.Compiler.Syntax;

internal class LateBoundConstructorReference : Function
{
    public LateBoundConstructorReference(TypeReference declaringType, ParameterListSyntax parameters)
    {
        ReturnType = declaringType;
    }

    public override TypeReference ReturnType { get; }

    public override bool Mutates => false;
}