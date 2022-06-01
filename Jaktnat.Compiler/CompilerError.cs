namespace Jaktnat.Compiler;

public class CompilerError : Exception
{
    public CompilerError(string message)
        : base(message)
    {
    }
}