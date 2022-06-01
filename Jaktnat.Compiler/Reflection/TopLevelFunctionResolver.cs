using System.Reflection;
using Jaktnat.Runtime;

namespace Jaktnat.Compiler.Reflection;

internal static class TopLevelFunctionResolver
{
    public static MethodInfo? Resolve(CompilationContext context, string name)
    {
        var matches = new List<MethodInfo>();

        // FIXME: cache this
        foreach (var assembly in context.LoadedAssemblies)
        {
            var types = assembly.GetTypes().Where(i => i.IsClass && i.IsVisible);

            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Where(i => i.GetCustomAttribute<FreeFunctionAttribute>() is { Name: string freeName } && freeName.Equals(name));

                matches.AddRange(methods);
            }
        }

        return matches.Count switch
        {
            0 => null,
            > 1 => throw new AmbiguousMatchException($"Unable to resolve free function \"{name}\", multiple matches found: {string.Join(",", matches)}"),
            _ => matches[0]
        };
    }
}