﻿using Jaktnat.Compiler.Syntax;

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
        InvokePreVisit(obj, context, node);
        
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

            VisitInternal(obj, context, function.Parameters);
            VisitInternal(obj, context, function.Body);
        }
        else if (node is ConstructorSyntax constructor)
        {
            VisitInternal(obj, context, constructor.Parameters);
        }
        else if (node is ExpressionBlockSyntax expressionBlock)
        {
            VisitInternal(obj, context, expressionBlock.Children[0]);
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

            if (parameter.DefaultArgument != null)
            {
                VisitInternal(obj, context, parameter.DefaultArgument);
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
        else if (node is GuardSyntax guardSyntax)
        {
            VisitInternal(obj, context, guardSyntax.Condition);
            VisitInternal(obj, context, guardSyntax.ElseNode);
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
            if (memberAccess.Target is { } target)
            {
                VisitInternal(obj, context, target);
            }

            VisitInternal(obj, context, memberAccess.Member);
        }
        else if (node is IndexerAccessSyntax indexerAccess)
        {
            VisitInternal(obj, context, indexerAccess.Target);
            VisitInternal(obj, context, indexerAccess.Argument);
        }
        else if (node is TypeDeclarationSyntax typeDeclaration)
        {
            // NOTE: this branch handles ClassDeclarationSyntax and StructDeclarationSyntax
            
            foreach (var typeCtor in typeDeclaration.Constructors)
            {
                VisitInternal(obj, context, typeCtor);
            }
            
            foreach (var member in typeDeclaration.Members)
            {
                VisitInternal(obj, context, member);
            }
        }
        else if (node is PropertySyntax property)
        {
            VisitInternal(obj, context, property.TypeIdentifier);
        }
        else if (node is MemberFunctionDeclarationSyntax memberFunction)
        {
            VisitInternal(obj, context, memberFunction.Function);
        }
        else if (node is TrySyntax trySyntax)
        {
            VisitInternal(obj, context, trySyntax.Tryable);
            VisitInternal(obj, context, trySyntax.Catch);
        }
        else if (node is CatchSyntax catchSyntax)
        {
            VisitInternal(obj, context, catchSyntax.Identifier);
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
        else if (node is DeferSyntax deferSyntax)
        {
            VisitInternal(obj, context, deferSyntax.Body);
        }
        else if (node is UnsafeBlockSyntax unsafeBlock)
        {
            VisitInternal(obj, context, unsafeBlock.Block);
        }
        else if (node is CSharpBlockSyntax csharpBlock)
        {
            VisitInternal(obj, context, csharpBlock.Block);
        }
        else if (node is ForInSyntax forInSyntax)
        {
            VisitInternal(obj, context, forInSyntax.Expression);
            VisitInternal(obj, context, forInSyntax.Identifier);
            
            // HACK.PI: Invoke an extra visit here before visiting block
            InvokeVisit(obj, context, node);
            
            VisitInternal(obj, context, forInSyntax.Block);
        }
        else if (node is MatchStatementSyntax matchStatementSyntax)
        {
            VisitInternal(obj, context, matchStatementSyntax.MatchExpression);
        }
        else if (node is MatchExpressionSyntax matchExpressionSyntax)
        {
            VisitInternal(obj, context, matchExpressionSyntax.Expression);
            
            foreach (var matchCase in matchExpressionSyntax.Cases)
            {
                VisitInternal(obj, context, matchCase);
            }
        }
        else if (node is MatchCaseSyntax matchCaseSyntax)
        {
            foreach (var patternSyntax in matchCaseSyntax.Patterns)
            {
                VisitInternal(obj, context, patternSyntax);
            }
            
            VisitInternal(obj, context, matchCaseSyntax.Body);
        }
        else if (node is MatchCasePatternExpressionSyntax patternExpressionSyntax)
        {
            VisitInternal(obj, context, patternExpressionSyntax.Expression);
        }
        else if (node is MatchCaseBlockBodySyntax matchCaseBlockBodySyntax)
        {
            VisitInternal(obj, context, matchCaseBlockBodySyntax.Block);
        }
        else if (node is MatchCaseExpressionBodySyntax matchCaseExpressionBodySyntax)
        {
            VisitInternal(obj, context, matchCaseExpressionBodySyntax.Expression);
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
    
    private static void InvokePreVisit<T>(T obj, CompilationContext context, SyntaxNode node)
    {
        var nodeType = node.GetType();
        var visitorType = VisitorType.MakeGenericType(nodeType);

        if (!visitorType.IsAssignableFrom(typeof(T)))
        {
            return;
        }

        var preVisitMethod = visitorType.GetMethod(nameof(ISyntaxVisitor<SyntaxNode>.PreVisit))!;

        preVisitMethod.Invoke(obj, new object?[] { context, node });
    }
}