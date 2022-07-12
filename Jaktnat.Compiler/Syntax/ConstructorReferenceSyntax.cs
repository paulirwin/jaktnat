namespace Jaktnat.Compiler.Syntax;

public abstract class ConstructorReferenceSyntax : SyntaxNode
{
    protected ConstructorReferenceSyntax(TypeReference declaringType, ParameterListSyntax parameters)
    {
        DeclaringType = declaringType;
        Parameters = parameters;
    }

    public TypeReference DeclaringType { get; }
    
    public ParameterListSyntax Parameters { get; }

    public override string ToString() => $"{DeclaringType.Name}({Parameters})";
}