namespace Jaktnat.Compiler.Syntax;

[Flags]
public enum VisibilityModifier
{
    None = 0,
    Public = 1 << 0,
    Private = 1 << 1,
}