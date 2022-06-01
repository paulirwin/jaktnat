using System.Reflection;
using Jaktnat.Compiler.Syntax;
using Mono.Cecil;

namespace Jaktnat.Compiler.ILGenerators;

internal static class AssemblyGenerator
{
    public static Assembly CompileAssembly(CompilationContext context, string assemblyName, CompilationUnitSyntax compilationUnit)
    {
        // FIXME: allow specifying assembly name
        var assembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(assemblyName, new Version(1, 0)), assemblyName, ModuleKind.Console);
        var module = assembly.MainModule;

        var objectType = module.ImportReference(typeof(object));

        var programClass = new TypeDefinition(assemblyName, "Program",
            Mono.Cecil.TypeAttributes.NotPublic | Mono.Cecil.TypeAttributes.Class,
            objectType);
        assembly.MainModule.Types.Add(programClass);
        
        foreach (var child in compilationUnit.Children)
        {
            if (child is FunctionSyntax functionSyntax)
            {
                TopLevelFunctionILGenerator.GenerateTopLevelFunction(context, functionSyntax, programClass);
            }
        }

        var outputDir = Path.Join(Environment.CurrentDirectory, "bin");
        Directory.CreateDirectory(outputDir);

        var outputFile = Path.Join(outputDir, $"{assemblyName}.exe");
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
        File.WriteAllText(Path.Join(outputDir, $"{assemblyName}.runtimeconfig.json"), runtimeConfigJson);

        File.Copy(context.RuntimeAssembly.Location, Path.Join(outputDir, Path.GetFileName(context.RuntimeAssembly.Location)), true);

        var assy = Assembly.LoadFile(outputFile);
        return assy;
    }
}