﻿namespace Jaktnat.Compiler.Syntax;

public class IndexerAccessSyntax : ExpressionSyntax
{
    public IndexerAccessSyntax(ExpressionSyntax target, ExpressionSyntax argument)
    {
        Target = target;
        Argument = argument;
    }

    public ExpressionSyntax Target { get; }

    // TODO: support multi-dimensional indexers
    public ExpressionSyntax Argument { get; }

    public override string ToString() => $"{Target}[{Argument}]";
}