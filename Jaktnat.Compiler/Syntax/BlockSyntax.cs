namespace Jaktnat.Compiler.Syntax;

public class BlockSyntax : AggregateSyntax
{
    public IDictionary<string, DeclarationSyntax> Declarations { get; set; } = new Dictionary<string, DeclarationSyntax>();

    public bool TryResolveDeclaration(string name, out DeclarationSyntax declaration)
    {
        if (Declarations.TryGetValue(name, out declaration!))
        {
            return true;
        }

        if (ParentBlock == null)
        {
            return false;
        }

        return ParentBlock.TryResolveDeclaration(name, out declaration);
    }

    // TODO: support indentation
    public override string ToString() => $"{{{Environment.NewLine}{string.Join(Environment.NewLine, Children)}{Environment.NewLine}}}";
}