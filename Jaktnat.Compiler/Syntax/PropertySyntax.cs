namespace Jaktnat.Compiler.Syntax;

public class PropertySyntax : MemberDeclarationSyntax
{
    public PropertySyntax(string name, TypeIdentifierSyntax typeIdentifier)
    {
        Name = name;
        TypeIdentifier = typeIdentifier;
    }

    public string Name { get; }

    public TypeIdentifierSyntax TypeIdentifier { get; }

    public TypeReference? Type { get; set; }

    public override string ToString() => $"{Name}: {TypeIdentifier}";
}