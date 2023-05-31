namespace Jaktnat.Compiler.Syntax;

public class MatchCasePatternElseSyntax : MatchCasePatternSyntax
{
    public override string ToString() => "else";

    public override bool Mutates => false;

    public override bool PreventsMutation => false;
}