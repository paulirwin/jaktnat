using Antlr4.Runtime.Misc;

namespace Jaktnat.Compiler;

public class ParserError : Exception
{
    public ParserError(string message, Interval sourceInterval)
        : base(message)
    {
        SourceInterval = sourceInterval;
    }

    public Interval SourceInterval { get; }
}