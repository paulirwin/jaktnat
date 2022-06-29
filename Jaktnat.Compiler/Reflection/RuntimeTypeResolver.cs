namespace Jaktnat.Compiler.Reflection;

internal static class RuntimeTypeResolver
{
    public static Type? TryResolveType(CompilationContext context, string name)
    {
        // TODO: restrict to imported namespaces

        foreach (var assembly in context.LoadedAssemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.Name.Equals(name))
                {
                    return type;
                }
            }    
        }

        return null;
    }
}