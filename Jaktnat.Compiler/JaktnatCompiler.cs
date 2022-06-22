using System.Reflection;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Jaktnat.Compiler.Backends;
using Jaktnat.Compiler.Reflection;
using Jaktnat.Compiler.Syntax;
using Jaktnat.Runtime;

namespace Jaktnat.Compiler;

public static class JaktnatCompiler
{
    public static Assembly CompileText(JaktnatCompilerOptions options, string assemblyName, string contents)
    {
        var runtimeAssembly = typeof(FreeFunctionAttribute).Assembly;
        
        // Phase 1: lexing and parsing
        var compilationUnit = ParseProgram(contents);
        var context = new CompilationContext(compilationUnit, runtimeAssembly, assemblyName);
        FreeFunctionResolver.PopulateGlobals(context);

        // Phase 2: scope resolution
        ScopeResolutionEngine.ResolveScopes(compilationUnit, null);

        // Phase 3: name, type, and overload resolution
        SyntaxVisitor.Visit<NameResolutionEngine>(context, compilationUnit);

        // Phase 4: assembly generation
        var backend = GetCompilerBackend(options.Backend);

        return backend.CompileAssembly(context, assemblyName, compilationUnit);
    }

    private static ICompilerBackend GetCompilerBackend(BackendType backend)
    {
        return backend switch
        {
            BackendType.ILGenerator => new ILGeneratorBackend(),
            BackendType.Roslyn => new RoslynBackend(),
            _ => throw new ArgumentOutOfRangeException(nameof(backend), backend, $"Backend type {backend} not supported"),
        };
    }

    private static CompilationUnitSyntax ParseProgram(string contents)
    {
        var lexer = new JaktnatLexer(new AntlrInputStream(contents));
        var parser = new JaktnatParser(new CommonTokenStream(lexer));
        var visitor = new JaktnatVisitor();

        parser.ErrorHandler = new BailErrorStrategy();


        try
        {
            var parserOutput = visitor.Visit(parser.file());

            if (parserOutput is not CompilationUnitSyntax compilationUnit)
            {
                throw new InvalidOperationException($"Unexpected output from parser visitor, expected {typeof(CompilationUnitSyntax)} but got {parserOutput?.GetType().ToString() ?? "null"}");
            }

            return compilationUnit;
        }
        catch (ParseCanceledException ex) when (ex.InnerException is NoViableAltException noViableAltEx)
        {
            throw new ParserError($"Parse error; no viable alternative rule at '{noViableAltEx.OffendingToken.Text}'", noViableAltEx.OffendingToken, ex);
        }
        catch (ParseCanceledException ex) when (ex.InnerException is InputMismatchException inputMismatchEx)
        {
            throw new ParserError($"Parse error; input mismatch at '{inputMismatchEx.OffendingToken.Text}'", inputMismatchEx.OffendingToken, ex);
        }
    }
}