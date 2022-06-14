namespace Jaktnat.Compiler.Syntax;

public class NamedTypeIdentifierSyntax : TypeIdentifierSyntax
{
    public NamedTypeIdentifierSyntax(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public override string ToString() => Name;
}