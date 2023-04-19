using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler;

internal class JaktnatVisitor : JaktnatBaseVisitor<SyntaxNode?>
{
    public override SyntaxNode? VisitFile(JaktnatParser.FileContext context)
    {
        var compilationUnit = new CompilationUnitSyntax();

        foreach (var child in context.children)
        {
            var childNode = Visit(child);

            if (childNode != null)
            {
                compilationUnit.Children.Add(childNode);
            }
        }

        return compilationUnit;
    }

    public override SyntaxNode? VisitFunction(JaktnatParser.FunctionContext context)
    {
        var name = context.NAME().GetText();

        SyntaxNode body;

        if (context.block() != null)
        {
            if (Visit(context.block()) is not BlockSyntax bodyBlock)
            {
                throw new ParserError("Incomplete function", context.Start);
            }

            body = bodyBlock;
        }
        else if (context.expression() != null)
        {
            if (Visit(context.expression()) is not ExpressionSyntax bodyExpression)
            {
                throw new ParserError("Incomplete function", context.Start);
            }

            body = bodyExpression;
        }
        else
        {
            throw new ParserError("Incomplete function", context.Start);
        }

        ParameterListSyntax? parameterList;

        if (context.parameterList() is { } parameters)
        {
            parameterList = Visit(parameters) as ParameterListSyntax;

            if (parameterList == null)
            {
                throw new ParserError($"Unable to parse parameters for function {name}", context.start);
            }
        }
        else
        {
            parameterList = new ParameterListSyntax();
        }

        bool throws = context.THROWS() != null;
        TypeIdentifierSyntax? returnType = null;

        if (context.functionReturnType() is { } functionReturnType)
        {
            returnType = Visit(functionReturnType.type()) as TypeIdentifierSyntax;

            if (returnType == null)
            {
                throw new ParserError($"Unable to parse return type for function {name}", functionReturnType.start);
            }
        }

        var modifier = VisibilityModifier.None;

        if (context.visibilityModifier() is { } visibilityModifier)
        {
            if (visibilityModifier.PUBLIC() != null)
            {
                modifier = VisibilityModifier.Public;
            }
            else if (visibilityModifier.PRIVATE() != null)
            {
                modifier = VisibilityModifier.Private;
            }
        }

        return new FunctionSyntax(modifier, name, parameterList, body, throws, returnType);
    }

    public override SyntaxNode VisitClassDeclaration(JaktnatParser.ClassDeclarationContext context)
    {
        var name = context.NAME().GetText();

        var classSyntax = new ClassDeclarationSyntax(name);

        foreach (var classMember in context.classMember())
        {
            var member = Visit(classMember) switch
            {
                MemberDeclarationSyntax m => m,
                FunctionSyntax f => new MemberFunctionDeclarationSyntax(f.Name, f),
                _ => throw new ParserError("Unexpected result from visiting class member", classMember.start)
            };
            
            classSyntax.Members.Add(member);
        }

        return classSyntax;
    }

    public override SyntaxNode? VisitProperty(JaktnatParser.PropertyContext context)
    {
        var name = context.NAME().GetText();

        TypeIdentifierSyntax? type;

        if (context.variableDeclarationType() is { } typeDecl)
        {
            if (Visit(typeDecl.type()) is not TypeIdentifierSyntax typeValue)
            {
                throw new ParserError("Unable to parse variable type", typeDecl.start);
            }

            type = typeValue;
        }
        else
        {
            throw new ParserError("Properties must have a declared type", context.start);
        }

        var modifiers = VisibilityModifier.None;

        if (context.visibilityModifier() is { } propertyModifier)
        {
            if (propertyModifier.PUBLIC() != null)
            {
                modifiers |= VisibilityModifier.Public;
            }
            else if (propertyModifier.PRIVATE() != null)
            {
                modifiers |= VisibilityModifier.Private;
            }
        }

        return new PropertySyntax(name, type, modifiers);
    }

    public override SyntaxNode? VisitParameterList(JaktnatParser.ParameterListContext context)
    {
        var list = new ParameterListSyntax();

        foreach (var child in context.children)
        {
            if (child.GetText() == ",")
            {
                continue;
            }

            if (Visit(child) is not ParameterSyntax parameter)
            {
                throw new ParserError("Unable to parse parameter list", context.start);
            }

            list.Parameters.Add(parameter);
        }

        return list;
    }

    public override SyntaxNode? VisitParameter(JaktnatParser.ParameterContext context)
    {
        if (context.namedParameter() is { } namedParameter)
        {
            var anonymous = namedParameter.ANONYMOUS() != null || namedParameter.ANON() != null;

            var name = namedParameter.NAME().GetText();
            var mutable = namedParameter.MUTABLE() != null;

            if (Visit(namedParameter.type()) is not TypeIdentifierSyntax type)
            {
                throw new ParserError("Unable to parse parameter type", namedParameter.start);
            }

            ExpressionSyntax? defaultArgumentExpression = null;

            if (namedParameter.defaultArgument() is { } defaultArgument)
            {
                if (VisitExpression(defaultArgument.expression()) is not ExpressionSyntax defaultExpression)
                {
                    throw new ParserError("Unable to parse default argument expression", defaultArgument.start);
                }

                defaultArgumentExpression = defaultExpression;
            }

            return new ParameterSyntax(anonymous, name, mutable, type, defaultArgumentExpression);
        }
        else if (context.thisParameter() is { } thisParameter)
        {
            var mutable = thisParameter.MUT() != null || thisParameter.MUTABLE() != null;

            return new ThisParameterSyntax(mutable);
        }
        else
        {
            throw new NotImplementedException("Unknown parameter type");
        }
    }

    public override SyntaxNode? VisitTypeName(JaktnatParser.TypeNameContext context)
    {
        var name = context.NUMBER_SUFFIX()?.GetText()
                   ?? context.NAME()?.GetText();

        if (name == null)
        {
            throw new ParserError($"Unable to parse type name: {context.GetText()}", context.start);
        }

        return new NamedTypeIdentifierSyntax(name);
    }

    public override SyntaxNode? VisitArrayType(JaktnatParser.ArrayTypeContext context)
    {
        // TODO: make this recursive to support arrays of arrays
        if (VisitTypeName(context.typeName()) is not NamedTypeIdentifierSyntax namedType)
        {
            throw new ParserError("Unable to parse array type syntax", context.start);
        }

        return new ArrayTypeIdentifierSyntax(namedType);
    }

    public override SyntaxNode? VisitBlock(JaktnatParser.BlockContext context)
    {
        var block = new BlockSyntax();

        foreach (var child in context.children)
        {
            if (child.GetText() is "{" or "}" or ";")
                continue;

            var childNode = Visit(child);

            if (childNode != null)
            {
                block.Children.Add(childNode);
            }
        }

        return block;
    }

    public override SyntaxNode? VisitExpressionStatement(JaktnatParser.ExpressionStatementContext context)
    {
        return VisitExpression(context.expression());
    }

    public override SyntaxNode? VisitCallArgument(JaktnatParser.CallArgumentContext context)
    {
        string? name = null;

        if (context.argumentName() is { } argumentName)
        {
            name = argumentName.NAME().GetText();
        }

        if (VisitExpression(context.expression()) is not ExpressionSyntax expr)
        {
            throw new ParserError("Unable to parse call argument expression", context.Start);
        }

        return new CallArgumentSyntax(name, expr);
    }

    public override SyntaxNode? VisitLiteral(JaktnatParser.LiteralContext context)
    {
        if (context.STRING() is { } str)
        {
            return new LiteralExpressionSyntax(str.GetText()[1..^1]);
        }

        if (context.CHARACTER() is { } chr)
        {
            var character = chr.GetText()[2..^1].Replace("\\'", "'");
            if (character.Length is 0 or > 1)
            {
                throw new ParserError("Too few or too many characters in character literal", context.start);
            }

            return new LiteralExpressionSyntax(character[0]);
        }

        if (context.number() is { } number)
        {
            return VisitNumber(number);
        }

        if (context.TRUE() is not null)
        {
            return new LiteralExpressionSyntax(true);
        }

        if (context.FALSE() is not null)
        {
            return new LiteralExpressionSyntax(false);
        }

        return base.VisitLiteral(context);
    }

    public override SyntaxNode? VisitNumber(JaktnatParser.NumberContext context)
    {
        var suffix = context.numberSuffix();
        Type? suffixType = null;

        if (suffix != null)
        {
            var suffixText = suffix.GetText();

            suffixType = suffixText switch
            {
                "i8" => typeof(sbyte),
                "i16" => typeof(short),
                "i32" => typeof(int),
                "i64" => typeof(long),
                "u8" => typeof(byte),
                "u16" => typeof(ushort),
                "u32" => typeof(uint),
                "u64" => typeof(ulong),
                "uz" => typeof(nuint),
                _ => throw new ParserError($"Unknown number suffix: {suffixText}", suffix.start)
            };
        }
        
        var floating = context.FLOATING();

        if (floating != null)
        {
            var value = double.Parse(floating.GetText().Replace("_", ""));

            return new LiteralExpressionSyntax(value);
        }

        var integer = context.INTEGER();

        if (integer != null)
        {
            var integerText = integer.GetText().Replace("_", "");

            var value = suffixType switch
            {
                { } => Convert.ChangeType(integerText, suffixType),
                _ => int.Parse(integerText)
            };
            
            return new LiteralExpressionSyntax(value);
        }

        return null;
    }

    public override IfSyntax VisitIfStatement(JaktnatParser.IfStatementContext context)
    {
        if (VisitExpression(context.expression()) is not ExpressionSyntax cond)
        {
            throw new ParserError("Unable to parse if condition", context.Start);
        }

        if (VisitBlock(context.block()) is not BlockSyntax block)
        {
            throw new ParserError("Unable to parse if block", context.Start);
        }

        ElseSyntax? elseNode = null;

        if (context.elseStatement() is { } elseStatement)
        {
            elseNode = VisitElseStatement(elseStatement);
        }

        return new IfSyntax(cond, block, elseNode);
    }
    
    public override GuardSyntax VisitGuardStatement(JaktnatParser.GuardStatementContext context)
    {
        if (VisitExpression(context.expression()) is not ExpressionSyntax cond)
        {
            throw new ParserError("Unable to parse guard condition", context.Start);
        }

        if (context.elseStatement() is not { } elseStatement)
        {
            throw new ParserError("Expected `else` keyword", context.start);
        }
        
        var elseNode = VisitElseStatement(elseStatement);

        return new GuardSyntax(cond, elseNode);
    }

    public override ElseSyntax VisitElseStatement(JaktnatParser.ElseStatementContext context)
    {
        if (context.block() is { } elseBlock && VisitBlock(elseBlock) is BlockSyntax elseBlockSyntax)
        {
            return new ElseSyntax(elseBlockSyntax);
        }

        if (context.ifStatement() is { } elseIfStatement && VisitIfStatement(elseIfStatement) is IfSyntax elseIfSyntax)
        {
            return new ElseSyntax(elseIfSyntax);
        }

        throw new ParserError("Unable to parse else block", context.start);
    }

    public override SyntaxNode? VisitLetStatement(JaktnatParser.LetStatementContext context)
    {
        if (VisitVariableDeclaration(context.variableDeclaration()) is not VariableDeclarationSyntax variableDeclaration)
        {
            throw new ParserError("Unable to parse variable declaration", context.Start);
        }

        if (VisitExpression(context.expression()) is not ExpressionSyntax initializer)
        {
            throw new ParserError("Unable to parse variable initialization expression", context.Start);
        }

        // HACK: just re-use this node type
        variableDeclaration.InitializerExpression = initializer;

        return variableDeclaration;
    }

    public override SyntaxNode? VisitMutStatement(JaktnatParser.MutStatementContext context)
    {
        var name = context.NAME().GetText();

        TypeIdentifierSyntax? type = null;

        if (context.variableDeclarationType() is { } typeDecl)
        {
            if (Visit(typeDecl.type()) is not TypeIdentifierSyntax typeValue)
            {
                throw new ParserError("Unable to parse variable type", typeDecl.start);
            }

            type = typeValue;
        }

        if (VisitExpression(context.expression()) is not ExpressionSyntax initializer)
        {
            throw new ParserError("Unable to parse variable initialization expression", context.Start);
        }
        
        return new VariableDeclarationSyntax(name, type, true)
        {
            InitializerExpression = initializer
        };
    }

    public override SyntaxNode? VisitVariableDeclaration(JaktnatParser.VariableDeclarationContext context)
    {
        var mutable = context.MUTABLE() != null;

        var name = context.NAME().GetText();

        TypeIdentifierSyntax? type = null;

        if (context.variableDeclarationType() is { } typeDecl)
        {
            if (Visit(typeDecl.type()) is not TypeIdentifierSyntax typeValue)
            {
                throw new ParserError("Unable to parse variable type", typeDecl.start);
            }

            type = typeValue;
        }

        return new VariableDeclarationSyntax(name, type, mutable);
    }

    public override SyntaxNode? VisitIdentifier(JaktnatParser.IdentifierContext context)
    {
        var name = context.NAME().GetText();

        return new IdentifierExpressionSyntax(name);
    }

    public override SyntaxNode? VisitArray(JaktnatParser.ArrayContext context)
    {
        if (context.children.Count != 3
            || context.children[0].GetText() != "["
            || context.children[2].GetText() != "]"
            || Visit(context.children[1]) is not ExpressionListSyntax expList)
        {
            throw new ParserError("Unable to parse array syntax", context.start);
        }
        
        return new ArraySyntax(expList);
    }

    public override SyntaxNode? VisitExpressionList(JaktnatParser.ExpressionListContext context)
    {
        var expList = new ExpressionListSyntax();

        foreach (var child in context.children)
        {
            if (child.GetText() == ",")
            {
                continue;
            }

            if (Visit(child) is not ExpressionSyntax expression)
            {
                throw new ParserError("Unable to parse expression list", context.start);
            }

            expList.Items.Add(expression);
        }

        return expList;
    }

    public override SyntaxNode? VisitWhileStatement(JaktnatParser.WhileStatementContext context)
    {
        if (Visit(context.expression()) is not ExpressionSyntax condition)
        {
            throw new ParserError("Unable to parse while condition", context.start);
        }

        if (Visit(context.block()) is not BlockSyntax block)
        {
            throw new ParserError("Unable to parse while block", context.start);
        }

        return new WhileSyntax(condition, block);
    }

    public override SyntaxNode? VisitLoopStatement(JaktnatParser.LoopStatementContext context)
    {
        if (Visit(context.block()) is not BlockSyntax block)
        {
            throw new ParserError("Unable to parse loop block", context.start);
        }

        return new LoopSyntax(block);
    }

    public override SyntaxNode? VisitBreakStatement(JaktnatParser.BreakStatementContext context)
    {
        return new BreakSyntax();
    }

    public override SyntaxNode? VisitContinueStatement(JaktnatParser.ContinueStatementContext context)
    {
        return new ContinueSyntax();
    }

    public override SyntaxNode? VisitReturnStatement(JaktnatParser.ReturnStatementContext context)
    {
        ExpressionSyntax? expression = null;

        if (context.expression() is { } expressionContext)
        {
            expression = Visit(expressionContext) as ExpressionSyntax;

            if (expression == null)
            {
                throw new ParserError("Unable to parse return expression", expressionContext.start);
            }
        }

        return new ReturnSyntax(expression);
    }

    public override SyntaxNode? VisitPrimaryExpr(JaktnatParser.PrimaryExprContext context)
    {
        if (context.prefixUnaryOperator() is { } prefixUnaryOperator)
        {
            return VisitPrefixUnaryExpression(context, prefixUnaryOperator);
        }

        return base.VisitPrimaryExpr(context);
    }

    public override SyntaxNode? VisitExpression(JaktnatParser.ExpressionContext context)
    {
        if (context.typeCastOperator() is { } typeCastOperator)
        {
            return VisitTypeCastExpression(context, typeCastOperator);
        }

        if (context.IS() != null)
        {
            return VisitTypeCheckExpression(context);
        }
        
        if (context.postfixUnaryOperator() is { } postfixUnaryOperator)
        {
            return VisitPostfixUnaryExpression(context, postfixUnaryOperator);
        }

        if (context.binaryAddSubtract() != null
            || context.binaryAssign() != null
            || context.binaryBitwiseAnd() != null
            || context.binaryBitwiseOr() != null 
            || context.binaryBitwiseXor() != null
            || context.binaryBoolean() != null
            || context.binaryLogicalAnd() != null
            || context.binaryMultiplyDivideModulo() != null
            || context.binaryOrCoalescing() != null
            || context.binaryShift() != null)
        {
            return VisitBinaryExpression(context);
        }

        if (context.call() is { } call)
        {
            return VisitCallExpression(context, call);
        }

        if (context.memberAccess() is { } memberAccess)
        {
            return VisitMemberAccessExpression(context, memberAccess);
        }

        if (context.indexerAccess() is { } indexer)
        {
            return VisitIndexerAccessExpression(context, indexer);
        }

        return base.VisitExpression(context);
    }

    public override SyntaxNode VisitThisExpression(JaktnatParser.ThisExpressionContext context)
    {
        return new ThisExpressionSyntax();
    }

    private SyntaxNode VisitMemberAccessExpression(JaktnatParser.ExpressionContext context, JaktnatParser.MemberAccessContext memberAccess)
    {
        var exprs = context.expression();

        if (exprs.Length != 1)
        {
            throw new ParserError("Unexpected extra expression in member access syntax", memberAccess.start);
        }

        if (Visit(exprs[0]) is not ExpressionSyntax target)
        {
            throw new ParserError("Invalid member access syntax", context.start);
        }

        if (Visit(memberAccess.identifier()) is not IdentifierExpressionSyntax identifier)
        {
            throw new ParserError("Unable to parse member access identifier", context.start);
        }

        return new MemberAccessSyntax(target, identifier);
    }

    private SyntaxNode VisitTypeCheckExpression(JaktnatParser.ExpressionContext context)
    {
        var exprs = context.expression();

        if (exprs.Length != 1)
        {
            throw new ParserError("Type check expressions need only one expression", context.start);
        }

        if (Visit(exprs[0]) is not ExpressionSyntax expr)
        {
            throw new ParserError("Unable to parse unary expression", exprs[0].start);
        }

        if (Visit(context.type()) is not TypeIdentifierSyntax type)
        {
            throw new ParserError($"Unable to parse type identifier", context.start);
        }

        return new TypeCheckSyntax(expr, type);
    }

    private SyntaxNode VisitPrefixUnaryExpression(JaktnatParser.PrimaryExprContext context, JaktnatParser.PrefixUnaryOperatorContext unaryOperator)
    {
        var expr = context.expression();
        
        if (Visit(expr) is not ExpressionSyntax expression)
        {
            throw new ParserError("Unable to parse unary expression", expr.start);
        }
        
        var op = unaryOperator.GetText() switch
        {
            "++" => UnaryOperator.PreIncrement,
            "--" => UnaryOperator.PreDecrement,
            "-" => UnaryOperator.Negate,
            "*" => UnaryOperator.Dereference,
            "&" => UnaryOperator.RawAddress,
            "not" => UnaryOperator.LogicalNot,
            "~" => UnaryOperator.BitwiseNot,
            _ => throw new ParserError($"Unknown prefix unary operator: {unaryOperator.GetText()}", unaryOperator.start),
        };

        return new UnaryExpressionSyntax(expression, op);
    }

    private SyntaxNode VisitPostfixUnaryExpression(JaktnatParser.ExpressionContext context, JaktnatParser.PostfixUnaryOperatorContext unaryOperator)
    {
        var exprs = context.expression();

        if (exprs.Length != 1)
        {
            throw new ParserError("Unary expressions need only one expression", context.start);
        }

        if (Visit(exprs[0]) is not ExpressionSyntax expr)
        {
            throw new ParserError("Unable to parse unary expression", exprs[0].start);
        }

        var op = unaryOperator.GetText() switch
        {
            "++" => UnaryOperator.PostIncrement,
            "--" => UnaryOperator.PostDecrement,
            _ => throw new ParserError($"Unknown postfix unary operator: {unaryOperator.GetText()}", unaryOperator.start),
        };

        return new UnaryExpressionSyntax(expr, op);
    }

    private SyntaxNode? VisitIndexerAccessExpression(JaktnatParser.ExpressionContext context, JaktnatParser.IndexerAccessContext indexer)
    {
        var exprs = context.expression();

        if (exprs.Length != 1)
        {
            throw new ParserError("Unexpected extra expression in indexer syntax", indexer.start);
        }

        if (Visit(exprs[0]) is not ExpressionSyntax target)
        {
            throw new ParserError("Invalid indexer access syntax", context.start);
        }

        if (Visit(indexer.expression()) is not ExpressionSyntax arg)
        {
            throw new ParserError("Unable to parse indexer access argument", context.start);
        }

        return new IndexerAccessSyntax(target, arg);
    }

    private SyntaxNode? VisitCallExpression(JaktnatParser.ExpressionContext context, JaktnatParser.CallContext call)
    {
        var exprs = context.expression();

        if (exprs.Length != 1)
        {
            throw new ParserError("Unexpected extra expression in call syntax", call.start);
        }

        if (Visit(exprs[0]) is not ExpressionSyntax target)
        {
            throw new ParserError("Invalid call syntax", context.start);
        }

        var args = call.callArgument()
            .Select(VisitCallArgument)
            .Cast<CallArgumentSyntax?>()
            .ToList();

        if (args.Any(i => i == null))
        {
            throw new ParserError("Unable to parse call argument(s)", context.start);
        }

        return new CallSyntax(target, args!);
    }
    
    private SyntaxNode VisitBinaryExpression(JaktnatParser.ExpressionContext context)
    {
        var exprs = context.expression();

        if (exprs.Length != 2)
        {
            throw new ParserError("Binary expressions need two expressions", context.start);
        }

        if (Visit(exprs[0]) is not ExpressionSyntax left)
        {
            throw new ParserError("Unable to parse left side of binary expression", exprs[0].start);
        }

        if (Visit(exprs[1]) is not ExpressionSyntax right)
        {
            throw new ParserError("Unable to parse right side of binary expression", exprs[1].start);
        }

        var op = context.children[1].GetText() switch
        {
            "+" => BinaryOperator.Add,
            "-" => BinaryOperator.Subtract,
            "*" => BinaryOperator.Multiply,
            "/" => BinaryOperator.Divide,
            "%" => BinaryOperator.Modulo,
            "<" => BinaryOperator.LessThan,
            ">" => BinaryOperator.GreaterThan,
            "<=" => BinaryOperator.LessThanOrEqual,
            ">=" => BinaryOperator.GreaterThanOrEqual,
            "==" => BinaryOperator.Equal,
            "!=" => BinaryOperator.NotEqual,
            "and" => BinaryOperator.LogicalAnd,
            "or" => BinaryOperator.LogicalOr,
            "&" => BinaryOperator.BitwiseAnd,
            "|" => BinaryOperator.BitwiseOr,
            "^" => BinaryOperator.BitwiseXor,
            "<<" => BinaryOperator.BitwiseLeftShift,
            "<<<" => BinaryOperator.ArithmeticLeftShift,
            ">>" => BinaryOperator.BitwiseRightShift,
            ">>>" => BinaryOperator.ArithmeticRightShift,
            "=" => BinaryOperator.Assign,
            "+=" => BinaryOperator.AddAssign,
            "-=" => BinaryOperator.SubtractAssign,
            "*=" => BinaryOperator.MultiplyAssign,
            "/=" => BinaryOperator.DivideAssign,
            "%=" => BinaryOperator.ModuloAssign,
            "&=" => BinaryOperator.BitwiseAndAssign,
            "|=" => BinaryOperator.BitwiseOrAssign,
            "^=" => BinaryOperator.BitwiseXorAssign,
            "<<=" => BinaryOperator.BitwiseLeftShiftAssign,
            ">>=" => BinaryOperator.BitwiseRightShiftAssign,
            "??" => BinaryOperator.NoneCoalescing,
            "??=" => BinaryOperator.NoneCoalescingAssign,
            _ => throw new ParserError($"Unknown binary operator: {context.children[1].GetText()}", context.start),
        };

        return new BinaryExpressionSyntax(left, op, right);
    }

    private SyntaxNode VisitTypeCastExpression(JaktnatParser.ExpressionContext context, JaktnatParser.TypeCastOperatorContext typeCastOperator)
    {
        var exprs = context.expression();

        if (exprs.Length != 1)
        {
            throw new ParserError("Unexpected extra expression in type cast", context.start);
        }

        if (Visit(exprs[0]) is not ExpressionSyntax expr)
        {
            throw new ParserError("Unable to parse type cast expression", context.start);
        }

        if (Visit(context.type()) is not TypeIdentifierSyntax type)
        {
            throw new ParserError($"Unable to parse type identifier", context.start);
        }

        bool fallible = typeCastOperator.ASQUESTION() != null;

        if (!fallible && typeCastOperator.ASBANG() == null)
        {
            throw new ParserError("Unexpected type cast operator", typeCastOperator.start);
        }

        return new TypeCastSyntax(expr, fallible, type);
    }

    public override SyntaxNode? VisitParenthesizedExpression(JaktnatParser.ParenthesizedExpressionContext context)
    {
        if (Visit(context.expression()) is not ExpressionSyntax expression)
        {
            throw new ParserError("Unable to parse parenthesized expression", context.start);
        }

        return new ParenthesizedExpressionSyntax(expression);
    }

    public override SyntaxNode? VisitTryStatement(JaktnatParser.TryStatementContext context)
    {
        SyntaxNode tryable;

        if (context.expression() is { } expressionTryable)
        {
            if (Visit(expressionTryable) is not ExpressionSyntax expressionTryableExpression)
            {
                throw new ParserError("Unable to parse try expression", expressionTryable.start);
            }

            tryable = expressionTryableExpression;
        }
        else if (context.block() is { } blockTryable)
        {
            if (Visit(blockTryable) is not BlockSyntax blockTryableBlock)
            {
                throw new ParserError("Unable to parse try block", blockTryable.start);
            }

            tryable = blockTryableBlock;
        }
        else
        {
            throw new ParserError("Unexpected tryable in try syntax", context.start);
        }

        var catchClause = context.catchClause();

        if (catchClause == null)
        {
            throw new ParserError("Try statement missing catch clause", context.start);
        }

        if (Visit(catchClause.identifier()) is not IdentifierExpressionSyntax catchIdentifier)
        {
            throw new ParserError("Unable to parse catch clause identifier", catchClause.start);
        }

        if (Visit(catchClause.block()) is not BlockSyntax catchBlock)
        {
            throw new ParserError("Unable to parse catch clause block", catchClause.start);
        }

        var catchSyntax = new CatchSyntax(new BlockScopedIdentifierSyntax(catchIdentifier.Name), catchBlock);

        return new TrySyntax(tryable, catchSyntax);
    }

    public override SyntaxNode? VisitScopeAccess(JaktnatParser.ScopeAccessContext context)
    {
        var identifiers = context.identifier();

        if (identifiers.Length != 2)
        {
            throw new ParserError("Unexpected number of identifiers in scope access", context.start);
        }

        if (Visit(identifiers[0]) is not IdentifierExpressionSyntax scopeIdentifier)
        {
            throw new ParserError("Unable to parse scope identifier", identifiers[0].start);
        }

        if (Visit(identifiers[1]) is not IdentifierExpressionSyntax memberIdentifier)
        {
            throw new ParserError("Unable to parse member identifier", identifiers[1].start);
        }

        return new ScopeAccessSyntax(scopeIdentifier, memberIdentifier);
    }

    public override SyntaxNode? VisitThrowStatement(JaktnatParser.ThrowStatementContext context)
    {
        ExpressionSyntax? throwable = null;

        if (context.expression() is { } expressionContext)
        {
            throwable = Visit(expressionContext) as ExpressionSyntax;

            if (throwable == null)
            {
                throw new ParserError("Unable to parse throw statement expression", expressionContext.start);
            }
        }

        return new ThrowSyntax(throwable);
    }

    public override SyntaxNode? VisitDeferStatement(JaktnatParser.DeferStatementContext context)
    {
        SyntaxNode? body = null;

        if (context.expression() is { } expression)
        {
            body = Visit(expression);
        }
        else if (context.block() is { } block)
        {
            body = Visit(block);
        }

        if (body == null)
        {
            throw new ParserError("Unable to parse defer statement", context.start);
        }

        return new DeferSyntax(body);
    }

    public override SyntaxNode? VisitUnsafeBlock(JaktnatParser.UnsafeBlockContext context)
    {
        if (Visit(context.block()) is not BlockSyntax block)
        {
            throw new ParserError("Unable to parse unsafe block", context.start);
        }

        return new UnsafeBlockSyntax(block);
    }

    public override SyntaxNode? VisitCsharpBlock(JaktnatParser.CsharpBlockContext context)
    {
        if (Visit(context.block()) is not BlockSyntax block)
        {
            throw new ParserError("Unable to parse csharp block", context.start);
        }

        return new CSharpBlockSyntax(block);
    }

    public override SyntaxNode? VisitForInStatement(JaktnatParser.ForInStatementContext context)
    {
        string identifier = context.identifier().GetText();

        if (Visit(context.expression()) is not ExpressionSyntax expression)
        {
            throw new ParserError("Unable to parse for-in target expression", context.start);
        }

        if (Visit(context.block()) is not BlockSyntax block)
        {
            throw new ParserError("Unable to parse for-in block", context.start);
        }
        
        return new ForInSyntax(new BlockScopedIdentifierSyntax(identifier), expression, block);
    }
}