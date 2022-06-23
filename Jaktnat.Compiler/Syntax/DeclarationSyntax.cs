namespace Jaktnat.Compiler.Syntax;

public abstract class DeclarationSyntax : SyntaxNode
{
    protected DeclarationSyntax(string name, TypeIdentifierSyntax? typeIdentifier, bool mutable)
    {
        Name = name;
        TypeIdentifier = typeIdentifier;
        Mutable = mutable;
    }

    public string Name { get; }

    public TypeIdentifierSyntax? TypeIdentifier { get; }

    public bool Mutable { get; }

    public TypeReference? Type { get; set; }
}