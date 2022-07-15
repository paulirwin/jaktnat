namespace Jaktnat.Compiler.Syntax;

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
    
    // NOTE: this is currently imperfect. Technically an indexer could mutate,
    // especially a .NET one. A future enhancement would be to investigate the
    // indexer to determine if it is truly mutating or not.
    public override bool Mutates => false;

    public override bool PreventsMutation => Target.PreventsMutation;
}