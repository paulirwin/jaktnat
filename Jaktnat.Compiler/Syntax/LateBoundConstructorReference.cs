namespace Jaktnat.Compiler.Syntax;

public class LateBoundConstructorReference : ConstructorReferenceSyntax
{
    public LateBoundConstructorReference(TypeReference declaringType, ParameterListSyntax parameters) 
        : base(declaringType, parameters)
    {
    }
}