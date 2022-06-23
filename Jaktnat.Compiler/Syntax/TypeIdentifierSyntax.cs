namespace Jaktnat.Compiler.Syntax;

public abstract class TypeIdentifierSyntax : SyntaxNode
{
    public TypeReference? Type { get; set; }
}