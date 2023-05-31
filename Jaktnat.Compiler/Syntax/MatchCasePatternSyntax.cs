namespace Jaktnat.Compiler.Syntax;

public abstract class MatchCasePatternSyntax : SyntaxNode
{
    public abstract bool Mutates { get; }
    
    public abstract bool PreventsMutation { get; }
}