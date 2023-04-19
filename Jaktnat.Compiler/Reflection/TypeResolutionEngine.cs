using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.Reflection;

internal class TypeResolutionEngine :
    ISyntaxVisitor<FunctionSyntax>,
    ISyntaxVisitor<ArrayTypeIdentifierSyntax>,
    ISyntaxVisitor<NamedTypeIdentifierSyntax>,
    ISyntaxVisitor<CatchSyntax>,
    ISyntaxVisitor<ParameterSyntax>
{
    public void Visit(CompilationContext context, FunctionSyntax node)
    {
        if (node.ReturnTypeIdentifier != null)
        {
            VisitTypeSyntax(context, node.ReturnTypeIdentifier);
            node.ReturnType = node.ReturnTypeIdentifier.Type;
        }
        else
            node.ReturnType = typeof(void);
    }
    
    private void VisitTypeSyntax(CompilationContext context, TypeIdentifierSyntax node)
    {
        switch (node)
        {
            case NamedTypeIdentifierSyntax namedTypeIdentifierSyntax:
                Visit(context, namedTypeIdentifierSyntax);
                break;
            case ArrayTypeIdentifierSyntax arrayTypeIdentifierSyntax:
                VisitTypeSyntax(context, arrayTypeIdentifierSyntax.ElementType);
                Visit(context, arrayTypeIdentifierSyntax);
                break;
            default:
                throw new CompilerError($"Unsure how to name resolve a {node.GetType().Name}");
        }
    }

    public void Visit(CompilationContext context, ArrayTypeIdentifierSyntax node)
    {
        if (node.ElementType.Type == null)
        {
            throw new CompilerError("Element type is not defined for array type");
        }

        node.Type = node.ElementType.Type.MakeArrayType();
    }

    public void Visit(CompilationContext context, NamedTypeIdentifierSyntax node)
    {
        if (context.CompilationUnit.TryResolveType(node.Name, out var typeDecl) && typeDecl != null)
        {
            node.Type = typeDecl;
        }
        else if (BuiltInTypeResolver.TryResolveType(node.Name) is Type type)
        {
            node.Type = type;
        }
        else
        {
            throw new CompilerError($"Unable to resolve named type: {node.Name}");
        }
    }

    public void Visit(CompilationContext context, CatchSyntax node)
    {
        node.Identifier.Type = typeof(Exception);
    }

    public void Visit(CompilationContext context, ParameterSyntax node)
    {
        if (node is ThisParameterSyntax)
        {
            return;
        }
        
        if (node.TypeIdentifier?.Type == null)
        {
            throw new CompilerError("Expected type identifier in parameter declaration");
        }
        
        node.Type = node.TypeIdentifier.Type;
    }
}