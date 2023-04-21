namespace Jaktnat.Compiler.Syntax;

public class PropertySyntax : MemberDeclarationSyntax
{
    public PropertySyntax(string name,
        TypeIdentifierSyntax typeIdentifier,
        VisibilityModifier modifiers,
        ExpressionSyntax? defaultExpression)
        : base(name)
    {
        TypeIdentifier = typeIdentifier;
        Modifiers = modifiers;
        DefaultExpression = defaultExpression;
    }

    public TypeIdentifierSyntax TypeIdentifier { get; }

    public VisibilityModifier Modifiers { get; }
    
    public ExpressionSyntax? DefaultExpression { get; }

    public TypeReference? Type { get; set; }

    public override string ToString() => $"{Name}: {TypeIdentifier}";
}