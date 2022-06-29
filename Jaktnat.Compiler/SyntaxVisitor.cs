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
        if (node is AggregateSyntax composite)
        {
            foreach (var child in composite.Children)
            {
                VisitInternal(obj, context, child);
            }
        }
        else if (node is FunctionSyntax function)
        {
            if (function.ReturnTypeIdentifier != null)
            {
                VisitInternal(obj, context, function.ReturnTypeIdentifier);
            }

            if (function.Parameters != null)
            {
                VisitInternal(obj, context, function.Parameters);
            }

            VisitInternal(obj, context, function.Body);
        }
        else if (node is ParameterListSyntax parameterList)
        {
            foreach (var parameter in parameterList.Parameters)
            {
                VisitInternal(obj, context, parameter);
            }
        }
        else if (node is ParameterSyntax parameter)
        {
            if (parameter.TypeIdentifier != null)
            {
                VisitInternal(obj, context, parameter.TypeIdentifier);
            }
        }
        else if (node is ArraySyntax array)
        {
            VisitInternal(obj, context, array.ItemsList);
        }
        else if (node is ExpressionListSyntax expressionList)
        {
            foreach (var item in expressionList.Items)
            {
                VisitInternal(obj, context, item);
            }
        }
        else if (node is ArrayTypeIdentifierSyntax arrayType)
        {
            VisitInternal(obj, context, arrayType.ElementType);
        }
        else if (node is IfSyntax ifSyntax)
        {
            VisitInternal(obj, context, ifSyntax.Condition);
            VisitInternal(obj, context, ifSyntax.Body);

            if (ifSyntax.ElseNode != null)
            {
                VisitInternal(obj, context, ifSyntax.ElseNode);
            }
        }
        else if (node is ElseSyntax elseSyntax)
        {
            VisitInternal(obj, context, elseSyntax.Child);
        }
        else if (node is WhileSyntax whileSyntax)
        {
            VisitInternal(obj, context, whileSyntax.Condition);
            VisitInternal(obj, context, whileSyntax.Body);
        }
        else if (node is LoopSyntax loopSyntax)
        {
            VisitInternal(obj, context, loopSyntax.Body);
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

            if (varDecl.TypeIdentifier != null)
            {
                VisitInternal(obj, context, varDecl.TypeIdentifier);
            }
        }
        else if (node is TypeCastSyntax typeCast)
        {
            VisitInternal(obj, context, typeCast.Expression);
            VisitInternal(obj, context, typeCast.Type);
        }
        else if (node is TypeCheckSyntax typeCheck)
        {
            VisitInternal(obj, context, typeCheck.Expression);
            VisitInternal(obj, context, typeCheck.Type);
        }
        else if (node is UnaryExpressionSyntax unaryExpression)
        {
            VisitInternal(obj, context, unaryExpression.Expression);
        }
        else if (node is BinaryExpressionSyntax binaryExpression)
        {
            VisitInternal(obj, context, binaryExpression.Left);
            VisitInternal(obj, context, binaryExpression.Right);
        }
        else if (node is ParenthesizedExpressionSyntax parenthesizedExpression)
        {
            VisitInternal(obj, context, parenthesizedExpression.Expression);
        }
        else if (node is MemberAccessSyntax memberAccess)
        {
            VisitInternal(obj, context, memberAccess.Target);

            // HACK: set parent target on member identifier
            memberAccess.Member.ParentTarget = memberAccess.Target;

            VisitInternal(obj, context, memberAccess.Member);
        }
        else if (node is IndexerAccessSyntax indexerAccess)
        {
            VisitInternal(obj, context, indexerAccess.Target);
            VisitInternal(obj, context, indexerAccess.Argument);
        }
        else if (node is ClassDeclarationSyntax classDeclaration)
        {
            foreach (var member in classDeclaration.Members)
            {
                VisitInternal(obj, context, member);
            }
        }
        else if (node is PropertySyntax property)
        {
            VisitInternal(obj, context, property.TypeIdentifier);
        }
        else if (node is TrySyntax trySyntax)
        {
            VisitInternal(obj, context, trySyntax.Tryable);
            VisitInternal(obj, context, trySyntax.Catch);
        }
        else if (node is CatchSyntax catchSyntax)
        {
            VisitInternal(obj, context, catchSyntax.CatchIdentifier);
            VisitInternal(obj, context, catchSyntax.CatchBlock);
        }
        else if (node is ScopeAccessSyntax scopeAccess)
        {
            VisitInternal(obj, context, scopeAccess.Scope);

            // HACK: set parent target on member identifier
            scopeAccess.Member.ParentTarget = scopeAccess.Scope;

            VisitInternal(obj, context, scopeAccess.Member);
        }
        else if (node is ReturnSyntax { Expression: { } returnExpr })
        {
            VisitInternal(obj, context, returnExpr);
        }
        else if (node is ThrowSyntax { Expression: { } throwExpr })
        {
            VisitInternal(obj, context, throwExpr);
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