﻿using System.Text;

namespace Jaktnat.Compiler.Syntax;

public class ParameterSyntax : DeclarationSyntax
{
    public ParameterSyntax(bool anonymous,
        string name,
        bool mutable,
        TypeIdentifierSyntax? typeIdentifier,
        ExpressionSyntax? defaultArgument)
        : base(name, typeIdentifier, mutable)
    {
        Anonymous = anonymous;
        DefaultArgument = defaultArgument;
    }

    public bool Anonymous { get; }
    
    public ExpressionSyntax? DefaultArgument { get; }
    
    public override string ToString()
    {
        var sb = new StringBuilder();

        if (Anonymous) sb.Append("anonymous ");

        sb.Append(Name);
        sb.Append(": ");

        if (Mutable) sb.Append("mutable ");

        sb.Append(TypeIdentifier);

        if (DefaultArgument != null)
        {
            sb.Append(" = ").Append(DefaultArgument);
        }

        return sb.ToString();
    }
}