namespace Jaktnat.Compiler.Syntax;

public class MatchCaseSyntax : SyntaxNode
{
    public MatchCaseSyntax(IReadOnlyList<MatchCasePatternSyntax> patterns, MatchCaseBodySyntax body)
    {
        Patterns = patterns;
        Body = body;
    }

    public IReadOnlyList<MatchCasePatternSyntax> Patterns { get; }
    
    public MatchCaseBodySyntax Body { get; }

    public bool Mutates => Patterns.Any(i => i.Mutates) || Body.Mutates;
    
    public bool PreventsMutation => Patterns.Any(i => i.PreventsMutation) || Body.PreventsMutation;

    public override string ToString() => $"{string.Join(" | ", Patterns)} => {Body}";
}