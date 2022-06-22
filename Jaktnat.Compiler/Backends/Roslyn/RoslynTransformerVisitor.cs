﻿using Jaktnat.Compiler.ObjectModel;
using Jaktnat.Compiler.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using BinaryExpressionSyntax = Jaktnat.Compiler.Syntax.BinaryExpressionSyntax;
using BlockSyntax = Jaktnat.Compiler.Syntax.BlockSyntax;
using CompilationUnitSyntax = Jaktnat.Compiler.Syntax.CompilationUnitSyntax;
using SyntaxNode = Jaktnat.Compiler.Syntax.SyntaxNode;
using RuntimeMethodInfoFreeFunction = Jaktnat.Compiler.ObjectModel.RuntimeMethodInfoFreeFunction;
using LiteralExpressionSyntax = Jaktnat.Compiler.Syntax.LiteralExpressionSyntax;
using VariableDeclarationSyntax = Jaktnat.Compiler.Syntax.VariableDeclarationSyntax;

using CSBlockSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax;
using CSParameterSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax;
using CSExpressionSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax;
using ParameterSyntax = Jaktnat.Compiler.Syntax.ParameterSyntax;
using ParenthesizedExpressionSyntax = Jaktnat.Compiler.Syntax.ParenthesizedExpressionSyntax;

namespace Jaktnat.Compiler.Backends.Roslyn;

internal class RoslynTransformerVisitor : ISyntaxTransformer<CSharpSyntaxNode?>
{
    public CSharpSyntaxNode? Visit(CompilationContext context, SyntaxNode node)
    {
        return node switch
        {
            CompilationUnitSyntax compilationUnit => VisitCompilationUnit(context, compilationUnit),
            FunctionSyntax function => VisitFunction(context, function),
            BlockSyntax block => VisitBlock(context, block),
            CallSyntax call => VisitCall(context, call),
            CallArgumentSyntax callArgument => VisitCallArgument(context, callArgument),
            LiteralExpressionSyntax literal => VisitLiteral(context, literal),
            VariableDeclarationSyntax variableDeclaration => VisitVariableDeclaration(context, variableDeclaration),
            IdentifierExpressionSyntax identifier => VisitIdentifier(context, identifier),
            IfSyntax ifSyntax => VisitIf(context, ifSyntax),
            ParameterSyntax parameter => VisitParameter(context, parameter),
            WhileSyntax whileSyntax => VisitWhile(context, whileSyntax),
            BinaryExpressionSyntax binary => VisitBinary(context, binary),
            TypeCastSyntax typeCast => VisitTypeCast(context, typeCast),
            MemberAccessSyntax memberAccess => VisitMemberAccess(context, memberAccess),
            ParenthesizedExpressionSyntax parenthesized => VisitParenthesized(context, parenthesized),
            IndexerAccessSyntax indexerAccess => VisitIndexerAccess(context, indexerAccess),
            UnaryExpressionSyntax unary => VisitUnary(context, unary),
            ArraySyntax array => VisitArray(context, array),
            _ => throw new NotImplementedException($"Support for visiting {node.GetType()} nodes in Roslyn transformer not yet implemented")
        };
    }

    private CSharpSyntaxNode VisitArray(CompilationContext context, ArraySyntax array)
    {
        var values = array.ItemsList.Items.Select(i => Visit(context, i)).Cast<CSExpressionSyntax>().ToList();

        return SyntaxFactory.ImplicitArrayCreationExpression(
            SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression, SyntaxFactory.SeparatedList(values))
        );
    }

    private CSharpSyntaxNode VisitUnary(CompilationContext context, UnaryExpressionSyntax unary)
    {
        if (Visit(context, unary.Expression) is not CSExpressionSyntax expression)
        {
            throw new CompilerError("Unary expression did not evaluate to an expression");
        }

        var (kind, prefix) = unary.Operator switch
        {
            UnaryOperator.BitwiseNot => (SyntaxKind.BitwiseNotExpression, true),
            UnaryOperator.PreIncrement => (SyntaxKind.PreIncrementExpression, true),
            UnaryOperator.PostIncrement => (SyntaxKind.PostIncrementExpression, false),
            UnaryOperator.PreDecrement => (SyntaxKind.PreDecrementExpression, true),
            UnaryOperator.PostDecrement => (SyntaxKind.PostDecrementExpression, false),
            UnaryOperator.Negate => (SyntaxKind.UnaryMinusExpression, true),
            UnaryOperator.Dereference => (SyntaxKind.PointerIndirectionExpression, true),
            UnaryOperator.RawAddress => (SyntaxKind.AddressOfExpression, true),
            UnaryOperator.LogicalNot => (SyntaxKind.LogicalNotExpression, true),
            _ => throw new ArgumentOutOfRangeException()
        };

        return prefix
            ? SyntaxFactory.PrefixUnaryExpression(kind, expression)
            : SyntaxFactory.PostfixUnaryExpression(kind, expression);
    }

    private CSharpSyntaxNode VisitParenthesized(CompilationContext context, ParenthesizedExpressionSyntax parenthesized)
    {
        if (Visit(context, parenthesized.Expression) is not CSExpressionSyntax expression)
        {
            throw new CompilerError("Parenthesized expression did not evaluate to an expression");
        }

        return SyntaxFactory.ParenthesizedExpression(expression);
    }

    private CSharpSyntaxNode VisitIndexerAccess(CompilationContext context, IndexerAccessSyntax indexerAccess)
    {
        if (Visit(context, indexerAccess.Target) is not CSExpressionSyntax expression)
        {
            throw new CompilerError("Indexer access expression did not evaluate to an expression");
        }

        if (Visit(context, indexerAccess.Argument) is not CSExpressionSyntax arg)
        {
            throw new CompilerError("Indexer access argument expression did not evaluate to an expression");
        }

        return SyntaxFactory.ElementAccessExpression(expression, SyntaxFactory.BracketedArgumentList(SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Argument(arg) })));
    }

    private CSharpSyntaxNode VisitMemberAccess(CompilationContext context, MemberAccessSyntax memberAccess)
    {
        if (Visit(context, memberAccess.Target) is not CSExpressionSyntax expression)
        {
            throw new CompilerError("Member access expression did not evaluate to an expression");
        }

        return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, SyntaxFactory.IdentifierName(memberAccess.Member.Name));
    }

    private CSharpSyntaxNode VisitTypeCast(CompilationContext context, TypeCastSyntax typeCast)
    {
        if (typeCast.Type.Type?.FullName == null)
        {
            throw new CompilerError("Unresolved type for cast");
        }

        if (Visit(context, typeCast.Expression) is not CSExpressionSyntax expression)
        {
            throw new CompilerError("Cast expression did not evaluate to an expression");
        }

        return SyntaxFactory.CastExpression(SyntaxFactory.ParseTypeName(typeCast.Type.Type.FullName), expression);
    }

    private CSharpSyntaxNode VisitBinary(CompilationContext context, BinaryExpressionSyntax binary)
    {
        if (Visit(context, binary.Left) is not CSExpressionSyntax left)
        {
            throw new CompilerError("Left side of binary expression did not evaluate to an expression");
        }

        if (Visit(context, binary.Right) is not CSExpressionSyntax right)
        {
            throw new CompilerError("Left side of binary expression did not evaluate to an expression");
        }

        var kind = binary.Operator switch
        {
            BinaryOperator.Add => SyntaxKind.AddExpression,
            BinaryOperator.Subtract => SyntaxKind.SubtractExpression,
            BinaryOperator.Multiply => SyntaxKind.MultiplyExpression,
            BinaryOperator.Divide => SyntaxKind.DivideExpression,
            BinaryOperator.Modulo => SyntaxKind.ModuloExpression,
            BinaryOperator.Equal => SyntaxKind.EqualsExpression,
            BinaryOperator.NotEqual => SyntaxKind.NotEqualsExpression,
            BinaryOperator.LessThan => SyntaxKind.LessThanExpression,
            BinaryOperator.GreaterThan => SyntaxKind.GreaterThanExpression,
            BinaryOperator.LessThanOrEqual => SyntaxKind.LessThanOrEqualExpression,
            BinaryOperator.GreaterThanOrEqual => SyntaxKind.GreaterThanOrEqualExpression,
            BinaryOperator.LogicalAnd => SyntaxKind.LogicalAndExpression,
            BinaryOperator.LogicalOr => SyntaxKind.LogicalOrExpression,
            BinaryOperator.BitwiseAnd => SyntaxKind.BitwiseAndExpression,
            BinaryOperator.BitwiseOr => SyntaxKind.BitwiseOrExpression,
            BinaryOperator.BitwiseXor => SyntaxKind.ExclusiveOrExpression,
            BinaryOperator.BitwiseLeftShift => SyntaxKind.LeftShiftExpression,
            BinaryOperator.BitwiseRightShift => SyntaxKind.RightShiftExpression,
            BinaryOperator.ArithmeticLeftShift => throw new NotImplementedException("Arithmetic left shift not supported"),
            BinaryOperator.ArithmeticRightShift => throw new NotImplementedException("Arithmetic right shift not supported"),
            BinaryOperator.Assign => SyntaxKind.SimpleAssignmentExpression,
            BinaryOperator.AddAssign => SyntaxKind.AddAssignmentExpression,
            BinaryOperator.SubtractAssign => SyntaxKind.SubtractAssignmentExpression,
            BinaryOperator.MultiplyAssign => SyntaxKind.MultiplyAssignmentExpression,
            BinaryOperator.DivideAssign => SyntaxKind.DivideAssignmentExpression,
            BinaryOperator.ModuloAssign => SyntaxKind.ModuloAssignmentExpression,
            BinaryOperator.BitwiseAndAssign => SyntaxKind.AndAssignmentExpression,
            BinaryOperator.BitwiseOrAssign => SyntaxKind.OrAssignmentExpression,
            BinaryOperator.BitwiseXorAssign => SyntaxKind.ExclusiveOrAssignmentExpression,
            BinaryOperator.BitwiseLeftShiftAssign => SyntaxKind.LeftShiftAssignmentExpression,
            BinaryOperator.BitwiseRightShiftAssign => SyntaxKind.RightShiftAssignmentExpression,
            BinaryOperator.NoneCoalescing => SyntaxKind.CoalesceExpression, // TODO: is this correct?
            BinaryOperator.NoneCoalescingAssign => SyntaxKind.CoalesceAssignmentExpression, // TODO: is this correct?
            _ => throw new ArgumentOutOfRangeException()
        };

        return kind is SyntaxKind.SimpleAssignmentExpression 
            or SyntaxKind.AddAssignmentExpression 
            or SyntaxKind.SubtractAssignmentExpression 
            or SyntaxKind.MultiplyAssignmentExpression 
            or SyntaxKind.DivideAssignmentExpression 
            or SyntaxKind.ModuloAssignmentExpression 
            or SyntaxKind.AndAssignmentExpression
            or SyntaxKind.OrAssignmentExpression
            or SyntaxKind.ExclusiveOrAssignmentExpression
            or SyntaxKind.LeftShiftAssignmentExpression
            or SyntaxKind.RightShiftAssignmentExpression
            or SyntaxKind.CoalesceAssignmentExpression
            ? SyntaxFactory.AssignmentExpression(kind, left, right)
            : SyntaxFactory.BinaryExpression(kind, left, right);
    }

    private CSharpSyntaxNode VisitWhile(CompilationContext context, WhileSyntax whileSyntax)
    {
        if (Visit(context, whileSyntax.Condition) is not CSExpressionSyntax condition)
        {
            throw new CompilerError("Condition did not evaluate to an expression");
        }

        if (Visit(context, whileSyntax.Body) is not CSBlockSyntax body)
        {
            throw new CompilerError("Body did not evaluate to a block");
        }

        return SyntaxFactory.WhileStatement(condition, body);
    }

    private CSharpSyntaxNode VisitParameter(CompilationContext context, ParameterSyntax parameter)
    {
        if (parameter.Type?.FullName == null)
        {
            throw new CompilerError("Parameter must have a type");
        }

        return SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameter.Name))
            .WithType(SyntaxFactory.ParseTypeName(parameter.Type.FullName));
    }

    private CSharpSyntaxNode VisitIf(CompilationContext context, IfSyntax ifSyntax)
    {
        if (Visit(context, ifSyntax.Condition) is not CSExpressionSyntax condition)
        {
            throw new CompilerError("Condition did not evaluate to an expression");
        }

        if (Visit(context, ifSyntax.Body) is not CSBlockSyntax body)
        {
            throw new CompilerError("Body did not evaluate to a block");
        }

        return SyntaxFactory.IfStatement(condition, body);
    }

    private CSharpSyntaxNode VisitIdentifier(CompilationContext context, IdentifierExpressionSyntax identifier)
    {
        return SyntaxFactory.IdentifierName(identifier.Name);
    }

    private CSharpSyntaxNode VisitVariableDeclaration(CompilationContext context, VariableDeclarationSyntax variableDeclaration)
    {
        var type = variableDeclaration.TypeIdentifier?.Type
            ?? variableDeclaration.InitializerExpression?.ExpressionType;

        if (type?.FullName == null)
        {
            throw new CompilerError("Type not determined for variable declaration");
        }

        var initializer = variableDeclaration.InitializerExpression != null ? Visit(context, variableDeclaration.InitializerExpression) as CSExpressionSyntax : null;
        
        return SyntaxFactory.LocalDeclarationStatement(
            SyntaxFactory.VariableDeclaration(
                SyntaxFactory.ParseTypeName(type.FullName),
                SyntaxFactory.SeparatedList(new[] {
                    SyntaxFactory.VariableDeclarator(variableDeclaration.Name).WithInitializer(initializer != null ? SyntaxFactory.EqualsValueClause(initializer) : null)
                })));
    }

    private CSharpSyntaxNode VisitLiteral(CompilationContext context, LiteralExpressionSyntax literal)
    {
        var kind = literal.Value switch
        {
            byte or sbyte or short or ushort or int or uint or long or ulong or nint or nuint or float or double or decimal => SyntaxKind.NumericLiteralExpression,
            char => SyntaxKind.CharacterLiteralExpression,
            string => SyntaxKind.StringLiteralExpression,
            true => SyntaxKind.TrueLiteralExpression,
            false => SyntaxKind.FalseLiteralExpression,
            _ => throw new NotImplementedException($"Literal expressions of type {literal.Value.GetType()} not yet implemented"),
        };

        var text = literal.Value switch
        {
            true => "true",
            false => "false",
            long l => $"{l}L",
            float f => $"{f}f",
            double d => $"{d}d",
            decimal m => $"{m}m",
            _ => literal.ToString() ?? throw new CompilerError("Literal evaluated to null")
        };

        return SyntaxFactory.LiteralExpression(kind, SyntaxFactory.ParseToken(text));
    }

    private CSharpSyntaxNode VisitCallArgument(CompilationContext context, CallArgumentSyntax callArgument)
    {
        if (Visit(context, callArgument.Expression) is not CSExpressionSyntax expr)
        {
            throw new CompilerError("Argument was not an expression");
        }

        return callArgument.ParameterName != null
            ? SyntaxFactory.Argument(expr).WithNameColon(SyntaxFactory.NameColon(callArgument.ParameterName))
            : SyntaxFactory.Argument(expr);
    }

    private CSharpSyntaxNode VisitCall(CompilationContext context, CallSyntax call)
    {
        if (call.MatchedMethod == null)
        {
            throw new CompilerError("No matched method to call");
        }

        var target = call.MatchedMethod switch
        {
            DeclaredFreeFunction declared => SyntaxFactory.IdentifierName(declared.FunctionSyntax.Name),
            RuntimeMethodInfoFreeFunction { Method: { DeclaringType: {} type } method } => SyntaxFactory.ParseName($"{type.FullName}.{method.Name}"),
            _ => throw new NotImplementedException("Unexpected free function type")
        };

        var args = new List<ArgumentSyntax>();

        foreach (var arg in call.Arguments)
        {
            var argValue = Visit(context, arg);

            if (argValue is not ArgumentSyntax argExpr)
            {
                throw new CompilerError("Only expressions can be used as arguments");
            }
            
            args.Add(argExpr);
        }

        return SyntaxFactory.InvocationExpression(target, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(args)));
    }

    private CSharpSyntaxNode VisitBlock(CompilationContext context, BlockSyntax block)
    {
        var statements = new List<StatementSyntax>();

        foreach (var child in block.Children)
        {
            var value = Visit(context, child);

            if (value == null)
            {
                continue;
            }

            var statement = value switch
            {
                StatementSyntax s => s,
                CSExpressionSyntax e => SyntaxFactory.ExpressionStatement(e),
                _ => throw new CompilerError("Unexpected non-statement in block"),
            };
            
            statements.Add(statement);
        }

        return SyntaxFactory.Block(statements);
    }

    private CSharpSyntaxNode VisitFunction(CompilationContext context, FunctionSyntax function)
    {
        if (function.ReturnType?.FullName == null)
        {
            throw new CompilerError("Function return type is null");
        }

        var returnType = function.ReturnType.FullName switch
        {
            "System.Void" => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
            _ => SyntaxFactory.ParseTypeName(function.ReturnType.FullName)
        };

        var parameters = new List<CSParameterSyntax>();

        if (function.Parameters != null)
        {
            foreach (var parameter in function.Parameters.Parameters)
            {
                var value = Visit(context, parameter);

                if (value is CSParameterSyntax csParameter)
                {
                    parameters.Add(csParameter);
                }
                else
                {
                    throw new CompilerError("Unexpected parameter value for function");
                }
            }
        }

        if (Visit(context, function.Body) is not CSBlockSyntax body)
        {
            throw new CompilerError("Only block syntax currently supported for function body");
        }
        
        string name = function.Name;

        if (function.Name == "main")
        {
            name = "Main";

            if (parameters.Count == 0)
            {
                parameters.Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier("args")).WithType(SyntaxFactory.ParseTypeName("string[]")));
            }
        }

        return SyntaxFactory.MethodDeclaration(returnType, name)
            .WithModifiers(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PublicKeyword)).Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword))) // HACK.PI: only static methods for now
            .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)))
            .WithBody(body);
    }

    private CSharpSyntaxNode VisitCompilationUnit(CompilationContext context, CompilationUnitSyntax compilationUnit)
    {
        var members = new List<MemberDeclarationSyntax>();
        bool addProgramClass = false;

        if (context.RoslynProgramClass == null)
        {
            context.RoslynProgramClass = SyntaxFactory.ClassDeclaration("Program")
                .WithModifiers(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PublicKeyword)).Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword)));

            addProgramClass = true;
        }

        foreach (var child in compilationUnit.Children)
        {
            var result = Visit(context, child);

            if (result is ClassDeclarationSyntax classDecl)
            {
                members.Add(classDecl);
            }
            else if (result is MethodDeclarationSyntax method)
            {
                // top-level function goes in program class
                context.RoslynProgramClass = context.RoslynProgramClass.AddMembers(method);
            }
            else if (result != null)
            {
                throw new NotImplementedException($"Unexpected transformed child of compilation unit: {result.GetType()}");
            }
        }

        if (addProgramClass)
        {
            members.Add(context.RoslynProgramClass);
        }

        return SyntaxFactory.CompilationUnit()
            .WithMembers(SyntaxFactory.List(new MemberDeclarationSyntax[]
            {
                SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(context.AssemblyName))
                    .WithMembers(SyntaxFactory.List(members))
            }));
    }
}