using System.Reflection;
using Antlr4.Runtime;
using Jaktnat.Compiler.ILGenerators;
using Jaktnat.Compiler.Reflection;
using Jaktnat.Compiler.Syntax;
using Jaktnat.Runtime;

namespace Jaktnat.Compiler;

public class JaktnatCompiler
{
    public Assembly CompileText(string assemblyName, string contents)
    {
        var runtimeAssembly = typeof(FreeFunctionAttribute).Assembly;

        var context = new CompilationContext(runtimeAssembly);

        // Phase 1: lexing and parsing
        var compilationUnit = ParseProgram(contents);

        // Phase 2: scope resolution
        ScopeResolutionEngine.ResolveScopes(compilationUnit, null);

        // Phase 3: name, type, and overload resolution
        NameResolutionEngine.Resolve(context, compilationUnit);

        // Phase 4: assembly generation
        return AssemblyGenerator.CompileAssembly(context, assemblyName, compilationUnit);
    }
    
    private static CompilationUnitSyntax ParseProgram(string contents)
    {
        var lexer = new JaktnatLexer(new AntlrInputStream(contents));
        var parser = new JaktnatParser(new CommonTokenStream(lexer));
        var visitor = new JaktnatVisitor();

        parser.ErrorHandler = new BailErrorStrategy();

        var parserOutput = visitor.Visit(parser.file());

        if (parserOutput is not CompilationUnitSyntax compilationUnit)
        {
            throw new InvalidOperationException($"Unexpected output from parser visitor, expected {typeof(CompilationUnitSyntax)} but got {parserOutput?.GetType().ToString() ?? "null"}");
        }

        return compilationUnit;
    }
}