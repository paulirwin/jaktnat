namespace Jaktnat.Compiler.Syntax;

public class ConstructorSyntax : SyntaxNode
{
    public ConstructorSyntax(TypeDeclarationSyntax declaringType, ParameterListSyntax parameters)
    {
        DeclaringType = declaringType;
        Parameters = parameters;
    }

    public TypeDeclarationSyntax DeclaringType { get; }

    public ParameterListSyntax Parameters { get; }

    public override string ToString() => $"{DeclaringType.Name}({Parameters})";
}