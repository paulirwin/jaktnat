namespace Jaktnat.Compiler.Syntax;

public class BlockScopedIdentifierSyntax : DeclarationSyntax
{
    public BlockScopedIdentifierSyntax(string name)
        : base(name, null, false)
    {
    }
    
    public override string ToString() => Name;
}