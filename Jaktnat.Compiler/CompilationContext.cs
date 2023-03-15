using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mono.Cecil;
using CompilationUnitSyntax = Jaktnat.Compiler.Syntax.CompilationUnitSyntax;

namespace Jaktnat.Compiler;

internal class CompilationContext
{
    public CompilationContext(CompilationUnitSyntax compilationUnit, Assembly runtimeAssembly, string assemblyName)
    {
        CompilationUnit = compilationUnit;
        RuntimeAssembly = runtimeAssembly;
        AssemblyName = assemblyName;
        LoadedAssemblies.Add(runtimeAssembly);

        var coreLib = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(i => i.GetName().Name == "System.Private.CoreLib");

        if (coreLib != null)
        {
            LoadedAssemblies.Add(coreLib);
        }
    }

    public string AssemblyName { get; }

    public CompilationUnitSyntax CompilationUnit { get; }

    public Assembly RuntimeAssembly { get; }

    public IList<Assembly> LoadedAssemblies { get; } = new List<Assembly>();


    // TODO.PI: move these properties out of this class

    public TypeDefinition? ProgramClass { get; set; }

    public ClassDeclarationSyntax? RoslynProgramClass { get; set; }
}