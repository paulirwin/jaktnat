using Jaktnat.Compiler.ObjectModel;
using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler.Reflection;

internal class ForwardDeclarationEngine :
    ISyntaxVisitor<ClassDeclarationSyntax>,
    ISyntaxVisitor<StructDeclarationSyntax>,
    ISyntaxVisitor<FunctionSyntax>,
    ISyntaxVisitor<MemberFunctionDeclarationSyntax>,
    ISyntaxVisitor<ParameterSyntax>
{
    public void PreVisit(CompilationContext context, ClassDeclarationSyntax node)
    {
        foreach (var member in node.Members)
        {
            member.DeclaringType = node;
        }
    }

    public void Visit(CompilationContext context, ClassDeclarationSyntax node)
    {
        context.CompilationUnit.DeclareType(node.Name, node);
        
        var parameters = node.Members.OfType<PropertySyntax>()
            .Select(i => new ParameterSyntax(
                anonymous: false, 
                name: i.Name, 
                mutable: false, 
                typeIdentifier: i.TypeIdentifier, 
                defaultArgument: i.DefaultExpression)
            {
                ParentBlock = node.ParentBlock
            })
            .ToList();

        node.Constructors.Add(new ConstructorSyntax(node, new ParameterListSyntax(parameters)));
    }
    
    public void PreVisit(CompilationContext context, StructDeclarationSyntax node)
    {
        foreach (var member in node.Members)
        {
            member.DeclaringType = node;
        }
    }
    
    public void Visit(CompilationContext context, StructDeclarationSyntax node)
    {
        context.CompilationUnit.DeclareType(node.Name, node);
        
        var parameters = node.Members.OfType<PropertySyntax>()
            .Select(i => new ParameterSyntax(
                anonymous: false, 
                name: i.Name, 
                mutable: false, 
                typeIdentifier: i.TypeIdentifier, 
                defaultArgument: i.DefaultExpression)
            {
                ParentBlock = node.ParentBlock
            })
            .ToList();
        
        node.Constructors.Add(new ConstructorSyntax(node, new ParameterListSyntax(parameters)));
    }

    public void Visit(CompilationContext context, FunctionSyntax node)
    {
        context.CompilationUnit.DeclareFreeFunction(node.Name, new DeclaredFunction(null, node));
    }

    public void PreVisit(CompilationContext context, MemberFunctionDeclarationSyntax node)
    {
        // HACK.PI: "this" parameters need a super early declaring type resolution
        foreach (var parameter in node.Function.Parameters.Parameters)
        {
            if (parameter is ThisParameterSyntax thisParameter)
            {
                thisParameter.DeclaringType = node.DeclaringType;
            }
        }
    }

    public void Visit(CompilationContext context, MemberFunctionDeclarationSyntax node)
    {
        node.Function.Body.DeclaringType = node.DeclaringType; // for `this` expression
    }

    public void Visit(CompilationContext context, ParameterSyntax node)
    {
        if (node is ThisParameterSyntax thisParameter)
        {
            if (thisParameter.DeclaringType == null)
            {
                throw new CompilerError("`this` did not resolve to anything");
            }

            node.Type = thisParameter.DeclaringType;
            return;
        }
        
        if (node.ParentBlock == null)
        {
            throw new CompilerError("Expected parameter to have a block");
        }

        node.ParentBlock.Declarations.Add(node.Name, node);
    }
}