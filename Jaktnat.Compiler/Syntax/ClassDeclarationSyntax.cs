namespace Jaktnat.Compiler.Syntax;

public class ClassDeclarationSyntax : TypeDeclarationSyntax
{
    public ClassDeclarationSyntax(string name)
        : base(name, isValueType: false)
    {
    }

    public override string ToString() => $"class {Name} {{{string.Join(Environment.NewLine, Members.Select(i => $"    {i}"))}}}";
}