namespace Jaktnat.Compiler.Syntax;

public class BlockSyntax : AggregateSyntax
{
    public IDictionary<string, VariableDeclarationSyntax> Variables { get; set; } = new Dictionary<string, VariableDeclarationSyntax>();

    public bool TryResolveVariable(string name, out VariableDeclarationSyntax variableDeclaration)
    {
        if (Variables.TryGetValue(name, out variableDeclaration!))
        {
            return true;
        }

        if (ParentBlock == null)
        {
            return false;
        }

        return ParentBlock.TryResolveVariable(name, out variableDeclaration);
    }

    // TODO: support indentation
    public override string ToString() => $"{{{Environment.NewLine}{string.Join(Environment.NewLine, Children)}{Environment.NewLine}}}";
}