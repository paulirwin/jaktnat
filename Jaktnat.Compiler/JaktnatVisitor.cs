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
        var body = VisitBlock(context.block());

        if (body == null)
        {
            throw new ParserError("Incomplete function", context.SourceInterval);
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

    public override SyntaxNode? VisitCall(JaktnatParser.CallContext context)
    {
        var name = context.NAME().GetText();
        var args = context.callArgument()
            .Select(VisitCallArgument)
            .Where(i => i != null)
            .Cast<CallArgumentSyntax>()
            .ToList();

        return new CallSyntax(name, args);
    }

    public override SyntaxNode? VisitCallArgument(JaktnatParser.CallArgumentContext context)
    {
        var name = (string?)null; // TODO: support parameter names

        if (VisitExpression(context.expression()) is not ExpressionSyntax expr)
        {
            throw new ParserError("Unable to parse call argument expression", context.SourceInterval);
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
            throw new ParserError("Unable to parse if condition", context.SourceInterval);
        }

        if (VisitBlock(context.block()) is not BlockSyntax block)
        {
            throw new ParserError("Unable to parse if block", context.SourceInterval);
        }

        return new IfSyntax(cond, block);
    }
}