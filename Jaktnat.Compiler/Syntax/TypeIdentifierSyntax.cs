namespace Jaktnat.Compiler.Syntax;

public abstract class TypeIdentifierSyntax : SyntaxNode
{
    public Type? Type { get; set; }
}