using System.Text;
using Mono.Cecil.Cil;

namespace Jaktnat.Compiler.Syntax;

public class VariableDeclarationSyntax : DeclarationSyntax
{
    public VariableDeclarationSyntax(string name, TypeIdentifierSyntax? typeIdentifier, bool mutable)
        : base (name, typeIdentifier, mutable)
    {
    }
    
    public ExpressionSyntax? InitializerExpression { get; set; }
    
    public VariableDefinition? ILVariableDefinition { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append("let ");

        if (Mutable) sb.Append("mutable ");

        sb.Append(Name);

        if (TypeIdentifier != null)
        {
            sb.Append(": ").Append(TypeIdentifier);
        }

        if (InitializerExpression != null)
        {
            sb.Append(" = ");
            sb.Append(InitializerExpression);
        }

        return sb.ToString();
    }
}