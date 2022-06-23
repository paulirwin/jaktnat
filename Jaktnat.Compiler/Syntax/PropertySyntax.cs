namespace Jaktnat.Compiler.Syntax;

public class PropertySyntax : MemberDeclarationSyntax
{
    public PropertySyntax(string name, TypeIdentifierSyntax typeIdentifier, PropertyModifier modifiers)
        : base(name)
    {
        TypeIdentifier = typeIdentifier;
        Modifiers = modifiers;
    }

    public TypeIdentifierSyntax TypeIdentifier { get; }

    public PropertyModifier Modifiers { get; }

    public TypeReference? Type { get; set; }

    public override string ToString() => $"{Name}: {TypeIdentifier}";
}