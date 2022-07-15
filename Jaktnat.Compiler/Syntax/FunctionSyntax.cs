using System.Text;

namespace Jaktnat.Compiler.Syntax;

public class FunctionSyntax : BodySyntax
{
    public FunctionSyntax(VisibilityModifier visibilityModifier,
        string name, 
        ParameterListSyntax? parameters, 
        BlockSyntax body, 
        bool throws, 
        TypeIdentifierSyntax? returnTypeIdentifier)
        : base(body)
    {
        VisibilityModifier = visibilityModifier;
        Name = name;
        Parameters = parameters;
        Throws = throws;
        ReturnTypeIdentifier = returnTypeIdentifier;
    }

    public VisibilityModifier VisibilityModifier { get; }
    
    public string Name { get; }

    public ParameterListSyntax? Parameters { get; }

    public bool Throws { get; }

    public TypeIdentifierSyntax? ReturnTypeIdentifier { get; }

    public TypeReference? ReturnType { get; set; }

    public bool HasThisParameter => Parameters != null
                                    && Parameters.Parameters.Count > 0
                                    && Parameters.Parameters[0] is ThisParameterSyntax;

    public override string ToString()
    {
        var sb = new StringBuilder("function ");
        sb.Append(Name).Append('(').Append(Parameters).Append(')');

        if (Throws)
        {
            sb.Append(" throws ");
        }

        if (ReturnTypeIdentifier != null)
        {
            sb.Append(" -> ").Append(ReturnTypeIdentifier);
        }

        return sb.ToString();
    }
}