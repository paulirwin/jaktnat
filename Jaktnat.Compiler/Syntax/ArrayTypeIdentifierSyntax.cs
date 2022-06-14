namespace Jaktnat.Compiler.Syntax;

public class ArrayTypeIdentifierSyntax : TypeIdentifierSyntax
{
    public ArrayTypeIdentifierSyntax(TypeIdentifierSyntax elementType)
    {
        ElementType = elementType;
    }

    public TypeIdentifierSyntax ElementType { get; } // TODO: multi-dimensional arrays

    public override string ToString() => $"[{ElementType}]";
}