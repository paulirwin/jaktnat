namespace Jaktnat.Compiler.Syntax;

public class PropertySyntax : MemberDeclarationSyntax
{
    public PropertySyntax(string name, TypeIdentifierSyntax typeIdentifier, VisibilityModifier modifiers)
        : base(name)
    {
        TypeIdentifier = typeIdentifier;
        Modifiers = modifiers;
    }

    public TypeIdentifierSyntax TypeIdentifier { get; }

    public VisibilityModifier Modifiers { get; }

    public TypeReference? Type { get; set; }

    public override string ToString() => $"{Name}: {TypeIdentifier}";
}