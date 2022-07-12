namespace Jaktnat.Compiler.Syntax;

public class ConstructorSyntax : ConstructorReferenceSyntax
{
    public ConstructorSyntax(TypeDeclarationSyntax declaringType, ParameterListSyntax parameters)
        : base(new DeclaredTypeReference(declaringType), parameters)
    {
    }
}