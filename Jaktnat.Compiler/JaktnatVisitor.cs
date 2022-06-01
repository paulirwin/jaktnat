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
        var expr = Visit(context.expression());

        if (expr == null)
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

        return null;
    }
}