using System.Reflection;
using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler;

internal static class SyntaxVisitor
{
    private static readonly Type VisitorType = typeof(ISyntaxVisitor<>);

    public static void Visit<T>(CompilationContext context, SyntaxNode node)
        where T : class, new()
    {
        var obj = new T();

        VisitInternal(obj, context, node);
    }

    private static void VisitInternal<T>(T obj, CompilationContext context, SyntaxNode node)
    {
        if (node is CompositeSyntax composite)
        {
            foreach (var child in composite.Children)
            {
                VisitInternal(obj, context, child);
            }
        }
        else if (node is FunctionSyntax function)
        {
            VisitInternal(obj, context, function.Body);
        }
        else if (node is IfSyntax ifSyntax)
        {
            VisitInternal(obj, context, ifSyntax.Condition);
            VisitInternal(obj, context, ifSyntax.Body);
        }
        else if (node is CallSyntax call)
        {
            VisitInternal(obj, context, call.Target);

            foreach (var argument in call.Arguments)
            {
                VisitInternal(obj, context, argument);
            }
        }
        else if (node is CallArgumentSyntax argument)
        {
            VisitInternal(obj, context, argument.Expression);
        }
        else if (node is VariableDeclarationSyntax varDecl)
        {
            if (varDecl.InitializerExpression != null)
            {
                VisitInternal(obj, context, varDecl.InitializerExpression);
            }
        }

        InvokeVisit(obj, context, node);
    }

    private static void InvokeVisit<T>(T obj, CompilationContext context, SyntaxNode node)
    {
        var nodeType = node.GetType();
        var visitorType = VisitorType.MakeGenericType(nodeType);

        if (!visitorType.IsAssignableFrom(typeof(T)))
        {
            return;
        }

        var visitMethod = visitorType.GetMethod(nameof(ISyntaxVisitor<SyntaxNode>.Visit))!;

        visitMethod.Invoke(obj, new object?[] { context, node });
    }
}