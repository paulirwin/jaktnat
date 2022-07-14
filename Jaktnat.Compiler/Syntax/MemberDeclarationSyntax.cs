namespace Jaktnat.Compiler.Syntax;

public abstract class MemberDeclarationSyntax : SyntaxNode
{
    protected MemberDeclarationSyntax(string name)
    {
        Name = name;
    }

    public string Name { get; }
    
    public TypeDeclarationSyntax? DeclaringType { get; set; }
}