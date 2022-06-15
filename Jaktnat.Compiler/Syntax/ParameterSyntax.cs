using System.Text;

namespace Jaktnat.Compiler.Syntax;

public class ParameterSyntax : DeclarationSyntax
{
    public ParameterSyntax(bool anonymous, string name, bool mutable, TypeIdentifierSyntax typeIdentifier)
        : base(name, typeIdentifier, mutable)
    {
        Anonymous = anonymous;
    }

    public bool Anonymous { get; }
    
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