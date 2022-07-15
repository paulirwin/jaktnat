namespace Jaktnat.Compiler.Syntax;

public class IdentifierExpressionSyntax : ExpressionSyntax
{
    public IdentifierExpressionSyntax(string name)
    {
        Name = name;
    }

    public string Name { get; }
    
    public ExpressionSyntax? ParentTarget { get; set; }

    public override string ToString() => Name;
    
    public override bool Mutates => false;

    public override bool PreventsMutation => 
        CompileTimeTarget is DeclarationSyntax { Mutable: false }; // TODO: is this complete?
}