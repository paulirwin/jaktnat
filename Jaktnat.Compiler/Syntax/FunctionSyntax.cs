using System.Text;

namespace Jaktnat.Compiler.Syntax;

public class FunctionSyntax : BodySyntax
{
    public FunctionSyntax(string name, ParameterListSyntax? parameters, BlockSyntax body, bool throws, TypeIdentifierSyntax? returnTypeIdentifier)
        : base(body)
    {
        Name = name;
        Parameters = parameters;
        Throws = throws;
        ReturnTypeIdentifier = returnTypeIdentifier;
    }

    public string Name { get; }

    public ParameterListSyntax? Parameters { get; }

    public bool Throws { get; }

    public TypeIdentifierSyntax? ReturnTypeIdentifier { get; }

    public TypeReference? ReturnType { get; set; }

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