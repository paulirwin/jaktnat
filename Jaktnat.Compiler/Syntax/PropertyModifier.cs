namespace Jaktnat.Compiler.Syntax;

[Flags]
public enum PropertyModifier
{
    None = 0,
    Public = 1 << 0,
    Private = 1 << 1,
}