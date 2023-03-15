using System.Text;
using Jaktnat.Compiler.ObjectModel;
using Jaktnat.Compiler.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using BinaryExpressionSyntax = Jaktnat.Compiler.Syntax.BinaryExpressionSyntax;
using BlockSyntax = Jaktnat.Compiler.Syntax.BlockSyntax;
using ClassDeclarationSyntax = Jaktnat.Compiler.Syntax.ClassDeclarationSyntax;
using CompilationUnitSyntax = Jaktnat.Compiler.Syntax.CompilationUnitSyntax;
using SyntaxNode = Jaktnat.Compiler.Syntax.SyntaxNode;
using RuntimeMethodInfoFunction = Jaktnat.Compiler.ObjectModel.RuntimeMethodInfoFunction;
using LiteralExpressionSyntax = Jaktnat.Compiler.Syntax.LiteralExpressionSyntax;
using VariableDeclarationSyntax = Jaktnat.Compiler.Syntax.VariableDeclarationSyntax;
using ParameterSyntax = Jaktnat.Compiler.Syntax.ParameterSyntax;
using ParenthesizedExpressionSyntax = Jaktnat.Compiler.Syntax.ParenthesizedExpressionSyntax;
using CSBlockSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax;
using CSParameterSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax;
using CSExpressionSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax;
using CSMemberDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax;
using CSClassDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax;
using CSMethodDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax;
using CSCompilationUnitSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax;
using ExpressionSyntax = Jaktnat.Compiler.Syntax.ExpressionSyntax;
using ThisExpressionSyntax = Jaktnat.Compiler.Syntax.ThisExpressionSyntax;

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
            LoopSyntax loopSyntax => VisitLoop(context, loopSyntax),
            BinaryExpressionSyntax binary => VisitBinary(context, binary),
            TypeCastSyntax typeCast => VisitTypeCast(context, typeCast),
            MemberAccessSyntax memberAccess => VisitMemberAccess(context, memberAccess),
            ParenthesizedExpressionSyntax parenthesized => VisitParenthesized(context, parenthesized),
            IndexerAccessSyntax indexerAccess => VisitIndexerAccess(context, indexerAccess),
            UnaryExpressionSyntax unary => VisitUnary(context, unary),
            ArraySyntax array => VisitArray(context, array),
            ClassDeclarationSyntax classDecl => VisitClassDeclaration(context, classDecl),
            MemberFunctionDeclarationSyntax memberFunction => VisitMemberFunction(context, memberFunction),
            BreakSyntax => SyntaxFactory.BreakStatement(),
            ContinueSyntax => SyntaxFactory.ContinueStatement(),
            ReturnSyntax returnSyntax => VisitReturn(context, returnSyntax),
            ThrowSyntax throwSyntax => VisitThrow(context, throwSyntax),
            TrySyntax trySyntax => VisitTry(context, trySyntax),
            ThisExpressionSyntax => SyntaxFactory.ThisExpression(),
            PropertySyntax property => VisitProperty(context, property),
            DeferSyntax defer => VisitDefer(context, defer),
            UnsafeBlockSyntax unsafeBlock => VisitUnsafeBlock(context, unsafeBlock),
            CSharpBlockSyntax csharpBlock => VisitCSharpBlock(context, csharpBlock),
            ForInSyntax forInSyntax => VisitForInSyntax(context, forInSyntax),
            _ => throw new NotImplementedException(
                $"Support for visiting {node.GetType()} nodes in Roslyn transformer not yet implemented")
        };
    }

    private CSharpSyntaxNode? VisitForInSyntax(CompilationContext context, ForInSyntax forInSyntax)
    {
        if (Visit(context, forInSyntax.Expression) is not CSExpressionSyntax expression)
        {
            throw new CompilerError("Expected an expression for for-in statement target");
        }

        if (Visit(context, forInSyntax.Block) is not CSBlockSyntax block)
        {
            // TODO: support block-less expression statements?
            throw new CompilerError("Expected a block for for-in statement body");
        }

        return SyntaxFactory.ForEachStatement(
            SyntaxFactory.IdentifierName("var"),
            forInSyntax.Identifier.Name,
            expression,
            block);
    }

    private CSharpSyntaxNode? VisitCSharpBlock(CompilationContext context, CSharpBlockSyntax csharpBlock)
    {
        var sb = new StringBuilder();

        foreach (var child in csharpBlock.Block.Children)
        {
            if (child is not LiteralExpressionSyntax { Value: string csLine })
            {
                throw new CompilerError("Only string literals are allowed inside inline C# blocks");
            }

            sb.Append(csLine);
        }

        var syntaxTree = CSharpSyntaxTree.ParseText(sb.ToString());

        if (syntaxTree.GetRoot() is not CSCompilationUnitSyntax rootNode)
        {
            throw new CompilerError("Unable to parse inline C# block");
        }

        var statements = new List<StatementSyntax>();

        foreach (var rootMember in rootNode.Members)
        {
            if (rootMember is GlobalStatementSyntax global)
            {
                statements.Add(global.Statement);
            }
            else
            {
                throw new CompilerError($"Expected a statement in inline C# block, got a {rootMember.GetType()}");
            }
        }

        return SyntaxFactory.Block(statements);
    }

    private CSharpSyntaxNode? VisitUnsafeBlock(CompilationContext context, UnsafeBlockSyntax unsafeBlock)
    {
        return Visit(context, unsafeBlock.Block);
    }

    private CSharpSyntaxNode? VisitDefer(CompilationContext context, DeferSyntax defer)
    {
        var body = Visit(context, defer.Body);

        if (body == null)
        {
            throw new CompilerError("Defer statements must have a block or expression");
        }

        // HACK.PI: possible hash code collision, should do this differently
        var identifier = $"defer{defer.GetHashCode()}";

        var deferTypeIdentifier = SyntaxFactory.QualifiedName(
            SyntaxFactory.QualifiedName(
                SyntaxFactory.IdentifierName("Jaktnat"),
                SyntaxFactory.IdentifierName("Runtime")),
            SyntaxFactory.IdentifierName("Defer"));

        var lambda = SyntaxFactory.ParenthesizedLambdaExpression().WithBody(body);

        // i.e.: using var defer0 = new Jaktnat.Runtime.Defer(() => ...);
        return SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                    .WithVariables(SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(identifier)
                            ).WithInitializer(SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.ObjectCreationExpression(deferTypeIdentifier)
                                    .WithArgumentList(SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
                                            SyntaxFactory.Argument(lambda))))
                            ))
                        )
                    )
            )
            .WithUsingKeyword(SyntaxFactory.Token(SyntaxKind.UsingKeyword));
    }

    private CSharpSyntaxNode? VisitProperty(CompilationContext context, PropertySyntax property)
    {
        if (property.Type?.FullName == null)
        {
            throw new CompilerError("Property must have a type");
        }

        return SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(property.Type.FullName),
                SyntaxFactory.Identifier(property.Name))
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(new[]
            {
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
            })));
    }

    private CSharpSyntaxNode VisitMemberFunction(CompilationContext context,
        MemberFunctionDeclarationSyntax memberFunction)
    {
        if (Visit(context, memberFunction.Function) is not CSMethodDeclarationSyntax method)
        {
            throw new CompilerError("Member function did not evaluate to a method");
        }

        return method;
    }

    private CSharpSyntaxNode VisitThrow(CompilationContext context, ThrowSyntax throwSyntax)
    {
        if (throwSyntax.Expression == null)
        {
            return SyntaxFactory.ThrowStatement();
        }

        if (Visit(context, throwSyntax.Expression) is not CSExpressionSyntax expression)
        {
            throw new CompilerError("Throw expression was invalid");
        }

        return SyntaxFactory.ThrowStatement(expression);
    }

    private CSharpSyntaxNode VisitTry(CompilationContext context, TrySyntax trySyntax)
    {
        var tryable = Visit(context, trySyntax.Tryable);
        if (tryable is not CSBlockSyntax tryableBlock)
        {
            if (tryable is CSExpressionSyntax tryableExpression)
            {
                tryableBlock = SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(tryableExpression));
            }
            else
            {
                throw new CompilerError("Try body did not evaluate to a block or expression");
            }
        }

        var catches = new List<CatchClauseSyntax>();

        if (Visit(context, trySyntax.Catch.CatchBlock) is not CSBlockSyntax catchBlock)
        {
            throw new CompilerError("Catch body did not evaluate to a block");
        }

        catches.Add(SyntaxFactory.CatchClause(
            SyntaxFactory.CatchDeclaration(SyntaxFactory.ParseTypeName("Exception"),
                SyntaxFactory.ParseToken(trySyntax.Catch.Identifier.Name)),
            null,
            catchBlock));

        return SyntaxFactory.TryStatement(tryableBlock, SyntaxFactory.List(catches), null);
    }

    private CSharpSyntaxNode VisitReturn(CompilationContext context, ReturnSyntax returnSyntax)
    {
        if (returnSyntax.Expression == null)
        {
            return SyntaxFactory.ReturnStatement();
        }

        if (Visit(context, returnSyntax.Expression) is not CSExpressionSyntax expression)
        {
            throw new CompilerError("Return expression was invalid");
        }

        return SyntaxFactory.ReturnStatement(expression);
    }

    private CSharpSyntaxNode VisitLoop(CompilationContext context, LoopSyntax loopSyntax)
    {
        if (Visit(context, loopSyntax.Body) is not CSBlockSyntax body)
        {
            throw new CompilerError("Body did not evaluate to a block");
        }

        return SyntaxFactory.WhileStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression), body);
    }

    private CSharpSyntaxNode VisitClassDeclaration(CompilationContext context, ClassDeclarationSyntax classDecl)
    {
        if (classDecl.Constructors.Count != 1)
        {
            throw new NotImplementedException("Records must have one constructor");
        }

        var parameters = classDecl.Constructors[0].Parameters.Parameters.Select(i => VisitParameter(context, i))
            .Cast<CSParameterSyntax>().ToList();

        var ctor = SyntaxFactory.ConstructorDeclaration(classDecl.Name)
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)))
            .WithBody(SyntaxFactory.Block(classDecl.Members.OfType<PropertySyntax>()
                .Select(i => SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.ThisExpression(), SyntaxFactory.IdentifierName(i.Name)),
                        SyntaxFactory.IdentifierName(i.Name))))));

        var members = classDecl.Members
            .Select(i => Visit(context, i))
            .Cast<CSMemberDeclarationSyntax>()
            .ToList();

        members.Insert(0, ctor);

        var clazz = SyntaxFactory.ClassDeclaration(classDecl.Name);

        return members.Count == 0
            ? clazz.WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
                .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken))
            : clazz.WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
                .WithMembers(SyntaxFactory.List(members))
                .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
    }

    private CSharpSyntaxNode VisitArray(CompilationContext context, ArraySyntax array)
    {
        var values = array.ItemsList.Items.Select(i => Visit(context, i)).Cast<CSExpressionSyntax>().ToList();

        return SyntaxFactory.ImplicitArrayCreationExpression(
            SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                SyntaxFactory.SeparatedList(values))
        );
    }

    private CSharpSyntaxNode VisitUnary(CompilationContext context, UnaryExpressionSyntax unary)
    {
        if (Visit(context, unary.Expression) is not CSExpressionSyntax expression)
        {
            throw new CompilerError("Unary expression did not evaluate to an expression");
        }

        if (unary is TypeCheckSyntax typeCheck)
        {
            if (typeCheck.Type.Type == null)
            {
                throw new CompilerError("Type check syntax missing resolved type");
            }

            return SyntaxFactory.BinaryExpression(SyntaxKind.IsExpression, expression,
                SyntaxFactory.ParseTypeName(typeCheck.Type.Type.FullName));
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

        return SyntaxFactory.ElementAccessExpression(expression,
            SyntaxFactory.BracketedArgumentList(SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Argument(arg) })));
    }

    private CSharpSyntaxNode VisitMemberAccess(CompilationContext context, MemberAccessSyntax memberAccess)
    {
        if (Visit(context, memberAccess.Target) is not CSExpressionSyntax expression)
        {
            throw new CompilerError("Member access expression did not evaluate to an expression");
        }

        return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression,
            SyntaxFactory.IdentifierName(memberAccess.Member.Name));
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
            BinaryOperator.ArithmeticLeftShift => throw new NotImplementedException(
                "Arithmetic left shift not supported"),
            BinaryOperator.ArithmeticRightShift => throw new NotImplementedException(
                "Arithmetic right shift not supported"),
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

        ElseClauseSyntax? elseClause = null;

        if (ifSyntax.ElseNode != null)
        {
            if (Visit(context, ifSyntax.ElseNode.Child) is not StatementSyntax elseStatement)
            {
                throw new CompilerError("Else clause must be a statement");
            }

            elseClause = SyntaxFactory.ElseClause(elseStatement);
        }

        return SyntaxFactory.IfStatement(condition, body).WithElse(elseClause);
    }

    private CSharpSyntaxNode VisitIdentifier(CompilationContext context, IdentifierExpressionSyntax identifier)
    {
        return SyntaxFactory.IdentifierName(identifier.Name);
    }

    private CSharpSyntaxNode VisitVariableDeclaration(CompilationContext context,
        VariableDeclarationSyntax variableDeclaration)
    {
        var type = variableDeclaration.TypeIdentifier?.Type
                   ?? variableDeclaration.InitializerExpression?.ExpressionType;

        if (type?.FullName == null)
        {
            throw new CompilerError("Type not determined for variable declaration");
        }

        var initializer = variableDeclaration.InitializerExpression != null
            ? Visit(context, variableDeclaration.InitializerExpression) as CSExpressionSyntax
            : null;

        return SyntaxFactory.LocalDeclarationStatement(
            SyntaxFactory.VariableDeclaration(
                variableDeclaration.TypeIdentifier != null
                    ? SyntaxFactory.ParseTypeName(type.FullName)
                    : SyntaxFactory.ParseTypeName("var"),
                SyntaxFactory.SeparatedList(new[]
                {
                    SyntaxFactory.VariableDeclarator(variableDeclaration.Name)
                        .WithInitializer(initializer != null ? SyntaxFactory.EqualsValueClause(initializer) : null)
                })));
    }

    private CSharpSyntaxNode VisitLiteral(CompilationContext context, LiteralExpressionSyntax literal)
    {
        var kind = literal.Value switch
        {
            byte or sbyte or short or ushort or int or uint or long or ulong or nint or nuint or float or double
                or decimal => SyntaxKind.NumericLiteralExpression,
            char => SyntaxKind.CharacterLiteralExpression,
            string => SyntaxKind.StringLiteralExpression,
            true => SyntaxKind.TrueLiteralExpression,
            false => SyntaxKind.FalseLiteralExpression,
            _ => throw new NotImplementedException(
                $"Literal expressions of type {literal.Value.GetType()} not yet implemented"),
        };

        var text = literal.Value switch
        {
            true => "true",
            false => "false",
            long l => $"{l}L",
            float f => $"{f}f",
            double d => $"{d}d",
            decimal m => $"{m}m",
            string s => SymbolDisplay.FormatLiteral(s, true),
            char c => SymbolDisplay.FormatLiteral(c, true),
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

        if (call.MatchedMethod != null)
        {
            CSExpressionSyntax target;

            if (call.CompileTimeTarget is SyntaxNode targetNode)
            {
                if (Visit(context, targetNode) is not CSExpressionSyntax targetExpr)
                {
                    throw new CompilerError("Call target did not resolve to an expression");
                }

                target = call.MatchedMethod switch
                {
                    DeclaredFunction { DeclaringType: { } declaringType } declared =>
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            targetExpr,
                            SyntaxFactory.IdentifierName(declared.FunctionSyntax.Name)),
                    RuntimeMethodInfoFunction { Method: { DeclaringType: { } type } method } =>
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            targetExpr,
                            SyntaxFactory.IdentifierName(method.Name)
                        ),
                    _ => throw new NotImplementedException("Unexpected function type")
                };
            }
            else
            {
                target = call.MatchedMethod switch
                {
                    DeclaredFunction { DeclaringType: null } declared =>
                        SyntaxFactory.IdentifierName(declared.FunctionSyntax.Name),
                    DeclaredFunction { DeclaringType: { } declaringType } declared =>
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(declaringType.Name),
                            SyntaxFactory.IdentifierName(declared.FunctionSyntax.Name)),
                    RuntimeMethodInfoFunction { Method: { DeclaringType: { } type } method } =>
                        SyntaxFactory.ParseName($"{type.FullName}.{method.Name}"),
                    _ => throw new NotImplementedException("Unexpected function type")
                };
            }

            return SyntaxFactory.InvocationExpression(target,
                SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(args)));
        }

        if (call.MatchedConstructor != null)
        {
            return SyntaxFactory
                .ObjectCreationExpression(SyntaxFactory.IdentifierName(call.MatchedConstructor.DeclaringType.Name))
                .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(args)));
        }

        throw new CompilerError("No matched method to call");
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
                if (parameter is not ThisParameterSyntax)
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
        }

        string name = function.Name;
        bool isMain = false;

        if (function.Name == "main")
        {
            name = "Main";
            isMain = true;

            if (parameters.Count == 0)
            {
                parameters.Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier("args"))
                    .WithType(SyntaxFactory.ParseTypeName("string[]")));
            }
        }

        var modifiers = new SyntaxTokenList();

        if (function.VisibilityModifier.HasFlag(VisibilityModifier.Public)
            || (isMain && function.VisibilityModifier == VisibilityModifier.None))
        {
            modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
        }
        else if (function.VisibilityModifier.HasFlag(VisibilityModifier.Private))
        {
            modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
        }

        if (!function.HasThisParameter)
        {
            modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
        }

        var baseDeclaration = SyntaxFactory.MethodDeclaration(returnType, name)
            .WithModifiers(modifiers)
            .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)));
        
        if (function.Body is ExpressionBlockSyntax expressionBodySyntax)
        {
            if (expressionBodySyntax.Children.Count != 1 
                || expressionBodySyntax.Children[0] is not ExpressionSyntax expr
                || Visit(context, expr) is not CSExpressionSyntax expressionBody)
            {
                throw new CompilerError("Function body did not evaluate to an expression");
            }

            if (function.ReturnTypeIdentifier == null
                && expr.ExpressionType != null)
            {
                baseDeclaration = baseDeclaration.WithReturnType(SyntaxFactory.ParseTypeName(expr.ExpressionType.FullName));
            }

            return baseDeclaration
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(expressionBody))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        if (Visit(context, function.Body) is not CSBlockSyntax body)
        {
            throw new CompilerError("Function body did not evaluate to a block");
        }

        return baseDeclaration.WithBody(body);
    }

    private CSharpSyntaxNode VisitCompilationUnit(CompilationContext context, CompilationUnitSyntax compilationUnit)
    {
        var members = new List<CSMemberDeclarationSyntax>();
        bool addProgramClass = false;

        if (context.RoslynProgramClass == null)
        {
            context.RoslynProgramClass = SyntaxFactory.ClassDeclaration("Program")
                .WithModifiers(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword)));

            addProgramClass = true;
        }

        foreach (var child in compilationUnit.Children)
        {
            var result = Visit(context, child);

            if (result is CSClassDeclarationSyntax classDecl)
            {
                members.Add(classDecl);
            }
            else if (result is RecordDeclarationSyntax record)
            {
                members.Add(record);
            }
            else if (result is MethodDeclarationSyntax method)
            {
                // top-level function goes in program class
                context.RoslynProgramClass = context.RoslynProgramClass.AddMembers(method);
            }
            else if (result != null)
            {
                throw new NotImplementedException(
                    $"Unexpected transformed child of compilation unit: {result.GetType()}");
            }
        }

        if (addProgramClass)
        {
            members.Add(context.RoslynProgramClass);
        }

        return SyntaxFactory.CompilationUnit()
            .WithUsings(SyntaxFactory.List(new[]
            {
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")),
                //SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Jaktnat.Runtime")),
            }))
            .WithMembers(SyntaxFactory.List(new CSMemberDeclarationSyntax[]
            {
                SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(context.AssemblyName))
                    .WithMembers(SyntaxFactory.List(members))
            }));
    }
}