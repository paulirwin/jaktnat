namespace Jaktnat.Compiler.Syntax;

public class MatchCaseBlockBodySyntax : MatchCaseBodySyntax
{
    public MatchCaseBlockBodySyntax(BlockSyntax block)
    {
        Block = block;
    }
    
    public BlockSyntax Block { get; }
    
    public override string ToString() => Block.ToString();
    
    public override bool Mutates => true; // TODO?

    public override bool PreventsMutation => false; // TODO?
}