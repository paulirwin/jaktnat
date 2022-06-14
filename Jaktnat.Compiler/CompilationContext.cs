using System.Reflection;
using Jaktnat.Compiler.Syntax;

namespace Jaktnat.Compiler;

internal class CompilationContext
{
    public CompilationContext(CompilationUnitSyntax compilationUnit, Assembly runtimeAssembly)
    {
        CompilationUnit = compilationUnit;
        RuntimeAssembly = runtimeAssembly;
        LoadedAssemblies.Add(runtimeAssembly);
    }

    public CompilationUnitSyntax CompilationUnit { get; }

    public Assembly RuntimeAssembly { get; }

    public IList<Assembly> LoadedAssemblies { get; } = new List<Assembly>();
}