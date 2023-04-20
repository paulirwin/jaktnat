namespace Jaktnat.Compiler.Syntax;

public class StructDeclarationSyntax : TypeDeclarationSyntax
{
    public StructDeclarationSyntax(string name)
        : base(name, isValueType: true)
    {
    }

    public override string ToString() => $"struct {Name} {{{string.Join(Environment.NewLine, Members.Select(i => $"    {i}"))}}}";
}