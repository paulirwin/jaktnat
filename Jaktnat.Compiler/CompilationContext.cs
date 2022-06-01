using System.Reflection;

namespace Jaktnat.Compiler;

internal class CompilationContext
{
    public CompilationContext(Assembly runtimeAssembly)
    {
        RuntimeAssembly = runtimeAssembly;
        LoadedAssemblies.Add(runtimeAssembly);
    }

    public Assembly RuntimeAssembly { get; }

    public IList<Assembly> LoadedAssemblies { get; } = new List<Assembly>();
}