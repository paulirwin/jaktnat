using System.Reflection;
using Antlr4.Runtime;
using Jaktnat.Compiler.ILGenerators;
using Jaktnat.Compiler.Syntax;
using Jaktnat.Runtime;
using Mono.Cecil;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace Jaktnat.Compiler;

public class JaktnatCompiler
{
    public Assembly CompileText(string contents)
    {
        var compilationUnit = ParseProgram(contents);

        return CompileAssembly(compilationUnit);
    }

    private static Assembly CompileAssembly(CompilationUnitSyntax compilationUnit)
    {
        // FIXME: allow specifying assembly name
        var assembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("Jaktnat.Output", new Version(1, 0)), "Jaktnat.Output", ModuleKind.Console);
        var module = assembly.MainModule;

        var objectType = module.ImportReference(typeof(object));

        var programClass = new TypeDefinition("Jaktnat.Output", "Program", 
            TypeAttributes.NotPublic | TypeAttributes.Class,
            objectType);
        assembly.MainModule.Types.Add(programClass);

        var context = new CompilationContext(assembly);
        var runtimeAssembly = typeof(FreeFunctionAttribute).Assembly;
        context.LoadedAssemblies.Add(runtimeAssembly);

        foreach (var child in compilationUnit.Children)
        {
            if (child is FunctionSyntax functionSyntax)
            {
                TopLevelFunctionILGenerator.GenerateTopLevelFunction(context, functionSyntax, programClass);
            } 
        }

        var outputDir = Path.Join(Environment.CurrentDirectory, "bin");
        Directory.CreateDirectory(outputDir);

        var outputFile = Path.Join(outputDir, "Jaktnat.Output.exe");
        assembly.Write(outputFile);

        var runtimeConfigJson = @"{
  ""runtimeOptions"": {
    ""tfm"": ""net6.0"",
    ""framework"": {
      ""name"": ""Microsoft.NETCore.App"",
      ""version"": ""6.0.0""
    }
  }
}";
        File.WriteAllText(Path.Join(outputDir, "Jaktnat.Output.runtimeconfig.json"), runtimeConfigJson);

        File.Copy(runtimeAssembly.Location, Path.Join(outputDir, Path.GetFileName(runtimeAssembly.Location)));

        var assy = Assembly.LoadFile(outputFile);
        return assy;
    }

    private static CompilationUnitSyntax ParseProgram(string contents)
    {
        var lexer = new JaktnatLexer(new AntlrInputStream(contents));
        var parser = new JaktnatParser(new CommonTokenStream(lexer));
        var visitor = new JaktnatVisitor();

        var parserOutput = visitor.Visit(parser.file());

        if (parserOutput is not CompilationUnitSyntax compilationUnit)
        {
            throw new InvalidOperationException($"Unexpected output from parser visitor, expected {typeof(CompilationUnitSyntax)} but got {parserOutput?.GetType().ToString() ?? "null"}");
        }

        return compilationUnit;
    }
}