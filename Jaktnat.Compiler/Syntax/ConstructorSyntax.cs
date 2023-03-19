namespace Jaktnat.Compiler.Syntax;

public class ConstructorSyntax : FunctionLikeSyntax
{
    public ConstructorSyntax(TypeReference declaringType, ParameterListSyntax parameters)
        : base(parameters)
    {
        DeclaringType = declaringType;
    }
    
    public TypeReference DeclaringType { get; }

    public override string ToString() => $".ctor({Parameters})";
}