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

        return new FunctionSyntax(name, body);
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

    public override SyntaxNode? VisitPrimaryExpr(JaktnatParser.PrimaryExprContext context)
    {
        var call = context.call();

        if (call != null)
        {
            if (Visit(context.primaryExpr()) is not ExpressionSyntax target)
            {
                throw new ParserError("Invalid call syntax", context.start);
            }

            var args = call.callArgument()
                .Select(VisitCallArgument)
                .Where(i => i != null)
                .Cast<CallArgumentSyntax>()
                .ToList();

            return new CallSyntax(target, args);
        }

        return base.VisitPrimaryExpr(context);
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

        return null;
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
}