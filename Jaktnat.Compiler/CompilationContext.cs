using System.Reflection;
using Mono.Cecil;

namespace Jaktnat.Compiler;

internal class CompilationContext
{
    public CompilationContext(AssemblyDefinition currentAssembly)
    {
        CurrentAssembly = currentAssembly;
    }

    public AssemblyDefinition CurrentAssembly { get; }

    public IList<Assembly> LoadedAssemblies { get; set; } = new List<Assembly>();
}