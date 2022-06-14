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
        
        if (VisitBlock(context.block()) is not BlockSyntax body)
        {
            throw new ParserError("Incomplete function", context.Start);
        }

        var parameters = context.parameterList();
        ParameterListSyntax? parameterList = null;

        if (parameters != null)
        {
            parameterList = Visit(parameters) as ParameterListSyntax;

            if (parameterList == null)
            {
                throw new ParserError($"Unable to parse parameters for function {name}", context.start);
            }
        }

        return new FunctionSyntax(name, parameterList, body);
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
        var anonymous = context.ANONYMOUS() != null;

        if (context.namedParameter() is { } namedParameter)
        {
            var name = namedParameter.NAME().GetText();
            var mutable = namedParameter.MUTABLE() != null;

            if (Visit(namedParameter.type()) is not TypeIdentifierSyntax type)
            {
                throw new ParserError("Unable to parse parameter type", namedParameter.start);
            }

            return new ParameterSyntax(anonymous, name, mutable, type);
        }
        else
        {
            throw new NotImplementedException("Need to support `this` parameter");
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
            if (child.GetText() is "{" or "}")
                continue;

            var childNode = Visit(child);

            if (childNode != null)
            {
                block.Children.Add(childNode);
            }
        }

        return block;
    }

    public override SyntaxNode? VisitCallArgument(JaktnatParser.CallArgumentContext context)
    {
        var name = (string?)null; // TODO: support parameter names

        if (VisitExpression(context.expression()) is not ExpressionSyntax expr)
        {
            throw new ParserError("Unable to parse call argument expression", context.Start);
        }

        return new CallArgumentSyntax(name, expr);
    }

    public override SyntaxNode? VisitLiteral(JaktnatParser.LiteralContext context)
    {
        var str = context.STRING();

        if (str != null)
        {
            return new LiteralExpressionSyntax(str.GetText()[1..^1]);
        }

        var number = context.number();

        if (number != null)
        {
            return VisitNumber(number);
        }

        var t = context.TRUE();

        if (t != null)
        {
            return new LiteralExpressionSyntax(true);
        }

        var f = context.FALSE();

        if (f != null)
        {
            return new LiteralExpressionSyntax(false);
        }

        return base.VisitLiteral(context);
    }

    public override SyntaxNode? VisitNumber(JaktnatParser.NumberContext context)
    {
        var suffix = context.numberSuffix();

        if (suffix != null)
        {
            var suffixText = suffix.GetText();

            throw new NotImplementedException($"Need to implement number suffix for {suffixText}");
        }

        var floating = context.FLOATING();

        if (floating != null)
        {
            var value = double.Parse(context.GetText().Replace("_", ""));

            return new LiteralExpressionSyntax(value);
        }

        var integer = context.INTEGER();

        if (integer != null)
        {
            var value = long.Parse(context.GetText().Replace("_", ""));

            // TODO: implement logic to scale down to i32/etc?
            return new LiteralExpressionSyntax(value);
        }

        return null;
    }

    public override SyntaxNode? VisitIfStatement(JaktnatParser.IfStatementContext context)
    {
        if (VisitExpression(context.expression()) is not ExpressionSyntax cond)
        {
            throw new ParserError("Unable to parse if condition", context.Start);
        }

        if (VisitBlock(context.block()) is not BlockSyntax block)
        {
            throw new ParserError("Unable to parse if block", context.Start);
        }

        return new IfSyntax(cond, block);
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

    public override SyntaxNode? VisitVariableDeclaration(JaktnatParser.VariableDeclarationContext context)
    {
        var mutable = context.MUTABLE() != null;

        var name = context.NAME().GetText();

        var typeDecl = context.variableDeclarationType();
        string? typeName = null;

        if (typeDecl != null)
        {
            var type = typeDecl.type();

            typeName = type.typeName().NAME().GetText();
        }

        return new VariableDeclarationSyntax(name, typeName, mutable);
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

        if (context.prefixUnaryOperator() is { } prefixUnaryOperator)
        {
            return VisitPrefixUnaryExpression(context, prefixUnaryOperator);
        }

        if (context.postfixUnaryOperator() is { } postfixUnaryOperator)
        {
            return VisitPostfixUnaryExpression(context, postfixUnaryOperator);
        }

        if (context.binaryOperator() is { } binaryOperator)
        {
            return VisitBinaryExpression(context, binaryOperator);
        }

        if (context.call() is { } call)
        {
            return VisitCallExpression(context, call);
        }

        if (context.indexerAccess() is { } indexer)
        {
            return VisitIndexerAccessExpression(context, indexer);
        }

        return base.VisitExpression(context);
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

    private SyntaxNode VisitPrefixUnaryExpression(JaktnatParser.ExpressionContext context, JaktnatParser.PrefixUnaryOperatorContext unaryOperator)
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
            "++" => UnaryOperator.PreIncrement,
            "--" => UnaryOperator.PreDecrement,
            "-" => UnaryOperator.Negate,
            "*" => UnaryOperator.Dereference,
            "&" => UnaryOperator.RawAddress,
            "not" => UnaryOperator.LogicalNot,
            "~" => UnaryOperator.BitwiseNot,
            _ => throw new ParserError($"Unknown prefix unary operator: {unaryOperator.GetText()}", unaryOperator.start),
        };

        return new UnaryExpressionSyntax(expr, op);
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

        return new IndexerAccessExpression(target, arg);
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

    private SyntaxNode VisitBinaryExpression(JaktnatParser.ExpressionContext context, JaktnatParser.BinaryOperatorContext binaryOperator)
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

        var op = binaryOperator.GetText() switch
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
            _ => throw new ParserError($"Unknown binary operator: {binaryOperator.GetText()}", binaryOperator.start),
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
}