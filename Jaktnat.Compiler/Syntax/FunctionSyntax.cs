using System.Text;

namespace Jaktnat.Compiler.Syntax;

public class FunctionSyntax : FunctionLikeSyntax
{
    public FunctionSyntax(VisibilityModifier visibilityModifier,
        string name, 
        ParameterListSyntax parameters, 
        SyntaxNode body, 
        bool throws, 
        TypeIdentifierSyntax? returnTypeIdentifier)
        : base(parameters)
    {
        VisibilityModifier = visibilityModifier;
        Name = name;
        Throws = throws;
        ReturnTypeIdentifier = returnTypeIdentifier;

        if (body is BlockSyntax bodyBlock)
        {
            Body = bodyBlock;
        }
        else if (body is ExpressionSyntax bodyExpression)
        {
            Body = new ExpressionBlockSyntax(bodyExpression);
        }
        else
        {
            // HACK.PI: make this a ctor overload
            throw new InvalidOperationException("Body must be a block or expression");
        }
    }

    public VisibilityModifier VisibilityModifier { get; }
    
    public string Name { get; }

    public bool Throws { get; }

    public TypeIdentifierSyntax? ReturnTypeIdentifier { get; }
    
    public BlockSyntax Body { get; }

    public TypeReference? ReturnType { get; set; }

    public bool HasThisParameter => Parameters.Parameters.Count > 0
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