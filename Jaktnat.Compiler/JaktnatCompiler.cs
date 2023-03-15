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
    public static Assembly CompileText(BuildOptions options, string assemblyName, string contents)
    {
        var compilationUnit = PrepareCompilation(options, assemblyName, contents, out var context, out var backend);

        return backend.CompileAssembly(context, assemblyName, compilationUnit);
    }

    public static string TranspileText(BuildOptions options, string assemblyName, string contents)
    {
        var compilationUnit = PrepareCompilation(options, assemblyName, contents, out var context, out var backend);

        if (!backend.CanTranspile)
        {
            throw new InvalidOperationException($"The {backend.GetType()} backend does not support transpiling");
        }

        return backend.Transpile(context, compilationUnit);
    }

    private static CompilationUnitSyntax PrepareCompilation(BuildOptions options, 
        string assemblyName, string contents,
        out CompilationContext context, out ICompilerBackend backend)
    {
        var runtimeAssembly = typeof(FreeFunctionAttribute).Assembly;

        // Phase 1: lexing and parsing
        var compilationUnit = ParseProgram(contents);
        
        context = new CompilationContext(compilationUnit, runtimeAssembly, assemblyName);
        
        FreeFunctionResolver.PopulateGlobals(context);

        // Phase 2: scope resolution
        ScopeResolutionEngine.ResolveScopes(compilationUnit, null);

        // Phase 3: name, type, and overload resolution
        SyntaxVisitor.Visit<NameResolutionEngine>(context, compilationUnit);

        // Phase 4: immutability/mutability validation
        SyntaxVisitor.Visit<ImmutabilityValidator>(context, compilationUnit);

        // Phase 5: assembly generation
        backend = GetCompilerBackend(options.Backend);
        
        return compilationUnit;
    }

    private static ICompilerBackend GetCompilerBackend(BackendType backend)
    {
        return backend switch
        {
            BackendType.MonoCecil => new MonoCecilBackend(),
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