namespace Jaktnat.Compiler.Syntax;

public abstract class MatchCaseBodySyntax : SyntaxNode
{
    public abstract bool Mutates { get; }
    
    public abstract bool PreventsMutation { get; }
}