namespace Jaktnat.Compiler.Syntax;

public abstract class TypeDeclarationSyntax : SyntaxNode
{
    protected TypeDeclarationSyntax(string name, bool isValueType)
    {
        Name = name;
        IsValueType = isValueType;
    }

    public string Name { get; }

    public bool IsValueType { get; }

    public IList<ConstructorSyntax> Constructors { get; } = new List<ConstructorSyntax>();
    
    public IList<MemberDeclarationSyntax> Members { get; } = new List<MemberDeclarationSyntax>();
}