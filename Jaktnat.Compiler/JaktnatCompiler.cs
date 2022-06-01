using System.Reflection;
using Antlr4.Runtime;

namespace Jaktnat.Compiler;

public class JaktnatCompiler
{
    public Assembly CompileText(string contents)
    {
        var lexer = new JaktnatLexer(new AntlrInputStream(contents));
        var parser = new JaktnatParser(new CommonTokenStream(lexer));
        var visitor = new JaktnatVisitor();

        var compilationUnit = visitor.Visit(parser.file());

        throw new NotImplementedException();
    }
}