namespace Jaktnat.Compiler.Syntax;

public class ThisParameterSyntax : ParameterSyntax
{
    public ThisParameterSyntax(bool mutable) 
        : base(false, "this", mutable, null, null)
    {
    }

    public TypeDeclarationSyntax? DeclaringType { get; set; }

    public override string ToString() => Mutable ? "mut this" : "this";
}