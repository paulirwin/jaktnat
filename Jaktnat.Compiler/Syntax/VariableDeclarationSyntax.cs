using Mono.Cecil.Cil;

namespace Jaktnat.Compiler.Syntax;

public class VariableDeclarationSyntax : SyntaxNode
{
    public VariableDeclarationSyntax(string name, string? typeName, bool mutable)
    {
        Name = name;
        TypeName = typeName;
        Mutable = mutable;
    }
    
    public string Name { get; }

    public string? TypeName { get; set; }
    
    public bool Mutable { get; }

    public ExpressionSyntax? InitializerExpression { get; set; }
    
    public Type? Type { get; set; }
    
    public VariableDefinition? ILVariableDefinition { get; set; }
}