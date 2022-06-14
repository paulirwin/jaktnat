using System.Text;

namespace Jaktnat.Compiler.Syntax;

public class ParameterSyntax : SyntaxNode
{
    public ParameterSyntax(bool anonymous, string name, bool mutable, TypeIdentifierSyntax typeIdentifier)
    {
        Anonymous = anonymous;
        Name = name;
        Mutable = mutable;
        TypeIdentifier = typeIdentifier;
    }

    public bool Anonymous { get; }

    public string Name { get; }

    public bool Mutable { get; }

    public TypeIdentifierSyntax TypeIdentifier { get; }

    public Type? ParameterType { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        if (Anonymous) sb.Append("anonymous ");

        sb.Append(Name);
        sb.Append(": ");

        if (Mutable) sb.Append("mutable ");

        sb.Append(TypeIdentifier);

        return sb.ToString();
    }
}