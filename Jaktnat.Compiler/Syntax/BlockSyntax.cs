namespace Jaktnat.Compiler.Syntax;

public class BlockSyntax : AggregateSyntax
{
    public TypeDeclarationSyntax? DeclaringType { get; set; }
    
    public IDictionary<string, DeclarationSyntax> Declarations { get; } = new Dictionary<string, DeclarationSyntax>();

    public IList<DeferSyntax> Defers { get; } = new List<DeferSyntax>();
    
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

    public bool TryResolveDeclaringType(out TypeDeclarationSyntax? declaringType)
    {
        declaringType = null;
        
        if (DeclaringType != null)
        {
            declaringType = DeclaringType;
            return true;
        }

        if (ParentBlock == null)
        {
            return false;
        }

        return ParentBlock.TryResolveDeclaringType(out declaringType);
    }

    // TODO: support indentation
    public override string ToString() => $"{{{Environment.NewLine}{string.Join(Environment.NewLine, Children)}{Environment.NewLine}}}";
}