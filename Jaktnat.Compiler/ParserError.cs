using Antlr4.Runtime;

namespace Jaktnat.Compiler;

public class ParserError : Exception
{
    public ParserError(string message, IToken offendingToken)
        : this(message, offendingToken, null)
    {
    }

    public ParserError(string message, IToken offendingToken, Exception? innerException)
        : base($"[{offendingToken.Line}:{offendingToken.Column}] {message}", innerException)
    {
        OffendingToken = offendingToken;
    }

    public IToken OffendingToken { get; }
}