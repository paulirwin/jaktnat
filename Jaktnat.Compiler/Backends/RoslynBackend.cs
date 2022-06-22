using System.Reflection;
using Jaktnat.Compiler.Backends.Roslyn;
using Jaktnat.Compiler.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace Jaktnat.Compiler.Backends;

internal class RoslynBackend : ICompilerBackend
{
    public Assembly CompileAssembly(CompilationContext context, string assemblyName, CompilationUnitSyntax compilationUnit)
    {
        var syntaxTrees = TransformSyntax(context, compilationUnit);
        
        var compilation = CSharpCompilation.Create(assemblyName, syntaxTrees, GetMetadataReference(context), GetCompilationOptions());

        // HACK.PI: code largely copied from AssemblyGenerator below
        var outputDir = Path.Join(Environment.CurrentDirectory, "bin");
        Directory.CreateDirectory(outputDir);

        var outputFile = Path.Join(outputDir, $"{assemblyName}.exe");

        using (var ms = new MemoryStream())
        {
            var emitResult = compilation.Emit(ms);

            if (!emitResult.Success)
            {
                throw new CompilerError($"Failed to emit Roslyn assembly: {string.Join(", ", emitResult.Diagnostics)}");
            }

            ms.Position = 0;

            using (var writer = File.OpenWrite(outputFile))
            {
                ms.CopyTo(writer);
                writer.Flush(true);
            }
        }

        var runtimeConfigJson = @"{
  ""runtimeOptions"": {
    ""tfm"": ""net6.0"",
    ""framework"": {
      ""name"": ""Microsoft.NETCore.App"",
      ""version"": ""6.0.0""
    }
  }
}";
        File.WriteAllText(Path.Join(outputDir, $"{assemblyName}.runtimeconfig.json"), runtimeConfigJson);

        File.Copy(context.RuntimeAssembly.Location, Path.Join(outputDir, Path.GetFileName(context.RuntimeAssembly.Location)), true);

        var assy = Assembly.LoadFile(outputFile);
        return assy;
    }

    public IEnumerable<MetadataReference> GetMetadataReference(CompilationContext context)
    {
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
        
        yield return MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll"));
        yield return MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Console.dll"));
        yield return MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll"));

        foreach (var assembly in context.LoadedAssemblies)
        {
            yield return MetadataReference.CreateFromFile(assembly.Location);
        }
    }

    private static IEnumerable<SyntaxTree> TransformSyntax(CompilationContext context, Syntax.SyntaxNode compilationUnit)
    {
        ISyntaxTransformer<CSharpSyntaxNode?> transformerVisitor = new RoslynTransformerVisitor();

        var cuNode = transformerVisitor.Visit(context, compilationUnit);

        if (cuNode == null)
        {
            throw new CompilerError("Roslyn transformer returned null");
        }

        var workspace = new AdhocWorkspace();

        cuNode = Formatter.Format(cuNode, workspace) as CSharpSyntaxNode ?? throw new CompilerError("Formatter returned a non-C# syntax node");

        return new List<SyntaxTree>
        {
            CSharpSyntaxTree.Create(cuNode)
        };
    }

    private static CSharpCompilationOptions GetCompilationOptions()
    {
        return new CSharpCompilationOptions(OutputKind.ConsoleApplication)
            .WithOverflowChecks(true)
            .WithOptimizationLevel(OptimizationLevel.Debug);
    }
}