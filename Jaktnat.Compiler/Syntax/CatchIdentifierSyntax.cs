namespace Jaktnat.Compiler.Syntax;

public class CatchIdentifierSyntax : DeclarationSyntax
{
    public CatchIdentifierSyntax(string name)
        : base(name, null, false)
    {
    }
    
    public override string ToString() => Name;
}